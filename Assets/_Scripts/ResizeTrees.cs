using UnityEngine;

using UnityEditor;

using System.Collections;

using System.Collections.Generic;

using Visiorama;



public class ResizeTrees : EditorWindow {
	
	
	
	[MenuItem("Terrain/ResizeTrees")]
	
	public static void ShowWindow()
		
	{
		
		EditorWindow.GetWindow(typeof(ResizeTrees));    
		
	}
	
	
	
	void OnGUI()
		
	{
		
		GUILayout.Label ("Resize Trees", EditorStyles.boldLabel);
		
		
		
		if (GUI.Button (new Rect(10, 90, 200,30), "Lower Trees"))
			
		{
			
			//List<TreeInstance> newTrees = new List<TreeInstance>(Terrain.activeTerrain.terrainData.treePrototypes);
			
			TreeInstance[] newTrees = Terrain.activeTerrain.terrainData.treeInstances;
			
			
			
			for (int i = 0; i < newTrees.Length; i++)
				
			{
				
				newTrees[i].heightScale *= 0.9f;
				
				newTrees[i].widthScale *= 0.9f;
				
				
				
				// Re asign it
				
				Terrain.activeTerrain.terrainData.treeInstances = newTrees;
				
				//Terrain.activeTerrain.terrainData.treeInstances[i] = ti;
				
			}
			
			
			
			Terrain.activeTerrain.terrainData.RefreshPrototypes ();
			
			Terrain.activeTerrain.Flush();
			
		}

		else if (GUI.Button (new Rect(10, 30, 200,30), "Raise Trees"))
			
		{
			
			//List<TreeInstance> newTrees = new List<TreeInstance>(Terrain.activeTerrain.terrainData.treePrototypes);
			
			TreeInstance[] newTrees = Terrain.activeTerrain.terrainData.treeInstances;
			
			
			
			for (int i = 0; i < newTrees.Length; i++)
				
			{
				
				newTrees[i].heightScale *= 1.1f;
				
				newTrees[i].widthScale *= 1.1f;
				
				
				
				// Re asign it
				
				Terrain.activeTerrain.terrainData.treeInstances = newTrees;
				
				//Terrain.activeTerrain.terrainData.treeInstances[i] = ti;
				
			}
			
			
			
			Terrain.activeTerrain.terrainData.RefreshPrototypes ();
			
			Terrain.activeTerrain.Flush();
			
		}
		
	}
}
