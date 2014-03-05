using UnityEngine;
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
	
	public UILabel CurrentCrystalsLabel;
	public UILabel CreatedUnitsLabel;
	public UILabel CreatedBuildingsLabel;
	
	public Transform options;
	public Transform menus;

	public Model.Player player { get; private set; }
	private List<Transform> listChildOptions;

	public void Init ()
	{
		//Deixar primeiro carregar o jogador
		Invoke ("InitScore", 0.5f);

		this.player = ConfigurationData.player;
		
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
					 Application.Quit ();
				});
			}
		}
		
		goMainMenu.SetActive (true);
	}

	void InitScore ()
	{
		Score.LoadScores
		(
			(Dictionary<string, Model.DataScore> dicScore) => 
			{
				//nao tem score, coloca zero como default
				Model.DataScore cc = Score.GetDataScore (DataScoreEnum.CurrentCrystals);
				Model.DataScore uc = Score.GetDataScore (DataScoreEnum.UnitsCreated);
				Model.DataScore bc = Score.GetDataScore (DataScoreEnum.BuildingsCreated);
			
				CurrentCrystalsLabel.text  = (cc == null) ? "" : cc.NrPoints.ToString ();
				CreatedUnitsLabel.text     = (uc == null) ? "" : uc.NrPoints.ToString ();
				CreatedBuildingsLabel.text = (bc == null) ? "" : bc.NrPoints.ToString ();
			}
		);
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
