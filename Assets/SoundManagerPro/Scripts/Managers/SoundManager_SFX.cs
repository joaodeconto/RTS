using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class SoundManager : Singleton<SoundManager> {
	
	/// <summary>
	/// Sets the SFX cap.
	/// </summary>
	/// <param name='cap'>
	/// Cap.
	/// </param>
	public static void SetSFXCap(int cap)
	{
		Instance.CAP_AMOUNT = cap;
	}
    
	/// <summary>
	/// Plays the SFX on an owned object, will default the location to (0,0,0), pitch to 1f, volume to 1f
	/// </summary>
    public static AudioSource PlaySFX(AudioClip clip, float volume, float pitch, Vector3 location)
    {
        if (Instance.offTheSFX)
            return null;
            
        if (clip == null)
            return null;
        
        return Instance.PlaySFXAt(clip, volume, pitch, location);
    }
	
	/// <summary>
	/// Plays the SFX on an owned object, will default the location to (0,0,0), pitch to 1f, volume to 1f
	/// </summary>
	public static AudioSource PlaySFX(AudioClip clip, float volume, float pitch)
    {
        return PlaySFX(clip, volume, pitch, Vector3.zero);
    }
	
	/// <summary>
	/// Plays the SFX on an owned object, will default the location to (0,0,0), pitch to 1f, volume to 1f
	/// </summary>
	public static AudioSource PlaySFX(AudioClip clip, float volume)
    {
        return PlaySFX(clip, volume, Instance.pitchSFX);
    }
	
	/// <summary>
	/// Plays the SFX on an owned object, will default the location to (0,0,0), pitch to 1f, volume to 1f
	/// </summary>
	public static AudioSource PlaySFX(AudioClip clip)
    {
        return PlaySFX(clip, Instance.volumeSFX);
    }
	
	/// <summary>
	/// Plays the SFX IFF other SFX with the same cappedID are not over the cap limit. Will default the location to (0,0,0), pitch to 1f, volume to 1f
	/// </summary>
    public static AudioSource PlayCappedSFX(AudioClip clip, string cappedID, float volume, float pitch, Vector3 location)
    {
        if (Instance.offTheSFX)
            return null;
            
        if (clip == null)
            return null;
		
		if(string.IsNullOrEmpty(cappedID))
			return null;
        
        // Play the clip if not at capacity
		if(!Instance.IsAtCapacity(cappedID, clip.name))
            return Instance.PlaySFXAt(clip, volume, pitch, location, true, cappedID);
		else
			return null;
    }
	
	/// <summary>
	/// Plays the SFX IFF other SFX with the same cappedID are not over the cap limit. Will default the location to (0,0,0), pitch to 1f, volume to 1f
	/// </summary>
	public static AudioSource PlayCappedSFX(AudioClip clip, string cappedID, float volume, float pitch)
    {
        return PlayCappedSFX(clip, cappedID, volume, pitch, Vector3.zero);
    }
	
	/// <summary>
	/// Plays the SFX IFF other SFX with the same cappedID are not over the cap limit. Will default the location to (0,0,0), pitch to 1f, volume to 1f
	/// </summary>
	public static AudioSource PlayCappedSFX(AudioClip clip, string cappedID, float volume)
    {
        return PlayCappedSFX(clip, cappedID, volume, Instance.pitchSFX);
    }
	
	/// <summary>
	/// Plays the SFX IFF other SFX with the same cappedID are not over the cap limit. Will default the location to (0,0,0), pitch to 1f, volume to 1f
	/// </summary>
	public static AudioSource PlayCappedSFX(AudioClip clip, string cappedID)
    {
        return PlayCappedSFX(clip, cappedID, Instance.volumeSFX);
    }
	
	/// <summary>
	/// Plays the SFX another gameObject of your choice, will default the looping to false, pitch to 1f, volume to 1f
	/// </summary>
    public static AudioSource PlaySFX(GameObject gO, AudioClip clip, bool looping, float volume, float pitch)
    {
        if (Instance.offTheSFX)
            return null;
            
        if ((clip == null) || (gO == null))
            return null;
        
        if (gO.audio == null)
            gO.AddComponent<AudioSource>();
		
		// Keep reference of unownedsfx objects
		if(!Instance.unOwnedSFXObjects.Contains(gO))
			Instance.unOwnedSFXObjects.Add(gO);
            
        gO.audio.Stop();
        gO.audio.pitch = pitch;
        gO.audio.clip = clip;
        gO.audio.loop = looping;
        gO.audio.volume = volume;
		gO.audio.mute = Instance.mutedSFX;
        gO.audio.Play();
		
		return gO.audio;
    }
	
	/// <summary>
	/// Plays the SFX another gameObject of your choice, will default the looping to false, pitch to 1f, volume to 1f
	/// </summary>
	public static AudioSource PlaySFX(GameObject gO, AudioClip clip, bool looping, float volume)
    {
        return PlaySFX(gO, clip, looping, volume, Instance.pitchSFX);
    }
	
	/// <summary>
	/// Plays the SFX another gameObject of your choice, will default the looping to false, pitch to 1f, volume to 1f
	/// </summary>
	public static AudioSource PlaySFX(GameObject gO, AudioClip clip, bool looping)
    {
        return PlaySFX(gO, clip, looping, Instance.volumeSFX);
    }
	
	/// <summary>
	/// Plays the SFX another gameObject of your choice, will default the looping to false, pitch to 1f, volume to 1f
	/// </summary>
	public static AudioSource PlaySFX(GameObject gO, AudioClip clip)
    {
        return PlaySFX(gO, clip, false);
    }
	
	/// <summary>
	/// Stops the SFX on another gameObject
	/// </summary>
    public static void StopSFXObject(GameObject gO)
    {
        if (gO == null)
            return;
        
        if (gO.audio == null)
            return;
            
        if (gO.audio.isPlaying)
            gO.audio.Stop();
    }
	
	/// <summary>
	/// Stops all SFX.
	/// </summary>
	public static void StopSFX()
	{
		Instance._StopSFX();
	}
	
	/// <summary>
	/// Plays the SFX in a loop on another gameObject of your choice.  This function is cattered more towards customizing a loop.
	/// You can set the loop to end when the object dies or a maximum duration, whichever comes first.
	/// tillDestroy defaults to true, volume to 1f, pitch to 1f, maxDuration to 0f
	/// </summary>
    public static AudioSource PlaySFXLoop(GameObject gO, AudioClip clip, bool tillDestroy, float volume, float pitch, float maxDuration)
    {
        if (Instance.offTheSFX)
            return null;
            
        if ((clip == null) || (gO == null))
            return null;
		
		if (gO.audio == null)
            gO.AddComponent<AudioSource>();
		
		if(!Instance.unOwnedSFXObjects.Contains(gO))
			Instance.unOwnedSFXObjects.Add(gO);
		
		AudioSource aSource = gO.audio;
		aSource.Stop();
		aSource.clip = clip;
		aSource.pitch = pitch;
		aSource.volume = volume;
		aSource.mute = Instance.mutedSFX;
		aSource.loop = true;
		aSource.Play();

        Instance.StartCoroutine(Instance._PlaySFXLoopTillDestroy(gO, aSource, tillDestroy, maxDuration));
		return aSource;
    }
	
	/// <summary>
	/// Plays the SFX in a loop on another gameObject of your choice.  This function is cattered more towards customizing a loop.
	/// You can set the loop to end when the object dies or a maximum duration, whichever comes first.
	/// tillDestroy defaults to true, volume to 1f, pitch to 1f, maxDuration to 0f
	/// </summary>
	public static AudioSource PlaySFXLoop(GameObject gO, AudioClip clip, bool tillDestroy, float volume, float pitch)
    {
        return PlaySFXLoop(gO, clip, tillDestroy, volume, pitch, 0f);
    }
	
	/// <summary>
	/// Plays the SFX in a loop on another gameObject of your choice.  This function is cattered more towards customizing a loop.
	/// You can set the loop to end when the object dies or a maximum duration, whichever comes first.
	/// tillDestroy defaults to true, volume to 1f, pitch to 1f, maxDuration to 0f
	/// </summary>
	public static AudioSource PlaySFXLoop(GameObject gO, AudioClip clip, bool tillDestroy, float volume)
    {
        return PlaySFXLoop(gO, clip, tillDestroy, volume, Instance.pitchSFX);
    }
	
	/// <summary>
	/// Plays the SFX in a loop on another gameObject of your choice.  This function is cattered more towards customizing a loop.
	/// You can set the loop to end when the object dies or a maximum duration, whichever comes first.
	/// tillDestroy defaults to true, volume to 1f, pitch to 1f, maxDuration to 0f
	/// </summary>
	public static AudioSource PlaySFXLoop(GameObject gO, AudioClip clip, bool tillDestroy)
    {
        return PlaySFXLoop(gO, clip, tillDestroy, Instance.volumeSFX);
    }
	
	/// <summary>
	/// Plays the SFX in a loop on another gameObject of your choice.  This function is cattered more towards customizing a loop.
	/// You can set the loop to end when the object dies or a maximum duration, whichever comes first.
	/// tillDestroy defaults to true, volume to 1f, pitch to 1f, maxDuration to 0f
	/// </summary>
	public static AudioSource PlaySFXLoop(GameObject gO, AudioClip clip)
    {
        return PlaySFXLoop(gO, clip, true);
    }
	
	/// <summary>
	/// Sets mute on all the SFX to 'mute'. Returns the result.
	/// </summary>
	public static bool MuteSFX(bool toggle)
    {
        Instance.mutedSFX = toggle;
		return Instance.mutedSFX;
    }
	
	/// <summary>
	/// Toggles mute on SFX. Returns the result.
	/// </summary>
	public static bool MuteSFX()
    {
        return MuteSFX(!Instance.mutedSFX);
    }
	
	/// <summary>
	/// Sets the maximum volume of SFX in the game relative to the global volume.
	/// </summary>
	public static void SetVolumeSFX(float setVolume)
	{
		setVolume = Mathf.Clamp01(setVolume);
		
		float currentPercentageOfVolume;
		currentPercentageOfVolume = Instance.volumeSFX / Instance.maxSFXVolume;
		
		Instance.maxSFXVolume = setVolume * Instance.maxVolume;
		
		if(float.IsNaN(currentPercentageOfVolume) || float.IsInfinity(currentPercentageOfVolume))
			currentPercentageOfVolume = 1f;
		
		Instance.volumeSFX = Instance.maxSFXVolume * currentPercentageOfVolume;
	}
	
	/// <summary>
	/// Sets the pitch of SFX in the game.
	/// </summary>
	public static void SetPitchSFX(float setPitch)
	{
		Instance.pitchSFX = setPitch;
	}
	
	/////////////////////////////////////////////////////
	
	/// <summary>
	/// Saves the SFX to the SoundManager prefab for easy access for frequently used SFX.  Will register the SFX to the group specified if it exists.
	/// </summary>
	public static void SaveSFX(AudioClip clip, string grpName)
	{
		if(clip == null)
			return;
		
		SFXGroup grp = Instance.GetGroupByGroupName(grpName);
		if(grp != null)
		{
			if(!Instance.clipToGroupKeys.Contains(clip.name))
				Instance.AddClipToGroup(clip.name, grpName);
			else
				Debug.LogWarning("This AudioClip("+clip.name+") is already assigned to a group: "+Instance.GetClipToGroup(clip.name)+". You cannot add a clip to 2 groups.");
		}
		else
			Debug.LogWarning("The SFX group, "+grpName+", does not exist.");
		
		SaveSFX(clip);
	}
	
	/// <summary>
	/// Saves the SFX to the SoundManager prefab for easy access for frequently used SFX.  Will register the SFX to the group specified.
	/// If the group doesn't exist, it will be added to SoundManager.
	public static void SaveSFX(AudioClip clip, SFXGroup grp)
	{
		if(clip == null)
			return;
		
		if(grp != null)
		{
			if(!Instance.sfxGroups.Contains(grp))
				Instance.sfxGroups.Add(grp);
			if(!Instance.clipToGroupKeys.Contains(clip.name))
				Instance.AddClipToGroup(clip.name, grp.groupName);
			else
				Debug.LogWarning("This AudioClip("+clip.name+") is already assigned to a group: "+Instance.GetClipToGroup(clip.name)+". You cannot add a clip to 2 groups.");
		}
		
		SaveSFX(clip);
	}
	
	/// <summary>
	/// Saves the SFX to the SoundManager prefab for easy access for frequently used SFX.
	/// </summary>
	public static void SaveSFX(AudioClip clip)
	{
		if(clip == null)
			return;
		
		Instance.storedSFXs.Add(clip);
	}
	
	/// <summary>
	/// Creates the SFX group and adds it to SoundManager.
	/// </summary>
	public static SFXGroup CreateSFXGroup(string grpName, int capAmount)
	{
		SFXGroup grp = ScriptableObject.CreateInstance<SFXGroup>();
		grp.Initialize(grpName, capAmount);
		if(!Instance.sfxGroups.Contains(grp))
		{
			Instance.sfxGroups.Add(grp);
			return grp;
		}
		Debug.LogWarning("This group already exists. Cannot add it.");
		return null;
	}
	
	/// <summary>
	/// Creates the SFX group and adds it to SoundManager.
	/// </summary>
	public static SFXGroup CreateSFXGroup(string grpName)
	{
		SFXGroup grp = ScriptableObject.CreateInstance<SFXGroup>();
		grp.Initialize(grpName);
		if(!Instance.sfxGroups.Contains(grp))
		{
			Instance.sfxGroups.Add(grp);
			return grp;
		}
		Debug.LogWarning("This group already exists. Cannot add it.");
		return null;
	}
	
	/// <summary>
	/// Moves a clip to the specified group. If the group doesn't exist, it will make the group.
	/// </summary>
	public static void MoveToSFXGroup(string clipName, string newGroupName)
	{
		SFXGroup newGrp = Instance.GetGroupByGroupName(newGroupName);
		if(!newGrp)
			CreateSFXGroup(newGroupName);
		
		SFXGroup grp = Instance.GetGroupForClipName(clipName);
		if(grp)
		{
			if(grp.groupName == newGroupName)
				return;
			Instance.SetClipToGroup(clipName, newGroupName);
		} else {
			Instance.AddClipToGroup(clipName, newGroupName);
		}
	}
	
	/// <summary>
	/// Loads a random SFX from a specified group.
	/// </summary>
	public static AudioClip LoadFromGroup(string grpName)
	{
		if(!Instance.GetGroupByGroupName(grpName))
		{
			Debug.LogError("There is no group by this name, "+grpName+".");
			return null;
		}
		
		AudioClip result = null;
		List<string> availableClipNames = new List<string>();
		
		// Get all clip names that match the group name
		for(int i = 0; i < Instance.clipToGroupKeys.Count; i++)
			if(Instance.clipToGroupValues[i] == grpName)
				availableClipNames.Add(Instance.clipToGroupKeys[i]);
		
		if(availableClipNames.Count == 0)
		{
			Debug.LogWarning("There are no clips in this group.");
			return null;
		}
		
		// Get a random clip name of that list of available clip names
		string clipNameToPlay = availableClipNames[Random.Range(0,availableClipNames.Count)];
		
		// Find it and return it
		result = Instance.storedSFXs.Find(delegate(AudioClip clip) {
			return clipNameToPlay == clip.name;
		});

		return result;
	}
	
	/// <summary>
	/// Loads all SFX from a specified group.
	/// </summary>
	public static AudioClip[] LoadAllFromGroup(string grpName)
	{
		if(!Instance.GetGroupByGroupName(grpName))
		{
			Debug.LogError("There is no group by this name, "+grpName+".");
			return null;
		}
		
		List<string> availableClipNames = new List<string>();
		
		// Get all clip names that match the group name
		for(int i = 0; i < Instance.clipToGroupKeys.Count; i++)
			if(Instance.clipToGroupValues[i] == grpName)
				availableClipNames.Add(Instance.clipToGroupKeys[i]);
		
		if(availableClipNames.Count == 0)
		{
			Debug.LogWarning("There are no clips in this group.");
			return null;
		}
		
		// Find all clips
		List<AudioClip> clips = Instance.storedSFXs.FindAll(delegate(AudioClip clip) {
			return availableClipNames.Contains(clip.name);
		});
		
		return clips.ToArray();
	}
	
	/// <summary>
	/// Load the specified clipname, at a custom path if you do not want to use RESOURCES_PATH.
	/// If custompath fails or is empty/null, it will query the stored SFXs.  If that fails, it'll query the default
	/// RESOURCES_PATH.  If all else fails, it'll return null.
	/// </summary>
	/// <param name='clipname'>
	/// Clipname.
	/// </param>
	/// <param name='customPath'>
	/// Custom path.
	/// </param>
	public static AudioClip Load(string clipname, string customPath)
	{
		AudioClip result = null;
		
		// Attempt to use custom path if provided
		if(!string.IsNullOrEmpty(customPath))
			if(customPath[customPath.Length-1] == '/')
				result = (AudioClip)Resources.Load(customPath.Substring(0,customPath.Length) + "/" + clipname);
			else
				result = (AudioClip)Resources.Load(customPath + "/" + clipname);
				
		if(result)
			return result;
		
		// If custom path fails, attempt to find it in our stored SFXs
		result = Instance.storedSFXs.Find(delegate(AudioClip clip) {
			return clipname == clip.name;
		});
		
		if(result)
			return result;
		
		// If it is not in our stored SFX, attempt to find it in our default resources path
		result = (AudioClip)Resources.Load(Instance.RESOURCES_PATH + "/" + clipname);
		
		return result;
	}
	
	/// <summary>
	/// Load the specified clipname from the stored SFXs.  If that fails, it'll query the default
	/// RESOURCES_PATH.  If all else fails, it'll return null.
	/// </summary>
	/// <param name='clipname'>
	/// Clipname.
	/// </param>
	public static AudioClip Load(string clipname)
	{
		return Load(clipname, "");
	}
	
	/// <summary>
	/// Resets the SFX object.
	/// </summary>
	public static void ResetSFXObject(GameObject sfxObj)
	{
		if(sfxObj.audio == null)
			return;
		
		sfxObj.audio.mute = false;
		sfxObj.audio.bypassEffects = false;
		sfxObj.audio.playOnAwake = false;
		sfxObj.audio.loop = false;
		
		sfxObj.audio.priority = 128;
		sfxObj.audio.volume = 1f;
		sfxObj.audio.pitch = 1f;
		
		sfxObj.audio.dopplerLevel = 1f;
		sfxObj.audio.rolloffMode = AudioRolloffMode.Logarithmic;
		sfxObj.audio.minDistance = 1f;
		sfxObj.audio.panLevel = 1f;
		sfxObj.audio.spread = 0f;
		sfxObj.audio.maxDistance = 500f;
		
		sfxObj.audio.pan = 0f;
	}
}
