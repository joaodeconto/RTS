using UnityEngine;
using System.Collections;
using Visiorama;

public class CottageFactory : FactoryBase
{
	protected bool construct = false;
	
	void FinishedConstruct ()
	{
		construct = true;
		gameplayManager.IncrementMaxOfUnits ();
	}
	
	void OnDie ()
	{
		if (construct) gameplayManager.DecrementMaxOfUnits ();
	}
}
