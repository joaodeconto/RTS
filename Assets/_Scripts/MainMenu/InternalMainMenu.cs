using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;
using System.Collections.Generic;
using Soomla.Store.RTSStoreAssets;

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
	public UILabel CurrentCrystalsLabel;
	public UILabel CreatedUnitsLabel;
	public UILabel RankingLabel;	
	public Transform options;
	public Transform menus;	
	public ShowScore score;
	public Model.Player player { get; private set; }
	private List<Transform> listChildOptions;
	private bool WasInitialized = false;

	public void Start ()
	{
		if (ConfigurationData.Logged) Init ();	
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

		avatarImg.spriteName = PlayerPrefs.GetString("Avatar");
	
		//Deixar primeiro carregar o jogador
		player = ConfigurationData.player;
		PlayerLabel.text = player.SzName;
		Debug.Log ("player: " + player);
		Invoke ("InitScore", 0.2f);

//		dcb = quickMatch.gameObject.AddComponent<DefaultCallbackButton> ();
//
//		dcb.Init(null, (ht_hud) =>
//		{
//			Hashtable roomProperties = new Hashtable() { { "closeRoom", false } };
//			PhotonNetwork.JoinRandomRoom (roomProperties, 0);
//
			//TODO fazer timeout de conexão
//		});
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
									if(ConfigurationData.addPass || ConfigurationData.multiPass){ QuitGame();}
									else	Advertisement.Show(null, new ShowOptions
										                    {
																pause = true,
																resultCallback = result => {QuitGame();}
															});														
								 });
			}
			
		}
		
		goMainMenu.SetActive (true);
	}

	public void InitScore ()
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
						Debug.LogWarning("OrichalsDB: " + currentCrystals.NrPoints.ToString ());
					}
				);
			}
		);	
		SetPlayerRank();
		StoreManager sm = ComponentGetter.Get<StoreManager>();		
		CurrentCrystalsLabel.text  = sm.GetBalance.ToString();
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

	public void SetPlayerRank()
	{
		int i = 0;
		Score.LoadRanking 
			(
				(List<Model.DataScoreRanking> ranking) => 	
				{
				foreach (Model.DataScoreRanking r in ranking)
				{
					i++;
					if (r.IdPlayer == player.IdPlayer)
					{							
						RankingLabel.text =i.ToString();	
						PlayerPrefs.SetInt("Rank", i);
						break;													
					}
				}
			}
			);		
	}
}
