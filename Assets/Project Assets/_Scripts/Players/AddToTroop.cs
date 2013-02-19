using UnityEngine;
using System.Collections;

public class AddToTroop : MonoBehaviour
{
	void Awake ()
	{
		GameController.GetInstance ().GetTroopController ().AddSoldier (this.GetComponent<Unit> ());
	}
	
	void OnDead ()
	{
		GameController.GetInstance ().GetTroopController ().RemoveSoldier (this.GetComponent<Unit> ());
	}
}
