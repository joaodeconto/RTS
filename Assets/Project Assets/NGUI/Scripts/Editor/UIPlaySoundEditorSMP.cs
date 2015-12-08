using UnityEngine;
using UnityEditor;
using antilunchbox;

[CanEditMultipleObjects()]
[CustomEditor(typeof(UIPlaySound))]
public class UIPlaySoundEditorSMP : Editor {

	private UIPlaySound script;
	
	private void OnEnable()
	{
		script = target as UIPlaySound;
	}

	public override void OnInspectorGUI ()
	{
		#region ClipType handler
		ClipType clipType = script.clipType;
		clipType = (ClipType)EditorGUILayout.EnumPopup("Clip Type", clipType);

		if(clipType != script.clipType)
		{
			SoundManagerEditorTools.RegisterObjectChange("Change Clip Type", script);
			script.clipType = clipType;
			if(script.clipType != ClipType.AudioClip)
				script.audioClip = null;
			EditorUtility.SetDirty(script);
		}
		if(clipType != ClipType.AudioClip && script.audioClip != null)
		{
			SoundManagerEditorTools.RegisterObjectChange("Change Clip Type", script);
			script.clipType = ClipType.AudioClip;
			EditorUtility.SetDirty(script);
		}
		
		switch(script.clipType)
		{
		case ClipType.ClipFromSoundManager:
			string clipName = script.clipName;
			clipName = EditorGUILayout.TextField("Audio Clip Name", clipName);
			if(clipName != script.clipName)
			{
				SoundManagerEditorTools.RegisterObjectChange("Change Clip Name", script);
				script.clipName = clipName;
				EditorUtility.SetDirty(script);
			}
			break;
		case ClipType.ClipFromGroup:
			string groupName = script.groupName;
			groupName = EditorGUILayout.TextField("SFXGroup Name", groupName);
			if(groupName != script.groupName)
			{
				SoundManagerEditorTools.RegisterObjectChange("Change SFXGroup Name", script);
				script.groupName = groupName;
				EditorUtility.SetDirty(script);
			}
			break;
		case ClipType.AudioClip:
		default:
			break;
		}
		#endregion
		base.OnInspectorGUI();
	}
}
