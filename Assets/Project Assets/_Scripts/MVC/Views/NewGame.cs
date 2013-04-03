using UnityEngine;
using System.Collections;

public class NewGame : MonoBehaviour
{
	float RefreshingInterval = 2.0f;
	bool wasInitialized = false;

	public void Open ()
	{
		if (wasInitialized)
			return;

		wasInitialized = true;

		string roomName;
		bool isVisible = true, isOpen = true;
		int maxPlayers;
		Hashtable properties;
		string[] roomPropsInLobby;

		DefaultCallbackButton dcb;

		Transform menu = this.transform.FindChild ("Menu");

		GameObject quickMatch = menu.FindChild ("Quick Match").gameObject;

		dcb = quickMatch.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, (ht_hud) =>
							{
								Hashtable roomProperties = new Hashtable() { { "closeRoom", false } };
								PhotonNetwork.JoinRandomRoom (roomProperties, 0);

								//TODO fazer timeout de conexão
							});

		GameObject match1x1 = menu.FindChild ("Match 1x1").gameObject;

		dcb = match1x1.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, (ht_hud) =>
							{
								PhotonNetwork.player.customProperties["team"] = 0;

								roomName = "Room" + PhotonNetwork.GetRoomList().Length + 1;
								maxPlayers = 2;
								properties = new Hashtable() { {"closeRoom", false } };
								roomPropsInLobby = new string[] { "closeRoom", "bool" };
								PhotonNetwork.CreateRoom(roomName, isVisible,
														 isOpen, maxPlayers,
														 properties, roomPropsInLobby);


								InvokeRepeating ("TryToEnterGame", 0.0f, RefreshingInterval);
							});

		GameObject match2x2 = menu.FindChild ("Match 2x2").gameObject;

		dcb = match2x2.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, (ht_hud) =>
							{
								PhotonNetwork.player.customProperties["team"] = 0;

								roomName = "Room" + PhotonNetwork.GetRoomList().Length + 1;
								maxPlayers = 3;
								properties = new Hashtable() { {"closeRoom", false } };
								roomPropsInLobby = new string[] { "closeRoom", "bool" };
								PhotonNetwork.CreateRoom(roomName, isVisible,
														 isOpen, maxPlayers,
														 properties, roomPropsInLobby);

								InvokeRepeating ("TryToEnterGame", 0.0f, RefreshingInterval);
							});

		GameObject matchTxT = menu.FindChild ("Match TxT").gameObject;

		dcb = matchTxT.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, (ht_hud) =>
							{
								PhotonNetwork.player.customProperties["team"] = 0;

								roomName = "Room" + PhotonNetwork.GetRoomList().Length + 1;
								maxPlayers = 4;
								properties = new Hashtable() { {"closeRoom", false } };
								roomPropsInLobby = new string[] { "closeRoom", "bool" };
								PhotonNetwork.CreateRoom(roomName, isVisible,
														 isOpen, maxPlayers,
														 properties, roomPropsInLobby);

								InvokeRepeating ("TryToEnterGame", 0.0f, RefreshingInterval);
							});

	}

	public void Close ()
	{

	}

	private void TryToEnterGame ()
	{
		if (PhotonNetwork.room == null)
		{
			Debug.Log("Algo estranho por aqui");
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
