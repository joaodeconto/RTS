using UnityEngine;
using System.Collections;

public class NetworkManager : Photon.MonoBehaviour {
	
	public Transform rexPrefab;
	
	public void Init ()
	{
		PhotonNetwork.isMessageQueueRunning = true;
	}
	
//	public void OnGUI ()
//	{
//	    if (GUILayout.Button("Return to Lobby"))
//	    {
//	        PhotonNetwork.LeaveRoom();
//	    }
//	}
	
	public void OnGUI ()
	{
		GUILayout.Space (50f);
		
	    if (GUILayout.Button("Instantiate Rex"))
	    {
			Vector3 pos = Random.insideUnitSphere * 15f;
			pos.y = transform.position.y;
	        PhotonNetwork.Instantiate(rexPrefab.name, pos, Quaternion.identity, 0);
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
