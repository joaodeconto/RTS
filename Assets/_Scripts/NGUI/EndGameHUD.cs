using UnityEngine;
using System.Collections;

public class EndGameHUD : MonoBehaviour {
	
	void Awake ()
	{
		DefaultCallbackButton defaultCallbackButton;
		
		GameObject option = transform.FindChild ("Defeat").
							transform.FindChild ("Main Menu").gameObject;
		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		defaultCallbackButton.Init (null,
		(ht_dcb) =>
		{
			if (!PhotonNetwork.offlineMode) PhotonNetwork.LeaveRoom ();
			Application.LoadLevel (0);
		});
		
		option = transform.FindChild ("Victory").
				 transform.FindChild ("Main Menu").gameObject;
		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		defaultCallbackButton.Init (null,
		(ht_dcb) =>
		{
			if (!PhotonNetwork.offlineMode) PhotonNetwork.LeaveRoom ();
			Application.LoadLevel (0);
		});
		
		option = transform.FindChild ("Victory").
				 transform.FindChild ("Back to game").gameObject;
		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		defaultCallbackButton.Init (null,
		(ht_dcb) =>
		{
			gameObject.SetActive (false);
		});

		option = transform.FindChild ("Victory").
			transform.FindChild ("Facebook Win1").gameObject;
		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		defaultCallbackButton.Init (null,
		                            (ht_dcb) =>
		                            {
			if (FB.IsLoggedIn)
			{

			FB.Feed(
					link: "https://www.facebook.com/RexTribalS",
					linkName: "Victory!",
					linkCaption: " 'Almighty leader, the enemy was defeated!'",
					linkDescription: " 'Let us prepare the ritual, gather the gold for the blessing of the gods!..'", 
					picture: "https://www.facebook.com/photo.php?fbid=181734578645845&set=pb.181504285335541.-2207520000.1400530680.&type=3&theater"
		
				
				);
			}
			
		});
	}
}