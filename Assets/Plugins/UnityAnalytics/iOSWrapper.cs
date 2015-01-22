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

		public override string appVersion
		{
			get { return UnityEngine_Cloud_GetAppVersion(); }
		}

		public override string appBundleIdentifier
		{
			get { return UnityEngine_Cloud_GetBundleIdentifier(); }
		}

		public override string appInstallMode
		{
			get { return UnityEngine_Cloud_GetAppInstallMode(); }
		}
		
		public override bool isRootedOrJailbroken
		{
			get { return UnityEngine_Cloud_IsJailbroken(); }
		}		
	}
}

