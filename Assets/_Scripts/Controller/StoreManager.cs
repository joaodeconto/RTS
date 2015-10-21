
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Soomla;
using Soomla.Profile;
using Soomla.Store;
//using Soomla.Levelup;
//using Soomla.Highway;
using Visiorama;


public class StoreManager : MonoBehaviour
{		
	private static StoreManager instance = null;
	private bool checkAffordable = false;	
	private Dictionary<string, bool> itemsAffordability;
	private static bool wasInitialized = false;

	void Awake()
	{
		if(instance == null){ 	
			instance = this;
			GameObject.DontDestroyOnLoad(this.gameObject);
		} 
		else GameObject.Destroy(this);
	}

	void Start()
	{
		if(!wasInitialized)
		{
			StoreEvents.OnSoomlaStoreInitialized += onSoomlaStoreInitialized;
			StoreEvents.OnCurrencyBalanceChanged += onCurrencyBalanceChanged;	
			StoreEvents.OnGoodBalanceChanged 	 += onGoodBalanceChanged;	
//			SoomlaHighway.Initialize();
//			SoomlaLevelUp.Initialize();
			SoomlaStore.Initialize(new RTSStoreAssets());		
			SoomlaProfile.Initialize();
			StoreInventory.GiveItem("pass_multiplayer", 1);		//dando multiplayerpass	
			
			StoreInventory.GiveItem("11", 1);

//			StoreInventory.TakeItem("no_ads", 1);
			wasInitialized = true;
		}
	}	

	public void ToggleAdsPass(bool passAdded)
	{
		if(passAdded)
		{
			ConfigurationData.multiPass = true ;
			ConfigurationData.addPass = true ;
		}
		else
		{
			ConfigurationData.multiPass = false ;
			ConfigurationData.addPass = false ;
		}
	}

	public void onSoomlaStoreInitialized()
	{
		if (StoreInfo.Currencies.Count>0) {
			try {
				StoreInventory.GiveItem(StoreInfo.Currencies[0].ItemId,1);
				SoomlaUtils.LogDebug("SOOMLA ExampleEventHandler", "Currency balance:" + StoreInventory.GetItemBalance(StoreInfo.Currencies[0].ItemId));
			} catch (VirtualItemNotFoundException ex){
				SoomlaUtils.LogError("SOOMLA ExampleEventHandler", ex.Message);
			}
		}
		setupItemsAffordability ();
		int mPass = StoreInventory.GetItemBalance("pass_multiplayer");
		if(mPass >=1) ConfigurationData.multiPass = true;
		
		int nAdd = StoreInventory.GetItemBalance("no_ads");
		if(nAdd >=1) ConfigurationData.addPass = true;
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
//		if(ConfigurationData.Logged) 
//		{
//			Score.AddScorePoints (DataScoreEnum.CurrentCrystals, amountAdded);
//			ComponentGetter.Get<Score>("$$$_Score").SaveScore();				
//			if(!ConfigurationData.InGame)ComponentGetter.Get<InternalMainMenu>().InitScore ();
//		}
//		if(!ConfigurationData.InGame && ConfigurationData.Offline) ComponentGetter.Get<OfflineMenu>().InitScore ();
	}
			
	public void onGoodBalanceChanged(VirtualGood good, int balance, int amountAdded)
	{
		if(good.ItemId == "pass_multiplayer")
		{
			int mPass = StoreInventory.GetItemBalance("pass_multiplayer");
			if(mPass >=1) ConfigurationData.multiPass = true;
		}

		else if(good.ItemId == "no_ads")
		{
			int nAdd = StoreInventory.GetItemBalance("no_ads");
			if(nAdd >=1)	ConfigurationData.addPass = true;
			if(ConfigurationData.Offline)	ComponentGetter.Get<OfflineMenu>().noAddBtn.SetActive(false);
		}
	}

	public void MultiPlayerPassPurchase()
	{
		try
		{
			StoreInventory.BuyItem("pass_multiplayer");
		}
		
		catch (Exception e)
		{
			Debug.Log ("SOOMLA/UNITY " + e.Message);
		}
	}

	static public void NoAdsPurchase()
	{
		try
		{
			StoreInventory.BuyItem("no_ads");
		}
		
		catch (Exception e)
		{
			Debug.Log ("SOOMLA/UNITY " + e.Message);
		}
	}

	static public void GiveOrichalBonus(int bonusQuant)
	{
		try
		{
			StoreInventory.GiveItem("currency_orichal", bonusQuant);
		}
		
		catch (Exception e)
		{
			Debug.Log ("SOOMLA/UNITY " + e.Message);
		}

	}

	public int GetBalance {get{return StoreInventory.GetItemBalance(StoreInfo.Currencies[0].ItemId);}}

	
	static public void OrichalPurchase (string orichalQuant) 
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


