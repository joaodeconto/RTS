using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;

public class Upgrade : MonoBehaviour {

	public string upgradeName;
	public string description;
	public string stats1Value;
	public string stats1Text;
	public string stats2Value;
	public string stats2Text;
	public string guiTextureName;
	public float timeToSpawn;
	public string requisites;	
	public bool modelUpgrade = false;
	public bool unique = false;
	public bool uniquelyUpgraded { get; set; }
	public List<string> TechsToActive = new List<string>();	
	public ResourcesManager costOfResources;
	protected TechTreeController techTreeController;

	void Start ()
	{
		techTreeController = Visiorama.ComponentGetter.Get<TechTreeController>();
		TechActiveBool(TechsToActive, true);
	}

	public void TechActiveBool(List<string> techs, bool isAvailable)	//Por Enquanto so ativa a disponibilidade do upgrade
	{
		foreach (string tech in techs)
		{
			techTreeController.TechBoolOperator(tech,isAvailable);
		}
	}	

	void Update ()
	{
	
	}
}
