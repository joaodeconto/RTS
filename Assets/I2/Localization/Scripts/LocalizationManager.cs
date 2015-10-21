﻿using UnityEngine;
using System.Collections.Generic;

namespace I2.Loc
{
	public static class LocalizationManager
	{
		#region Variables: CurrentLanguage
		
		public static string CurrentLanguage
		{
			get {
				InitializeIfNeeded();
				return mCurrentLanguage;
			}
			set {
				string SupportedLanguage = GetSupportedLanguage(value);
				if (!string.IsNullOrEmpty(SupportedLanguage) && mCurrentLanguage != SupportedLanguage)
				{
					SetLanguageAndCode( SupportedLanguage, GetLanguageCode(SupportedLanguage));
				}
			}
		}
		public static string CurrentLanguageCode
		{
			get { 
				InitializeIfNeeded();
				return mLanguageCode; }
			set {
				if (mLanguageCode!=value)
				{
					string LanName = GetLanguageFromCode( value );
					if (!string.IsNullOrEmpty(LanName))
						SetLanguageAndCode( LanName, value );
				}
			}
		}

		static string mCurrentLanguage;
		static string mLanguageCode;
		public static bool IsRight2Left = false;

		static void InitializeIfNeeded()
		{
			if (string.IsNullOrEmpty(mCurrentLanguage))
			{
				UpdateSources();
				SelectStartupLanguage();
			}
		}

		static void SetLanguageAndCode( string LanguageName, string LanguageCode, bool RememberLanguage = true )
		{
			if (mCurrentLanguage != LanguageName || mLanguageCode != LanguageCode)
			{
				if (RememberLanguage)
					PlayerPrefs.SetString ("I2 Language", LanguageName);
				mCurrentLanguage = LanguageName;
				mLanguageCode = LanguageCode;
				IsRight2Left = IsRTL (mLanguageCode);
				LocalizeAll(false);
			}
		}


		static void SelectStartupLanguage()
		{
			// Use the system language if there is a source with that language, 
			// or pick any of the languages provided by the sources

			string SavedLanguage = PlayerPrefs.GetString ("I2 Language", string.Empty);
			string SysLanguage = Application.systemLanguage.ToString();
			if (SysLanguage=="ChineseSimplified") SysLanguage = "Chinese (Simplified)";
			if (SysLanguage=="ChineseTraditional") SysLanguage = "Chinese (Traditional)";

			// Try selecting the System Language
			// But fallback to the first language found  if the System Language is not available in any source

			if (HasLanguage (SavedLanguage, Initialize:false))
			{
				CurrentLanguage = SavedLanguage;
				return;
			}

			// Check if the device language is supported. 
			// Also recognize when not region is set ("English (United State") will be used if sysLanguage is "English")
			string ValidLanguage = GetSupportedLanguage(SysLanguage);
			if (!string.IsNullOrEmpty(ValidLanguage))
			{
				SetLanguageAndCode( ValidLanguage, GetLanguageCode(ValidLanguage), false);
				return;
			}

			//--[ Use first language ]-----------
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				if (Sources[i].mLanguages.Count>0)
				{
					SetLanguageAndCode( Sources[i].mLanguages[0].Name, Sources[i].mLanguages[0].Code, false);
					return;
				}
		}

		#endregion

		#region Variables: Misc

		//public static Dictionary<string, string> Terms = new Dictionary<string, string>();
		public static List<LanguageSource> Sources = new List<LanguageSource>();
		public static string[] GlobalSources = new string[]{"I2Languages"};

		public delegate void OnLocalizeCallback ();
		public static event OnLocalizeCallback OnLocalizeEvent;

		#endregion

		#region Localization

		public static string GetTermTranslation (string Term, bool FixForRTL = false)
		{
			string Translation;
			if (TryGetTermTranslation(Term, out Translation, FixForRTL))
				return Translation;

			return string.Empty;
		}

		public static bool TryGetTermTranslation(string Term, out string Translation, bool FixForRTL = false)
		{
			Translation = string.Empty;
            if (string.IsNullOrEmpty(Term))
				return false;

			InitializeIfNeeded();

			for (int i=0, imax=Sources.Count; i<imax; ++i)
				if (Sources[i].TryGetTermTranslation (Term, out Translation))
				{
					if (LocalizationManager.IsRight2Left && FixForRTL)
						Translation = ArabicSupport.ArabicFixer.Fix(Translation);
                	return true;
				}

			return false;
		}

		public static string FixRTL_IfNeeded(string text)
		{
			if (LocalizationManager.IsRight2Left)
				return ArabicSupport.ArabicFixer.Fix(text);
			else
				return text;
		}

		internal static void LocalizeAll(bool Force = false)
		{
			Localize[] Locals = (Localize[])Resources.FindObjectsOfTypeAll( typeof(Localize) );
			for (int i=0, imax=Locals.Length; i<imax; ++i)
			{
				Localize local = Locals[i];
				//if (ObjectExistInScene (local.gameObject))
				local.OnLocalize(Force);
			}
			if (OnLocalizeEvent != null)
				OnLocalizeEvent ();
			ResourceManager.pInstance.CleanResourceCache();
		}

		#endregion

		#region Sources

		public static bool UpdateSources()
		{
			UnregisterDeletededSources();
			RegisterSourceInResources();
			RegisterSceneSources();
            return Sources.Count>0;
        }

