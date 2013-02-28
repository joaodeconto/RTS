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
	
//	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
//    {
//        if (stream.isWriting)
//        {
//            //We own this player: send the others our data
//            stream.SendNext((int)factory.state);
//        }
//        else
//        {
//            //Network player, receive data
//            factory.state = (State)(int)stream.ReceiveNext();
//        }
//    }
}
