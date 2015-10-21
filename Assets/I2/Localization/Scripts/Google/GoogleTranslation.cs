using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace I2.Loc
{
	public static class GoogleTranslation
	{
		// LanguageCodeFrom can be "auto"
		// After the translation is returned from Google, it will call OnTranslationReady(TranslationResult)
		// TranslationResult will be null if translatio failed
		public static void Translate( string text, string LanguageCodeFrom, string LanguageCodeTo, Action<string> OnTranslationReady )
		{
			WWW www = GetTranslationWWW( text, LanguageCodeFrom, LanguageCodeTo );
			CoroutineManager.pInstance.StartCoroutine(WaitForTranslation(www, OnTranslationReady, text));
		}
		
		static IEnumerator WaitForTranslation(WWW www, Action<string> OnTranslationReady, string OriginalText)
		{
			yield return www;

			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.LogError (www.error);
				OnTranslationReady(string.Empty);
			}
			else
			{
				string Translation = ParseTranslationResult(www.text, OriginalText);
				OnTranslationReady( Translation );
			}
		}
		
		// Querry google for the translation and waits until google returns
		public static string ForceTranslate ( string text, string LanguageCodeFrom, string LanguageCodeTo )
		{
			WWW www = GetTranslationWWW( text, LanguageCodeFrom, LanguageCodeTo );
			while (!www.isDone);
			
			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.LogError ("-- " + www.error);
				foreach(KeyValuePair<string, string> entry in www.responseHeaders) 
					Debug.Log(entry.Value + "=" + entry.Key);
				
				return string.Empty;
			}
			else
			{
				return ParseTranslationResult(www.text, text);
			}
		}

		static WWW GetTranslationWWW(  string text, string LanguageCodeFrom, string LanguageCodeTo )
		{
			LanguageCodeFrom = GoogleLanguages.GetGoogleLanguageCode(LanguageCodeFrom);
			LanguageCodeTo = GoogleLanguages.GetGoogleLanguageCode(LanguageCodeTo);

			text = text.ToLower();
			string url = string.Format ("http://www.google.com/translate_t?hl=en&vi=c&ie=UTF8&oe=UTF8&submit=Translate&langpair={0}|{1}&text={2}", LanguageCodeFrom, LanguageCodeTo, Uri.EscapeUriString( text ));
			WWW www = new WWW(url);
			return www;
		}
		
		static string ParseTranslationResult( string html, string OriginalText )
		{
			try
			{
				// This is a Hack for reading Google Translation while Google doens't change their response format
				int iStart = html.IndexOf("TRANSLATED_TEXT") + "TRANSLATED_TEXT='".Length;
				int iEnd = html.IndexOf("';INPUT_TOOL_PATH", iStart);
				
				string Translation = html.Substring( iStart, iEnd-iStart);
				
				// Convert to normalized HTML
				Translation = System.Text.RegularExpressions.Regex.Replace(Translation,
				                                                           @"\\x([a-fA-F0-9]{2})",
				                                                           match => char.ConvertFromUtf32(Int32.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber)));
				
				// Convert ASCII Characters
				Translation = System.Text.RegularExpressions.Regex.Replace(Translation,
				                                                           @"&#(\d+);",
				                                                           match => char.ConvertFromUtf32(Int32.Parse(match.Groups[1].Value)));
				
				Translation = Translation.Replace("<br>", "\n");

				if (OriginalText.ToUpper()==OriginalText)
					Translation = Translation.ToUpper();
				else
				if (UppercaseFirst(OriginalText)==OriginalText)
					Translation = UppercaseFirst(Translation);
				else
				if (TitleCase(OriginalText)==OriginalText)
					Translation = TitleCase(Translation);

				return Translation;
			}
			catch (System.Exception ex) 
			{ 
				Debug.LogError(ex.Message); 
				return string.Empty;
			}
		}

		public static string UppercaseFirst(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			char[] a = s.ToLower().ToCharArray();
			a[0] = char.ToUpper(a[0]);
			return new string(a);
		}
		public static string TitleCase(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}

			#if NETFX_CORE
			s[0] = char.ToUpper(s[0]);
			for (int i = 1, imax=s.Length; i<imax; ++i)
			{
				if (char.IsWhiteSpace(s[i - 1]))
					s[i] = char.ToUpper(s[i]);
				else
					s[i] = char.ToLower(s[i]);
			}
			return s;
			#else
			return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);
			#endif
		}
	}
}

