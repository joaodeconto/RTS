using UnityEngine;
using System.Collections;

public class NetworkManager : Photon.MonoBehaviour {
	
	public bool offlineMode;
	
	public void Init ()
	{
		PhotonNetwork.offlineMode = offlineMode;
		
		int playerLoads = (int)PhotonNetwork.room.customProperties["playerLoads"];
		playerLoads += 1;
		Hashtable setPlayerLoads = new Hashtable() {{"playerLoads", playerLoads}};
		PhotonNetwork.room.SetCustomProperties (setPlayerLoads);
		
		InvokeRepeating ("VerifyAllEnterInScene", 0.3f, 0.3f);
	}
	
	void VerifyAllEnterInScene ()
	{
		if ((int)PhotonNetwork.room.customProperties["playerLoads"] >= PhotonNetwork.countOfPlayersInRooms)
		{
			PhotonNetwork.isMessageQueueRunning = true;
			CancelInvoke ();
		}
	}

    public void OnLeftRoom()
    {
		Debug.Log("OnLeftRoom (local)");
        
        // back to main menu        
        Application.LoadLevel(0);
    }

    public void OnMasterClientSwitched(PhotonPlayer player)
    {
        Debug.Log("OnMasterClientSwitched: " + player);

        if (PhotonNetwork.connected)
        {
            photonView.RPC("SendChatMessage", PhotonNetwork.masterClient, "Hi master! From:" + PhotonNetwork.player);
            photonView.RPC("SendChatMessage", PhotonTargets.All, "WE GOT A NEW MASTER: " + player + "==" + PhotonNetwork.masterClient + " From:" + PhotonNetwork.player);
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
        Debug.Log("OnPlayerDisconneced: " + player);
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

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("OnPhotonInstantiate " + info.sender);
    }
}
