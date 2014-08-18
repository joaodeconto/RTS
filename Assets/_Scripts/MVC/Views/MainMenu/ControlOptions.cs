using UnityEngine;
using System.Collections;
using Visiorama;
using Visiorama.Utils;

public class ControlOptions : MonoBehaviour
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

		Transform slider;

		DefaultCallbackButton dcb;
		
		Transform touch = this.transform.FindChild ("Menu").FindChild ("TouchSense");
		
		slider = touch.FindChild ("Slider");
		
		slider.GetComponent<UISlider>().value = PlayerPrefs.GetFloat("TouchSense") - 0.1f;

		Transform doubleClick = this.transform.FindChild ("Menu").FindChild ("DoubleClick");
		
		slider = doubleClick.FindChild ("Slider");
		
		slider.GetComponent<UISlider>().value = PlayerPrefs.GetFloat("DoubleClickSpeed") - 0.1f;

		Transform close = this.transform.FindChild ("Menu").FindChild ("Resume");
		
		if (close != null)
		{
			dcb = close.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			(ht_dcb) => 
			{
				gameObject.SetActive (false);
			});
		}
	}

	public void SetPlayerTouchSense (float touchSense)
	{
		PlayerPrefs.SetFloat("TouchSense", touchSense + 0.1f);

		CameraMovement cam = ComponentGetter.Get<CameraMovement>();

		cam.SetSpeed (touchSense);


	}

	public void SetPlayerDoubleClickSpeed (float doubleClick)
	{
		PlayerPrefs.SetFloat("DoubleClickSpeed", doubleClick + 0.1f);
	}

	public void Close ()
	{

	}
}
