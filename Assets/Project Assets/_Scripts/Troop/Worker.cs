using UnityEngine;
using System.Collections;
using Visiorama;
using Visiorama.Utils;
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

	[System.Serializable]
		public class FactoryConstruction
		{
			public FactoryBase factory;
			public ResourcesManager costOfResources;
			public IStats.GridItemAttributes gridItemAttributes;
		}

	public enum WorkerState
	{
		None		 = 0,
		Extracting	 = 1,
		Carrying	 = 2,
		CarryingIdle = 3,
		Building	 = 4,
		Repairing	 = 5
	}

	public int forceToExtract;
	public int numberMaxGetResources;
	public float distanceToExtract = 5f;
	public ResourceWorker[] resourceWorker;
	public FactoryConstruction[] factoryConstruction;
	public int constructionAndRepairForce;

	public WorkerState workerState {get; set;}

	public bool IsExtracting {get; protected set;}
	public bool IsRepairing {get; protected set;}
	public bool IsBuilding {get; protected set;}

	public int resourceId {get; set;}
	public Resource.Type resourceType {get; protected set;}
	public Resource resource {get; protected set;}
	public int currentNumberOfResources {get; protected set;}
	public bool hasResource {get; protected set;}
	protected int lastResourceId;
	protected Resource lastResource;
	protected bool isSettingWorkerNull;
	
	protected FactoryBase factoryChoose, lastFactory;
	protected bool isMovingToFactory;
	protected bool isCheckedSendResourceToFactory;

	public override void Init ()
	{
		base.Init ();

		resourceId = -1;

		foreach (ResourceWorker rw in resourceWorker)
		{
			rw.carryingObject.SetActive (false);
			rw.extractingObject.SetActive (false);
		}

		hasResource = isSettingWorkerNull = false;

		workerState = WorkerState.None;
	}

	public override void IAStep ()
	{
		if (!playerUnit)
			return;

		switch (workerState)
		{
			case WorkerState.Extracting:
				if (factoryChoose != null)
				{
					resourceWorker[resourceId].extractingObject.SetActive (false);
					resourceId = -1;
					resource.RemoveWorker (this);
					lastResource = resource = null;
					workerState = WorkerState.None;
					Move (factoryChoose.transform.position);
				}
				else if (resource != lastResource || resource == null)
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
//				if (resource != null &&
//					!settingWorkerNull)
//				{
//					settingWorkerNull = true;
//					resource.RemoveWorker (this);
//				}
			    if (!isCheckedSendResourceToFactory)
				{
					if (!isMovingToFactory)
					{
						if (factoryChoose == null)
						{
							SetMoveToFactory (resource.type);
							SetMoveToFactory (typeof(MainFactory));
						}
					
						if (factoryChoose == null)
						{
							isMovingToFactory = true;
							Move (transform.position);
						}
					}
					isCheckedSendResourceToFactory = true;
				}
			
				if (isMovingToFactory)
				{
					if (factoryChoose == null)
					{
						isMovingToFactory = false;
						Move (transform.position);
					}
				}

				if (!MoveComplete ())
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
			
				if (factoryChoose == null) return;

				if (!factoryChoose.wasBuilt)
				{
					resource = null;
					resourceWorker[resourceId].carryingObject.SetActive (false);
					ResetPathfindValue ();

					workerState = WorkerState.None;

					return;
				}
			
//				Debug.Log (Vector3.Distance (transform.position, factoryChoose.transform.position) < transform.GetComponent<CapsuleCollider>().radius + factoryChoose.GetComponent<CapsuleCollider>().radius);

				if (Vector3.Distance (transform.position, factoryChoose.transform.position) < transform.GetComponent<CapsuleCollider>().radius + factoryChoose.GetComponent<CapsuleCollider>().radius)
				{
				
					gameplayManager.resources.Set (resourceType, currentNumberOfResources);

					if (resource != null) Move (resource.transform.position);
					else
					{
						pathfind.Stop ();
						unitState = Unit.UnitState.Idle;
					}

					currentNumberOfResources = 0;

					factoryChoose = null;

					workerState = WorkerState.None;

					isCheckedSendResourceToFactory = isMovingToFactory = hasResource = isSettingWorkerNull = false;

					resourceWorker[resourceId].carryingObject.SetActive (false);

					resourceId = -1;

					ResetPathfindValue ();
				}
				break;
			case WorkerState.Building:
			case WorkerState.Repairing:
				if (factoryChoose == null ||
					factoryChoose != lastFactory)
				{
					// Patch para tirar travada ¬¬
					Move (transform.position - transform.forward);

					resourceWorker[0].extractingObject.SetActive (false);

					isMovingToFactory = false;

					if (hasResource)
					{
						resource = lastResource;
						GetResource (currentNumberOfResources);
					}
					else
					{
						workerState = WorkerState.None;
					}
				}

				pathfind.Stop ();
				if (workerState == WorkerState.Building)
				{
					if (!IsBuilding) StartCoroutine (StartConstruct ());
				}
				else
				{
					if (!IsRepairing) StartCoroutine (StartRepair ());
				}
				break;
			case WorkerState.None:
				CheckConstructFactory ();

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
									resourceType = resource.type;
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

				base.IAStep ();
				break;
		}
	}

	public override void SyncAnimation ()
	{
		if (IsVisible)
		{
			switch (workerState)
			{
				case WorkerState.Carrying:
					if (resourceWorker[resourceId].workerAnimation.Carrying)
					{
						ControllerAnimation.PlayCrossFade (resourceWorker[resourceId].workerAnimation.Carrying);
						resourceWorker[resourceId].carryingObject.SetActive (true);
						resourceWorker[resourceId].extractingObject.SetActive (false);
						lastResourceId = resourceId;
					}
					break;
	
				case WorkerState.CarryingIdle:
					if (resourceWorker[resourceId].workerAnimation.CarryingIdle)
					{
						ControllerAnimation.PlayCrossFade (resourceWorker[resourceId].workerAnimation.CarryingIdle);
						resourceWorker[resourceId].carryingObject.SetActive (true);
						resourceWorker[resourceId].extractingObject.SetActive (false);
						lastResourceId = resourceId;
					}
					break;
	
				case WorkerState.Extracting:
					if (resourceWorker[resourceId].workerAnimation.Extracting)
					{
						ControllerAnimation.PlayCrossFade (resourceWorker[resourceId].workerAnimation.Extracting);
						resourceWorker[resourceId].extractingObject.SetActive (true);
						resourceWorker[resourceId].carryingObject.SetActive (false);
						lastResourceId = resourceId;
					}
					break;
	
				case WorkerState.Building:
				case WorkerState.Repairing:
					if (resourceWorker[0].workerAnimation.Extracting)
					{
						ControllerAnimation.PlayCrossFade (resourceWorker[0].workerAnimation.Extracting);
						resourceWorker[0].extractingObject.SetActive (true);
						if (lastResourceId != -1)
						{
							resourceWorker[lastResourceId].carryingObject.SetActive (false);
							lastResourceId = resourceId;
						}
					}
					break;
	
				case WorkerState.None:
					if (unitState == Unit.UnitState.Attack)
					{
						resourceWorker[0].extractingObject.SetActive (true);
					}
					else
					{
						resourceWorker[0].extractingObject.SetActive (false);
					}
	
					if (lastResourceId != -1)
					{
						resourceWorker[lastResourceId].carryingObject.SetActive (false);
						lastResourceId = resourceId;
					}
	
					base.SyncAnimation ();
					break;
			}
		}
	}

	public override void Select ()
	{
		base.Select ();

		if (!playerUnit) return;

		Hashtable ht = null;
		foreach (FactoryConstruction fc in factoryConstruction)
		{
			ht = new Hashtable();
			ht["factory"] = fc;
			ht["price"]   = fc.costOfResources.NumberOfRocks;

			hudController.CreateButtonInInspector ( fc.factory.name,
													fc.gridItemAttributes.Position,
													ht,
													fc.factory.guiTextureName,
													(ht_hud) =>
													{
//														FactoryConstruction factory = (FactoryConstruction)ht_hud["factory"];
//														InstanceGhostFactory (factory);
														InstanceGhostFactory (ht_hud);
													});
		}
	}

	public override IEnumerator OnDie ()
	{
		workerState = WorkerState.None;

		return base.OnDie ();
	}

	public void InstanceGhostFactory (Hashtable ht)
	{
		FactoryConstruction factoryConstruct = (FactoryConstruction)ht["factory"];

		if (CanConstruct (factoryConstruct, false))
		{
			GameObject ghostFactory = null;

			if (PhotonNetwork.offlineMode)
				ghostFactory = Instantiate (factoryConstruct.factory.gameObject, Vector3.zero, factoryConstruct.factory.transform.rotation) as GameObject;
			else
				ghostFactory = PhotonNetwork.Instantiate ( factoryConstruct.factory.gameObject.name, Vector3.zero, factoryConstruct.factory.transform.rotation, 0);

			ghostFactory.AddComponent<GhostFactory>().Init (this, factoryConstruct);
		}
		else
			eventManager.AddEvent("out of founds", factoryConstruct.factory.name);
	}

	public void SetResource (Resource newResource)
	{
		resource = newResource;
	}

	public bool CanConstruct (FactoryConstruction factory, bool discount = true)
	{
		return gameplayManager.resources.CanBuy (factory.costOfResources, discount);
	}