		static void UnregisterDeletededSources()
		{
			// Delete sources that were part of another scene and not longer available
			for (int i=Sources.Count-1; i>=0; --i)
				if (Sources[i] == null)
					RemoveSource( Sources[i] );
		}

		static void RegisterSceneSources()
		{
			LanguageSource[] SceneSources = (LanguageSource[])Resources.FindObjectsOfTypeAll( typeof(LanguageSource) );
			for (int i=0, imax=SceneSources.Length; i<imax; ++i)
				if (!Sources.Contains(SceneSources[i]))
				{
					AddSource( SceneSources[i] );
				}
		}		

		static void RegisterSourceInResources()
		{
			// Find the Source that its on the Resources Folder
			foreach (string SourceName in GlobalSources)
			{
				GameObject Prefab = (ResourceManager.pInstance.GetAsset<GameObject>(SourceName));
				LanguageSource GlobalSource = (Prefab ? Prefab.GetComponent<LanguageSource>() : null);
				
				if (GlobalSource && !Sources.Contains(GlobalSource))
					AddSource( GlobalSource );
			}
		}		

		internal static void AddSource ( LanguageSource Source )
		{
			if (Sources.Contains (Source))
				return;

			//Debug.Log ("AddSource " + Source+" " + Source.GetInstanceID());

			Sources.Add( Source );
			Source.UpdateDictionary();
			Source.Import_Google();
		}

		internal static void RemoveSource (LanguageSource Source )
		{
			//Debug.Log ("RemoveSource " + Source+" " + Source.GetInstanceID());
			Sources.Remove( Source );
		}

		public static bool IsGlobalSource( string SourceName )
		{
			return System.Array.IndexOf(GlobalSources, SourceName)>=0;
		}

		public static bool HasLanguage( string Language, bool AllowDiscartingRegion = true, bool Initialize=true )
		{
			if (Initialize)
				LocalizationManager.InitializeIfNeeded();

			// First look for an exact match
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				if (Sources[i].GetLanguageIndex(Language, false)>=0)
					return true;

			// Then allow matching "English (Canada)" to "english"
			if (AllowDiscartingRegion)
			{
				for (int i=0, imax=Sources.Count; i<imax; ++i)
					if (Sources[i].GetLanguageIndex(Language, true)>=0)
						return true;
			}
			return false;
		}

		// Returns the provided language or a similar one without the Region 
		//(e.g. "English (Canada)" could be mapped to "english" or "English (United States)" if "English (Canada)" is not found
		public static string GetSupportedLanguage( string Language )
		{
			// First look for an exact match
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndex(Language, false);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Name;
			}
			
			// Then allow matching "English (Canada)" to "english"
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndex(Language, true);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Name;
			}

			return string.Empty;
		}

		public static string GetLanguageCode( string Language )
		{
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndex(Language);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Code;
			}
			return string.Empty;
		}

		public static string GetLanguageFromCode( string Code )
		{
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndexFromCode(Code);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Name;
			}
			return string.Empty;
		}


		public static List<string> GetAllLanguages ()
		{
			List<string> Languages = new List<string> ();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				for (int j=0, jmax=Sources[i].mLanguages.Count; j<jmax; ++j)
				{
					if (!Languages.Contains(Sources[i].mLanguages[j].Name))
						Languages.Add(Sources[i].mLanguages[j].Name);
				}
			}
			return Languages;
		}

		public static List<string> GetCategories ()
		{
			List<string> Categories = new List<string> ();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				Sources[i].GetCategories(false, Categories);
            return Categories;
        }



		public static List<string> GetTermsList ()
		{
			if (Sources.Count==0)
				UpdateSources();

			if (Sources.Count==1)
				return Sources[0].GetTermsList();

			HashSet<string> Terms = new HashSet<string> ();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				Terms.UnionWith( Sources[i].GetTermsList() );
			return new List<string>(Terms);
		}

		public static TermData GetTermData( string term )
		{
			TermData data;
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				data = Sources[i].GetTermData(term);
				if (data!=null)
					return data;
			}

			return null;
		}

		public static LanguageSource GetSourceContaining( string term, bool fallbackToFirst = true )
		{
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				if (Sources[i].GetTermData(term) != null)
					return Sources[i];
            }
            
            return ((fallbackToFirst && Sources.Count>0) ? Sources[0] :  null);
		}
        

		#endregion

		public static Object FindAsset (string value)
		{
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				Object Obj = Sources[i].FindAsset(value);
				if (Obj)
					return Obj;
			}
			return null;
		}

		public static string GetVersion()
		{
			return "2.6.0 f2";
		}

		public static int GetRequiredWebServiceVersion()
		{
			return 3;
		}

		#region Left to Right Languages

		static string[] LanguagesRTL = {"ar-DZ", "ar","ar-BH","ar-EG","ar-IQ","ar-JO","ar-KW","ar-LB","ar-LY","ar-MA","ar-OM","ar-QA","ar-SA","ar-SY","ar-TN","ar-AE","ar-YE",
										"he","ur","ji"};

		static bool IsRTL(string Code)
		{
			return System.Array.IndexOf(LanguagesRTL, Code)>=0;
		}

		#endregion

#if UNITY_EDITOR
		// This function should only be called from within the Localize Inspector to temporaly preview that Language

		public static void PreviewLanguage(string NewLanguage)
		{
			mCurrentLanguage = NewLanguage;
		}
#endif
	}

	public class TermsPopup : PropertyAttribute {}
}