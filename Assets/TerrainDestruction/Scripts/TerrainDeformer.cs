using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class TerrainDeformer : MonoBehaviour
{
	public Texture2D crater;
	public float craterSize = 5;
	
	Color[] template;

	TerrainData data;
	Terrain terrain;

	float[,] heightRestore;
	float[,,] alphaRestore;
	Thread worker;
	List<TerrainDeformerJob> inbox = new List<TerrainDeformerJob> ();
	List<TerrainDeformerJob> outbox = new List<TerrainDeformerJob> ();
	int craterWidth, craterHeight;
	
	void OnApplicationQuit ()
	{
		data.SetHeights (0, 0, heightRestore);
		data.SetAlphamaps (0, 0, alphaRestore);
		worker.Abort ();
	}

	void OnDestroy ()
	{
		worker.Abort ();
	}

	void Start ()
	{
		craterWidth = crater.width;
		craterHeight = crater.height;
		terrain = GetComponent<Terrain> ();
		data = terrain.terrainData;
		heightRestore = data.GetHeights (0, 0, data.heightmapWidth, data.heightmapHeight);
		alphaRestore = data.GetAlphamaps (0, 0, data.alphamapHeight, data.alphamapWidth);
		template = crater.GetPixels ();
		worker = new Thread (WorkerMethod);
		worker.Start ();
		StartCoroutine (ApplyDamage ());
	}


	IEnumerator ApplyDamage ()
	{
		TerrainDeformerJob job;
		while (true) {
			job = null;
			lock (outbox) {
				if (outbox.Count > 0) {
					job = outbox[0];
					outbox.RemoveAt (0);
				}
			}
			if (job != null) {
				ApplyJob(job);
				continue;
			}
			yield return null;
		}
	}

	void WorkerMethod ()
	{
		try {
			TerrainDeformerJob job;
			while (true) {
				job = null;
				lock (inbox) {
					if (inbox.Count > 0) {
						job = inbox[0];
						inbox.RemoveAt (0);
					}
				}
				if (job == null) {
					Thread.Sleep (10);
					continue;
				}
				ProcessJob(job);
				lock (outbox) {
					outbox.Add (job);
				}
			}
		} catch (System.Exception e) {
			Debug.Log ("Thread Error:" + e.ToString ());
		}
	}

	void ProcessJob (TerrainDeformerJob job)
	{
		
		for (var x = 0; x < job.hw; x++) {
			for (var y = 0; y < job.hh; y++) {
				var cx = Mathf.RoundToInt(Mathf.InverseLerp(0, job.hw, x) * craterWidth);
				var cy = Mathf.RoundToInt(Mathf.InverseLerp(0, job.hh, y) * craterHeight);
				var d = template[cy * craterWidth + cx][job.useLayer];
				job.heights[x, y] -= (d * job.depthScale);
				
			}
		}
		
		for (var x = 0; x < job.aw; x++) {
			for (var y = 0; y < job.ah; y++) {
				var cx = Mathf.RoundToInt(Mathf.InverseLerp(0, job.aw, x) * craterWidth);
				var cy = Mathf.RoundToInt(Mathf.InverseLerp(0, job.ah, y) * craterHeight);
				var d = template[cy * craterWidth + cx][job.useLayer];
				for (var z = 0; z < job.alphamapLayers; z++) {
					job.splat[x, y, z] += z == 0 ? d : -d;
				}
			}
		}
				
	}
	
	void ApplyJob(TerrainDeformerJob job) {
		data.SetAlphamaps (job.ax, job.ay, job.splat);
		data.SetHeights (job.hx, job.hy, job.heights);
	}

	public void Damage (Vector3 position, float depth)
	{
		
		var local = position - transform.position;
    	var nlocal = new Vector3(Mathf.InverseLerp(0f, data.size.x, local.x), 0f, Mathf.InverseLerp(0f, data.size.z, local.z));
		
		
		
		
		var job = new TerrainDeformerJob ();
		
		job.hx = Mathf.FloorToInt(nlocal.x * data.heightmapWidth);
		job.hy = Mathf.FloorToInt(nlocal.z * data.heightmapHeight);
		job.hw = Mathf.FloorToInt(craterSize / (data.size.x / data.heightmapWidth));
		job.hh = Mathf.FloorToInt(craterSize / (data.size.z / data.heightmapHeight));
		job.hx -= (job.hw / 2);
		job.hy -= (job.hh / 2);
		
		job.ax = Mathf.FloorToInt(nlocal.x * data.alphamapWidth);
		job.ay = Mathf.FloorToInt(nlocal.z * data.alphamapHeight);
		job.aw = Mathf.FloorToInt(craterSize / (data.size.x / data.alphamapWidth));
		job.ah = Mathf.FloorToInt(craterSize / (data.size.z / data.alphamapHeight));
		job.ax -= (job.aw / 2);
		job.ay -= (job.ah / 2);
		
		
		job.useLayer = Random.Range (0, 3);
		
		job.heights = data.GetHeights (job.hx, job.hy, job.hw, job.hh);
		job.splat = data.GetAlphamaps (job.ax, job.ay, job.aw, job.ah);
		job.depthScale = depth / data.size.y;
		job.alphamapLayers = data.alphamapLayers;
		lock (inbox) {
			inbox.Add (job);
		}
		
	}

	class TerrainDeformerJob
	{
		public int aw, ah, hw, hh, ax, ay, hx, hy;
		public float[,] heights;
		public float[,,] splat;
		public int alphamapLayers;
		public int useLayer;
		public float depthScale;
	}
	
}
