using UnityEngine;
using System.Collections;

public class FactoryNetworkTransform : Photon.MonoBehaviour
{
	FactoryBase factory;
	
	void Awake()
    {
		factory = GetComponent <FactoryBase> ();
		
        gameObject.name = gameObject.name + photonView.viewID;
		
        enabled = !photonView.isMine;
		factory.enabled = photonView.isMine;
    }
}
