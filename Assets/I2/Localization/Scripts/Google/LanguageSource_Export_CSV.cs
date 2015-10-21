using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace I2.Loc
{
	public partial class LanguageSource
	{
		public string Export_CSV( string Category, char Separator = ',' )
		{
			StringBuilder Builder = new StringBuilder();
			
			int nLanguages = (mLanguages.Count);
			Builder.AppendFormat ("Key{0}Type{0}Desc", Separator);

			foreach (LanguageData langData in mLanguages)
			{
				Builder.Append (Separator);
				AppendString ( Builder, GoogleLanguages.GetCodedLanguage(langData.Name, langData.Code), Separator );
			}
			Builder.Append ("\n");
			
			foreach (TermData termData in mTerms)
			{
				string Term;

				if (string.IsNullOrEmpty(Category) || (Category==EmptyCategory && termData.Term.IndexOfAny(CategorySeparators)<0))
					Term = termData.Term;
				else
				if (termData.Term.StartsWith(Category) && Category!=termData.Term)
					Term = termData.Term.Substring(Category.Length+1);
				else
					continue;	// Term doesn't belong to this category

				AppendTerm( Builder, nLanguages, Term, termData, null, termData.Languages, termData.Languages_Touch, Separator, (int)TranslationFlag.AutoTranslated_Normal, (int)TranslationFlag.AutoTranslated_Touch);

				if (termData.HasTouchTranslations())
					AppendTerm( Builder, nLanguages, Term, termData, "[touch]", termData.Languages_Touch, null, Separator, (int)TranslationFlag.AutoTranslated_Touch, (int)TranslationFlag.AutoTranslated_Normal);
			}
			return Builder.ToString();
		}

		static void AppendTerm( StringBuilder Builder, int nLanguages, string Term, TermData termData, string prefix, string[] aLanguages, string[] aSecLanguages, char Separator, int FlagBitMask, int SecFlagBitMask )
		{
			//--[ Key ] --------------				
			AppendString( Builder, Term, Separator );

			if (!string.IsNullOrEmpty(prefix))
				Builder.Append( prefix );
			
			//--[ Type and Description ] --------------
			Builder.Append (Separator);
			Builder.Append (termData.TermType.ToString());
			Builder.Append (Separator);
			AppendString(Builder, termData.Description, Separator);
			
			//--[ Languages ] --------------
			for (int i=0; i<Mathf.Min (nLanguages, aLanguages.Length); ++i)
			{
				Builder.Append (Separator);

				string s = aLanguages[i];
				bool isAutoTranslated = ((termData.Flags[i]&FlagBitMask)>0);
				if (string.IsNullOrEmpty(s) && aSecLanguages!=null)
				{
					s = aSecLanguages[i];
					isAutoTranslated = ((termData.Flags[i]&SecFlagBitMask)>0);
				}

				if (isAutoTranslated) Builder.Append("[i2auto]");
				AppendString(Builder, s, Separator);
			}
			Builder.Append ("\n");
		}
		
		
		static void AppendString( StringBuilder Builder, string Text, char Separator )
		{
			if (string.IsNullOrEmpty(Text))
				return;
			Text = Text.Replace ("\\n", "\n");
			if (Text.IndexOfAny((Separator+"\n\"").ToCharArray())>=0)
			{
				Text = Text.Replace("\"", "\"\"");
				Builder.AppendFormat("\"{0}\"", Text);
			}
			else 
				Builder.Append(Text);
		}

	}
}