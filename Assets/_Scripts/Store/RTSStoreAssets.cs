/// Copyright (C) 2012-2014 Soomla Inc.
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///      http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.

using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Soomla.Store {

	/// <summary>
	/// This class defines our game's economy, which includes virtual goods, virtual currencies
	/// and currency packs, virtual categories
	/// </summary>
	public class RTSStoreAssets : IStoreAssets{

		/// <summary>
		/// see parent.
		/// </summary>
		public int GetVersion() {
			return 2;
		}

		/// <summary>
		/// see parent.
		/// </summary>
		public VirtualCurrency[] GetCurrencies() {
			return new VirtualCurrency[]{ORICHAL_CURRENCY};
		}

		/// <summary>
		/// see parent.
		/// </summary>
	    public VirtualGood[] GetGoods() {
			return new VirtualGood[] {REX_ZERO_GOOD, MULTIPLAYER_PASS, NO_ADS,
				LEVEL11_KEY,LEVEL12_KEY,LEVEL13_KEY,
				LEVEL21_KEY,LEVEL22_KEY,LEVEL23_KEY,
				LEVEL31_KEY,LEVEL32_KEY,LEVEL33_KEY,
				LEVEL41_KEY,LEVEL42_KEY,LEVEL43_KEY,
				LEVEL51_KEY,LEVEL52_KEY,LEVEL53_KEY,
				LEVEL61_KEY,LEVEL62_KEY,LEVEL63_KEY,
				LEVEL71_KEY,LEVEL72_KEY,LEVEL73_KEY,
				LEVEL81_KEY,LEVEL82_KEY,LEVEL83_KEY,};
		}

		/// <summary>
		/// see parent.
		/// </summary>
	    public VirtualCurrencyPack[] GetCurrencyPacks() {
			return new VirtualCurrencyPack[] {HUNDREDORICHAL_PACK, FIVEHUNDREDORICHAL_PACK, THOUSANDORICHAL_PACK};
		}

		/// <summary>
		/// see parent.
		/// </summary>
	    public VirtualCategory[] GetCategories() {
			return new VirtualCategory[]{UNIT_CATEGORY, LevelKeys};
		}

	    /** Static Final Members **/

		public const string ORICHAL_CURRENCY_ID      = "currency_orichal";
		public const string HUNDREDORICHAL_PACK_PRODUCT_ID      = "orichal";
		public const string FIVEHUNDREDORICHAL_PACK_PRODUCT_ID    = "orichal1";
		public const string THOUSANDORICHAL_PACK_PRODUCT_ID = "orichal2";	   
		public const string REX_ZERO_GOOD_ITEM_ID   = "rex_zero";	    
		public const string NO_ADS_LIFETIME_PRODUCT_ID = "no_ads";
		public const string MULTIPLAYER_PASS_LIFETIME_PRODUCT_ID = "pass_multiplayer";


	    /** Virtual Currencies **/

		public static VirtualCurrency ORICHAL_CURRENCY = new VirtualCurrency(
	            "Orichals",											// name
				"Orichals",											// description
				ORICHAL_CURRENCY_ID							// item id
	    );


	    /** Virtual Currency Packs **/

		public static VirtualCurrencyPack HUNDREDORICHAL_PACK = new VirtualCurrencyPack(
				"100 Orichals",                                   // name
				"100 Orichal Pack",                       // description
	            "orichal",                                   // item id
				100,												// number of currencies in the pack
				ORICHAL_CURRENCY_ID,                        // the currency associated with this pack
				new PurchaseWithMarket(HUNDREDORICHAL_PACK_PRODUCT_ID, 0.99)
		);

		public static VirtualCurrencyPack FIVEHUNDREDORICHAL_PACK = new VirtualCurrencyPack(
				"500 Orichals",                                   // name
	            "",                 // description
				"orichal1",                                   // item id
				500,                                             // number of currencies in the pack
				ORICHAL_CURRENCY_ID,                        // the currency associated with this pack
			new PurchaseWithMarket(FIVEHUNDREDORICHAL_PACK_PRODUCT_ID, 4.99)
		);

		public static VirtualCurrencyPack THOUSANDORICHAL_PACK = new VirtualCurrencyPack(
	            "1000 Orichals",                                  // name
	            "",                 	// description
				"orichal2",                                  // item id
				1000,                                            // number of currencies in the pack
				ORICHAL_CURRENCY_ID,                        // the currency associated with this pack
			new PurchaseWithMarket(THOUSANDORICHAL_PACK_PRODUCT_ID, 9.99));		


		    /** Virtual Goods **/

		public static VirtualGood MULTIPLAYER_PASS = new LifetimeVG(
	            "Multiplayer Pass",                                       		// name
	            "Whole new gameplay, with multiplayer, ranks and bets", // description
				"pass_multiplayer",                                       		// item id
			new PurchaseWithMarket(MULTIPLAYER_PASS_LIFETIME_PRODUCT_ID, 3.99)); // the way this virtual good is purchased

		public static VirtualGood NO_ADS = new LifetimeVG(
				"No Ads", 														// name
				"No More Ads!",				 									// description
				"no_ads",														// item id
			new PurchaseWithMarket(NO_ADS_LIFETIME_PRODUCT_ID, 0.99));	// the way this virtual good is purchased
	
		public static VirtualGood REX_ZERO_GOOD = new LifetimeVG(
	            "T-Rex",                                         			// name
	            "The most awesome dinosaur", // description
	            "rex_zero",                                          		// item id
			new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID, 200)); // the way this virtual good is purchased

	    /** Virtual Categories **/
	    // The muffin rush theme doesn't support categories, so we just put everything under a general category.
	    public static VirtualCategory UNIT_CATEGORY = new VirtualCategory(
			"Units", new List<string>(new string[] { REX_ZERO_GOOD_ITEM_ID })
	    );

		public static VirtualGood LEVEL11_KEY = new LifetimeVG("11","KEY TO LEVEL 1-1", "11", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,100));
		public static VirtualGood LEVEL12_KEY = new LifetimeVG("12","KEY TO LEVEL 1-2", "12", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,130));
		public static VirtualGood LEVEL13_KEY = new LifetimeVG("13","KEY TO LEVEL 1-3", "13", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,160));
		public static VirtualGood LEVEL21_KEY = new LifetimeVG("21","KEY TO LEVEL 2-1", "21", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,200));
		public static VirtualGood LEVEL22_KEY = new LifetimeVG("22","KEY TO LEVEL 2-2", "22", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,230));
		public static VirtualGood LEVEL23_KEY = new LifetimeVG("23","KEY TO LEVEL 2-3", "23", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,260));
		public static VirtualGood LEVEL31_KEY = new LifetimeVG("31","KEY TO LEVEL 3-1", "31", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,300));
		public static VirtualGood LEVEL32_KEY = new LifetimeVG("32","KEY TO LEVEL 3-2", "32", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,330));
		public static VirtualGood LEVEL33_KEY = new LifetimeVG("33","KEY TO LEVEL 3-3", "33", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,360));
		public static VirtualGood LEVEL41_KEY = new LifetimeVG("41","KEY TO LEVEL 4-1", "41", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,400));
		public static VirtualGood LEVEL42_KEY = new LifetimeVG("42","KEY TO LEVEL 4-2", "42", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,430));
		public static VirtualGood LEVEL43_KEY = new LifetimeVG("43","KEY TO LEVEL 4-3", "43", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,460));
		public static VirtualGood LEVEL51_KEY = new LifetimeVG("51","KEY TO LEVEL 5-1", "51", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,500));
		public static VirtualGood LEVEL52_KEY = new LifetimeVG("52","KEY TO LEVEL 5-2", "52", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,530));
		public static VirtualGood LEVEL53_KEY = new LifetimeVG("53","KEY TO LEVEL 5-3", "53", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,560));
		public static VirtualGood LEVEL61_KEY = new LifetimeVG("61","KEY TO LEVEL 6-1", "61", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,600));
		public static VirtualGood LEVEL62_KEY = new LifetimeVG("62","KEY TO LEVEL 6-2", "62", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,630));
		public static VirtualGood LEVEL63_KEY = new LifetimeVG("63","KEY TO LEVEL 6-3", "63", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,660));
		public static VirtualGood LEVEL71_KEY = new LifetimeVG("71","KEY TO LEVEL 7-1", "71", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,700));
		public static VirtualGood LEVEL72_KEY = new LifetimeVG("72","KEY TO LEVEL 7-2", "72", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,730));
		public static VirtualGood LEVEL73_KEY = new LifetimeVG("73","KEY TO LEVEL 7-3", "73", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,760));
		public static VirtualGood LEVEL81_KEY = new LifetimeVG("81","KEY TO LEVEL 8-1", "81", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,800));
		public static VirtualGood LEVEL82_KEY = new LifetimeVG("82","KEY TO LEVEL 8-2", "82", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,830));
		public static VirtualGood LEVEL83_KEY = new LifetimeVG("83","KEY TO LEVEL 8-3", "83", new PurchaseWithVirtualItem(ORICHAL_CURRENCY_ID,860));

		public static VirtualCategory LevelKeys = new VirtualCategory(
			"LevelKeys", new List<string>(new string[] { "12","13","21","22","23","31","32","33","41","42","43","51","52","53","61","62","63","71","72","73","81","82","83"})
			);		
	}

}
