using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class SettingsMenu : MonoBehaviour {

	private bool wasInitialized = false;
	public Transform pathOption;
	
	public GameObject controlsOptionPanel;
	public GameObject audioOptionPanel;
	public GameObject graphicPanel;
	public GameObject avatarPanel;
	
	public void OnEnable ()
	{
		Open ();
	}
	
	public void OnDisable ()
	{
		Close ();
	}
	
	public void Open ()
	{
		if (wasInitialized)
			return;
		
		wasInitialized = true;
		
		DefaultCallbackButton dcb;
		
		Transform controls = pathOption.transform.FindChild ("Controls");
		
		if (controls != null)
		{
			dcb = controls.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) => 
			         {
				
				controlsOptionPanel.SetActive (true);
				

			});
		}

		Transform graphics = pathOption.transform.FindChild ("Graphics");
		
		if (graphics != null)
		{
			dcb = graphics.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) => 
			         {
				
				graphicPanel.SetActive (true);
				
	
			});
		}
		
		Transform audio = pathOption.transform.FindChild ("Audio");
		
		if (audio != null)
		{
			dcb = audio.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) => 
			         {
				
				audioOptionPanel.SetActive (true);
				
	
			});
		}
		
		Transform avatar = pathOption.transform.FindChild ("Avatar");


		if (avatar != null)
		{
			dcb = avatar.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) => 
			         {
				
				avatarPanel.SetActive (true);
				
			
			});
		}
		
		
		
		Transform close = pathOption.transform.FindChild ("Resume");
		
		if (close != null)
		{
			dcb = close.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) => 
			         {

			});
		}
	}
	
	public void Close ()
	{
		
	}
}
//
//
//
//	public GameObject goOptions;
//	public Transform options;
//	public Transform menus;
//
//	private List<Transform> listChildOptions;
//	
//	private bool WasInitialized = false;
//	
//	public void Awake ()
//	{
//		Init ();
//	}
//	
//	public void Init ()
//	{
//			
//		listChildOptions = new List<Transform>();
//		foreach (Transform child in options)
//		{
//			listChildOptions.Add (child);
//			
//			Transform button = child.FindChild ("Button");
//			DefaultCallbackButton dcb;
//			
//			if (button)
//			{
//				Hashtable ht = new Hashtable ();
//				ht["optionName"] = child.name;
//				
//				dcb = ComponentGetter.Get <DefaultCallbackButton> (button, false);
//				dcb.Init (ht, (ht_hud) =>
//				          {
//					ShowMenu ((string)ht_hud["optionName"]);
//				});
//			}
//			
//		}
//
//	}
//	
//
//	private void ShowMenu (string optionName)
//	{
//		Transform menu;
//		foreach (Transform child in menus)
//		{
//			menu = child.FindChild ("Menu");
//			
//			if (menu != null)
//			{
//				menu.gameObject.SetActive (false);
//			}
//			
//			child.SendMessage ("Close", SendMessageOptions.DontRequireReceiver);
//		}
//		
//		Transform option = menus.FindChild (optionName);
//		
//		menu = option.FindChild ("Menu");
//		
//		if (menu != null)
//		{
//			menu.gameObject.SetActive (true);
//		}
//		
//		option.SendMessage ("Open", SendMessageOptions.DontRequireReceiver);
//
//		goOptions.SetActive (false);
//	}