#region Funções que o Worker pode fazer
	IEnumerator StartConstruct ()
	{
		IsBuilding = true;

		ControllerAnimation.PlayCrossFade (resourceWorker[0].workerAnimation.Extracting, WrapMode.Once);
		yield return StartCoroutine (ControllerAnimation.WhilePlaying (resourceWorker[0].workerAnimation.Extracting));

		if (factoryChoose != null && !factoryChoose.Construct (this))
		{
			factoryChoose = null;
		}

		IsBuilding = false;
	}

	IEnumerator StartRepair ()
	{
		IsRepairing = true;

		ControllerAnimation.PlayCrossFade (resourceWorker[0].workerAnimation.Extracting, WrapMode.Once);
		yield return StartCoroutine (ControllerAnimation.WhilePlaying (resourceWorker[0].workerAnimation.Extracting));

		if (factoryChoose != null)
		{
			if (!factoryChoose.Repair (this))
			{
				factoryChoose = null;
			}
		}

		IsRepairing = false;
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
#endregion

#region Pega os recursos e atribui para ele
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
		
		if (resource != null)
		{
			resource.RemoveWorker (this);
		}
		
		workerState = WorkerState.Carrying;
	}
#endregion

#region Mover até factory escolhida
	public void SetMoveToFactory (FactoryBase factory)
	{
		factoryChoose = factory;

		if (factoryChoose != null)
		{
			Move (factoryChoose.transform.position);
			isMovingToFactory = true;
		}
		else
		{
			isMovingToFactory = false;
		}
	}

	void SetMoveToFactory (Resource.Type resourceType)
	{
		SearchFactory (resourceType);

		if (factoryChoose != null)
		{
			Move (factoryChoose.transform.position);
			isMovingToFactory = true;
		}
	}

	void SetMoveToFactory (System.Type type)
	{
		SearchFactory (type);

		if (factoryChoose != null)
		{
			Move (factoryChoose.transform.position);
			isMovingToFactory = true;
		}
	}
