using UnityEngine;
using System.Collections;

public class MainFactory : FactoryBase
{
	public const int numberOfIncementUnits = 10;

	public override void Init ()
	{
		base.Init ();

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
			gameplayManager.IncrementMaxOfUnits (numberOfIncementUnits);
	}

	public override IEnumerator OnDie ()
	{
		if (photonView.isMine)
		{
			if (wasBuilt)
			{
				gameplayManager.DecrementMaxOfUnits (numberOfIncementUnits);
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
