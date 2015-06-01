using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;
using Visiorama;

public class InGameAd : MonoBehaviour {

	protected GameplayManager gameplayManager;
	private float adTrigger = 320f;

	void Start () 
	{
		if(PlayerPrefs.GetInt("Logins") <= 3) enabled = false;
		gameplayManager = ComponentGetter.Get<GameplayManager>();	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(gameplayManager.gameTime >= adTrigger)
		{
			Advertisement.Show(null, new ShowOptions{pause = true,resultCallback = result => {} });
		}	
	}

	void DestroyAd()
	{
		Destroy(gameObject);
	}

}
