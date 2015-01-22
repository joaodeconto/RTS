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
			#if UNITY_EDITOR || UNITY_IPHONE || UNITY_ANDROID 
			IUnityAnalyticsSession session = UnityAnalytics.GetSingleton();
			return session.StartWithAppId(appId);
			#else
			return AnalyticsResult.UnsupportedPlatform;
			#endif
		}

		public static void SetLogLevel(LogLevel logLevel, bool enableLogging=true)
		{
			#if UNITY_EDITOR || UNITY_IPHONE || UNITY_ANDROID 
			Logger.EnableLogging = enableLogging;
			Logger.logLevel = logLevel;
			#endif
		}
		
		public static AnalyticsResult SetUserId(string userId)
		{
			#if UNITY_EDITOR || UNITY_IPHONE || UNITY_ANDROID 
			IUnityAnalyticsSession session = UnityAnalytics.GetSingleton();
			return session.SetUserId(userId);
			#else
			return AnalyticsResult.UnsupportedPlatform;
			#endif
		}

		public static AnalyticsResult SetUserGender(SexEnum gender)
		{
			#if UNITY_EDITOR || UNITY_IPHONE || UNITY_ANDROID 
			IUnityAnalyticsSession session = UnityAnalytics.GetSingleton();
			return session.SetUserGender( gender==SexEnum.M ? "M" : gender==SexEnum.F ? "F" : "U" );
			#else
			return AnalyticsResult.UnsupportedPlatform;
			#endif
		}

		public static AnalyticsResult SetUserBirthYear(int birthYear)
		{
			#if UNITY_EDITOR || UNITY_IPHONE || UNITY_ANDROID 
			IUnityAnalyticsSession session = UnityAnalytics.GetSingleton();
			return session.SetUserBirthYear(birthYear);
			#else
			return AnalyticsResult.UnsupportedPlatform;
			#endif
		}

		public static AnalyticsResult Transaction(string productId, decimal amount, string currency)
		{
			#if UNITY_EDITOR || UNITY_IPHONE || UNITY_ANDROID 
			IUnityAnalyticsSession session = UnityAnalytics.GetSingleton();
			return session.Transaction(productId, amount, currency, null, null);
			#else
			return AnalyticsResult.UnsupportedPlatform;
			#endif
		}

		public static AnalyticsResult Transaction(string productId, decimal amount, string currency, string receiptPurchaseData, string signature)
		{
			#if UNITY_EDITOR || UNITY_IPHONE || UNITY_ANDROID 
			IUnityAnalyticsSession session = UnityAnalytics.GetSingleton();
			return session.Transaction(productId, amount, currency, receiptPurchaseData, signature);
			#else
			return AnalyticsResult.UnsupportedPlatform;
			#endif
		}

		public static AnalyticsResult CustomEvent(string customEventName, IDictionary<string, object> eventData)
		{
			#if UNITY_EDITOR || UNITY_IPHONE || UNITY_ANDROID 
			IUnityAnalyticsSession session = UnityAnalytics.GetSingleton();
			return session.CustomEvent(customEventName, eventData);
			#else
			return AnalyticsResult.UnsupportedPlatform;
			#endif
		}

		private static SessionImpl s_Implementation;

		private static IUnityAnalyticsSession GetSingleton()
		{
			if (s_Implementation == null) {
				Logger.loggerInstance = new UnityLogger();
				IPlatformWrapper platformWrapper = PlatformWrapper.platform;
				IFileSystem fileSystem = new FileSystem();
				ICoroutineManager coroutineManager = new UnityCoroutineManager();
				s_Implementation = new SessionImpl(platformWrapper, coroutineManager, fileSystem);
				GameObserver.CreateComponent(platformWrapper, s_Implementation);
			}
			return s_Implementation;
		}
	}
}
