using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SoundManager))]
public class ProxySoundEditor : Editor {
	
	SoundManagerEditor owner;
	
	public ProxySoundEditor()
	{
		CreateOwner();
	}
	
	public override void OnInspectorGUI ()
    {
        owner.OnInspectorGUI ();
    }
	
	public void OnEnable ()
	{
		if(!owner)
			CreateOwner();
		owner.Enable();
	}
	
	public void OnDisable ()
	{
		if(owner)
		{
			owner.Disable();
			DestroyImmediate(owner);
		}
	}
	
	public void CreateOwner()
	{
		owner = ScriptableObject.CreateInstance<SoundManagerEditor>();
		owner.ProxyInitialize(this);
	}
}
