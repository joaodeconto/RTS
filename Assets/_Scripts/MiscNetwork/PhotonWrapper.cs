using UnityEngine;
using System.Collections;
using System.Linq;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PhotonWrapper : Photon.MonoBehaviour
{
	public delegate void StartGameCallback ();
	public delegate void ConnectionCallback (string message);
	public delegate void PlayerReadyCallback (int nPlayersReady, int nPlayers);

	public float RefreshingInterval = 0.2f;

	public GameObject[] menusDissapearWhenLogged;
	

	protected bool checkingStatus = false;

	private string playerName = "";
	private string roomNameTemp = "";
	private bool isTryingToEnterGame;
	private bool isInvokeRetryEnterLobby;
	private ConnectionCallback cb;
	private PlayerReadyCallback prc;
	private StartGameCallback sgc;

	private bool wasInitialized = false;
	public void Init ()
	{
		if (wasInitialized)
			return;

		wasInitialized = true;

		//if (!PhotonNetwork.connected)
		{
			Application.runInBackground = true;
			PhotonNetwork.networkingPeer.DisconnectTimeout = 30000;
			PhotonNetwork.ConnectUsingSettings (ConfigurationData.VERSION);
		}
	}
	
	void Update ()
	{
		if (PhotonNetwork.connectionState == ConnectionState.Disconnected)
        {
			PhotonNetwork.ConnectToMaster ( PhotonNetwork.PhotonServerSettings.ServerAddress,
											PhotonNetwork.PhotonServerSettings.ServerPort,
											PhotonNetwork.PhotonServerSettings.AppID,
											ConfigurationData.VERSION);
		}
		
//		Debug.Log ("PhotonNetwork.connectionState:" + PhotonNetwork.connectionState);
//		Debug.Log ("PhotonNetwork.connectionStateDetailed:" + PhotonNetwork.connectionStateDetailed);
	}

	public void SetPlayer (string playerName, bool isReady)
	{
		this.playerName = playerName;

		PhotonNetwork.playerName = playerName;
		PhotonPlayer player = PhotonNetwork.player;
		Hashtable someCustomPropertiesToSet = new Hashtable();
		someCustomPropertiesToSet.Add ("ready", isReady);
		player.SetCustomProperties (someCustomPropertiesToSet);
	}

	public void JoinQuickMatch ()
	{
		Hashtable expectedCustomRoomProperties = new Hashtable() { { "closeRoom", false } };
		PhotonNetwork.JoinRandomRoom (expectedCustomRoomProperties, 0);

		foreach (GameObject menu in menusDissapearWhenLogged)
		{
			menu.SetActive (false);
		}
	}

	public void JoinRoom (string roomName)
	{
		PhotonNetwork.JoinRoom (roomName);

		roomNameTemp = roomName;
		
		foreach (GameObject menu in menusDissapearWhenLogged)
		{
			menu.SetActive (false);
		}
	}

	public bool LeaveRoom ()
	{
		foreach (GameObject menu in menusDissapearWhenLogged)
		{
			menu.SetActive (true);
		}
		
		if (PhotonNetwork.room == null)
			return false;

		PhotonNetwork.LeaveRoom ();

		return true;
	}

	public void CreateTestRoom ()
	{
		CreateRoom ("test_room_" + (PhotonNetwork.GetRoomList().Length + 1) + (Random.value * 10000), 1, true, true, 1, 1);

		foreach (GameObject menu in menusDissapearWhenLogged)
		{
			menu.SetActive (false);
		}
	}

	public void CreateRoom (string roomName, int bid, bool isVisible, bool isOpen, int maxPlayers, int map,  Hashtable customProperties = null)
	{
		if (customProperties == null)
			customProperties = new Hashtable ();

		customProperties.Add ("closeRoom", false);
		customProperties.Add ("bid", bid);
		customProperties.Add ("map", map);
		
		string[] roomPropsInLobby = { "closeRoom", "bool", "bid", "int" };

		PhotonNetwork.CreateRoom (roomName, isVisible, isOpen, maxPlayers, customProperties, roomPropsInLobby);
		
		roomNameTemp = roomName;
		
		foreach (GameObject menu in menusDissapearWhenLogged)
		{
			menu.SetActive (false);
		}
	}

	public RoomInfo[] GetRoomList ()
	{
		return PhotonNetwork.GetRoomList ();
	}

	public object GetPropertyOnPlayer (string key)
	{
		return PhotonNetwork.player.customProperties[key];
	}

	public void SetPropertyOnPlayer (string key, object value)
	{
		Hashtable someCustomPropertiesToSet = new Hashtable();
		someCustomPropertiesToSet.Add (key, value);

		PhotonNetwork.player.SetCustomProperties (someCustomPropertiesToSet);
	}
	
	public Room GetCurrentRoom ()
	{
		return PhotonNetwork.room;
	}

	public object GetPropertyOnRoom (string key)
	{
		if (PhotonNetwork.room == null)
			return null;
		
		if (PhotonNetwork.room.customProperties == null)
			return null;
		
		return PhotonNetwork.room.customProperties[key];
	}

	public void SetPropertyOnRoom (string key, object value)
	{
		Hashtable someCustomPropertiesToSet = new Hashtable();
		someCustomPropertiesToSet.Add (key, value);

		PhotonNetwork.room.SetCustomProperties (someCustomPropertiesToSet);
	}

	public string CheckingStatus ()
	{
		PeerState peerState = PhotonNetwork.connectionStateDetailed;

		// Verificando conexão
		if (peerState == PeerState.ConnectingToMasterserver)
		{
			return "Checking connection";
		}
		// Conectado
		else if (peerState == PeerState.ConnectedToMaster)
		{
			return "Connected";
		}
		// Desconectado ou sem conexão
		else if (peerState == PeerState.Disconnected)
		{
			return "Disconnected";
		}
		else if (peerState == PeerState.Authenticated)
		{
			return "Authenticated Network Settings";
		}
		else if (peerState == PeerState.JoinedLobby)
		{
			return "Joined Game Network";
		}
		else if (peerState == PeerState.DisconnectingFromMasterserver)
		{
			return "Checking Server";
		}
		else if(peerState == PeerState.ConnectingToGameserver)
		{
			return "Connecting to Game Server";
		}
		else if(peerState == PeerState.ConnectedToGameserver)
		{
			return "Connected to Game Server";
		}
		// Conexão criada
		else if (peerState == PeerState.PeerCreated)
		{
			return "Connection Created";
		}

		return peerState.ToString ();
	}

	// Photon Methods
	private void OnJoinedRoom()
	{
		checkingStatus = false;

		SetPlayer ( playerName, true );
		SetPropertyOnPlayer ("team", PhotonNetwork.playerList.Length - 1);

		if (PhotonNetwork.room.customProperties.ContainsKey ("battle"))
			ConfigurationData.battle = new Model.Battle ((string)PhotonNetwork.room.customProperties["battle"]);
		else
			Debug.LogWarning ("Nao contem battle key in the CustomProperties of the joined room");
	}

	private void OnCreatedRoom()
	{
		checkingStatus = false;

		SetPlayer ( playerName, true );
	}

	public void OnLeftRoom()
	{
		PhotonNetwork.player.customProperties.Remove("team");
		PhotonNetwork.player.customProperties.Remove("ready");
	}

	private void OnDisconnectedFromPhoton()
	{

	}

	private void OnFailedToConnectToPhoton(object parameters)
	{
		Debug.LogWarning ("OnFailedToConnectToPhoton. StatusCode: " + parameters);
	}

	public void SetStartGame (StartGameCallback sgc)
	{
		this.sgc = sgc;
	}

	public void TryToEnterGame (float maxTimeToWait, ConnectionCallback cb, PlayerReadyCallback prc = null)
	{
		this.cb  = cb;
		this.prc = prc;

		InvokeRepeating ("_TryToEnterGame", 1.0f, RefreshingInterval);
		Invoke ("StopTryingEnterGame", maxTimeToWait);
	}

	private void StopTryingEnterGame ()
	{
		CancelInvoke ("_TryToEnterGame");

		cb ("Timeout");
	}

	private void _TryToEnterGame ()
	{
		Room room = PhotonNetwork.room;
		
		if (PhotonNetwork.room == null)
		{
			if (PhotonNetwork.isNonMasterClientInRoom)
			{
				if (!isInvokeRetryEnterLobby)
				{
					Invoke ("RetryEnterLobby", 5f);
					isInvokeRetryEnterLobby = true;
				}
			}
			return;
		}
		else
		{
			if (PhotonNetwork.isNonMasterClientInRoom)
			{
				if (isInvokeRetryEnterLobby)
				{
					CancelInvoke ("RetryEnterLobby");
					isInvokeRetryEnterLobby = false;
				}
			}
		}

		int numberOfReady = 0;
		foreach (PhotonPlayer p in PhotonNetwork.playerList)
		{
			if (      p.customProperties.ContainsKey("ready") &&
			    (bool)p.customProperties["ready"] == true)
			{
				numberOfReady++;
			}
		}

		if (prc != null)
		{
			prc (numberOfReady, room.maxPlayers);
		}

		if (numberOfReady == room.maxPlayers)
		{
			StopTryingEnterGame ();
			StartCoroutine (CallStartGameCallback ());
		}
	}
	
	void RetryEnterLobby ()
	{
		JoinRoom (roomNameTemp);
		isInvokeRetryEnterLobby = false;
	}
	
	IEnumerator CallStartGameCallback ()
	{
		yield return new WaitForSeconds (2f);
		
		if (PhotonNetwork.isMasterClient)
		{
			Room room = PhotonNetwork.room;
			
			Hashtable roomProperty = new Hashtable() {{"closeRoom", true}};
					room.SetCustomProperties (roomProperty);
					
			if (GameplayManager.mode == GameplayManager.Mode.Cooperative)
			{
				int maxNumberOfAllies = room.maxPlayers / 2;
				
				int numberAllies0 = 0;
				int numberAllies1 = numberAllies0;
				
				foreach (PhotonPlayer pp in PhotonNetwork.playerList)
				{
					int numberRaffled = 0;
//					int numberRaffled = Random.Range(0, 2);
					
//					if (numberRaffled == 2) numberRaffled = 1;
					
					if (numberAllies0 == maxNumberOfAllies)
						numberRaffled = 1;
					else if (numberAllies1 == maxNumberOfAllies)
						numberRaffled = 0;
					
					if (numberRaffled == 0)
						numberAllies0++;
					else
						numberAllies1++;
					
					photonView.RPC ("SetAllyOnPlayerRPC", pp, "allies", numberRaffled);
				}
				
				photonView.RPC ("SetGameplayMode", PhotonTargets.Others, 1);
			}
			else
			{
				photonView.RPC ("SetGameplayMode", PhotonTargets.Others, 0);
			}
		}
		
		ConfigurationData.InGame = true;
		
		yield return new WaitForSeconds (2f);
		
		sgc ();
	}

	public void StartGame ()
	{
		StartCoroutine (YieldStartGame ());
	}

	private IEnumerator YieldStartGame ()
    {
        while (PhotonNetwork.room == null)
		{
			yield return 0;
		}
		
		PhotonNetwork.isMessageQueueRunning = false;

    }

	
	// RPCS

	[RPC]
	public void SetAllyOnPlayerRPC (string key, int value)
	{
		Hashtable someCustomPropertiesToSet = new Hashtable();
		someCustomPropertiesToSet.Add (key, value);

		PhotonNetwork.player.SetCustomProperties (someCustomPropertiesToSet);
	}

	[RPC]
	public void SetGameplayMode (int mode)
	{
		switch (mode)
		{
		case 0:
			GameplayManager.mode = GameplayManager.Mode.Deathmatch;
			break;
		case 1:
			GameplayManager.mode = GameplayManager.Mode.Cooperative;
			break;
		case 2:
			GameplayManager.mode = GameplayManager.Mode.Survival;
			break;
		default:
			// do nothing
			break;
		}
	}
}
