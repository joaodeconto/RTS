using UnityEngine;
using System.Collections;

[System.Serializable]
public class SFXGroup : ScriptableObject {
	public string groupName;
	public int specificCapAmount;
	
	/// <summary>
	/// Initialize a SFX Group.  'name' is the name of the group, and 'capAmount' is a custom SFX cap for that group.
	/// Use -1 as the cap amount to use the default global cap amount, and use 0 if you don't want the group to use a specific cap amount at all.
	/// The specific cap amount will only be respected when using SoundManager.PlayCappedSFX
	/// </summary>
	public void Initialize(string name, int capAmount)
	{
		groupName = name;
		specificCapAmount = capAmount;
	}
	
	/// <summary>
	/// Initialize a SFX Group.  'name' is the name of the group
	/// </summary>
	public void Initialize(string name)
	{
		Initialize(name, 0);
	}
}
