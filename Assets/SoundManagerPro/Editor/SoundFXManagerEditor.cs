using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public partial class SoundManagerEditor : Editor {
	private SerializedProperty mStoredSFXsCount;
	private SerializedProperty mClipToGroupKeysCount;
	private SerializedProperty mClipToGroupValuesCount;
	private SerializedProperty mSFXGroupsCount;
	
	private static string mStoredSFXsCountPath = "storedSFXs.Array.size";
	private static string mStoredSFXsData = "storedSFXs.Array.data[{0}]";
	
	private static string mClipToGroupKeyPath = "clipToGroupKeys.Array.size";
	private static string mClipToGroupKeyData = "clipToGroupKeys.Array.data[{0}]";
	private static string mClipToGroupValuePath = "clipToGroupValues.Array.size";
	private static string mClipToGroupValueData = "clipToGroupValues.Array.data[{0}]";
	
	private static string mSFXGroupsPath = "sfxGroups.Array.size";
	private static string mSFXGroupsData = "sfxGroups.Array.data[{0}]";
	private static string mSFXgroupname = "groupName";
	private static string mSFXcapamount = "specificCapAmount";
	
	private SerializedProperty mOffSFX;
	private SerializedProperty mResourcesPath;
	private SerializedProperty mCapAmount;
	
	void EnableSFX()
	{
		mOffSFX = mObject.FindProperty("offTheSFX");
		mResourcesPath = mObject.FindProperty("RESOURCES_PATH");
		mCapAmount = mObject.FindProperty("CAP_AMOUNT");
		mStoredSFXsCount = mObject.FindProperty(mStoredSFXsCountPath);
		mClipToGroupKeysCount = mObject.FindProperty(mClipToGroupKeyPath);
		mClipToGroupValuesCount = mObject.FindProperty(mClipToGroupValuePath);
		mSFXGroupsCount = mObject.FindProperty(mSFXGroupsPath);

		//Only do this if the sfx groups get corrupted.  It will clear all the lists since dictionaries are not supported for serialization.
		//ClearAllLists();
	}
	
	void ShowSoundFX()
	{
		GUI.enabled = enabled;
		
		ShowSoundFXGroupsTitle();
		
		ShowSoundFXGroupsList();
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		ShowSoundFXTitle();
		
		ShowSoundFXList();
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		ShowAddSoundFX();
	}
	
	void ShowSoundFXTitle()
	{
		EditorGUILayout.LabelField("Stored SFX", EditorStyles.boldLabel);
	}
	
	void ShowSoundFXGroupsTitle()
	{
		EditorGUILayout.LabelField("SFX Groups", EditorStyles.boldLabel);
	}

	void ShowSoundFXGroupsList()
	{
		EditorGUILayout.BeginVertical();
		{
			EditorGUI.indentLevel++;
			if(script.storage.helpOn)
				EditorGUILayout.HelpBox("SFX Groups are used to:\n1. Access random clips in a set.\n2. Apply specific cap amounts to clips when using SoundManager.PlayCappedSFX." +
					"\n\n-Setting the cap amount to -1 will make a group use the default SFX Cap Amount\n\n-Setting the cap amount to 0 will make a group not use a cap at all.",MessageType.Info);
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("Group Name:", GUILayout.ExpandWidth(true));
				EditorGUILayout.LabelField("Cap:", GUILayout.Width(40f));
				script.storage.helpOn = GUILayout.Toggle(script.storage.helpOn, "?", GUI.skin.button, GUILayout.Width(35f));
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("", GUILayout.Width(10f));
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
			}
			EditorGUILayout.EndHorizontal();
			
			for(int i = 0 ; i < mSFXGroupsCount.intValue ; i++)
			{
				EditorGUILayout.BeginHorizontal();
				{
					SerializedObject grp = new SerializedObject(mObject.FindProperty(string.Format(mSFXGroupsData, i)).objectReferenceValue);
					
					if(grp != null)
					{					
						EditorGUILayout.LabelField(grp.FindProperty(mSFXgroupname).stringValue, GUILayout.ExpandWidth(true));
						int oldVal = grp.FindProperty(mSFXcapamount).intValue;
						bool hasChangedFromButton = false;
						bool isAuto = false, isNo = false;
						
						if(oldVal == -1)
							isAuto = true;
						else if(oldVal == 0)
							isNo = true;
						
						bool newAuto = GUILayout.Toggle(isAuto, "Auto Cap", GUI.skin.button, GUILayout.ExpandWidth(false));
						bool newNo = GUILayout.Toggle(isNo, "No Cap", GUI.skin.button, GUILayout.ExpandWidth(false));
						
						if(newAuto != isAuto && newAuto)
						{
							grp.FindProperty(mSFXcapamount).intValue = -1;
						}
						if(newNo != isNo && newNo)
						{
							grp.FindProperty(mSFXcapamount).intValue = 0;
						}
						
						int newVal = EditorGUILayout.IntField(grp.FindProperty(mSFXcapamount).intValue, GUILayout.Width(40f));
						
						if(newVal < -1) newVal = -1;
						if(oldVal != newVal)
							#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
							Undo.RegisterSceneUndo("Change Group Cap");
							#else
							Undo.RecordObject(script, "Change Group Cap");
							#endif
						grp.FindProperty(mSFXcapamount).intValue = newVal;
						if(GUI.changed || hasChangedFromButton)
							grp.ApplyModifiedProperties();
						EditorGUILayout.LabelField("", GUILayout.Width(10f));
						GUI.color = Color.red;
						if(GUILayout.Button("X", GUILayout.Width(20f)))
						{
							RemoveGroup(i);
						}
						GUI.color = Color.white;
					}
				}
				EditorGUILayout.EndHorizontal();				
			}
			ShowAddGroup();
			EditorGUI.indentLevel--;
			EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical();
	}
	
	void ShowAddGroup()
	{
		EditorGUI.indentLevel += 2;
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("Add a Group:");
			script.storage.groupToAdd = EditorGUILayout.TextField(script.storage.groupToAdd, GUILayout.ExpandWidth(true));
			GUI.color = softGreen;
			if(GUILayout.Button("add", GUILayout.Width(40f)))
			{
				AddGroup(script.storage.groupToAdd, -1);
				script.storage.groupToAdd = "";
				GUIUtility.keyboardControl = 0;
			}
			GUI.color = Color.white;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel -= 2;
	}
	
	void ShowSoundFXList()
	{
		string[] groups = GetAvailableGroups();
		EditorGUILayout.BeginVertical();
		{
			EditorGUI.indentLevel++;
			for(int i = 0 ; i < mStoredSFXsCount.intValue ; i++)
			{
				EditorGUILayout.BeginHorizontal();
				{
					SerializedProperty obj = mObject.FindProperty(string.Format(mStoredSFXsData, i));
				
					if(obj != null)
					{
						Object newClip = EditorGUILayout.ObjectField(obj.objectReferenceValue, typeof(AudioClip), false);
						if(newClip != null && newClip.GetType() == typeof(AudioClip))
							obj.objectReferenceValue = newClip;
						if(GUI.changed)
						{
							if(newClip == null)
								RemoveSFX(mObject, mStoredSFXsData, mStoredSFXsCountPath, i);
							else
								mObject.ApplyModifiedProperties();
						}
						
						string clipName = (obj.objectReferenceValue as AudioClip).name;
						int oldIndex = IndexOfKey(clipName);
						if(oldIndex >= 0) // if in a group, find index
							oldIndex = IndexOfGroup(mObject.FindProperty(string.Format(mClipToGroupValueData, oldIndex)).stringValue);
						
						if(oldIndex < 0) // if not in a group, set it to none
							oldIndex = 0;
						else //if in a group, add 1 to index to cover for None group type
							oldIndex++;
						
						EditorGUILayout.LabelField("Group:",GUILayout.Width(60f), GUILayout.ExpandWidth(false));
						int newIndex = EditorGUILayout.Popup(oldIndex, groups, EditorStyles.popup, GUILayout.Width(80f));
						if(oldIndex != newIndex)
							#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
							Undo.RegisterSceneUndo("Change Group");
							#else
							Undo.RecordObject(script, "Change Group");
							#endif
						
						string groupName = groups[newIndex];
						
						if(GUI.changed)
							ChangeGroup(clipName, oldIndex, newIndex, groupName);
						
						GUI.color = Color.red;
						if(GUILayout.Button("X", GUILayout.Width(20f)))
						{
							RemoveSFX(mObject, mStoredSFXsData, mStoredSFXsCountPath, i);
						}
						GUI.color = Color.white;
					}
				}
				EditorGUILayout.EndHorizontal();				
			}
			EditorGUI.indentLevel--;
			EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical();
	}
	
	void ShowAddSoundFX()
	{
		ShowAddSoundFXDropGUI();
		
		ShowAddSoundFXSelector();
	}
	
	void ShowAddSoundFXDropGUI()
	{
		GUI.color = softGreen;
		EditorGUILayout.BeginVertical();
		{
			var evt = Event.current;
			
			var dropArea = GUILayoutUtility.GetRect(0f,50f,GUILayout.ExpandWidth(true));
			GUI.Box (dropArea, "Drag SFX(s) Here");
			
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
						
						if(AlreadyContainsSFX(aC))
							Debug.LogError("You already have that sound effect(" +aC.name+ ") attached, or a sound effect with the same name.");
						else
							AddSFX(mObject, mStoredSFXsData, mStoredSFXsCountPath, aC);
					}
				}
				Event.current.Use();
				break;
			}
		}
		EditorGUILayout.EndVertical();
		GUI.color = Color.white;
	}
	
	void ShowAddSoundFXSelector()
	{
		EditorGUILayout.BeginHorizontal();
		{
			script.storage.editAddSFX = EditorGUILayout.ObjectField("Select A SFX:", script.storage.editAddSFX, typeof(AudioClip), false) as AudioClip;
			if(GUI.changed)
				HideVariables();
					
			GUI.color = softGreen;
			if(GUILayout.Button("add", GUILayout.Width(40f)))
			{
				if(AlreadyContainsSFX(script.storage.editAddSFX))
					Debug.LogError("You already have that sound effect(" +script.storage.editAddSFX.name+ ") attached, or a sound effect with the same name.");
				else
					AddSFX(mObject, mStoredSFXsData, mStoredSFXsCountPath, script.storage.editAddSFX);
				script.storage.editAddSFX = null;
				HideVariables();
			}
			GUI.color = Color.white;
		}
		EditorGUILayout.EndHorizontal();
	}
	
	void AddSFX(SerializedObject obj, string dataString, string sizeString, AudioClip clip)
	{
		if(clip == null) return;
		
		#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterSceneUndo("Add SFX");
		#else
		Undo.RecordObject(script, "Add SFX");
		#endif
		
		SerializedProperty sizeProp = obj.FindProperty(sizeString);
		sizeProp.intValue++;
		obj.FindProperty(string.Format(dataString, sizeProp.intValue - 1)).objectReferenceValue = clip;
			
		obj.ApplyModifiedProperties();
		
		SceneView.RepaintAll();
	}
	
	void RemoveSFX(SerializedObject obj, string dataString, string sizeString, int index)
	{
		#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterSceneUndo("Remove SFX");
		#else
		Undo.RecordObject(script, "Remove SFX");
		#endif
		
		string clipName = (obj.FindProperty(string.Format(dataString, index)).objectReferenceValue as AudioClip).name;
		
		SerializedProperty sizeProp = obj.FindProperty(sizeString);
		for( int i = index; i < sizeProp.intValue-1; i++)
			obj.FindProperty(string.Format(dataString, i)).objectReferenceValue = obj.FindProperty(string.Format(dataString, i+1)).objectReferenceValue;
		
		sizeProp.intValue--;
		
		if(IsInAGroup(clipName))
			RemoveFromGroup(clipName);
		
		obj.ApplyModifiedProperties();
		
		SceneView.RepaintAll();
	}
	
	void AddGroup(string groupName, int capAmount)
	{
		if(AlreadyContainsGroup(groupName))
			return;
		
		#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterSceneUndo("Add Group");
		#else
		Undo.RecordObject(script, "Add Group");
		#endif
		
		SFXGroup grp = ScriptableObject.CreateInstance<SFXGroup>();
		grp.Initialize(groupName, capAmount);
		
		mSFXGroupsCount.intValue++;
		mObject.FindProperty(string.Format(mSFXGroupsData, mSFXGroupsCount.intValue -1)).objectReferenceValue = grp;

		mObject.ApplyModifiedProperties();
		
		SceneView.RepaintAll();
	}
	
	void RemoveGroup(int index)
	{
		#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterSceneUndo("Remove Group");
		#else
		Undo.RecordObject(script, "Remove Group");
		#endif
		
		string groupName = (mObject.FindProperty(string.Format(mSFXGroupsData, index)).objectReferenceValue as SFXGroup).groupName;
		
		for( int i = index; i < mSFXGroupsCount.intValue-1; i++)
			mObject.FindProperty(string.Format(mSFXGroupsData, i)).objectReferenceValue = mObject.FindProperty(string.Format(mSFXGroupsData, i+1)).objectReferenceValue;
		
		mSFXGroupsCount.intValue--;
		
		RemoveAllInGroup(groupName);
		
		mObject.ApplyModifiedProperties();
		
		SceneView.RepaintAll();
	}
	
	void RemoveAllInGroup(string groupName)
	{
		for( int i = 0; i < mClipToGroupKeysCount.intValue; i++)
			if(mObject.FindProperty(string.Format(mClipToGroupValueData, i)).stringValue == groupName)
				RemoveFromGroup(mObject.FindProperty(string.Format(mClipToGroupKeyData, i)).stringValue);
	}
	
	string[] GetAvailableGroups()
	{
		List<string> result = new List<string>();
		result.Add("None");
		for( int i = 0; i < mSFXGroupsCount.intValue; i++)
			result.Add((mObject.FindProperty(string.Format(mSFXGroupsData, i)).objectReferenceValue as SFXGroup).groupName);
		return result.ToArray();
	}
	
	bool IsInAGroup(string clipname)
	{
		for( int i = 0; i < mClipToGroupKeysCount.intValue; i++)
			if(mObject.FindProperty(string.Format(mClipToGroupKeyData, i)).stringValue == clipname)
				return true;
		return false;
	}
	
	int IndexOfKey(string clipname)
	{
		for( int i = 0; i < mClipToGroupKeysCount.intValue; i++)
			if(mObject.FindProperty(string.Format(mClipToGroupKeyData, i)).stringValue == clipname)
				return i;
		return -1;
	}
	
	int IndexOfGroup(string groupname)
	{
		for( int i = 0; i < mSFXGroupsCount.intValue; i++)
			if((mObject.FindProperty(string.Format(mSFXGroupsData, i)).objectReferenceValue as SFXGroup).groupName == groupname)
				return i;
		return -1;
	}
	
	void ClearAllLists()
	{
		for( int i = 0; i <= mClipToGroupKeysCount.intValue; i++)
		{
			mClipToGroupKeysCount.intValue--;
			mClipToGroupValuesCount.intValue--;
		}
		for( int i = 0; i <= mSFXGroupsCount.intValue; i++)
		{
			mSFXGroupsCount.intValue--;
		}
		
		Debug.Log("Keys Count: " + mClipToGroupKeysCount.intValue);
		Debug.Log("Values Count: " + mClipToGroupValuesCount.intValue);
		Debug.Log("SFX Groups Count: " + mSFXGroupsCount.intValue);
	}
	
	void ChangeGroup(string clipName, int previousIndex, int nextIndex, string groupName)
	{
		if(previousIndex == 0) //wasnt in group, so add it
		{
			AddToGroup(clipName, groupName);
		}
		else if (nextIndex == 0) //was in group but now doesn't want to be
		{
			RemoveFromGroup(clipName);
		}
		else //just changing groups
		{
			int index = IndexOfKey(clipName);
			mObject.FindProperty(string.Format(mClipToGroupValueData,index)).stringValue = groupName;
		}
	}
	
	void AddToGroup(string clipName, string groupName)
	{
		if(IsInAGroup(clipName) || !AlreadyContainsGroup(groupName))
			return;
		
		mClipToGroupKeysCount.intValue++;
		mClipToGroupValuesCount.intValue++;
		
		mObject.FindProperty(string.Format(mClipToGroupKeyData, mClipToGroupKeysCount.intValue -1)).stringValue = clipName;
		mObject.FindProperty(string.Format(mClipToGroupValueData, mClipToGroupValuesCount.intValue -1)).stringValue = groupName;
		
		mObject.ApplyModifiedProperties();
		
		SceneView.RepaintAll();
	}
	
	void RemoveFromGroup(string clipName)
	{
		if(!IsInAGroup(clipName))
			return;
		int index = IndexOfKey(clipName);
		
		for(int i = index; i < mClipToGroupKeysCount.intValue-1; i++)
		{
			mObject.FindProperty(string.Format(mClipToGroupKeyData,i)).stringValue = mObject.FindProperty(string.Format(mClipToGroupKeyData,i+1)).stringValue;
			mObject.FindProperty(string.Format(mClipToGroupValueData,i)).stringValue = mObject.FindProperty(string.Format(mClipToGroupValueData,i+1)).stringValue;
		}
		
		mClipToGroupKeysCount.intValue--;
		mClipToGroupValuesCount.intValue--;
		
		mObject.ApplyModifiedProperties();
		
		SceneView.RepaintAll();
	}
	
	bool AlreadyContainsSFX(AudioClip clip)
	{
		for(int i = 0 ; i < mStoredSFXsCount.intValue ; i++)
		{
			SerializedProperty obj = mObject.FindProperty(string.Format(mStoredSFXsData, i));
		
			if(obj == null) continue;
			if((obj.objectReferenceValue as AudioClip).name == clip.name || obj.objectReferenceValue as AudioClip == clip)
				return true;			
		}
		return false;
	}
	
	bool AlreadyContainsGroup(string grpName)
	{
		for(int i = 0 ; i < script.sfxGroups.Count ; i++)
		{
			SerializedProperty grp = mObject.FindProperty(string.Format(mSFXGroupsData, i));
		
			if(grp == null) continue;
			if((grp.objectReferenceValue as SFXGroup).groupName == grpName)
				return true;
		}
		return false;
	}
}
