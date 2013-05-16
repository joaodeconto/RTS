using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	public Transform options;
	public Transform menus;

	public Model.Player player { get; private set; }
	private List<Transform> listChildOptions;

	public void Init (Model.Player player)
	{
		Debug.Log ("Chegou!");
		Invoke ("InitScore", Random.value * 2.0f);

		this.player = player;
//		dcb = quickMatch.gameObject.AddComponent<DefaultCallbackButton> ();
//
//		dcb.Init(null, (ht_hud) =>
//		{
//			Hashtable roomProperties = new Hashtable() { { "closeRoom", false } };
//			PhotonNetwork.JoinRandomRoom (roomProperties, 0);
//
			//TODO fazer timeout de conexão
//		});

		goMainMenu.SetActive (true);

		PlayerLabel.text = player.SzName;

		listChildOptions = new List<Transform>();
		foreach (Transform child in options)
		{
			listChildOptions.Add (child);

			Transform button = child.FindChild ("Button");

			if (button != null)
			{
				Hashtable ht = new Hashtable ();
				ht["optionName"] = child.name;

				button
					.gameObject
					.AddComponent<DefaultCallbackButton>()
					.Init (ht, (ht_hud) =>
					{
						ShowMenu ((string)ht_hud["optionName"]);
					});
			}

			if (child.name == "Quit")
			{
				button
					.gameObject
					.AddComponent<DefaultCallbackButton>()
					.ChangeParams (null, (ht_dcb) =>
					{
						 Application.Quit ();
					});
			}
		}
	}

	void InitScore ()
	{
		Score.LoadScore ();
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
