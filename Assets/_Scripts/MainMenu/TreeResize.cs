using UnityEngine;

using UnityEditor;

using System.Collections;

using System.Collections.Generic;



public class ResizeTrees : EditorWindow {
	
	
	
	[MenuItem("Terrain/ResizeTrees")]
	
	public static void ShowWindow()
		
	{
		
		EditorWindow.GetWindow(typeof(ResizeTrees));    
		
	}
	
	
	
	void OnGUI()
		
	{
		
		GUILayout.Label ("Resize Trees", EditorStyles.boldLabel);
		
		
		
		if (GUI.Button (new Rect(10, 60, 80,30), "Resize -"))
			
		{
			
			//List<TreeInstance> newTrees = new List<TreeInstance>(Terrain.activeTerrain.terrainData.treePrototypes);
			
			TreeInstance[] newTrees = Terrain.activeTerrain.terrainData.treeInstances;
			
			
			
			for (int i = 0; i < newTrees.Length; i++)
				
			{
				
				newTrees[i].heightScale *= 0.8f;
				
				newTrees[i].widthScale *= 0.8f;
				
				
				
				// Re asign it
				
				Terrain.activeTerrain.terrainData.treeInstances = newTrees;
				
				//Terrain.activeTerrain.terrainData.treeInstances[i] = ti;
				
			}


			
			
			
			Terrain.activeTerrain.terrainData.RefreshPrototypes ();
			
			Terrain.activeTerrain.Flush();
			
		}

		if (GUI.Button (new Rect(10, 30, 80,30), "Resize +"))
			
		{
			
			//List<TreeInstance> newTrees = new List<TreeInstance>(Terrain.activeTerrain.terrainData.treePrototypes);
			
			TreeInstance[] newTrees = Terrain.activeTerrain.terrainData.treeInstances;
			
			
			
			for (int i = 0; i < newTrees.Length; i++)
				
			{
				
				newTrees[i].heightScale *= 1.2f;
				
				newTrees[i].widthScale *= 1.2f;
				
				
				
				// Re asign it
				
				Terrain.activeTerrain.terrainData.treeInstances = newTrees;
				
				//Terrain.activeTerrain.terrainData.treeInstances[i] = ti;
				
			}
			
			
			
			
			
			Terrain.activeTerrain.terrainData.RefreshPrototypes ();
			
			Terrain.activeTerrain.Flush();
			
		}

	}
	
}