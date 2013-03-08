using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class FactoryController : MonoBehaviour
{
	public List<FactoryBase> factorys = new List<FactoryBase> ();
	public FactoryBase selectedFactory;

	public void Init ()
	{
	}

	public void AddFactory (FactoryBase factory)
	{
		factorys.Add (factory);
		ComponentGetter.Get<MiniMapController> ().AddStructure (factory.transform, factory.Team);
		ComponentGetter.Get<FogOfWar> ().AddEntity (factory.transform, factory);
	}

	public void RemoveFactory (FactoryBase factory)
	{
		factorys.Remove (factory);
		ComponentGetter.Get<MiniMapController> ().RemoveStructure (factory.transform, factory.Team);
		ComponentGetter.Get<FogOfWar> ().RemoveEntity (factory.transform, factory);
	}

	public void SelectFactory (FactoryBase factory)
	{
		if(!factory.IsVisible)
			return;

		selectedFactory = factory;

		factory.Select ();
	}

	public void DeselectFactory ()
	{
		if (selectedFactory != null)
		{
			if (selectedFactory.Deselect ()) selectedFactory = null;
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

	public void ChangeVisibility (FactoryBase factory, bool visibility)
	{
		ComponentGetter.Get<MiniMapController> ().SetVisibilityStructure (factory.transform, factory.Team, visibility);
	}
}
