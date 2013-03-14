#define GUI

using UnityEngine;
using System.Collections;
using System.Linq;

public class NetworkGUI : Photon.MonoBehaviour {
	
	public string playerName = "";
	
	protected bool checkingStatus = false;
	
#if GUI
	protected delegate void GUIMethod ();
	
	protected GUIMethod CurrentGUI;
	
	void Start ()
	{
		if (!PhotonNetwork.connected)
        {
			PhotonNetwork.ConnectUsingSettings(ConfigurationData.VERSION);
		}
		
		Application.runInBackground = true;
		
		qualityNames = QualitySettings.names;
		
		CurrentGUI = MainMenu;
	}
	
	void OnGUI ()
	{
		if (PhotonNetwork.connectionState == ConnectionState.Disconnected)
        {
			PhotonNetwork.Connect ( PhotonNetwork.PhotonServerSettings.ServerAddress,
									PhotonNetwork.PhotonServerSettings.ServerPort,
									PhotonNetwork.PhotonServerSettings.AppID,
									ConfigurationData.VERSION);
		}
		
		if (PhotonNetwork.room == null &&
			!checkingStatus)
		{
			bool error = false;
			
			GUILayout.Label ("Player name:");
			playerName = GUILayout.TextField (playerName, 12, GUILayout.Width(200f));
			if (string.IsNullOrEmpty(playerName))
			{
				ErrorMessage ("Name is empty");
				
				error = true;
			}
			
			if (error) return;
			
		}
		
		CurrentGUI ();
	}
	
	// Métodos de interface para GUI
	void MainMenu ()
	{
		BeginCenter ();
		
		GUI.enabled = PhotonNetwork.connected;
		if (GUILayout.Button ("Play Quick Game"))
		{
			PlayQuickGame ();
			Invoke ("NoRoom", 10f);
			CurrentGUI = CheckingRoom;
		}
		
		GUILayout.Space (10f);
		
		if (GUILayout.Button ("Join a game"))
		{
			CurrentGUI = JoinGame;
		}
		
		GUILayout.Space (10f);
		
		if (GUILayout.Button ("Options"))
		{
			CurrentGUI = Options;
		}
		
		GUILayout.Space (10f);
		
		if (!PhotonNetwork.connected) 
		{
			GUI.enabled = true;
			ErrorMessage ("Not have a conection!");
		}
		
		EndCenter ();
		
	}
	
	string[] qualityNames;
	
	void Options ()
	{
		BeginCenter ();
		
		int quality = GUILayout.Toolbar (QualitySettings.GetQualityLevel (), qualityNames); 
		QualitySettings.SetQualityLevel (quality);
		
		GUILayout.Space (10f);
		
		if (GUILayout.Button ("Back"))
		{
			CurrentGUI = MainMenu;
		}
		
		EndCenter ();
	}
	
	protected Vector2 position = Vector2.zero;
	
