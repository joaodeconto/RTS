using UnityEngine;
using System.Collections;

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
									(ht_dcb) =>
									{	
										xdeformer.ResetTerrain();
			endGameUI.SetActive(true);
			Loading ld = endGameUI.GetComponent<Loading>();
										ld.forwardAlpha();										
										checkSaving = true;
									});


		
		option = transform.FindChild ("Victory").transform.FindChild ("End Game").gameObject;		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		defaultCallbackButton.Init (null,
									(ht_dcb) =>
									{	
										xdeformer.ResetTerrain();
			endGameUI.SetActive(true);
			Loading ld = endGameUI.GetComponent<Loading>();
										ld.forwardAlpha();										
										checkSaving = true;
									});

		option = transform.FindChild ("Victory").transform.FindChild ("Facebook Win1").gameObject;		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		defaultCallbackButton.Init (null,
		                            (ht_dcb) =>
		                            {
										if (FB.IsLoggedIn)
										{

										FB.Feed(
												link: "https://play.google.com/store/apps/details?id=com.Visiorama.RTS",
												linkName: "Victory!",
												linkCaption: " 'Almighty leader, the enemy was defeated!'",
												linkDescription: " 'Let us prepare for the ritual, gather the gold for the blessing of the gods!..'", 
												picture: "http://www.visiorama.com.br/uploads/RTS/mkimages/Achiv10.png"								
											
												);
										}
										
									});		
//		option = transform.FindChild ("Victory").
//			transform.FindChild ("Back to game").gameObject;
//		
//		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
//		defaultCallbackButton.Init (null,
//		                            (ht_dcb) =>
//		                            {
//			gameObject.SetActive (false);
//		});
//
//		option = transform.FindChild ("Defeat").
//			transform.FindChild ("Back to game").gameObject;
//		
//		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
//		defaultCallbackButton.Init (null,
//		                            (ht_dcb) =>
//		                            {
//			gameObject.SetActive (false);
//		});
	}

	void Update()
	{
		if(checkSaving)
		{
			if(!score.isSaving)
			{
				Application.LoadLevel (1);
				checkSaving = false;
			}
		}
	}
}