using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class FactoryNetworkTransform : Photon.MonoBehaviour
{
	FactoryBase factory;
	
	void Awake()
    {
		factory = GetComponent <FactoryBase> ();
		
        gameObject.name = gameObject.name + photonView.viewID;
		
        enabled = !photonView.isMine;
    }
	
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            stream.SendNext (factory.Health);
			stream.SendNext (factory.buildingState);
            stream.SendNext (transform.position);
            stream.SendNext (transform.rotation);
        }
        else
        {
            //Network player, receive data
			factory.SetHealth ((int)stream.ReceiveNext());
			factory.buildingState = (FactoryBase.BuildingState)(int)stream.ReceiveNext();
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
        }
    }
	
	private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this

    void Update()
    {
		if (!photonView.isMine)
        transform.position = correctPlayerPos;
        transform.rotation = correctPlayerRot;
		
		factory.SyncAnimation ();
    }
}
