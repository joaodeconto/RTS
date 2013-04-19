#region Using Statements
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
#endregion

[CustomEditor(typeof(PoolManager))]
public class PoolManagerEditor : Editor
{
	#region Fields
	
	/// <summary>
	/// True for indices corresponding to individual PrefabPool foldouts which should be expanded.
	/// </summary>
	private List<bool> PrefabFoldouts;
	
	/// <summary>
	/// The selected PoolManager object.
	/// </summary>
	private PoolManager TargetObject;
	
	/// <summary>
	/// The PrefabPool collection belonging to the PoolManager.
	/// Local reference for convenience only.
	/// </summary>
	private List<PrefabPool> PoolCollection;
	
	#endregion
	
	#region Methods
	
	/// <summary>
	/// Called when an object containing a component of type PoolManager is selected.
	/// </summary>
	public void OnEnable()
	{
		TargetObject = target as PoolManager;
		PoolCollection = TargetObject.PrefabPoolCollection;
		
		PrefabFoldouts = new List<bool>();
		if (PoolCollection != null)
			for (int n = 0; n < PoolCollection.Count; n++)
				PrefabFoldouts.Add(true);
		
		ClearNullReferences();
	}
	
	/// <summary>
	/// Checks all PrefabPools in the collection for null reference Prefab objects. These
	/// can sometimes occur when the user has created a PrefabPool for an object which
	/// they have later deleted.
	/// </summary>
	private void ClearNullReferences()
	{
		if (PoolCollection == null)
			return;
		
		int n = 0;
		while (n < PoolCollection.Count)
		{
			if (PoolCollection[n].Prefab == null)
				RemovePoolAtIndex(n);
			else
				n++;
		}
	}
	
	/// <summary>
	/// Add a pool of the specified GameObject (if one doesn't exist)
	/// </summary>
	private void AddPool(GameObject go)
	{
		PrefabPool newPrefabPool = new PrefabPool();
		
		if (PoolCollection == null)
			PoolCollection = new List<PrefabPool>();
		
		if (PoolCollection != null)
			foreach (PrefabPool pp in PoolCollection)
				if (pp.Prefab.gameObject.name == go.name)
				{
					EditorUtility.DisplayDialog("Pool Manager", "<PoolManager> already manages a GameObject with the name '" + go.name + "'.\n\nIf you are attempting to manage multiple GameObjects sharing the same name, you will need to first give them unique names.", "OK");
					return;
				}
		newPrefabPool.Prefab = go.GetComponent<PoolObject>();
		
		PoolCollection.Add(newPrefabPool);
		while(PoolCollection.Count > PrefabFoldouts.Count)
			PrefabFoldouts.Add(false);
	}
	
	/// <summary>
	/// Remove the pool at the specified index.
	/// </summary>
	private void RemovePoolAtIndex(int index)
	{
		for (int n = index; n < PoolCollection.Count - 1; n++)
			PoolCollection[n] = PoolCollection[n + 1];
		
		PoolCollection.RemoveAt(PoolCollection.Count - 1);
	}
	
