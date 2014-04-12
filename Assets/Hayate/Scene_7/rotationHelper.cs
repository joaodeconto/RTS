using UnityEngine;
using System.Collections;

public class rotationHelper : MonoBehaviour {

	public bool rotateX;
	public bool rotateY;
	public bool rotateZ;
	
	public float speedX;
	public float speedY;
	public float speedZ;
	
	void Update () {
		if(rotateX)
			transform.Rotate(Vector3.left * speedX * Time.deltaTime);
		
		if(rotateY)
			transform.Rotate(Vector3.up * speedY * Time.deltaTime);
		
		if(rotateZ)
			transform.Rotate(Vector3.forward * speedZ * Time.deltaTime);
	}
}
