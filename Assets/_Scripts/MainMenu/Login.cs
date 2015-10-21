using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;

using Visiorama;

public class Login : IController
{
	public bool UseRealLogin = true;
	protected PhotonWrapper pw;

	public void Start ()
	{
		Init ();
	}
	
	public void Init ()
	{
		if (ConfigurationData.Logged) return;

		#if UNITY_IOS
		if (Advertisement.isSupported) {
			Advertisement.Initialize("35534", false);
		}
		#else
		if (Advertisement.isSupported) {
			Advertisement.Initialize("18990", false);
		}
		#endif
		pw = ComponentGetter.Get<PhotonWrapper>();
		pw.Init();		
		CheckAllViews ();		
		Index ();				
	}
	
	public void Index ()
	{
		HideAllViews ();		
		LoginIndex index = GetView <LoginIndex> ("Index");
		index.SetActive (true);
		index.Init ();
	}

	public void HiddeViews ()
	{
		HideAllViews ();
	}
	
	public void NewAccount ()
	{
		HideAllViews ();		
		NewAccount newAccount = GetView <NewAccount> ("NewAccount");
		newAccount.SetActive (true);
		newAccount.Init ();
	}

	public void EnterOfflineMode ()
	{
		HideAllViews ();		
		ConfigurationData.Offline = true;
		OfflineMenu offlineMenu = ComponentGetter.Get <OfflineMenu> ();
		offlineMenu.Init();
	}

	
	public void DoLogin (Hashtable ht)
	{
		string username = (string)ht["username"];
		string password = (string)ht["password"];
		string idFacebook = (string)ht["providerId"];
		
		if (!UseRealLogin)	{
			ConfigurationData.player = new Model.Player () { 
			IdPlayer = 5,
			SzName			  = username,
			SzPassword		  = password,
			IdFacebookAccount = idFacebook
			};			
			pw.SetPlayer (username, true);
			pw.SetPropertyOnPlayer ("player", ConfigurationData.player.ToString ());
			pw.SetPropertyOnPlayer ("avatar", PlayerPrefs.GetString("Avatar"));
			EnterInternalMainMenu (username);
		}
		else  {
			PlayerDAO playerDao = ComponentGetter.Get<PlayerDAO>();			
			playerDao.GetPlayer (username, password, idFacebook,(player, message) =>{
				ConfigurationData.player = player;	
				if (player == null)	{
					LoginIndex index = GetView <LoginIndex> ("Index");
					Debug.Log("Incorrect User or Password");
				}
				else{
					pw.SetPlayer (username, true);
					pw.SetPropertyOnPlayer ("player", player.ToString ());
					EnterInternalMainMenu (username);
				}
			});
		}
	}
	
	public void EnterInternalMainMenu (string username)
	{
		ConfigurationData.Offline = false;
		ConfigurationData.Logged = true;		
		HideAllViews ();		
		InternalMainMenu imm = ComponentGetter.Get <InternalMainMenu> ();
		imm.Init ();
	}
}
