using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;
using Visiorama;

public class EndGameHUD : MonoBehaviour
{
	private GameObject endGameWait;
	protected Score score;
	protected XTerrainDeformer xdeformer;
	protected GameplayManager gm;
	public GameObject endGameUI;
	public GameObject AdsBtnPanel;
	private DefaultCallbackButton dcb;
	
	void Start ()
	{
		gm = ComponentGetter.Get<GameplayManager>();
		xdeformer = GameObject.Find ("Terrain").GetComponent<XTerrainDeformer>(); 
		score = Visiorama.ComponentGetter.Get <Score> ("$$$_Score");

		
		GameObject option = transform.FindChild ("Defeat").transform.FindChild ("End Game").gameObject;		
		dcb = option.AddComponent<DefaultCallbackButton> ();
		dcb.Init (null,(ht_dcb) =>{QuitSave();});

		option = transform.FindChild ("Defeat").transform.FindChild ("Rematch").gameObject;		
		if(VersusScreen.modeLabelString  == "Single Player"){
			dcb = option.AddComponent<DefaultCallbackButton>();
			dcb.Init (null,(ht_dcb) =>{	
				ResetLevel();
				if(ConfigurationData.addPass) {Application.LoadLevel (Application.loadedLevel);}
				else{
					//ActiveAdsPanel();
					Advertisement.Show(null, new ShowOptions{resultCallback = result =>{Application.LoadLevel (Application.loadedLevel);}});				
				}										
			});
		}
		else option.SetActive(false);

		option = transform.FindChild ("Victory").transform.FindChild ("End Game").gameObject;		
		dcb = option.AddComponent<DefaultCallbackButton> ();
		dcb.Init (null,(ht_dcb) =>{QuitSave();});

		option = transform.FindChild ("Victory").transform.FindChild ("Next Level").gameObject;
		if(VersusScreen.modeLabelString  == "Single Player"){
			dcb = option.AddComponent<DefaultCallbackButton> ();
			dcb.Init (null,(ht_dcb) =>{
				ResetLevel();
				if(ConfigurationData.level == 3){
					ConfigurationData.level = 1;
					if(ConfigurationData.addPass) {Application.LoadLevel(Application.loadedLevel+1);}
					else{
						//ActiveAdsPanel();
						Advertisement.Show(null, new ShowOptions{resultCallback = result =>{Application.LoadLevel(Application.loadedLevel+1);}});
					}					
				}										
				else {
					ConfigurationData.level ++;
					if(ConfigurationData.addPass) {Application.LoadLevel(Application.loadedLevel);}
					else{
						//ActiveAdsPanel();
						Advertisement.Show(null, new ShowOptions{resultCallback = result =>{Application.LoadLevel(Application.loadedLevel);}});
					}	
				}
			});
		}
		else option.SetActive(false);
	}

	void QuitSave()
	{
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
		while (!async.isDone){yield return 0;}
	}

	void ActiveAdsPanel()
	{
		AdsBtnPanel.SetActive(true);
		Transform purNoAds = AdsBtnPanel.transform.FindChild ("Yes");
		if (purNoAds != null){
			dcb = purNoAds.GetComponent<DefaultCallbackButton>();
			dcb.Init(null,(ht_dcb)=>{
				StoreManager.NoAdsPurchase();
				AdsBtnPanel.SetActive(false);
			});
		}		
		Transform closeAds = AdsBtnPanel.transform.FindChild ("No");
		if (closeAds != null){
			dcb = closeAds.GetComponent<DefaultCallbackButton>();
			dcb.Init(null,(ht_dcb)=>{
				AdsBtnPanel.SetActive(false);
			});
		}
	}

	void ResetLevel()
	{	
		gm.selectedLevel.gameLevel.SetActive(false);
		ComponentGetter.Get<OfflineScore>().DestroyMe();
		xdeformer.ResetTerrain();
		endGameUI.SetActive(true);
		Loading ld = endGameUI.GetComponent<Loading>();
		ld.forwardAlpha();									
	}
}