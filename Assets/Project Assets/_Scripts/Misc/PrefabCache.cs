using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefabCache : MonoBehaviour
{
	[System.Serializable]
	public class Prefab
	{
		public string Name;
		public int minimumCacheRemaining;
		public GameObject prefab;
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
		if (Prefabs[cCacheIterator].minimumCacheRemaining > Cache[cCacheIterator].Count)
		{
			int i = (int)(/*Random.value % */Prefabs[cCacheIterator].minimumCacheRemaining);
			for (; i != -1; --i)
			{
				GameObject go =  Instantiate(Prefabs[cCacheIterator].prefab,
														 Vector3.zero,
														 Quaternion.identity) as GameObject;

				go.transform.parent = fakePanel.transform;

				Cache[cCacheIterator].Push (go);
			}
		}

		if ((++cCacheIterator) == Prefabs.Length)
		{
			cCacheIterator = 0;
			//CancelInvoke ("FillCaches");
		}
	}

	public GameObject Get (string prefabName)
	{
		GameObject go = null;

		prefabName = prefabName.ToLower ();

		for (int i = Prefabs.Length - 1; i != -1; --i)
		{
			if (Prefabs[i].Name.Equals(prefabName))
			{
				if (Cache[i].Count != 0)
				{
					go = Cache[i].Pop ();
					//go.SetActive (false);
				}
				else
				{
					Debug.LogError ("Whata?");
				}

				//if (go.)

				break;
			}
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
}
