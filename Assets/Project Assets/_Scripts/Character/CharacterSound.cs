using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Game/Audio/Character Sound")]
public class CharacterSound : MonoBehaviour
{
	
//	[System.Serializable]
//	public class ExtraSounds
//	{
//		public string name;
//		public GameObject soundSource;
//		public AudioClip soundClip;
//		
//		private AudioSource audioSource;
//		public AudioSource GetAudioSource
//		{
//			get
//			{
//				if (null == audioSource)
//				{
//					sourceAudio = soundSource.audio;
//				}
//				return sourceAudio;
//			}
//		}
//		
//		public void Play ()
//		{
//			SourceAudio.PlayOneShot (soundClip);
//		}
//	}
//	
//	public AudioClip selectedSound;
//	public AudioClip deathSoundClip;
//	
//	public List<ExtraSounds> extraSounds = new List<ExtraSounds> ();
//	
//	private AudioSource audioSource;
//	public AudioSource GetAudioSource
//	{
//		get
//		{
//			if (null == audioSource)
//			{
//				GameObject newAudioSource = Instantiate ();
//			}
//			return footstepAudioSource;
//		}
//	}
//	
//	private AudioSource damageAudioSource;
//	public AudioSource DamageAudioSource
//	{
//		get
//		{
//			if (null == damageAudioSource)
//			{
//				damageAudioSource = damageSoundSource.audio;
//			}
//			return damageAudioSource;
//		}
//	}
//	
//	private AudioSource deathAudioSource;
//	public AudioSource DeathAudioSource
//	{
//		get
//		{
//			if (null == deathAudioSource)
//			{
//				deathAudioSource = deathSoundSource.audio;
//			}
//			return deathAudioSource;
//		}
//	}
//	
//	public void PlayExtra (string nameExtra)
//	{
//		foreach (ExtraSounds es in extraSounds)
//		{
//			if (nameExtra.Equals(es.name))
//			{
//				es.Play ();
//			}
//		}
//	}
}