using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;
using Visiorama;
using Soomla.Store.RTSStoreAssets;


public class MarketMenu : MonoBehaviour
{

	private bool receivedBonus = false; 
	private bool wasInitialized = false;
	private FacebookLoginHandler fh;
	public UIGrid ButtonsGrid;

	void OnEnable()
	{		
		DefaultCallbackButton defaultCallbackButton;		
		GameObject option = transform.FindChild ("Menu").transform.FindChild ("Button (Facebook)").gameObject;
		GameObject goFacebookHandler;	
		goFacebookHandler = new GameObject ("FacebookLoginHandler");
		goFacebookHandler.transform.parent = this.transform;		
		fh = goFacebookHandler.AddComponent <FacebookLoginHandler> ();		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		if (!receivedBonus)
		{
			defaultCallbackButton.Init (null,
			                            (ht_dcb) =>
			                            {
												if (FB.IsLoggedIn)
												{					
													FB.Feed(
														link: "https://play.google.com/store/apps/details?id=com.Visiorama.RTS",
														linkName: "Join Rex Tribal Society!",
														linkCaption: " 'Gruuuarhhh!!!, can you SAY the word of our salvation? '",
														linkDescription: " Join RTS, Alpha testing with free gameplay and coins!", 
														picture: "https://www.visiorama.com.br/uploads/RTS/mkimages/Achiv10.png",
														callback: AddBonusFacebook
														
														);				
												}
												else fh.DoLogin ();
												
											});
										}
	}

	void AddBonusFacebook(FBResult response)
	{
		Debug.Log("Result: " + response.Text);
		if (response != null)
		{
			var responseObject = Json.Deserialize(response.Text) as Dictionary<string, object>;                                                              
			object obj = 0;   

			if (responseObject.TryGetValue ("cancelled", out obj) || response.Error != null)                                                                 
			{
				Debug.Log("cancelled");                                                                             
			}   
			else 
			{
				StoreManager sm = ComponentGetter.Get<StoreManager>();
				sm.GiveOrichalBonus(10);
				receivedBonus = true;
				Score.AddScorePoints (DataScoreEnum.CurrentCrystals, 10);
				ComponentGetter.Get<Score>("$$$_Score").SaveScore();
			}	
		}
	}

	public void Open ()
	{
		if (wasInitialized)
			return;
		
		wasInitialized = true;

		DefaultCallbackButton dcb;
		
		Transform close = this.transform.FindChild ("Menu").FindChild ("Resume");
		
		if (close != null)
		{
			dcb = close.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) => 
			         {
				gameObject.SetActive (false);
			});
		}
	}

}