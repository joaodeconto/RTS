using UnityEngine;
using System.Collections;

namespace I2.Loc
{
	#if TextMeshPro 
	public partial class Localize
	{
		TMPro.TextMeshPro 	mTarget_TMPLabel;
		#if TMProBeta
		TMPro.TextMeshProUGUI mTarget_TMPUGUILabel;
		#endif

		public void RegisterEvents_TextMeshPro()
		{
			EventFindTarget += FindTarget_TMPLabel;
			#if TMProBeta
			EventFindTarget += FindTarget_TMPUGUILabel;
			#endif
		}
		
		void FindTarget_TMPLabel() 	{ FindAndCacheTarget (ref mTarget_TMPLabel, SetFinalTerms_TMPLabel, DoLocalize_TMPLabel, true, true, false); }

		#if TMProBeta
		void FindTarget_TMPUGUILabel() 	{ FindAndCacheTarget (ref mTarget_TMPUGUILabel, SetFinalTerms_TMPUGUILabel, DoLocalize_TMPUGUILabel, true, true, false); }
		#endif
		void SetFinalTerms_TMPLabel(string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
		{
			string second = (mTarget_TMPLabel.font!=null ? mTarget_TMPLabel.font.name : string.Empty);
			SetFinalTerms (mTarget_TMPLabel.text, second,		out primaryTerm, out secondaryTerm);
		}

		#if TMProBeta
		void SetFinalTerms_TMPUGUILabel(string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
		{
			string second = (mTarget_TMPUGUILabel.font!=null ? mTarget_TMPUGUILabel.font.name : string.Empty);
			SetFinalTerms (mTarget_TMPUGUILabel.text, second,		out primaryTerm, out secondaryTerm);
		}
		#endif
		
		public void DoLocalize_TMPLabel(string MainTranslation, string SecondaryTranslation)
		{
			if (!string.IsNullOrEmpty(MainTranslation) && mTarget_TMPLabel.text != MainTranslation)
			{
				mTarget_TMPLabel.text = MainTranslation;
				mTarget_TMPLabel.ForceMeshUpdate();
			}

			//--[ Localize Font Object ]----------
			TMPro.TextMeshProFont newFont = GetSecondaryTranslatedObj<TMPro.TextMeshProFont>(ref MainTranslation, ref SecondaryTranslation);
			if (newFont!=null && mTarget_TMPLabel.font != newFont)
					mTarget_TMPLabel.font = newFont;
		}

		#if TMProBeta
		public void DoLocalize_TMPUGUILabel(string MainTranslation, string SecondaryTranslation)
		{
			if (!string.IsNullOrEmpty(MainTranslation) && mTarget_TMPUGUILabel.text != MainTranslation)
			{
				mTarget_TMPUGUILabel.text = MainTranslation;
				mTarget_TMPUGUILabel.ForceMeshUpdate();
			}
			
			//--[ Localize Font Object ]----------
			TMPro.TextMeshProFont newFont = GetSecondaryTranslatedObj<TMPro.TextMeshProFont>(ref MainTranslation, ref SecondaryTranslation);
			if (newFont!=null && mTarget_TMPUGUILabel.font != newFont)
					mTarget_TMPUGUILabel.font = newFont;
		}
		#endif

	}
	#else
	public partial class Localize
	{
		public static void RegisterEvents_TextMeshPro()
		{
		}
	}
	#endif	
}