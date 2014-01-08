Hey!

In order to integrate SoundManagerPro into NGUI, you have to update your NGUITools.cs file that is included in this package.

Free version users, THIS WILL NOT WORK FOR YOU. NGUITools is embedded in a DLL so you can't access that.

However, for everyone else, this will work fine.  If you have an older version of NGUI you can just Diff the files for the differences.

I replaced this block of code:


				/*    REPLACED!!!!
				AudioSource source = mListener.audio;
				if (source == null) source = mListener.gameObject.AddComponent<AudioSource>();
				source.pitch = pitch;
				source.PlayOneShot(clip, volume);
				*/
				return SoundManager.PlaySFX(mListener.gameObject, clip, false, volume, pitch);

And Voila! It works!

Happy Game Making,
AntiLunchBox Studios