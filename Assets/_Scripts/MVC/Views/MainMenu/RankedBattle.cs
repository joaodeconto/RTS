using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Visiorama;
using I2.Loc;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RankedBattle : MonoBehaviour
{
	public UILabel messageActiveGame;
	public string playerMode = "1x1";
	public UIPopupList mapSelection;
	public Transform btnCreateRoom;
	public Transform BtnLeaveRoom;
	public GameObject marketGlow;
	public UILabel errorMessage;
	public int minimumBet = 1;
	public UIInput bidInput;
	public UILabel cristal;

	private Transform toggleButtons;
	private string battleMode;
	private string battleName;
	private int players = 2;
	private int mapScene;
	private int orichals {
		get {
			return (cristal) ? int.Parse (cristal.text) :1;}	
	}
	private int CurrentBid {	
		get {
			return (bidInput) ? int.Parse (bidInput.value) : 1; }
	}
	protected PhotonWrapper pw;

	public void OnEnable ()
	{
		Open ();
	}
	public void Open ()
	{
		toggleButtons = this.transform.FindChild("Menu").FindChild("Buttons").transform;
		btnCreateRoom.gameObject.SetActive (true);
		messageActiveGame.gameObject.SetActive (false);
		errorMessage.gameObject.SetActive (false);
		pw = ComponentGetter.Get<PhotonWrapper> ();		
		DefaultCallbackButton dcb;

		if (BtnLeaveRoom){
			dcb = ComponentGetter.Get <DefaultCallbackButton> (BtnLeaveRoom, false);
			dcb.Init ( null, (ht_hud) => { if (pw.LeaveRoom ()) Close (); } );

		}
		if (PhotonNetwork.connectionState == ConnectionState.Disconnected){
			PhotonNetwork.ConnectToMaster ( PhotonNetwork.PhotonServerSettings.ServerAddress,
			                               PhotonNetwork.PhotonServerSettings.ServerPort,
			                               PhotonNetwork.PhotonServerSettings.AppID,
			                               ConfigurationData.VERSION);
		}
	}

	public void Close ()
	{
		CancelInvoke ("TryToEnterGame");

		foreach (GameObject menu in pw.menusDissapearWhenLogged)
		{
			menu.SetActive (true);
		}

		mapSelection.gameObject.SetActive (true);
		toggleButtons.gameObject.SetActive(true);
		bidInput.gameObject.SetActive(true);
		btnCreateRoom.gameObject.SetActive (true);
		messageActiveGame.gameObject.SetActive (false);
		marketGlow.SetActive (false);
		gameObject.SetActive (false);
		if (PhotonNetwork.connectionState == ConnectionState.Connected)	PhotonNetwork.Disconnect();
	}


	IEnumerator CreateRoom (int maxPlayers, int bid, string battleTypeName, string bMode, int map)
	{	
		PlayerBattleDAO playerBattleDao = ComponentGetter.Get <PlayerBattleDAO> ();

		if (bMode == "DeathMatch"){
			GameplayManager.Mode mode = GameplayManager.Mode.Deathmatch;
			GameplayManager.mode = mode;
		}

		if (bMode == "Cooperative"){
			GameplayManager.Mode mode = GameplayManager.Mode.Cooperative;
			GameplayManager.mode = mode;
		}

		messageActiveGame.gameObject.SetActive (true);
		messageActiveGame.text = "Connecting to Server";			
		if (BtnLeaveRoom)	BtnLeaveRoom.gameObject.SetActive (true);
		
		while (PhotonNetwork.connectionState != ConnectionState.Connected){
			yield return new WaitForSeconds(0.2f);
		}

		playerBattleDao.CreateBattle (battleTypeName, bid, DateTime.Now, maxPlayers,
		(battle) =>{
			string roomName = "Room" + (PhotonNetwork.GetRoomList().Length + 1) + " : " + bMode;
			bool isVisible = true, isOpen = true;			
			ConfigurationData.battle = battle;			
			Hashtable properties = new Hashtable ();
			properties.Add ("battle", battle.ToString ());
			pw.CreateRoom (roomName, bid, isVisible, isOpen, maxPlayers, map, properties);			
			pw.SetPropertyOnPlayer ("team", 0);
			pw.SetPropertyOnPlayer ("ready", true);
			pw.TryToEnterGame (10000.0f,(other_message) =>{			
					messageActiveGame.gameObject.SetActive (true);
					Invoke ("CloseErrorMessage", 5.0f);
				},
				(playersReady, nMaxPlayers) =>{
				messageActiveGame.text = ScriptLocalization.Get("Menus/Waiting Player") + playersReady + "/" + nMaxPlayers;
				});
		});
	}

	private void CloseErrorMessage ()
	{
		errorMessage.gameObject.SetActive (false);
		marketGlow.SetActive (false);
		btnCreateRoom.gameObject.SetActive (true);
		mapSelection.gameObject.SetActive (true);
		toggleButtons.gameObject.SetActive(true);
		bidInput.gameObject.SetActive(true);	
	}

	public void BattleModeToogle (string pMode)
	{
		playerMode = pMode;		
		pw = ComponentGetter.Get<PhotonWrapper> ();		
		DefaultCallbackButton dcb;	
		
		if (playerMode == "1x1"){
			players = 2;
			battleMode = "DeathMatch";
			battleName = "DeathMatch";
		}		
		if (playerMode == "2x2"){
			players = 4;
			battleMode = "Cooperative";
			battleName = "2x2";
		}		
		if (playerMode == "4vs"){
			players = 4;
			battleMode = "DeathMatch";
			battleName = "DeathMatch";
		}		
		if (btnCreateRoom){
			dcb = ComponentGetter.Get <DefaultCallbackButton> (btnCreateRoom, false);
			dcb.Init ( null, (ht_hud) =>{				
				if (CurrentBid <= orichals && CurrentBid >= minimumBet){
					btnCreateRoom.gameObject.SetActive (false);
					mapSelection.gameObject.SetActive (false);
					toggleButtons.gameObject.SetActive(false);
					bidInput.gameObject.SetActive(false);
					StartCoroutine(CreateRoom (players, CurrentBid, battleName, battleMode, mapScene+1));
				}
				
				else if (CurrentBid < minimumBet) {
					btnCreateRoom.gameObject.SetActive (false);					
					marketGlow.SetActive (true);						
					errorMessage.text = "ranked matches require the minimum bet of "+minimumBet+" orichals";						
					errorMessage.gameObject.SetActive (true);									
					Invoke ("CloseErrorMessage", 5.0f);
				}
				
				else{
					btnCreateRoom.gameObject.SetActive (false);									
					marketGlow.SetActive (true);									
					errorMessage.text = "insuficient orichals, acquire more from the market";									
					errorMessage.gameObject.SetActive (true);									
					Invoke ("CloseErrorMessage", 5.0f);
				}
			});
		}		
	}	
	
	public void SceneSelection (string popSelect)
	{
		if (popSelect == "Dementia Forest")		mapScene = 1;
		if (popSelect == "Hollow Fields")		mapScene = 2;
		if (popSelect == "Gargantua")			mapScene = 3;
		if (popSelect == "Living Desert")		mapScene = 4;
		if (popSelect == "Sandstone Salvation")	mapScene = 5;
		if (popSelect == "Swamp King")			mapScene = 6;
		if (popSelect == "Lang Lagoon")			mapScene = 7;
		if (popSelect == "Arthanus")			mapScene = 8;

	}
}
