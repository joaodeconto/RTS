#region Using Statements
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#endregion

[System.Serializable]
public class PrefabPool
{
	#region Fields
	
	/// <summary>
	/// The prefab managed by this class.
	/// </summary>
	public PoolObject Prefab;
	
	/// <summary>
	/// The number of the prefab to pre-allocate.
	/// </summary>
	public int PreAlloc = 8;
	
	/// <summary>
	/// The number of this prefab to allocate when requesting from an empty pool.
	/// </summary>
	public int AllocBlock = 1;
	
	/// <summary>
	/// True if no instances beyond the limit should be created.
	/// </summary>
	public bool HardLimit = false;
	
	/// <summary>
	/// The limit for hard limited pools.
	/// </summary>
	public int Limit = 8;
	
	/// <summary>
	/// True if excess prefabs should be culled.
	/// </summary>
	public bool Cull = false;
	
	/// <summary>
	/// The maximum number of the prefab to maintain in the pool.
	/// </summary>
	public int CullAbove = 8;
	
	/// <summary>
	/// The frequency at which to cull excess prefabs.
	/// </summary>
	public float CullDelay = 10f;
	
	/// <summary>
	/// The pool.
	/// </summary>
	private Stack<GameObject> Pool;
	
	/// <summary>
	/// The time of the last cull.
	/// </summary>
	private float TimeOfLastCull = float.MinValue;
	
	/// <summary>
	/// How many instances have been requested?
	/// </summary>
	private int SpawnCount = 0;
	
	#endregion
	
	#region Methods
	
	/// <summary>
	/// Local initialize.
	/// </summary>
	public void Awake()
	{
		Prefab.PrefabName = Prefab.gameObject.name;
		Pool = new Stack<GameObject>(PreAlloc);
		Allocate(PreAlloc);
	}
	
	/// <summary>
	/// Adds the specified count of prefab instances to the pool.
	/// </summary>
	private void Allocate(int count)
	{
		if (HardLimit && Pool.Count + count > Limit)
			count = Limit - Pool.Count;
		
		for (int n = 0; n < count; n++)
		{
			GameObject go = GameObject.Instantiate(Prefab.gameObject) as GameObject;
			go.name = go.name + n.ToString();
			Pool.Push(go);
		}
	}
	
	/// <summary>
	/// Get a pooled item if one is available, or if legal create a new instance.
	/// </summary>
	public GameObject Pop()
	{
		if (HardLimit && SpawnCount >= Limit)
			return null;
		
		if (Pool.Count > 0)
		{
			SpawnCount++;
			return Pool.Pop();
		}
		
		Allocate(AllocBlock);
		return Pop();
	}
	
	/// <summary>
	/// Return an item to the pool.
	/// </summary>
	public void Push(GameObject go)
	{
		if (HardLimit && Pool.Count >= Limit)
			return;
		
		SpawnCount = (int)Mathf.Max(SpawnCount - 1, 0);
		Pool.Push(go);
	}
	
	/// <summary>
	/// Poll for culling.
	/// </summary>
	public void Poll()
	{
		if (!Cull || Pool.Count <= CullAbove) return;
		
		if (Time.time > TimeOfLastCull + CullDelay)
		{
			TimeOfLastCull = Time.time;
			for (int n = CullAbove; n <= Pool.Count; n++)
				GameObject.Destroy(Pool.Pop());
		}
	}
	
	/// <summary>
	/// Spawn an object from the pool.
	/// </summary>
	public GameObject Spawn()
	{
		GameObject go = Pop();
		
		if (go != null)
			go.GetComponent<PoolObject>().OnSpawn();
		
		return go;
	}
	
	/// <summary>
	/// Despawn an object back to the pool.
	/// </summary>
	public void Despawn(GameObject go)
	{
	 	PoolObject po = go.GetComponent<PoolObject>();
		if (po == null || po.PrefabName != Prefab.PrefabName)
			return;
		
		po.OnDespawn();
		Push(go);
	}
	
	#endregion
}