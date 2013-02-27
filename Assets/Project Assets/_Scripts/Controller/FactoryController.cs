using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FactoryController : MonoBehaviour
{
	internal List<FactoryBase> factorys = new List<FactoryBase> ();
	internal FactoryBase selectedFactory;
	
	public void Init ()
	{
	}
	
	public void AddFactory (FactoryBase factory)
	{
		factorys.Add (factory);
	}
	
	public void RemoveFactory (FactoryBase factory)
	{
		factorys.Remove (factory);
	}
	
	public void SelectFactory (FactoryBase factory)
	{
		selectedFactory = factory;
		
		factory.Active ();
	}
	
	public void DeselectFactory ()
	{
		if (selectedFactory != null)
		{
			selectedFactory.Deactive ();
			selectedFactory = null;
		}
	}
	
	public FactoryBase FindFactory (string name)
	{
		foreach (FactoryBase factory in factorys)
		{
			if (factory.name.Equals(name))
			{
				return factory;
			}
		}
		
		return null;
	}
}
