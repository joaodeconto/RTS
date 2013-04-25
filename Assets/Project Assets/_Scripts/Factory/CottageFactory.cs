using UnityEngine;
using System.Collections;

public class CottageFactory : FactoryBase
{
	void ConstructFinished ()
	{
		if (gameplayManager.ReachedMaxPopulation)
			eventManager.AddEvent("reach max population");
		else
			gameplayManager.IncrementMaxOfUnits (10);
	}
	
	public override IEnumerator OnDie ()
	{
		if (wasBuilt)
		{
			if (photonView.isMine) gameplayManager.DecrementMaxOfUnits (10);
		}
		
		return base.OnDie ();
	}
	
	// RPC
	[RPC]
	public override void InstantiatParticleDamage ()
	{
		base.InstantiatParticleDamage ();
	}
	
	[RPC]
	public override void SendRemove ()
	{
		base.SendRemove ();
	}
}
