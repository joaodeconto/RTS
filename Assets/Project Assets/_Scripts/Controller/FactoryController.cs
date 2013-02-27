using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

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
		ComponentGetter.Get<MiniMapController> ().AddStructure (factory.transform, factory.Team);
	}

	public void RemoveFactory (FactoryBase factory)
	{
		factorys.Remove (factory);
		ComponentGetter.Get<MiniMapController> ().RemoveStructure (factory.transform, factory.Team);
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
}
