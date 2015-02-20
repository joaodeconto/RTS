using UnityEngine;
using Visiorama;
using System.Collections;
using System;

public class MeleeAtkUp : MonoBehaviour {

	public int atkBonus;
	public string category = "Unit"; //typeof deve ser uma classe;
	protected StatsController statsController;

	void Start ()
	{
		statsController = ComponentGetter.Get<StatsController>();


		foreach (IStats stat in statsController.myStats)
		{
			if (stat.GetType() == typeof(Unit))
			{
				Unit u = stat as Unit;
				u.AdditionalForce += atkBonus;

			}
		}



	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
