using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace I2.Loc
{
	[CustomEditor(typeof(LanguageSource))]
	public partial class LocalizationEditor : Editor
	{
		#region Variables
		
		SerializedObject mSerializedObj_Source;
		SerializedProperty 	mProp_Assets, mProp_Languages, 
							mProp_Google_WebServiceURL, mProp_GoogleUpdateFrequency, mProp_Google_SpreadsheetKey, mProp_Google_SpreadsheetName, 
							mProp_Spreadsheet_LocalFileName, mProp_CaseInsensitiveTerms;

		public static LanguageSource mLanguageSource;

		static bool mIsParsing = false;  // This is true when the editor is opening several scenes to avoid reparsing objects

		#endregion
		
		#region Variables GUI
		
		GUIStyle Style_ToolBar_Big, Style_ToolBarButton_Big;
		
		public GUISkin CustomSkin;

		static Vector3 mScrollPos_Languages;
		static string mLanguages_NewLanguage = "";

		#endregion

		#region Inspector

		void OnEnable()
		{
			LocalizationManager.UpdateSources();
			mLanguageSource = (LanguageSource)target;
			mSerializedObj_Source 		= new SerializedObject( mLanguageSource );
			mProp_Assets 					= mSerializedObj_Source.FindProperty("Assets");
			mProp_Languages 				= mSerializedObj_Source.FindProperty("mLanguages");
			mProp_Google_WebServiceURL		= mSerializedObj_Source.FindProperty("Google_WebServiceURL");
			mProp_GoogleUpdateFrequency 	= mSerializedObj_Source.FindProperty("GoogleUpdateFrequency");
			mProp_Google_SpreadsheetKey 	= mSerializedObj_Source.FindProperty("Google_SpreadsheetKey");
			mProp_Google_SpreadsheetName	= mSerializedObj_Source.FindProperty("Google_SpreadsheetName");
			mProp_Spreadsheet_LocalFileName = mSerializedObj_Source.FindProperty("Spreadsheet_LocalFileName");
			mProp_CaseInsensitiveTerms 		= mSerializedObj_Source.FindProperty("CaseInsensitiveTerms");

			if (!mIsParsing)
			{
				if (string.IsNullOrEmpty(mLanguageSource.Google_SpreadsheetKey))
					mSpreadsheetMode = eSpreadsheetMode.Local;
				else
					mSpreadsheetMode = eSpreadsheetMode.Google;

				mCurrentViewMode = (mLanguageSource.mLanguages.Count>0 ? eViewMode.Keys : eViewMode.Languages);

				UpdateSelectedKeys();
				ParseTerms(true);
			}
			UpgradeManager.EnablePlugins();
		}

		void UpdateSelectedKeys()
		{
			// Remove all keys that are not in this source
			string trans;
			for (int i=mSelectedKeys.Count-1; i>=0; --i)
				if (!mLanguageSource.TryGetTermTranslation(mSelectedKeys[i], out trans))
					mSelectedKeys.RemoveAt(i);

			// Remove all Categories that are not in this source
			var mCateg = mLanguageSource.GetCategories();
			for (int i=mSelectedCategories.Count-1; i>=0; --i)
				if (!mCateg.Contains(mSelectedCategories[i]))
					mSelectedCategories.RemoveAt(i);
			if (mSelectedCategories.Count==0)
				mSelectedCategories = mCateg;

			if (mSelectedScenes.Count==0)
				mSelectedScenes.Add (EditorApplication.currentScene);
        }
        
        public override void OnInspectorGUI()
		{
			// Load Test:
			/*if (mLanguageSource.mTerms.Count<4000)
			{
				mLanguageSource.mTerms.Clear();
				for (int i=0; i<4500; ++i)
					mLanguageSource.AddTerm("ahh"+i.ToString("00000"));
				mLanguageSource.UpdateDictionary();
			}*/

			mIsParsing = false;
			//mSerializedObj_Source.UpdateIfDirtyOrScript();

			GUI.backgroundColor = Color.Lerp (Color.black, Color.gray, 1);
			GUILayout.BeginVertical(LocalizeInspector.GUIStyle_Background);
			GUI.backgroundColor = Color.white;
			
			if (GUILayout.Button("Language Source", LocalizeInspector.GUIStyle_Header))
			{
				Application.OpenURL(LocalizeInspector.HelpURL_Documentation);
			}

				InitializeStyles();

				GUILayout.Space(10);

				OnGUI_Main();

			GUILayout.Space (10);
			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
				if (GUILayout.Button("v"+LocalizationManager.GetVersion(), EditorStyles.miniLabel))
					Application.OpenURL(LocalizeInspector.HelpURL_ReleaseNotes);

				GUILayout.FlexibleSpace ();
				if (GUILayout.Button("Tutorials", EditorStyles.miniLabel))
					Application.OpenURL(LocalizeInspector.HelpURL_Tutorials);
			
				GUILayout.Space(10);

				if (GUILayout.Button("Ask a Question", EditorStyles.miniLabel))
					Application.OpenURL(LocalizeInspector.HelpURL_forum);

				GUILayout.Space(10);

				if (GUILayout.Button("Documentation", EditorStyles.miniLabel))
					Application.OpenURL(LocalizeInspector.HelpURL_Documentation);
			GUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 0;

			GUILayout.EndVertical();

			mSerializedObj_Source.ApplyModifiedProperties();
		}

		/*void OnDisable()
		{
			if (!mIsParsing)
				mParsedTerms.Clear ();
		}*/


		#endregion
	}
}