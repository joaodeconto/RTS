using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class FactoryController : MonoBehaviour
{
	[System.NonSerialized]
	public List<FactoryBase> factorys = new List<FactoryBase> ();
	[System.NonSerialized]
	public FactoryBase selectedFactory;
	
	protected SoundManager soundManager;
	
	public void Init ()
	{
		soundManager = ComponentGetter.Get<SoundManager> ();
	}

	public void AddFactory (FactoryBase factory)
	{
		factorys.Add (factory);
		ComponentGetter.Get<MiniMapController> ().AddStructure (factory.transform, factory.team);
		ComponentGetter.Get<FogOfWar> ().AddEntity (factory.transform, factory);
	}

	public void RemoveFactory (FactoryBase factory)
	{
		factorys.Remove (factory);
		ComponentGetter.Get<MiniMapController> ().RemoveStructure (factory.transform, factory.team);
		ComponentGetter.Get<FogOfWar> ().RemoveEntity (factory.transform, factory);
	}

	public void SelectFactory (FactoryBase factory)
	{
		if(!factory.IsVisible)
			return;

		selectedFactory = factory;
		
		PlaySelectSound ();

		factory.Select ();
	}

	public void DeselectFactory ()
	{
		if (selectedFactory != null)
		{
			if (selectedFactory.Deselect (false))
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
	
	public void PlaySelectSound ()
	{
		soundManager.PlayRandom ("BuildingSelected");
	}

	public void ChangeVisibility (FactoryBase factory, bool visibility)
	{
		ComponentGetter.Get<MiniMapController> ().SetVisibilityStructure (factory.transform, factory.team, visibility);
	}
}
