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
			Application.LoadLevel (0);
		});
		
		option = transform.FindChild ("Victory").
				 transform.FindChild ("Main Menu").gameObject;
		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		defaultCallbackButton.Init (null,
		(ht_dcb) =>
		{
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
	}
}