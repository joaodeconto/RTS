//#define UGUI
//#define NGUI
//#define DFGUI

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace I2.Loc
{
	[CustomEditor(typeof(Localize))]
	public partial class LocalizeInspector : Editor
	{
		#region Variables

		SerializedObject mSerializedObj_Localize;
		Localize mLocalize;
		SerializedProperty 	mProp_mTerm, mProp_mTermSecondary,
							mProp_TranslatedObjects, mProp_IgnoreRTL, mProp_LocalizeOnAwake;


		bool mAllowEditKeyName = false;
		string mNewKeyName = "";

		bool mGUI_ShowReferences = false,
			 mGUI_ShowTems = true,
			 mGUI_ShowCallback = true;

		string[] mTermsArray = null;


		public static string HelpURL_forum 			= "http://goo.gl/Uiyu8C";//http://www.inter-illusion.com/forum/i2-localization";
		public static string HelpURL_Documentation 	= "http://goo.gl/cpeVTV";//http://www.inter-illusion.com/assets/I2Localization.pdf";
		public static string HelpURL_Tutorials		= "http://inter-illusion.com/tools/i2-localization";
		public static string HelpURL_ReleaseNotes	= "http://inter-illusion.com/forum/i2-localization/26-release-notes";


		#endregion
		
		#region Inspector
		
		void OnEnable()
		{
			mLocalize = (Localize)target;
			mSerializedObj_Localize = new SerializedObject( mLocalize );
			mProp_mTerm 			= mSerializedObj_Localize.FindProperty("mTerm");
			mProp_mTermSecondary	= mSerializedObj_Localize.FindProperty("mTermSecondary");
			mProp_TranslatedObjects = mSerializedObj_Localize.FindProperty("TranslatedObjects");
			mProp_IgnoreRTL			= mSerializedObj_Localize.FindProperty("IgnoreRTL");
			mProp_LocalizeOnAwake   = mSerializedObj_Localize.FindProperty("LocalizeOnAwake");

			LocalizationManager.UpdateSources();

			mGUI_ShowReferences = (mLocalize.TranslatedObjects!=null && mLocalize.TranslatedObjects.Length>0);
			mGUI_ShowCallback = (mLocalize.LocalizeCallBack.Target!=null);
			mGUI_ShowTems = true;
			LocalizationEditor.mKeysDesc_AllowEdit = false;
			GUI_SelectedTerm = 0;
			mNewKeyName = mLocalize.Term;

			if (mLocalize.Source!=null)
				LocalizationEditor.mLanguageSource = mLocalize.Source;

			UpgradeManager.EnablePlugins();
		}

		void OnDisable()
		{
		}

		#endregion

		#region GUI
		
		public override void OnInspectorGUI()
		{
			GUI.backgroundColor = Color.Lerp (Color.black, Color.gray, 1);
			GUILayout.BeginVertical(GUIStyle_Background, GUILayout.Height(1));
			GUI.backgroundColor = Color.white;

			if (GUILayout.Button("Localize", GUIStyle_Header))
			{
				Application.OpenURL(HelpURL_Documentation);
			}
			GUILayout.Space(-10);

			LocalizationManager.UpdateSources();

			if (LocalizationManager.Sources.Count==0)
			{
				EditorGUILayout.HelpBox("Unable to find a Language Source.", MessageType.Warning);
			}
			else
			{
				GUILayout.Space(10);
					OnGUI_Target ();
				GUILayout.Space(10);
					OnGUI_Terms();

				//if (mGUI_ShowTems || mGUI_ShowReferences) GUILayout.Space(5);

					OnGUI_References();

				if (mGUI_ShowReferences || mGUI_ShowCallback) GUILayout.Space(10);

					Localize loc = target as Localize;

				//--[ Localize Callback ]----------------------
					string HeaderTitle = "On Localize Call:";
					if (!mGUI_ShowCallback && loc.LocalizeCallBack.Target!=null && !string.IsNullOrEmpty(loc.LocalizeCallBack.MethodName))
						HeaderTitle = string.Concat(HeaderTitle, " <b>",loc.LocalizeCallBack.Target.name, ".</b><i>", loc.LocalizeCallBack.MethodName, "</i>");
					mGUI_ShowCallback = GUITools.DrawHeader(HeaderTitle, mGUI_ShowCallback);
					if (mGUI_ShowCallback)
					{
						GUITools.BeginContents();
							DrawEventCallBack( loc.LocalizeCallBack );
						GUITools.EndContents();
					}
			}
			OnGUI_Source ();

			GUILayout.Space (10);

			GUILayout.BeginHorizontal();
				if (GUILayout.Button("v"+LocalizationManager.GetVersion(), EditorStyles.miniLabel))
					Application.OpenURL(LocalizeInspector.HelpURL_ReleaseNotes);

				GUILayout.FlexibleSpace ();
				if (GUILayout.Button("Tutorials", EditorStyles.miniLabel))
					Application.OpenURL(HelpURL_Tutorials);
			
				GUILayout.Space(10);

				if (GUILayout.Button("Ask a Question", EditorStyles.miniLabel))
					Application.OpenURL(HelpURL_forum);

				GUILayout.Space(10);

				if (GUILayout.Button("Documentation", EditorStyles.miniLabel))
					Application.OpenURL(HelpURL_Documentation);
			GUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 0;

			GUILayout.EndVertical();
			
			mSerializedObj_Localize.ApplyModifiedProperties();
		}

		#endregion

		#region References

		void OnGUI_References()
		{
			if (mGUI_ShowReferences = GUITools.DrawHeader ("References", mGUI_ShowReferences))
			{
				GUITools.BeginContents();
				GUITools.DrawObjectsArray( mProp_TranslatedObjects );
				GUITools.EndContents();
			}
		}

		#endregion


		#region Terms

		int GUI_SelectedTerm = 0;
		void OnGUI_Terms()
		{
			if (mGUI_ShowTems=GUITools.DrawHeader ("Terms", mGUI_ShowTems))
			{
				//--[ tabs: Main and Secondary Terms ]----------------
				int oldTab = GUI_SelectedTerm;
				if (mLocalize.CanUseSecondaryTerm)
				{
					GUI_SelectedTerm = GUITools.DrawTabs (GUI_SelectedTerm, new string[]{"Main", "Secondary"});
				}
				else
				{
					GUI_SelectedTerm = 0;
					GUITools.DrawTabs (GUI_SelectedTerm, new string[]{"Main", ""});
				}

				GUITools.BeginContents();

					if (GUI_SelectedTerm==0) OnGUI_PrimaryTerm( oldTab!=GUI_SelectedTerm );
										else OnGUI_SecondaryTerm(oldTab!=GUI_SelectedTerm);

				GUITools.EndContents();

				//--[ Right To Left ]-------------
				GUILayout.BeginHorizontal();
					GUI.changed = false;
					bool bIgnore = GUILayout.Toggle( mLocalize.IgnoreRTL, " Ignore Right To Left Languages" );
					if (GUI.changed)
						mProp_IgnoreRTL.boolValue = bIgnore;

					GUILayout.FlexibleSpace();

					GUI.changed=false;
					int val = EditorGUILayout.Popup(GUI_SelectedTerm==0 ? (int)mLocalize.PrimaryTermModifier : (int)mLocalize.SecondaryTermModifier, System.Enum.GetNames(typeof(Localize.TermModification)));
					if (GUI.changed)
					{
						mSerializedObj_Localize.FindProperty( GUI_SelectedTerm==0 ? "PrimaryTermModifier" : "SecondaryTermModifier").enumValueIndex = val;
						GUI.changed = false;
					}

				GUILayout.EndHorizontal();

				var bOnAwake = GUILayout.Toggle(mProp_LocalizeOnAwake.boolValue, new GUIContent(" Pre-Localize on Awake", "Localizing on Awake could result in a lag when the level is loaded but faster later when objects are enabled. If false, it will Localize OnEnable, so will yield faster level load but could have a lag when screens are enabled") );
				if (bOnAwake != mProp_LocalizeOnAwake.boolValue)
					mProp_LocalizeOnAwake.boolValue = bOnAwake;				
			}
		}

		void OnGUI_PrimaryTerm( bool OnOpen )
		{
			string Key = mLocalize.mTerm;
			if (string.IsNullOrEmpty(Key))
			{
				string SecondaryTerm;
				mLocalize.GetFinalTerms( out Key, out SecondaryTerm );
			}

			if (OnOpen) mNewKeyName = Key;
			if ( OnGUI_SelectKey( ref Key, string.IsNullOrEmpty(mLocalize.mTerm)))
				mProp_mTerm.stringValue = Key;
			LocalizationEditor.OnGUI_Keys_Languages( Key, mLocalize, true );
		}

		void OnGUI_SecondaryTerm( bool OnOpen )
		{
			string Key = mLocalize.mTermSecondary;

			if (string.IsNullOrEmpty(Key))
			{
				string ss;
				mLocalize.GetFinalTerms( out ss, out Key );
			}
			
			if (OnOpen) mNewKeyName = Key;
			if ( OnGUI_SelectKey( ref Key, string.IsNullOrEmpty(mLocalize.mTermSecondary)))
				mProp_mTermSecondary.stringValue = Key;
			LocalizationEditor.OnGUI_Keys_Languages( Key, mLocalize, false );
		}

		bool OnGUI_SelectKey( ref string Term, bool Inherited )  // Inherited==true means that the mTerm is empty and we are using the Label.text instead
		{
			GUILayout.Space (5);
			GUILayout.BeginHorizontal();

			bool bChanged = false;
			mAllowEditKeyName = GUILayout.Toggle(mAllowEditKeyName, "Term:", EditorStyles.foldout, GUILayout.ExpandWidth(false));
			if (bChanged && mAllowEditKeyName)
				mNewKeyName = Term;

			bChanged = false;

			if (mTermsArray==null)
				UpdateTermsList(Term);

			if (Inherited)
				GUI.contentColor = Color.yellow*0.8f;

			int Index = System.Array.IndexOf( mTermsArray, Term );

			GUI.changed = false;

			int newIndex = EditorGUILayout.Popup( Index, mTermsArray);

			GUI.contentColor = Color.white;
			if (/*newIndex != Index && newIndex>=0*/GUI.changed)
			{
				GUI.changed = false;
				mTermsArray [mTermsArray.Length - 1] = string.Empty;
				mNewKeyName = Term = mTermsArray[newIndex];
				mAllowEditKeyName = false;
				bChanged = true;
			}
			LanguageSource source =  LocalizationManager.GetSourceContaining(Term);
			TermData termData = source.GetTermData(Term);
			if (termData!=null)
			{
				eTermType NewType = (eTermType)EditorGUILayout.EnumPopup(termData.TermType, GUILayout.Width(90));
				if (termData.TermType != NewType)
					termData.TermType = NewType;
			}
			
			GUILayout.EndHorizontal();
			
			if (mAllowEditKeyName)
			{
				GUILayout.BeginHorizontal(GUILayout.Height (1));
				GUILayout.BeginHorizontal(EditorStyles.toolbar);
				if(mNewKeyName==null) mNewKeyName = string.Empty;

				GUI.changed = false;
				mNewKeyName = EditorGUILayout.TextField(mNewKeyName, new GUIStyle("ToolbarSeachTextField"), GUILayout.ExpandWidth(true));
				if (GUI.changed)
				{
					mTermsArray = null;	// regenerate this array to apply filtering
					GUI.changed = false;
				}

				if (GUILayout.Button (string.Empty, string.IsNullOrEmpty(mNewKeyName) ? "ToolbarSeachCancelButtonEmpty" : "ToolbarSeachCancelButton", GUILayout.ExpandWidth(false)))
				{
					mTermsArray = null;	// regenerate this array to apply filtering
					mNewKeyName = string.Empty;
				}

				GUILayout.EndHorizontal();

				string ValidatedName = mNewKeyName;
				LanguageSource.ValidateFullTerm( ref ValidatedName );

				bool CanUseNewName = (source.GetTermData(ValidatedName)==null);
				GUI.enabled = (!string.IsNullOrEmpty(mNewKeyName) && CanUseNewName);
				if (GUILayout.Button ("Create", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
				{
					mNewKeyName = ValidatedName;
					Term = mNewKeyName;
					mTermsArray=null;	// this recreates that terms array

					LanguageSource Source = null;
					#if UNITY_EDITOR
					if (mLocalize.Source!=null)
						Source = mLocalize.Source;
					#endif

					if (Source==null)
						Source = LocalizationManager.Sources[0];

					Source.AddTerm( mNewKeyName, eTermType.Text );
					mAllowEditKeyName = false;
					bChanged = true;
					GUIUtility.keyboardControl = 0;
				}
				GUI.enabled = (termData!=null && !string.IsNullOrEmpty(mNewKeyName) && CanUseNewName);
				if (GUILayout.Button (new GUIContent("Rename","Renames the term in the source and updates every object using it in the current scene"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
				{
					mNewKeyName = ValidatedName;
					Term = mNewKeyName;
					mTermsArray=null;     // this recreates that terms array
					mAllowEditKeyName = false;
					bChanged = true;
					LocalizationEditor.TermReplacements = new Dictionary<string, string>();
					LocalizationEditor.TermReplacements[ termData.Term ] = mNewKeyName;
					termData.Term = mNewKeyName;
					LocalizationEditor.ReplaceTermsInCurrentScene();
					GUIUtility.keyboardControl = 0;
					//ParseTerms(true);
				}
				GUI.enabled = true;
				GUILayout.EndHorizontal();

				OnGUI_SelectKey_PreviewTerms ( ref Term);
			}
			
			GUILayout.Space (5);
			return bChanged;
		}

		void UpdateTermsList( string currentTerm )
		{
			List<string> Terms = LocalizationManager.GetTermsList();
			
			// If there is a filter, remove all terms not matching that filter
			if (mAllowEditKeyName && !string.IsNullOrEmpty(mNewKeyName)) 
			{
				string Filter = mNewKeyName.ToUpper();
				for (int i=Terms.Count-1; i>=0; --i)
					if (!Terms[i].ToUpper().Contains(Filter) && Terms[i]!=currentTerm)
						Terms.RemoveAt(i);
				
			}

			if (!Terms.Contains(currentTerm))
				Terms.Add (currentTerm);

			Terms.Sort(System.StringComparer.OrdinalIgnoreCase);
			Terms.Add ("<none>");
			mTermsArray = Terms.ToArray();
		}

		void OnGUI_SelectKey_PreviewTerms ( ref string Term)
		{
			if (mTermsArray==null)
				UpdateTermsList(Term);

			int nTerms = mTermsArray.Length;
			if (nTerms<=0)
				return;

			if (nTerms==1 && mTermsArray[0]==Term)
				return;


			GUI.backgroundColor = Color.gray;
			GUILayout.BeginVertical ("AS TextArea");
			for (int i = 0, imax = Mathf.Min (nTerms, 3); i < imax; ++i) 
			{
				ParsedTerm parsedTerm;
				int nUses = -1;
				if (LocalizationEditor.mParsedTerms.TryGetValue (mTermsArray [i], out parsedTerm))
					nUses = parsedTerm.Usage;

				string FoundText = mTermsArray [i];
				if (nUses > 0)
					FoundText = string.Concat ("(", nUses, ") ", FoundText);

				if (GUILayout.Button (FoundText, EditorStyles.miniLabel)) 
				{
					mNewKeyName = Term = mTermsArray [i];
					GUIUtility.keyboardControl = 0;
				}
			}
			if (nTerms > 3)
				GUILayout.Label ("...");
			GUILayout.EndVertical ();
			GUI.backgroundColor = Color.white;
		}

		#endregion

		#region Target

		void OnGUI_Target()
		{
			List<string> TargetTypes = new List<string>();
			int CurrentTarget = -1;

			mLocalize.FindTarget();
			TestTargetType<GUIText>		( ref TargetTypes, "GUIText", ref CurrentTarget );
			TestTargetType<TextMesh>	( ref TargetTypes, "TextMesh", ref CurrentTarget );
			TestTargetType<AudioSource>	( ref TargetTypes, "AudioSource", ref CurrentTarget );
			TestTargetType<GUITexture>	( ref TargetTypes, "GUITexture", ref CurrentTarget );

			#if UGUI
			TestTargetType<UnityEngine.UI.Text>		( ref TargetTypes, "UGUI Text", ref CurrentTarget );
			TestTargetType<UnityEngine.UI.Image>	( ref TargetTypes, "UGUI Image", ref CurrentTarget );
			TestTargetType<UnityEngine.UI.RawImage>	( ref TargetTypes, "UGUI RawImage", ref CurrentTarget );
			#endif

			#if NGUI
				TestTargetType<UILabel>		( ref TargetTypes, "NGUI UILabel", ref CurrentTarget );
				TestTargetType<UISprite>	( ref TargetTypes, "NGUI UISprite", ref CurrentTarget );
				TestTargetType<UITexture>	( ref TargetTypes, "NGUI UITexture", ref CurrentTarget );
			#endif

			#if DFGUI
				TestTargetType<dfButton>		( ref TargetTypes, "DFGUI Button", ref CurrentTarget );
				TestTargetType<dfLabel>			( ref TargetTypes, "DFGUI Label", ref CurrentTarget );
				TestTargetType<dfPanel>			( ref TargetTypes, "DFGUI Panel", ref CurrentTarget );
				TestTargetType<dfSprite>		( ref TargetTypes, "DFGUI Sprite", ref CurrentTarget );
				TestTargetType<dfRichTextLabel>	( ref TargetTypes, "DFGUI RichTextLabel", ref CurrentTarget );
			#endif

			#if TK2D
			TestTargetType<tk2dTextMesh>		( ref TargetTypes, "2DToolKit Label", ref CurrentTarget );
			TestTargetType<tk2dBaseSprite>		( ref TargetTypes, "2DToolKit Sprite", ref CurrentTarget );
			#endif

			#if TextMeshPro
			TestTargetType<TMPro.TextMeshPro>		( ref TargetTypes, "TextMeshPro Label", ref CurrentTarget );
			#endif
			#if TMProBeta
			TestTargetType<TMPro.TextMeshProUGUI>	( ref TargetTypes, "TextMeshPro UGUI", ref CurrentTarget );
			#endif

			
			TestTargetTypePrefab	( ref TargetTypes, "Prefab", ref CurrentTarget );

			if (CurrentTarget==-1)
			{
				CurrentTarget = TargetTypes.Count;
				TargetTypes.Add("None");
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label ("Target:", GUILayout.Width (60));
			GUI.changed = false;
			int index = EditorGUILayout.Popup(CurrentTarget, TargetTypes.ToArray());
			if (GUI.changed)
			{
				switch (TargetTypes[index])
				{
					case "GUIText" 				:  mLocalize.mTarget = mLocalize.GetComponent<GUIText>(); break;
					case "TextMesh" 			:  mLocalize.mTarget = mLocalize.GetComponent<TextMesh>(); break;
					case "AudioSource" 			:  mLocalize.mTarget = mLocalize.GetComponent<AudioSource>(); break;
					case "GUITexture" 			:  mLocalize.mTarget = mLocalize.GetComponent<GUITexture>(); break;
					
					#if UGUI
					case "UGUI Text" 			:  mLocalize.mTarget = mLocalize.GetComponent<UnityEngine.UI.Text>(); break;
					case "UGUI Image" 			:  mLocalize.mTarget = mLocalize.GetComponent<UnityEngine.UI.Image>(); break;
					case "UGUI RawImage" 		:  mLocalize.mTarget = mLocalize.GetComponent<UnityEngine.UI.RawImage>(); break;
					#endif
					
					#if NGUI
					case "NGUI UILabel" 		:  mLocalize.mTarget = mLocalize.GetComponent<UILabel>(); break;
					case "NGUI UISprite" 		:  mLocalize.mTarget = mLocalize.GetComponent<UISprite>(); break;
					case "NGUI UITexture" 		:  mLocalize.mTarget = mLocalize.GetComponent<UITexture>(); break;
					#endif
					
					#if DFGUI
					case "DFGUI Button" 		:  mLocalize.mTarget = mLocalize.GetComponent<dfButton>(); break;
					case "DFGUI Label" 			:  mLocalize.mTarget = mLocalize.GetComponent<dfLabel>(); break;
					case "DFGUI Panel" 			:  mLocalize.mTarget = mLocalize.GetComponent<dfPanel>(); break;
					case "DFGUI Sprite" 		:  mLocalize.mTarget = mLocalize.GetComponent<dfSprite>(); break;
					case "DFGUI RichTextLabel" 	:  mLocalize.mTarget = mLocalize.GetComponent<dfRichTextLabel>(); break;
					#endif

					#if TK2D
					case "2DToolKit Label" 		:  mLocalize.mTarget = mLocalize.GetComponent<tk2dTextMesh>(); break;
					case "2DToolKit Sprite"		:  mLocalize.mTarget = mLocalize.GetComponent<tk2dBaseSprite>(); break;
					#endif

					#if TextMeshPro
					case "TextMeshPro Label" 	:  mLocalize.mTarget = mLocalize.GetComponent<TMPro.TextMeshPro>(); break;
					#endif
					#if TMProBeta
					case "TextMeshPro UGUI" 	:  mLocalize.mTarget = mLocalize.GetComponent<TMPro.TextMeshProUGUI>(); break;
					#endif

					case "Prefab" 				:  mLocalize.mTarget = mLocalize.transform.GetChild(0).gameObject; break;
				}
				mLocalize.FindTarget();
			}
			GUILayout.EndHorizontal();
		}

		void TestTargetType<T>( ref List<string> TargetTypes, string TypeName, ref int CurrentTarget ) where T : Component
		{
			if (mLocalize.GetComponent<T>()==null)
				return;
			TargetTypes.Add(TypeName);

			if ((mLocalize.mTarget as T) != null)
				CurrentTarget = TargetTypes.Count-1;
		}

		void TestTargetTypePrefab( ref List<string> TargetTypes, string TypeName, ref int CurrentTarget )
		{
			if (mLocalize.transform.childCount==0)
				return;

			TargetTypes.Add(TypeName);
			
			if ((mLocalize.mTarget as GameObject) != null)
				CurrentTarget = TargetTypes.Count-1;
		}

		#endregion

		#region Source

		void OnGUI_Source()
		{
			GUILayout.BeginHorizontal();

				LanguageSource currentSource  = mLocalize.Source;
				if (currentSource==null)
				{
					currentSource = LocalizationManager.GetSourceContaining(mLocalize.Term);
	            }

            	if (GUILayout.Button("Open Source", EditorStyles.toolbarButton, GUILayout.Width (100)))
				{
					Selection.activeObject = currentSource;

					string sTerm, sSecondary;
					mLocalize.GetFinalTerms( out sTerm, out sSecondary );
					if (GUI_SelectedTerm==1) sTerm = sSecondary;
					LocalizationEditor.mKeyToExplore = sTerm;
				}

				GUILayout.Space (2);

				GUILayout.BeginHorizontal(EditorStyles.toolbar);
					EditorGUI.BeginChangeCheck ();
					if (!mLocalize.Source)
					{
						GUI.contentColor = Color.Lerp (Color.gray, Color.yellow, 0.1f);
					}
					LanguageSource NewSource = EditorGUILayout.ObjectField( currentSource, typeof(LanguageSource), true) as LanguageSource;
					GUI.contentColor = Color.white;
					if (EditorGUI.EndChangeCheck())
					{
						mLocalize.Source = NewSource;
					}
				GUILayout.EndHorizontal();

			GUILayout.EndHorizontal();
		}

		#endregion

		
		#region Event CallBack
		
		static public void DrawEventCallBack( EventCallback CallBack )
		{
			if (CallBack==null)
				return;
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Target:", GUILayout.ExpandWidth(false));
			CallBack.Target = EditorGUILayout.ObjectField( CallBack.Target, typeof(MonoBehaviour), true) as MonoBehaviour;
			GUILayout.EndHorizontal();
			
			if (CallBack.Target!=null)
			{
				GameObject GO = CallBack.Target.gameObject;
				List<MethodInfo> Infos = new List<MethodInfo>();

				var targets = GO.GetComponents(typeof(MonoBehaviour));
				foreach (var behavior in targets)
					Infos.AddRange( behavior.GetType().GetMethods() );

				List<string> Methods = new List<string>();
				
				for (int i = 0, imax=Infos.Count; i<imax; ++i)
				{
					MethodInfo mi = Infos[i];
					
					if (IsValidMethod(mi))
						Methods.Add (mi.Name);
				}
				
				int Index = Methods.IndexOf(CallBack.MethodName);
				
				int NewIndex = EditorGUILayout.Popup(Index, Methods.ToArray(), GUILayout.ExpandWidth(true));
				if (NewIndex!=Index)
					CallBack.MethodName = Methods[ NewIndex ];
			}
		}
		
		static bool IsValidMethod( MethodInfo mi )
		{
			if (mi.DeclaringType == typeof(MonoBehaviour) || mi.ReturnType != typeof(void))
				return false;
			
			ParameterInfo[] Params = mi.GetParameters ();
			if (Params.Length == 0)	return true;
			if (Params.Length > 1)  return false;
			
			if (Params [0].ParameterType.IsSubclassOf (typeof(UnityEngine.Object)))	return true;
			if (Params [0].ParameterType == typeof(UnityEngine.Object))	return true;
			return false;
		}
		
		
		#endregion

		#region Styles
		
		public static GUIStyle GUIStyle_Header {
			get{
				if (mGUIStyle_Header==null)
				{
					mGUIStyle_Header = new GUIStyle("HeaderLabel");
					mGUIStyle_Header.fontSize = 25;
					mGUIStyle_Header.normal.textColor = Color.Lerp(Color.white, Color.gray, 0.5f);
					mGUIStyle_Header.fontStyle = FontStyle.BoldAndItalic;
					mGUIStyle_Header.alignment = TextAnchor.UpperCenter;
				}
				return mGUIStyle_Header;
			}
		}
		static GUIStyle mGUIStyle_Header;
		
		public static GUIStyle GUIStyle_SubHeader {
			get{
				if (mGUIStyle_SubHeader==null)
				{
					mGUIStyle_SubHeader = new GUIStyle("HeaderLabel");
					mGUIStyle_SubHeader.fontSize = 13;
					mGUIStyle_SubHeader.fontStyle = FontStyle.Normal;
					mGUIStyle_SubHeader.margin.top = -50;
					mGUIStyle_SubHeader.alignment = TextAnchor.UpperCenter;
				}
				return mGUIStyle_SubHeader;
			}
		}
		static GUIStyle mGUIStyle_SubHeader;
		
		public static GUIStyle GUIStyle_Background {
			get{
				if (mGUIStyle_Background==null)
				{
					mGUIStyle_Background = new GUIStyle("AS TextArea");
					mGUIStyle_Background.overflow.left = 50;
					mGUIStyle_Background.overflow.right = 50;
					mGUIStyle_Background.overflow.top = -5;
					mGUIStyle_Background.overflow.bottom = 0;
				}
				return mGUIStyle_Background;
			}
		}
		static GUIStyle mGUIStyle_Background;
		
		#endregion
	}
}