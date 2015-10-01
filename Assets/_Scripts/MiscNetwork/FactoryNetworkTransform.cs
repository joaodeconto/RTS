using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class FactoryNetworkTransform : Photon.MonoBehaviour
{
	FactoryBase factory;
	private Vector3 correctPlayerPos;
	private Quaternion correctPlayerRot;
	
	void Awake()
    {
		if (PhotonNetwork.offlineMode){
			enabled = false;
		}

		else{
			factory = GetComponent <FactoryBase> ();			
			correctPlayerPos = factory.transform.position; //We lerp towards this
			correctPlayerRot = factory.transform.rotation; //We lerp towards this
			gameObject.name = gameObject.name + photonView.viewID;
						
			if (factory.IsNetworkInstantiate){
				if(GetComponent<GhostFactory>() != null) enabled = false;
				else enabled = !photonView.isMine;
			}
			
			else {
				if(GetComponent<GhostFactory>() != null) enabled = false;
				else enabled = !photonView.isMine;				
			}
		}
    }
	
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting){
            //We own this player: send the others our data
            stream.SendNext (factory.Health);
			stream.SendNext (factory.buildingState);
            stream.SendNext (transform.position);
            stream.SendNext (transform.rotation);
        }
        else{
            //Network player, receive data
			factory.SetHealth ((int)stream.ReceiveNext());
			factory.buildingState = (FactoryBase.BuildingState)(int)stream.ReceiveNext();
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
        }
    }

    void Update()
    {
		if (!photonView.isMine){
        	transform.position = correctPlayerPos;
        	transform.rotation = correctPlayerRot;
			factory.SyncAnimation ();
		}
    }
}
