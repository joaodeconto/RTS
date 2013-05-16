using UnityEngine;
using System.Collections;

public class UnitCallbackButton : MonoBehaviour
{
	public Unit unit;
	public FactoryBase factory;

	public void Init (Unit unit, FactoryBase factory)
	{
		this.unit    = unit;
		this.factory = factory;
	}

	void OnClick ()
	{
		if (!factory.OverLimitCreateUnit)
			factory.EnqueueUnitToCreate (unit);
	}
}
