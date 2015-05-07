using UnityEngine;
using System.Collections;

public class ActiveTargetObject : MonoBehaviour 
{

	public GameObject target;
	// Use this for initialization
	void OnEnable () {

		target.SetActive(true);
	
	}
	
	// Update is called once per frame
	void OnDisable () {

		target.SetActive(false);
	
	}
}
