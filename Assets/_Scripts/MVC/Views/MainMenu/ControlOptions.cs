using UnityEngine;
using System.Collections;
using Visiorama;
using Visiorama.Utils;

public class ControlOptions : MonoBehaviour
{

	protected TouchController touchController;
		

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


		touchController = ComponentGetter.Get<TouchController> ();

		Transform slider;

		DefaultCallbackButton dcb;
		
		Transform touch = this.transform.FindChild ("Menu").FindChild ("TouchSense");
		
		slider = touch.FindChild ("Slider");
		
		slider.GetComponent<UISlider>().value = PlayerPrefs.GetFloat("TouchSense");

		Transform doubleClick = this.transform.FindChild ("Menu").FindChild ("DoubleClick");
		
		slider = doubleClick.FindChild ("Slider");
		
		slider.GetComponent<UISlider>().value = PlayerPrefs.GetFloat("DoubleClickSpeed");

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
		PlayerPrefs.SetFloat("TouchSense", touchSense);

		if (touchController.mainCamera != null)
		{
		touchController.mainCamera.GetComponent<CameraMovement>().SetSpeed();
		}
	
	}

	public void SetPlayerDoubleClickSpeed (float doubleClick)
	{
		PlayerPrefs.SetFloat("DoubleClickSpeed", doubleClick + 0.1f);

		if (touchController != null)
		{
			touchController.SetDoubleClick (doubleClick);
		}
	}
	public void SetFullScreen (bool isFullScreen)
	{
		Screen.fullScreen = isFullScreen;
	}

	public void Close ()
	{
		gameObject.SetActive (false);
	}
}
