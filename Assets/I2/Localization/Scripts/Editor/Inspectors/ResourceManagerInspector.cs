using UnityEngine;
using UnityEditor;

namespace I2.Loc
{
	[CustomEditor(typeof(ResourceManager))]
	public class ResourceManagerInspector : Editor 
	{
		SerializedObject mSerializedObj;
		SerializedProperty mAssets;

		void OnEnable()
		{
			UpgradeManager.EnablePlugins();
			mSerializedObj = new SerializedObject( target as ResourceManager );
			mAssets = mSerializedObj.FindProperty("Assets");
		}

		public override void OnInspectorGUI()
		{
			GUILayout.Space(5);
			GUITools.DrawHeader("Assets:", true);
			GUITools.BeginContents();
				///GUILayout.Label ("Assets:");
				GUITools.DrawObjectsArray( mAssets );
			GUITools.EndContents();

			mSerializedObj.ApplyModifiedProperties();
		}
	}
}