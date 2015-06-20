using UnityEngine;
using System.Collections;
using PathologicalGames;

public class Despawn : MonoBehaviour {

	public string poolString;
	public float despawnTimer = 3f;

	private void OnSpawned(SpawnPool pool)
	{
		Debug.Log
			(
				string.Format
				(
				"OnSpawnedExample | OnSpawned running for '{0}' in pool '{1}'.", 
				this.name, 
				pool.poolName
				)
				);
			PoolManager.Pools[pool.poolName].Despawn(this.transform, despawnTimer);
	}
}
