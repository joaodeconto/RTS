using UnityEngine;
using System.Collections;

public class mouseFollowScript : MonoBehaviour {
	
	void Update () {
		if(Input.GetMouseButton(0))
		{
			Vector3 mp = Input.mousePosition;
			mp.z = 12f;
			Vector3 tempPos = Camera.main.ScreenToWorldPoint(mp);
			transform.position = tempPos;
		}
		
		
	}
}
