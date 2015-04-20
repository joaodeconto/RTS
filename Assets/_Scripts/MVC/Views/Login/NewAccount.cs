using UnityEngine;
using System.Collections;
using Visiorama;

public class NewAccount : IView
{
	public UIInput username;
	public UIInput email;
	public UIInput password;
	public UIInput password_confirmation;
	public UIToggle AcceptedTerms;
	public UIToggle ReceiveNewsletter;
	public GameObject wUser;
	public GameObject wEmail;
	public GameObject wPass;
	public GameObject wTerm;

	public GameObject VerifyAccountNameButton;
	public GameObject CreateAccountButton;
	public GameObject AvatarButton;
	public GameObject AvatarMenu;
	public int NumberOfCoinsNewPlayerStartsWith = 500;

	public bool AccountAlreadyExists;
	private FacebookLoginHandler fh;


	public NewAccount Init ()
	{
		if (!PlayerPrefs.HasKey("Avatar"))
		{
			PlayerPrefs.SetString("Avatar", "AVA_Gnarl");
		}

		Login login = ComponentGetter.Get<Login>();
		GameObject goFacebookHandler;	
		goFacebookHandler = new GameObject ("FacebookLoginHandler");
		goFacebookHandler.transform.parent = this.transform;		
		fh = goFacebookHandler.AddComponent <FacebookLoginHandler> ();
		fh.OnLoggedIn = Yupy;

		AvatarButton
			.AddComponent<DefaultCallbackButton> ()
				.Init (null,
				       (ht_dcb) =>
				       {
							AvatarMenu.SetActive (true);

						});
	


		CreateAccountButton
			.AddComponent<DefaultCallbackButton> ()
			.Init (null,
			(ht_dcb) =>
			{
					if (AccountAlreadyExists)
					{
						wUser.SetActive (true);
							Invoke ("CloseErrorMessage", 5.0f);
						Debug.Log("AccountAlreadyExists");

						return;
					}

					if (string.IsNullOrEmpty (password.value))
					{
						Debug.Log ("empty password");
						wPass.SetActive (true);
						Invoke ("CloseErrorMessage", 5.0f);
					}
				
					if (string.IsNullOrEmpty (password_confirmation.value) || password.value != password_confirmation.value)
					{
						Debug.Log("you need confirm password ");
						wPass.SetActive (true);
						Invoke ("CloseErrorMessage", 5.0f);
					}

					if (string.IsNullOrEmpty (username.value))
					{
						wUser.SetActive (true);
						Invoke ("CloseErrorMessage", 5.0f);
						Debug.Log("invalid username");
					}

					if (string.IsNullOrEmpty(email.value))
					{
						wEmail.SetActive (true);
						Invoke ("CloseErrorMessage", 5.0f);
						Debug.Log("invalid email");
						return;
					}

					if (AcceptedTerms.value != true)
					{
						wTerm.SetActive (true);
						Invoke ("CloseErrorMessage", 5.0f);
						Debug.Log("Must Accept Terms");
						return;
					}

					else 
					{								
						Hashtable ht = new Hashtable ();
						ht["username"]              = username.value;
						ht["password"]              = password.value;
						ht["email"]                 = email.value;
						DoNewAccount(ht);
						fh.DoLogin ();
						login.DoLogin(ht);
					}
				
			});

//		VerifyAccountNameButton
//			.AddComponent<DefaultCallbackButton> ()
//			.Init (null,
//			(ht_dcb) =>
//			{
//							
//					Debug.Log("AccountAlreadyExists");//TODO mostrar que conta já existe
//			});

		return this;
	}


	public void DoNewAccount (Hashtable ht)
	{
		string username 	= (string)ht["username"];
		string password 	= (string)ht["password"];
		string idFacebook 	= "";
		string email    	= (string)ht["email"];

		AccountAlreadyExists = false;
		
		PlayerDAO playerDao = ComponentGetter.Get<PlayerDAO>();
				
		playerDao.CreatePlayer (username, password, idFacebook, email, 
		                        (player, message) =>
		                        {
									if (player == null)
									{
										AccountAlreadyExists = true;
									}
									else
									{
										Debug.Log ("Novo DB.Player");
										PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
										pw.SetPlayer (username, true);
										pw.SetPropertyOnPlayer ("player", player.ToString ());
														
									}
								});
	}

//	public void ShowErrorMessage ()
//	{
//		errorMessage.enabled = true;
//		errorMessage.text = "Incorrect User or Password";
//		Invoke ("CloseErrorMessage", 5.0f);
//	}
	
	private void CloseErrorMessage ()
	{
		wUser.SetActive(false);
		wPass.SetActive(false);
		wEmail.SetActive(false);
		wTerm.SetActive(false);
	}

	public bool Yupy ()
	{
		FB.AppRequest( message:"3D Real-Time Strategy game for mobile!", title:"Join me in RTS - Rex Tribal Society!");
		return true;
	}
}
