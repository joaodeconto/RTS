using UnityEngine;
using System.Collections;

using Visiorama;

public class LoginIndex : IView
{				
	public UIInput username;
	public UIInput password;
	public UILabel errorMessage;
	public GameObject FacebookButton;
	public GameObject SubmitButton;
	public GameObject NewAccountButton;
		
	private FacebookLoginHandler fh;

	// Use this for initialization
	public void Init ()
	{
		LoadRePrefs();

		errorMessage.enabled = false;
		
		FacebookButton
			.AddComponent<DefaultCallbackButton>()
			.Init
			(
				null,
				(ht_hud) =>
				{
					fh.DoLogin ();
				}
			);
		
		GameObject goFacebookHandler;	
		goFacebookHandler = new GameObject ("FacebookLoginHandler");
		goFacebookHandler.transform.parent = this.transform;
		
		fh = goFacebookHandler.AddComponent <FacebookLoginHandler> ();

		fh.OnLoggedIn = Yupy;





		
		SubmitButton
			.AddComponent<DefaultCallbackButton>()
			.Init (null,
			(ht_hud) =>
			{
				//TODO lógica de login do jogo
				if (string.IsNullOrEmpty(username.value) ||
					string.IsNullOrEmpty(password.value))
					return;

				Hashtable ht = new Hashtable ();
					ht["username"] = username.value;
					ht["password"] = password.value;

				controller.SendMessage ("DoLogin", ht, SendMessageOptions.DontRequireReceiver );
			});

		NewAccountButton
			.AddComponent<DefaultCallbackButton>()
			.Init
			(
				null,
				(ht_hud) =>
				{
					controller.SendMessage ("NewAccount", SendMessageOptions.DontRequireReceiver );
				}
			);
	}
	public void FBShareRTS ()
	{
		FB.AppRequest(
			message:"3D Real-Time Strategy game for mobile!",
			title:"Join me in RTS - Rex Tribal Society!"
			
			);
	}
	
	public bool Yupy ()
	{
		FB.AppRequest(
			message:"3D Real-Time Strategy game for mobile!",
			title:"Join me in RTS - Rex Tribal Society!"
			
			);
		errorMessage.enabled = true;
		errorMessage.text = "Facebook Authorized";
		Invoke ("CloseErrorMessage", 5.0f);
	
		return true;
	}

	public void ShowErrorMessage ()
	{
		errorMessage.enabled = true;
		errorMessage.text = "Incorrect User or Password";
		Invoke ("CloseErrorMessage", 5.0f);
	}

	private void CloseErrorMessage ()
	{
		errorMessage.enabled = false;
	}

	public void LoadRePrefs ()
	{
		username.value = PlayerPrefs.GetString("ReUser");
		password.value = PlayerPrefs.GetString("RePassword");
	}
}
