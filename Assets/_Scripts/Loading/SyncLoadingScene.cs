using UnityEngine;
using System.Collections;

public class SyncLoadingScene : MonoBehaviour 
{
	public UISlider progBar;

	void Start()
	{
		//PlayerPrefs.DeleteAll();
		//PlayerPrefs.SetString("ReUser","");
		//PlayerPrefs.SetString("RePassword","");
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
	}

	public void LoadPlayerPrefabs ()
	{		
		if (!PlayerPrefs.HasKey("TouchSense"))			PlayerPrefs.SetFloat("TouchSense", 0.5f);	
		if (!PlayerPrefs.HasKey("DoubleClickSpeed"))	PlayerPrefs.SetFloat("DoubleClickSpeed", 1f);			
		if (!PlayerPrefs.HasKey("AllVolume"))			PlayerPrefs.SetFloat("AllVolume", 0.8f);
		if (!PlayerPrefs.HasKey("MusicVolume"))			PlayerPrefs.SetFloat("MusicVolume", 1f);
		if (!PlayerPrefs.HasKey("SFXVolume"))			PlayerPrefs.SetFloat("SFXVolume", 1f);			
		if (!PlayerPrefs.HasKey("GraphicQuality"))		QualitySettings.SetQualityLevel(1);				
		if (!PlayerPrefs.HasKey("Logins"))				PlayerPrefs.SetInt("Logins", 0);

		int logins = PlayerPrefs.GetInt("Logins");
		PlayerPrefs.SetInt("Logins", (logins+1));
			
		SoundManager.SetVolume (PlayerPrefs.GetFloat("AllVolume"));
		SoundManager.SetVolumeMusic (PlayerPrefs.GetFloat("MusicVolume"));
		SoundManager.SetVolumeSFX (PlayerPrefs.GetFloat("SFXVolume"));		
		QualitySettings.SetQualityLevel (PlayerPrefs.GetInt("GraphicQuality"));
	}

}
