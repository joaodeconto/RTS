using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class SoundManager : Singleton<SoundManager> {

    private void SetupSoundFX()
    {
		SetupDictionary();
		
		ownedSFXObjects.Clear();
		unOwnedSFXObjects.Clear();
		cappedSFXObjects.Clear();
		
		AddOwnedSFXObject();
        mCurrentOwnedSFXObject = 0;
    }
	
	/* these functions have to be here for the editor to work with dictionaries */
	private void SetupDictionary()
	{
		if(clipToGroupKeys.Count != clipToGroupValues.Count) //this should never be the case, but in case they are out of sync, sync them.
		{
			if(clipToGroupKeys.Count > clipToGroupValues.Count)
				clipToGroupKeys.RemoveRange(clipToGroupValues.Count, clipToGroupKeys.Count - clipToGroupValues.Count);
			else if(clipToGroupValues.Count > clipToGroupKeys.Count)
				clipToGroupValues.RemoveRange(clipToGroupKeys.Count, clipToGroupValues.Count - clipToGroupKeys.Count);
		}
	}
	
	private void AddClipToGroup(string key, string val)
	{
		clipToGroupKeys.Add(key);
		clipToGroupValues.Add(val);
	}
	
	private void SetClipToGroup(string key, string val)
	{
		int index = clipToGroupKeys.IndexOf(key);
		clipToGroupValues[index] = val;
	}
	
	private void RemoveClipToGroup(string key)
	{
		int index = clipToGroupKeys.IndexOf(key);
		clipToGroupKeys.RemoveAt(index);
		clipToGroupValues.RemoveAt(index);
	}
	
	private string GetClipToGroup(string key)
	{
		int index = clipToGroupKeys.IndexOf(key);
		return clipToGroupValues[index];
	}
	/* end of editor necessary functions */	

	private void PSFX(bool pause)
	{
		foreach(GameObject ownedSFXObject in ownedSFXObjects)
		{
#if UNITY_3_4 || UNITY_3_5
			if(ownedSFXObject != null && ownedSFXObject.active)
#else
			if(ownedSFXObject != null && ownedSFXObject.activeSelf)
#endif
				if(ownedSFXObject.audio != null)
					if(pause)
						ownedSFXObject.audio.Pause();
					else
						ownedSFXObject.audio.Play();
		}
		foreach(GameObject unOwnedSFXObject in unOwnedSFXObjects)
		{
#if UNITY_3_4 || UNITY_3_5
			if(unOwnedSFXObject != null && unOwnedSFXObject.active)
#else
			if(unOwnedSFXObject != null && unOwnedSFXObject.activeSelf)
#endif
				if(unOwnedSFXObject.audio != null)
					if(pause)
						unOwnedSFXObject.audio.Pause();
					else
						unOwnedSFXObject.audio.Play();
		}
	}
	
	private void HandleSFX()
    {
		if(isPaused)
			return;
		
        // Deactivate objects
        for (int i=0; i<ownedSFXObjects.Count; ++i)
        {
#if UNITY_3_4 || UNITY_3_5
            if (ownedSFXObjects[i].active)
#else
			if(ownedSFXObjects[i].activeSelf)
#endif
                if (!ownedSFXObjects[i].audio.isPlaying)
			    {
					int instanceID = ownedSFXObjects[i].GetInstanceID();
				    if(cappedSFXObjects.ContainsKey(instanceID))
					     cappedSFXObjects.Remove(instanceID);
#if UNITY_3_4 || UNITY_3_5
                    ownedSFXObjects[i].SetActiveRecursively(false);
#else
					ownedSFXObjects[i].SetActive(false);
#endif
			    }
        }
		
		// Handle removing unowned audio sfx
		for(int i = unOwnedSFXObjects.Count - 1; i >= 0; i--)
		{
			if(unOwnedSFXObjects[i] != null)
				if(unOwnedSFXObjects[i].audio != null)
					if(unOwnedSFXObjects[i].audio.isPlaying || unOwnedSFXObjects[i].audio.mute)
						continue;
			unOwnedSFXObjects.RemoveAt(i);
		}
    }

    private GameObject GetNextInactiveSFXObject()
    {
        for (int i = (mCurrentOwnedSFXObject + 1)% ownedSFXObjects.Count; i != mCurrentOwnedSFXObject; i = (i + 1) % ownedSFXObjects.Count)
        {
#if UNITY_3_4 || UNITY_3_5
            if (!ownedSFXObjects[i].active)
#else
			if (!ownedSFXObjects[i].activeSelf)
#endif
            {
                mCurrentOwnedSFXObject = i;
				ResetSFXObject(ownedSFXObjects[i]);
                return ownedSFXObjects[i];
            }
        }
        return AddOwnedSFXObject();
    }
	
	private GameObject AddOwnedSFXObject()
	{
		GameObject SFXObject = new GameObject("SFX", typeof(AudioSource));
		SFXObject.name += "(" + SFXObject.GetInstanceID() + ")";
		SFXObject.audio.playOnAwake = false;
		GameObject.DontDestroyOnLoad(SFXObject);
		ownedSFXObjects.Add(SFXObject);
		ResetSFXObject(SFXObject);
		return SFXObject;
	}

    private AudioSource PlaySFXAt(AudioClip clip, float volume, float pitch, Vector3 location, bool capped, string cappedID)
    {
        
        GameObject tempGO = GetNextInactiveSFXObject();
        if (tempGO == null)
            return null;
		
		tempGO.transform.position = location;
#if UNITY_3_4 || UNITY_3_5
        tempGO.SetActiveRecursively(true);
#else
		tempGO.SetActive(true);
#endif
        AudioSource aSource = tempGO.audio;
        aSource.Stop();
        aSource.clip = clip;
        aSource.pitch = pitch;
        aSource.volume = volume;
		aSource.mute = mutedSFX;
        aSource.Play();
		
		if(capped && !string.IsNullOrEmpty(cappedID))
			cappedSFXObjects.Add(tempGO.GetInstanceID(), cappedID);
		
        return aSource;
    }
	
	private AudioSource PlaySFXAt(AudioClip clip, float volume, float pitch, Vector3 location)
    {
        return PlaySFXAt(clip, volume, pitch, location, false, "");
    }
	
	private AudioSource PlaySFXAt(AudioClip clip, float volume, float pitch)
    {
        return PlaySFXAt(clip, volume, pitch, Vector3.zero);
    }

	private IEnumerator _PlaySFXLoopTillDestroy(GameObject gO, AudioSource source, bool tillDestroy, float maxDuration)
	{
		
		bool trackEndTime = false;
		float endTime = Time.time + maxDuration;
		if(!tillDestroy || maxDuration > 0.0f)
			trackEndTime = true;
		
		
		while(ShouldContinueLoop(gO, trackEndTime, endTime))
		{
			yield return null;
		}
		
		source.Stop();
	}
	
	private void _StopSFX()
	{
		foreach(GameObject ownedSFXObject in ownedSFXObjects)
#if UNITY_3_4 || UNITY_3_5
			if(ownedSFXObject != null && ownedSFXObject.active)
#else
			if(ownedSFXObject != null && ownedSFXObject.activeSelf)
#endif
				if(ownedSFXObject.audio != null)
					ownedSFXObject.audio.Stop();
		
		foreach(GameObject unOwnedSFXObject in unOwnedSFXObjects)
#if UNITY_3_4 || UNITY_3_5
			if(unOwnedSFXObject != null && unOwnedSFXObject.active)
#else
			if(unOwnedSFXObject != null && unOwnedSFXObject.activeSelf)
#endif
				if(unOwnedSFXObject.audio != null)
					unOwnedSFXObject.audio.Stop();
	}
	
	private bool ShouldContinueLoop(GameObject gO, bool trackEndTime, float endTime)
	{
#if UNITY_3_4 || UNITY_3_5
		bool shouldContinue = (gO != null && gO.active);
#else
		bool shouldContinue = (gO != null && gO.activeSelf);
#endif
		if(trackEndTime)
			shouldContinue = (shouldContinue && Time.time < endTime);
		return shouldContinue;
	}
	
	/// <summary>
	/// Determines whether the specified cappedID is at capacity.
	/// </summary>
	private bool IsAtCapacity(string cappedID, string clipName)
	{
		int thisCapAmount = CAP_AMOUNT;
		
		// Check if in a group and has a specific cap amount
		SFXGroup grp = GetGroupForClipName(clipName);
		if(grp)
		{
			if(grp.specificCapAmount == 0) // If no cap amount on this group
				return false;
			if(grp.specificCapAmount != -1) // If it is a specific cap amount
				thisCapAmount = grp.specificCapAmount;
		}
		
		// If there are no other capped objects with this cappedID, then it can't be at capacity
		if(!cappedSFXObjects.ContainsValue(cappedID))
			return false;
		
		// Check the count of capped objects with the same cappedID, if >= return true
		int count = 0;
		foreach(string id in cappedSFXObjects.Values)
		{
			if(id == cappedID)
			{
				count++;
				if(count >= thisCapAmount)
					return true;
			}
		}
		return false;
	}
	
	private SFXGroup GetGroupForClipName(string clipName)
	{
		if(clipToGroupKeys.Contains(clipName))
			return sfxGroups.Find(delegate(SFXGroup grp) {
				return grp.groupName == GetClipToGroup(clipName);
			});
		return null;
	}
	
	private SFXGroup GetGroupByGroupName(string grpName)
	{
		return sfxGroups.Find(delegate(SFXGroup grp) {
			return grp.groupName == grpName;
		});
	}
}
