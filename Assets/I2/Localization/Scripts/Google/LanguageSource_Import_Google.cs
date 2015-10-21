using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace I2.Loc
{
	public partial class LanguageSource
	{
		#region Variables

		public string Google_WebServiceURL;
		public string Google_SpreadsheetKey;
		public string Google_SpreadsheetName;
		public string Google_LastUpdatedVersion;

		public enum eGoogleUpdateFrequency { Always, Never, Daily, Weekly, Monthly }
		public eGoogleUpdateFrequency GoogleUpdateFrequency = eGoogleUpdateFrequency.Weekly;

		public event Action<LanguageSource> Event_OnSourceUpdateFromGoogle;
		
		#endregion

		#region Connection to Web Service 

		public void Import_Google( bool ForceUpdate = false)
		{
			if (GoogleUpdateFrequency==eGoogleUpdateFrequency.Never)
				return;

			if (!Application.isPlaying)
				return;

			string PlayerPrefName = GetSourcePlayerPrefName();
			if (!ForceUpdate && GoogleUpdateFrequency!=eGoogleUpdateFrequency.Always)
			{
				string sTimeOfLastUpdate = PlayerPrefs.GetString("LastGoogleUpdate_"+PlayerPrefName, "");
				DateTime TimeOfLastUpdate;
				if (DateTime.TryParse(sTimeOfLastUpdate, out TimeOfLastUpdate))
				{
					double TimeDifference = (DateTime.Now-TimeOfLastUpdate).TotalDays;
					switch (GoogleUpdateFrequency)
					{
						case eGoogleUpdateFrequency.Daily 	: if (TimeDifference<1) return;
																break;
						case eGoogleUpdateFrequency.Weekly 	: if (TimeDifference<8) return;
																break;
						case eGoogleUpdateFrequency.Monthly : if (TimeDifference<31) return;
																break;
					}
				}
			}
			PlayerPrefs.SetString("LastGoogleUpdate_"+PlayerPrefName, DateTime.Now.ToString());

			//--[ Import saved data ]-----------------
			string I2SavedData = PlayerPrefs.GetString("I2Source_"+Google_SpreadsheetKey, "");
			if (!string.IsNullOrEmpty(I2SavedData))
			{
				//Debug.Log ("Use Saved data " + I2SavedData);
				Import_Google_Result(I2SavedData, eSpreadsheetUpdateMode.Replace);
			}

			//--[ Checking google for updated data ]-----------------
			CoroutineManager.pInstance.StartCoroutine(Import_Google_Coroutine());
		}

		string GetSourcePlayerPrefName()
		{
			// If its a global source, use its name, otherwise, use the name and the level it is in
			if (System.Array.IndexOf(LocalizationManager.GlobalSources, name)>=0)
				return name;
			else
				return Application.loadedLevelName+"_"+name;
		}

		IEnumerator Import_Google_Coroutine()
		{
			WWW www = Import_Google_CreateWWWcall();
			if (www==null)
				yield break;

			while (!www.isDone)
				yield return null;

			//Debug.Log ("Google Result: " + www.text);
			if (string.IsNullOrEmpty(www.error) && www.text != "\"\"")
			{
				PlayerPrefs.SetString("I2Source_"+Google_SpreadsheetKey, www.text);
				PlayerPrefs.Save();

				Import_Google_Result(www.text, eSpreadsheetUpdateMode.Replace);
				if (Event_OnSourceUpdateFromGoogle!=null)
					Event_OnSourceUpdateFromGoogle(this);

				LocalizationManager.LocalizeAll();
				Debug.Log ("Done Google Sync '" + www.text+"'");
			}
			else
				Debug.Log ("Language Source was up-to-date with Google Spreadsheet");
		}

		public WWW Import_Google_CreateWWWcall( bool ForceUpdate = false )
		{
			#if UNITY_WEBPLAYER
			Debug.Log ("Contacting google translation is not yet supported on WebPlayer" );
			return null;
			#else

			if (!HasGoogleSpreadsheet())
				return null;

			string query =  string.Format("{0}?key={1}&action=GetLanguageSource&version={2}", 
			                              Google_WebServiceURL,
			                              Google_SpreadsheetKey,
			                              ForceUpdate ? "0" : Google_LastUpdatedVersion);
			WWW www = new WWW(query);
			return www;
			#endif
		}

		public bool HasGoogleSpreadsheet()
		{
			return !string.IsNullOrEmpty(Google_WebServiceURL) && !string.IsNullOrEmpty(Google_SpreadsheetKey);
		}

		public string Import_Google_Result( string JsonString, eSpreadsheetUpdateMode UpdateMode )
		{
			string ErrorMsg = string.Empty;
            if (string.IsNullOrEmpty(JsonString) || JsonString == "\"\"")
			{
				Debug.Log ("Language Source was up to date");
				return ErrorMsg;
			}

			if (UpdateMode == eSpreadsheetUpdateMode.Replace)
				ClearAllData();

			int idxV = JsonString.IndexOf("version=");
			int idxSV = JsonString.IndexOf("script_version=");
			if (idxV<0 || idxSV<0)
			{
				return "Invalid Response from Google, Most likely the WebService needs to be updated";
			}

			idxV += "version=".Length;
			idxSV += "script_version=".Length;

			Google_LastUpdatedVersion = JsonString.Substring(idxV, JsonString.IndexOf(",",idxV)-idxV);
			var version = int.Parse( JsonString.Substring(idxSV, JsonString.IndexOf(",",idxSV)-idxSV));

			if (version<LocalizationManager.GetRequiredWebServiceVersion())
			{
				return "The current Google WebService is not supported.\nPlease, delete the WebService from the Google Drive and Install the latest version.";
			}

			int CSVstartIdx = JsonString.IndexOf("[i2category]");
			while (CSVstartIdx>0)
			{
				CSVstartIdx += "[i2category]".Length;
				int endCat = JsonString.IndexOf("[/i2category]", CSVstartIdx);
				string category = JsonString.Substring(CSVstartIdx, endCat-CSVstartIdx);
				endCat += "[/i2category]".Length;

				int endCSV = JsonString.IndexOf("[/i2csv]", endCat);
				string csv = JsonString.Substring(endCat, endCSV-endCat);

				CSVstartIdx = JsonString.IndexOf("[i2category]", endCSV);

				Import_CSV( category, csv, UpdateMode, ',' );
				
				// Only the first CSV should clear the Data
				if (UpdateMode == eSpreadsheetUpdateMode.Replace)
					UpdateMode = eSpreadsheetUpdateMode.Merge;
			}

#if UNITY_EDITOR
			if (!string.IsNullOrEmpty(ErrorMsg))
				UnityEditor.EditorUtility.SetDirty(this);
#endif
			return ErrorMsg;
		}

		#endregion
	}
}