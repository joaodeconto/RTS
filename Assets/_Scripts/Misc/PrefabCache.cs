using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefabCache : MonoBehaviour
{
	[System.Serializable]
	public class Prefab
	{
		[System.Serializable]
		public class KeyValue
		{
			public string Key;
			public UnityEngine.Object Value;
		}

		public string Name;
		public int minimumCacheRemaining;
		public GameObject prefab;

		//public KeyValue[] KeyValues;
	}

	public Prefab[] Prefabs;
	public UIPanel fakePanel;

	Stack<GameObject>[] Cache;

	int cCacheIterator = 0;
	public void Start ()
	{
		Cache = new Stack<GameObject>[Prefabs.Length];

		for (int i = Prefabs.Length - 1; i != -1; --i)
		{
			Prefabs[i].Name = Prefabs[i].Name.ToLower ();
			Cache[cCacheIterator] = new Stack<GameObject>();
			FillCache ();
		}

		InvokeRepeating ("FillCache", 1f, 1f);
	}

	void FillCache ()
	{
		_fillCache (cCacheIterator);
	}

	void _fillCache (int indexToFill)
	{
		if (Prefabs[indexToFill].minimumCacheRemaining > Cache[indexToFill].Count)
		{
			int i = (int)(Random.value % Prefabs[indexToFill].minimumCacheRemaining);
			for (; i != -1; --i)
			{
				GameObject go =  Instantiate(Prefabs[indexToFill].prefab,
											 Vector3.zero,
											 Quaternion.identity) as GameObject;

				go.transform.parent = fakePanel.transform;

				Cache[indexToFill].Push (go);
			}
		}

		if ((++indexToFill) == Prefabs.Length)
		{
			indexToFill = 0;
			//CancelInvoke ("FillCaches");
		}
	}

	public GameObject Get (string prefabName)
	{
		GameObject go = null;

		int index = GetPrefabIndex (prefabName);

		if (index != -1)
		{
			if (Cache[index].Count == 0)
				_fillCache (index);

			go = Cache[index].Pop ();
		}

		//TODO adicionar aviso de problema
		return go;
	}

	public GameObject Get (Transform parent, string prefabName)
	{
		GameObject go = Get (prefabName);

		Transform t     = go.transform;
		t.parent        = parent.transform;
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
		t.localScale    = Vector3.one;
		go.layer        = parent.gameObject.layer;

		//go.SetActive (true);

		return go;
	}

	//public UnityEngine.Object GetData (string prefabName, )
	//{



	//}

	private int GetPrefabIndex (string prefabName)
	{
		prefabName = prefabName.ToLower ();

		for (int i = Prefabs.Length - 1; i != -1; --i)
		{
			if (Prefabs[i].Name.Equals(prefabName))
			{
				return i;
			}
		}

		return -1;
	}
}
