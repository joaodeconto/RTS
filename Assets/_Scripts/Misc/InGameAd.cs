using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;
using Visiorama;

public class InGameAd : MonoBehaviour {

	public GameObject AdPanel;
	protected GameplayManager gameplayManager;
	private float adTrigger;


	void Start () 
	{
		gameplayManager = ComponentGetter.Get<GameplayManager>();
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(gameplayManager.gameTime > adTrigger)
		{
			Advertisement.Show(null, new ShowOptions{pause = true,resultCallback = result => {} });
		}	
	}

	void ActiveAdPanel()
	{
	}

}
