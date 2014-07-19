using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SFXPoolInfo {
	public int currentIndexInPool = 0;
	public int prepoolAmount = 0;
	public float baseVolume = 1f;
	public float volumeVariation = 0f;
	public float pitchVariation = 0f;
	public List<float> timesOfDeath = new List<float>();
	public List<GameObject> ownedAudioClipPool = new List<GameObject>();
	
	public SFXPoolInfo(int index, int minAmount, List<float> times, List<GameObject> pool, float baseVol=1f, float volVar=0f, float pitchVar=0f)
	{
		currentIndexInPool = index;
		prepoolAmount = minAmount;
		timesOfDeath = times;
		ownedAudioClipPool = pool;
		baseVolume = baseVol;
		volumeVariation = volVar;
		pitchVariation = pitchVar;
	}
}
