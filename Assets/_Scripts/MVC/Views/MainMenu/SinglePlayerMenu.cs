using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class SinglePlayerMenu : MonoBehaviour
{
	public UILabel messageActiveGame;
	public UILabel errorMessage;

	public UIPopupList mapSelection;
	public string playerMode;
	
	public Transform BtnLeaveRoom;
	public Transform createRoom;
	

	private int players;
		
	private string battleMode;
	private string battleName;
		
	bool wasInitialized      = false;
	
	PhotonWrapper pw;
	
	public void OnEnable ()
	{
		Open ();
	}
	
	public void BattleModeToogle (string pMode)
	{
		playerMode = pMode;
		
		if (playerMode == "4vs")
		{
			players = 1;
			battleMode = "DeathMatch";
			battleName = "DeathMatch";
		}
		
		if (playerMode == "1x1")
		{
			players = 1;
			battleMode = "DeathMatch";
			battleName = "DeathMatch";
		}
		
		if (playerMode == "2x2")
		{
			players = 1;
			battleMode = "Cooperative";
			battleName = "2x2";
		}
		
		pw = ComponentGetter.Get<PhotonWrapper> ();
		
		DefaultCallbackButton dcb;
		
		
		if (createRoom)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (createRoom, false);
			dcb.Init ( null, (ht_hud) =>
			          {
				

					CreateRoom (players, 0, battleName, battleMode);
					createRoom.gameObject.SetActive (false);
								
				
					} );
		}
		
	}
	
	
	
	public void Open ()
	{
		if (wasInitialized)
			return;
		
		wasInitialized = true;
		
		createRoom.gameObject.SetActive (true);
		
		messageActiveGame.gameObject.SetActive (false);
		
		errorMessage.gameObject.SetActive (false);
		
		pw = ComponentGetter.Get<PhotonWrapper> ();
		
		DefaultCallbackButton dcb;
		
		//		Debug.Log ("player na segunda cena: " + ComponentGetter.Get<PhotonWrapper> ().GetPropertyOnPlayer ("player"));
		
		if (BtnLeaveRoom)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (BtnLeaveRoom, false);
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
		
		createRoom.gameObject.SetActive (true);
		
		messageActiveGame.gameObject.SetActive (false);
			
		gameObject.SetActive (false);
	}
	
	
	private void CreateRoom (int maxPlayers, int bid, string battleTypeName, string bMode)
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
			
			//	VDebug.Log ("battle: " + properties["battle"]);
			
			
			messageActiveGame.gameObject.SetActive (true);
			messageActiveGame.text = "Game Created";
			
			if (BtnLeaveRoom)
			{
				BtnLeaveRoom.gameObject.SetActive (true);
			}
			
			pw.TryToEnterGame (10000.0f,
			                   (other_message) =>
			                   {
				Debug.Log("message: " + other_message);
				
				messageActiveGame.gameObject.SetActive (true);
				
				createRoom.gameObject.SetActive (true); 
				
				errorMessage.gameObject.SetActive (true);
				
				Invoke ("CloseErrorMessage", 5.0f);
			},
			(playersReady, nMaxPlayers) =>
			{
				messageActiveGame.text = "Waiting Players " + playersReady + "/" + nMaxPlayers;
			});
		});
	}
	
	private void CloseErrorMessage ()
	{
		errorMessage.gameObject.SetActive (false);
		createRoom.gameObject.SetActive (true);
		
	}
}
