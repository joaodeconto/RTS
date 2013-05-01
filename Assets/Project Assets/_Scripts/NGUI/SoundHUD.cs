using UnityEngine;
using System.Collections;
using Visiorama.Audio;

public class SoundHUD : MonoBehaviour {
	
	void OnActivate (bool isChecked)
	{
		Sound.soundOn = isChecked;
		AudioSource[] audioSources = GameObject.FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		if (Sound.soundOn)
		{
			Sound.SetVolume (1f, audioSources);
		}
		else
		{
			Sound.SetVolume (0f, audioSources);
		}
	}
}
