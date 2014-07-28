using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class InteractionController : MonoBehaviour
{
	public delegate void InteractionCallback(Vector3 position, Transform hit);

	public GameObject uiExitGameObject;

	protected TouchController touchController;
	protected StatsController statsController;
	protected GameplayManager gameplayManager;
	protected HUDController hudController;



	private Stack<InteractionCallback> stackInteractionCallbacks;

	public void Init ()
	{
		touchController = ComponentGetter.Get<TouchController> ();
		statsController = ComponentGetter.Get<StatsController> ();
		gameplayManager = ComponentGetter.Get<GameplayManager> ();
		hudController   = ComponentGetter.Get<HUDController> ();


		stackInteractionCallbacks = new Stack<InteractionCallback>();
	}

	public void AddCallback(TouchController.IdTouch id, InteractionCallback ic)
	{
		stackInteractionCallbacks.Push(ic);
	}

	public bool HasCallbacksForTouchId(TouchController.IdTouch id)
	{
		return (stackInteractionCallbacks.Count != 0);
	}

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Escape))
		{
			if (uiExitGameObject != null)
			{
				uiExitGameObject.SetActive (true);
			}
		}
		
#if (!UNITY_IPHONE && !UNITY_ANDROID) || UNITY_EDITOR
		if (touchController.touchType != TouchController.TouchType.Ended)
			return;

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
					ic(touchController.GetFinalPoint, touchController.GetFinalRaycastHit.transform);
				}
			}
			break;
		}
#endif
	}

	public void Interaction (Transform hit)
	{
		if (statsController.selectedStats.Count == 0) return;

		if (hit.transform.CompareTag ("TribeCenter")||hit.transform.CompareTag ("Obelisk")|| hit.transform.CompareTag ("ArmyStructure") || hit.transform.CompareTag ("House"))
		{
			FactoryBase factory = hit.GetComponent<FactoryBase> ();
			if(GameplayManager.mode == GameplayManager.Mode.Cooperative)
			{
				if (!gameplayManager.IsAlly (factory))
				{
					if (factory.IsVisible)
					{
						statsController.AttackTroop (factory.gameObject);
						return;
					}
				}
#if (!UNITY_IPHONE && !UNITY_ANDROID) || UNITY_EDITOR
				else
				{
					statsController.WorkerCheckFactory (factory);
					return;
				}
#endif
			}
			else
			{
				if (!gameplayManager.IsSameTeam (factory))
				{
					if (factory.IsVisible)
					{
						statsController.AttackTroop (factory.gameObject);
						return;
					}
				}
#if (!UNITY_IPHONE && !UNITY_ANDROID) || UNITY_EDITOR
				else
				{
					statsController.WorkerCheckFactory (factory);
					return;
				}
#endif
			}
		}
		else if (hit.CompareTag ("Unit"))
		{
			Unit unit = hit.GetComponent<Unit> ();

			if(GameplayManager.mode == GameplayManager.Mode.Cooperative)
			{
				if (gameplayManager.IsAlly (unit))
				{
					statsController.FollowTroop (unit);
					return;
				}
				else
				{
					if (unit.IsVisible)
					{
						statsController.AttackTroop (unit.gameObject);
						return;
					}
				}
			}
			else
			{
				if (gameplayManager.IsSameTeam (unit))
				{
					statsController.FollowTroop (unit);
					return;
				}
				else
				{
					if (unit.IsVisible)
					{
						statsController.AttackTroop (unit.gameObject);
						return;
					}
				}
			}
		}
		else if (hit.CompareTag ("Resource"))
		{
			bool feedback = false;

			foreach (IStats stat in statsController.selectedStats)
			{
				Worker worker = stat as Worker;
				
				if (worker == null) continue;

				if (worker.IsRepairing ||
					worker.IsBuilding)
				{
					worker.SetMoveToFactory (null);
				}
				
				worker.SetResource (hit.GetComponent<Resource> ());

				feedback = true;

				
				hudController.DestroyInspector ("unit");
				hudController.DestroyOptionsBtns();
				
				worker.Deselect ();
				
				if (statsController.selectedStats.Count == 1) 
				{
					statsController.DeselectAllStats();
					statsController.selectedStats.Clear();
					
				}



			}

			if (feedback)
							
			{  // hudController.CreateSubstanceHealthBar (this, 6, Resource, "Health Reference");
				hudController.CreateFeedback (HUDController.Feedbacks.Move,hit.position, 2f,
											  gameplayManager.GetColorTeam ());

			}
			return;
		}
		else
		{
			foreach (IStats stat in statsController.selectedStats)
			{
				Worker worker = stat as Worker;
				
				if (worker == null) continue;
				
				if (!worker.hasResource)
				{
					if (worker.resource != null)
					{
						worker.SetResource (null);
					}
				}
				if (worker.HasFactory ())
				{
					worker.SetMoveToFactory (null);
				}
			}


		}
		
		statsController.MoveTroop (touchController.GetFinalPoint);

	}
}
