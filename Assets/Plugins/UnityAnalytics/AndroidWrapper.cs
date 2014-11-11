using UnityEngine;
using System.Collections;

namespace UnityEngine.Cloud.Analytics
{
	internal class AndroidWrapper : BasePlatformWrapper
	{
		public override string GetAppVersion()
		{
			string appVer = null;
			#if UNITY_ANDROID && !UNITY_EDITOR
			using(var appUtilClass = new AndroidJavaClass("com.unityengine.cloud.AppUtil"))
			{
				appVer = appUtilClass.CallStatic<string>("getAppVersion");
			}
			#endif
			return appVer;
		}

		public override string GetAppBundleIdentifier()
		{
			string appBundleId = null;
			#if UNITY_ANDROID && !UNITY_EDITOR
			using(var appUtilClass = new AndroidJavaClass("com.unityengine.cloud.AppUtil"))
			{
				appBundleId = appUtilClass.CallStatic<string>("getAppPackageName");
			}
			#endif
			return appBundleId;
		}

		public override string GetAppInstallMode()
		{
			string appInstallMode = null;
			#if UNITY_ANDROID && !UNITY_EDITOR
			using(var appUtilClass = new AndroidJavaClass("com.unityengine.cloud.AppUtil"))
			{
				appInstallMode = appUtilClass.CallStatic<string>("getAppInstallMode");
			}
			#endif
			return appInstallMode;
		}
		
		public override bool IsRootedOrJailbroken()
		{
			bool isBroken = false;
			#if UNITY_ANDROID && !UNITY_EDITOR
			using(var appUtilClass = new AndroidJavaClass("com.unityengine.cloud.AppUtil"))
			{
				isBroken = appUtilClass.CallStatic<bool>("isDeviceRooted");
			}
			#endif
			return isBroken;
		}
	}
}

