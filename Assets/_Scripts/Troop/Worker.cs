using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Visiorama.Extension;
using Visiorama;
using Visiorama.Utils;

public class Worker : Unit
{
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
	public class WorkerAnimation
	{
		public AnimationClip Extracting;
		public AnimationClip CarryingIdle;
		public AnimationClip Carrying;
		public float carryingAnimSpeed = 0.7f;
	}

	[System.Serializable]
	public class FactoryConstruction
	{
		public FactoryBase factory;
		public ResourcesManager costOfResources;
		public bool VIP = false;
		public bool m_techActive = false;
		public bool techAvailable { get {if (VIP) return VIP; else return m_techActive; } set { m_techActive = value; } }
		public IStats.GridItemAttributes gridItemAttributes;
	}

	public enum WorkerState
	{
		Idle		 = 0,
		Extracting	 = 1,
		Carrying	 = 2,
		CarryingIdle = 3,
		Building	 = 4,
		Repairing	 = 5,
		Praying      = 6
	}

	public int forceToExtract;
	public int numberMaxGetResources;
	public float distanceToExtract = 5f;
	public ResourceWorker[] resourceWorker;
	public FactoryConstruction[] factoryConstruction;
	public int constructionAndRepairForce;
	public WorkerState workerState;
	public bool IsExtracting {get; protected set;}
	public bool IsRepairing {get; protected set;}
	public bool IsBuilding {get; protected set;}
	public int resourceId = -1;
	public Resource.Type resourceType {get; protected set;}
	public Resource resource {get; protected set;}
	public int currentNumberOfResources {get; protected set;}
	public bool hasResource {get; protected set;}
	public int lastResourceId;
	protected Resource lastResource;
	protected bool isSettingWorkerNull;
	private bool workerInitialized = false;
	protected FactoryBase factoryChoose, lastFactory;
	protected bool isMovingToFactory;
	protected bool isCheckedSendResourceToFactory;

	public override void Init ()
	{
		if (workerInitialized)	return;
		
		workerInitialized = true;
		base.Init ();
		moveAttack = false;
		resourceId = -1;

		DisableResourceTools();

		hasResource = isSettingWorkerNull = false;

		workerState = WorkerState.Idle;
	}

	public void InitWorkerTechAvailability ()
	{
		foreach (FactoryConstruction fc in factoryConstruction)
		{
			fc.techAvailable = fc.VIP;
			
		}
	}

