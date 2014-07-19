using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "AntiLunchBox/SoundPocket" )]
public class SoundPocket : MonoBehaviour {
	public string pocketName = "Pocket";
	public SoundPocketType pocketType = SoundPocketType.Additive;
	public List<AudioClip> pocketClips = new List<AudioClip>();
	
	public List<string> sfxGroups = new List<string>();
	
	public List<string> clipToGroupKeys = new List<string>();
	public List<string> clipToGroupValues = new List<string>();
	public List<int> sfxPrePoolAmounts = new List<int>();
	public List<float> sfxBaseVolumes = new List<float>();
	public List<float> sfxVolumeVariations = new List<float>();
	public List<float> sfxPitchVariations = new List<float>();
	
	private Dictionary<string, string> clipsInGroups = new Dictionary<string, string>();
	
	/// EDITOR variables. DO NOT TOUCH.
	public bool showAsGrouped = false;
	public List<bool> showSFXDetails = new List<bool>();
	/// EDITOR variables. DO NOT TOUCH.
	public int groupAddIndex = 0;
	/// EDITOR variables. DO NOT TOUCH.
	public int autoPrepoolAmount = 0;
	/// EDITOR variables. DO NOT TOUCH.
	public float autoBaseVolume = 1f;
	/// EDITOR variables. DO NOT TOUCH.
	public float autoVolumeVariation = 0f;
	/// EDITOR variables. DO NOT TOUCH.
	public float autoPitchVariation = 0f;
	
	void Awake()
	{
		Setup();
		DestroyMe();
	}
	
	public void Setup()
	{
		SetupDictionaries();
		switch(pocketType)
		{
		case SoundPocketType.Subtractive:
			if(SoundManager.Instance.currentPockets.Count == 1 && SoundManager.Instance.currentPockets[0] == pocketName)
				return;
			SoundManager.DeleteSFX();
			SoundManager.Instance.currentPockets.Clear();
			break;
		case SoundPocketType.Additive:
		default:
			if(SoundManager.Instance.currentPockets.Contains(pocketName))
				return;
			break;
		}
		
		for(int i = 0; i < pocketClips.Count; i++)
		{
			AudioClip pocketClip = pocketClips[i];
			if(clipsInGroups.ContainsKey(pocketClip.name))
				SoundManager.SaveSFX(pocketClip, clipsInGroups[pocketClip.name]);
			else
				SoundManager.SaveSFX(pocketClip);
			
			SoundManager.ApplySFXAttributes(pocketClip, sfxPrePoolAmounts[i], sfxBaseVolumes[i], sfxVolumeVariations[i], sfxPitchVariations[i]);
		}
		
		SoundManager.Instance.currentPockets.Add(pocketName);
	}
	
	private void SetupDictionaries()
	{
		clipsInGroups.Clear();
		for(int i = 0; i < clipToGroupKeys.Count; i++)
			clipsInGroups.Add(clipToGroupKeys[i], clipToGroupValues[i]);
	}
	
	public void DestroyMe()
	{
		
		if((gameObject.GetComponents<Component>().Length-gameObject.GetComponents<Transform>().Length) == 1)
			Destroy(gameObject);
		else
			Destroy(this);
	}
}

public enum SoundPocketType
{
	Additive,
	Subtractive
}
