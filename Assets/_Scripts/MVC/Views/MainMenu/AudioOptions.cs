using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class AudioOptions : MonoBehaviour
{
	private bool wasInitialized = false;
	public UISlider allVolume;
	public UISlider sfxVolume;
	public UISlider musicVolume;

	public void OnEnable ()
	{
		Open ();
	}

	public void OnDisable ()
	{
		Close ();
	}

	public void SetPlayerAllVolume (float volume)
	{
		PlayerPrefs.SetFloat("AllVolume", volume);
		SoundManager.SetVolume (PlayerPrefs.GetFloat("AllVolume"));

	}

	public void SetPlayerMusicVolume (float volume)
	{
		PlayerPrefs.SetFloat("MusicVolume", volume);
		SoundManager.SetVolumeMusic (PlayerPrefs.GetFloat("MusicVolume"));

	}

	public void SetPlayerSFXVolume (float volume)
	{
		PlayerPrefs.SetFloat("SFXVolume", volume);
		SoundManager.SetVolumeSFX (PlayerPrefs.GetFloat("SFXVolume"));
	}

void Open ()
	{
		if (wasInitialized)
			return;

		wasInitialized = true;

		allVolume.value = PlayerPrefs.GetFloat("AllVolume");
		musicVolume.value = PlayerPrefs.GetFloat("MusicVolume");
		sfxVolume.value = PlayerPrefs.GetFloat("SFXVolume");

		DefaultCallbackButton dcb;
				
				
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
		gameObject.SetActive (false);
	}
}
