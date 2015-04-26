using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Soomla;
using Visiorama;

namespace Soomla.Store.RTSStoreAssets
{
	public class StoreManager : MonoBehaviour
	{		
		private static StoreManager instance = null;
		private bool checkAffordable = false;	
		private Dictionary<string, bool> itemsAffordability;

		private bool receivedBonus = false; 
		private bool wasInitialized = false;

		void Awake()
		{
			if(instance == null)
			{ 	
				instance = this;
				GameObject.DontDestroyOnLoad(this.gameObject);
			} 
			else GameObject.Destroy(this);
		}
	
		void Start()
		{
			StoreEvents.OnSoomlaStoreInitialized += onSoomlaStoreInitialized;
			StoreEvents.OnCurrencyBalanceChanged += onCurrencyBalanceChanged;		
			SoomlaStore.Initialize(new RTSStoreAssets());
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