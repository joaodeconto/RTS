using UnityEngine;
using System.Collections;
using PathologicalGames;

public class Despawn : MonoBehaviour {

	public string poolString;
	public float despawnTimer = 3f;

	private void OnSpawned(SpawnPool pool)
	{
			PoolManager.Pools[pool.poolName].Despawn(this.transform, despawnTimer);
	}
}
