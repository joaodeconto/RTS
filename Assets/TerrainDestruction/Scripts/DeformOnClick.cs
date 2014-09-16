using UnityEngine;
using System.Collections;

public class DeformOnClick : MonoBehaviour {

	TerrainDeformer deformer;
	void Start () {
		deformer = GetComponent<TerrainDeformer>();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000)) {
				deformer.Damage(hit.point, 10);
			}
		}
	}
}
