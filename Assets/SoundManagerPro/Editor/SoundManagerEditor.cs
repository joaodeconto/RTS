using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public partial class SoundManagerEditor : Editor {
	/* Serialized Object ( target ) */
	private SerializedObject mObject;
	private SoundManager script;
	
	/* Serialized Properties */
	private SerializedProperty mSoundConnectionsCount;
	
	private AudioSource mAudio1;
	private AudioSource mAudio2;
	
	private SerializedProperty mCrossDuration;
	
	private SerializedProperty mShowDebug;
	private SerializedProperty mOffBGM;
	
	/* Access Strings */
	private static string mListCountPath = "soundConnections.Array.size";
	private static string mListData = "soundConnections.Array.data[{0}]";
	
	private static string mAudioSourcesCountPath = "audios.Array.size";
	private static string mAudioSourceData = "audios.Array.data[{0}]";
	
	private static string mSCLevel = "level";
	private static string mSCIsCustomLevel = "isCustomLevel";
	private static string mSCSoundToPlay = "soundsToPlay.Array.data[{0}]";
	private static string mSCSoundSize = "soundsToPlay.Array.size";
	private static string mSCPlayMethod = "playMethod";
	private static string mSCMinDelay = "minDelay";
	private static string mSCMaxDelay = "maxDelay";
	private static string mSCDelay = "delay";
	
	/* Local Objects */	
	private bool listeningForGuiChanges;
    private bool guiChanged;
	
	private Color softGreen = new Color(.67f,.89f,.67f,1f);
	private Color hardGreen = Color.green;
	
	Editor proxy;
	
	bool enabled;
	
	private string defaultName = "-enter name-";
	private bool repaintNextFrame = false;
	
	public void ProxyInitialize (Editor proxy)
	{
		this.proxy = proxy;
	}
	
	public void Enable()
	{
		enabled = (PrefabUtility.GetPrefabType(proxy.target) != PrefabType.Prefab);
		
		script = proxy.target as SoundManager;
		mObject = new SerializedObject(proxy.target);
		mCrossDuration = mObject.FindProperty("crossDuration");
		mShowDebug = mObject.FindProperty("showDebug");
		mOffBGM = mObject.FindProperty("offTheBGM");
		mSoundConnectionsCount = mObject.FindProperty(mListCountPath);
		
		if(script.GetComponent<Transform>().hideFlags != HideFlags.HideInInspector)
			script.GetComponent<Transform>().hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
		
		if(script.storage == null)
		{
			script.storage = ScriptableObject.CreateInstance<EditorVariableStorage>();
			script.storage.hideFlags = HideFlags.HideAndDontSave;
		}
		
		CheckNullMonoBehaviours();
		EnableSFX();
		init();
		
		if(!script.storage.titleBar)
			script.storage.titleBar = Resources.LoadAssetAtPath ("Assets/Gizmos/TitleBar.png", typeof(Texture2D)) as Texture2D;
		if(!script.storage.footer)
			script.storage.footer = Resources.LoadAssetAtPath ("Assets/Gizmos/AntiLunchBox Logo.png", typeof(Texture2D)) as Texture2D;
		if(!script.storage.icon)
			script.storage.icon = Resources.LoadAssetAtPath ("Assets/Gizmos/SoundManager Icon.png", typeof(Texture2D)) as Texture2D;
		
		HideVariables();
	}
	
	void HideVariables()
	{
		if(script != null && script.storage)
			script.storage.Hide();
	}
	
	public void Disable()
	{
		HideVariables();
		if(script != null)
		{
			if(script.GetComponent<Transform>().hideFlags != HideFlags.HideInInspector)
				script.GetComponent<Transform>().hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
		}
	}
	
	void CheckNullMonoBehaviours()
	{
		foreach (Component c in script.GetComponents(typeof(Component))) {
            if (c == null)
			{
                DestroyImmediate(c);
            }
        }
	}
	
	void init()
	{
		script.storage.isEditing = false;
		for(int i = 0; i < mSoundConnectionsCount.intValue ; i++)
		{
			SerializedObject sc = new SerializedObject(mObject.FindProperty(string.Format(mListData, i)).objectReferenceValue);
			var levelName = sc.FindProperty(mSCLevel).stringValue;
			if(!script.storage.songStatus.ContainsKey(levelName))
				script.storage.songStatus.Add(levelName, EditorVariableStorage.HIDE);
		}
	}
	
	public GUIStyle CreateFoldoutGUI()
	{
		GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.foldout);
		Color myStyleColor = Color.white;
		myFoldoutStyle.fontStyle = FontStyle.Bold;
		myFoldoutStyle.normal.textColor = myStyleColor;
		myFoldoutStyle.onNormal.textColor = myStyleColor;
		myFoldoutStyle.hover.textColor = myStyleColor;
		myFoldoutStyle.onHover.textColor = myStyleColor;
		myFoldoutStyle.focused.textColor = myStyleColor;
		myFoldoutStyle.onFocused.textColor = myStyleColor;
		myFoldoutStyle.active.textColor = myStyleColor;
		myFoldoutStyle.onActive.textColor = myStyleColor;
		
		return myFoldoutStyle;
	}

	public override void OnInspectorGUI()
	{
		mObject.Update();
		GUI.enabled = enabled;
		GUI.color = Color.white;
		GUIStyle foldoutStyle = CreateFoldoutGUI();
		Color inactiveColor = new Color(.65f,.65f,.65f);
		var expand = '\u2261'.ToString();
		GUIContent expandContent = new GUIContent(expand, "Expand/Collapse");
		if(enabled && (!script || !script.storage))
			return;
		//base.OnInspectorGUI();
		if(script.storage.titleBar != null)
		{
			GUILayout.BeginHorizontal();
			{
	    		GUILayout.FlexibleSpace();
				{
	    			GUILayout.Label(script.storage.titleBar);
				}
	    		GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
		}
		//
		if(script.storage.showInfo)
			GUI.color = inactiveColor;
		EditorGUILayout.BeginVertical(EditorStyles.objectFieldThumb, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUI.color = Color.white;
				script.storage.showInfo = EditorGUILayout.Foldout(script.storage.showInfo, new GUIContent("Info", "Information about the current scene and tracks playing."), foldoutStyle);
				script.storage.showInfo = GUILayout.Toggle(script.storage.showInfo, expandContent, EditorStyles.toolbarButton, GUILayout.Width(50f));
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
		
		if(script.storage.showInfo)
		{
			EditorGUILayout.BeginVertical();
			{
				ShowInfo();
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.Separator();
		//
		if(script.storage.showDev)
			GUI.color = inactiveColor;
		EditorGUILayout.BeginVertical(EditorStyles.objectFieldThumb, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUI.color = Color.white;
				script.storage.showDev = EditorGUILayout.Foldout(script.storage.showDev, new GUIContent("Developer Settings", "Settings to customize how SoundManager behaves."), foldoutStyle);
				script.storage.showDev = GUILayout.Toggle(script.storage.showDev, expandContent, EditorStyles.toolbarButton, GUILayout.Width(50f));
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
		
		if(script.storage.showDev)
		{
			EditorGUILayout.BeginVertical();
			{
				ShowDeveloperSettings();
			}
			EditorGUILayout.EndVertical();	
		}
		EditorGUILayout.Separator();
		//
		if(script.storage.showList)
			GUI.color = inactiveColor;
		EditorGUILayout.BeginVertical(EditorStyles.objectFieldThumb, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUI.color = Color.white;
				script.storage.showList = EditorGUILayout.Foldout(script.storage.showList, new GUIContent("SoundConnections", "List of existing SoundConnections that tie a group of clips, a play method, and various other variables to a scene."), foldoutStyle);	
				GUILayout.FlexibleSpace();
				if(!script.storage.viewAll && GUILayout.Button(new GUIContent("view all", "show all SoundConnections"), EditorStyles.toolbarButton, GUILayout.Width(75f)))
				{
					script.storage.showList = true;
					script.storage.viewAll = true;
				} else if (script.storage.viewAll && GUILayout.Button(new GUIContent("hide all", "hide all SoundConnections"), EditorStyles.toolbarButton, GUILayout.Width(75f))) {
					script.storage.viewAll = false;
				}
				EditorGUILayout.Space();
				script.storage.showList = GUILayout.Toggle(script.storage.showList, expandContent, EditorStyles.toolbarButton, GUILayout.Width(50f));
			}
			EditorGUILayout.EndHorizontal();		
		}
		EditorGUILayout.EndVertical();
		
		if(script.storage.showList)
		{
			EditorGUILayout.BeginVertical();
			{
				ShowSoundConnectionList();
			}
			EditorGUILayout.EndVertical();	
		}
		EditorGUILayout.Separator();
		//
		if(script.storage.showAdd)
			GUI.color = inactiveColor;
		EditorGUILayout.BeginVertical(EditorStyles.objectFieldThumb, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUI.color = Color.white;
				script.storage.showAdd = EditorGUILayout.Foldout(script.storage.showAdd, new GUIContent("Add SoundConnection(s)", "Where SoundConnections are made."), foldoutStyle);
				script.storage.showAdd = GUILayout.Toggle(script.storage.showAdd, expandContent, EditorStyles.toolbarButton, GUILayout.Width(50f));
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
		
		if(script.storage.showAdd)
		{
			EditorGUILayout.BeginVertical();
			{
				ShowAddSoundConnection();
			}
			EditorGUILayout.EndVertical();	
		}
		EditorGUILayout.Separator();
		//

		if(script.storage.showSFX)
			GUI.color = inactiveColor;
		EditorGUILayout.BeginVertical(EditorStyles.objectFieldThumb, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUI.color = Color.white;
				script.storage.showSFX = EditorGUILayout.Foldout(script.storage.showSFX, new GUIContent("SFX", "Section for handling SFX and applying attributes to groups of SFX"), foldoutStyle);
				script.storage.showSFX = GUILayout.Toggle(script.storage.showSFX, expandContent, EditorStyles.toolbarButton, GUILayout.Width(50f));
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
		
		if(script.storage.showSFX)
		{
			EditorGUILayout.BeginVertical();
			{
				ShowSoundFX();
			}
			EditorGUILayout.EndVertical();
		}
		//
		if(script.storage.footer != null)
		{
			GUILayout.BeginHorizontal();
			{
	    		GUILayout.FlexibleSpace();
				{
	    			GUILayout.Label(script.storage.footer, GUI.skin.label);
				}
	    		GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
		}
		
		mObject.ApplyModifiedProperties();
	}
	
	#region GUI functions
	void ShowInfo()
	{
		EditorGUI.indentLevel++;
		{
			EditorGUILayout.LabelField("Current Scene:", Application.loadedLevelName);
			mCrossDuration.floatValue = EditorGUILayout.FloatField("Cross Duration:",mCrossDuration.floatValue);
			if(mCrossDuration.floatValue < 0) mCrossDuration.floatValue = 0;
			
			int audioSize = (mObject.FindProperty(mAudioSourcesCountPath).intValue);
			if(audioSize != 0)
			{
				mAudio1 = (mObject.FindProperty(string.Format(mAudioSourceData, 0)).objectReferenceValue as AudioSource);
				mAudio2 = (mObject.FindProperty(string.Format(mAudioSourceData, 1)).objectReferenceValue as AudioSource);
				
				string name1,name2;
				name1 = name2 = "No Song";
				
				GUI.color = hardGreen;
				if(mAudio1 && mAudio1.clip)
					name1 = mAudio1.clip.name;
				else
					GUI.color = Color.red;
				
				Rect rect = GUILayoutUtility.GetRect (28, 28, "TextField");
				if(name1 != "No Song")
					name1 += "\n" + System.TimeSpan.FromSeconds(mAudio1.time).ToString().Split('.')[0] + "/" + System.TimeSpan.FromSeconds(mAudio1.clip.length).ToString().Split('.')[0];
				EditorGUI.ProgressBar(rect,mAudio1.volume,name1);
				
				GUI.color = hardGreen;
				if(mAudio2 && mAudio2.clip)
					name2 = mAudio2.clip.name;
				else
					GUI.color = Color.red;
				
				Rect rect2 = GUILayoutUtility.GetRect(28, 28, "TextField");
				if(name2 != "No Song")
					name2 += "\n" + System.TimeSpan.FromSeconds(mAudio2.time).ToString().Split('.')[0] + "/" + System.TimeSpan.FromSeconds(mAudio2.clip.length).ToString().Split('.')[0];
				EditorGUI.ProgressBar(rect2,mAudio2.volume,name2);
				
				GUI.color = Color.white;
				Repaint();
			} else {
				GUI.color = Color.red;
				Rect rect = GUILayoutUtility.GetRect (28, 28, "TextField");
				EditorGUI.ProgressBar(rect,0f,"Standing By...");
				
				Rect rect2 = GUILayoutUtility.GetRect(28, 28, "TextField");
				EditorGUI.ProgressBar(rect2,0f,"Standing By...");
				GUI.color = Color.white;
			}
		}
		EditorGUI.indentLevel--;
		EditorGUILayout.Space();
	}
	
	void ShowSoundConnectionList()
	{
		EditorGUI.indentLevel++;
		for(int i = 0 ; i < mSoundConnectionsCount.intValue ; i++)
		{
			ShowSoundConnection(i);
		}
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
		EditorGUI.indentLevel--;
		EditorGUILayout.Space();
	}
	
	void ShowSoundConnection(int index)
	{
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
		
		string status = EditorVariableStorage.HIDE;
		SerializedObject sc = new SerializedObject(mObject.FindProperty(string.Format(mListData, index)).objectReferenceValue);
		EditorGUILayout.BeginVertical();
		{
			EditorGUILayout.BeginHorizontal();
			{
				var levelName = "";
					
				levelName = sc.FindProperty(mSCLevel).stringValue;
				status = script.storage.songStatus[levelName] as string;
				if(status == EditorVariableStorage.HIDE) GUI.enabled = false;
				if(script.storage.icon)
					GUILayout.Label(script.storage.icon, GUILayout.ExpandWidth(false));
				GUI.enabled = true;
				
				if(sc.FindProperty(mSCIsCustomLevel).boolValue)
					EditorGUILayout.LabelField("<custom>"+levelName);
				else
					EditorGUILayout.LabelField(levelName);
				
				if( status == EditorVariableStorage.HIDE && GUILayout.Button("view", GUILayout.Width(40f)))
				{
					script.storage.songStatus[levelName] = EditorVariableStorage.VIEW;
				} else if(status == EditorVariableStorage.VIEW) {
					GUI.color = Color.white;
					if(GUILayout.Button("hide", GUILayout.Width(40f)))
					{
						script.storage.songStatus[levelName] = EditorVariableStorage.HIDE;
					}
				}
				GUI.color = Color.cyan;
				if(status != EditorVariableStorage.EDIT && script.storage.isEditing) GUI.enabled = false;
				if( status == EditorVariableStorage.HIDE && GUILayout.Button("edit", GUILayout.Width(40f)))
				{
					script.storage.isEditing = true;
					script.storage.songStatus[levelName] = EditorVariableStorage.EDIT;
					script.storage.delayToEdit = sc.FindProperty(mSCDelay).floatValue;
					script.storage.minDelayToEdit = sc.FindProperty(mSCMinDelay).floatValue;
					script.storage.maxDelayToEdit = sc.FindProperty(mSCMaxDelay).floatValue;
				} else if (status == EditorVariableStorage.EDIT) {
					GUI.color = Color.white;
					if(GUILayout.Button("done", GUILayout.Width(40f)))
					{
						script.storage.isEditing = false;
						script.storage.songStatus[levelName] = EditorVariableStorage.HIDE;
					}
				}
				GUI.enabled = true;
				GUI.color = Color.red;
				if(status == EditorVariableStorage.HIDE && GUILayout.Button("X", GUILayout.Width(25f)))
				{
					RemoveSoundConnection(index);
				}
				GUI.color = Color.white;
			}
			EditorGUILayout.EndHorizontal();
			
			if(status != EditorVariableStorage.HIDE)
			{
				EditorGUI.indentLevel++;
				if(status == EditorVariableStorage.VIEW)
				{
					ShowSoundConnectionInfo(sc,false);	
				}
				else if (status == EditorVariableStorage.EDIT)
				{
					ShowSoundConnectionInfo(sc,true);
				}
				EditorGUI.indentLevel--;
			}
		}
		EditorGUILayout.EndVertical();
	}
	
	void ShowSoundConnectionInfo(SerializedObject obj, bool editable)
	{
		EditorGUILayout.BeginVertical();
		{
			SoundManager.PlayMethod method = (SoundManager.PlayMethod)obj.FindProperty(mSCPlayMethod).intValue;
			if(!editable)
			{
				EditorGUILayout.LabelField("Play Method: ", method.ToString());
			} else {
				int oldMethod = obj.FindProperty(mSCPlayMethod).intValue;
				int newMethod = (int)(SoundManager.PlayMethod)EditorGUILayout.EnumPopup("Play Method:", (SoundManager.PlayMethod)obj.FindProperty(mSCPlayMethod).intValue, EditorStyles.popup);
				if(oldMethod != newMethod)
					#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
					Undo.RegisterSceneUndo("Change Play Method");
					#else
					Undo.RecordObject(script, "Change Play Method");
					#endif
				obj.FindProperty(mSCPlayMethod).intValue = newMethod;
				if(GUI.changed)
					obj.ApplyModifiedProperties();
			}
			ShowMethodDetails(obj,editable,method);
			EditorGUILayout.HelpBox(GetPlayMethodDescription(method), MessageType.Info);
			ShowSongList(obj,editable);
		}
		EditorGUILayout.EndVertical();
	}
	
	void ShowMethodDetails(SerializedObject obj, bool editable, SoundManager.PlayMethod method)
	{
		CheckUndo();
		EditorGUI.indentLevel++;
		switch (method)
		{
		case SoundManager.PlayMethod.ContinuousPlayThrough:
		case SoundManager.PlayMethod.OncePlayThrough:
		case SoundManager.PlayMethod.ShufflePlayThrough:
			break;
		case SoundManager.PlayMethod.ContinuousPlayThroughWithDelay:
		case SoundManager.PlayMethod.OncePlayThroughWithDelay:
		case SoundManager.PlayMethod.ShufflePlayThroughWithDelay:
			if(!editable)
				EditorGUILayout.LabelField("Delay:" + obj.FindProperty(mSCDelay).floatValue + " second(s)");
			else {
				script.storage.delayToEdit = obj.FindProperty(mSCDelay).floatValue;

				#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
				EditorGUI.BeginChangeCheck();
				#endif
				script.storage.delayToEdit = EditorGUILayout.FloatField("Delay:",script.storage.delayToEdit);
				if(script.storage.delayToEdit < 0) script.storage.delayToEdit = 0;

				#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
				obj.FindProperty(mSCDelay).floatValue = script.storage.delayToEdit;
				#else
				if(EditorGUI.EndChangeCheck())
				{
					Undo.RecordObjects(new Object[]{script, script.storage}, "Modify Delay");
					obj.FindProperty(mSCDelay).floatValue = script.storage.delayToEdit;
				}
				#endif

				#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
				if(GUI.changed)
				{
					if(!guiChanged && listeningForGuiChanges)
					{
						guiChanged = true;
						CheckUndo();
					}
					guiChanged = true;
					obj.ApplyModifiedProperties();
				}
				#endif
			}
			break;
		case SoundManager.PlayMethod.ContinuousPlayThroughWithRandomDelayInRange:
		case SoundManager.PlayMethod.OncePlayThroughWithRandomDelayInRange:
		case SoundManager.PlayMethod.ShufflePlayThroughWithRandomDelayInRange:
			if(!editable)
			{
				EditorGUILayout.LabelField("Min Delay:" + obj.FindProperty(mSCMinDelay).floatValue + " second(s)");
				EditorGUILayout.LabelField("Max Delay:" + obj.FindProperty(mSCMaxDelay).floatValue + " second(s)");
			} else {
				script.storage.minDelayToEdit = obj.FindProperty(mSCMinDelay).floatValue;
				script.storage.maxDelayToEdit = obj.FindProperty(mSCMaxDelay).floatValue;

				#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
				EditorGUI.BeginChangeCheck();
				#endif

				script.storage.minDelayToEdit = EditorGUILayout.FloatField("Minimum Delay:",script.storage.minDelayToEdit);
				if(script.storage.minDelayToEdit < 0) script.storage.minDelayToEdit = 0;

				#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
				obj.FindProperty(mSCMinDelay).floatValue = script.storage.minDelayToEdit;
				#else
				if(EditorGUI.EndChangeCheck())
				{
					Undo.RecordObjects(new Object[]{script, script.storage}, "Modify Minimum Delay");
					obj.FindProperty(mSCMinDelay).floatValue = script.storage.minDelayToEdit;
				}
				#endif
				
				if(script.storage.maxDelayToEdit < script.storage.minDelayToEdit) script.storage.maxDelayToEdit = script.storage.minDelayToEdit;
				script.storage.maxDelayToEdit = EditorGUILayout.FloatField("Maximum Delay:",script.storage.maxDelayToEdit);
				if(script.storage.maxDelayToEdit < 0) script.storage.maxDelayToEdit = 0;

				#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
				obj.FindProperty(mSCMaxDelay).floatValue = script.storage.maxDelayToEdit;
				#else
				if(EditorGUI.EndChangeCheck())
				{
					Undo.RecordObjects(new Object[]{script, script.storage}, "Modify Maximum Delay");
					obj.FindProperty(mSCMaxDelay).floatValue = script.storage.maxDelayToEdit;
				}
				#endif

				#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
				if(GUI.changed)
				{
					if(!guiChanged && listeningForGuiChanges)
					{
						guiChanged = true;
						CheckUndo();
					}
					guiChanged = true;
					obj.ApplyModifiedProperties();
				}
				#endif
			}
			break;
		}
		EditorGUI.indentLevel--;
	}
	
	void ShowSongList(SerializedObject obj, bool editable)
	{
		int size = obj.FindProperty(mSCSoundSize).intValue;
		
		EditorGUILayout.LabelField("Sound List:");
		EditorGUI.indentLevel++;
		
		for(int i = 0; i < size ; i++)
		{
			EditorGUILayout.BeginVertical();
			{
				EditorGUILayout.BeginHorizontal();
				{
					if(!editable)
					{
						AudioClip thisClip = obj.FindProperty(string.Format(mSCSoundToPlay, i)).objectReferenceValue as AudioClip;
						EditorGUILayout.LabelField(thisClip.name);
					} else {
						if(obj.FindProperty(string.Format(mSCSoundToPlay, i)) == null) return;
						Object newClip = EditorGUILayout.ObjectField(obj.FindProperty(string.Format(mSCSoundToPlay, i)).objectReferenceValue, typeof(AudioClip), false);
						if(newClip != null && newClip.GetType() == typeof(AudioClip))
							obj.FindProperty(string.Format(mSCSoundToPlay, i)).objectReferenceValue = newClip;
						
						if(GUI.changed)
						{
							if(newClip == null)
								RemoveSound(obj, mSCSoundToPlay, mSCSoundSize, i);
						}
						
						bool oldEnabled = GUI.enabled;
						if(i == 0) GUI.enabled = false;
						if(GUILayout.Button("U", GUILayout.Width(35f)))
						{
							SwapSounds(obj, mSCSoundToPlay, i, i-1);
						}
						GUI.enabled = oldEnabled;
						
						if(i == size-1) GUI.enabled = false;
						if(GUILayout.Button("D", GUILayout.Width(35f)))
						{
							SwapSounds(obj, mSCSoundToPlay, i, i+1);
						}
						GUI.enabled = oldEnabled;
						
						GUI.color = Color.red;
						if(GUILayout.Button("-", GUILayout.Width(20f)))
						{
							RemoveSound(obj, mSCSoundToPlay, mSCSoundSize, i);
						}
						GUI.color = Color.white;
						
						if(GUI.changed)
							obj.ApplyModifiedProperties();
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
		}
		
		if(editable)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUI.indentLevel++;
				script.storage.editAddClip = EditorGUILayout.ObjectField("Add A Sound",script.storage.editAddClip, typeof(AudioClip), false) as AudioClip;
				if(GUI.changed)
					HideVariables();
				
				GUI.color = softGreen;
				if(GUILayout.Button("add", GUILayout.Width(40f)))
				{
					AddSound(obj, mSCSoundToPlay, mSCSoundSize, script.storage.editAddClip);
					script.storage.editAddClip = null;
					HideVariables();
				}
				GUI.color = Color.white;
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUI.indentLevel--;
	}
	
	void ShowAddSoundConnection()
	{
		/*if(script.storage.showNewSoundConnectionButton)
		{
			ShowNewSoundConnectionButton();
		} else {
			ShowAddSoundConnectionArea();
		}*/
		ShowAddSoundConnectionArea();
	}
	
	void ShowNewSoundConnectionButton()
	{
		GUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			{
				GUI.color = softGreen;
				if(GUILayout.Button("New SoundConnection", GUILayout.MaxWidth(200f)))
				{
					script.storage.showNewSoundConnectionButton = false;
				}
				GUI.color = Color.white;
			};
			GUILayout.FlexibleSpace();
		}
		GUILayout.EndHorizontal();
	}
	
	void ShowAddSoundConnectionArea()
	{
		bool full = ShowRequiredSoundConnectionInfo();
		if(full)
		{
			ShowAddSongList();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			DropAreaGUI();
			ShowAddClipSelector();
		}
		
		if(!full) GUI.enabled = false;
		ShowAddSoundConnectionButton();
		GUI.enabled = enabled;
		
		//ShowCancelAddSoundConnectionButton();
	}
	
	void ShowCancelAddSoundConnectionButton()
	{
		GUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			{
				GUI.color = Color.red;
				if(GUILayout.Button("Cancel", GUILayout.MaxWidth(100f)))
				{
					script.storage.showNewSoundConnectionButton = true;
				}
				GUI.color = Color.white;
			}
			GUILayout.FlexibleSpace();
		}
		GUILayout.EndHorizontal();
	}
	
	void ShowAddSongList()
	{
		int soundToRemove = -1;
		EditorGUILayout.LabelField("Sound List:");
		EditorGUI.indentLevel++;
		if(script.storage != null)
		{
			foreach(AudioClip soundToAdd in script.storage.soundsToAdd)
			{
				EditorGUILayout.BeginVertical();
				{
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField(soundToAdd.name);
						bool oldEnabled = GUI.enabled;
						if(!SoundCanMoveUp(soundToAdd,script.storage.soundsToAdd)) GUI.enabled = false;
						if(GUILayout.Button("U", GUILayout.Width(35f)))
						{
							int thisIndex = script.storage.soundsToAdd.IndexOf(soundToAdd);
							SwapSounds(soundToAdd,script.storage.soundsToAdd[thisIndex-1],script.storage.soundsToAdd);
						}
						GUI.enabled = oldEnabled;
						if(!SoundCanMoveDown(soundToAdd,script.storage.soundsToAdd)) GUI.enabled = false;
						if(GUILayout.Button("D", GUILayout.Width(35f)))
						{
							int thisIndex = script.storage.soundsToAdd.IndexOf(soundToAdd);
							SwapSounds(soundToAdd,script.storage.soundsToAdd[thisIndex+1],script.storage.soundsToAdd);
						}
						GUI.enabled = oldEnabled;
						GUI.color = Color.red;
						if(GUILayout.Button("-", GUILayout.Width(20f)))
						{
							soundToRemove = script.storage.soundsToAdd.IndexOf(soundToAdd);
						}
						GUI.color = Color.white;
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
			}
			if(soundToRemove >= 0)
			{
				script.storage.soundsToAdd.RemoveAt(soundToRemove);
			}
		}
		EditorGUI.indentLevel--;
	}
	
	bool ShowRequiredSoundConnectionInfo()
	{
		bool canMoveOn = true, forceRepaint = false;
		if(canMoveOn)
		{
			canMoveOn = false;
			string[] availableNames = GetAvailableLevelNamesForAddition();
			if(availableNames.Length == 1)
			{
				EditorGUILayout.LabelField("All enabled scenes have SoundConnections already.");
				//return false;
			}
			if(script.storage.levelIndex >= availableNames.Length)
				script.storage.levelIndex = availableNames.Length-1;
			
			script.storage.levelIndex = EditorGUILayout.Popup("Choose Level:",script.storage.levelIndex, availableNames, EditorStyles.popup);
			
			if(script.storage.levelIndex == availableNames.Length-1) //must be custom
			{
				if(!script.storage.isCustom)
				{
					if(script.storage.levelToAdd != defaultName)
					{
						script.storage.levelToAdd = defaultName;
						forceRepaint = true;
						GUIUtility.keyboardControl = 0;
					}
				}
				script.storage.isCustom = true;
				bool isValidName = true, isEmptyName = false;
				if(string.IsNullOrEmpty(script.storage.levelToAdd) || script.storage.levelToAdd == defaultName)
					isEmptyName = true;
				if(isEmptyName || IsStringSceneName(script.storage.levelToAdd) || IsStringAlreadyTaken(script.storage.levelToAdd))
					isValidName = false;
				
				if(isValidName)
					GUI.color = Color.green;
				else if(!isEmptyName)
					GUI.color = Color.red;
				EditorGUILayout.BeginHorizontal();
				{
					script.storage.levelToAdd = EditorGUILayout.TextField("Custom Level Name:",script.storage.levelToAdd, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
					if(isValidName)
						EditorGUILayout.LabelField("OK", GUILayout.Width(35));
					else if(!isEmptyName)
						EditorGUILayout.LabelField("BAD", GUILayout.Width(35));
					canMoveOn = isValidName;
				}
				EditorGUILayout.EndHorizontal();
				GUI.color = Color.white;
			}
			else
			{
				if(script.storage.isCustom)
				{
					forceRepaint = repaintNextFrame = true;
					GUIUtility.keyboardControl = 0;
				}
					
				script.storage.isCustom = false;
				script.storage.levelToAdd = availableNames[script.storage.levelIndex];
				canMoveOn = true;
			}
			
			
			if(canMoveOn)
			{
				canMoveOn = false;
				script.storage.playMethodToAdd = (SoundManager.PlayMethod)EditorGUILayout.EnumPopup("Play Method:", script.storage.playMethodToAdd, EditorStyles.popup);
				
				canMoveOn = ShowPlayMethodParameters(script.storage.playMethodToAdd);
			}
		}
		if(forceRepaint)
			SceneView.RepaintAll();
		else if(repaintNextFrame)
		{
			SceneView.RepaintAll();
			repaintNextFrame = false;
		}
		return canMoveOn;
	}
	
	bool ShowPlayMethodParameters(SoundManager.PlayMethod method)
	{
		bool canMoveOn = true;
		switch (method)
		{
		case SoundManager.PlayMethod.ContinuousPlayThrough:
		case SoundManager.PlayMethod.OncePlayThrough:
		case SoundManager.PlayMethod.ShufflePlayThrough:
			break;
		case SoundManager.PlayMethod.ContinuousPlayThroughWithDelay:
		case SoundManager.PlayMethod.OncePlayThroughWithDelay:
		case SoundManager.PlayMethod.ShufflePlayThroughWithDelay:
			canMoveOn = false;
			script.storage.delayToAdd = EditorGUILayout.FloatField("Delay:",script.storage.delayToAdd);
			if(script.storage.delayToAdd < 0) script.storage.delayToAdd = 0;
			canMoveOn = true;
			break;
		case SoundManager.PlayMethod.ContinuousPlayThroughWithRandomDelayInRange:
		case SoundManager.PlayMethod.OncePlayThroughWithRandomDelayInRange:
		case SoundManager.PlayMethod.ShufflePlayThroughWithRandomDelayInRange:
			canMoveOn = false;
			script.storage.minDelayToAdd = EditorGUILayout.FloatField("Minimum Delay:",script.storage.minDelayToAdd);
			if(script.storage.minDelayToAdd < 0) script.storage.minDelayToAdd = 0;
			if(script.storage.maxDelayToAdd < script.storage.minDelayToAdd) script.storage.maxDelayToAdd = script.storage.minDelayToAdd;
			script.storage.maxDelayToAdd = EditorGUILayout.FloatField("Maximum Delay:",script.storage.maxDelayToAdd);
			if(script.storage.maxDelayToAdd < 0) script.storage.maxDelayToAdd = 0;
			canMoveOn = true;
			break;
		}
		return canMoveOn;
	}
	
	void ShowAddSoundConnectionButton()
	{
		GUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			{
				GUI.color = softGreen;
				if(GUILayout.Button("Finish and Add SoundConnection", GUILayout.MaxWidth(250f)))
				{
					AddSoundConnection();
				}
				GUI.color = Color.white;
			}
			GUILayout.FlexibleSpace();
		}
		GUILayout.EndHorizontal();
	}
	
	void ShowDeveloperSettings()
	{
		EditorGUI.indentLevel++;
			mShowDebug.boolValue = EditorGUILayout.Toggle("Show Debug Info:" ,mShowDebug.boolValue);
			mOffBGM.boolValue = EditorGUILayout.Toggle("Music Always Off:" , mOffBGM.boolValue);	
			mOffSFX.boolValue = EditorGUILayout.Toggle("SFX Always Off:" , mOffSFX.boolValue);
			mCapAmount.intValue = EditorGUILayout.IntField("SFX Cap Amount:", mCapAmount.intValue, GUILayout.Width(3f*Screen.width/4f));
			if(mCapAmount.intValue < 0) mCapAmount.intValue = 0;
			mResourcesPath.stringValue = EditorGUILayout.TextField("Default Load Path:", mResourcesPath.stringValue, GUILayout.Width(3f*Screen.width/4f));
		EditorGUI.indentLevel--;
		EditorGUILayout.Space();
	}
	
	void ShowAddClipSelector()
	{
		EditorGUILayout.BeginHorizontal();
		{
			script.storage.editAddClipFromSelector = EditorGUILayout.ObjectField("Select An AudioClip:", script.storage.editAddClipFromSelector, typeof(AudioClip), false) as AudioClip;
			if(GUI.changed)
				HideVariables();
			GUI.color = softGreen;
			if(GUILayout.Button("add", GUILayout.Width(40f)))
			{
				if(script.storage.editAddClipFromSelector != null)
				{
					script.storage.soundsToAdd.Add(script.storage.editAddClipFromSelector);
					script.storage.editAddClipFromSelector = null;
					HideVariables();
				}
			}
			GUI.color = Color.white;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
	}
	
	void DropAreaGUI()
	{
		GUI.color = softGreen;
		EditorGUILayout.BeginVertical();
		{
			var evt = Event.current;
			
			var dropArea = GUILayoutUtility.GetRect(0f,50f,GUILayout.ExpandWidth(true));
			GUI.Box (dropArea, "Drag AudioClip(s) Here");
			
			switch (evt.type)
			{
			case EventType.DragUpdated:
			case EventType.DragPerform:
				if(!dropArea.Contains (evt.mousePosition))
					break;
				
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				
				if( evt.type == EventType.DragPerform)
				{
					DragAndDrop.AcceptDrag();
					
					foreach (var draggedObject in DragAndDrop.objectReferences)
					{
						var aC = draggedObject as AudioClip;
						if(!aC || aC.GetType() != typeof(AudioClip))
							continue;
						
						//Add audioclip to arsenal of SoundConnection
						script.storage.soundsToAdd.Add(aC);
					}
					HideVariables();
				}
				Event.current.Use();
				break;
			}
		}
		EditorGUILayout.EndVertical();
		GUI.color = Color.white;
	}
	#endregion
	
	
	#region Functional
	void AddSoundConnection()
	{
		#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterSceneUndo("Add SoundConnection");
		#else
		Undo.RecordObject(script, "Add SoundConnection");
		#endif
		SoundConnection sc = null;
		switch (script.storage.playMethodToAdd)
		{
		case SoundManager.PlayMethod.ContinuousPlayThrough:
		case SoundManager.PlayMethod.OncePlayThrough:
		case SoundManager.PlayMethod.ShufflePlayThrough:
			sc = ScriptableObject.CreateInstance<SoundConnection>();
			sc.Initialize(script.storage.levelToAdd,script.storage.playMethodToAdd,script.storage.soundsToAdd.ToArray());
			break;
		case SoundManager.PlayMethod.ContinuousPlayThroughWithDelay:
		case SoundManager.PlayMethod.OncePlayThroughWithDelay:
		case SoundManager.PlayMethod.ShufflePlayThroughWithDelay:
			sc = ScriptableObject.CreateInstance<SoundConnection>();
			sc.Initialize(script.storage.levelToAdd,script.storage.playMethodToAdd,script.storage.delayToAdd,script.storage.soundsToAdd.ToArray());
			break;
		case SoundManager.PlayMethod.ContinuousPlayThroughWithRandomDelayInRange:
		case SoundManager.PlayMethod.OncePlayThroughWithRandomDelayInRange:
		case SoundManager.PlayMethod.ShufflePlayThroughWithRandomDelayInRange:
			sc = ScriptableObject.CreateInstance<SoundConnection>();
			sc.Initialize(script.storage.levelToAdd,script.storage.playMethodToAdd,script.storage.minDelayToAdd,script.storage.maxDelayToAdd,script.storage.soundsToAdd.ToArray());
			break;
		}
		if(script.storage.isCustom)
		{
			sc.SetToCustom();
			script.storage.levelToAdd = defaultName;
			repaintNextFrame = true;
		}
		mSoundConnectionsCount.intValue++;
		SetSoundConnection(mSoundConnectionsCount.intValue - 1, sc);
		
		RecalculateBools();
		SceneView.RepaintAll();
	}
	
	void RemoveSoundConnection(int index)
	{
		#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterSceneUndo("Remove SoundConnection");
		#else
		Undo.RecordObject(script, "Remove SoundConnection");
		#endif
		for( int i = index; i < mSoundConnectionsCount.intValue - 1; i++)
			SetSoundConnection(i, GetSoundConnection(i + 1));
		
		mSoundConnectionsCount.intValue--;
		
		RecalculateBools();
		SceneView.RepaintAll();
	}
	
	void RecalculateBools()
	{
		for(int i = 0; i < mSoundConnectionsCount.intValue ; i++)
		{
			SerializedObject sc = new SerializedObject(mObject.FindProperty(string.Format(mListData, i)).objectReferenceValue);
			var levelName = sc.FindProperty(mSCLevel).stringValue;
			
			if(!script.storage.songStatus.ContainsKey(levelName))
			{
				script.storage.songStatus.Add(levelName,EditorVariableStorage.HIDE);
			}
		}
	}
	
	List<SoundConnection> GetSoundConnectionList()
	{
		var listSC = new List<SoundConnection>();
		for(int i = 0 ; i < mSoundConnectionsCount.intValue ; i++)
		{
			listSC.Add(GetSoundConnection(i));
		}
		return listSC;
	}
	
	SoundConnection GetSoundConnection( int index )
	{
		return mObject.FindProperty(string.Format(mListData,index)).objectReferenceValue as SoundConnection;
	}
	
	void SetSoundConnection ( int index, SoundConnection sc)
	{
		mObject.FindProperty(string.Format(mListData,index)).objectReferenceValue = sc;
	}
	
	string[] GetAvailableLevelNamesForAddition()
	{
		List<string> availableScenes = new List<string>();
		foreach ( EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if(scene.enabled)
			{
				string sceneNameBare = scene.path.Substring(scene.path.LastIndexOf('/') + 1, (scene.path.LastIndexOf('.') - scene.path.LastIndexOf('/')) - 1);
				if(!GetSoundConnectionList().Find(delegate(SoundConnection obj) { return obj.level == sceneNameBare; }))
				{
					availableScenes.Add(sceneNameBare);
				}	
			}
		}
		availableScenes.Add("<custom>");
		return availableScenes.ToArray();
	}
	
	bool IsStringSceneName(string nameToTest)
	{
		foreach ( EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if(scene.enabled)
			{
				string sceneNameBare = scene.path.Substring(scene.path.LastIndexOf('/') + 1, (scene.path.LastIndexOf('.') - scene.path.LastIndexOf('/')) - 1);
				if(nameToTest == sceneNameBare)
					return true;
			}
		}
		return false;
	}
	
	bool IsStringAlreadyTaken(string nameToTest)
	{
		if(GetSoundConnectionList().Find(delegate(SoundConnection obj) { return obj.level == nameToTest; }))
		{
			return true;
		}
		return false;
	}
	
	bool SoundCanMoveUp(AudioClip sound, List<AudioClip> list)
	{
		if(!list.Contains(sound)) return false;
		int index = list.IndexOf(sound);
		if(index == 0) return false;
		return true;
	}
	
	bool SoundCanMoveDown(AudioClip sound, List<AudioClip> list)
	{
		if(!list.Contains(sound)) return false;
		int index = list.IndexOf(sound);
		if(index == list.Count-1) return false;
		return true;
	}
	
	void SwapSounds(AudioClip sound1, AudioClip sound2, List<AudioClip> list)
	{
		if(!(list.Contains(sound1) && list.Contains(sound2))) return;
		int index1 = list.IndexOf(sound1);
		int index2 = list.IndexOf(sound2);
		list[index1] = sound2;
		list[index2] = sound1;
	}
	
	/*bool SoundCanMoveUp(SerializedObject obj, string dataString, int index)
	{
		if(!Contains(obj, dataString, index)) return false;
		
		int index = list.IndexOf(sound);
		if(index == 0) return false;
		return true;
	}
	
	bool SoundCanMoveDown(SerializedObject obj, string dataString, int index)
	{
		if(!list.Contains(sound)) return false;
		int index = list.IndexOf(sound);
		if(index == list.Count-1) return false;
		return true;
	}*/
	
	void SwapSounds(SerializedObject obj, string dataString, int index1, int index2)
	{
		#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterSceneUndo("Move Sounds");
		#else
		Undo.RecordObject(script, "Move Sounds");
		#endif
		
		if(!(Contains(obj,dataString,index1) && Contains(obj,dataString,index2))) return;
		Object tempClip = obj.FindProperty(string.Format(dataString, index1)).objectReferenceValue;
		obj.FindProperty(string.Format(dataString, index1)).objectReferenceValue = obj.FindProperty(string.Format(dataString, index2)).objectReferenceValue;
		obj.FindProperty(string.Format(dataString, index2)).objectReferenceValue = tempClip;
		
		obj.ApplyModifiedProperties();
	}
	
	void RemoveSound(SerializedObject obj, string dataString, string sizeString, int index)
	{
		#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterSceneUndo("Remove Sound");
		#else
		Undo.RecordObject(script, "Remove Sound");
		#endif
		
		SerializedProperty sizeProp = obj.FindProperty(sizeString);
		for( int i = index; i < sizeProp.intValue-1; i++)
			obj.FindProperty(string.Format(dataString, i)).objectReferenceValue = obj.FindProperty(string.Format(dataString, i+1)).objectReferenceValue;
		
		sizeProp.intValue--;
		
		obj.ApplyModifiedProperties();
		
		SceneView.RepaintAll();
	}
	
	void AddSound(SerializedObject obj, string dataString, string sizeString, AudioClip clip)
	{
		if(clip == null)
			return;
		
		#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterSceneUndo("Add Sound");
		#else
		Undo.RecordObject(script, "Add Sound");
		#endif
		
		SerializedProperty sizeProp = obj.FindProperty(sizeString);
		sizeProp.intValue++;
		obj.FindProperty(string.Format(dataString, sizeProp.intValue - 1)).objectReferenceValue = clip;
			
		obj.ApplyModifiedProperties();
		
		SceneView.RepaintAll();
	}
	
	bool Contains(SerializedObject obj, string dataString, int index)
	{
		return (obj.FindProperty(string.Format(mSCSoundToPlay, index)).objectReferenceValue != null);
	}
	
	private void CheckUndo()
    {
		#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
        Event e = Event.current;
 
        if ( e.type == EventType.MouseDown && e.button == 0 || e.type == EventType.KeyUp && ( e.keyCode == KeyCode.Tab ) ) {
            Undo.SetSnapshotTarget( new Object[]{proxy.target, this}, "Modify Delay" );
            Undo.CreateSnapshot();
            Undo.ClearSnapshotTarget();
            listeningForGuiChanges = true;
            guiChanged = false;
        }
 
        if ( listeningForGuiChanges && guiChanged ) {
            Undo.SetSnapshotTarget( new Object[]{proxy.target, this}, "Modify Delay" );
            Undo.RegisterSnapshot();
            Undo.ClearSnapshotTarget();
            listeningForGuiChanges = false;
        }
		#endif
    }
	#endregion
					
	#region Informational
	string GetPlayMethodDescription(SoundManager.PlayMethod method)	
	{
		string desc = "";
		switch(method)
		{
		case SoundManager.PlayMethod.ContinuousPlayThrough:
			desc = "Repeat All In Order";
			break;
		case SoundManager.PlayMethod.OncePlayThrough:
			desc = "Play All In Order Once";
			break;
		case SoundManager.PlayMethod.ShufflePlayThrough:
			desc = "Repeat All Shuffled";
			break;
		case SoundManager.PlayMethod.ContinuousPlayThroughWithDelay:
			desc = "Repeat All In Order With Delay Between Songs";
			break;
		case SoundManager.PlayMethod.OncePlayThroughWithDelay:
			desc = "Play All In Order Once With Delay Between Songs";
			break;
		case SoundManager.PlayMethod.ShufflePlayThroughWithDelay:
			desc = "Repeat All Shuggled With Delay Between Songs";
			break;
		case SoundManager.PlayMethod.ContinuousPlayThroughWithRandomDelayInRange:
			desc = "Repeat All In Order With Delay Between Songs Within A Range";
			break;
		case SoundManager.PlayMethod.OncePlayThroughWithRandomDelayInRange:
			desc = "Play All In Order Once With Delay Between Songs Within A Range";
			break;
		case SoundManager.PlayMethod.ShufflePlayThroughWithRandomDelayInRange:
			desc = "Repeat All Shuffled With Delay Between Songs Within A Range";
			break;
		default:
			break;
		}
		return desc;
	}
	#endregion
}

