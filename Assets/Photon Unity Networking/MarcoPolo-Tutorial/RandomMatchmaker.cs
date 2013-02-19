using UnityEngine;
using System.Collections;

public class RandomMatchmaker : Photon.MonoBehaviour
{
    private PhotonView myMonsterPv;

    // Use this for initialization
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings("0.1");
    }

    void OnJoinedLobby()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null, true, true, 4);  // no name (gets a guid), visible and open with 4 players max
    }

    void OnJoinedRoom()
    {
        GameObject monster = PhotonNetwork.Instantiate("monsterprefab", Vector3.zero, Quaternion.identity, 0);
        ThirdPersonController controller = monster.GetComponent<ThirdPersonController>();
        myMonsterPv = monster.GetComponent<PhotonView>();
        controller.enabled = true;
    }

    void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());

        if (PhotonNetwork.connectionStateDetailed == PeerState.Joined)
        {
            bool shoutMarco = GameLogic.playerWhoIsIt == PhotonNetwork.player.ID;

            if (shoutMarco && GUILayout.Button("Marco!"))
            {
                this.myMonsterPv.RPC("Marco", PhotonTargets.All);
            }
            if (!shoutMarco && GUILayout.Button("Polo!"))
            {
                this.myMonsterPv.RPC("Polo", PhotonTargets.All);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}