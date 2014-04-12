using UnityEngine;
using System.Collections;

public class AddForce : MonoBehaviour {
	
	public float strength;
	
	void Update () {
		if(Input.GetMouseButtonDown(0))
		{
			rigidbody.AddForce(new Vector3(Random.Range(-strength,strength), Random.Range(-strength,strength), Random.Range(-strength,strength)));
		}
	}
}
