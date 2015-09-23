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
	public bool hasInternet;
	public GameObject mpassMarket;

	public void Init ()
	{
		InternetChecker internetChecker = GetComponent<InternetChecker>();
		internetChecker.enabled = true;
		LoadRePrefs();
		errorMessage.enabled = false;
		Login login = ComponentGetter.Get<Login>();
		OfflineButton.GetComponent<DefaultCallbackButton>().Init(null,(ht_hud) =>{login.EnterOfflineMode();});
						
		SubmitButton.GetComponent<DefaultCallbackButton>().Init (null, (ht_hud) =>{
																	if (!ConfigurationData.multiPass) MultiPassBtn();
																	else{
																			//TODO lógica de login do jogo
																			if (string.IsNullOrEmpty(username.value) ||
																				string.IsNullOrEmpty(password.value))
																				return;
																			if (hasInternet)
																			{
																				Hashtable ht = new Hashtable ();
																				ht["username"] = username.value;
																				ht["password"] = password.value;																							
																				internetChecker.enabled = false;
																				controller.SendMessage ("DoLogin", ht, SendMessageOptions.DontRequireReceiver );
																			}
																			else ShowErrorMessage("check internet");
																		}
																	});

		NewAccountButton.GetComponent<DefaultCallbackButton>().Init( null,(ht_hud) =>{
																				if (!ConfigurationData.multiPass) MultiPassBtn();
																				else{
																						if (hasInternet){
																							controller.SendMessage ("NewAccount", SendMessageOptions.DontRequireReceiver );
																							internetChecker.enabled = false;
																						}
																						else ShowErrorMessage("no internet conection");
																					}
		
		});
	}
	public void ClosemPassMarket()
	{
		mpassMarket.SetActive(false);
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
