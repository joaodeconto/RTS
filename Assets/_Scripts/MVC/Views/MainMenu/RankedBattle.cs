using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RankedBattle : MonoBehaviour
{
	public UILabel messageActiveGame;
	public UILabel errorMessage;
	public UILabel cristal;
	public UIInput bidInput;
	public GameObject marketGlow;


	public UIPopupList mapSelection;
	public string playerMode = "1x1";

	public Transform BtnLeaveRoom;
	public Transform createRoom;

	public int minimumBet = 1;
	private int players = 2;
	private int mapScene;
	private Transform toggleButtons;
	private int orichals {

		get {return (cristal) ? int.Parse (cristal.text) :1;}
		
		}

	private string battleMode;
	private string battleName;
	private int CurrentBid {
	
		get { return (bidInput) ? int.Parse (bidInput.value) : 1; }
	}



	bool wasInitialized      = false;

	PhotonWrapper pw;

	public void OnEnable ()
	{
		Open ();
	}

	public void BattleModeToogle (string pMode)
	{
		playerMode = pMode;

				
		if (playerMode == "1x1")
		{
			players = 2;
			battleMode = "DeathMatch";
			battleName = "DeathMatch";
		}
		
		if (playerMode == "2x2")
		{
			players = 4;
			battleMode = "Cooperative";
			battleName = "2x2";
		}

		if (playerMode == "4vs")
		{
			players = 4;
			battleMode = "DeathMatch";
			battleName = "DeathMatch";
		}

		pw = ComponentGetter.Get<PhotonWrapper> ();
		
		DefaultCallbackButton dcb;
		
		
		if (createRoom)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (createRoom, false);
			dcb.Init ( null, (ht_hud) =>
			          {

								if (CurrentBid <= orichals && CurrentBid >= minimumBet)
								{
									CreateRoom (players, CurrentBid, battleName, battleMode, mapScene);
									createRoom.gameObject.SetActive (false);
									mapSelection.gameObject.SetActive (false);
									toggleButtons.gameObject.SetActive(false);
									bidInput.gameObject.SetActive(false);
								}

								else if (CurrentBid < minimumBet)
							    {
									Debug.Log("message: ");

									createRoom.gameObject.SetActive (false);
					
									marketGlow.SetActive (true);
						
									errorMessage.text = "ranked matches require the minimum bet of "+minimumBet+" orichals";
						
									errorMessage.gameObject.SetActive (true);
									
									Invoke ("CloseErrorMessage", 5.0f);
								}
												
								else
								{
									Debug.Log("message: ");

									createRoom.gameObject.SetActive (false);
									
									marketGlow.SetActive (true);
									
									errorMessage.text = "insuficient orichals, acquire more from the market";
									
									errorMessage.gameObject.SetActive (true);
									
									Invoke ("CloseErrorMessage", 5.0f);
								}
				
				
			} );
		}
		
	}

	
	
	public void Open ()
	{
		if (wasInitialized)
			return;
		
		wasInitialized = true;

		toggleButtons = this.transform.FindChild("Menu").FindChild("Buttons").transform;

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

		mapSelection.gameObject.SetActive (true);
		toggleButtons.gameObject.SetActive(true);
		bidInput.gameObject.SetActive(true);

		createRoom.gameObject.SetActive (true);

		messageActiveGame.gameObject.SetActive (false);

		marketGlow.SetActive (false);

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



		//TODO refazer as battles
		playerBattleDao.CreateBattle (battleTypeName, bid, DateTime.Now, maxPlayers,
		(battle) =>
		{
//			Debug.Log ("message: " + message);
//			Debug.Log ("playerBattle: " + playerBattle);

			string roomName = "Room" + (PhotonNetwork.GetRoomList().Length + 1) + " : " + bMode;
			bool isVisible = true, isOpen = true;
			
			ConfigurationData.battle = battle;
			
			Hashtable properties = new Hashtable ();
			properties.Add ("battle", battle.ToString ());

			pw.CreateRoom (roomName, bid, isVisible, isOpen, maxPlayers, map, properties);
			
			pw.SetPropertyOnPlayer ("team", 0);
			pw.SetPropertyOnPlayer ("ready", true);

		//	VDebug.Log ("battle: " + properties["battle"]);


			messageActiveGame.gameObject.SetActive (true);
			messageActiveGame.text = "Connecting to Server";
			
			if (BtnLeaveRoom)
			{
				BtnLeaveRoom.gameObject.SetActive (true);
			}

			pw.TryToEnterGame (10000.0f,
			(other_message) =>
			{
			
				messageActiveGame.gameObject.SetActive (true);

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
		marketGlow.SetActive (false);
		createRoom.gameObject.SetActive (true);
		mapSelection.gameObject.SetActive (true);
		toggleButtons.gameObject.SetActive(true);
		bidInput.gameObject.SetActive(true);
	
	}

	public void SceneSelection (string popSelect)
	{
		if (popSelect == "Dementia Forest")
		{
			mapScene = 1;
		}
		
		if (popSelect == "Living Desert")
		{
			mapScene = 2;
		}

		if (popSelect == "Swamp King")
		{
			mapScene = 3;
		}

		if (popSelect == "Hollow Fields")
		{
			mapScene = 4;

		}
		if (popSelect == "Sandstone Salvation")
		{
			mapScene = 5;
		}
	}
}
