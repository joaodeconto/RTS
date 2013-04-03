using UnityEngine;
using System.Collections;
using System.Linq;

public class PhotonWrapper : Photon.MonoBehaviour
{
	string playerName = "";

	protected bool checkingStatus = false;

	private bool wasInitialized = false;
	public void Init ()
	{
		if (wasInitialized)
			return;

		wasInitialized = true;

		if (!PhotonNetwork.connected)
			 PhotonNetwork.ConnectUsingSettings (ConfigurationData.VERSION);

		Application.runInBackground = true;
	}

	void Awake ()
	{
		Init ();
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
	}

	public void JoinRoom (string roomName)
	{
		PhotonNetwork.JoinRoom (roomName);
	}

	public void CreateTestRoom ()
	{
		CreateRoom ("test_room_" + (PhotonNetwork.GetRoomList().Length + 1) + (Random.value * 10000), true, true, 1);
	}

	public void CreateRoom (string roomName, bool isVisible, bool isOpen, int maxPlayers)
	{
		Hashtable someCustomPropertiesToSet = new Hashtable();
		someCustomPropertiesToSet.Add ("closeRoom", false);
		string[] roomPropsInLobby = { "closeRoom", "bool" };

		PhotonNetwork.CreateRoom (roomName, isVisible, isOpen, maxPlayers, someCustomPropertiesToSet, roomPropsInLobby);
	}

	public RoomInfo[] GetRoomList ()
	{
		return  (from r in PhotonNetwork.GetRoomList ()
									where ((bool)r.customProperties["closeRoom"] == false)
									   && (r.playerCount != r.maxPlayers)
									select r).ToArray ();
	}

	public void SetProperty (string key, object value)
	{
		if (PhotonNetwork.player.customProperties.ContainsKey (key))
			PhotonNetwork.player.customProperties [key] = value;
		else
			PhotonNetwork.player.customProperties.Add (key, value);
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
	}

	private void OnCreatedRoom()
	{
		checkingStatus = false;

		SetPlayer ( playerName, true );

		Hashtable someCustomPropertiesToSet = new Hashtable();
		someCustomPropertiesToSet.Add ("playerLoads", 0);
		PhotonNetwork.room.SetCustomProperties (someCustomPropertiesToSet);
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

	public IEnumerator StartGame ()
	{
		while (PhotonNetwork.room == null)
		{
			yield return 0;
		}

		// Temporary disable processing of futher network messages
		PhotonNetwork.isMessageQueueRunning = false;
		Application.LoadLevel(1);
	}
}
