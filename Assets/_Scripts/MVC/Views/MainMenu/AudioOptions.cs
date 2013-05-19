using UnityEngine;
using System.Collections;
using Visiorama;

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

		VolumeController volumeController = ComponentGetter.Get<VolumeController>();
		
		DefaultCallbackButton dcb;
		
		Transform slider;
		Transform checkbox;
		
		Transform music = this.transform.FindChild ("Menu").FindChild ("Music");
		
		slider = music.FindChild ("Slider");
		
		slider.GetComponent<UISlider> ().sliderValue = VolumeController.musicVolume;
		
		dcb = slider.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null,
		(ht_dcb, sliderValue) => 
		{
			Debug.Log ("HERE: Music");
			VolumeController.musicVolume = sliderValue;
			volumeController.SetAllAudios ();
		});
		
		checkbox = music.FindChild ("Checkbox");

		checkbox.GetComponent<UICheckbox> ().isChecked = VolumeController.musicOn;
		
		dcb = checkbox.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null, null,
		(ht_dcb, checkedValue) => 
		{
			VolumeController.musicOn = checkedValue;
			volumeController.SetAllAudios ();
		});
		
		Transform sound = this.transform.FindChild ("Menu").FindChild ("Sound");
		
		slider = sound.FindChild ("Slider");
		
		slider.GetComponent<UISlider> ().sliderValue = VolumeController.soundVolume;

		dcb = slider.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null,
		(ht_dcb, sliderValue) => 
		{
			Debug.Log ("HERE: Sound");
			VolumeController.soundVolume = sliderValue;
			volumeController.SetAllAudios ();
		});
		
		checkbox = sound.FindChild ("Checkbox");

		checkbox.GetComponent<UICheckbox> ().isChecked = VolumeController.soundOn;
		
		dcb = checkbox.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null, null,
		(ht_dcb, checkedValue) => 
		{
			VolumeController.soundOn = checkedValue;
			volumeController.SetAllAudios ();
		});
		
		Transform voice = this.transform.FindChild ("Menu").FindChild ("Voice");
		
		slider = voice.FindChild ("Slider");
		
		slider.GetComponent<UISlider> ().sliderValue = VolumeController.voiceVolume;

		dcb = slider.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null,
		(ht_dcb, sliderValue) => 
		{
			Debug.Log ("HERE: Voice");
			VolumeController.voiceVolume = sliderValue;
			volumeController.SetAllAudios ();
		});
		
		checkbox = voice.FindChild ("Checkbox");

		checkbox.GetComponent<UICheckbox> ().isChecked = VolumeController.voiceOn;
		
		dcb = checkbox.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null, null,
		(ht_dcb, checkedValue) => 
		{
			VolumeController.voiceOn = checkedValue;
			volumeController.SetAllAudios ();
		});
		
		Transform close = this.transform.FindChild ("Menu").FindChild ("Close");
		
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

	public void Close ()
	{

	}
}
