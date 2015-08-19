using UnityEngine;
using System.Collections;
using Visiorama;

public class FogObserver : IStats {

	public override void Init () 
	{
		FogOfWar fw = ComponentGetter.Get<FogOfWar>();
		fw.AddEntity(transform, this);
	}

	#region Visibility
	public override void SetVisible(bool isVisible)
	{
		statsController.ChangeVisibility (this, isVisible);
	}
	
	public override bool IsVisible
	{
		get 
		{
			return model.activeSelf;
		}
	}
	#endregion

	
	// Update is called once per frame
	void Update () {
	
	}
}
