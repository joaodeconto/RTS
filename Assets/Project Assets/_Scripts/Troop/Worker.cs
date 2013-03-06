using UnityEngine;
using System.Collections;
using Visiorama.Extension;

public class Worker : Unit
{
	[System.Serializable]
	public class WorkerAnimation
	{
		public AnimationClip Extracting;
		public AnimationClip CarryingIdle;
		public AnimationClip Carrying;
	}

	[System.Serializable]
	public class ResourceWorker 
	{
		public Resource.Type type;
		public GameObject extractingObject;
		public GameObject carryingObject;
		public float carryingSpeed;
		public float carryingAcceleration;
		public float carryingAngularSpeed;
		public WorkerAnimation workerAnimation;
	}
	
	public enum WorkerState
	{
		Extracting = 0,
		Carrying = 1,
		None = 2
	}
	
	public int forceToExtract;
	public int numberMaxGetResources;
	public float distanceToExtract = 5f;
	public ResourceWorker[] resourceWorker;
	
	public WorkerState workerState {get; protected set;}
	
	public bool IsExtracting {get; protected set;}
	
	public Resource resource {get; protected set;}
	public int currentNumberOfResources {get; protected set;}
	public bool hasResource {get; protected set;}
	protected int resourceId;
	protected bool settingWorkerNull;
	
	protected TouchController touchController;
	
	protected FactoryBase mainFactory;
	protected bool MovingToMainFactory;
	
	public override void Init ()
	{
		base.Init ();
		
		resourceId = -1;
		
		foreach (ResourceWorker rw in resourceWorker)
		{
			rw.carryingObject.SetActive (false);
			rw.extractingObject.SetActive (false);
		}
		
		hasResource = settingWorkerNull = false;
		
		workerState = WorkerState.None;
		
		touchController = Visiorama.ComponentGetter.Get<TouchController>();
	}
	
	public override void UnitStatus ()
	{
		if (playerUnit)
		{
			switch (workerState)
			{
			case WorkerState.Extracting:
				if (resource == null)
				{
					resourceWorker[resourceId].extractingObject.SetActive (false);
					resourceId = -1;
					return;
				}
				
				if (!IsExtracting) StartCoroutine (Extract ());
				break;
				
			case WorkerState.Carrying:
				if (resource != null &&
					!settingWorkerNull)
				{
					settingWorkerNull = true;
					resource.SetWorker (null);
				}
				
				if (!MovingToMainFactory)
				{
					SetResourceInMainBuilding ();
				}
				
				if (!MoveComplete())
				{
					if (resourceWorker[resourceId].workerAnimation.Carrying)
					{
						ControllerAnimation[resourceWorker[resourceId].workerAnimation.Carrying.name].normalizedSpeed = unitAnimation.walkSpeed * Mathf.Clamp(pathfind.velocity.sqrMagnitude, 0f, 1f);
						ControllerAnimation.PlayCrossFade (resourceWorker[resourceId].workerAnimation.Carrying, WrapMode.Loop);
					}
				}
				else
				{
					if (resourceWorker[resourceId].workerAnimation.CarryingIdle)
						ControllerAnimation.PlayCrossFade (resourceWorker[resourceId].workerAnimation.CarryingIdle);
				}
				
				if (Vector3.Distance (transform.position, mainFactory.transform.position) < transform.GetComponent<CapsuleCollider>().radius + mainFactory.GetComponent<CapsuleCollider>().radius)
				{
					if (resource != null) Move (resource.transform.position);
					gameplayManager.resources.Set (resource.type, currentNumberOfResources);
					currentNumberOfResources = 0;
					workerState = WorkerState.None;
					MovingToMainFactory = hasResource = settingWorkerNull = false;
					
					resourceWorker[resourceId].carryingObject.SetActive (false);
					
					resourceId = -1;
					
					ResetPathfindValue ();
				}
				break;
				
			case WorkerState.None:
				base.UnitStatus ();
				
				if (resource != null)
				{
					if (Vector3.Distance (transform.position, resource.transform.position) < distanceToExtract + resource.collider.radius)
					{
						pathfind.Stop ();
						
						if (resource.SetWorker (this))
						{
							int i = 0;
							foreach (ResourceWorker rw in resourceWorker)
							{
								if (rw.type == resource.type)
								{
									resourceId = i;
									rw.extractingObject.SetActive (true);
								}
							}
							
							workerState = WorkerState.Extracting;
						}
						else
						{
							if (unitState != Unit.UnitState.Idle) unitState = Unit.UnitState.Idle;
						}
					}
				}
				break;
			}
			
//			if (touchController.touchType == TouchController.TouchType.Ended)
//			{
//				if (touchController.idTouch == TouchController.IdTouch.Id1)
//				{
//					if (touchController.GetFinalRaycastHit.transform.GetComponent<Resource>() != null)
//					{
//						if (resource == null)
//						{
//							resource = touchController.GetFinalRaycastHit.transform.GetComponent<Resource>();
//						}
//					}
//					else
//					{
//						if (!hasResource)
//						{
//							resource.SetWorker (null);
//							resource = null;
//							if (workerState != WorkerState.None) workerState = WorkerState.None;
//						}
//						else
//						{
//							if (touchController.GetFinalRaycastHit.transform.GetComponent<MainFactory>() != null)
//							{
//								if (gameplayManager.IsSameTeam (touchController.GetFinalRaycastHit.transform.GetComponent<MainFactory>()))
//								{
//									MovingToMainFactory = false;
//									mainFactory = touchController.GetFinalRaycastHit.transform.GetComponent<FactoryBase>();
//								}
//							}
//						}
//					}
//				}
//			}
		}
	}
	
