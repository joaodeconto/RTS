using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;

public class ObjectQualitySettings : MonoBehaviour {
	
	[System.Serializable]
	public class ComponentQuality
	{
		public System.Type component;
		public List<FieldInfo> fields = new List<FieldInfo> ();
		public List<FieldInfo> selectedFields = new List<FieldInfo> ();
		public int selectedField;
		
		public ComponentQuality (System.Type component, List<FieldInfo> fields)
		{
			this.component = component;
			this.fields = fields;
			this.selectedField = 0;
		}
		
		public ComponentQuality (System.Type component, FieldInfo[] fields)
		{
			this.component = component;
			this.fields = new List<FieldInfo>(fields);
		}
	}
	
	[System.Serializable]
	public class ObjectQuality
	{
		public GameObject gameObject;
		public List<ComponentQuality> components = new List<ComponentQuality> ();
		public List<ComponentQuality> selectedComponents = new List<ComponentQuality> ();
		public int selectedComponent;
		
		public ObjectQuality ()
		{
			gameObject = null;
			selectedComponents = components = new List<ComponentQuality> ();
			selectedComponent = 0;
		}
		
		public bool Contains (System.Type component)
		{
			if ((object) component == null)
			{
				for (int index = 0; index < selectedComponents.Count; ++index)
				{
					if ((object) selectedComponents[index].component == null)
						return true;
				}
				return false;
			}
			else
			{
				EqualityComparer<System.Type> @default = EqualityComparer<System.Type>.Default;
				for (int index = 0; index < selectedComponents.Count; ++index)
				{
					if (@default.Equals(selectedComponents[index].component, component))
						return true;
				}
				return false;
			}
		}
	}
	
	public List<ObjectQuality> objects = new List<ObjectQuality> ();
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
