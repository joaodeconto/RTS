using System;
using System.Collections.Generic;

namespace UnityEngine.Cloud.Analytics
{
	public enum SexEnum
	{
		M,
		F,
		U
	}
	
	public static class UnityAnalytics
	{
		public static AnalyticsResult StartSDK(string appId)
		{
			IUnityAnalyticsSession session = UnityAnalyticsSession.GetSingleton();
			return session.StartWithAppId(appId, PlatformWrapper.GetPlatform());
		}

		public static void SetLogLevel(LogType logType, bool enableLogging=true)
		{
			IUnityAnalyticsSession session = UnityAnalyticsSession.GetSingleton();
			session.SetLogLevel(logType, enableLogging);
		}
		
		public static AnalyticsResult SetUserId(string userId)
		{
			IUnityAnalyticsSession session = UnityAnalyticsSession.GetSingleton();
			return session.SetUserId(userId);
		}

		public static AnalyticsResult SetUserGender(SexEnum gender)
		{
			IUnityAnalyticsSession session = UnityAnalyticsSession.GetSingleton();
			return session.SetUserGender( gender==SexEnum.M ? "M" : gender==SexEnum.F ? "F" : "U" );
		}

		public static AnalyticsResult SetUserBirthYear(int birthYear)
		{
			IUnityAnalyticsSession session = UnityAnalyticsSession.GetSingleton();
			return session.SetUserBirthYear(birthYear);
		}

		public static AnalyticsResult Transaction(string productId, decimal amount, string currency)
		{
			IUnityAnalyticsSession session = UnityAnalyticsSession.GetSingleton();
			return session.Transaction(productId, amount, currency, null, null);
		}

		public static AnalyticsResult Transaction(string productId, decimal amount, string currency, string receiptPurchaseData, string signature)
		{
			IUnityAnalyticsSession session = UnityAnalyticsSession.GetSingleton();
			return session.Transaction(productId, amount, currency, receiptPurchaseData, signature);
		}

		public static AnalyticsResult CustomEvent(string customEventName, IDictionary<string, object> eventData)
		{
			IUnityAnalyticsSession session = UnityAnalyticsSession.GetSingleton();
			return session.CustomEvent(customEventName, eventData);
		}
	}
	
}
