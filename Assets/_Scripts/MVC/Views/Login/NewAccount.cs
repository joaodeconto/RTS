using UnityEngine;
using System.Collections;
using Visiorama;
using Soomla.Profile;
using Soomla.Store;
using System.Linq;

public class NewAccount : IView
{
	public UIInput username;
	public UIToggle AcceptedTerms;
	public GameObject wUser;
	public GameObject wTerm;
	public GameObject CreateAccountButton;
	public GameObject AvatarButton;
	public GameObject AvatarMenu;
	public GameObject FacebookButton;	
	public GameObject TwitterButton;
	public GameObject GoogleButton;
	public bool AccountAlreadyExists;
	private bool providerLogged = false;
	public GameObject loginWithProvider;
	private UserProfile userProfile;

	public NewAccount Init ()
	{
		if (!PlayerPrefs.HasKey("Avatar")){
			PlayerPrefs.SetString("Avatar", "AVA_Gnarl");
		}

		AvatarButton.AddComponent<DefaultCallbackButton>().Init (null,(ht_dcb) =>{
			AvatarMenu.SetActive (true);
		});

		FacebookButton.AddComponent<DefaultCallbackButton>().Init(null, (ht_hud) =>{
			SoomlaProfile.Login(Provider.FACEBOOK);
		});	
		GoogleButton.AddComponent<DefaultCallbackButton>().Init(null,(ht_hud) =>{
			SoomlaProfile.Login(Provider.GOOGLE);
		});	
		TwitterButton.AddComponent<DefaultCallbackButton>().Init(null,(ht_hud) =>{
			SoomlaProfile.Login(Provider.TWITTER);
		});	
	
		CreateAccountButton.AddComponent<DefaultCallbackButton> ().Init (null,(ht_dcb) =>{
			if (AccountAlreadyExists){
				wUser.GetComponent<UILabel>().text = "Username Already in Use";
				wUser.SetActive (true);
				Invoke ("CloseErrorMessage", 3.0f);
				//Debug.Log("AccountAlreadyExists");
				return;
			}						

			else if (username.value.Length > 11 || username.value.Length < 4 || !username.value.All(char.IsLetterOrDigit)){
				wUser.GetComponent<UILabel>().text = "Invalid Username";
				wUser.SetActive (true);
				Invoke ("CloseErrorMessage", 3.0f);
				//Debug.Log("invalid username");
			}

			else if (AcceptedTerms.value != true){
				wTerm.SetActive (true);
				Invoke ("CloseErrorMessage", 3.0f);
				//Debug.Log("Must Accept Terms");
				return;
			}

			else{DoNewAccount();}		
		});

		ProfileEvents.OnLoginFinished += (UserProfile userP, string payload) => 
		{
			if (SoomlaProfile.IsLoggedIn(Provider.FACEBOOK)){
				userProfile = userP;
				Debug.Log ("logou no facebook");	
				Debug.Log (userProfile.ProfileId);
				//SoomlaProfile.UpdateStatus(Provider.FACEBOOK," Join me in RTS - Rex Tribal Society! #RexTribals","");
			}
			else if (SoomlaProfile.IsLoggedIn(Provider.TWITTER)){
				userProfile = userP;
				Debug.Log ("logou no twitter");
				//SoomlaProfile.UpdateStatus(Provider.TWITTER," Join me in RTS - Rex Tribal Society! #RexTribals","");
			}
			else if (SoomlaProfile.IsLoggedIn(Provider.GOOGLE)){
				userProfile = userP;
				Debug.Log ("logou no google");
				//SoomlaProfile.UpdateStatus(Provider.GOOGLE," Join me in RTS - Rex Tribal Society! #RexTribals","");
			}
			
			CreateAccountButton.SetActive(true);
			loginWithProvider.SetActive(false);

			Debug.Log("Logged into "+userProfile.Provider);	
		};

		return this;
	}


	public void DoNewAccount ()
	{
		Hashtable ht = new Hashtable ();
		ht["username"]      = username.value;
		ht["password"]      = userProfile.ProfileId;
		ht["providerId"]    = userProfile.ProfileId;
		ht["email"]         = userProfile.Email;

		string user 		= (string)ht["username"];
		string pass		 	= (string)ht["password"];
		string idFacebook 	= (string)ht["providerId"];
		string mail    		= (string)ht["email"];

		AccountAlreadyExists = false;		
		PlayerDAO playerDao = ComponentGetter.Get<PlayerDAO>();				
		playerDao.CreatePlayer (user, pass, idFacebook, mail, (player, message) =>{
			if (player == null){
				AccountAlreadyExists = true;
				Init();
			}
			else{
				Debug.Log ("Novo DB.Player");
				PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
				pw.SetPlayer (user, true);
				pw.SetPropertyOnPlayer ("player", player.ToString ());
			    Login login = ComponentGetter.Get<Login>();
				login.DoLogin(ht);
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
		wTerm.SetActive(false);
		AccountAlreadyExists = false;
	}

	public void onLoginFinished(UserProfile userProfileJson, bool autoLogin, string payload) {

			
	}

	public bool Yupy ()
	{
		FB.AppRequest( message:"3D Real-Time Strategy for mobile!", title:"Join me in RTS - Rex Tribal Society!");
		return true;
	}
}
