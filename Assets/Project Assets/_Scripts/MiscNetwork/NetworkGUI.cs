#define GUI

using UnityEngine;
using System.Collections;

public class NetworkGUI : Photon.MonoBehaviour {
	
#if GUI
	protected delegate void GUIMethod ();
	
	protected GUIMethod CurrentGUI;
	
	void Start ()
	{
		if (!PhotonNetwork.connected)
        {
			PhotonNetwork.ConnectUsingSettings(ConfigurationData.VERSION);
		}
		
		CurrentGUI = VerifyStatusGUI;
	}
	
	void OnGUI ()
	{
		CurrentGUI ();
	}
	
	// Métodos de interface para GUI
	void MainMenu ()
	{
		if (GUILayout.Button ("Play Quick Game"))
		{
			PlayQuickGame ();
			CurrentGUI = VerifyStatusGUI;
		}
		if (GUILayout.Button ("Join a game"))
		{
			CurrentGUI = JoinGame;
		}
		
	}
	
	protected Vector2 position = Vector2.zero;
	
	void JoinGame ()
	{
		position = GUILayout.BeginScrollView (position, "box");
		
		if (PhotonNetwork.GetRoomList().Length == 0)
		{
			GUILayout.Label ("There's anywhere room");
		}
		else
		{
			foreach (RoomInfo room in PhotonNetwork.GetRoomList ())
			{
				if (GUILayout.Button (room.customProperties + " - Players: " + room.playerCount + "/" + room.maxPlayers))
				{
					JoinRoom (room.name);
					CurrentGUI = VerifyStatusGUI;
				}
			}
		}
		
		GUILayout.EndScrollView ();
		
		GUILayout.Space (10f);
		
		if (GUILayout.Button ("Create a lobby"))
		{
			CurrentGUI = CreateLobby;
		}
		
		if (GUILayout.Button ("Back")) CurrentGUI = MainMenu;
	}
	
	protected string nameRoom = "";
	
	void CreateLobby ()
	{
		bool error = false;
		
		GUILayout.Label ("Name:");
		nameRoom = GUILayout.TextField (nameRoom, 12);
		if (string.IsNullOrEmpty(nameRoom))
		{
			ErrorMessage ("Name is empty");
			error = true;
		}
		
		GUILayout.Space (10f);
		
		GUI.enabled = !error;
		if (GUILayout.Button ("Create"))
		{
			CreateRoom (nameRoom, true, true, 4);
			CurrentGUI = VerifyStatusGUI;
		}
		
		GUI.enabled = true;
		if (GUILayout.Button ("Back")) CurrentGUI = JoinGame;
	}
	
	void ShowRoom ()
	{
		PhotonPlayer[] players = PhotonNetwork.playerList;
		
		foreach (PhotonPlayer p in players)
		{
			GUILayout.Label (p.name);
		}
	}
	
	PeerState ps;
	
	void VerifyStatusGUI ()
	{
		GUILayout.Label(VerifyStatus ());
		
		if (ps != PhotonNetwork.connectionStateDetailed)
		{
			ps = PhotonNetwork.connectionStateDetailed;
			
			if (ps == PeerState.JoinedLobby)
			{
				CurrentGUI = MainMenu;
			}
		}
	}
	
	public void ErrorMessage (string message)
	{
		Color tempColor = GUI.color;
		GUI.color = Color.red;
		
		GUILayout.Label (message);
		
		GUI.color = tempColor;
	}
	
	void SetNamePlayer ()
	{
		PhotonNetwork.playerName = "Player " + PhotonNetwork.playerList.Length;
	}
	
#else
	public NetworkGUI instance;
	
	void Awake ()
	{
		if (!PhotonNetwork.connected)
        {
			PhotonNetwork.ConnectUsingSettings(ConfigurationData.VERSION);
		}
		
		instance = this;
	}
	
#endif
	
	// Métodos para aplicar em GUI e NGUI
	
	public void PlayQuickGame ()
	{
		PhotonNetwork.JoinRandomRoom ();
	}
	
	public void JoinRoom (string roomName)
	{
		PhotonNetwork.JoinRoom (roomName);
	}
	
	public void CreateRoom (string roomName, bool isVisible, bool isOpen, int maxPlayers)
	{
		PhotonNetwork.CreateRoom (roomName, isVisible, isOpen, maxPlayers);
	}
	
	public string VerifyStatus ()
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
		// Conexão criada
		else if (peerState == PeerState.PeerCreated)
		{
			return "Connection created";
		}
		
		return peerState.ToString ();
	}
	
	// Photon Methods
	
	private void OnJoinedRoom()
    {
		CurrentGUI = ShowRoom;
		SetNamePlayer ();
    }

    private void OnCreatedRoom()
    {
        CurrentGUI = ShowRoom;
		SetNamePlayer ();
    }

    private void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from Photon.");
    }

    private void OnFailedToConnectToPhoton(object parameters)
    {
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters);
    }
}
