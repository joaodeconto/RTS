using UnityEngine;
using System.Collections;

public class CottageFactory : FactoryBase
{
	void ConstructFinished ()
	{
		if (gameplayManager.ReachedMaxPopulation)
			eventManager.AddEvent("reach max population");
		else
			gameplayManager.IncrementMaxOfUnits (5);
	}
	
	public override void OnDie ()
	{
		if (wasBuilt) gameplayManager.DecrementMaxOfUnits (5);
		
		base.OnDie ();
	}
}
