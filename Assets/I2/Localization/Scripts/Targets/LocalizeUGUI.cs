//#define UGUI

using UnityEngine;
using System.Collections;

namespace I2.Loc
{
	#if UGUI
	public partial class Localize
	{
		#region Cache

		UnityEngine.UI.Text 		mTarget_uGUI_Text;
		UnityEngine.UI.Image 		mTarget_uGUI_Image;
		UnityEngine.UI.RawImage 	mTarget_uGUI_RawImage;

		public void RegisterEvents_UGUI()
		{
			EventFindTarget += FindTarget_uGUI_Text;
			EventFindTarget += FindTarget_uGUI_Image;
			EventFindTarget += FindTarget_uGUI_RawImage;
		}
		
		#endregion
		
		#region Find Target
		
		void FindTarget_uGUI_Text() 	{ FindAndCacheTarget (ref mTarget_uGUI_Text, SetFinalTerms_uGUI_Text, DoLocalize_uGUI_Text, true, true, false); }
		void FindTarget_uGUI_Image() 	{ FindAndCacheTarget (ref mTarget_uGUI_Image, SetFinalTerms_uGUI_Image, DoLocalize_uGUI_Image, false, false, false); }
		void FindTarget_uGUI_RawImage()	{ FindAndCacheTarget (ref mTarget_uGUI_RawImage, SetFinalTerms_uGUI_RawImage, DoLocalize_uGUI_RawImage, false, false, false); }

		#endregion
		
		#region SetFinalTerms
		
		void SetFinalTerms_uGUI_Text(string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
		{
			string second = (mTarget_uGUI_Text.font!=null ? mTarget_uGUI_Text.font.name : string.Empty);
			SetFinalTerms (mTarget_uGUI_Text.text, second,		out primaryTerm, out secondaryTerm);
			
		}
		
		public void SetFinalTerms_uGUI_Image(string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
		{
			SetFinalTerms (mTarget_uGUI_Image.mainTexture.name, 	null,	out primaryTerm, out secondaryTerm);
		}

		public void SetFinalTerms_uGUI_RawImage(string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
		{
			SetFinalTerms (mTarget_uGUI_RawImage.texture.name, 	null,	out primaryTerm, out secondaryTerm);
		}

		#endregion
		
		#region DoLocalize

		public static T FindInParents<T> (Transform tr) where T : Component
		{
			if (!tr) 
				return null;

			T comp = tr.GetComponent<T>();
			while (!comp && tr)
			{
				comp = tr.GetComponent<T>();
				tr = tr.parent;
			}
			return comp;
		}
		
		public void DoLocalize_uGUI_Text(string MainTranslation, string SecondaryTranslation)
		{
			if (!string.IsNullOrEmpty(MainTranslation) && mTarget_uGUI_Text.text != MainTranslation) 
				mTarget_uGUI_Text.text = MainTranslation;

			//--[ Localize Font Object ]----------
			Font newFont = GetSecondaryTranslatedObj<Font>(ref MainTranslation, ref SecondaryTranslation);
			if (newFont!=null && newFont!=mTarget_uGUI_Text.font) 
				mTarget_uGUI_Text.font = newFont;
		}

		public void DoLocalize_uGUI_Image(string MainTranslation, string SecondaryTranslation)
		{
			Sprite Old = mTarget_uGUI_Image.sprite;
			if (Old==null || Old.name!=MainTranslation)
				mTarget_uGUI_Image.sprite = FindTranslatedObject<Sprite>(MainTranslation);
			
			// If the old value is not in the translatedObjects, then unload it as it most likely was loaded from Resources
			//if (!HasTranslatedObject(Old))
			//	Resources.UnloadAsset(Old);
		}

		public void DoLocalize_uGUI_RawImage(string MainTranslation, string SecondaryTranslation)
		{
			Texture Old = mTarget_uGUI_RawImage.texture;
			if (Old==null || Old.name!=MainTranslation)
				mTarget_uGUI_RawImage.texture = FindTranslatedObject<Texture>(MainTranslation);
			
			// If the old value is not in the translatedObjects, then unload it as it most likely was loaded from Resources
			//if (!HasTranslatedObject(Old))
			//	Resources.UnloadAsset(Old);
		}
		#endregion	
	}
	#else
	public partial class Localize
	{
		public static void RegisterEvents_UGUI()
		{
		}
	}
	#endif
}

