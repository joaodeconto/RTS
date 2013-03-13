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

//        if (PhotonNetwork.connected)
//        {
//            photonView.RPC("SendChatMessage", PhotonNetwork.masterClient, "Hi master! From:" + PhotonNetwork.player);
//            photonView.RPC("SendChatMessage", PhotonTargets.All, "WE GOT A NEW MASTER: " + player + "==" + PhotonNetwork.masterClient + " From:" + PhotonNetwork.player);
//        }
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
            photonView.RPC("SendChatMessage", PhotonTargets.All, "Player Disconnected: " + player.name);
        }
		
		GameplayManager gameplayManager = Visiorama.ComponentGetter.Get<GameplayManager> ();
		gameplayManager.RemoveAllStats ((int)player.customProperties["team"]);
    }

    public void OnReceivedRoomList()
    {
        Debug.Log("OnReceivedRoomList");
    }

    public void OnReceivedRoomListUpdate()
    {
        Debug.Log("OnReceivedRoomListUpdate");
    }

    public void OnConnectedToPhoton()
    {
        Debug.Log("OnConnectedToPhoton");
    }

    public void OnFailedToConnectToPhoton()
    {
        Debug.Log("OnFailedToConnectToPhoton");
    }

//	[RPC]
//	void ChangeLevel (int level)
//	{
//		Application.LoadLevel (0);
//	}
}
