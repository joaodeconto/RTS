using UnityEngine;
using System.Collections;

public class AudioOptions : MonoBehaviour
{
	private bool wasInitialized = false;

	public void OnEnable ()
	{
		Open ();
	}

	public void OnDisable ()
	{
		Close ();
	}

	public void Open ()
	{
		if (wasInitialized)
			return;

		wasInitialized = true;

		DefaultCallbackButton dcb;
		
		Transform slider;
		Transform checkbox;
		
		Transform music = this.transform.FindChild ("Menu").FindChild ("Music");
		
		slider = music.FindChild ("Slider");

		dcb = slider.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null,
		(ht_dcb, sliderValue) => 
		{
			
		});
		
		checkbox = music.FindChild ("Checkbox");

		dcb = checkbox.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null, null,
		(ht_dcb, checkedValue) => 
		{
		});

	}

	public void Close ()
	{

	}
}
