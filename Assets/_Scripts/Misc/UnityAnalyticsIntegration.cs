using UnityEngine;
using System.Collections;
using UnityEngine.Cloud.Analytics;

public class UnityAnalyticsIntegration : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		
		const string appId = "3469204743055734222";
		UnityAnalytics.StartSDK (appId);
		
	}
	
}
