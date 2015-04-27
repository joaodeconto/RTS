using UnityEngine;
using System.Collections;
using Visiorama;
using Visiorama.Utils;

public class ControlOptions : MonoBehaviour
{

	protected TouchController touchController;
	private UISlider touchSlider;
	private UISlider clickSlider;
		

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
		if(ConfigurationData.InGame) touchController = ComponentGetter.Get<TouchController> ();

		DefaultCallbackButton dcb;
		
		Transform touch = this.transform.FindChild ("Menu").FindChild ("TouchSense").FindChild ("Slider");;
		
		touchSlider = touch.GetComponent<UISlider>();

		touchSlider.value = PlayerPrefs.GetFloat("TouchSense");

		Transform doubleClick = this.transform.FindChild ("Menu").FindChild ("DoubleClick").FindChild ("Slider");;
				
		clickSlider =  doubleClick.GetComponent<UISlider>();

		clickSlider.value = PlayerPrefs.GetFloat("DoubleClickSpeed");

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
		PlayerPrefs.SetFloat("DoubleClickSpeed", doubleClick);

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
		PlayerPrefs.Save();
		gameObject.SetActive (false);
	}
}
