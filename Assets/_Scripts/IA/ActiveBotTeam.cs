using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;

public class ActiveBotTeam : MonoBehaviour {


	public int timeToActivateBotTeam = 5;


	// Use this for initialization
	void Start () 
	{
		Invoke ("Init",5);
	
	}

	public void Activate()
	{
		this.gameObject.SetActive(true);
	}

}