	public override void IAStep ()
	{
		if (!playerUnit)
			return;

		switch (workerState)
		{
			case WorkerState.Praying:

			if (HasFactory ())
			{
				resourceWorker[resourceId].extractingObject.SetActive (false);
				resourceId = -1;
				resource.RemoveWorker (this);
				lastResource = resource = null;
				workerState = WorkerState.Idle;
				Move (factoryChoose.transform.position);
				return;
			}
			else if (resource != lastResource || resource == null)
			{
				resourceWorker[lastResourceId].extractingObject.SetActive (false);
				lastResource.RemoveWorker (this);
				workerState = WorkerState.Idle;
				return;
			}
			else
			{
				NavAgent.Stop ();
				transform.LookAt (resource.transform);
				NavAgent.obstacleAvoidanceType = normalObstacleAvoidance;
				if (!IsExtracting) StartCoroutine (Pray ());
			}
			
			if (!IsExtracting) StartCoroutine (Pray ());
			unitState = UnitState.Idle;
			
			
			
			break;
			
		case WorkerState.Extracting:

				if(unitState == Unit.UnitState.Walk)	NavAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
				else 									NavAgent.obstacleAvoidanceType = normalObstacleAvoidance;
				
				if (HasFactory ())
				{
					resourceWorker[resourceId].extractingObject.SetActive (false);
					resourceId = -1;
					resource.RemoveWorker (this);
					lastResource = resource = null;
					workerState = WorkerState.Idle;
					Move (factoryChoose.transform.position);
					return;
				}
				else if (resource != lastResource || resource == null)
				{
					resourceWorker[lastResourceId].extractingObject.SetActive (false);
					lastResource.RemoveWorker (this);
					workerState = WorkerState.Idle;
					return;
				}
				else
				{
					NavAgent.Stop ();
					transform.LookAt (resource.transform);
					if (!IsExtracting) StartCoroutine (Extract ());
				}

				if (!IsExtracting) StartCoroutine (Extract ());
				unitState = UnitState.Idle;
			break;

			case WorkerState.Carrying:
			case WorkerState.CarryingIdle:				

				if(unitState == Unit.UnitState.Walk)	NavAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
				else 									NavAgent.obstacleAvoidanceType = normalObstacleAvoidance;

			    if (!isCheckedSendResourceToFactory)
				{
					if (!isMovingToFactory)
					{
						if (!HasFactory ())
						{
							SearchFactory (resource.type);						
							SetMoveToFactory (factoryChoose);
						}

						else 
						{
							unitState = UnitState.Idle;
							Debug.Log("nao achou nenhuma factory");
						}
					}

					isCheckedSendResourceToFactory = true;
				}

				if (isMovingToFactory)
				{
					if (!HasFactory ())
					{
						isMovingToFactory = false;
						SearchFactory (resource.type);						
						SetMoveToFactory (factoryChoose);
					}									
				}

				if (!MoveComplete ())
				{
					if (resourceWorker[resourceId].workerAnimation.Carrying)
					{
						ControllerAnimation[resourceWorker[resourceId].workerAnimation.Carrying.name].normalizedSpeed = resourceWorker[resourceId].workerAnimation.carryingAnimSpeed * Mathf.Clamp(NavAgent.velocity.sqrMagnitude, 0f, 1f);
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

				if (!HasFactory ()) return;

				if (!factoryChoose.wasBuilt)
				{
					resource = null;
					resourceWorker[resourceId].carryingObject.SetActive (false);
					
					workerState = WorkerState.Idle;

					return;
				}

				if (Vector3.Distance (transform.position, factoryChoose.transform.position) < transform.GetComponent<CapsuleCollider>().radius + factoryChoose.helperCollider.radius)
				{
					gameplayManager.resources.DeliverResources (resourceType, currentNumberOfResources);
					WorkerReset();

					if (resource != null) SetResource (resource);
					else
					{
						NavAgent.Stop ();
						unitState = Unit.UnitState.Idle;
					}					
				}
				break;

			case WorkerState.Building:
			case WorkerState.Repairing:

				if(unitState == Unit.UnitState.Walk)	NavAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
				else 									NavAgent.obstacleAvoidanceType = normalObstacleAvoidance;
			
				if (!HasFactory () || factoryChoose != lastFactory)
				{
					Move (transform.position - transform.forward);
					resourceWorker[0].extractingObject.SetActive (false);
					isMovingToFactory = false;
					workerState = WorkerState.Idle;
					break;
				}

				NavAgent.Stop ();
				if (workerState == WorkerState.Building)
				{
					if (!IsBuilding) StartCoroutine (StartConstruct ());					
				}
				else
				if (!IsRepairing) StartCoroutine (StartRepair ());

			break;
			case WorkerState.Idle:				
				
				if(unitState == Unit.UnitState.Walk)	NavAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
				else 									NavAgent.obstacleAvoidanceType = normalObstacleAvoidance;
				bool WorkerisAttacking = (unitState == Unit.UnitState.Attack);
				if (WorkerisAttacking)	resourceWorker[0].extractingObject.SetActive (true);

				else if (lastResourceId != -1)
				{
					resourceWorker[lastResourceId].carryingObject.SetActive (false);
					lastResourceId = resourceId;
				}

				CheckResource ();
				CheckConstructFactory ();				
				
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
				case WorkerState.Praying:
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

				case WorkerState.Idle:
					
					bool WorkerisAttacking = (unitState == Unit.UnitState.Attack);
					
					if (WorkerisAttacking)	resourceWorker[0].extractingObject.SetActive (true);
								
					else if (lastResourceId != -1)
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
			if(!fc.techAvailable)
			{
				ht = new Hashtable();
				ht["factory"] = fc;
				ht["disable"] = 0;
				
				hudController.CreateButtonInInspector (fc.factory.buttonName,
				                                       fc.gridItemAttributes.Position,
				                                       ht,
				                                       fc.factory.guiTextureName,
				                                       null,
				                                       (ht_dcb, onClick) => 
				                                       {
															FactoryConstruction factory = (FactoryConstruction)ht_dcb["factory"];
															
															hudController.OpenInfoBoxFactory(factory.factory, false);																				
															
														});
			}

			else
			{
				ht = new Hashtable();
				ht["factory"] = fc;
				ht["time"] = 0;

				if (fc.costOfResources.Rocks != 0)
				{
					ht["gold"] = fc.costOfResources.Rocks;
				}
				
				if (fc.costOfResources.Mana != 0)
				{
					ht["mana"] = fc.costOfResources.Mana;
				}

				hudController.CreateButtonInInspector ( fc.factory.name,
														fc.gridItemAttributes.Position,
														ht,
														fc.factory.guiTextureName,
				                                        null,
				                                       	(ht_dcb, isDown) =>
														{
																FactoryConstruction factory = (FactoryConstruction)ht_dcb["factory"];
															
																if (isDown)
																{
																	ht["time"] = Time.time;
																}
																else
																{
																	if (Time.time - (float)ht["time"] > 0.4f)
																	{	
																		hudController.OpenInfoBoxFactory(factory.factory, true);
																		
																	}
																	else
																	{
																		hudController.CloseInfoBox();
																		InstanceGhostFactory (ht_dcb);
																	}
																}
														});
			}
		}
	}

	public override IEnumerator OnDie ()
	{
		workerState = WorkerState.Idle;
		return base.OnDie ();
	}

	public void InstanceGhostFactory (Hashtable ht)
	{
		FactoryConstruction factoryConstruct = (FactoryConstruction)ht["factory"];

		if (gameplayManager.resources.CanBuy (factoryConstruct.costOfResources))
		{
			GameObject ghostFactory = null;

			if (PhotonNetwork.offlineMode)
				ghostFactory = Instantiate (factoryConstruct.factory.gameObject, Vector3.zero, factoryConstruct.factory.transform.rotation) as GameObject;
			else
				ghostFactory = PhotonNetwork.Instantiate ( factoryConstruct.factory.gameObject.name, Vector3.zero, factoryConstruct.factory.transform.rotation, 0);

			ghostFactory.AddComponent<GhostFactory>().Init (this, factoryConstruct);
		}
		else
			eventController.AddEvent("out of funds", hudController.rocksFeedback ,factoryConstruct.factory.name);
	}

	public void SetResource (Resource newResource)
	{
		resource = newResource;

		if (resource != null)
		{
			CapsuleCollider col = resource.GetComponent<CapsuleCollider> ();
			Vector3 randomVector = (Random.onUnitSphere * col.radius * 0.75f);
			Vector3 position = resource.transform.position - randomVector;
			position.y = resource.transform.position.y;				
			Move (position);
		}
	}

#region Funções que o Worker pode fazer
	IEnumerator StartConstruct ()
	{
		NavAgent.avoidancePriority = 0;
		IsBuilding = true;
		PlayWorkerSfx("Building");
		ControllerAnimation.PlayCrossFade (resourceWorker[0].workerAnimation.Extracting, WrapMode.Once);
		yield return StartCoroutine (ControllerAnimation.WhilePlaying (resourceWorker[0].workerAnimation.Extracting));

		if (HasFactory () && !factoryChoose.Construct (this))
		{
			factoryChoose = null;
		}

		IsBuilding = false;
	}

	IEnumerator StartRepair ()
	{
		NavAgent.avoidancePriority = 0;
		IsRepairing = true;
		PlayWorkerSfx("Building");
		ControllerAnimation.PlayCrossFade (resourceWorker[0].workerAnimation.Extracting, WrapMode.Once);
		yield return StartCoroutine (ControllerAnimation.WhilePlaying (resourceWorker[0].workerAnimation.Extracting));
		if (HasFactory ())
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
		NavAgent.avoidancePriority = 0;
		IsExtracting = true;
		PlayWorkerSfx("Mining");
		ControllerAnimation.PlayCrossFade (resourceWorker[resourceId].workerAnimation.Extracting, WrapMode.Once);
		yield return StartCoroutine (ControllerAnimation.WhilePlaying (resourceWorker[resourceId].workerAnimation.Extracting));
		IsExtracting = false;
		if (resource != null && resourceId == 0)
			resource.ExtractResource (this);
		else
		{
			workerState = WorkerState.Idle;
		}

		NavAgent.avoidancePriority = normalAvoidancePriority;
	}

	IEnumerator Pray ()
	{
		NavAgent.avoidancePriority = 0;
		IsExtracting = true;
//		PlayWorkerSfx("Mining");
		resourceId = 1;
		ControllerAnimation.PlayCrossFade (resourceWorker[resourceId].workerAnimation.Extracting, WrapMode.Once);
		yield return StartCoroutine (ControllerAnimation.WhilePlaying (resourceWorker[resourceId].workerAnimation.Extracting));
		IsExtracting = false;
		if (resource != null && resourceId == 1)
		{
			gameplayManager.resources.DeliverResources (resourceType, resource.resistance);
		}
		else
		{
			workerState = WorkerState.Idle;
		}

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

		NavAgent.acceleration = resourceWorker[resourceId].carryingAcceleration;
		NavAgent.speed = resourceWorker[resourceId].carryingSpeed;
		NavAgent.angularSpeed = resourceWorker[resourceId].carryingAngularSpeed;

		resourceWorker[resourceId].extractingObject.SetActive (false);
		resourceWorker[resourceId].carryingObject.SetActive (true);

		if (resource != null)
		{
			resource.RemoveWorker (this);
		}

		workerState = WorkerState.Carrying;
	}

	public void WorkerReset()
	{		
		currentNumberOfResources = 0;
		
		factoryChoose = null;
		
		workerState = WorkerState.Idle;
		
		isCheckedSendResourceToFactory = isMovingToFactory = hasResource = isSettingWorkerNull = false;
		
		resourceWorker[0].carryingObject.SetActive (false);
		resourceWorker[0].extractingObject.SetActive (false);

		resourceWorker[1].carryingObject.SetActive (false);
		resourceWorker[1].extractingObject.SetActive (false);
		
		resourceId = -1;
		
		ResetNavAgentValues ();
	}

#endregion

#region Mover até factory escolhida
	public void SetMoveToFactory (FactoryBase factory)
	{
		factoryChoose = factory;

		if (HasFactory ())
		{
			Vector3 position = factoryChoose.transform.position;
			position.y = factoryChoose.transform.position.y;			
			Move (position);
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

		if (HasFactory ())
		{
			Vector3 position = factoryChoose.transform.position;
			position.y = factoryChoose.transform.position.y;			
			Move (position);
			isMovingToFactory = true;
		}
		else
		{
			isMovingToFactory = false;
		}
	}

	void SetMoveToFactory (System.Type type)
	{
		SearchFactory (type);

		if (HasFactory ())
		{
			Vector3 position = factoryChoose.transform.position;
			position.y = factoryChoose.transform.position.y;			
			Move (position);
			isMovingToFactory = true;
		}
		else
		{
			isMovingToFactory = false;
		}
	}
#endregion

#region Procura resource por resourceType ou System.Type
	void SearchFactory (Resource.Type resourceType)
	{
		foreach (IStats stat in statsController.myStats)
		{
			FactoryBase fb = stat as FactoryBase;

			if (fb == null || !fb.wasBuilt) continue;

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
		foreach (IStats stat in statsController.myStats)
		{
			FactoryBase fb = stat as FactoryBase;

			if (fb == null) continue;

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
		if (!HasFactory ())
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

			else 
				CheckConstructFactory();
		}
	}

	void CheckConstructFactory ()
	{
		if (HasFactory ())
		{
			if (Vector3.Distance (transform.position, factoryChoose.transform.position) < transform.GetComponent<CapsuleCollider>().radius + factoryChoose.helperCollider.radius+1)
			{
				if (!factoryChoose.wasBuilt)
				{
					resourceWorker[0].extractingObject.SetActive (true);
					workerState = WorkerState.Building;
				}

				else if (factoryChoose.IsDamaged)
				{
					resourceWorker[0].extractingObject.SetActive (true);
					workerState = WorkerState.Repairing;
				}

				lastFactory = factoryChoose;
			}

			else
			{
				if (!factoryChoose.wasBuilt ||	factoryChoose.IsDamaged)
				{   														
					Move (factoryChoose.transform.position);
				}

				else
				{
					NavAgent.Stop ();
					workerState = WorkerState.Idle;
					factoryChoose = null;
					isMovingToFactory = false;
				}
			}
		}

		else if (factoryChoose != null)
		{
			if (factoryChoose.WasRemoved)
			{
				factoryChoose = null;
				NavAgent.Stop ();
				workerState = WorkerState.Idle;
				factoryChoose = null;
				isMovingToFactory = false;
			}
		}
	}

	public bool HasFactory ()
	{
		if (factoryChoose == null)
			return false;

		if (factoryChoose.WasRemoved)
			return false;

		return true;
	}

	void CheckResource ()
	{
		if (resource != null)
		{
			if (Vector3.Distance (transform.position, resource.transform.position) < distanceToExtract + resource.capsuleCollider.radius)
			{
				NavAgent.Stop ();

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
					if (resourceType == Resource.Type.Mana) workerState = WorkerState.Praying;
					else 									workerState = WorkerState.Extracting;
				}
			}
		}
	}

	public void StructureTechBool(string category, bool isAvailable)
	{
		foreach (FactoryConstruction fc in factoryConstruction)
		{
			if (fc.factory.category == category)
			{
				fc.techAvailable = isAvailable;

			}
		}
	}

	void PlayWorkerSfx(string audioGroup)
	{
		AudioClip sfx = SoundManager.LoadFromGroup(audioGroup);		
		Vector3 u = this.transform.position;		
		AudioSource smas = SoundManager.PlayCappedSFX (sfx, audioGroup, 0.6f, 1f, u);		
		if (smas != null)
		{		
			smas.dopplerLevel = 0.0f;
			smas.minDistance = 3.0f;
			smas.maxDistance = 30.0f;
			smas.rolloffMode =AudioRolloffMode.Linear;
		}
	}

	public void  DisableResourceTools()
	{
		foreach (ResourceWorker rw in resourceWorker)
		{
			rw.carryingObject.SetActive (false);
			rw.extractingObject.SetActive (false);
		}
	}


	// RPC
	[RPC]
	public override void AttackStat (string name, int force)
	{
		base.AttackStat (name, force);
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
