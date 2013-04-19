using UnityEngine;
using System.Collections;
using Visiorama.Audio;

public class SoundSource : MonoBehaviour
{
	public Sound[] sounds;
	
	private AudioSource audioSource;
	public AudioSource GetAudioSource
	{
		get
		{
			if (audioSource == null)
			{
				audioSource = audio;
				if (audioSource == null)
					audioSource = gameObject.AddComponent<AudioSource>();
			}
			
			return audioSource;
		}
	}
	
	public void Play (string name)
	{
		foreach (Sound sound in sounds)
		{
			if (name.ToLower().Equals(sound.name.ToLower()))
			{
				sound.Play (GetAudioSource, 0);
			}
		}
	}
}
