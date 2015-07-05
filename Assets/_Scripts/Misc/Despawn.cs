using UnityEngine;
using System.Collections;
using PathologicalGames;

public class Despawn : MonoBehaviour {

	public float despawnTimer = 3f;

	private void OnSpawned(SpawnPool pool)
	{
			transform.parent = pool.group;
			PoolManager.Pools[pool.poolName].Despawn(this.transform, despawnTimer);
	}
}
