// ImposterLOD for Automatic 3D Billboard Imposters
// By CWKX

using UnityEngine;

public class ImposterLOD : MonoBehaviour 
{
	public float lodDistance = 100f;
	
	private Vector3 sqrStippling;	// Squared stippling distance regions
	private Renderer[] lod0Rend; 	// Original mesh renderer(s)
	private Renderer   lod1Rend; 	// Imposter renderer
	private Renderer   lod2Rend;    // Batching imposter renderer (when not stippling)
	
	private int frameSkip; // Fast priority updates
	private int  curFrame; // Fast priority updates
	
	void Start() { Reset(); }
	
	public void Reset()
	{
		GameObject lod0 = null;
		GameObject lod1 = null;
		
		if (transform.FindChild("Lod_0")) lod0 = transform.FindChild("Lod_0").gameObject;
		if (transform.FindChild("Lod_1")) lod1 = transform.FindChild("Lod_1").gameObject;
		
		// Square distance to stop doing Sqrt in the Update() loop
		float maxScale = Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
		float sqrDistance = lodDistance * lodDistance * maxScale * maxScale;
		
		// A 25% (squared = Sqrt(0.25) stippling region
		sqrStippling = new Vector3(sqrDistance - 0.5f * sqrDistance, sqrDistance + 0.5f * sqrDistance, sqrDistance);
		
		// Acquire renderers to set the stippling shader parameters
		lod0Rend = lod0.GetComponentsInChildren<Renderer>();
		lod1Rend = lod1.GetComponent<Renderer>();
		
		// Create a batching renderer using shared material
		Transform trans = transform.FindChild("Lod_2");
		GameObject lod2 = null;
		
		if (!trans) lod2 = (GameObject)Instantiate(lod1, Vector3.zero, Quaternion.identity);
		else lod2 = trans.gameObject;
		
		lod2.name = "Lod_2";
		lod2.transform.parent = lod1.transform.parent;
		lod2Rend = lod2.renderer;
		
		curFrame = 0;
		frameSkip = 0;
	}
	
	void Update()
	{
		if (curFrame++ < frameSkip) return;
		
		float curDistance = (gameObject.transform.position - Camera.main.transform.position).sqrMagnitude;
		float s = Mathf.InverseLerp(sqrStippling.y, sqrStippling.x, curDistance);		
		
		curFrame = 0;
		frameSkip = Mathf.Clamp(Mathf.FloorToInt(curDistance / sqrStippling.y), 0, 120);

		foreach (Renderer rend in lod0Rend)
		{
			rend.enabled = curDistance < sqrStippling.y;
			
			if (rend.enabled)
				foreach (Material mat in rend.materials)
					mat.SetFloat("_Stippling", s);
		}
		
		lod1Rend.enabled = curDistance > sqrStippling.x && s != 0f;
		lod2Rend.enabled = curDistance > sqrStippling.x && s == 0f;
		
		if (lod1Rend.enabled)
			lod1Rend.material.SetFloat("_Stippling", s);
		
		if (lod2Rend.enabled)
			lod2Rend.sharedMaterial.SetFloat("_Stippling", 0f);
	}
}