	void JoinGame ()
	{
		BeginCenter ();
		
		position = GUILayout.BeginScrollView (position, "box");
		
		if (PhotonNetwork.GetRoomList().Length == 0)
		{
			GUILayout.Label ("There's anywhere room");
		}
		else
		{
			RoomInfo[] roomQuery =
            (from r in PhotonNetwork.GetRoomList ()
            where (bool)r.customProperties["closeRoom"] == false &&
					r.playerCount != r.maxPlayers
            select r).ToArray ();
			
			if (roomQuery.Length == 0)
			{
				GUILayout.Label ("There's anywhere room");
			}
			else
			{
				foreach (RoomInfo room in roomQuery)
				{
					if (GUILayout.Button (room.name + " - Players: " + room.playerCount + "/" + room.maxPlayers))
					{
						JoinRoom (room.name);
						CurrentGUI = CheckingStatusGUI;
					}
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
		
		EndCenter ();
	}
	
	protected string nameRoom = "";
	protected int numberOfPlayers = 4;
	
	void CreateLobby ()
	{
		BeginCenter ();
		
		bool error = false;
		
		GUILayout.Label ("Name of room:");
		nameRoom = GUILayout.TextField (nameRoom, 12, GUILayout.Width(200f));
		if (string.IsNullOrEmpty(nameRoom))
		{
			ErrorMessage ("Name is empty");
			error = true;
		}
		
		GUILayout.Label ("Number of Players:");
		GUILayout.BeginHorizontal ();
		numberOfPlayers = Mathf.FloorToInt(GUILayout.HorizontalSlider ((float)numberOfPlayers, 1f, 4f));
		GUILayout.Label (numberOfPlayers+"", GUILayout.Width(25f));
		GUILayout.EndHorizontal ();
		
		GUILayout.Space (10f);
		
		GUI.enabled = !error;
		if (GUILayout.Button ("Create"))
		{
			CreateRoom (nameRoom, true, true, numberOfPlayers);
			CurrentGUI = CheckingStatusGUI;
		}
		
		GUI.enabled = true;
		if (GUILayout.Button ("Back")) CurrentGUI = JoinGame;
		
		EndCenter ();
	}
	
	string masterRoom = "";
	
	void ShowRoom ()
	{
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Room name:", GUILayout.Width(150f));
		GUILayout.Label (PhotonNetwork.room.name);
		GUILayout.EndHorizontal ();
		
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Master Room:", GUILayout.Width(150f));
		GUILayout.Label (masterRoom);
		GUILayout.EndHorizontal ();
		
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("N:", GUILayout.Width(50f));
		GUILayout.Label ("Name:", GUILayout.Width(150f));
		GUILayout.Label ("Team:", GUILayout.Width(100f));
		GUILayout.EndHorizontal ();
		
		int numberOfReady = 0;
		
		int i = 0;
		
		PhotonPlayer player = PhotonNetwork.player;
		
		foreach (PhotonPlayer p in PhotonNetwork.playerList)
		{
			GUI.color = i == 0 ? Color.green : Color.white;
			
			GUILayout.BeginHorizontal ();
			
			GUILayout.Label ((i+1)+"", GUILayout.Width(50f));
			
			GUILayout.Label (p.name, GUILayout.Width(150f));
			
			if (p.customProperties["team"] != null)
			{
				GUILayout.Label((int)p.customProperties["team"]+"", GUILayout.Width(100f));
			}
			
			if (p.customProperties["ready"] != null)
			{
				if((bool)p.customProperties["ready"] == true)
				{
					GUILayout.Label("Yeah, I'm Ready!");
					numberOfReady++;
				}
				else if((bool)p.customProperties["ready"] == false)
				{
					GUILayout.Label("No, I'm not Ready!");
				}
			}
			GUILayout.EndHorizontal ();
			
			if (p.isMasterClient) masterRoom = p.name;
			
			i++;
		}
		
		GUI.color = Color.white;
		
		if ((PhotonNetwork.room.maxPlayers - PhotonNetwork.playerList.Length) != 0)
		{
			for (int k = 0; k != PhotonNetwork.room.maxPlayers - PhotonNetwork.playerList.Length; k++)
			{
				GUILayout.Label ((k+PhotonNetwork.playerList.Length+1)+"", GUILayout.Width(50f));
			}
		}
		
		GUILayout.Space (10f);
		
		if (player.customProperties["team"] == null)
		{
			Hashtable teamProperty = new Hashtable();
			teamProperty.Add ("team", PhotonNetwork.playerList.Length-1);
			player.SetCustomProperties(teamProperty);
		}
//		else
//		{
//			Hashtable teamProperty = new Hashtable() {{"team", PhotonNetwork.playerList.Length-1}};
//			player.SetCustomProperties (teamProperty);
//		}
		
		bool ready = GUILayout.Toggle ((bool)player.customProperties["ready"], " Ready?");
		Hashtable readyPropety = new Hashtable() {{"ready", ready}};
		player.SetCustomProperties (readyPropety);
		
		if (GUILayout.Button ("Back to Main Menu"))
		{
			player.customProperties.Remove("team");
			player.customProperties.Remove("ready");
			PhotonNetwork.LeaveRoom ();
		}
		
		if (numberOfReady == PhotonNetwork.room.maxPlayers)
		{
			if (PhotonNetwork.isMasterClient)
			{
				Hashtable roomProperty = new Hashtable() {{"closeRoom", true}};
				PhotonNetwork.room.SetCustomProperties(roomProperty);
			}
			StartCoroutine (StartGame ());
		}
	}
	
	PeerState ps;
	
	void CheckingStatusGUI ()
	{
		BeginCenter ();
		
		GUILayout.Label(CheckingStatus ());
		
		EndCenter ();
		
		checkingStatus = true;
	}
	
	void CheckingRoom ()
	{
		BeginCenter ();
		
		GUILayout.Label("Searching a room...");
		
		EndCenter ();
		
		checkingStatus = true;
	}
	
	void NoRoom ()
	{
		checkingStatus = false;
		CurrentGUI = MainMenu;
	}
	
	public void ErrorMessage (string message)
	{
		Color tempColor = GUI.color;
		GUI.color = Color.red;
		
		GUILayout.Label (message);
		
		GUI.color = tempColor;
	}
	
	void SetPlayer ()
	{
		PhotonNetwork.playerName = playerName;
		PhotonPlayer player = PhotonNetwork.player;
		Hashtable someCustomPropertiesToSet = new Hashtable();
		someCustomPropertiesToSet.Add ("ready", false);
		player.SetCustomProperties (someCustomPropertiesToSet);
	}
	
	void BeginCenter ()
	{
		GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
	    GUILayout.FlexibleSpace();
	    GUILayout.BeginHorizontal();
	    GUILayout.FlexibleSpace();
		
		GUILayout.BeginVertical ();
	}
	
	void EndCenter ()
	{
		GUILayout.EndVertical ();
 
	    GUILayout.FlexibleSpace();
	    GUILayout.EndHorizontal();
	    GUILayout.FlexibleSpace();
	    GUILayout.EndArea();
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
		Hashtable expectedCustomRoomProperties = new Hashtable() { { "closeRoom", false } };
		PhotonNetwork.JoinRandomRoom (expectedCustomRoomProperties, 0);
	}
	
	public void JoinRoom (string roomName)
	{
		PhotonNetwork.JoinRoom (roomName);
	}
	
	public void CreateRoom (string roomName, bool isVisible, bool isOpen, int maxPlayers)
	{
		Hashtable someCustomPropertiesToSet = new Hashtable();
		someCustomPropertiesToSet.Add ("closeRoom", false);
		string[] roomPropsInLobby = { "closeRoom", "bool" };
		
		PhotonNetwork.CreateRoom (roomName, isVisible, isOpen, maxPlayers, someCustomPropertiesToSet, roomPropsInLobby);
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
		
		SetPlayer ();
#if GUI
		// Caso for em Play Quick Game
		CancelInvoke ("NoRoom");
		
		CurrentGUI = ShowRoom;
#endif
    }

    private void OnCreatedRoom()
    {
		checkingStatus = false;
		
		SetPlayer ();
		
		Hashtable someCustomPropertiesToSet = new Hashtable();
		someCustomPropertiesToSet.Add ("playerLoads", 0);
		PhotonNetwork.room.SetCustomProperties (someCustomPropertiesToSet);
		
#if GUI
		CurrentGUI = ShowRoom;
#endif
    }
	
	public void OnLeftRoom()
    {
#if GUI
		CurrentGUI = MainMenu;
#endif
    }

    private void OnDisconnectedFromPhoton()
    {
#if GUI
		CurrentGUI = MainMenu;
#endif
    }

    private void OnFailedToConnectToPhoton(object parameters)
    {
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters);
    }
	
	private IEnumerator StartGame ()
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
