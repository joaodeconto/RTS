using UnityEngine;
using System.Collections;
using Visiorama;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[System.Serializable]
public class InitInstantiateNetwork : Photon.MonoBehaviour
{
	public GameObject prefabInstantiate;
	private bool wasInitialized = false;
	private GameObject prefab;

	public virtual void Init ()
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
			int playerLoads = 0;			
			Room cRoom = PhotonNetwork.room;
			if (cRoom.customProperties.ContainsKey ("playerLoads"))	playerLoads = (int)cRoom.customProperties["playerLoads"];
			playerLoads += 1;			
			Hashtable setPlayerLoads = new Hashtable() {{"playerLoads", playerLoads}};
			cRoom.SetCustomProperties (setPlayerLoads);
			InvokeRepeating ("NetworkInstantiatePrefab", 0.1f, 0.2f);
		}
	}

	void InstantiatePrefab ()
	{
		prefab = Instantiate (prefabInstantiate, transform.position, prefabInstantiate.transform.rotation) as GameObject;
		
		IStats stats = prefab.GetComponent<IStats>();
		stats.SetTeam (0, 0);
		stats.transform.parent = transform.parent;
		FactoryBase fb = prefab.GetComponent<FactoryBase>();

		if (fb != null)
		{
			fb.wasBuilt = true;
			fb.Init();
			fb.SendMessage ("ConstructFinished", SendMessageOptions.DontRequireReceiver);
			if (fb.playerUnit)fb.TechActiveBool(fb.TechsToActive, true);
			fb.wasVisible = false;
		}
		else
			stats.Init();

		Destroy (this.gameObject);
	}

	void NetworkInstantiatePrefab ()
	{
		if ((int)PhotonNetwork.room.customProperties["playerLoads"] >= PhotonNetwork.countOfPlayersInRooms)
		{
			if ((int)PhotonNetwork.player.customProperties["team"] == (int.Parse (transform.parent.name)))
			{
				GameObject prefab = PhotonNetwork.Instantiate (prefabInstantiate.name, transform.position, prefabInstantiate.transform.rotation, 0);
				prefab.transform.parent = transform.parent;
				if (prefab.GetComponent<FactoryBase>() != null)
				{				
					FactoryBase fb = prefab.GetComponent<FactoryBase>();
					fb.wasBuilt = true;	
					fb.IsNetworkInstantiate = true;
					fb.SendMessage ("ConstructFinished", SendMessageOptions.DontRequireReceiver);
					if (fb.playerUnit)fb.TechActiveBool(fb.TechsToActive, true);
				}	
				else 
				{
					IStats stats = prefab.GetComponent<IStats>();
					stats.Init();
				}
			}
		
			CancelInvoke ("NetworkInstantiatePrefab");
			Destroy (this.gameObject);
		}
	}
	void OnDrawGizmos()
	{	
		int teamTransfor = int.Parse(this.transform.parent.name);
		if (teamTransfor == 0) Gizmos.color = Color.red;
		if (teamTransfor == 1) Gizmos.color = Color.blue;
		if (teamTransfor == 2) Gizmos.color = Color.magenta;
		if (teamTransfor == 3) Gizmos.color = Color.cyan;
		float prefabSize = prefabInstantiate.GetComponent<CapsuleCollider>().radius;
		Gizmos.DrawWireSphere (this.transform.position, prefabSize);		
	}
}
