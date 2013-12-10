using UnityEngine;
using System.Collections;

public class PaintAgent : MonoBehaviour
{
    
	public float paintSize;

  
	public void OnTriggerEnter(Collider other)
    {

        if (other.GetComponent<TerrainDeformer>() != null)
        {
           other.GetComponent<TerrainDeformer>().DestroyTerrain(this.transform.position,paintSize);
		   
			Destroy (rigidbody);
		}
       
    }
}