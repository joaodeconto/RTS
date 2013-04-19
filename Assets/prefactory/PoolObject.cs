#region Using Statements
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#endregion

public class PoolObject : MonoBehaviour
{
	#region Fields
	
	/// <summary>
	/// The prefab name for this object. GameObject name can be changed
	/// at runtime, and the prefab name is stored to facilitate pooling.
	/// </summary>
	public string PrefabName;
	
	/// <summary>
	/// Called when the object is spawned.
	/// </summary>
	public EventHandler Spawned;
	
	/// <summary>
	/// Called when the object is despawned.
	/// </summary>
	public EventHandler Despawned;
	
	/// <summary>
	/// Dynamic objects do not cache their hierarchy.
	/// </summary>
	public bool IsDynamic = false;
	
	/// <summary>
	/// Cache a list of attached GameObjects for non-dynamic objects.
	/// </summary>
	private List<GameObject> GameObjectCache;
	
	/// <summary>
	/// Cache a list of attached Renderers for non-dynamic objects.
	/// </summary>
	private List<Renderer> RendererCache;
	
	/// <summary>
	/// Is the object currently considered 'spawned'.
	/// </summary>
	private bool m_isSpawned = false;
	public bool IsSpawned
	{
		get { return m_isSpawned; }
		private set { m_isSpawned = value; }
	}
	
	/// <summary>
	/// Is the object counting down to an auto-despawn?
	/// </summary>
	private bool HasDespawnTimer = false;
	
	/// <summary>
	/// The time that the PoolObject was spawned.
	/// </summary>
	private float m_timeLastSpawned = -1f;
	public float TimeLastSpawned
	{
		get { return m_timeLastSpawned; }
		private set { m_timeLastSpawned = value; }
	}
	
	/// <summary>
	/// The time that the PoolObject was despawned.
	/// </summary>
	private float m_timeLastDespawned = -1f;
	public float TimeLastDespawned
	{
		get { return m_timeLastDespawned; }
		private set { m_timeLastDespawned = value; }
	}
	
	/// <summary>
	/// The time that the despawn timer was initialized.
	/// </summary>
	private float DespawnTimerInitialized = -1f;
	
	/// <summary>
	/// The total time until the despawn timer is completed.
	/// </summary>
	private float DespawnDelay = -1f;
	
	/// <summary>
	/// Get the age of the object since spawn.
	/// </summary>
	private float Age
	{
		get
		{
			if (!IsSpawned)
				return -1f;
			return Time.time - TimeLastSpawned;
		}
	}
	
	/// <summary>
	/// Get the age of the object as a scalar relating to the despawn timer. If
	/// a timed despawn has not been triggered, the object's age will always be 1.
	/// </summary>
	private float AgeAsScalar
	{
		get
		{
			if (!IsSpawned)
				return -1f;
			if (DespawnDelay <= 0f)
				return 1f;
			return (Time.time - DespawnTimerInitialized) / DespawnDelay;
		}
	}
	
	#endregion
	
	#region Methods
	
	/// <summary>
	/// Local initialize.
	/// </summary>
	private void Awake()
	{
		GameObjectCache = new List<GameObject>();
		RendererCache = new List<Renderer>();
		RefreshCache();
		SetActive(false);
	}
	
	/// <summary>
	/// Calculate the GameObject and Renderer hierarchy.
	/// </summary>
	private void RefreshCache()
	{
		GameObjectCache.Clear();
		RendererCache.Clear();
		foreach(Transform t in transform.GetComponentsInChildren<Transform>())
			GameObjectCache.Add(t.gameObject);
		foreach(GameObject go in GameObjectCache)
			foreach(Renderer r in go.GetComponents<Renderer>())
				RendererCache.Add(r);
	}
	
	/// <summary>
	/// Set active recursively (cached).
	/// </summary>
	private void SetActive(bool active)
	{
		foreach(GameObject go in GameObjectCache)
			go.active = active;
	
		foreach(Renderer r in RendererCache)
			r.enabled = active;
	}
	
	/// <summary>
	/// Enable the GameObject and all children.
	/// </summary>
	public void OnSpawn()
	{
		if (IsSpawned)
			return;
		
		IsSpawned = true;
		TimeLastSpawned = Time.time;
		
		if (IsDynamic)
			RefreshCache();
		
		SetActive(true);
		
		if (Spawned != null)
			Spawned(this, null);
	}
	
	/// <summary>
	/// Disable the GameObject and all children.
	/// </summary>
	public void OnDespawn()
	{
		if (!IsSpawned)
			return;
		
		IsSpawned = false;
		HasDespawnTimer = false;
		TimeLastDespawned = Time.time;
		DespawnTimerInitialized = -1f;
		DespawnDelay = -1f;
		StopAllCoroutines();
		
		if (IsDynamic)
			RefreshCache();
		
		SetActive(false);
		
		if (Despawned != null)
			Despawned(this, null);
	}
	
	/// <summary>
	/// Despawn this gameObject.
	/// </summary>
	public void Despawn()
	{
		PoolManager.Despawn(gameObject);
	}
	
	/// <summary>
	/// Despawn in the specified number of seconds.
	/// </summary>
	public void DespawnAfterSeconds(float delay)
	{
		if (!IsSpawned)
			return;
		
		DespawnTimerInitialized = Time.time;
		DespawnDelay = delay - Time.deltaTime;
		
		if (!HasDespawnTimer)
			StartCoroutine(CRDespawnAfterSeconds(delay));
	}
	
	/// <summary>
	/// Despawn in the specified number of seconds.
	/// </summary>
	private IEnumerator CRDespawnAfterSeconds(float delay)
	{
		HasDespawnTimer = true;
		while (Time.time < DespawnTimerInitialized + DespawnDelay)
			yield return null;
		Despawn();
	}
	
	#endregion
}