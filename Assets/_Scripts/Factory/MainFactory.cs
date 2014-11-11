using UnityEngine;
using System.Collections;

public class MainFactory : FactoryBase, IHouse
{
	public const int mainFactoryPopulation = 10;
	
	void OnInstanceFactory ()
	{
		if (photonView.isMine)
		{
			gameplayManager.IncrementMainBase (team);
		}
	}

	void ConstructFinished ()
	{
		if (gameplayManager.ReachedMaxPopulation)
			eventController.AddEvent("reach max population");
		else
			gameplayManager.AddHouse (this);
	}

	public override IEnumerator OnDie ()
	{
		if (photonView.isMine)
		{
			if (wasBuilt)
			{
				gameplayManager.RemoveHouse (this);
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

	#region IHouse implementation

	public int GetHousePopulation ()
	{
		return mainFactoryPopulation;
	}

	#endregion
}
