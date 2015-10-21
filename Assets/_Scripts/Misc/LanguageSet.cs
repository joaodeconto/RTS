using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;

public class LanguageSet : MonoBehaviour {

	public UIGrid     languageTable;
	public GameObject  btnLanguage;
	private List<string> langList = new List<string>();

	// Use this for initialization
	void Start () {

		LanguageTable();
	}

	public void LanguageTable(){

		langList = LocalizationManager.GetAllLanguages();
		DefaultCallbackButton dcb;
		foreach (string lang in langList)
		{
			GameObject btnLang = NGUITools.AddChild(languageTable.gameObject, btnLanguage);
			btnLang.transform.FindChild("Label").gameObject.GetComponent<UILabel>().text = lang;
			Hashtable ht = new Hashtable();
			ht["languageName"] = lang;
			dcb = btnLang.GetComponent<DefaultCallbackButton>();
			dcb.Init ( null, (ht_hud) =>{ 
				SelectLanguagePop(ht);
			});
		}
		languageTable.sorting = UIGrid.Sorting.Alphabetic;
		languageTable.repositionNow = true;
	}

	public void SelectLanguagePop(Hashtable ht){

		string lang 		 = (string)ht["languageName"];

		LocalizationManager.CurrentLanguage = lang;
	}
	
	// Update is called once per frame
	public void Close () {
		transform.parent.gameObject.SetActive(false);
	
	}
}
