using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
	
	[System.Serializable]
	public class Sound
	{
		public static bool soundOn = true;
		
		public string name;
		public List<AudioClip> soundsClip;
		
		public Sound ()
		{
			name = "";
			soundsClip = new List<AudioClip>();
			soundsClip.Add (null);
		}
		
		public void Play (AudioSource source)
		{
			Play (source, 0);
		}
		
		public void PlayRandom (AudioSource source)
		{
			Play (source, Random.Range (0, soundsClip.Count));
		}
		
		public void Play (AudioSource source, int index)
		{
			if (!soundOn) return;
			
			if (index < soundsClip.Count)
			{
				if (source.isPlaying) source.Stop ();
				source.clip = soundsClip[index];
				source.Play ();
			}
			else	
			{
				Debug.LogError ("Null reference index soundsClip. Index: " + index 
								+ " - Number of soundsClip: " + soundsClip.Count);
			}
		}
	}
	
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