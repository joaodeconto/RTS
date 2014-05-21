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
//
//	public void MuteVolume (bool check)
//	{
//		SoundManager.Mute (check);
//	}
//	
//	public void SetVolume (float sliderVolume)
//	{
//		SoundManager.SetVolume (sliderVolume);
//	}
//
//	public void SetMusicVolume (float sliderVolume)
//	{
//		SoundManager.SetVolumeMusic (sliderVolume);
//	}
//
//	public void SetSFXVolume (float sliderVolume)
//	{
//		SoundManager.SetVolumeSFX (sliderVolume);
//	}
//
//	public void MuteMusicVolume (bool check)
//	{
//		SoundManager.MuteMusic (check);
//	}
//
//	public void MuteSFXVolume (bool check)
//	{
//		SoundManager.MuteSFX (check);
//	}

	public void Open ()
	{
		if (wasInitialized)
			return;

		wasInitialized = true;


      //  soundManager = ComponentGetter.Get<SoundManager> ().maxVolume;

		DefaultCallbackButton dcb;
		
		Transform slider;
		Transform checkbox;


		Transform music = this.transform.FindChild ("Menu").FindChild ("Music");
		
		slider = music.FindChild ("Slider");
		
		slider.GetComponent<UISlider> ().value = SoundManager.GetVolumeMusic();
		
		dcb = slider.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null,
		(ht_dcb, sliderValue) => 
		{

			SoundManager.SetVolumeMusic (sliderValue);
		});
		
		checkbox = music.FindChild ("CheckBox");

		checkbox.GetComponent<UIToggle> ().value = SoundManager.IsMuted ();
		
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

		checkbox.GetComponent<UIToggle> ().value = !SoundManager.IsSFXMuted ();
		
		dcb = checkbox.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null, null,
		(ht_dcb, checkedValue) => 
		{
			SoundManager.MuteSFX (!checkedValue);
		});
		
		Transform voice = this.transform.FindChild ("Menu").FindChild ("Voice");
		
		slider = voice.FindChild ("Slider");
		
		slider.GetComponent<UISlider> ().value = SoundManager.GetVolume();;

		dcb = slider.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null,
		(ht_dcb, sliderValue) => 
		{
			SoundManager.SetVolume( sliderValue);
		});
		
		checkbox = voice.FindChild ("CheckBox");

		checkbox.GetComponent<UIToggle> ().value = SoundManager.IsMuted();
		
		dcb = checkbox.gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init(null, null, null, null,
		(ht_dcb, checkedValue) => 
		{
			SoundManager.Mute (!checkedValue);
		});
		
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

	public void Close ()
	{

	}
}
