using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;
using Visiorama.Utils;

public class InGameMenu : MonoBehaviour
{
	private bool wasInitialized = false;
	public GameObject controlsOptionPanel;
	public GameObject audioOptionPanel;
	public GameObject surrenderPanel;
	public GameObject AdsBtnPanel;
	protected GameplayManager gameplayManager;
	
	public void OnEnable ()
	{
		Open ();
	}
	
	public void Open ()
	{
		gameplayManager = ComponentGetter.Get<GameplayManager>();
		if (gameplayManager.pauseGame)	gameplayManager.GamePaused(true);
		if (wasInitialized)	return;		
		wasInitialized = true;		
		DefaultCallbackButton dcb;		
		Transform controls = this.transform.FindChild ("Controls");
		if (controls != null){
			dcb = controls.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,(ht_dcb) =>{controlsOptionPanel.SetActive (true);});
		}
		Transform audio = this.transform.FindChild ("Audio");		
		if (audio != null){
			dcb = audio.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,(ht_dcb) =>{audioOptionPanel.SetActive (true);});
		}
		Transform quit = this.transform.FindChild ("Surrender");		
		if (quit != null){
			dcb = quit.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,(ht_dcb) =>{surrenderPanel.SetActive (true);});
		}
		Transform close = this.transform.FindChild ("Resume");		
		if (close != null){
			dcb = close.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,(ht_dcb) =>{Close (); });
		}
		if(AdsBtnPanel){
			Transform purNoAds = AdsBtnPanel.transform.FindChild ("Yes");
			if (purNoAds != null){
				dcb = purNoAds.GetComponent<DefaultCallbackButton>();
				dcb.Init(null,(ht_dcb)=>{
					StoreManager.NoAdsPurchase();
					AdsBtnPanel.SetActive(false);
				});
			}		
			Transform closeAds = AdsBtnPanel.transform.FindChild ("No");
			if (closeAds != null){
				dcb = closeAds.GetComponent<DefaultCallbackButton>();
				dcb.Init(null,(ht_dcb)=>{
					AdsBtnPanel.SetActive(false);
				});
			}		
		}
	}
	
	public void Close ()
	{
		if (gameplayManager.pauseGame == true)	gameplayManager.GamePaused(false);
		gameObject.SetActive(false);		
	}
}
