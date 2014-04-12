using UnityEngine;
using System.Collections;

public class RotateCamera : MonoBehaviour {
	
	public Transform RotationCenter;
	public float speed = 5;
	
	
	void Update () {
		transform.RotateAround(RotationCenter.position, Vector3.up, speed * Time.deltaTime);
	}
}
