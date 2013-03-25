using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SoundManager))]
public class SoundManagerInspector : Editor
{

	SoundManager soundManager;
	Vector2 scrollPosition = Vector2.zero;
	
	void OnEnable ()
	{
		soundManager = target as SoundManager;
	}
	
	public override void OnInspectorGUI ()
	{
		EditorGUILayout.LabelField ("Sound Settings:");
		
		if (GUILayout.Button (new GUIContent("Add Sound", 
			"Create a new Sound to you set.")))
		{
			soundManager.sounds.Add (new SoundManager.Sound());
		}
		
		if (soundManager.sounds.Count == 0)
		{
			EditorGUILayout.HelpBox ("Don't have any kind of \"Sound\". Click in \"Add Sound\" to resolve this problem.", MessageType.Warning);
			return;
		}
		
		scrollPosition = EditorGUILayout.BeginScrollView (scrollPosition);
		{
			for (int i = 0; i != soundManager.sounds.Count; i++)
			{
				GUILayout.BeginVertical ("box");
				{
					EditorGUI.indentLevel = 1;
					GUILayout.BeginHorizontal ();
					{
						GUILayout.BeginVertical ();
						{
							soundManager.sounds[i].name = EditorGUILayout.TextField ("Name:", soundManager.sounds[i].name);
							if (GUILayout.Button (new GUIContent("Add Sound Clip", 
								"Add a new Sound Clip.")))
							{
								soundManager.sounds[i].soundsClip.Add (new AudioClip ());
							}
							EditorGUILayout.LabelField ("Sounds clip:");
							for (int k = 0; k != soundManager.sounds[i].soundsClip.Count; k++)
							{
								GUILayout.BeginHorizontal ();
								{
									soundManager.sounds[i].soundsClip[k] = EditorGUILayout.ObjectField ("Index " + k + ":", soundManager.sounds[i].soundsClip[k], 
																										typeof(AudioClip), false) as AudioClip;
									if (k != 0)
									{
										if (GUILayout.Button (new GUIContent("-", 
											"Remove this Sound Clip."), GUILayout.Width (30f)))
										{
											soundManager.sounds[i].soundsClip.RemoveAt (k);
											break;
										}
									}
								}
								GUILayout.EndHorizontal ();
							}
						}
						EditorGUILayout.EndVertical ();
						if (GUILayout.Button (new GUIContent("-", 
							"Remove this Sound."), GUILayout.Width (30f)))
						{
							soundManager.sounds.RemoveAt (i);
							break;
						}
					}
					GUILayout.EndHorizontal ();
					if (string.IsNullOrEmpty(soundManager.sounds[i].name))
					{
						EditorGUILayout.HelpBox ("Name can't be null!", MessageType.Error);
					}
					EditorGUI.indentLevel = 0;
				}
				EditorGUILayout.EndVertical ();
				EditorGUILayout.Space ();
			}
		}
		EditorGUILayout.EndScrollView ();
	}
}