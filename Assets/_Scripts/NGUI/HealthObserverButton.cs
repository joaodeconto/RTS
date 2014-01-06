﻿using UnityEngine;
using System.Collections;

public class HealthObserverButton : PersonalizedCallbackButton
{
	private IHealthObservable observable = null;
	private HealthBar healthBar = null;
	
	void OnDestroy ()
	{
		StopToObserve ();
	}

	public override void Init(Hashtable ht,
	                          OnClickDelegate onClick = null,
	                          OnPressDelegate onPress = null,
	                          OnSliderChangeDelegate onSliderChangeDelegate = null,
	                          OnActivateDelegate onActivateDelegate = null,
	                          OnRepeatClickDelegate onRepeatClickDelegate = null,
	                          OnDragDelegate onDrag = null,
	                          OnDropDelegate onDrop = null)
	{
		healthBar = GetComponentInChildren<HealthBar> ();

		base.Init(ht, onClick, onPress, onSliderChangeDelegate, onActivateDelegate, onRepeatClickDelegate, onDrag, onDrop);
		
		if (!ht.ContainsKey ("observableHealth"))
		{
			Debug.LogError ("Eh necessario ter um \"observableHealth\" na hashtable enviada como parametro");
			return;
		}

		observable = ht["observableHealth"] as IHealthObservable;

//		observable.RegisterHealthObserver (healthBar);
		healthBar.SetTarget (observable);

		ChangeParams(ht, onClick, onPress, onSliderChangeDelegate, onActivateDelegate, onRepeatClickDelegate, onDrag, onDrop);
	}
	
	public override void ChangeParams(Hashtable ht,
	                                  OnClickDelegate onClick = null,
	                                  OnPressDelegate onPress = null,
	                                  OnSliderChangeDelegate onSliderChangeDelegate = null,
	                                  OnActivateDelegate onActivateDelegate = null,
	                                  OnRepeatClickDelegate onRepeatClickDelegate = null,
	                                  OnDragDelegate onDrag = null,
	                                  OnDropDelegate onDrop = null)
	{
		base.ChangeParams(ht, onClick, onPress, onSliderChangeDelegate, onActivateDelegate, onRepeatClickDelegate, onDrag, onDrop);
		
		healthBar.gameObject.SetActive (true);
	}

	public void StopToObserve ()
	{
		observable.UnRegisterHealthObserver (healthBar);
		healthBar.gameObject.SetActive (false);
	}
}