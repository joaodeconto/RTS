using UnityEngine;
using System.Collections;

public class EveryplayHudCamera : MonoBehaviour {
	void OnPreRender() {
		Everyplay.SharedInstance.SnapshotRenderbuffer();	
	}
}