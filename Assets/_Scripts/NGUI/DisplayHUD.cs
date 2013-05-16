using UnityEngine;
using System.Collections;

public class DisplayHUD : MonoBehaviour {
	
	public GameObject prefab;
	public Transform target;
	
	void Start ()
	{
		if (HUDRoot.go == null)
		{
			GameObject.Destroy(this);
			return;
		}

		GameObject child = NGUITools.AddChild(HUDRoot.go, prefab);

		// Make the UI follow the target
		child.AddComponent<UIFollowTarget>().target = target;
	}
}
