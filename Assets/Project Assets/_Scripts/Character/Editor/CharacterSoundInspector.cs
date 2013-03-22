using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CharacterSound))]
public class CharacterSoundInspector : Editor
{

//	CharacterSound characterSound;
//	SerializedObject m_Object;
//	SerializedProperty m_Property;
//	
//	void OnEnable ()
//	{
//		characterSound = target as CharacterSound;
//		m_Object = new SerializedObject(characterSound);
//	}
//	
//	public override void OnInspectorGUI ()
//	{
//		#region Character Sound Editor
//		EditorGUILayout.HelpBox ("You can control the sound system of character, adhering " +
//			"to what one can do with their respective number. Example: The character has been " +
//			"instantiated and features with sounds number 2.", MessageType.Info);
//		
//		EditorGUILayout.Space ();
//		
//		EditorGUILayout.LabelField ("Settings:");
//		characterSound.footstepSoundSource = EditorGUILayout.ObjectField ("Footstep Source Transform:", characterSound.footstepSoundSource, typeof(GameObject), true) as GameObject;
//		characterSound.damageSoundSource = EditorGUILayout.ObjectField ("Damage Source Transform:", characterSound.damageSoundSource, typeof(GameObject), true) as GameObject;
//		characterSound.deathSoundSource = EditorGUILayout.ObjectField ("Death Source Transform:", characterSound.deathSoundSource, typeof(GameObject), true) as GameObject;
//		
//		EditorGUILayout.Space ();
//		
//		EditorGUILayout.LabelField ("Audio clips:");
//		
//		characterSound.footstepSoundClip = EditorGUILayout.ObjectField ("Footstep Sound", characterSound.footstepSoundClip, typeof(AudioClip), true) as AudioClip;
//		
//		EditorGUILayout.BeginHorizontal ();
//		{
//			if (GUILayout.Button (new GUIContent("Add", 
//				"Add new Character Sound.")))
//			{
//				if (characterSound.damageSoundClips == null) characterSound.damageSoundClips = new System.Collections.Generic.List<AudioClip> ();
//				characterSound.damageSoundClips.Add(new AudioClip());
//				if (characterSound.deathSoundClips == null) characterSound.deathSoundClips = new System.Collections.Generic.List<AudioClip> ();
//				characterSound.deathSoundClips.Add(new AudioClip());
//				return;
//			}
//			if (characterSound.damageSoundClips != null)
//			{
//				if (characterSound.damageSoundClips.Count > 0)
//				{
//					if (GUILayout.Button (new GUIContent("Remove", 
//						"Remove the last Character Sound.")))
//					{
//						characterSound.damageSoundClips.RemoveAt(characterSound.damageSoundClips.Count-1);
//						characterSound.deathSoundClips.RemoveAt(characterSound.deathSoundClips.Count-1);
//						return;
//					}
//				}
//			}
//		}
//		EditorGUILayout.EndHorizontal ();
//		
//		for (int i = 0; i != characterSound.damageSoundClips.Count; i++)
//		{
//			GUILayout.BeginVertical ("box");
//			{
//				EditorGUILayout.LabelField ("Character Sound " + (i+1) + ":");
//				EditorGUI.indentLevel = 1;
//				characterSound.damageSoundClips[i] = EditorGUILayout.ObjectField ("Damage Sound", characterSound.damageSoundClips[i], typeof(AudioClip), true) as AudioClip;
//				characterSound.deathSoundClips[i] = EditorGUILayout.ObjectField ("Death Sound", characterSound.deathSoundClips[i], typeof(AudioClip), true) as AudioClip;
//				EditorGUILayout.Space ();
//				EditorGUI.indentLevel = 0;
//			}
//			EditorGUILayout.EndVertical ();
//		}
//		
//		EditorGUILayout.Space ();
//		
//		m_Property =  m_Object.FindProperty("extraSounds");
//		EditorGUILayout.PropertyField(m_Property, m_Property.isExpanded);
//		m_Object.ApplyModifiedProperties();
//		
//		EditorGUILayout.Space ();
//		
//		if (GUILayout.Button (new GUIContent("Create Sources", 
//			"Create Audio Sources in transforms that you set.")))
//		{
//			CreateSources ();
//		}
//		#endregion
//	}
//	
//	void CreateSources ()
//	{
//		if (null == characterSound.footstepSoundSource.audio)
//		{
//			characterSound.footstepSoundSource.AddComponent<AudioSource> ();
//			characterSound.footstepSoundSource.audio.playOnAwake = false;
//		}
//		if (null == characterSound.damageSoundSource.audio)
//		{
//			characterSound.damageSoundSource.AddComponent<AudioSource> ();
//			characterSound.damageSoundSource.audio.playOnAwake = false;
//		}
//		if (null == characterSound.deathSoundSource.audio)
//		{
//			characterSound.deathSoundSource.AddComponent<AudioSource> ();
//			characterSound.deathSoundSource.audio.playOnAwake = false;
//		}
//		
//		if (characterSound.extraSounds.Count != 0)
//		{
//			foreach (CharacterSound.ExtraSounds es in characterSound.extraSounds)
//			{
//				if (null == es.soundSource.audio)
//				{
//					es.soundSource.AddComponent<AudioSource> ();
//					es.soundSource.audio.playOnAwake = false;
//				}
//			}
//		}
//	}
}