using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorVariableStorage : ScriptableObject {
	/*													 /
	 * 					DO NOT MODIFY					 /
	 * 													*/
	
	/// EDITOR variables. DO NOT TOUCH.
	public List<AudioClip> soundsToAdd = new List<AudioClip>();
	/// EDITOR variables. DO NOT TOUCH.
	public string levelToAdd;
	/// EDITOR variables. DO NOT TOUCH.
	public bool isCustom;
	/// EDITOR variables. DO NOT TOUCH.
	public SoundManager.PlayMethod playMethodToAdd;
	/// EDITOR variables. DO NOT TOUCH.
	public float minDelayToAdd;
	/// EDITOR variables. DO NOT TOUCH.
	public float maxDelayToAdd;
	/// EDITOR variables. DO NOT TOUCH.
	public float delayToAdd;
	/// EDITOR variables. DO NOT TOUCH.
	public bool viewAll {
		get {
			return _viewAll;
		} set {
			_viewAll = value;
			List<string> keys = new List<string>();
	        foreach (DictionaryEntry de in songStatus)
	            keys.Add(de.Key.ToString());
	
	        foreach(string key in keys)
	        {
				if(_viewAll)
				{
	            	songStatus[key] = VIEW;
				} else {
					songStatus[key] = HIDE;
				}
	        }
		}
	}
	private bool _viewAll = false;
	
	/// EDITOR variables. DO NOT TOUCH.
	public bool showNewSoundConnectionButton = true;
	/// EDITOR variables. DO NOT TOUCH.
	public int levelIndex = 0;
	[SerializeField]
	/// EDITOR variables. DO NOT TOUCH.
	public Hashtable songStatus = new Hashtable();
	
	/// EDITOR variables. DO NOT TOUCH.
	public bool isEditing = false;
	/// EDITOR variables. DO NOT TOUCH.
	public AudioClip editAddClip;
	/// EDITOR variables. DO NOT TOUCH.
	public AudioClip editAddClipFromSelector;
	// EDITOR varialbes. DO NOT TOUCH
	public AudioClip editAddSFX;
	
	/// EDITOR variables. DO NOT TOUCH.
	public float minDelayToEdit;
	/// EDITOR variables. DO NOT TOUCH.
	public float maxDelayToEdit;
	/// EDITOR variables. DO NOT TOUCH.
	public float delayToEdit;
	
	/// EDITOR variables. DO NOT TOUCH.
	public const string VIEW = "view";
	/// EDITOR variables. DO NOT TOUCH.
	public const string EDIT = "edit";
	/// EDITOR variables. DO NOT TOUCH.
	public const string HIDE = "hide";
	
	/// EDITOR variables. DO NOT TOUCH.
	public Texture2D titleBar;
	/// EDITOR variables. DO NOT TOUCH.
	public Texture2D footer;
	/// EDITOR variables. DO NOT TOUCH.
	public Texture2D icon;
	/// EDITOR variables. DO NOT TOUCH.
	public bool helpOn = false;
	/// EDITOR variables. DO NOT TOUCH.
	public string groupToAdd;
	/// EDITOR variables. DO NOT TOUCH.
	public int groupIndex = 0;
	/// EDITOR variables. DO NOT TOUCH.
	public bool showInfo = true;
	/// EDITOR variables. DO NOT TOUCH.
	public bool showDev = true;
	/// EDITOR variables. DO NOT TOUCH.
	public bool showList = true;
	/// EDITOR variables. DO NOT TOUCH.
	public bool showAdd = true;
	/// EDITOR variables. DO NOT TOUCH.
	public bool showSFX = true;
	
	public void Hide()
	{
		if(titleBar)
			titleBar.hideFlags = HideFlags.HideAndDontSave;
		if(footer)
			footer.hideFlags = HideFlags.HideAndDontSave;
		if(editAddClip)
			editAddClip.hideFlags = HideFlags.HideAndDontSave;
		if(editAddClipFromSelector)
			editAddClipFromSelector.hideFlags = HideFlags.HideAndDontSave;
		if(editAddSFX)
			editAddSFX.hideFlags = HideFlags.HideAndDontSave;
		if(soundsToAdd != null && soundsToAdd.Count != 0)
		{
			foreach(AudioClip sound in soundsToAdd)
				sound.hideFlags = HideFlags.HideAndDontSave;
		}
		if(icon)
			icon.hideFlags = HideFlags.HideAndDontSave;
	}
	
	public void ClearStorage()
	{
		if(editAddClip)
			editAddClip = null;
		if(editAddClipFromSelector)
			editAddClipFromSelector = null;
		if(editAddSFX)
			editAddSFX = null;
		if(soundsToAdd != null && soundsToAdd.Count != 0)
		{
			soundsToAdd.Clear();
			soundsToAdd = null;
		}
		if(titleBar)
			titleBar = null;
		if(footer)
			footer = null;
		if(icon)
			icon = null;
	}
}
