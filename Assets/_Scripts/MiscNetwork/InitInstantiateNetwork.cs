using UnityEngine;
using System.Collections;

using Hashtable = ExitGames.Client.Photon.Hashtable;

[System.Serializable]
public class InitInstantiateNetwork : Photon.MonoBehaviour
{
	public GameObject prefabInstantiate;

	void Awake ()
	{
		InvokeRepeating ("CheckNetwork", 0.1f, 0.5f);
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

			if (cRoom.customProperties.ContainsKey ("playerLoads"))
				playerLoads = (int)cRoom.customProperties["playerLoads"];

			playerLoads += 1;
			
			Hashtable setPlayerLoads = new Hashtable() {{"playerLoads", playerLoads}};
			cRoom.SetCustomProperties (setPlayerLoads);

			InvokeRepeating ("NetworkInstantiatePrefab", 0.1f, 0.5f);
		}
	}

	void InstantiatePrefab ()
	{
		GameObject prefab = Instantiate (prefabInstantiate, transform.position, prefabInstantiate.transform.rotation) as GameObject;
		
		IStats stats = prefab.GetComponent<IStats>();
		stats.SetTeam (int.Parse (transform.parent.name), Random.Range (0, 9999));
		stats.Init ();
	}

	void NetworkInstantiatePrefab ()
	{
		if ((int)PhotonNetwork.room.customProperties["playerLoads"] >= PhotonNetwork.countOfPlayersInRooms)
		{
			if ((int)PhotonNetwork.player.customProperties["team"] == (int.Parse (transform.parent.name)))
			{
				GameObject prefab = PhotonNetwork.Instantiate (prefabInstantiate.name, transform.position, prefabInstantiate.transform.rotation, 0);
				prefab.transform.parent = transform.parent;
				if (prefab.GetComponent<FactoryBase>() != null)	prefab.SendMessage ("ConstructFinished", SendMessageOptions.DontRequireReceiver);

				
			}
			CancelInvoke ("NetworkInstantiatePrefab");
			Destroy (this.gameObject);
		}
	}
}
