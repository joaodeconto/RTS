using UnityEngine;
using System.Collections;
using Visiorama;

public class CottageGrowth : MonoBehaviour {

	public int unitsToGrowth = 5;

	// Use this for initialization
	void Start () {

		CottageFactory cottage = GetComponentInParent<CottageFactory>();
		cottage.cottagePopulation += unitsToGrowth;
		GameplayManager gm = ComponentGetter.Get<GameplayManager>();
		gm.VerifyPopulation();


	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
