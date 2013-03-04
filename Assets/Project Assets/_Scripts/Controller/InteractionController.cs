using UnityEngine;
using System.Collections;

using Visiorama;

public class InteractionController : MonoBehaviour
{

	protected TouchController touchController;
	protected TroopController troopController;
	protected GameplayManager gameplayManager;

	public void Init ()
	{
		touchController = ComponentGetter.Get<TouchController>();
		troopController = ComponentGetter.Get<TroopController>();
		gameplayManager = ComponentGetter.Get<GameplayManager>();
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
				Interaction (touchController.GetFinalRay);
			}
		}
#else
		if (touchController.touchType == TouchController.TouchType.Ended)
		{
			if (touchController.idTouch == TouchController.IdTouch.Id1)
			{
				Interaction (touchController.GetFinalRay);
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
			if (hit.transform.CompareTag ("Factory"))
			{
				if (!gameplayManager.IsSameTeam (hit.transform.GetComponent<FactoryBase> ()))
				{
					troopController.AttackTroop (hit.transform.gameObject);
				}
				return;
			}
			if (hit.transform.CompareTag ("Unit"))
			{
				if (!gameplayManager.IsSameTeam (hit.transform.GetComponent<Unit> ()))
				{
					troopController.AttackTroop (hit.transform.gameObject);
				}
				return;
			}
		}

		troopController.MoveTroop (touchController.GetFinalPoint);
	}
}
