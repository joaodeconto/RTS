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
		None = 0,
		Extracting = 1,
		Carrying = 2,
		CarryingIdle = 3
	}
	
	public int forceToExtract;
	public int numberMaxGetResources;
	public float distanceToExtract = 5f;
	public ResourceWorker[] resourceWorker;
	
	public WorkerState workerState {get; set;}
	
	public bool IsExtracting {get; protected set;}
	
	public int resourceId {get; set;}
	public Resource resource {get; protected set;}
	public int currentNumberOfResources {get; protected set;}
	public bool hasResource {get; protected set;}
	protected Resource lastResource;
	protected bool settingWorkerNull;
	
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
	}
	
	public override void UnitStatus ()
	{
		if (playerUnit)
		{
			switch (workerState)
			{
			case WorkerState.Extracting:
				if (resource != lastResource)
				{
					resourceWorker[resourceId].extractingObject.SetActive (false);
					resourceId = -1;
					lastResource.RemoveWorker (this);
					workerState = WorkerState.None;
					return;
				}
				else
				{
					pathfind.Stop ();
					transform.LookAt (resource.transform);
				}
				if (!IsExtracting) StartCoroutine (Extract ());
				break;
				
			case WorkerState.Carrying:
			case WorkerState.CarryingIdle:
				if (resource != null &&
					!settingWorkerNull)
				{
					settingWorkerNull = true;
					resource.RemoveWorker (this);
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
					
					workerState = WorkerState.Carrying;
				}
				else
				{
					if (resourceWorker[resourceId].workerAnimation.CarryingIdle)
						ControllerAnimation.PlayCrossFade (resourceWorker[resourceId].workerAnimation.CarryingIdle);
					
					workerState = WorkerState.CarryingIdle;
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
						
						if (resource.AddWorker (this))
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
							
							lastResource = resource;
							
							workerState = WorkerState.Extracting;
						}
						else
						{
							if (unitState != Unit.UnitState.Idle)
							{
								unitState = Unit.UnitState.Idle;
								
								Collider[] nearbyResources = Physics.OverlapSphere (transform.position, distanceView);

								if (nearbyResources.Length == 0) return;
								
								Resource.Type typeResource = resource.type;
								
								Resource resourceSelected = null;
						        for (int i = 0; i != nearbyResources.Length; i++)
								{
									if (nearbyResources[i].GetComponent<Resource> ())
									{
										if (nearbyResources[i].GetComponent<Resource> ().type == typeResource)
										{
											if (nearbyResources[i].GetComponent<Resource> ().IsLimitWorkers)
												continue;
											
											if (resourceSelected == null) resourceSelected = nearbyResources[i].GetComponent<Resource> ();
											else
											{
												if (Vector3.Distance (transform.position, nearbyResources[i].transform.position) <
													Vector3.Distance (transform.position, resourceSelected.transform.position))
												{
													resourceSelected = nearbyResources[i].GetComponent<Resource> ();
												}
											}
										}
									}
						        }
						
								if (resourceSelected == null) return;
								else
								{
									resource = resourceSelected;
									Move (resource.transform.position);
								}
							}
						}
					}
				}
				break;
			}
		}
	}
	
	public override void SyncAnimation ()
	{
		switch (workerState)
		{
		case WorkerState.Carrying:
			if (resourceWorker[resourceId].workerAnimation.CarryingIdle)
			{
				ControllerAnimation.PlayCrossFade (resourceWorker[resourceId].workerAnimation.CarryingIdle);
				if (IsVisible) resourceWorker[resourceId].carryingObject.SetActive (true);
			}
			break;
			
		case WorkerState.CarryingIdle:
			if (resourceWorker[resourceId].workerAnimation.CarryingIdle)
			{
				ControllerAnimation.PlayCrossFade (resourceWorker[resourceId].workerAnimation.CarryingIdle);
				if (IsVisible) resourceWorker[resourceId].carryingObject.SetActive (true);
			}
			break;
			
		case WorkerState.Extracting:
			if (resourceWorker[resourceId].workerAnimation.Extracting)
			{
				ControllerAnimation.PlayCrossFade (resourceWorker[resourceId].workerAnimation.Extracting);
				if (IsVisible) resourceWorker[resourceId].extractingObject.SetActive (true);
			}
			break;
			
		case WorkerState.None:
			base.SyncAnimation ();
			break;
		}
	}
	
	public void SetResource (Resource newResource)
	{
		resource = newResource;
	}
	
	IEnumerator Extract ()
	{
		IsExtracting = true;
		
		ControllerAnimation.PlayCrossFade (resourceWorker[resourceId].workerAnimation.Extracting, WrapMode.Once);
		yield return StartCoroutine (ControllerAnimation.WhilePlaying (resourceWorker[resourceId].workerAnimation.Extracting));
		
		if (resource != null) resource.ExtractResource (this);
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