using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables

		#endregion

		void OnGUI_Languages()
		{
			//GUILayout.Space(5);

			OnGUI_ShowMsg();

			OnGUI_LanguageList();
		}

		#region GUI Languages
		
		void OnGUI_LanguageList()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
				GUILayout.FlexibleSpace();
				GUILayout.Label ("Languages:", EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
				GUILayout.FlexibleSpace();
				GUILayout.Label ("Code:", EditorStyles.miniLabel, GUILayout.Width(76));
			GUILayout.EndHorizontal();
			
			//--[ Language List ]--------------------------

			int IndexLanguageToDelete = -1;
			int LanguageToMoveUp = -1;
			int LanguageToMoveDown = -1;
			mScrollPos_Languages = GUILayout.BeginScrollView( mScrollPos_Languages, "AS TextArea", GUILayout.MinHeight (100), GUILayout.MaxHeight(Screen.height), GUILayout.ExpandHeight(false));

			List<string> codes = GoogleLanguages.GetAllInternationalCodes();
			codes.Sort();
			codes.Insert(0, string.Empty);

			for (int i=0, imax=mProp_Languages.arraySize; i<imax; ++i)
			{
				GUILayout.BeginHorizontal();

				SerializedProperty Prop_Lang = mProp_Languages.GetArrayElementAtIndex(i);
				SerializedProperty Prop_LangName = Prop_Lang.FindPropertyRelative("Name");
				SerializedProperty Prop_LangCode = Prop_Lang.FindPropertyRelative("Code");

				if (GUILayout.Button ("X", "toolbarbutton", GUILayout.ExpandWidth(false)))
				{
					IndexLanguageToDelete = i;
				}
				
				GUILayout.BeginHorizontal(EditorStyles.toolbar);

				GUI.changed = false;
				string LanName = EditorGUILayout.TextField(Prop_LangName.stringValue, GUILayout.ExpandWidth(true));
				if (GUI.changed && !string.IsNullOrEmpty(LanName))
				{
					Prop_LangName.stringValue = LanName;
					GUI.changed = false;
				}

				int Index = Mathf.Max(0, codes.IndexOf (Prop_LangCode.stringValue));
				GUI.changed = false;
				Index = EditorGUILayout.Popup(Index, codes.ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(60));
				if (GUI.changed && Index>=0)
				{
					Prop_LangCode.stringValue = codes[Index];
				}

				GUILayout.EndHorizontal();

				GUI.enabled = (i<imax-1);
				if (GUILayout.Button( "\u25BC", EditorStyles.toolbarButton, GUILayout.Width(18))) LanguageToMoveDown = i;
				GUI.enabled = i>0;
				if (GUILayout.Button( "\u25B2", EditorStyles.toolbarButton, GUILayout.Width(18))) LanguageToMoveUp = i;
				GUI.enabled = true;
				
				GUILayout.EndHorizontal();
			}
			
			GUILayout.EndScrollView();
			
			OnGUI_AddLanguage( mProp_Languages );
			
			if (IndexLanguageToDelete>=0)
			{
				mLanguageSource.RemoveLanguage( mLanguageSource.mLanguages[IndexLanguageToDelete].Name );
				mSerializedObj_Source.Update();
				ParseTerms(true, false);
			}

			if (LanguageToMoveUp>=0)   SwapLanguages( LanguageToMoveUp, LanguageToMoveUp-1 );
			if (LanguageToMoveDown>=0) SwapLanguages( LanguageToMoveDown, LanguageToMoveDown+1 );
		}

		void SwapLanguages( int iFirst, int iSecond )
		{
			mSerializedObj_Source.ApplyModifiedProperties();
			LanguageSource Source = mLanguageSource;

			SwapValues( Source.mLanguages, iFirst, iSecond );
			foreach (TermData termData in Source.mTerms)
			{
				SwapValues ( termData.Languages, iFirst, iSecond );
				SwapValues ( termData.Languages_Touch, iFirst, iSecond );
				SwapValues ( termData.Flags, iFirst, iSecond );
			}
			mSerializedObj_Source.Update();
		}

		void SwapValues( List<LanguageData> mList, int Index1, int Index2 )
		{
			LanguageData temp = mList[Index1];
			mList[Index1] = mList[Index2];
			mList[Index2] = temp;
		}
		void SwapValues( string[] mList, int Index1, int Index2 )
		{
			string temp = mList[Index1];
			mList[Index1] = mList[Index2];
			mList[Index2] = temp;
		}
		void SwapValues( int[] mList, int Index1, int Index2 )
		{
			int temp = mList[Index1];
			mList[Index1] = mList[Index2];
			mList[Index2] = temp;
		}

		
		void OnGUI_AddLanguage( SerializedProperty Prop_Languages)
		{
			//--[ Add Language Upper Toolbar ]-----------------
			
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			mLanguages_NewLanguage = EditorGUILayout.TextField("", mLanguages_NewLanguage, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();

			if (GUILayout.Button("Add", EditorStyles.toolbarButton, GUILayout.Width(50)))
			{
				Prop_Languages.serializedObject.ApplyModifiedProperties();
				mLanguageSource.AddLanguage( mLanguages_NewLanguage, GoogleLanguages.GetLanguageCode(mLanguages_NewLanguage) );
				Prop_Languages.serializedObject.Update();
				mLanguages_NewLanguage = "";
			}
			
			GUILayout.EndHorizontal();
			
			
			//--[ Add Language Bottom Toolbar ]-----------------
			
			GUILayout.BeginHorizontal();
			
			//-- Language Dropdown -----------------
			string CodesToExclude = string.Empty;
			foreach (var LanData in mLanguageSource.mLanguages)
				CodesToExclude = string.Concat(CodesToExclude, "[", LanData.Code, "]");

			List<string> Languages = GoogleLanguages.GetLanguagesForDropdown(mLanguages_NewLanguage, CodesToExclude);

			GUI.changed = false;
			int index = EditorGUILayout.Popup(0, Languages.ToArray(), EditorStyles.toolbarDropDown);

			if (GUI.changed && index>=0)
			{
				mLanguages_NewLanguage = GoogleLanguages.GetFormatedLanguageName( Languages[index] );
			}
			
			
			if (GUILayout.Button("Add", EditorStyles.toolbarButton, GUILayout.Width(50)) && index>=0)
			{
				Prop_Languages.serializedObject.ApplyModifiedProperties();
				mLanguages_NewLanguage = GoogleLanguages.GetFormatedLanguageName( Languages[index] );
				if (!string.IsNullOrEmpty(mLanguages_NewLanguage)) 
					mLanguageSource.AddLanguage( mLanguages_NewLanguage, GoogleLanguages.GetLanguageCode(mLanguages_NewLanguage) );
				Prop_Languages.serializedObject.Update();
				mLanguages_NewLanguage = "";
			}
			
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUI.color = Color.white;
		}

		#endregion
	}
}