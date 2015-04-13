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
		paintSize = GetComponent<CapsuleCollider>().radius * 3;	
	}

	 
//	public bool TerrainPaint

	public void Paint (Vector3 paintPosition, float paintRadius)
	{
		xterrainDeformer = GameObject.Find (terrainName).GetComponent<XTerrainDeformer>(); 
		xterrainDeformer.DestroyTerrain(paintPosition, paintRadius);		
	}

}