using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;
using Visiorama;

public class EndGameHUD : MonoBehaviour
{
	private GameObject endGameWait;
	private bool checkSaving = false;
	protected Score score;
	protected XTerrainDeformer xdeformer;
	public GameObject endGameUI;
	
	void Start ()
	{
		xdeformer = GameObject.Find ("Terrain").GetComponent<XTerrainDeformer>(); 
		score = Visiorama.ComponentGetter.Get <Score> ("$$$_Score");
		DefaultCallbackButton defaultCallbackButton;
		
		GameObject option = transform.FindChild ("Defeat").transform.FindChild ("End Game").gameObject;		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		defaultCallbackButton.Init (null,
									(ht_dcb) =>{QuitSave();});

		option = transform.FindChild ("Defeat").transform.FindChild ("Rematch").gameObject;		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		defaultCallbackButton.Init (null,
									(ht_dcb) =>{	
										if(Everyplay.IsRecording())	Everyplay.StopRecording();
										ComponentGetter.Get<OfflineScore>().DestroyMe();
										xdeformer.ResetTerrain();
										endGameUI.SetActive(true);
										Loading ld = endGameUI.GetComponent<Loading>();
										ld.forwardAlpha();										
										if(ConfigurationData.addPass || ConfigurationData.multiPass) {Application.LoadLevel (Application.loadedLevelName);}
										else{
										Advertisement.Show(null, new ShowOptions{pause = false,resultCallback = result =>{Application.LoadLevel (Application.loadedLevelName);}});
				
										}										
									});

		option = transform.FindChild ("Victory").transform.FindChild ("End Game").gameObject;		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		defaultCallbackButton.Init (null,
									(ht_dcb) =>{QuitSave();});

		option = transform.FindChild ("Victory").transform.FindChild ("Facebook Win1").gameObject;		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		defaultCallbackButton.Init (null,
		                            (ht_dcb) =>
		                            {
										if (FB.IsLoggedIn){
											FB.Feed(
													link: "https://play.google.com/store/apps/details?id=com.Visiorama.RTS",
													linkName: "Victory!",
													linkCaption: " 'Almighty leader, the enemy was defeated!'",
													linkDescription: " 'Let us prepare for the ritual, gather the gold for the blessing of the gods!..'", 
													picture: "http://www.visiorama.com.br/uploads/RTS/mkimages/Achiv10.png"								
												
													);
										}
										
									});	
	}

	void QuitSave()
	{
		if(Everyplay.IsRecording())	Everyplay.StopRecording();
		GameplayManager gm = ComponentGetter.Get<GameplayManager>();
		gm.selectedLevel.gameLevel.SetActive(false);
		StartCoroutine (LoadingScene());
	}
	
	IEnumerator LoadingScene()		
	{
		endGameUI.SetActive(true);
		Loading ld = endGameUI.GetComponent<Loading>();
		ld.forwardAlpha();	
		xdeformer.ResetTerrain();
		yield return new WaitForSeconds(2);
		AsyncOperation async = Application.LoadLevelAsync("main_menu");
		while (!async.isDone){
				yield return 0;
		}
	}
}