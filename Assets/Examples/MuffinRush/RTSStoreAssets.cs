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
using System.Collections;
using System.Collections.Generic;

namespace Soomla.Store.RTSStoreAssets {

	/// <summary>
	/// This class defines our game's economy, which includes virtual goods, virtual currencies
	/// and currency packs, virtual categories
	/// </summary>
	public class RTSStoreAssets : IStoreAssets{

		/// <summary>
		/// see parent.
		/// </summary>
		public int GetVersion() {
			return 0;
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
			return new VirtualGood[] {REX_ZERO_GOOD, MULTIPLAYER_PASS, NO_ADS};
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
			return new VirtualCategory[]{UNIT_CATEGORY};
		}

	    /** Static Final Members **/

		public const string ORICHAL_CURRENCY_ITEM_ID      = "currency_orichal";

		public const string HUNDREDORICHAL_PACK_PRODUCT_ID      = "orichal";

		public const string FIVEHUNDREDORICHAL_PACK_PRODUCT_ID    = "orichal1";

		public const string THOUSANDORICHAL_PACK_PRODUCT_ID = "orichal2";	   

		public const string REX_ZERO_GOOD_ITEM_ID   = "rex_zero";
	    
		public const string NO_ADS_LIFETIME_PRODUCT_ID = "no_ads";

		public const string MULTIPLAYER_PASS_LIFETIME_PRODUCT_ID = "multiplayer_pass";


	    /** Virtual Currencies **/

		public static VirtualCurrency ORICHAL_CURRENCY = new VirtualCurrency(
	            "Orichals",											// name
	            "",													// description
				ORICHAL_CURRENCY_ITEM_ID							// item id
	    );


	    /** Virtual Currency Packs **/

		public static VirtualCurrencyPack HUNDREDORICHAL_PACK = new VirtualCurrencyPack(
				"100 Orichals",                                   // name
	            "",                       // description
	            "orichal",                                   // item id
				100,												// number of currencies in the pack
				ORICHAL_CURRENCY_ITEM_ID,                        // the currency associated with this pack
				new PurchaseWithMarket(HUNDREDORICHAL_PACK_PRODUCT_ID, 0.99)
		);

		public static VirtualCurrencyPack FIVEHUNDREDORICHAL_PACK = new VirtualCurrencyPack(
				"500 Orichals",                                   // name
	            "",                 // description
				"orichal1",                                   // item id
				500,                                             // number of currencies in the pack
				ORICHAL_CURRENCY_ITEM_ID,                        // the currency associated with this pack
			new PurchaseWithMarket(FIVEHUNDREDORICHAL_PACK_PRODUCT_ID, 4.99)
		);

		public static VirtualCurrencyPack THOUSANDORICHAL_PACK = new VirtualCurrencyPack(
	            "1000 Orichals",                                  // name
	            "",                 	// description
				"orichal2",                                  // item id
				1000,                                            // number of currencies in the pack
				ORICHAL_CURRENCY_ITEM_ID,                        // the currency associated with this pack
			new PurchaseWithMarket(THOUSANDORICHAL_PACK_PRODUCT_ID, 9.99));		


		    /** Virtual Goods **/

		public static VirtualGood MULTIPLAYER_PASS = new LifetimeVG(
	            "Multiplayer Pass",                                       		// name
	            "Whole new gameplay, with multiplayer, ranks and bets", // description
	            "multiplayer_pass",                                       		// item id
			new PurchaseWithMarket(MULTIPLAYER_PASS_LIFETIME_PRODUCT_ID, 2.99)); // the way this virtual good is purchased

		public static VirtualGood NO_ADS = new LifetimeVG(
				"No Ads", 														// name
				"No More Ads!",				 									// description
				"no_ads",														// item id
			new PurchaseWithMarket(NO_ADS_LIFETIME_PRODUCT_ID, 0.99));	// the way this virtual good is purchased
	
		public static VirtualGood REX_ZERO_GOOD = new LifetimeVG(
	            "T-Rex",                                         			// name
	            "The most awesome dinosaur", // description
	            "rex_zero",                                          		// item id
			new PurchaseWithVirtualItem(REX_ZERO_GOOD_ITEM_ID, 200)); // the way this virtual good is purchased

	    /** Virtual Categories **/
	    // The muffin rush theme doesn't support categories, so we just put everything under a general category.
	    public static VirtualCategory UNIT_CATEGORY = new VirtualCategory(
			"Units", new List<string>(new string[] { REX_ZERO_GOOD_ITEM_ID })
	    );
	}

}
