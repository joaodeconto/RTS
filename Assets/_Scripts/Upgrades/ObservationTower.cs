using UnityEngine;
using System.Collections;
using Visiorama;

public class ObservationTower : MonoBehaviour {

	public int newFieldOfView = 30;

	// Use this for initialization
	void Start () {

		FactoryBase tower = GetComponentInParent<FactoryBase>();
		tower.fieldOfView = newFieldOfView;
					
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
