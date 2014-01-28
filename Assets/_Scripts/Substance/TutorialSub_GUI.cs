using UnityEngine;
using System;
using System.Collections;

public class TutorialSub_GUI : MonoBehaviour 
{
	#region Public Fields
	public Rect mainGuiRect;
	public Transform subMesh;
	public Vector2 scrollVal;
	public Texture2D colorPicker;
	#endregion

	#region Private Fields
	private ProceduralPropertyDescription[] curProperties;
	private ProceduralMaterial healthSubstance;
	#endregion

	// Use this for initialization
	void Start ()
	{
		healthSubstance = subMesh.renderer.sharedMaterial as ProceduralMaterial;
		curProperties = healthSubstance.GetProceduralPropertyDescriptions();
	
//		int i=0;
//		while(i < curProperties.Length)
//		{
//			Debug.Log(curProperties[i].name.ToString() + "is of type : " + curProperties[i].type.ToString());
//			i++;
//		}
	}

	void OnGUI()
	{
		mainGuiRect = new Rect (mainGuiRect.x, mainGuiRect.y, Screen.width * 0.25f, Screen.height);

		if(healthSubstance)
		{
			GUI.Window(0, mainGuiRect, HealthPropertiesGUI, "Engine Tweaks");
		}
		else
		{
			Debug.LogWarning("can't find substance on this transform: " + subMesh.name.ToString());
		}
	}

	void HealthPropertiesGUI(int guiID)
	{
		//start GUI Layout
		scrollVal = GUILayout.BeginScrollView(scrollVal);

		//loop through properties
		int i = 0;
		while(i<curProperties.Length)
		{
			ProceduralPropertyDescription curProperty = curProperties[i];
			ProceduralPropertyType curType = curProperties[i].type;

			//create sliders for the floats
			if(curType == ProceduralPropertyType.Float)
			{
				if(curProperty.hasRange)
				{

				GUILayout.Label (curProperty.name);
				float curFloat = healthSubstance.GetProceduralFloat(curProperty.name);
				float oldFloat = curFloat;
				curFloat = GUILayout.HorizontalSlider(curFloat, curProperty.minimum, curProperty.maximum);
				
					if(curFloat != oldFloat)
					{
						healthSubstance.SetProceduralFloat(curProperty.name, curFloat);
					}
				}
			}

			else if(curType == ProceduralPropertyType.Color4)
			{
				GUILayout.Label(curProperty.name);
				Color curColor = healthSubstance.GetProceduralColor(curProperty.name);
				Color oldColor = curColor;

				Rect curRect = GUILayoutUtility.GetLastRect();

				if(GUILayout.RepeatButton(colorPicker))
				{
					Vector2 mousePosition = Event.current.mousePosition;
					float currentPickerPosX = mousePosition.x - curRect.x;
					float currentPickerPosY = mousePosition.y - curRect.y;

					int x = Convert.ToInt32 (currentPickerPosX);
					int y = Convert.ToInt32 (currentPickerPosY);

					Color col = colorPicker.GetPixel(x, -y);
					curColor = col;
				}

				if(curColor != oldColor)
				{
					healthSubstance.SetProceduralColor(curProperty.name, curColor);
				}
			}
			i++;
		}

		//rebuild substance material
		healthSubstance.RebuildTextures();

		GUILayout.EndScrollView();
	}
}
