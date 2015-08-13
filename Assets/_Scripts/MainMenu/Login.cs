using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;

using Visiorama;

public class Login : IController
{
	public bool UseRealLogin = true;
	public UIToggle rememberUser;
	protected PhotonWrapper pw;

	
	public void Start ()
	{
		Init ();
	}
	
	public void Init ()
	{
		if (ConfigurationData.Logged) return;
		
		LoadPlayerPrefabs ();

		#if UNITY_IOS
		Advertisement.Initialize("35534", false);
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
		string idFacebook = "";		 
		
		if (!UseRealLogin)
		{
				ConfigurationData.player = new Model.Player () { IdPlayer = 5,
				SzName			  = username,
				SzPassword		  = password,
				IdFacebookAccount = idFacebook };
			
			pw.SetPlayer (username, true);
			pw.SetPropertyOnPlayer ("player", ConfigurationData.player.ToString ());
			pw.SetPropertyOnPlayer ("avatar", PlayerPrefs.GetString("Avatar"));
			EnterInternalMainMenu (username);
		}
		else
		{
			PlayerDAO playerDao = ComponentGetter.Get<PlayerDAO>();
			
			playerDao.GetPlayer (username, password, idFacebook,
			                     (player, message) =>
			                     {
									ConfigurationData.player = player;
									
									Debug.Log ("player: " + player);
									Debug.Log ("name: " + username);
									
									if (player == null)
									{
										LoginIndex index = GetView <LoginIndex> ("Index");
										index.ShowErrorMessage ("Incorrect User or Password");
									}
									else
									{
										if ( rememberUser.value == true)
										{
											PlayerPrefs.SetString("ReUser", username);
											PlayerPrefs.SetString("RePassword", password);
										}

										else
										{
											PlayerPrefs.SetString("ReUser", username);
											PlayerPrefs.SetString("RePassword", null);
										}

										pw.SetPlayer (username, true);
										pw.SetPropertyOnPlayer ("player", player.ToString ());	
										ConfigurationData.Offline = false;
										EnterInternalMainMenu (username);
									}
								});
		}
	}
	
	public void EnterInternalMainMenu (string username)
	{
		ConfigurationData.Logged = true;		
		HideAllViews ();
		
		InternalMainMenu imm = ComponentGetter.Get <InternalMainMenu> ();
		imm.Init ();
	}
	
	//	public void DoNewAccount (Hashtable ht)
	//	{
	//		string username = (string)ht["username"];
	//		string password = (string)ht["password"];
	//		string idFacebook = "";
	//		string email    = (string)ht["email"];
	//		
	//		PlayerDAO playerDao = ComponentGetter.Get<PlayerDAO>();
	//		
	//        playerDao.CreatePlayer (username, password, idFacebook, email,
	//		(player, message) =>
	//		{
	//			if (player == null)
	//			{
	//
	//			}
	//			else
	//			{
	//				Debug.Log ("Novo DB.Player");
	//
	//				PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
	//				
	//				pw.SetPlayer (username, true);
	//                pw.SetPropertyOnPlayer ("player", player.ToString ());
	//
	//
	//
	//				Score.SetScorePoints (DataScoreEnum.TotalCrystals,   NumberOfCoinsNewPlayerStartsWith, -1);
	//				Score.AddScorePoints (DataScoreEnum.CurrentCrystals, NumberOfCoinsNewPlayerStartsWith, -1);
	//				
	//				Index ();
	//			}
	//		});
	//	}
	
	public void LoadPlayerPrefabs ()
	{
		
		if (!PlayerPrefs.HasKey("TouchSense"))
		{
			PlayerPrefs.SetFloat("TouchSense", .5f);
		}
		if (!PlayerPrefs.HasKey("DoubleClickSpeed"))
		{
			PlayerPrefs.SetFloat("DoubleClickSpeed", .5f);
		}		
		if (!PlayerPrefs.HasKey("AllVolume"))
		{
			PlayerPrefs.SetFloat("AllVolume", 1f);
		}
		if (!PlayerPrefs.HasKey("MusicVolume"))
		{
			PlayerPrefs.SetFloat("MusicVolume", 1f);
		}
		if (!PlayerPrefs.HasKey("SFXVolume"))
		{
			PlayerPrefs.SetFloat("SFXVolume", 1f);
		}		
		if (!PlayerPrefs.HasKey("GraphicQuality"))
		{
			QualitySettings.SetQualityLevel(4);
		}

		if (!PlayerPrefs.HasKey("Logins"))
		{
			PlayerPrefs.SetInt("Logins", 0);
		}
		else
		{
			int logins = PlayerPrefs.GetInt("Logins");
			PlayerPrefs.SetInt("Logins", (logins+1));
		}
	
		SoundManager.SetVolume (PlayerPrefs.GetFloat("AllVolume"));
		SoundManager.SetVolumeMusic (PlayerPrefs.GetFloat("MusicVolume"));
		SoundManager.SetVolumeSFX (PlayerPrefs.GetFloat("SFXVolume"));		
		QualitySettings.SetQualityLevel (PlayerPrefs.GetInt("GraphicQuality"));
	}

	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Escape))
		{
			Application.Quit();
		}
	}
}
