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
	public GameObject OfflineButton;
	public bool CanLogin;
		
	private FacebookLoginHandler fh;

	// Use this for initialization
	public void Init ()
	{
		InternetChecker internetChecker = GetComponent<InternetChecker>();
		internetChecker.enabled = true;
		LoadRePrefs();

		errorMessage.enabled = false;
		Login login = ComponentGetter.Get<Login>();

		OfflineButton.AddComponent<DefaultCallbackButton>().Init(null,(ht_hud) =>
		                                                         {
																	login.EnterOfflineMode();
																});
		
		FacebookButton.AddComponent<DefaultCallbackButton>().Init(null,(ht_hud) =>
																		{
																			fh.DoLogin ();
																		});		
		GameObject goFacebookHandler;	
		goFacebookHandler = new GameObject ("FacebookLoginHandler");
		goFacebookHandler.transform.parent = this.transform;		
		fh = goFacebookHandler.AddComponent <FacebookLoginHandler> ();
		fh.OnLoggedIn = Yupy;
		
		SubmitButton.AddComponent<DefaultCallbackButton>().Init (null, (ht_hud) =>
																		{
																			//TODO lógica de login do jogo
																			if (string.IsNullOrEmpty(username.value) ||
																				string.IsNullOrEmpty(password.value))
																				return;
																			if (CanLogin)
																			{
																				Hashtable ht = new Hashtable ();
																				ht["username"] = username.value;
																				ht["password"] = password.value;																							
																				internetChecker.enabled = false;
																				controller.SendMessage ("DoLogin", ht, SendMessageOptions.DontRequireReceiver );
																			}
																			else ShowErrorMessage("no internet conection");
																		});

		NewAccountButton.AddComponent<DefaultCallbackButton>().Init( null,(ht_hud) =>
																			{
																				if (CanLogin)
																				{
																					controller.SendMessage ("NewAccount", SendMessageOptions.DontRequireReceiver );
																					internetChecker.enabled = false;
																				}
																				else ShowErrorMessage("no internet conection");
																			});
	}

	public void FBShareRTS ()
	{
		FB.AppRequest (message:"3D Real-Time Strategy game for mobile!",	title:"Join me in RTS - Rex Tribal Society!");
	}
	
	public bool Yupy ()
	{
		FB.AppRequest( message:"3D Real-Time Strategy game for mobile!", title:"Join me in RTS - Rex Tribal Society!");
		errorMessage.enabled = true;
		errorMessage.text = "Facebook Authorized";
		Invoke ("CloseErrorMessage", 5.0f);
	    Login login	= transform.parent.gameObject.GetComponent<Login>();
		if(PlayerPrefs.GetString("ReUser") == null)  login.NewAccount();
		return true;
	}

	public void ShowErrorMessage (string errorText)
	{
		errorMessage.enabled = true;
		errorMessage.text = errorText;
		Invoke ("CloseErrorMessage", 2.0f);
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
