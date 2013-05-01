using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class InteractionController : MonoBehaviour
{
	public delegate void InteractionCallback(Vector3 position);
	
	public GameObject uiExitGameObject;
	
	protected TouchController touchController;
	protected TroopController troopController;
	protected GameplayManager gameplayManager;
	protected HUDController hudController;

	private Stack<InteractionCallback> stackInteractionCallbacks;

	public void Init ()
	{
		touchController = ComponentGetter.Get<TouchController> ();
		troopController = ComponentGetter.Get<TroopController> ();
		gameplayManager = ComponentGetter.Get<GameplayManager> ();
		hudController   = ComponentGetter.Get<HUDController> ();
		
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
		if (Input.GetKeyDown (KeyCode.Escape))
		{
			if (uiExitGameObject != null)
			{
				uiExitGameObject.SetActive (true);
			}
		}
		
#if (!UNITY_IPHONE && !UNITY_ANDROID) || UNITY_EDITOR
		if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown (KeyCode.K))
		{
			gameplayManager.resources.NumberOfRocks += 100;
		}
		
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
					ic(touchController.GetFinalPoint);
				}
			}
			break;
		}
#endif
	}

	public void Interaction (Transform hit)
	{
		if (troopController.selectedSoldiers.Count == 0) return;

		if (hit.CompareTag ("Factory"))
		{
			if (!gameplayManager.IsSameTeam (hit.GetComponent<FactoryBase> ()))
			{
				if (hit.GetComponent<FactoryBase> ().IsVisible)
				{
					troopController.AttackTroop (hit.transform.gameObject);
					return;
				}
			}
#if (!UNITY_IPHONE && !UNITY_ANDROID) || UNITY_EDITOR
			else
			{
				troopController.WorkerCheckFactory (hit.GetComponent<FactoryBase>());
				return;
			}
#endif
		}
		else if (hit.CompareTag ("Unit"))
		{
			if (!gameplayManager.IsSameTeam (hit.GetComponent<Unit> ()))
			{
				if (hit.GetComponent<Unit> ().IsVisible)
				{
					troopController.AttackTroop (hit.gameObject);
					return;
				}
			}
		}
		else if (hit.GetComponent<Resource> () != null)
		{
			bool feedback = false;
			
			foreach (Unit unit in troopController.selectedSoldiers)
			{
				if (unit.GetType() == typeof(Worker))
				{
					Worker worker = unit as Worker;
					
					if (worker.IsRepairing ||
						worker.IsBuilding)
					{
						worker.SetMoveToFactory (null);
					}
					
					worker.Move (touchController.GetFinalPoint);
					worker.SetResource(hit.GetComponent<Resource> ());
					
					feedback = true;
				}
			}
			
			if (feedback)
			{
				hudController.CreateFeedback (HUDController.Feedbacks.Self, 
											  hit.position,
											  hit.GetComponent<Resource>().collider.radius * hit.localScale.x * 2f, 
											  gameplayManager.GetColorTeam ());
				
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
					if (!worker.hasResource)
					{
						if (worker.resource != null)
						{
							worker.SetResource (null);
						}
					}
					if (worker.IsRepairing ||
						worker.IsBuilding)
					{
						worker.SetMoveToFactory (null);
					}
				}
			}
		}
		
		troopController.MoveTroop (touchController.GetFinalPoint);
	}
}
