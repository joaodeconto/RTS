using UnityEngine;
using System.Collections;

public class ItemsIndex : MonoBehaviour {

	public GameObject unitView;
	public GameObject structureView;
	public GameObject upgradeView;
	public GameObject techtreeView;
	// Use this for initialization
	void Start()
	{
		unitView.SetActive(true);
		structureView.SetActive(false);
		upgradeView.SetActive(false);
		techtreeView.SetActive(false);
	}
	public void ActiveView (string viewType)
	{
		if (viewType == "Units")
		{
			unitView.SetActive(true);
			structureView.SetActive(false);
			upgradeView.SetActive(false);
			techtreeView.SetActive(false);
		}
		else if (viewType == "Structures")
		{
			unitView.SetActive(false);
			structureView.SetActive(true);
			upgradeView.SetActive(false);
			techtreeView.SetActive(false);
		}
		else if (viewType == "Upgrades")
		{
			unitView.SetActive(false);
			structureView.SetActive(false);
			upgradeView.SetActive(true);
			techtreeView.SetActive(false);
		}
		else if (viewType == "Tech Tree")
		{
			unitView.SetActive(false);
			structureView.SetActive(false);
			upgradeView.SetActive(false);
			techtreeView.SetActive(true);
		}
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
