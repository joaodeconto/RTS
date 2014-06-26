using UnityEngine;
using OnePF;
using System.Collections;
using System.Collections.Generic;
using Visiorama;

public class MarketMenu : MonoBehaviour
{


	private bool wasInitialized = false;

	void Awake()
	{
		
		DefaultCallbackButton defaultCallbackButton;
		
		GameObject option = transform.FindChild ("Menu").transform.FindChild ("Button (Facebook)").gameObject;
		
		defaultCallbackButton = option.AddComponent<DefaultCallbackButton> ();
		defaultCallbackButton.Init (null,
		                            (ht_dcb) =>
		                            {
			if (FB.IsLoggedIn)
			{
				
				FB.Feed(
					link: "https://www.facebook.com/RexTribalS",
					linkName: "Join Rex Tribal Society!",
					linkCaption: " 'Hail ya, can you say the word of our salvation? '",
					linkDescription: " Join RTS, Alpha testing with free gameplay and coins!", 
					picture: "https://scontent-b-gru.xx.fbcdn.net/hphotos-xpf1/t1.0-9/10462920_334632973356004_6602741597610783962_n.png"
					
					
					);
			}
			
		});
	}


	#if UNITY_ANDROID
	const string STORE_CUSTOM = "store";
	const string SKU = "orichal";
	
	private void OnEnable() {
		// Listen to all events for illustration purposes
		OpenIABEventManager.billingSupportedEvent += billingSupportedEvent;
		OpenIABEventManager.billingNotSupportedEvent += billingNotSupportedEvent;
		OpenIABEventManager.queryInventorySucceededEvent += queryInventorySucceededEvent;
		OpenIABEventManager.queryInventoryFailedEvent += queryInventoryFailedEvent;
		OpenIABEventManager.purchaseSucceededEvent += purchaseSucceededEvent;
		OpenIABEventManager.purchaseFailedEvent += purchaseFailedEvent;
		OpenIABEventManager.consumePurchaseSucceededEvent += consumePurchaseSucceededEvent;
		OpenIABEventManager.consumePurchaseFailedEvent += consumePurchaseFailedEvent;
		Open();
	}
	private void OnDisable() {
		// Remove all event handlers
		OpenIABEventManager.billingSupportedEvent -= billingSupportedEvent;
		OpenIABEventManager.billingNotSupportedEvent -= billingNotSupportedEvent;
		OpenIABEventManager.queryInventorySucceededEvent -= queryInventorySucceededEvent;
		OpenIABEventManager.queryInventoryFailedEvent -= queryInventoryFailedEvent;
		OpenIABEventManager.purchaseSucceededEvent -= purchaseSucceededEvent;
		OpenIABEventManager.purchaseFailedEvent -= purchaseFailedEvent;
		OpenIABEventManager.consumePurchaseSucceededEvent -= consumePurchaseSucceededEvent;
		OpenIABEventManager.consumePurchaseFailedEvent -= consumePurchaseFailedEvent;
		Close();
	}
	
//	private void Start() {
//		// Map sku for different stores
//		OpenIAB.mapSku(SKU, OpenIAB_Android.STORE_GOOGLE, "google-play.sku");
//		OpenIAB.mapSku(SKU, STORE_CUSTOM, "onepf.sku");
//	}

	public void Open ()
	{
		if (wasInitialized)
			return;
		
		wasInitialized = true;

		OpenIAB.mapSku(SKU, OpenIAB_iOS.STORE, "some.ios.sku");
		OpenIAB.mapSku(SKU, OpenIAB_Android.STORE_GOOGLE, "google-play.sku");
		OpenIAB.mapSku(SKU, STORE_CUSTOM, "onepf.sku");

		var public_key = "key";
		
		var options = new Options();
		options.verifyMode = OptionsVerifyMode.VERIFY_SKIP;
		options.storeKeys = new Dictionary<string, string> {
			{OpenIAB_Android.STORE_GOOGLE, public_key}
		};
		
		// Transmit options and start the service
		OpenIAB.init(options);
		
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
		
			OpenIAB.purchaseProduct(SKU);
		
		}

		if (orichalQuant == "500")
		{
			
			OpenIAB.purchaseProduct(SKU + "1");
			
		}

		if (orichalQuant == "1000")
		{
			
			OpenIAB.purchaseProduct(SKU + "2");
			
		}

		OpenIAB.queryInventory();
	

//		if (GUI.Button(new Rect(xPos, yPos, width, height), "Initialize OpenIAB")) {
//			// Application public key
//			var public_key = "key";
//			
//			var options = new Options();
//			options.verifyMode = OptionsVerifyMode.VERIFY_SKIP;
//			options.storeKeys = new Dictionary<string, string> {
//				{OpenIAB_Android.STORE_GOOGLE, public_key}
//			};
//			
//			// Transmit options and start the service
//			OpenIAB.init(options);
//		}
//		if (GUI.Button(new Rect(xPos, yPos += heightPlus, width, height), "Test Purchase")) {
//			OpenIAB.purchaseProduct("android.test.purchased");
//		}
//		
//		if (GUI.Button(new Rect(xPos, yPos += heightPlus, width, height), "Test Refund")) {
//			OpenIAB.purchaseProduct("android.test.refunded");
//		}
//		
//		if (GUI.Button(new Rect(xPos, yPos += heightPlus, width, height), "Test Item Unavailable")) {
//			OpenIAB.purchaseProduct("android.test.item_unavailable");
//		}
//		
//		xPos = Screen.width - width - 5.0f;
//		yPos = 5.0f;
//		
//		if (GUI.Button(new Rect(xPos, yPos, width, height), "Test Purchase Canceled")) {
//			OpenIAB.purchaseProduct("android.test.canceled");
//		}
//		
//		if (GUI.Button(new Rect(xPos, yPos += heightPlus, width, height), "Query Inventory")) {
//			OpenIAB.queryInventory();
//		}
//		
//		if (GUI.Button(new Rect(xPos, yPos += heightPlus, width, height), "Purchase Real Product")) {
//			OpenIAB.purchaseProduct(SKU);
//		}
		
//		if (GUI.Button(new Rect(xPos, yPos += heightPlus, width, height), "Stop Billing Service")) {
//			OpenIAB.unbindService();
//		}
	}
	
	private void billingSupportedEvent() {
		Debug.Log("billingSupportedEvent");
	}
	private void billingNotSupportedEvent(string error) {
		Debug.Log("billingNotSupportedEvent: " + error);
	}
	private void queryInventorySucceededEvent(Inventory inventory) {
		Debug.Log("queryInventorySucceededEvent: " + inventory);
	}
	private void queryInventoryFailedEvent(string error) {
		Debug.Log("queryInventoryFailedEvent: " + error);
	}
	private void purchaseSucceededEvent(Purchase purchase) {
		Debug.Log("purchaseSucceededEvent: " + purchase);
	}
	private void purchaseFailedEvent(string error) {
		Debug.Log("purchaseFailedEvent: " + error);
	}
	private void consumePurchaseSucceededEvent(Purchase purchase) {
		Debug.Log("consumePurchaseSucceededEvent: " + purchase);
	}
	private void consumePurchaseFailedEvent(string error) {
		Debug.Log("consumePurchaseFailedEvent: " + error);
	}

	public void Close ()
	{
		OpenIAB.unbindService();
		
	}
	#endif


		
		
		
		
}