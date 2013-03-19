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
            //stream.SendNext((int)factory.state);
//			stream.SendNext(factory.stats);
            stream.SendNext(factory.Health);
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            //Network player, receive data
            //factory.state = (State)(int)stream.ReceiveNext();
			factory.SetHealth ((int)stream.ReceiveNext());
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
        }
    }
	
	private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this

    void Update()
    {
        transform.position = correctPlayerPos;
        transform.rotation = correctPlayerRot;
    }
}
