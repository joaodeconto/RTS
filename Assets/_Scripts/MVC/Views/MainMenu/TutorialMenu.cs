using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Visiorama;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class TutorialMenu : MonoBehaviour
{
	[System.Serializable ()]
	public class TutorialMenuButtons
	{
		public Transform Btn0;
		public Transform Btn1;
		public Transform Btn2;
		public Transform Btn3;
		public Transform Btn4;
		public Transform Btn5;
		public Transform Btn6;		
		public Transform Btn7;
		public Transform BtnLeaveRoom;
		
		public IEnumerable<Transform> Iterate
		{
			get
			{
				yield return Btn0;
				yield return Btn1;
				yield return Btn2;
				yield return Btn3;
				yield return Btn4;				
				yield return Btn5;				
				yield return Btn6;
				yield return Btn7;
				yield return BtnLeaveRoom;
			}
		}
	}
	
	public TutorialMenuButtons buttons;	
	public UILabel messageActiveGame;
	public UILabel errorMessage;
	bool wasInitialized      = false;
	
	protected PhotonWrapper pw;
	protected VersusScreen vs;
	protected Login login;
	public void OnEnable ()
	{
		Open ();
	}
	
	public void Open ()
	{

		messageActiveGame.gameObject.SetActive (false);
		
		errorMessage.gameObject.SetActive (false);

		pw = ComponentGetter.Get<PhotonWrapper>();
		vs = ComponentGetter.Get<VersusScreen>();
		login = ComponentGetter.Get<Login>();
		
		DefaultCallbackButton dcb;
		
		if (buttons.Btn0)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn0, false);
			dcb.Init ( null, (ht_hud) => { vs.InitOfflineGame (1, 0,"Tutorial", 1); if (ConfigurationData.Logged) Close ();
																					else CloseOffline();} );
		}
		
		if (buttons.Btn1)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn1, false);
			dcb.Init ( null, (ht_hud) => { vs.InitOfflineGame (1, 0,"Tutorial", 2); if (ConfigurationData.Logged) Close ();
				else CloseOffline();} );
		}
		
		if (buttons.Btn2)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn2, false);
			dcb.Init ( null, (ht_hud) => { vs.InitOfflineGame (1, 0,"Tutorial", 3);if (ConfigurationData.Logged) Close ();
				else CloseOffline();} );
		}
		
		if (buttons.Btn3)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn3, false);
			dcb.Init ( null, (ht_hud) => { vs.InitOfflineGame (1, 0,"Tutorial", 4); if (ConfigurationData.Logged) Close ();
				else CloseOffline();} );
		}

		if (buttons.Btn4)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn4, false);
			dcb.Init ( null, (ht_hud) => { vs.InitOfflineGame (1, 0,"Tutorial", 5); if (ConfigurationData.Logged) Close ();
				else CloseOffline();} );
		}

		if (buttons.Btn5)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn5, false);
			dcb.Init ( null, (ht_hud) => { vs.InitOfflineGame (1, 0,"Tutorial", 6); if (ConfigurationData.Logged) Close ();
				else CloseOffline();} );
		}

		if (buttons.Btn6)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn6, false);
			dcb.Init ( null, (ht_hud) => { vs.InitOfflineGame (1, 0,"Tutorial", 7); if (ConfigurationData.Logged) Close ();
				else CloseOffline();} );
		}

		if (buttons.Btn7)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.Btn7, false);
			dcb.Init ( null, (ht_hud) => { vs.InitOfflineGame (1, 0,"Tutorial", 8); if (ConfigurationData.Logged) Close ();
				else CloseOffline();} );
		}
						
		if (buttons.BtnLeaveRoom)
		{
			dcb = ComponentGetter.Get <DefaultCallbackButton> (buttons.BtnLeaveRoom, false);
			dcb.Init ( null, (ht_hud) => { 
				if(!ConfigurationData.Offline) Close();
				else CloseOffline();} );
		}
	}

	public void Close ()
	{
		if (!ConfigurationData.Offline)
		{
			foreach (GameObject menu in pw.menusDissapearWhenLogged)
			{
				menu.SetActive (true);
			}

			if (buttons == null) return;
					
					foreach (Transform button in buttons.Iterate)
					{
						if (button)
						{
							button.gameObject.SetActive (true);
						}
					}
			
			messageActiveGame.gameObject.SetActive (false);
						
			gameObject.SetActive (false);
		}
	}

	public void CloseOffline ()
	{
		OfflineMenu om = ComponentGetter.Get<OfflineMenu>();
		om.goMainMenu.SetActive(false);
	}
}
