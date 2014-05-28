using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class NewGameMenu : MonoBehaviour {
	
	private bool wasInitialized = false;
	public Transform pathOption;
	
	public GameObject tutorialPanel;
	public GameObject rankedPanel;
	public GameObject singlePanel;
	public GameObject survivalPanel;
	
	public void OnEnable ()
	{
		Open ();
	}
	
//	public void OnDisable ()
//	{
//		Close ();
//	}
	
	public void Open ()
	{
		if (wasInitialized)
			return;
		
		wasInitialized = true;
		
		DefaultCallbackButton dcb;
		
		Transform tutorial = pathOption.transform.FindChild ("Tutorial");
		
		if (tutorial != null)
		{
			dcb = tutorial.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) => 
			         {
				
				tutorialPanel.SetActive (true);
				
				
			});
		}
		
		Transform ranked = pathOption.transform.FindChild ("RankedBattle");
		
		if (ranked != null)
		{
			dcb = ranked.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) => 
			         {
				
				rankedPanel.SetActive (true);
				
				
			});
		}
		
		Transform single = pathOption.transform.FindChild ("SinglePlayer");
		
		if (single != null)
		{
			dcb = single.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) => 
			         {
				
				singlePanel.SetActive (true);
				
				
			});
		}
		
		Transform survival = pathOption.transform.FindChild ("Survival");
		
		if (survival != null)
		{
			dcb = survival.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) => 
			         {
				
				survivalPanel.SetActive (true);
				
				
			});
		}
		
		
		
//		Transform close = pathOption.transform.FindChild ("Resume");
//		
//		if (close != null)
//		{
//			dcb = close.gameObject.AddComponent<DefaultCallbackButton> ();
//			dcb.Init(null,
//			         (ht_dcb) => 
//			         {
//				
//			});
//		}
	}
//	
//	public void Close ()
//	{
//		
//	}
}