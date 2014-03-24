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

		DefaultCallbackButton dcb;
		
		Transform slider;
		Transform checkbox;
		
		Transform music = this.transform.FindChild ("Menu").FindChild ("Music");
		
		slider = music.FindChild ("Slider");
		
		slider.GetComponent<UISlider> ().value = SoundManager.GetVolumeMusic ();
		
		dcb = slider.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null,
		(ht_dcb, sliderValue) => 
		{
			SoundManager.SetVolumeMusic (sliderValue);
		});
		
		checkbox = music.FindChild ("CheckBox");

		checkbox.GetComponent<UIToggle> ().value = SoundManager.IsMusicMuted ();
		
		dcb = checkbox.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null, null,
		(ht_dcb, checkedValue) => 
		{
			SoundManager.MuteMusic (!checkedValue);
		});
		
		Transform sound = this.transform.FindChild ("Menu").FindChild ("Sound");
		
		slider = sound.FindChild ("Slider");
		
		slider.GetComponent<UISlider> ().sliderValue = SoundManager.GetVolumeSFX ();

		dcb = slider.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null,
		(ht_dcb, sliderValue) => 
		{
			SoundManager.SetVolumeSFX (sliderValue);
		});
		
		checkbox = sound.FindChild ("CheckBox");

		checkbox.GetComponent<UIToggle> ().value = SoundManager.IsSFXMuted ();
		
		dcb = checkbox.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null, null,
		(ht_dcb, checkedValue) => 
		{
			SoundManager.MuteSFX (!checkedValue);
		});
		
		Transform voice = this.transform.FindChild ("Menu").FindChild ("Voice");
		
		slider = voice.FindChild ("Slider");
		
		slider.GetComponent<UISlider> ().value = VolumeController.voiceVolume;

		dcb = slider.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null,
		(ht_dcb, sliderValue) => 
		{
			Debug.Log ("HERE: Voice");
		});
		
		checkbox = voice.FindChild ("CheckBox");

		checkbox.GetComponent<UIToggle> ().value = VolumeController.voiceOn;
		
		dcb = checkbox.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null, null,
		(ht_dcb, checkedValue) => 
		{
			Debug.Log ("HERE: Voice");
//			VolumeController.voiceOn = checkedValue;
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
