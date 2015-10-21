using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Soomla.Profile;
using Visiorama;

public class NewGameMenu : MonoBehaviour {
	
	private bool wasInitialized = false;
	public Transform pathOption;	
	public GameObject tutorialPanel;
	public GameObject rankedPanel;
	public GameObject singlePanel;
	public GameObject survivalPanel;
	public GameObject ratePanel;
	
	public void OnEnable ()
	{
		Open ();
	}
		
	public void Open ()
	{		
		DefaultCallbackButton dcb;	

		if(PlayerPrefs.GetInt("Logins") >=2 && PlayerPrefs.GetInt("Rated") < 1){
			ratePanel.SetActive(true);
			Transform btnA = ratePanel.transform.FindChild("Rate");
			dcb = btnA.GetComponent<DefaultCallbackButton>();
			dcb.Init(null,(ht_dcb) =>{
				SoomlaProfile.OpenAppRatingPage();
				ratePanel.SetActive(false);
				PlayerPrefs.SetInt("Rated",1);												
			});
			Transform btnB = ratePanel.transform.FindChild("Later");
			dcb = btnB.GetComponent<DefaultCallbackButton>();
			dcb.Init(null,(ht_dcb) =>{
				ratePanel.SetActive(false);	
				PlayerPrefs.SetInt("Rated",1);
			});
		}
			
		Transform tutorial = pathOption.transform.FindChild ("Tutorial");		
		if (tutorial != null){
			dcb = tutorial.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) =>{				
						tutorialPanel.SetActive (true);	
			});
		}
		
		Transform ranked = pathOption.transform.FindChild ("RankedBattle");		
		if (ranked != null){
			dcb = ranked.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,(ht_dcb) =>{
							rankedPanel.SetActive (true);		
			});
		}
		
		Transform single = pathOption.transform.FindChild ("SinglePlayer");		
		if (single != null){
			dcb = single.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,(ht_dcb) =>{				
							singlePanel.SetActive (true);
			});
		}
		
		Transform survival = pathOption.transform.FindChild ("Survival");		
		if (survival != null){
			dcb = survival.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,(ht_dcb) =>{
							survivalPanel.SetActive (true);
			});
		}	
	}
}