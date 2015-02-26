using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Hashtable = ExitGames.Client.Photon.Hashtable;

[System.Serializable]
public class InitInstantiateEnemy : Photon.MonoBehaviour
{

	public GameObject prefabInstantiate;
	private bool wasInitialized = false;

	public void Init ()
	{
		if(!wasInitialized) InvokeRepeating ("CheckNetwork", 0.1f, 0.5f);

		wasInitialized = true;
	}

	void CheckNetwork ()
	{
		if (PhotonNetwork.offlineMode)
		{
			CancelInvoke ("CheckNetwork");
			InstantiatePrefab ();
			return;
		}

		if (PhotonNetwork.isMessageQueueRunning)
		{
			CancelInvoke ("CheckNetwork");

			InvokeRepeating ("NetworkInstantiatePrefab", 0.1f, 0.5f);
		}
	}

	void InstantiatePrefab ()
	{
		GameObject prefab = Instantiate (prefabInstantiate, transform.position, transform.rotation) as GameObject;
		prefab.transform.parent = transform.parent;
		IStats stats = prefab.GetComponent<IStats>();
		stats.SetTeam (8,8);
		stats.Init ();
		if (prefab.GetComponent<FactoryBase>() != null)
		{
			prefab.SendMessage ("ConstructFinished", SendMessageOptions.DontRequireReceiver);
			FactoryBase fb = prefab.GetComponent<FactoryBase>();
			fb.wasBuilt = true;

		}				
		CancelInvoke ("NetworkInstantiatePrefab");
		Destroy (this.gameObject);

	}

	void NetworkInstantiatePrefab ()
	{
		GameObject prefab = PhotonNetwork.Instantiate (prefabInstantiate.name, transform.position, transform.rotation, 0);
		prefab.transform.parent = transform.parent;
		IStats stats = prefab.GetComponent<IStats>();
		stats.SetTeam (8,8);
		stats.Init ();
		if (prefab.GetComponent<FactoryBase>() != null)
		{
			prefab.SendMessage ("ConstructFinished", SendMessageOptions.DontRequireReceiver);
			FactoryBase fb = prefab.GetComponent<FactoryBase>();
			fb.wasBuilt = true;
		}	

		CancelInvoke ("NetworkInstantiatePrefab");
		Destroy (this.gameObject);

	}

		void OnDrawGizmos ()
		{			
			Gizmos.color = Color.green;
			float prefabSize = prefabInstantiate.GetComponent<CapsuleCollider>().radius;
			Gizmos.DrawWireSphere (this.transform.position, prefabSize);			
		}
}
