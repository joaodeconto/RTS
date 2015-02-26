using UnityEngine;
using System.Collections;

public class PaintAgent : MonoBehaviour
{
    
	private float paintSize;
	public string terrainName = "Terrain";
	private XTerrainDeformer xterrainDeformer;

	public void Start ()
	{
		xterrainDeformer = GameObject.Find (terrainName).GetComponent<XTerrainDeformer>(); 
		paintSize = GetComponent<CapsuleCollider>().radius * 2;	
	}

	 
//	public bool TerrainPaint

	public void Paint ()
	{
		xterrainDeformer.DestroyTerrain(this.transform.position,paintSize);

	}
	void ConstructFinished ()
	{
		xterrainDeformer = GameObject.Find (terrainName).GetComponent<XTerrainDeformer>(); 
		xterrainDeformer.DestroyTerrain(this.transform.position,paintSize);		
	}

}