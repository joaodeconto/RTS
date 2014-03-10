using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NewGame : MonoBehaviour
{
	[System.Serializable ()]
	public class NewGameMenuButtons
	{
		public Transform BtnTutorial;
		public Transform Btn3P;
		public Transform Btn4P;
		public Transform Btn1x1;
		public Transform Btn2x2;
		public Transform BtnLeaveRoom;

		public IEnumerable<Transform> Iterate
		{
			get
			{
				yield return BtnTutorial;
				yield return Btn3P;
				yield return Btn4P;
				yield return Btn1x1;
				yield return Btn2x2;
				yield return BtnLeaveRoom;
			}
		}
	}

	public NewGameMenuButtons buttons;

	public UILabel messageActiveGame;
	public UILabel errorMessage;
	
	public UILabel bidLabel;
	
	private int CurrentBid {
		get { return (bidLabel) ? int.Parse (bidLabel.text) : 1; }
	}

	float RefreshingInterval = 2.0f;
	bool wasInitialized      = false;

	PhotonWrapper pw;

	public void Open ()
	{
//		Debug.Log ("player na segunda cena: " + ComponentGetter.Get<PhotonWrapper> ().GetPropertyOnPlayer ("player"));
//		if (wasInitialized)
//			return;

//		wasInitialized = true;
		messageActiveGame.enabled = false;

		pw = ComponentGetter.Get<PhotonWrapper> ();

		DefaultCallbackButton dcb;

		if (buttons.BtnTutorial)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.BtnTutorial, false);
			dcb.Init ( null, (ht_hud) => { CreateRoom (1, CurrentBid, "Tutorial"); } );
		}
		
		if (buttons.Btn3P)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn3P, false);
			dcb.Init ( null, (ht_hud) => { CreateRoom (3, CurrentBid,"3P"); } );
		}
		
		if (buttons.Btn4P)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn4P, false);
			dcb.Init ( null, (ht_hud) => { CreateRoom (4, CurrentBid,"4P"); } );
		}
		
		if (buttons.Btn1x1)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn1x1, false);
			dcb.Init ( null, (ht_hud) => { CreateRoom (2, CurrentBid,"1x1"); } );
		}
		
		if (buttons.Btn2x2)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn2x2, false);
			dcb.Init ( null, (ht_hud) => { CreateRoom (4, CurrentBid, "2x2", GameplayManager.Mode.Cooperative); } );
		}
		
		if (buttons.BtnLeaveRoom)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.BtnLeaveRoom, false);
			dcb.Init ( null, (ht_hud) => { if (pw.LeaveRoom ()) Close (); } );
			buttons.BtnLeaveRoom.gameObject.SetActive (false);
		}
	}

	public void Close ()
	{
		CancelInvoke ("TryToEnterGame");

		if (buttons == null) return;

		foreach (Transform button in buttons.Iterate)
		{
			if (button)
			{
				button.gameObject.SetActive (true);
			}
		}

		messageActiveGame.enabled = false;

		buttons.BtnLeaveRoom.gameObject.SetActive (false);
	}

	private void CreateRoom (int maxPlayers, int bid, string battleTypeName, GameplayManager.Mode mode = GameplayManager.Mode.Deathmatch)
	{
		Model.Player player = ComponentGetter.Get <InternalMainMenu>().player;
		PlayerBattleDAO playerBattleDao = ComponentGetter.Get <PlayerBattleDAO> ();
		PlayerDAO playerDao = ComponentGetter.Get <PlayerDAO> ();

		//TODO refazer as battles
		playerBattleDao.CreateBattle (battleTypeName, bid, DateTime.Now, maxPlayers,
		(battle) =>
		{
//			Debug.Log ("message: " + message);
//			Debug.Log ("playerBattle: " + playerBattle);

			string roomName = "Room" + (PhotonNetwork.GetRoomList().Length + 1) + " : " + System.DateTime.Now.ToString ("mm-ss");
			bool isVisible = true, isOpen = true;
			
			ConfigurationData.battle = battle;
			
			Hashtable properties = new Hashtable ();
			properties.Add ("battle", battle.ToString ());

			pw.CreateRoom (roomName, bid, isVisible, isOpen, maxPlayers, properties);
			
			pw.SetPropertyOnPlayer ("team", 0);
			pw.SetPropertyOnPlayer ("ready", true);

			VDebug.Log ("battle: " + properties["battle"]);

			GameplayManager.mode = mode;

			messageActiveGame.enabled = true;
			messageActiveGame.text = "Waiting For Other Players...";

			foreach (Transform button in buttons.Iterate)
			{
				if (button)
				{
					button.gameObject.SetActive (false);
				}
			}
			
			if (buttons.BtnLeaveRoom)
			{
				buttons.BtnLeaveRoom.gameObject.SetActive (true);
			}

			pw.TryToEnterGame (10000.0f,
			(other_message) =>
			{
				Debug.Log("message: " + other_message);

				messageActiveGame.enabled = false;

//				buttons.gameObject.SetActive (true);

				errorMessage.enabled = true;

				Invoke ("CloseErrorMessage", 5.0f);
			},
			(playersReady, nMaxPlayers) =>
			{
				messageActiveGame.text = "Waiting For Other Players - " + playersReady + "/" + nMaxPlayers;
			});
		});
	}

	private void CloseErrorMessage ()
	{
		errorMessage.enabled = false;
	}
}
