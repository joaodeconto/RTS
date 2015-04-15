using UnityEngine;
using System.Collections;

public class OfflineMode : IView
{
	protected TutorialMenu tutorialMenu;

	// Use this for initialization
	public OfflineMode Init ()
	{
		tutorialMenu =  GetComponent<TutorialMenu>();
	
		return this;
	}	
	
	// Update is called once per frame
	void Update () {
	
	}
}
