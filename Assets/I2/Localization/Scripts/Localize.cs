using UnityEngine;
using System.Collections;

// localize: Subtitle

namespace I2.Loc
{
	[AddComponentMenu("I2/Localization/Localize")]
	public partial class Localize : MonoBehaviour 
	{
		#region Variables: Term
		public string Term 
		{ 
			get { return mTerm; } 
			set { mTerm = value; }
		}
		public string SecondaryTerm 
		{ 
			get { return mTermSecondary; } 
			set { mTermSecondary = value; }
		}

		public string mTerm = string.Empty,  		  // if Target is a Label, this will be the text,  if sprite, this will be the spriteName, etc
					  mTermSecondary =  string.Empty; // if Target is a Label, this will be the font Name,  if sprite, this will be the Atlas name, etc

		// This are the terms actually used (will be mTerm/mSecondaryTerm or will get them from the objects if those are missing. e.g. Labels' text and font name)
		// This are set when the component starts
		string FinalTerm, FinalSecondaryTerm;

		public enum TermModification {DontModify, ToUpper, ToLower, ToUpperFirst, ToTitle/*, CustomRange*/}
		public TermModification PrimaryTermModifier = TermModification.DontModify, 
								SecondaryTermModifier = TermModification.DontModify;

		public bool LocalizeOnAwake = true;

		string LastLocalizedLanguage;	// Used to avoid Localizing everytime the object is Enabled

		#if UNITY_EDITOR
			public LanguageSource Source;	// Source used while in the Editor to preview the Terms
		#endif

		#endregion

		#region Variables: Target

		// This is the Object/Component that should be localized
		public Object mTarget;

		public event System.Action EventFindTarget;

		public delegate void DelegateSetFinalTerms(string Main, string Secondary, out string primaryTerm, out string secondaryTerm);
		public DelegateSetFinalTerms EventSetFinalTerms;

		public delegate void DelegateDoLocalize(string primaryTerm, string secondaryTerm);
		public DelegateDoLocalize EventDoLocalize;

		public bool CanUseSecondaryTerm = false;

		public bool AllowMainTermToBeRTL = false;	//	Whatever or not this localize should Fix the MainTranslation on Right To Left Languages
		public bool AllowSecondTermToBeRTL = false; // Same for secondary Translation
		public bool IgnoreRTL = false;	// If false, no Right To Left processing will be done

		#endregion

		#region Variables: References

		public Object[] TranslatedObjects;	// For targets that reference objects (e.g. AudioSource, UITexture,etc) 
											// this keeps a reference to the possible options.
											// If the value is not the name of any of this objects then it will try to load the object from the Resources

		#endregion

		#region Variable Translation Modifiers

		public EventCallback LocalizeCallBack = new EventCallback();	// This allows scripts to modify the translations :  e.g. "Player {0} wins"  ->  "Player Red wins"	
		public static string MainTranslation, SecondaryTranslation;		// The callback should use and modify this variables
		public static string CallBackTerm, CallBackSecondaryTerm;		// during the callback, this will hold the FinalTerm and FinalSecondary  to know what terms are originating the translation

		#endregion

		#region Localize

		void Awake()
		{
			RegisterTargets();
			EventFindTarget(); // Finds a new target if mTarget is null. Also caches the target into the mTarget_XXX variables

			if (LocalizeOnAwake)
				OnLocalize();
		}

		void RegisterTargets()
		{
			if (EventFindTarget!=null)
				return;
			RegisterEvents_NGUI();
			RegisterEvents_DFGUI();
			RegisterEvents_UGUI();
			RegisterEvents_2DToolKit();
			RegisterEvents_TextMeshPro();
			RegisterEvents_UnityStandard();
		}

		void OnEnable()
		{
			OnLocalize ();
		}

