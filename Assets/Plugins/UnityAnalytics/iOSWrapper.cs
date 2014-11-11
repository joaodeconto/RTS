using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;

namespace UnityEngine.Cloud.Analytics
{
	internal class iOSWrapper : BasePlatformWrapper
	{
		[DllImport ("__Internal")]
		public static extern string UnityEngine_Cloud_GetAppVersion();
		[DllImport ("__Internal")]
		public static extern string UnityEngine_Cloud_GetBundleIdentifier();
		[DllImport ("__Internal")]
		public static extern string UnityEngine_Cloud_GetAppInstallMode();
		[DllImport ("__Internal")]
		public static extern bool UnityEngine_Cloud_IsJailbroken();

		public override string GetAppVersion()
		{
			return UnityEngine_Cloud_GetAppVersion();
		}

		public override string GetAppBundleIdentifier()
		{
			return UnityEngine_Cloud_GetBundleIdentifier();
		}

		public override string GetAppInstallMode()
		{
			return UnityEngine_Cloud_GetAppInstallMode();
		}
		
		public override bool IsRootedOrJailbroken()
		{
			return UnityEngine_Cloud_IsJailbroken();
		}		
	}
}

