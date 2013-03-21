using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class InteractionController : MonoBehaviour
{
	public delegate void InteractionCallback(Vector3 position);

	protected TouchController touchController;
	protected TroopController troopController;
	protected GameplayManager gameplayManager;

	private Stack<InteractionCallback> stackInteractionCallbacks;

	public void Init ()
	{
		touchController = ComponentGetter.Get<TouchController>();
		troopController = ComponentGetter.Get<TroopController>();
		gameplayManager = ComponentGetter.Get<GameplayManager>();

		stackInteractionCallbacks = new Stack<InteractionCallback>();
	}

	public void AddCallback(TouchController.IdTouch id, InteractionCallback ic)
	{
		stackInteractionCallbacks.Push(ic);
	}

	public bool HaveCallbacksForTouchId(TouchController.IdTouch id)
	{
		return (stackInteractionCallbacks.Count != 0);
	}

	// Update is called once per frame
	void Update ()
	{
		if (touchController.touchType != TouchController.TouchType.Ended)
			return;

#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR
		if (!touchController.DragOn)
		{
			if (touchController.idTouch == TouchController.IdTouch.Id1 )
			{
				Interaction (touchController.GetFinalRaycastHit.transform);
			}
		}
#else
		switch (touchController.idTouch)
		{
		case TouchController.IdTouch.Id1:
			Interaction (touchController.GetFinalRaycastHit.transform);
			break;
		case TouchController.IdTouch.Id0:
			while(stackInteractionCallbacks.Count != 0)
			{
				InteractionCallback ic = stackInteractionCallbacks.Pop();

				if(ic != null)
				{
					ic(touchController.GetFinalPoint);
				}
			}
			break;
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
				foreach (Unit unit in troopController.selectedSoldiers)
				{
					if (unit.GetType() == typeof(Worker))
					{
						Worker worker = unit as Worker;
						FactoryBase factory = hit.GetComponent<FactoryBase>();

						if (!factory.wasBuilt)
						{
							worker.SetMoveToFactory(factory);
						}
						else if (worker.hasResource)
						{
							if (factory.receiveResource == worker.resource.type)
							{
								worker.SetMoveToFactory(factory);
							}
							else if (hit.GetComponent<MainFactory>() != null)
							{
								worker.SetMoveToFactory(factory);
							}
						}
						else if (factory.IsNeededRepair)
						{
							worker.SetMoveToFactory(factory);
						}
					}
				}
			}
			return;
		}
		else
		{
			foreach (Unit unit in troopController.selectedSoldiers)
			{
				if (unit.GetType() == typeof(Worker))
				{
					Worker worker = unit as Worker;
					worker.SetMoveToFactory (null);
				}
			}
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

		if (hit.CompareTag ("Unit"))
		{
			if (!gameplayManager.IsSameTeam (hit.GetComponent<Unit> ()))
			{
				troopController.AttackTroop (hit.gameObject);
			}
			return;
		}

		troopController.MoveTroop (touchController.GetFinalPoint);
	}
}
