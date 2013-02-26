using UnityEngine;
using System.Collections;

public class UnitCallbackButton : MonoBehaviour
{
	public Unit unit;
	public float timeToCreate;
	public FactoryBase factory;
	
	public void Init (Unit unit, float timeToCreate, FactoryBase factory)
	{
		this.unit = unit;
		this.timeToCreate = timeToCreate;
		this.factory = factory;
	}
	
	void OnClick ()
	{
		if (!factory.OverLimitCreateUnit ()) factory.CallUnit (unit, timeToCreate);
	}
}
