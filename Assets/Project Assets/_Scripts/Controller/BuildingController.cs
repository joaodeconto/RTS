using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildingController : MonoBehaviour
{
	public List<GameObject> buildings;
	
	internal GameObject selectedBuilding;
	
	private TouchController touchController;
	
	public void Init ()
	{
		touchController = GameController.GetInstance().GetTouchController();
	}
	
	public void AddBuilding (GameObject building)
	{
		buildings.Add (building);
	}
	
	public void RemoveBuilding (GameObject building)
	{
		buildings.Remove (building);
	}

}
