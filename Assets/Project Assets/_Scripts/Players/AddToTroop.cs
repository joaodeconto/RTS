using UnityEngine;
using System.Collections;
using Visiorama;

public class AddToTroop : MonoBehaviour
{
	void Awake ()
	{
		ComponentGetter.Get<TroopController> ().AddSoldier (this.GetComponent<Unit> ());
	}

	void OnDead ()
	{
		ComponentGetter.Get<TroopController> ().RemoveSoldier (this.GetComponent<Unit> ());
	}
}
