using UnityEngine;
using System.Collections;

public class InteractionController : MonoBehaviour
{
	
	protected TouchController touchController;
	protected TroopController troopController;
	protected BuildingController buildingController;
	
	public void Init ()
	{
		touchController = GameController.GetInstance().GetTouchController();
		troopController = GameController.GetInstance().GetTroopController();
		buildingController = GameController.GetInstance().GetBuildingController();
	}
	
	// Update is called once per frame
	void Update ()
	{
#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR
		if (touchController.touchType == TouchController.TouchType.Ended
			&& !touchController.DragOn)
		{
			if (touchController.idTouch == TouchController.IdTouch.Id0)
			{
				Interaction (touchController.GetFinalRaycast);
			}
		}
#else
		if (touchController.touchType == TouchController.TouchType.Ended)
		{
			if (touchController.idTouch == TouchController.IdTouch.Id1)
			{
				Interaction (touchController.GetFinalRaycast);
			}
		}
#endif
	}
	
	void Interaction (Ray raycast)
	{
		if (troopController.selectedSoldiers.Count == 0) return;
		
		RaycastHit hit;
				
		if (Physics.Raycast (raycast, out hit))
		{
			if (hit.transform.CompareTag ("Player"))
			{
				return;
			}
			if (hit.transform.CompareTag ("Building"))
			{
				return;
			}
			if (hit.transform.CompareTag ("Enemy"))
			{
				troopController.AttackTroop (hit.transform.gameObject);
				return;
			}
		}
		
		troopController.MoveTroop (touchController.GetFinalPoint);
	}
}
