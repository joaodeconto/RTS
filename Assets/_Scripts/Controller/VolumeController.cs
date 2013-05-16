using UnityEngine;
using System.Collections.Generic;

public enum SoundType
{
	Music,
	Sound,
	Voice
}

public class VolumeController : MonoBehaviour
{	
	public static bool musicOn = true;
	public static float musicVolume = 1f;
	public static bool soundOn = true;
	public static float soundVolume = 1f;
	public static bool voiceOn = true;
	public static float voiceVolume = 1f;
	
	public List<SoundSource> soundSources = new List<SoundSource>();
	
	public void AddSoundSource (SoundSource soundSource)
	{
		soundSources.Add (soundSource);
		SetAudio (soundSource);
	}
	
	public void RemoveSoundSource (SoundSource soundSource)
	{
		soundSources.Remove (soundSource);
	}
	
	public void SetAllAudios ()
	{
		if (soundSources.Count == 0) return;
		
		foreach (SoundSource soundSource in soundSources)
		{
			SetAudio (soundSource);
		}
	}
	
	public void SetAudio (SoundSource soundSource)
	{
		if (soundSource.soundType == SoundType.Music)
		{
			soundSource.GetAudioSource.mute   = !musicOn;
			soundSource.GetAudioSource.volume = musicVolume;
		}
		else if (soundSource.soundType == SoundType.Sound)
		{
			soundSource.GetAudioSource.mute   = !soundOn;
			soundSource.GetAudioSource.volume = soundVolume;
		}
		else
		{
			soundSource.GetAudioSource.mute   = !voiceOn;
			soundSource.GetAudioSource.volume = voiceVolume;
		}
	}
}
