using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;
using System.Collections.Generic;
using Soomla.Store.RTSStoreAssets;

using Visiorama;

public class OfflineMenu : MonoBehaviour
{
	[System.Serializable]
	public class Menu
	{
		public string name;
		public GameObject goMenu;
	}

	public GameObject goMainMenu;
	public UILabel CurrentCrystalsLabel;
	public Transform options;
	public Transform menus;	
	public GameObject noAddBtn;
	protected ShowScore score;
	private List<Transform> listChildOptions;
		
	public void Init ()
	{
		listChildOptions = new List<Transform>();
		foreach (Transform child in options)
		{
			listChildOptions.Add (child);

			Transform button = child.FindChild ("Button");
			DefaultCallbackButton dcb;

			if ( ConfigurationData.addPass == true) noAddBtn.SetActive(false);

			if (button)
			{
				Hashtable ht = new Hashtable ();
				ht["optionName"] = child.name;

				dcb = ComponentGetter.Get <DefaultCallbackButton> (button, false);
				dcb.Init (ht, (ht_hud) =>
				{
					ShowMenu ((string)ht_hud["optionName"]);
				});
			}

			if (child.name == "Quit")
			{
				dcb = ComponentGetter.Get <DefaultCallbackButton> (button, false);
				dcb.ChangeParams (null, (ht_dcb) =>
				                  { 
										if (!ConfigurationData.addPass)
										{
										
											Advertisement.Show(null, new ShowOptions
						                    {
												pause = true,
												resultCallback = result => {QuitGame();}
											});
										}
										else QuitGame();
													
									});
			}
		}
		
		InitScore ();
		goMainMenu.SetActive (true);
	}


	private void QuitGame ()
	{
		Application.Quit();
	}

	public void InitScore ()
	{
		StoreManager sm = ComponentGetter.Get<StoreManager>();
		CurrentCrystalsLabel.text  = sm.GetBalance.ToString();
	}

	private void ShowMenu (string optionName)
	{
		Transform menu;
		foreach (Transform child in menus)
		{
			menu = child.FindChild ("Menu");

			if (menu != null)
			{
				menu.gameObject.SetActive (false);
			}

			child.SendMessage ("Close", SendMessageOptions.DontRequireReceiver);
		}

		Transform option = menus.FindChild (optionName);

		menu = option.FindChild ("Menu");

		if (menu != null)
		{
			menu.gameObject.SetActive (true);
		}

		option.SendMessage ("Open", SendMessageOptions.DontRequireReceiver);
	}
}
