using UnityEngine;
using System.Collections;

public class NetworkManager : Photon.MonoBehaviour {
	
	public bool offlineMode;
	
	public void Init ()
	{
		PhotonNetwork.offlineMode = offlineMode;
		PhotonNetwork.isMessageQueueRunning = true;
	}

    public void OnLeftRoom()
    {
		Debug.Log("OnLeftRoom (local)");
    }

    public void OnMasterClientSwitched(PhotonPlayer player)
    {
        Debug.Log("OnMasterClientSwitched: " + player);

        if (PhotonNetwork.connected)
        {
            photonView.RPC("SendChatMessage", PhotonNetwork.masterClient, "Hi master! Welcome (:");
            photonView.RPC("SendChatMessage", PhotonNetwork.player, "WE GOT A NEW MASTER: " + PhotonNetwork.masterClient);
        }
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("OnDisconnectedFromPhoton");

        // Back to main menu
        Application.LoadLevel(0);
    }

    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerConnected: " + player);
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.Log("OnPlayerDisconnected: " + player);
		
        if (PhotonNetwork.connected)
        {
            photonView.RPC("SendChatMessage", PhotonNetwork.player, "Player Disconnected: " + player.name);
        }
		
		GameplayManager gameplayManager = Visiorama.ComponentGetter.Get<GameplayManager> ();
		if (GameplayManager.mode == GameplayManager.Mode.Cooperative)
		{
			gameplayManager.photonView.RPC ("Defeat", PhotonNetwork.player,
				player.customProperties["team"], 
				player.customProperties["allies"]);
		}
		else
		{
			gameplayManager.photonView.RPC ("Defeat", PhotonNetwork.player,
				player.customProperties["team"], 
				0);
		}
    }

    public void OnReceivedRoomList ()
    {
        Debug.Log("OnReceivedRoomList");
    }

    public void OnReceivedRoomListUpdate ()
    {
        Debug.Log("OnReceivedRoomListUpdate");
    }

    public void OnConnectedToPhoton ()
    {
        Debug.Log("OnConnectedToPhoton");
    }

    public void OnFailedToConnectToPhoton ()
    {
        Debug.Log("OnFailedToConnectToPhoton");
    }

//	[RPC]
//	void ChangeLevel (int level)
//	{
//		Application.LoadLevel (0);
//	}
}
