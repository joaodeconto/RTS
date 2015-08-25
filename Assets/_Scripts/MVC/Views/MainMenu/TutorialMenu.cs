using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Visiorama;
using Soomla.Store;
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
	DefaultCallbackButton dcb;
	
	protected PhotonWrapper pw;
	protected VersusScreen vs;
	protected Login login;

	public void Start ()
	{
		Open ();
		pw = ComponentGetter.Get<PhotonWrapper>();
		vs = ComponentGetter.Get<VersusScreen>();
		login = ComponentGetter.Get<Login>();
	}
	
	public void Open ()
	{
		messageActiveGame.gameObject.SetActive (false);
		
		errorMessage.gameObject.SetActive (false);

		if (buttons.Btn0)
		{
//			VirtualCategory vc = StoreInfo.Categories[1];
//			foreach(string s in vc.GoodItemIds)
//			{
//				StoreInventory.TakeItem(s,1);
//			}
			StoreInventory.GiveItem("11", 1);

			LevelBtnsStatus(buttons.Btn0, "1");
		}
		
		if (buttons.Btn1)
		{
			LevelBtnsStatus(buttons.Btn1, "2");
		}
		
		if (buttons.Btn2)
		{
			LevelBtnsStatus(buttons.Btn2,"3");
		}
		
		if (buttons.Btn3)
		{
			LevelBtnsStatus(buttons.Btn3, "4");
		}

		if (buttons.Btn4)
		{
			LevelBtnsStatus(buttons.Btn4, "5");
		}

		if (buttons.Btn5)
		{
			LevelBtnsStatus(buttons.Btn5, "6");
		}

		if (buttons.Btn6)
		{
			LevelBtnsStatus(buttons.Btn6, "7");
		}

		if (buttons.Btn7)
		{
			LevelBtnsStatus(buttons.Btn7, "8");
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

	void LevelBtnsStatus(Transform missions, string sceneString)
	{
		int scene = Int32.Parse(sceneString); 	
		missions = missions.FindChild("Missions");
		foreach (Transform t in missions)
		{
			int level = Int32.Parse(t.name);
			Transform m1 = t.FindChild("sprt_open");
			Transform m2 = t.FindChild("sprt_done");				
			Transform m3 = t.FindChild("sprt_closed");			
	
			dcb = t.gameObject.GetComponent<DefaultCallbackButton>();
			if(StoreInventory.GetItemBalance(scene.ToString() + level.ToString())>0)
			{  
				m1.gameObject.SetActive(true);
				m2.gameObject.SetActive(false);						
				m3.gameObject.SetActive(false);
				dcb.Init ( null, (ht_hud) =>
				{ 
					vs.InitOfflineGame (1, 0,"Tutorial", scene, level);
					if(level == 3)	StoreInventory.GiveItem((scene+1).ToString() + "1", 1);					
						else StoreInventory.GiveItem(scene.ToString() + (level+1).ToString(), 1);
					if (ConfigurationData.Logged) Close ();																				
						else CloseOffline(); 
				});
			}
			else 
			{
				m1.gameObject.SetActive(false);
				m2.gameObject.SetActive(false);
				dcb.Init ( null, (ht_hud) => {StoreInventory.BuyItem(scene.ToString() + level.ToString()); Open ();});
			}
		}
	}
	
	public void CloseOffline ()
	{
		OfflineMenu om = ComponentGetter.Get<OfflineMenu>();
		om.goMainMenu.SetActive(false);
	}
}
