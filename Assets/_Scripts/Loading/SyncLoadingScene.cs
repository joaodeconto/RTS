﻿using UnityEngine;
using System.Collections;

public class SyncLoadingScene : MonoBehaviour 
{
	public UISlider progBar;

	void Start()
	{
		StartCoroutine (LoadingScene ());
	}

	IEnumerator LoadingScene()
	
	{
		yield return new WaitForSeconds(1f);
		AsyncOperation async = Application.LoadLevelAsync("main_menu");
		LoadPlayerPrefabs ();
		while (!async.isDone){
			progBar.value = async.progress + 0.1f;
			yield return 0;
		}

		Debug.Log("Loading complete");
	}

	public void LoadPlayerPrefabs ()
	{
		
		if (!PlayerPrefs.HasKey("TouchSense"))
		{
			PlayerPrefs.SetFloat("TouchSense", 1f);
		}
		if (!PlayerPrefs.HasKey("DoubleClickSpeed"))
		{
			PlayerPrefs.SetFloat("DoubleClickSpeed", 1f);
		}		
		if (!PlayerPrefs.HasKey("AllVolume"))
		{
			PlayerPrefs.SetFloat("AllVolume", 1f);
		}
		if (!PlayerPrefs.HasKey("MusicVolume"))
		{
			PlayerPrefs.SetFloat("MusicVolume", 1f);
		}
		if (!PlayerPrefs.HasKey("SFXVolume"))
		{
			PlayerPrefs.SetFloat("SFXVolume", 1f);
		}		
		if (!PlayerPrefs.HasKey("GraphicQuality"))
		{
			QualitySettings.SetQualityLevel(4);
		}
		
		if (!PlayerPrefs.HasKey("Logins"))
		{
			PlayerPrefs.SetInt("Logins", 0);
		}
		else
		{
			int logins = PlayerPrefs.GetInt("Logins");
			PlayerPrefs.SetInt("Logins", (logins+1));
		}
		
		SoundManager.SetVolume (PlayerPrefs.GetFloat("AllVolume"));
		SoundManager.SetVolumeMusic (PlayerPrefs.GetFloat("MusicVolume"));
		SoundManager.SetVolumeSFX (PlayerPrefs.GetFloat("SFXVolume"));		
		QualitySettings.SetQualityLevel (PlayerPrefs.GetInt("GraphicQuality"));
	}

}
