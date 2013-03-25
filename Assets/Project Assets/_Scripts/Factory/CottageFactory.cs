using UnityEngine;
using System.Collections;

public class CottageFactory : FactoryBase
{
	void ConstructFinished ()
	{
		if (gameplayManager.ReachedMaxPopulation)
			eventManager.AddEvent("reach max population");
		else
			gameplayManager.IncrementMaxOfUnits ();
	}

	void OnDie ()
	{
		if (wasBuilt) gameplayManager.DecrementMaxOfUnits ();
	}
}
