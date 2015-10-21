using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace I2.Loc
{
	public class ParsedTerm
	{
		public string Category, Term;
		public int Usage;
	}
	
	public partial class LocalizationEditor
	{
		#region Variables

		public static SortedDictionary<string, ParsedTerm> mParsedTerms = new SortedDictionary<string, ParsedTerm>(); // All Terms resulted from parsing the scenes and collecting the Localize.Term and how many times the terms are used
		public static SortedDictionary<string, int> mParsedCategories = new SortedDictionary<string, int>();	// Categories and how many terms used them

		public static bool mParseTermsIn_Scenes = true;
		public static bool mParseTermsIn_Scripts = true;

		#endregion
		
		#region GUI Parse Keys
		
		void OnGUI_Tools_ParseTerms()
		{
			OnGUI_ScenesList();

			GUI.backgroundColor = Color.Lerp (Color.gray, Color.white, 0.2f);
			GUILayout.BeginVertical("AS TextArea", GUILayout.Height(1));
			GUI.backgroundColor = Color.white;

			GUILayout.Space (5);

				EditorGUILayout.HelpBox("This tool searches all Terms used in the selected scenes and updates the usage counter in the Terms Tab", UnityEditor.MessageType.Info);

				GUILayout.Space (5);

				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace();
					GUILayout.BeginHorizontal ("Box");
					mParseTermsIn_Scenes = GUILayout.Toggle(mParseTermsIn_Scenes, new GUIContent("Parse SCENES", "Opens the selected scenes and finds all the used terms"));
					GUILayout.FlexibleSpace();
					mParseTermsIn_Scripts = GUILayout.Toggle(mParseTermsIn_Scripts, new GUIContent("Parse SCRIPTS", "Searches all .cs files and counts all terms like: ScriptLocalization.Get(\"xxx\")"));
					GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace();
					if (GUILayout.Button("Parse Localized Terms"))
						EditorApplication.update += ParseTermsInSelectedScenes;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}
		
		#endregion

		#region Parsed Terms Handlers

		public static ParsedTerm GetParsedTerm( string Term )
		{
			ParsedTerm data;
			if (!mParsedTerms.TryGetValue(Term, out data))
			{
				data = new ParsedTerm();
				data.Usage = 0;
				LanguageSource.DeserializeFullTerm( Term, out data.Term, out data.Category );
				mParsedCategories[data.Category]=1;
				mParsedTerms[Term] = data;
			}
			return data;
		}

		public static void RemoveParsedTerm( string Term )
		{
			mParsedTerms.Remove(Term);
			string category, key;
			LanguageSource.DeserializeFullTerm( Term, out key, out category );

			int usage;
			if (mParsedCategories.TryGetValue(category, out usage))
			{
				if (usage<=1)
					mParsedCategories.Remove(category);
				else
					mParsedCategories[category]=usage-1;
			}
		}

		public static void DecreaseParsedTerm( string Term )
		{
			ParsedTerm data = GetParsedTerm(Term);
			data.Usage = Mathf.Max (0, data.Usage-1);
		}


		#endregion

		#region ParseKeys

		void ParseTermsInSelectedScenes()
		{
			EditorApplication.update -= ParseTermsInSelectedScenes;
			ParseTerms(false);
		}
		
		void ParseTerms( bool OnlyCurrentScene, bool OpenTermsTab = true )
		{ 
			mIsParsing = true;

			mParsedTerms.Clear();
			mSelectedKeys.Clear ();

			if (mParseTermsIn_Scripts)
				ParseTermsInScripts();

			if (mParseTermsIn_Scenes)
			{
				if (!OnlyCurrentScene)
					ExecuteActionOnSelectedScenes( FindTermsInCurrentScene );
				else 
					FindTermsInCurrentScene();
			}
			
			FindTermsNotUsed();
			
			if (mParsedTerms.Count<=0)
			{
				ShowInfo ("No terms where found during parsing");
				return;
			}

			if (OpenTermsTab) 
			{
				mFlagsViewKeys = ((int)eFlagsViewKeys.Used | (int)eFlagsViewKeys.NotUsed | (int)eFlagsViewKeys.Missing);
				mCurrentViewMode = eViewMode.Keys;
			}
			mIsParsing = false;
		}
		
		void FindTermsInCurrentScene()
		{
			Localize[] Locals = (Localize[])Resources.FindObjectsOfTypeAll(typeof(Localize));
			
			if (Locals==null)
				return;
			
			for (int i=0, imax=Locals.Length; i<imax; ++i)
			{
				Localize localize = Locals[i];
				if (localize==null/* || localize.gameObject==null || !GUITools.ObjectExistInScene(localize.gameObject)*/)
					continue;
				 
				string Term, SecondaryTerm;
				//Term = localize.Term;
				//SecondaryTerm = localize.SecondaryTerm;
				localize.GetFinalTerms( out Term, out SecondaryTerm );

				if (!string.IsNullOrEmpty(Term))
					GetParsedTerm(Term).Usage++;

				if (!string.IsNullOrEmpty(SecondaryTerm))
					GetParsedTerm(SecondaryTerm).Usage++;
			}
		}

		void FindTermsNotUsed()
		{
			// every Term that is in the DB but not in mParsedTerms

			foreach (TermData termData in mLanguageSource.mTerms)
				GetParsedTerm(termData.Term);	// this will create the ParsedTerm if it doesn't exist
		}

        void ParseTermsInScripts() 
		{
            string[] scriptFiles = AssetDatabase.GetAllAssetPaths().Where(path => path.ToLower().EndsWith(".cs")).ToArray();

            Regex regex = new Regex(@"ScriptLocalization\.Get\(\""(.*?)\""\)", RegexOptions.Multiline);

            foreach (string scriptFile in scriptFiles) 
			{
                string scriptContents = File.ReadAllText(scriptFile);
                MatchCollection matches = regex.Matches(scriptContents);
                for (int matchNum = 0; matchNum < matches.Count; matchNum++) 
				{
                    Match match = matches[matchNum];
                    string term = match.Groups[1].Value;
                    GetParsedTerm(term).Usage++;
                }
            }
            
        }
		#endregion
	}
}
