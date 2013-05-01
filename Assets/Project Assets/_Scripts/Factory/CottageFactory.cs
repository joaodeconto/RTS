using UnityEngine;
using System.Collections;

public class CottageFactory : FactoryBase
{
	public const int numberOfIncementUnits = 7;
	
	void ConstructFinished ()
	{
		if (gameplayManager.ReachedMaxPopulation)
			eventManager.AddEvent("reach max population");
		else
			gameplayManager.IncrementMaxOfUnits (numberOfIncementUnits);
	}
	
	public override IEnumerator OnDie ()
	{
		if (wasBuilt)
		{
			if (photonView.isMine) gameplayManager.DecrementMaxOfUnits (numberOfIncementUnits);
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
