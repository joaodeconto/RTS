using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables
		EditorBuildSettingsScene[] mScenesInBuildSettings;
		bool Tools_ShowScenesList = false;
		#endregion

		#region GUI

		void OnGUI_ScenesList( bool SmallSize = false )
		{
			mScenesInBuildSettings = EditorBuildSettings.scenes;

			if (!Tools_ShowScenesList)
			{
				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
					Tools_ShowScenesList = GUILayout.Toggle(Tools_ShowScenesList, "", EditorStyles.foldout, GUILayout.ExpandWidth(false));

					string sceneText = string.Empty;
					if (mSelectedScenes.Count==1 && mSelectedScenes[0]==EditorApplication.currentScene)
						sceneText = "Current Scene";
					else
						sceneText = string.Format("{0} of {1} Scenes", mSelectedScenes.Count, mScenesInBuildSettings.Length);
					var stl = new GUIStyle("toolbarbutton");
					stl.richText = true;
					if (GUILayout.Button("Scenes to Parse: <i>"+sceneText+"</i>", stl))
						Tools_ShowScenesList = true;
				GUILayout.EndHorizontal();
				GUILayout.Space(10);
				return;
			}
			OnGUI_ScenesList_TitleBar();

			mScrollPos_BuildScenes = GUILayout.BeginScrollView( mScrollPos_BuildScenes, "AS TextArea", GUILayout.Height ( SmallSize ? 100 : 200));
			
			bool bShowCurrentScene = true;
			for (int i=0, imax=mScenesInBuildSettings.Length; i<imax; ++i)
			{
				GUILayout.BeginHorizontal();
				
				OnGUI_SelectableToogleListItem( mScenesInBuildSettings[i].path, ref mSelectedScenes, "OL Toggle" );
				
				bool bSelected = mSelectedScenes.Contains(mScenesInBuildSettings[i].path);
				GUI.color = (bSelected ? Color.white : Color.Lerp(Color.gray, Color.white, 0.5f));
				if (GUILayout.Button (mScenesInBuildSettings[i].path, "Label"))
				{
					if (mSelectedScenes.Contains(mScenesInBuildSettings[i].path))
						mSelectedScenes.Remove(mScenesInBuildSettings[i].path);
					else
						mSelectedScenes.Add(mScenesInBuildSettings[i].path);
				}
				GUI.color = Color.white;
				
				if (mScenesInBuildSettings[i].path == EditorApplication.currentScene)
					bShowCurrentScene = false;
				
				GUILayout.EndHorizontal();
			}
			
			if (bShowCurrentScene) 
			{
				GUILayout.BeginHorizontal();
				OnGUI_SelectableToogleListItem( EditorApplication.currentScene, ref mSelectedScenes, "OL Toggle" );
				
				bool bSelected = mSelectedScenes.Contains(EditorApplication.currentScene);
				GUI.color = (bSelected ? Color.white : Color.Lerp(Color.gray, Color.white, 0.5f));
				
				if (GUILayout.Button (EditorApplication.currentScene, "Label"))
				{
					if (mSelectedScenes.Contains(EditorApplication.currentScene))
						mSelectedScenes.Remove(EditorApplication.currentScene);
					else
						mSelectedScenes.Add(EditorApplication.currentScene);
				}
				GUI.color = Color.white;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}

		void OnGUI_ScenesList_TitleBar()
		{
			GUILayout.BeginHorizontal();
				Tools_ShowScenesList = GUILayout.Toggle(Tools_ShowScenesList, "", EditorStyles.foldout, GUILayout.ExpandWidth(false));

				if (GUILayout.Button("Scenes to Parse:", "toolbarbutton"))
					Tools_ShowScenesList = false;

				if (GUILayout.Button("All", "toolbarbutton", GUILayout.ExpandWidth(false)))  
				{ 
					mSelectedScenes.Clear(); 
					for (int i=0, imax=mScenesInBuildSettings.Length; i<imax; ++i)
						mSelectedScenes.Add (mScenesInBuildSettings[i].path);
					if (!mSelectedScenes.Contains(EditorApplication.currentScene))
						mSelectedScenes.Add (EditorApplication.currentScene);
				}
				if (GUILayout.Button("None", "toolbarbutton", GUILayout.ExpandWidth(false))) { mSelectedScenes.Clear(); }
				if (GUILayout.Button("Used", "toolbarbutton", GUILayout.ExpandWidth(false)))  
				{ 
					mSelectedScenes.Clear(); 
					for (int i=0, imax=mScenesInBuildSettings.Length; i<imax; ++i)
						if (mScenesInBuildSettings[i].enabled)
							mSelectedScenes.Add (mScenesInBuildSettings[i].path);
				}
				if (GUILayout.Button("Current", "toolbarbutton", GUILayout.ExpandWidth(false)))  
				{ 
					mSelectedScenes.Clear(); 
					mSelectedScenes.Add (EditorApplication.currentScene);
				}
			GUILayout.EndHorizontal();
		}
		
		void SelectUsedScenes()
		{
			mSelectedScenes.Clear();
			for (int i=0, imax=mScenesInBuildSettings.Length; i<imax; ++i)
				if (mScenesInBuildSettings[i].enabled)
					mSelectedScenes.Add( mScenesInBuildSettings[i].path );
		}
		
		#endregion
	
		#region Iterate thru the Scenes

		delegate void Delegate0();

		void ExecuteActionOnSelectedScenes( Delegate0 Action )
		{
			string InitialScene = EditorApplication.currentScene;
			
			if (mSelectedScenes.Count<=0)
				mSelectedScenes.Add (InitialScene);
			
			bool HasSaved = false;
			
			foreach (string ScenePath in mSelectedScenes)
			{
				if (ScenePath != EditorApplication.currentScene)
				{
					if (!HasSaved)	// Saving the initial scene to avoid loosing changes
					{
						EditorApplication.SaveScene();
						HasSaved = true;
					}
					EditorApplication.OpenScene( ScenePath );
				}

				Action();
			}
			
			if (InitialScene != EditorApplication.currentScene)
				EditorApplication.OpenScene( InitialScene );
			
			if (mLanguageSource)
				Selection.activeObject = mLanguageSource.gameObject;
		}
		#endregion
	}
}