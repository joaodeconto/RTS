using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;

[CustomEditor(typeof(ObjectQualitySettings))]
public class ObjectQualitySettingsInspector : Editor
{
	ObjectQualitySettings _target;
	
	string[] qualityNames;
	List<int> selectedComponents;
	
	void OnEnable ()
	{
		_target = target as ObjectQualitySettings;
		qualityNames = QualitySettings.names;
		selectedComponents = new List<int>();
	}
	
	public override void OnInspectorGUI ()
	{
//		foreach (string qualityName in qualityNames)
//		{
//			EditorGUILayout.LabelField (qualityName);
//		}
		
		if (GUILayout.Button ("Add"))
		{
			_target.objects.Add (new ObjectQualitySettings.ObjectQuality ());
		}
		
		if (_target.objects.Count == 0) return;
		
		foreach (ObjectQualitySettings.ObjectQuality objectQuality in _target.objects)
		{
			EditorGUILayout.BeginVertical ("box");
			EditorGUI.indentLevel = 0;
			EditorGUILayout.LabelField ("GameObject Reference:");
			objectQuality.gameObject = EditorGUILayout.ObjectField (objectQuality.gameObject, typeof(GameObject)) as GameObject;
			EditorGUILayout.Space ();
			
			if (objectQuality.gameObject != null)
			{
				Component[] comps = objectQuality.gameObject.GetComponents (typeof(Component));
				objectQuality.components = new List<ObjectQualitySettings.ComponentQuality>();
				foreach (Component c in comps)
				{
					if (c.GetType ().GetFields ().Length != 0)
					{
						if (!objectQuality.Contains (c.GetType ()))
						{
							objectQuality.components.Add (new ObjectQualitySettings.ComponentQuality (c.GetType (), c.GetType ().GetFields ()));
						}
					}
				}
				
				if (objectQuality.selectedComponent >= objectQuality.components.Count)
					objectQuality.selectedComponent = objectQuality.components.Count-1;
				
				EditorGUI.indentLevel = 1;
				if (objectQuality.components.Count != 0)
				{
					EditorGUILayout.LabelField ("Component to Selected:");
					EditorGUILayout.BeginHorizontal ();
					{
						objectQuality.selectedComponent = EditorGUILayout.Popup (objectQuality.selectedComponent, ComponentsNames (objectQuality));
						if (GUILayout.Button ("Apply"))
						{
							objectQuality.selectedComponents.Add (objectQuality.components[objectQuality.selectedComponent]);
						}
					}
					EditorGUILayout.EndHorizontal ();
				}
				
				if (objectQuality.selectedComponents.Count != 0)
				{
					EditorGUI.indentLevel = 2;
					EditorGUILayout.LabelField ("Component to Selected:");
					for (int i = 0; i != objectQuality.selectedComponents.Count; i++)
					{
						EditorGUILayout.BeginHorizontal ();
						{
							EditorGUILayout.LabelField (objectQuality.selectedComponents[i].component.ToString ());
							if (GUILayout.Button ("-"))
							{
								objectQuality.selectedComponents.RemoveAt (i);
								i--;
							}
						}
						EditorGUILayout.EndHorizontal ();
						
						EditorGUI.indentLevel = 3;
						EditorGUILayout.LabelField ("Fields:");
						EditorGUILayout.BeginHorizontal ();
						{
							objectQuality.selectedComponents[i].selectedField = 
								EditorGUILayout.Popup (objectQuality.selectedComponents[i].selectedField, FieldsNames (objectQuality.selectedComponents[i]));
							if (GUILayout.Button ("Apply"))
							{
								objectQuality.selectedComponents[i].selectedFields.Add (
									objectQuality.selectedComponents[i].fields[objectQuality.selectedComponents[i].selectedField]
								);
							}
						}
					}
				}
				
				EditorGUILayout.Space ();
			}
			
//			if (objectQuality.gameObject != null)
//			{
//				Component[] comps = objectQuality.gameObject.GetComponents (typeof(Component));
//				foreach (Component c in comps)
//				{
//					if (c.GetType ().GetFields ().Length != 0) {
//						objectQuality.components.Add (c.GetType ());
//					}
//				}
//				
//				if (_target.components.Count == 0)
//				{
//					_target.selectedComp = 0;
//					return;
//				}
//				
//				if (_target.selectedComp > _target.components.Count)
//				{
//					_target.selectedComp = _target.components.Count-1;
//					return;
//				}
//				
//				GUILayout.Label ("Components:");
//				_target.selectedComp = EditorGUILayout.Popup (_target.selectedComp, ComponentsNames ());
//				GUILayout.Space (5f);
//				
//				if (_target.selectedComp != 0)
//				{
//					_target.vars = new List<FieldInfo> ();
//					string type = "";
//					foreach (FieldInfo f in _target.components[_target.selectedComp-1].GetFields())
//					{
//						if (f.FieldType == type.GetType ())
//						{
//							_target.vars.Add (f);
//						}
//					}
//					
//					if (_target.vars.Count == 0 ||
//						_target.selectedVar > _target.vars.Count)
//					{
//						_target.selectedVar = 0;
//						return;
//					}
//					
//					GUILayout.Label ("String Variables:");
//					_target.selectedVar = EditorGUILayout.Popup (_target.selectedVar, VarsNames ());
//					GUILayout.Space (5f);
//					if (_target.selectedVar != 0)
//					{
//						string val = _target.vars [_target.selectedVar - 1].GetValue (
//										_target.gameObject.GetComponent (
//										_target.components [_target.selectedComp - 1])).ToString ();
//						_target.label = val;
//					}
//				}
//			}
			EditorGUILayout.EndVertical ();
		}
		
		for (int i = 0; i != qualityNames.Length; i++)
		{
		}
	}
	
	string[] ComponentsNames (ObjectQualitySettings.ObjectQuality obj)
	{
		string[] names = new string[obj.components.Count + 1];
		int i = 0;
		foreach (ObjectQualitySettings.ComponentQuality c in obj.components) {
			names [i] = c.component.Name;
			++i;
		}
		return names;
	}
	
	string[] FieldsNames (ObjectQualitySettings.ComponentQuality comp)
	{
		string[] names = new string[comp.fields.Count + 1];
		int i = 0;
		foreach (FieldInfo f in comp.fields)
		{
			names [i] = f.Name;
			++i;
		}
		return names;
	}
}
