using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;
using System;
using Soomla;
using Visiorama;

namespace Soomla.Store.RTSStoreAssets {

	public class MarketMenu : MonoBehaviour
	{

		private static MarketMenu instance = null;
		private bool checkAffordable = false;	
		private Dictionary<string, bool> itemsAffordability;

		private bool receivedBonus = false; 
		private bool wasInitialized = false;
		private FacebookLoginHandler fh;

		void Awake()
		{
			if(instance == null)
			{ 	
				instance = this;
				GameObject.DontDestroyOnLoad(this.gameObject);
			}

			else 	GameObject.Destroy(this);
		}

		void Start()
		{
			StoreEvents.OnSoomlaStoreInitialized += onSoomlaStoreInitialized;
			StoreEvents.OnCurrencyBalanceChanged += onCurrencyBalanceChanged;
		
			SoomlaStore.Initialize(new RTSStoreAssets());
			
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
			bool pay = true;
			Debug.Log("Result: " + response.Text);
			if (response != null)
			{
				var responseObject = Json.Deserialize(response.Text) as Dictionary<string, object>;                                                              
				object obj = 0;                                                                                                        
				if (!responseObject.TryGetValue ("cancelled", out obj))                                                                 
				{
					pay = false;
					Debug.Log("cancelled");                                                                             
				}   
				if (pay == true)
				{
					StoreInventory.GiveItem("currency_orichal",10);
					receivedBonus = true;
					Score.AddScorePoints (DataScoreEnum.CurrentCrystals, 10);
					ComponentGetter.Get<Score>("$$$_Score").SaveScore();
					ComponentGetter.Get<InternalMainMenu>().InitScore ();
				}
		
			}
		}

		public void onSoomlaStoreInitialized()
		{
			
			// some usage examples for add/remove currency
			// some examples
			if (StoreInfo.Currencies.Count>0) {
				try {
					StoreInventory.GiveItem(StoreInfo.Currencies[0].ItemId,1);
					SoomlaUtils.LogDebug("SOOMLA ExampleEventHandler", "Currency balance:" + StoreInventory.GetItemBalance(StoreInfo.Currencies[0].ItemId));
				} catch (VirtualItemNotFoundException ex){
					SoomlaUtils.LogError("SOOMLA ExampleEventHandler", ex.Message);
				}
			}

			setupItemsAffordability ();
		}

		public void setupItemsAffordability()
		{
			itemsAffordability = new Dictionary<string, bool> ();
			
			foreach (VirtualGood vg in StoreInfo.Goods) {
				itemsAffordability.Add(vg.ID, StoreInventory.CanAfford(vg.ID));
			}
		}

		public void onCurrencyBalanceChanged(VirtualCurrency virtualCurrency, int balance, int amountAdded) {
			if (itemsAffordability != null)
			{
				List<string> keys = new List<string> (itemsAffordability.Keys);
				foreach(string key in keys)
					itemsAffordability[key] = StoreInventory.CanAfford(key);
			}
			Score.AddScorePoints (DataScoreEnum.CurrentCrystals, amountAdded);
			ComponentGetter.Get<Score>("$$$_Score").SaveScore();
			ComponentGetter.Get<InternalMainMenu>().CurrentCrystalsLabel.text = balance.ToString();
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
		
		public void OrichalPurchase (string orichalQuant) 
		{			
			if (orichalQuant == "100")
			{				
				foreach(VirtualCurrencyPack cp in StoreInfo.CurrencyPacks)
				{			
					if (cp.Name == "100 Orichals")
					{
						Debug.Log("SOOMLA/UNITY Wants to buy: " + cp.Name);
						try
						{
							StoreInventory.BuyItem(cp.ItemId);
						}

						catch (Exception e)
						{
							Debug.Log ("SOOMLA/UNITY " + e.Message);
						}
					}
				}
			}
			if (orichalQuant == "500")
			{				
				foreach(VirtualCurrencyPack cp in StoreInfo.CurrencyPacks)
				{			
					if (cp.Name == "500 Orichals")
					{
						Debug.Log("SOOMLA/UNITY Wants to buy: " + cp.Name);
						try
						{
							StoreInventory.BuyItem(cp.ItemId);
						}
						catch (Exception e)
						{
							Debug.Log ("SOOMLA/UNITY " + e.Message);
						}
					}
				}
			}

			if (orichalQuant == "1000")
			{				
				foreach(VirtualCurrencyPack cp in StoreInfo.CurrencyPacks)
				{			
					if (cp.Name == "1000 Orichals")
					{
						Debug.Log("SOOMLA/UNITY Wants to buy: " + cp.Name);
						try
						{
							StoreInventory.BuyItem(cp.ItemId);
						}
						catch (Exception e)
						{
							Debug.Log ("SOOMLA/UNITY " + e.Message);
						}
					}
				}
			}
		}	
	}			
}