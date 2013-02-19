using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Game/Audio/Character Sound")]
public class CharacterSound : MonoBehaviour
{
	
	[System.Serializable]
	public class ExtraSounds
	{
		public string name;
		public GameObject soundSource;
		public AudioClip soundClip;
		
		private AudioSource sourceAudio;
		public AudioSource SourceAudio
		{
			get
			{
				if (null == sourceAudio)
				{
					sourceAudio = soundSource.audio;
				}
				return sourceAudio;
			}
		}
		
		public void Play ()
		{
			SourceAudio.PlayOneShot (soundClip);
		}
	}
	
	public GameObject footstepSoundSource;
	public GameObject damageSoundSource;
	public GameObject deathSoundSource;
	
	public AudioClip footstepSoundClip;
	public List<AudioClip> damageSoundClips = new List<AudioClip> ();
	public List<AudioClip> deathSoundClips = new List<AudioClip> ();
	
	public List<ExtraSounds> extraSounds = new List<ExtraSounds> ();
	
	private AudioSource footstepAudioSource;
	public AudioSource FootstepAudioSource
	{
		get
		{
			if (null == footstepAudioSource)
			{
				footstepAudioSource = footstepSoundSource.audio;
			}
			return footstepAudioSource;
		}
	}
	
	private AudioSource damageAudioSource;
	public AudioSource DamageAudioSource
	{
		get
		{
			if (null == damageAudioSource)
			{
				damageAudioSource = damageSoundSource.audio;
			}
			return damageAudioSource;
		}
	}
	
	private AudioSource deathAudioSource;
	public AudioSource DeathAudioSource
	{
		get
		{
			if (null == deathAudioSource)
			{
				deathAudioSource = deathSoundSource.audio;
			}
			return deathAudioSource;
		}
	}
	
	public void PlayExtra (string nameExtra)
	{
		foreach (ExtraSounds es in extraSounds)
		{
			if (nameExtra.Equals(es.name))
			{
				es.Play ();
			}
		}
	}
}