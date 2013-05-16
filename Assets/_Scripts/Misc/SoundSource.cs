using UnityEngine;
using System.Collections.Generic;
using Visiorama.Audio;
using Visiorama;

[AddComponentMenu("Game/Audio/SoundSource")]
public class SoundSource : MonoBehaviour
{
	public List<Sound> sounds = new List<Sound> ();
	
	public SoundType soundType;
	
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
			
//			switch (soundType)
//			{
//			case SoundType.Music:
//				audioSource.volume = VolumeController.musicVolume;
//				break;
//			case SoundType.Sound:
//				audioSource.volume = VolumeController.soundVolume;
//				break;
//			case SoundType.Voice:
//				audioSource.volume = VolumeController.voiceVolume;
//				break;
//			}
			
			return audioSource;
		}
	}
	
	void Awake ()
	{
		ComponentGetter.Get<VolumeController> ().AddSoundSource (this);
	}
	
//	void OnDestroy ()
//	{
//		ComponentGetter.Get<VolumeController> ().RemoveSoundSource (this);
//	}
	
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
