using UnityEngine;
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
		while (!async.isDone)
		{
			progBar.value = async.progress + 0.1f;
			yield return 0;

		}

		Debug.Log("Loading complete");
	}

}
