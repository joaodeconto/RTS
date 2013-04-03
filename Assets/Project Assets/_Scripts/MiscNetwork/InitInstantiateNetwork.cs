using UnityEngine;
using System.Collections;

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
			int playerLoads = 0;

			if (PhotonNetwork.room.customProperties.ContainsKey ("playerLoads"))
				playerLoads = (int)PhotonNetwork.room.customProperties["playerLoads"];

			playerLoads += 1;
			Hashtable setPlayerLoads = new Hashtable() {{"playerLoads", playerLoads}};
			PhotonNetwork.room.SetCustomProperties (setPlayerLoads);

			CancelInvoke ("CheckNetwork");
			InvokeRepeating ("NetworkInstantiatePrefab", 0.1f, 0.5f);
		}
	}

	void InstantiatePrefab ()
	{
		GameObject prefab = Instantiate (prefabInstantiate, transform.position, transform.rotation) as GameObject;
		prefab.GetComponent<IStats>().Team = int.Parse (transform.parent.name);
		prefab.GetComponent<IStats>().Init ();
	}

	void NetworkInstantiatePrefab ()
	{
		if ((int)PhotonNetwork.room.customProperties["playerLoads"] >= PhotonNetwork.countOfPlayersInRooms)
		{
			if ((int)PhotonNetwork.player.customProperties["team"] == (int.Parse (transform.parent.name)))
			{
				GameObject prefab = PhotonNetwork.Instantiate (prefabInstantiate.name, transform.position, transform.rotation, 0);
				prefab.transform.parent = transform.parent;
				if (prefab.GetComponent<FactoryBase>() != null) prefab.SendMessage ("ConstructFinished", SendMessageOptions.DontRequireReceiver);
			}
			CancelInvoke ("NetworkInstantiatePrefab");
			Destroy (this.gameObject);
		}
	}
}
