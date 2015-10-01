using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;
using Visiorama;
using Soomla.Store;


public class MarketMenu : MonoBehaviour
{
	static bool receivedBonus = false; 
	private bool wasInitialized = false;
	private FacebookLoginHandler fh;
	public UIGrid ButtonsGrid;
	public GameObject btnNoads;
	public GameObject btnHundredOri;
	public GameObject btnFiveHOri;
	public GameObject btnThousendOri;

	void AddBonusFacebook(FBResult response)
	{
		if (response != null){
			var responseObject = Json.Deserialize(response.Text) as Dictionary<string, object>;                                                              
			object obj = 0;   

			if (responseObject.TryGetValue ("cancelled", out obj) || response.Error != null){
				Debug.Log("cancelled");                                                                             
			}   
			else{
				StoreManager.GiveOrichalBonus(10);
				receivedBonus = true;
				Score.AddScorePoints (DataScoreEnum.CurrentCrystals, 10);
				ComponentGetter.Get<Score>("$$$_Score").SaveScore();
			}	
		}
	}

	public void Open ()
	{
		DefaultCallbackButton dcb;
		GameObject option = transform.FindChild ("Menu").transform.FindChild ("Button (Facebook)").gameObject;		
		dcb = option.GetComponent<DefaultCallbackButton> ();

		if (!receivedBonus){
			dcb.Init (null,(ht_dcb) =>{
				if (FB.IsLoggedIn){					
					FB.Feed(
						link: "https://play.google.com/store/apps/details?id=com.Visiorama.RTS",
						linkName: "Join Rex Tribal Society!",
						linkCaption: " 'Gruuuarhhh!!!, can you SAY the word of our salvation? '",
						linkDescription: " Join RTS, Alpha testing with free gameplay and coins!", 
						picture: "https://www.visiorama.com.br/uploads/RTS/mkimages/Achiv10.png",
						callback: AddBonusFacebook);				
				}
				else FB.Login ("email, publish_actions");												
			});
		}
		if(StoreInventory.GetItemBalance("no_ads")>1){
			Destroy(btnNoads);
			ButtonsGrid.Reposition();
			Open();
		}
		if (btnNoads != null){
			dcb = btnNoads.GetComponent<DefaultCallbackButton>();
			dcb.Init(null,(ht_dcb) =>{StoreManager.NoAdsPurchase();});
		}
		if (btnHundredOri != null){
			dcb = btnHundredOri.GetComponent<DefaultCallbackButton>();
			dcb.Init(null,(ht_dcb) =>{StoreManager.OrichalPurchase("100");});
		}
		if (btnFiveHOri != null){
			dcb = btnFiveHOri.GetComponent<DefaultCallbackButton>();
			dcb.Init(null,(ht_dcb) =>{StoreManager.OrichalPurchase("500");});
		}
		if (btnThousendOri != null){
			dcb = btnThousendOri.GetComponent<DefaultCallbackButton>();
			dcb.Init(null,(ht_dcb) =>{StoreManager.OrichalPurchase("1000");});
		}
	}
}