		public void OnLocalize( bool Force = false )
		{
			if (!Force && (!enabled || !gameObject.activeInHierarchy))
				return;

			if (string.IsNullOrEmpty(LocalizationManager.CurrentLanguage))
				return;

			if (!Force && LastLocalizedLanguage==LocalizationManager.CurrentLanguage)
				return;
			LastLocalizedLanguage = LocalizationManager.CurrentLanguage;

			if (!HasTargetCache()) 
				FindTarget();

			if (!HasTargetCache()) return;

			// This are the terms actually used (will be mTerm/mSecondaryTerm or will get them from the objects if those are missing. e.g. Labels' text and font name)
			if (string.IsNullOrEmpty(FinalTerm) || string.IsNullOrEmpty(FinalSecondaryTerm))
				GetFinalTerms( out FinalTerm, out FinalSecondaryTerm );

			if (string.IsNullOrEmpty (FinalTerm) && string.IsNullOrEmpty (FinalSecondaryTerm))
				return;

			CallBackTerm = FinalTerm;
			CallBackSecondaryTerm = FinalSecondaryTerm;
			MainTranslation = LocalizationManager.GetTermTranslation (FinalTerm);
			SecondaryTranslation = LocalizationManager.GetTermTranslation (FinalSecondaryTerm);

			if (string.IsNullOrEmpty (MainTranslation) && string.IsNullOrEmpty (SecondaryTranslation))
				return;

			LocalizeCallBack.Execute( this );  // This allows scripts to modify the translations :  e.g. "Player {0} wins"  ->  "Player Red wins"

			if (LocalizationManager.IsRight2Left && !IgnoreRTL)
			{
				if (AllowMainTermToBeRTL && !string.IsNullOrEmpty(MainTranslation))   
					MainTranslation = ArabicSupport.ArabicFixer.Fix(MainTranslation);
				if (AllowSecondTermToBeRTL && !string.IsNullOrEmpty(SecondaryTranslation)) 
					SecondaryTranslation = ArabicSupport.ArabicFixer.Fix(SecondaryTranslation);
			}
			switch (PrimaryTermModifier)
			{
				case TermModification.ToUpper 		: MainTranslation = MainTranslation.ToUpper(); break;
				case TermModification.ToLower 		: MainTranslation = MainTranslation.ToLower(); break;
				case TermModification.ToUpperFirst 	: MainTranslation = GoogleTranslation.UppercaseFirst(MainTranslation); break;
				case TermModification.ToTitle 		: MainTranslation = GoogleTranslation.TitleCase(MainTranslation); break;
			}
			switch (SecondaryTermModifier)
			{
				case TermModification.ToUpper 		: SecondaryTranslation = SecondaryTranslation.ToUpper();  break;
				case TermModification.ToLower 		: SecondaryTranslation = SecondaryTranslation.ToLower();  break;
				case TermModification.ToUpperFirst 	: SecondaryTranslation = GoogleTranslation.UppercaseFirst(SecondaryTranslation); break;
				case TermModification.ToTitle 		: SecondaryTranslation = GoogleTranslation.TitleCase(SecondaryTranslation); break;
			}
			EventDoLocalize( MainTranslation, SecondaryTranslation );
		}

		#endregion

		#region Finding Target

		public bool FindTarget()
		{
			if (EventFindTarget==null)
				RegisterTargets();

			EventFindTarget();
			return HasTargetCache();
		}

		public void FindAndCacheTarget<T>( ref T targetCache, DelegateSetFinalTerms setFinalTerms, DelegateDoLocalize doLocalize, bool UseSecondaryTerm, bool MainRTL, bool SecondRTL ) where T : Component
		{
			if (mTarget!=null)
				targetCache = (mTarget as T);
			else
				mTarget = targetCache = GetComponent<T>();

			if (targetCache != null)
			{
				EventSetFinalTerms = setFinalTerms;
				EventDoLocalize = doLocalize;

				CanUseSecondaryTerm = UseSecondaryTerm;
				AllowMainTermToBeRTL 	= MainRTL;
				AllowSecondTermToBeRTL	= SecondRTL;
			}
		}

		void FindAndCacheTarget( ref GameObject targetCache, DelegateSetFinalTerms setFinalTerms, DelegateDoLocalize doLocalize, bool UseSecondaryTerm, bool MainRTL, bool SecondRTL )
		{
			if (mTarget!=targetCache && targetCache)
			{
#if UNITY_EDITOR
				DestroyImmediate (targetCache);
#else
				Destroy (targetCache);
#endif
			}

			if (mTarget!=null)
				targetCache = (mTarget as GameObject);
			else
			{
				Transform mThis = transform;
				mTarget = targetCache = (mThis.childCount<1 ? null : mThis.GetChild(0).gameObject);
			}
			if (targetCache != null)
			{
				EventSetFinalTerms = setFinalTerms;
				EventDoLocalize = doLocalize;

				CanUseSecondaryTerm = UseSecondaryTerm;
				AllowMainTermToBeRTL 	= MainRTL;
				AllowSecondTermToBeRTL	= SecondRTL;
			}
		}

		bool HasTargetCache() { return EventDoLocalize!=null; }

		#endregion

		#region Finding Term
		
