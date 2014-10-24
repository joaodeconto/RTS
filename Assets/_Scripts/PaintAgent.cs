using UnityEngine;
using System.Collections;

public class PaintAgent : MonoBehaviour
{
    
	public float paintSize;
	public string terrainName = "Terrain";
	private XTerrainDeformer xterrainDeformer;

	public void Start ()
	{
		xterrainDeformer = GameObject.Find (terrainName).GetComponent<XTerrainDeformer>(); 
	
	}

	 
//	public bool TerrainPaint (
  
//	public void OnCollision (Collider other)
//    {
//
//        if (other.GetComponent<TerrainDeformer>() != null)
//        {
//           other.GetComponent<TerrainDeformer>().DestroyTerrain(this.transform.position,paintSize);
//		   
//
//		}
//       
//    }

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