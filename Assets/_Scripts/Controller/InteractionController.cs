using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;

public class InteractionController : MonoBehaviour
{
	#region Declares

	public delegate void InteractionCallback(Vector3 position, Transform hit);
	public GameObject uiExitGameObject;
	protected TouchController touchController;
	protected StatsController statsController;
	protected GameplayManager gameplayManager;
	protected HUDController hudController;
	private Stack<InteractionCallback> stackInteractionCallbacks;
	#endregion

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

		NavMeshHit navHit;

		if (hit.transform.CompareTag ("TribeCenter")|| hit.transform.CompareTag ("ArmyStructure") || hit.transform.CompareTag ("House")|| hit.transform.CompareTag ("Depot"))
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
						foreach (IStats stat in statsController.selectedStats)
						{
							Unit myUnit = stat as Unit;
							
							//Nao permite seguir a si mesmo nem alguma unidade nula
							if (myUnit == null ) continue;
							
							myUnit.UnFollow();	
						}
						statsController.AttackTroop (unit.gameObject);
						return;
					}
				}
			}
		}		

		else if (hit.CompareTag ("Resource") || hit.CompareTag("Obelisk"))   
		{
			bool feedback = false;

			foreach (IStats stat in statsController.selectedStats)
			{
				Worker worker = stat as Worker;
				
				if (worker == null) continue;
//				if (worker.IsExtracting)
//				{
//					continue;
//				}
				worker.WorkerReset();	
				if (worker.resource != null)
				{
					worker.resource.RemoveWorker (worker);
					worker.SetResource (null);
				}
				if (worker.HasFactory ())
					worker.SetMoveToFactory (null);		
							
				worker.SetResource (hit.GetComponent<Resource> ());
				feedback = true;
			}

			if (feedback)
							
			{  // hudController.CreateSubstanceHealthBar (this, 6, Resource, "Health Reference");
				hudController.CreateFeedback (HUDController.Feedbacks.Move, hit.position, 2f, gameplayManager.GetColorTeam ());
			}
			return;
		}
		else
		{
			foreach (IStats stat in statsController.selectedStats)
			{
				Worker worker = stat as Worker;
				
				if (worker == null) continue;				
				if (worker.resource != null)
				{
					worker.resource.RemoveWorker (worker);
					worker.SetResource (null);
				}
				if (worker.HasFactory ())	worker.SetMoveToFactory (null);
				worker.WorkerReset();
			}
		}

		if(NavMesh.SamplePosition (touchController.GetFinalPoint, out navHit, 0.8f, 1))statsController.MoveTroop (touchController.GetFinalPoint);

		else 
		{
			hudController.CreateFeedback (HUDController.Feedbacks.Invalid, touchController.GetFinalPoint, 1f, Color.red);
		}
	}
}
