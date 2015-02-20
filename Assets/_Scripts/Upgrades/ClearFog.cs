using UnityEngine;
using System.Collections;
using Visiorama;

public class ClearFog : MonoBehaviour {

	// Use this for initialization
	void Start () {
		FogOfWar fogOfWar = ComponentGetter.Get<FogOfWar>();
		fogOfWar.RemoveBlackFog();
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
