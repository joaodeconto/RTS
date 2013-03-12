using UnityEngine;
using System.Collections;

public class InitInstantiateNetwork : Photon.MonoBehaviour
{
	public GameObject prefabInstantiate;
	
	void Awake ()
	{
		InvokeRepeating ("Init", 0.5f, 0.5f);
	}
	
	void Init ()
	{
		if (PhotonNetwork.isMessageQueueRunning)
		{
			if ((int)PhotonNetwork.player.customProperties["team"] == (int.Parse (transform.parent.name)))
			{
				PhotonNetwork.Instantiate (prefabInstantiate.name, transform.position, transform.rotation, 0);
			}
			CancelInvoke ();
		}
	}
}
