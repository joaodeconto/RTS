using UnityEngine;
using System.Collections;

public class EndGameHUD : MonoBehaviour {

	protected Score score;
	private bool pressedQuit = false;
	private GameObject endGameWait;
	
	void Awake ()
	{
		endGameWait = transform.parent.FindChild("WaitingPlayersUI").gameObject;
		score = Visiorama.ComponentGetter.Get <Score> ("$$$_Score");
		DefaultCallbackButton defaultCallbackButton;
		
		GameObject option = transform.FindChild ("Defeat").transform.FindChild ("End Game").gameObject;		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		defaultCallbackButton.Init (null,
									(ht_dcb) =>
									{
										pressedQuit = true;
									});


		
		option = transform.FindChild ("Victory").transform.FindChild ("End Game").gameObject;		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		defaultCallbackButton.Init (null,
									(ht_dcb) =>
									{
										pressedQuit = true;
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
												linkDescription: " 'Let us prepare the ritual, gather the gold for the blessing of the gods!..'", 
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
		if(pressedQuit)
		{
			endGameWait.SetActive(true);
			if(!score.isSaving)
			{
				if (!PhotonNetwork.offlineMode) PhotonNetwork.LeaveRoom ();
				Application.LoadLevel (0);
			}
		}
	}
}