		// Returns the term that will actually be translated
		// its either the Term value in this class or the text of the label if there is no term
		public void GetFinalTerms( out string PrimaryTerm, out string SecondaryTerm )
		{
			if (!mTarget && !HasTargetCache())
				FindTarget();

			PrimaryTerm 	= string.Empty;
			SecondaryTerm 	= string.Empty;



			// if either the primary or secondary term is missing, get them. (e.g. from the label's text and font name)
			if (mTarget!=null && (string.IsNullOrEmpty(mTerm) || string.IsNullOrEmpty(mTermSecondary)))
			{
				if (EventSetFinalTerms!=null)
					EventSetFinalTerms(mTerm, mTermSecondary,	out PrimaryTerm, out SecondaryTerm );  // If no term is set, use the text from the label, the spritename from the Sprite, etc
			}

			// If there are values already set, go with those
			if (!string.IsNullOrEmpty(mTerm)) 
				PrimaryTerm = mTerm;

			if (!string.IsNullOrEmpty(mTermSecondary))
				SecondaryTerm = mTermSecondary;
		}
		
		void SetFinalTerms( string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm )
		{
			PrimaryTerm = Main;
			SecondaryTerm = Secondary;
		}
		
		#endregion

		#region Misc

		public void SetTerm (string primary, string secondary=null)
		{
			if (!string.IsNullOrEmpty(primary))
				FinalTerm = Term = primary;
			if (!string.IsNullOrEmpty(secondary))
				FinalSecondaryTerm = SecondaryTerm = secondary;

			OnLocalize (true);
		}

		T GetSecondaryTranslatedObj<T>( ref string MainTranslation, ref string SecondaryTranslation ) where T: Object
		{
			//--[ Allow main translation to override Secondary ]-------------------
			string secondary = null;
			DeserializeTranslation(MainTranslation, out MainTranslation, out secondary);

			T obj = GetObject<T>(secondary);
			if (obj==null)
				obj = GetObject<T>(SecondaryTranslation);

			return obj;
		}

		T GetObject<T>( string Translation ) where T: Object
		{
			if (string.IsNullOrEmpty (Translation))
				return null;
			T obj = GetTranslatedObject<T>(Translation);
			
			if (obj==null)
			{
				// Remove path and search by name
				int Index = Translation.LastIndexOfAny("/\\".ToCharArray());
				if (Index>=0)
				{
					Translation = Translation.Substring(Index+1);
					obj = GetTranslatedObject<T>(Translation);
				}
			}
			return obj;
		}

		T GetTranslatedObject<T>( string Translation ) where T: Object
		{
			T Obj = FindTranslatedObject<T>(Translation);
			/*if (Obj == null) 
				return null;
			
			if ((Obj as T) != null) 
				return Obj as T;
			
			// If the found Obj is not of type T, then try finding a component inside
			if (Obj as Component != null)
				return (Obj as Component).GetComponent(typeof(T)) as T;
			
			if (Obj as GameObject != null)
				return (Obj as GameObject).GetComponent(typeof(T)) as T;
			*/
			return Obj;
		}


		// translation format: "[secondary]value"   [secondary] is optional
		void DeserializeTranslation( string translation, out string value, out string secondary )
		{
			if (!string.IsNullOrEmpty(translation) && translation.Length>1 && translation[0]=='[')
			{
				int Index = translation.IndexOf(']');
				if (Index>0)
				{
					secondary = translation.Substring(1, Index-1);
					value = translation.Substring(Index+1);
					return;
				}
			}
			value = translation;
			secondary = string.Empty;
		}
		
		public T FindTranslatedObject<T>( string value ) where T : UnityEngine.Object
		{
			if (string.IsNullOrEmpty(value))
				return null;

			if (TranslatedObjects!=null)
			for (int i=0, imax=TranslatedObjects.Length; i<imax; ++i)
				if (TranslatedObjects[i] as T !=null && value==TranslatedObjects[i].name)
					return TranslatedObjects[i] as T;

			T obj = LocalizationManager.FindAsset(value) as T;
			if (obj)
				return obj;

			obj = ResourceManager.pInstance.GetAsset<T>(value);
			return obj;
		}

		bool HasTranslatedObject( Object Obj )
		{
			if (System.Array.IndexOf (TranslatedObjects, Obj) >= 0) 
				return true;
			return ResourceManager.pInstance.HasAsset(Obj);

		}

		#endregion
	
		#region Utilities
		// This can be used to set the language when a button is clicked
		public void SetGlobalLanguage( string Language )
		{
			LocalizationManager.CurrentLanguage = Language;
		}
		#endregion
	}
}