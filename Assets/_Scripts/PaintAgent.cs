using UnityEngine;
using System.Collections;

public class PaintAgent : MonoBehaviour
{
    
	public float paintSize;
	public string terrainName = "Terrain";
	private TerrainDeformer terrainDeformer;

	public void Start ()
	{
		terrainDeformer = GameObject.Find (terrainName).GetComponent<TerrainDeformer>(); 
	
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
		terrainDeformer.DestroyTerrain(this.transform.position,paintSize);

	}
}