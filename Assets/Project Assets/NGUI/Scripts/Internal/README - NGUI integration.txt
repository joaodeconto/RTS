Hey!

In order to integrate SoundManagerPro into NGUI, you have to update your NGUITools.cs and UIPlaySound files that are included in this package.

You also have to import the editor file UIPlaySoundEditorSMP, to add extra features to UIPlaySound in the editor.

Free version users, THIS WILL NOT WORK FOR YOU. NGUITools is embedded in a DLL so you can't access that.

However, for everyone else, this will work fine.  If you have an older version of NGUI you can just Diff the files for the differences.

I replaced this block of code in NGUITools.cs:


				/*    REPLACED!!!!
				AudioSource source = mListener.audio;
				if (source == null) source = mListener.gameObject.AddComponent<AudioSource>();
				source.pitch = pitch;
				source.PlayOneShot(clip, volume);
				*/
				return SoundManager.PlaySFX(mListener.gameObject, clip, false, 0f, volume * SoundManager.GetVolumeSFX(), pitch * SoundManager.GetPitchSFX());

I added this code to the UIPlaySound.cs file:
				// SMP Additions
				[HideInInspector]
				public antilunchbox.ClipType clipType = antilunchbox.ClipType.AudioClip;
				[HideInInspector]
				public string clipName;
				[HideInInspector]
				public string groupName;
				private AudioClip _audioClip {
					get {
						switch(clipType)
						{
						case antilunchbox.ClipType.ClipFromSoundManager:
							return SoundManager.Load(clipName);
						case antilunchbox.ClipType.ClipFromGroup:
							return SoundManager.LoadFromGroup(groupName);
						case antilunchbox.ClipType.AudioClip:
						default:
							return audioClip;
						}
					}
				}
				
And replaced all instances of this line in UIPlaySound.cs:
				/*    REPLACED!!!!
				NGUITools.PlaySound(audioClip, volume, pitch);
				*/
				NGUITools.PlaySound(_audioClip, volume, pitch);
				
And Voila! It works!

Happy Game Making,
AntiLunchBox Studios