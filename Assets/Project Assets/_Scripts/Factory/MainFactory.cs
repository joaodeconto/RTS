using UnityEngine;
using System.Collections;

public class MainFactory : FactoryBase
{
	void OnInstance ()
	{
		if (photonView.isMine)
		{
			gameplayManager.IncrementMainBase (team);
		}
	}
	
	void ConstructFinished ()
	{
		if (gameplayManager.ReachedMaxPopulation)
			eventManager.AddEvent("reach max population");
		else
			gameplayManager.IncrementMaxOfUnits (15);
	}
	
	public override IEnumerator OnDie ()
	{
		if (photonView.isMine)
		{
			if (wasBuilt)
			{
				gameplayManager.DecrementMaxOfUnits (15);
			}
			
			gameplayManager.DecrementMainBase (team);
		}
		
		return base.OnDie ();
	}
	
	// RPCs
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
