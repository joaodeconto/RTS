using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class InternalMainMenu : MonoBehaviour
{
	[System.Serializable]
	public class Menu
	{
		public string name;
		public GameObject goMenu;
	}

	public GameObject goMainMenu;
	public UILabel PlayerLabel;
	public UISprite avatarImg;

	private bool canQuit = false;
	
	public UILabel CurrentCrystalsLabel;
	public UILabel CreatedUnitsLabel;
	public UILabel CreatedBuildingsLabel;
	
	public Transform options;
	public Transform menus;
	
	public ShowScore score;

	public Model.Player player { get; private set; }
	private List<Transform> listChildOptions;

	private bool WasInitialized = false;
	
	public void Awake ()
	{
		Init ();
	
	}
	
	public void Init ()
	{
		if (WasInitialized || ConfigurationData.player == null)
			return;
	
		//Se ja fez um jogo antes
		if (ConfigurationData.Logged && ConfigurationData.InGame)
		{
			ConfigurationData.InGame = false;
			score.Init ();
			return;
		}

		if (Advertisement.isSupported) {
			Advertisement.allowPrecache = true;
			Advertisement.Initialize ("18990");
		}
		
		else {
			
			Debug.Log("Platform not supported");
		}


		SoundManager.SetVolumeMusic (PlayerPrefs.GetFloat("MusicVolume"));

		avatarImg.spriteName = PlayerPrefs.GetString("Avatar");
	
		//Deixar primeiro carregar o jogador
		Invoke ("InitScore", 0.5f);

		this.player = ConfigurationData.player;
		
		Debug.Log ("player: " + player);
		
//		dcb = quickMatch.gameObject.AddComponent<DefaultCallbackButton> ();
//
//		dcb.Init(null, (ht_hud) =>
//		{
//			Hashtable roomProperties = new Hashtable() { { "closeRoom", false } };
//			PhotonNetwork.JoinRandomRoom (roomProperties, 0);
//
			//TODO fazer timeout de conexão
//		});

		PlayerLabel.text = player.SzName;

		listChildOptions = new List<Transform>();
		foreach (Transform child in options)
		{
			listChildOptions.Add (child);

			Transform button = child.FindChild ("Button");
			DefaultCallbackButton dcb;

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

					Advertisement.Show(null, new ShowOptions {
						pause = true,
						resultCallback = result => {

							QuitGame();

						}
					});
								
				});
			}
		}
		
		goMainMenu.SetActive (true);
	}

	void InitScore ()
	{
		Score.LoadScores
		(
			() => 
			{
				Score.GetDataScore
				(
					DataScoreEnum.CurrentCrystals,
					(currentCrystals) =>
					{
						CurrentCrystalsLabel.text  = currentCrystals.NrPoints.ToString ();
					}
				);
				
				Score.GetDataScore
				(
					DataScoreEnum.UnitsCreated,
					(unitsCreated) =>
					{
						CreatedUnitsLabel.text = unitsCreated.NrPoints.ToString ();
					}
				);
						
				Score.GetDataScore
				(
					DataScoreEnum.BuildingsCreated,
					(buildingsCreated) =>
					{
						CreatedBuildingsLabel.text = buildingsCreated.NrPoints.ToString ();
					}
				);
			}
		);
	}
	private void QuitGame ()
	{
		Application.Quit();
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