	public void SetResource (Resource newResource)
	{
		if (resource == null)
		{
			resource = newResource;
			if (newResource == null)
			{
				if (workerState != WorkerState.None) workerState = WorkerState.None;
			}
		}
	}
	
	IEnumerator Extract ()
	{
		IsExtracting = true;
		
		ControllerAnimation.PlayCrossFade (resourceWorker[resourceId].workerAnimation.Extracting, WrapMode.Once);
		yield return StartCoroutine (ControllerAnimation.WhilePlaying (resourceWorker[resourceId].workerAnimation.Extracting));
		
		if (resource != null) resource.ExtractResource (forceToExtract);
		else workerState = WorkerState.None;
		
		IsExtracting = false;
	}
	
	public void GetResource ()
	{
		GetResource (numberMaxGetResources);
	}
	
	public void GetResource (int gotNumberResources)
	{
		currentNumberOfResources = gotNumberResources;
		hasResource = true;
		
		pathfind.acceleration = resourceWorker[resourceId].carryingAcceleration;
		pathfind.speed = resourceWorker[resourceId].carryingSpeed;
		pathfind.angularSpeed = resourceWorker[resourceId].carryingAngularSpeed;
		resourceWorker[resourceId].extractingObject.SetActive (false);
		resourceWorker[resourceId].carryingObject.SetActive (true);
		
		workerState = WorkerState.Carrying;
	}
	
	public void SetResourceInMainBuilding (FactoryBase factory)
	{
		mainFactory = factory;
		Move (mainFactory.transform.position);
		MovingToMainFactory = true;
	}
	
	void SetResourceInMainBuilding ()
	{
		if (mainFactory == null) SearchFactory ();
		
		if (mainFactory != null)
		{
			Move (mainFactory.transform.position);
			MovingToMainFactory = true;
		}
	}
	
	void SearchFactory ()
	{
		foreach (FactoryBase fb in factoryController.factorys)
		{
			if (gameplayManager.IsSameTeam (fb))
			{
				if (fb.GetType () == typeof(MainFactory))
				{
					if (mainFactory == null)
					{
						mainFactory = fb;
					}
					else
					{
						if (Vector3.Distance (transform.position, fb.transform.position) < Vector3.Distance (transform.position, mainFactory.transform.position))
						{
							mainFactory = fb;
						}
					}
				}
			}
		}
	}
	
	// GIZMOS
	
	public override void DrawGizmosSelected ()
	{
		base.DrawGizmosSelected ();
		
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere (this.transform.position, distanceToExtract);
	}
}