#endregion

#region Procura resource por resourceType ou System.Type
	void SearchFactory (Resource.Type resourceType)
	{
		foreach (FactoryBase fb in factoryController.factorys)
		{
			if (gameplayManager.IsSameTeam (fb))
			{
				if (fb.receiveResource == resourceType)
				{
					CheckFactory (fb);
				}
			}
		}
	}

	void SearchFactory (System.Type type)
	{
		foreach (FactoryBase fb in factoryController.factorys)
		{
			if (gameplayManager.IsSameTeam (fb))
			{
				if (fb.GetType () == type)
				{
					CheckFactory (fb);
				}
			}
		}
	}
#endregion

	/// <summary>
	/// Verifica se a factory está disponível e atribui para a variavél "factoryChoose"
	/// </summary>
	/// <param name='factory'>
	/// Factory.
	/// </param>
	void CheckFactory (FactoryBase factory)
	{
		if (factoryChoose == null)
		{
			if (factory.wasBuilt) factoryChoose = factory;
		}
		else
		{
			if (factory.wasBuilt)
			{
				if (Vector3.Distance (transform.position, factory.transform.position) < Vector3.Distance (transform.position, factoryChoose.transform.position))
				{
					factoryChoose = factory;
				}
			}
		}
	}

	void CheckConstructFactory ()
	{
		if (factoryChoose != null)
		{
			if (Vector3.Distance (transform.position, factoryChoose.transform.position) < transform.GetComponent<CapsuleCollider>().radius + factoryChoose.GetComponent<CapsuleCollider>().radius)
			{
				if (!factoryChoose.wasBuilt)
				{
					resourceWorker[0].extractingObject.SetActive (true);
					workerState = WorkerState.Building;
				}
				else if (factoryChoose.IsNeededRepair)
				{
					resourceWorker[0].extractingObject.SetActive (true);
					workerState = WorkerState.Repairing;
				}

				lastFactory = factoryChoose;
			}
			else
			{
				if (!factoryChoose.wasBuilt ||
					factoryChoose.IsNeededRepair)
				{
					Move (factoryChoose.transform.position);
				}
				else
				{
					pathfind.Stop ();
					unitState = Unit.UnitState.Idle;
					factoryChoose = null;
					isMovingToFactory = false;
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

	// RPC
	[RPC]
	public override void AttackUnit (string nameUnit, int force)
	{
		base.AttackUnit (nameUnit, force);
	}

	[RPC]
	public override void AttackFactory (string nameFactory, int force)
	{
		base.AttackFactory (nameFactory, force);
	}

	[RPC]
	public override void InstantiatParticleDamage ()
	{
		base.InstantiatParticleDamage ();
	}
	
	[RPC]
	public override void SendRemove ()
	{
		base.SendRemove ();
	}
}
