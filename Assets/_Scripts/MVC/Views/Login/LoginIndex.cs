using UnityEngine;
using System.Collections;
using Visiorama;
using Soomla.Profile;

public class LoginIndex : IView
{				
	public UIInput username;
	public UIInput password;
	public UILabel errorMessage;
	public GameObject FacebookButton;
	public GameObject SubmitButton;
	public bool hasInternet{get{return ConfigurationData.connected;}} 
	public GameObject mpassMarket;

	public void Init ()
	{
//		InternetChecker internetChecker = GetComponent<InternetChecker>();
//		internetChecker.enabled = true;
		LoadRePrefs();
		errorMessage.enabled = false;
		Login login = ComponentGetter.Get<Login>();

		if(!hasInternet){
			ShowErrorMessage("No Internet connection - Offline Mode");
			SubmitButton.GetComponent<DefaultCallbackButton>().Init(null,(ht_hud) =>{
				login.EnterOfflineMode();
			});
		}
		else if(PlayerPrefs.GetString("ReUser") == ""){
			SubmitButton.GetComponent<DefaultCallbackButton>().Init( null,(ht_hud) =>{
				if (!ConfigurationData.multiPass) MultiPassBtn();
				else{
					controller.SendMessage ("NewAccount", SendMessageOptions.DontRequireReceiver );
				}
			});
		}
		else{						
			SubmitButton.GetComponent<DefaultCallbackButton>().Init (null, (ht_hud) =>{
				if (!ConfigurationData.multiPass) MultiPassBtn();
				else{
					Hashtable ht = new Hashtable ();
					ht["username"] = PlayerPrefs.GetString("ReUser");
					ht["password"] = PlayerPrefs.GetString("RePassword");	
					controller.SendMessage ("DoLogin", ht, SendMessageOptions.DontRequireReceiver );
				}
			});	
		}
	}
	public void ClosemPassMarket()
	{
		mpassMarket.SetActive(false);
	}

	public void ShowErrorMessage (string errorText)
	{
		errorMessage.enabled = true;
		errorMessage.text = errorText;
		//Invoke ("CloseErrorMessage", 4.0f);
	}

	private void CloseErrorMessage ()
	{
		errorMessage.enabled = false;
	}

	void MultiPassBtn()
	{
		mpassMarket.SetActive(true);
		StoreManager sm = ComponentGetter.Get<StoreManager>();
		GameObject mpassBtn = mpassMarket.transform.FindChild("Button MultiPass").gameObject;
		mpassBtn.GetComponent<DefaultCallbackButton>().Init( null,(ht_hud) => {sm.MultiPlayerPassPurchase(); ClosemPassMarket();});
	}

	public void LoadRePrefs ()
	{
		username.value = PlayerPrefs.GetString("ReUser");
		password.value = PlayerPrefs.GetString("RePassword");
	}
}
