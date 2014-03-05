using UnityEngine;
using System.Collections;

public class CottageFactory : FactoryBase, IHouse
{
	public const int cottagePopulation = 7;
	
	void ConstructFinished ()
	{
		if (gameplayManager.ReachedMaxPopulation)
			eventManager.AddEvent("reach max population");
		else
			gameplayManager.AddHouse (this);
	}
	
	public override IEnumerator OnDie ()
	{
		if (wasBuilt)
		{
			if (photonView.isMine) gameplayManager.RemoveHouse (this);
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

	#region IHouse implementation

	public int GetHousePopulation ()
	{
		return cottagePopulation;
	}

	#endregion
}
