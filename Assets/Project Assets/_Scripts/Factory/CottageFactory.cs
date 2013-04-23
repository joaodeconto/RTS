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
	
	public override IEnumerator OnDie ()
	{
		if (wasBuilt)
		{
			if (photonView.isMine) gameplayManager.DecrementMaxOfUnits (5);
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
