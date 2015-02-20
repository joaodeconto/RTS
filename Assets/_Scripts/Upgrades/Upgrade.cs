using UnityEngine;
using System.Collections;

public class Upgrade : MonoBehaviour {


	public string upgradeName;
	public string description;
	public string stats1Value;
	public string stats1Text;
	public string stats2Value;
	public string stats2Text;
	public string guiTextureName;
	public float timeToSpawn;
	public bool modelUpgrade = false;
	public bool unique = false;
	public bool uniquelyUpgraded { get; set; }


	public ResourcesManager costOfResources;


	// Use this for initialization
	void Start () {


	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
