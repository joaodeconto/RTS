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
		if (touchController.touchType != TouchController.TouchType.Ended)
			return;

#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR
		if ((touchController.idTouch == TouchController.IdTouch.Id0) && (!touchController.DragOn))
		{
			Interaction (touchController.GetFinalRaycastHit.transform);
		}
#else
		if (touchController.idTouch == TouchController.IdTouch.Id1)
		{
			Interaction (touchController.GetFinalRaycastHit.transform);
		}
#endif
	}

	void Interaction (Transform hit)
	{
		if (troopController.selectedSoldiers.Count == 0) return;

		if (hit.CompareTag ("Factory"))
		{
			if (!gameplayManager.IsSameTeam (hit.GetComponent<FactoryBase> ()))
			{
				troopController.AttackTroop (hit.transform.gameObject);
			}
			else
			{
				if (hit.GetComponent<MainFactory>() != null)
				{
					foreach (Unit unit in troopController.selectedSoldiers)
					{
						if (unit.GetType() == typeof(Worker))
						{
							Worker worker = unit as Worker;
							if (worker.hasResource)
							{
								worker.SetMoveToFactory(hit.GetComponent<FactoryBase>());
							}
						}
					}
				}
				else
				{
					Debug.Log ("HERE");
					foreach (Unit unit in troopController.selectedSoldiers)
					{
						if (unit.GetType() == typeof(Worker))
						{
							Worker worker = unit as Worker;
							FactoryBase factory = hit.GetComponent<FactoryBase>();

							if (!factory.wasBuilt)
							{
								worker.SetMoveToFactory(hit.GetComponent<FactoryBase>());
							}
						}
					}
				}
			}
			return;
		}
		if (hit.CompareTag ("Unit"))
		{
			if (!gameplayManager.IsSameTeam (hit.GetComponent<Unit> ()))
			{
				troopController.AttackTroop (hit.gameObject);
			}
			return;
		}

		if (hit.GetComponent<Resource> () != null)
		{
			foreach (Unit unit in troopController.selectedSoldiers)
			{
				if (unit.GetType() == typeof(Worker))
				{
					Worker worker = unit as Worker;
					worker.SetResource(hit.GetComponent<Resource> ());
				}
			}
		}
		else
		{
			foreach (Unit unit in troopController.selectedSoldiers)
			{
				if (unit.GetType() == typeof(Worker))
				{
					Worker worker = unit as Worker;
					if (!worker.hasResource)
					{
						if (worker.resource != null)
						{
							worker.SetResource (null);
						}
					}
				}
			}
		}

		troopController.MoveTroop (touchController.GetFinalPoint);
	}
}
