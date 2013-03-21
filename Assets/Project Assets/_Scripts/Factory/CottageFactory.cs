using UnityEngine;
using System.Collections;
using Visiorama;

public class CottageFactory : FactoryBase
{
	void ConstructFinished ()
	{
		gameplayManager.IncrementMaxOfUnits ();
	}
	
	void OnDie ()
	{
		if (wasBuilt) gameplayManager.DecrementMaxOfUnits ();
	}
}