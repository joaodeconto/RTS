using UnityEngine;
using System.Collections.Generic;

using Visiorama.Audio;

public class SoundManager : MonoBehaviour
{
	
	public List<Sound> sounds = new List<Sound> ();
	
	private AudioSource audioSource;
	public AudioSource GetAudioSource
	{
		get
		{
			if (audioSource == null)
			{
				audioSource = gameObject.audio;
				if (audioSource == null)
				{
					audioSource = gameObject.AddComponent<AudioSource> ();
				}
			}
			return audioSource;
		}
	}

	
	public void Play (string name)
	{
		Play (name, 0);
	}
	
	public void Play (string name, int index)
	{
		foreach (Sound sound in sounds)
		{
			if (name.ToLower().Equals(sound.name.ToLower()))
			{
				sound.Play (GetAudioSource, index);
			}
		}
	}
	
	public void PlayRandom (string name)
	{
		foreach (Sound sound in sounds)
		{
			if (name.ToLower().Equals(sound.name.ToLower()))
			{
				sound.PlayRandom (GetAudioSource);
			}
		}
	}
}