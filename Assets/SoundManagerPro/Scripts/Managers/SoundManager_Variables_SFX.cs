using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class SoundManager : Singleton<SoundManager> {

	// Path to folder where SFX are held in resources
	public string RESOURCES_PATH = "Sounds/SFX";

	// List of local AudioClip SFXs added in inspector or through StoreSFX()
	public List<AudioClip> storedSFXs = new List<AudioClip>();

	// List of SFX Objects the SoundManager pools
	public List<GameObject> ownedSFXObjects = new List<GameObject>();

	// List of other gameobjects with SFX attached
	public List<GameObject> unOwnedSFXObjects = new List<GameObject>();

	// Dictionary of instance ID to cappedID to keep track of capped SFX
	public Dictionary<int, string> cappedSFXObjects = new Dictionary<int, string>();
	
	// List of SFX groups
	public List<SFXGroup> sfxGroups = new List<SFXGroup>();
	
	// Map of clip names to group names (dictionaries and hashtables are not supported for serialization)
	public List<string> clipToGroupKeys = new List<string>();
	public List<string> clipToGroupValues = new List<string>();

	public int mCurrentOwnedSFXObject;
	public bool offTheSFX = false;
	public int CAP_AMOUNT = 3;

	public float volumeSFX {
		get{
			return _volumeSFX;
		} set {
			foreach(GameObject ownedSFXObject in Instance.ownedSFXObjects)
			{
				if(ownedSFXObject != null)
					if(ownedSFXObject.audio != null)
						ownedSFXObject.audio.volume = value;
			}
			foreach(GameObject unOwnedSFXObject in Instance.unOwnedSFXObjects)
			{
				if(unOwnedSFXObject != null)
					if(unOwnedSFXObject.audio != null)
						unOwnedSFXObject.audio.volume = value;
			}
			_volumeSFX = value;
		}
	}
	private float _volumeSFX = 1f;

	public float pitchSFX {
		get{
			return _pitchSFX;
		} set {
			foreach(GameObject ownedSFXObject in Instance.ownedSFXObjects)
			{
				if(ownedSFXObject != null)
					if(ownedSFXObject.audio != null)
						ownedSFXObject.audio.pitch = value;
			}
			foreach(GameObject unOwnedSFXObject in Instance.unOwnedSFXObjects)
			{
				if(unOwnedSFXObject != null)
					if(unOwnedSFXObject.audio != null)
						unOwnedSFXObject.audio.pitch = value;
			}
			_pitchSFX = value;
		}
	}
	private float _pitchSFX = 1f;

	public float maxSFXVolume {
		get{
			return _maxSFXVolume;
		} set {
			_maxSFXVolume = value;
		}
	}
	private float _maxSFXVolume = 1f;

	public bool mutedSFX {
		get {
			return _mutedSFX;
		} set {
			foreach(GameObject ownedSFXObject in Instance.ownedSFXObjects)
			{
				if(ownedSFXObject != null)
					if(ownedSFXObject.audio != null)
						if(value)
							ownedSFXObject.audio.mute = value;
						else
							if(Instance.offTheSFX)
								ownedSFXObject.audio.mute = value;
			}
			foreach(GameObject unOwnedSFXObject in Instance.unOwnedSFXObjects)
			{
				if(unOwnedSFXObject != null)
					if(unOwnedSFXObject.audio != null)
						if(value)
							unOwnedSFXObject.audio.mute = value;
						else
							if(Instance.offTheSFX)
								unOwnedSFXObject.audio.mute = value;
			}
			_mutedSFX = value;
		}
	}
	private bool _mutedSFX = false;
}
