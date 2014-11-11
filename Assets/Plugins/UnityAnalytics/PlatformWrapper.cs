#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 
#define UNITY_COMPATLEVEL_UNITY4
//#elif ((UNITY_4_5 || UNITY_4_6) && (!(UNITY_WP8 || UNITY_METRO)) )
#elif ((UNITY_4_5 || UNITY_4_6) && UNITY_STANDALONE )
#define UNITY_COMPATLEVEL_UNITY4
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Cloud.Analytics
{
	internal static class PlatformWrapper
	{
		public static IPlatformWrapper GetPlatform()
		{
			#if USE_ANALYTICS_PLUGIN
			#if UNITY_ANDROID && !UNITY_EDITOR
			return new AndroidWrapper();
			#elif UNITY_IPHONE && !UNITY_EDITOR
			return new iOSWrapper();
			#else
			return new BasePlatformWrapper();
			#endif
			#else
			return new RuntimePlatformWrapper();
			#endif
		}
	}
	
	internal class BasePlatformWrapper : IPlatformWrapper, IWWWRequestCreator
	{
		public virtual string GetAppVersion()
		{
			return null;
		}
		
		public virtual string GetAppBundleIdentifier()
		{
			return null;
		}
		
		public virtual string GetAppInstallMode()
		{
			return null;
		}
		
		public virtual bool IsRootedOrJailbroken()
		{
			return false;
		}

		public IWWWRequestCreator GetIWWWRequestCreator()
		{
			return this;
		}

		#if UNITY_COMPATLEVEL_UNITY4
		public object CreateWWWRequest(string url, byte[] body, Dictionary<string, string> headers)
		{
			return new WWW(url, body, DictToHash(headers));
		}
		
		private Hashtable DictToHash(Dictionary<string, string> headers)
		{
			var result = new Hashtable();
			foreach (var kvp in headers)
				result[kvp.Key] = kvp.Value;
			return result;
		}
		#else
		public object CreateWWWRequest(string url, byte[] body, Dictionary<string, string> headers)
		{
			return new WWW(url, body, headers);
		}
		#endif
		
	}
	
	#if !USE_ANALYTICS_PLUGIN
	internal class RuntimePlatformWrapper : BasePlatformWrapper
	{
		public override string GetAppVersion()
		{
			string appVer = null;
			return appVer;
		}
		
		public override string GetAppBundleIdentifier()
		{
			string appBundleId = null;
			return appBundleId;
		}
		
		public override string GetAppInstallMode()
		{
			string appInstallMode = null;
			return appInstallMode;
		}
		
		public override bool IsRootedOrJailbroken()
		{
			bool isBroken = false;
			return isBroken;
		}
	}
	#endif
	
}

