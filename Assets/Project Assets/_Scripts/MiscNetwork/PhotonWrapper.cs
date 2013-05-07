using UnityEngine;
using System.Collections;
using System.Linq;

public class PhotonWrapper : Photon.MonoBehaviour
{
	public delegate void ConnectionCallback (string message);
	public delegate void PlayerReadyCallback (int nPlayersReady, int nPlayers);

	public float RefreshingInterval = 0.2f;

	public GameObject[] menusDissapearWhenLogged;

	protected bool checkingStatus = false;

	private string playerName = "";
	private bool isTryingToEnterGame;
	private ConnectionCallback cb;
	private PlayerReadyCallback prc;

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

		foreach (GameObject menu in menusDissapearWhenLogged)
		{
			menu.SetActive (false);
		}
	}

	public bool LeaveRoom ()
	{
		if (PhotonNetwork.room == null)
			return false;

		PhotonNetwork.LeaveRoom ();

		foreach (GameObject menu in menusDissapearWhenLogged)
		{
			menu.SetActive (true);
		}

		return true;
	}

	public void CreateTestRoom ()
	{
		CreateRoom ("test_room_" + (PhotonNetwork.GetRoomList().Length + 1) + (Random.value * 10000), true, true, 1);

		foreach (GameObject menu in menusDissapearWhenLogged)
		{
			menu.SetActive (false);
		}
	}

	public void CreateRoom (string roomName, bool isVisible, bool isOpen, int maxPlayers)
	{
		Hashtable someCustomPropertiesToSet = new Hashtable();
		someCustomPropertiesToSet.Add ("closeRoom", false);
		string[] roomPropsInLobby = { "closeRoom", "bool" };

		PhotonNetwork.CreateRoom (roomName, isVisible, isOpen, maxPlayers, someCustomPropertiesToSet, roomPropsInLobby);

		foreach (GameObject menu in menusDissapearWhenLogged)
		{
			menu.SetActive (false);
		}
	}

	public RoomInfo[] GetRoomList ()
	{
		return PhotonNetwork.GetRoomList ();
	}

	public void SetPropertyOnPlayer (string key, object value)
	{
		Hashtable someCustomPropertiesToSet = new Hashtable();
		someCustomPropertiesToSet.Add (key, value);
		
		PhotonNetwork.player.SetCustomProperties (someCustomPropertiesToSet);
	}

	public string CheckingStatus ()
	{
		PeerState peerState = PhotonNetwork.connectionStateDetailed;

		// Verificando conexão
		if (peerState == PeerState.Connecting)
		{
			return "Checking connection";
		}
		// Conectado
		else if (peerState == PeerState.Connected)
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

		if (room == null)
		{
			//Debug.Log("Algo estranho por aqui");
			return;
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
			if (PhotonNetwork.isMasterClient)
			{
				Hashtable roomProperty = new Hashtable() {{"closeRoom", true}};
				room.SetCustomProperties(roomProperty);
				
				if (GameplayManager.mode == GameplayManager.Mode.Allies)
				{
					int maxNumberOfAllies = room.maxPlayers / 2;
					
					int numberAllies0 = 0;
					int numberAllies1 = numberAllies0;
					
					foreach (PhotonPlayer pp in PhotonNetwork.playerList)
					{
						int numberRaffled = Random.Range(0, 1);
						
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
			StopTryingEnterGame ();
			StartCoroutine (StartGame ());
		}
	}

	private IEnumerator StartGame ()
    {
        while (PhotonNetwork.room == null)
        {
            yield return 0;
        }
		
		yield return new WaitForSeconds (3f);
		
        // Temporary disable processing of futher network messages
        PhotonNetwork.isMessageQueueRunning = false;
		Application.LoadLevel(1);
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
			GameplayManager.mode = GameplayManager.Mode.Normal;
			break;
		case 1:
			GameplayManager.mode = GameplayManager.Mode.Allies;
			break;
		default:
			// do nothing
			break;
		}
	}
}