	/// <summary>
	/// Draw the custom inspector.
	/// </summary>
	public override void OnInspectorGUI()
	{	
		/*
		// Log
		EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Log Messages ", EditorStyles.label, GUILayout.Width(135f));
			TargetObject.Log = EditorGUILayout.Toggle(TargetObject.Log);
			GUILayout.Space(10f);
		EditorGUILayout.EndHorizontal();
		*/
		
		GUILayout.Space(15f);
		DropAreaGUI();
		
		if (PoolCollection == null)
			return;
		
		GUILayout.Space(5f);
		GUILayout.Label("Pool Objects", EditorStyles.boldLabel);
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(10f);
		EditorGUILayout.BeginVertical();
		
		for (int n = 0; n < PoolCollection.Count; n++)
		{
			PrefabPool pp = PoolCollection[n];
			string name;
			if (pp.Prefab != null)
				name = pp.Prefab.name;
			else
				name = "EMPTY";
			
			// PrefabPool DropDown
			EditorGUILayout.BeginHorizontal();
				PrefabFoldouts[n] = EditorGUILayout.Foldout(PrefabFoldouts[n], name, EditorStyles.foldout);
				if (GUILayout.Button("-", GUILayout.Width(20f)))
					RemovePoolAtIndex(n);
			EditorGUILayout.EndHorizontal();
			
			if (PrefabFoldouts[n])
			{
				EditorGUILayout.BeginHorizontal();
					GUILayout.Space(10f);
					EditorGUILayout.BeginVertical();
				
					// PreAlloc
					EditorGUILayout.BeginHorizontal();
						GUILayout.Label("Pre-Allocate", EditorStyles.label, GUILayout.Width(115f));
						pp.PreAlloc = EditorGUILayout.IntField(pp.PreAlloc);
						if (pp.PreAlloc < 0) pp.PreAlloc = 0;
					EditorGUILayout.EndHorizontal();
				
					// AllocBlock
					EditorGUILayout.BeginHorizontal();
						GUILayout.Label("Allocate Block", EditorStyles.label, GUILayout.Width(115f));
						pp.AllocBlock = EditorGUILayout.IntField(pp.AllocBlock);
						if (pp.AllocBlock < 1) pp.AllocBlock = 1;
					EditorGUILayout.EndHorizontal();
				
					// HardLimit
					EditorGUILayout.BeginHorizontal();
						GUILayout.Label("Hard Limit ", EditorStyles.label, GUILayout.Width(115f));
						pp.HardLimit = EditorGUILayout.Toggle(pp.HardLimit);
					EditorGUILayout.EndHorizontal();
					
					bool oldEnabled = GUI.enabled;
					GUI.enabled = pp.HardLimit && oldEnabled;
				
					EditorGUILayout.BeginHorizontal();
						GUILayout.Space(20f);
						GUILayout.Label("Limit", EditorStyles.label, GUILayout.Width(100f));
						pp.Limit = EditorGUILayout.IntField(pp.Limit);
						if (pp.Limit < 1) pp.Limit = 1;
					EditorGUILayout.EndHorizontal();
				
					GUI.enabled = oldEnabled;
					
					EditorGUILayout.BeginHorizontal();
						GUILayout.Label("Cull ", EditorStyles.label, GUILayout.Width(115f));
						pp.Cull = EditorGUILayout.Toggle(pp.Cull);
					EditorGUILayout.EndHorizontal();
					
					GUI.enabled = pp.Cull && oldEnabled;
				
					EditorGUILayout.BeginHorizontal();
						GUILayout.Space(20f);
						GUILayout.Label("Cull Above", EditorStyles.label, GUILayout.Width(100f));
						pp.CullAbove = EditorGUILayout.IntField(pp.CullAbove);
						if (pp.CullAbove < 0) pp.CullAbove = 0;
						if (pp.HardLimit && pp.CullAbove > pp.Limit) pp.CullAbove = pp.Limit;
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
						GUILayout.Space(20f);
						GUILayout.Label("Cull Delay", EditorStyles.label, GUILayout.Width(100f));
						pp.CullDelay = EditorGUILayout.FloatField(pp.CullDelay);
						if (pp.CullDelay < 0) pp.CullDelay = 0;
					EditorGUILayout.EndHorizontal();
				
					GUI.enabled = oldEnabled;
				
					EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			}
			
			EditorGUILayout.Space();
		}
	}
	
	/// <summary>
	/// Drop Area GUI.
	/// </summary>
	private void DropAreaGUI()
	{
		var evt = Event.current;
		var dropArea = GUILayoutUtility.GetRect(0f, 50f, GUILayout.ExpandWidth(true));
		GUI.Box(dropArea, "Drop a Prefab or GameObject here to add it to PoolManager");
		
		switch (evt.type)
		{
			case EventType.DragUpdated:
			case EventType.DragPerform:
				if (!dropArea.Contains(evt.mousePosition))
					break;
				
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				
				if (evt.type == EventType.DragPerform)
				{
					DragAndDrop.AcceptDrag();
					foreach(var draggedObject in DragAndDrop.objectReferences)
					{
						var go = draggedObject as GameObject;
						if (!go)
							continue;
						
						var poolableObject = go.GetComponent<PoolObject>();
						if (!poolableObject)
						{
							EditorUtility.DisplayDialog("Pool Manager", "<PoolManager> cannot manage the object '" + go.name + "' as it has no <PoolObject> component.", "OK");
							continue;
						}
						
						AddPool(go);
					}
				}
				
				Event.current.Use();
				break;
		}
	}
	
	#endregion
}