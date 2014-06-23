using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class TutorialMenu : MonoBehaviour
{
	[System.Serializable ()]
	public class TutorialMenuButtons
	{
		public Transform Btn0;
		public Transform Btn1;
		public Transform Btn2;
		public Transform Btn3;
		public Transform BtnLeaveRoom;
		
		public IEnumerable<Transform> Iterate
		{
			get
			{
				yield return Btn0;
				yield return Btn1;
				yield return Btn2;
				yield return Btn3;
				yield return BtnLeaveRoom;
			}
		}
	}
	
	public TutorialMenuButtons buttons;
	
	public UILabel messageActiveGame;
	public UILabel errorMessage;

	private int mapScene = 4;


	bool wasInitialized      = false;
	
	PhotonWrapper pw;

	public void OnEnable ()
	{
		Open ();
	}
	
	public void Open ()
	{
		//		Debug.Log ("player na segunda cena: " + ComponentGetter.Get<PhotonWrapper> ().GetPropertyOnPlayer ("player"));
		//		if (wasInitialized)
		//			return;
		
		//		wasInitialized = true;
		if (wasInitialized)
			return;
		
		wasInitialized = true;
		
		messageActiveGame.gameObject.SetActive (false);
		
		errorMessage.gameObject.SetActive (false);

		pw = ComponentGetter.Get<PhotonWrapper> ();
		
		DefaultCallbackButton dcb;
		
		if (buttons.Btn0)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn0, false);
			dcb.Init ( null, (ht_hud) => { CreateRoom (1, 0, "Basics", "Tutorial", 4);  } );
		}
		
		if (buttons.Btn1)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn1, false);
			dcb.Init ( null, (ht_hud) => { CreateRoom (1, 0,"Combat", "Tutorial", 5); } );
		}
		
		if (buttons.Btn2)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn2, false);
			dcb.Init ( null, (ht_hud) => { CreateRoom (1, 0,"Favor", "Tutorial", 6); } );
		}
		
		if (buttons.Btn3)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn3, false);
			dcb.Init ( null, (ht_hud) => { CreateRoom (2, 0, "Online", "DeathMatch", 1); } );
		}
		
				
		if (buttons.BtnLeaveRoom)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.BtnLeaveRoom, false);
			dcb.Init ( null, (ht_hud) => { if (pw.LeaveRoom ()) Close (); } );

		}
	}

	public void Close ()
	{
		CancelInvoke ("TryToEnterGame");
		
		foreach (GameObject menu in pw.menusDissapearWhenLogged)
		{
			menu.SetActive (true);
		}

		if (buttons == null) return;
				
				foreach (Transform button in buttons.Iterate)
				{
					if (button)
					{
						button.gameObject.SetActive (true);
					}
				}
		
		messageActiveGame.gameObject.SetActive (false);
					
		gameObject.SetActive (false);
	}
	
	
	private void CreateRoom (int maxPlayers, int bid, string battleTypeName, string bMode, int map)
	{
		Model.Player player = ComponentGetter.Get <InternalMainMenu>().player;
		PlayerBattleDAO playerBattleDao = ComponentGetter.Get <PlayerBattleDAO> ();
		PlayerDAO playerDao = ComponentGetter.Get <PlayerDAO> ();
						
		if (bMode == "DeathMatch")
		{
			GameplayManager.Mode mode = GameplayManager.Mode.Deathmatch;
			GameplayManager.mode = mode;
			
			
		}
		
		if (bMode == "Cooperative")
		{
			GameplayManager.Mode mode = GameplayManager.Mode.Cooperative;
			GameplayManager.mode = mode;
		}

		if (bMode == "Tutorial")
		{
			GameplayManager.Mode mode = GameplayManager.Mode.Tutorial;
			GameplayManager.mode = mode;
		}
		
		
		
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
			
			pw.CreateRoom (roomName, bid, isVisible, isOpen, maxPlayers, map, properties);
			
			pw.SetPropertyOnPlayer ("team", 0);
			pw.SetPropertyOnPlayer ("ready", true);
			
			//	VDebug.Log ("battle: " + properties["battle"]);

			foreach (Transform button in buttons.Iterate)
			{
				if (button)
				{
					button.gameObject.SetActive (false);
				}
			}
			
			messageActiveGame.gameObject.SetActive (true);
			messageActiveGame.text = "Game Created";
			
			if (buttons.BtnLeaveRoom)
			{
				buttons.BtnLeaveRoom.gameObject.SetActive (true);
			}

			pw.TryToEnterGame (10000.0f,
			                   (other_message) =>
			                   {
				Debug.Log("message: " + other_message);
				
				messageActiveGame.gameObject.SetActive (true);
				
				errorMessage.gameObject.SetActive (false);
				
				Invoke ("CloseErrorMessage", 5.0f);
			},
			(playersReady, nMaxPlayers) =>
			{
				messageActiveGame.text = "Ready to play";
			});
		});
	}
	
	private void CloseErrorMessage ()
	{
		errorMessage.gameObject.SetActive (false);
		if (buttons == null) return;
		
		foreach (Transform button in buttons.Iterate)
		{
			if (button)
			{
				button.gameObject.SetActive (true);
			}
		}
		
	}
}
