using UnityEngine;
using System.Collections;

public class MainFactory : FactoryBase
{
	void ConstructFinished ()
	{
		if (gameplayManager.ReachedMaxPopulation)
			eventManager.AddEvent("reach max population");
		else
			gameplayManager.IncrementMaxOfUnits (8);
	}
	
	public override void OnDie ()
	{
		if (wasBuilt) gameplayManager.DecrementMaxOfUnits (8);
		
		base.OnDie ();
	}
}
