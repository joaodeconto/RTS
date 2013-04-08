using UnityEngine;
using System.Collections;
using Visiorama.Extension;
using Visiorama;
using System.Linq;

public class Unit : IStats
{
	public const string UnitGroupQueueName = "Unit Group";

	[System.Serializable]
	public class UnitAnimation
	{
		public AnimationClip Idle;
		public AnimationClip Walk;
		public float walkSpeed = 1f;
		public AnimationClip Attack;
		public AnimationClip DieAnimation;
		public AnimationClip[] SpecialAttack;
	}

	public enum UnitState
	{
		Idle   = 0,
		Walk   = 1,
		Attack = 2,
		Die    = 3
	}

	public enum UnitType
	{
		Worker      = 0,
		Neanderthal = 1,
		Raptor      = 2,
		Triceratops = 3,
		Rex         = 4,
		Other       = 5
	}

	public int force;
	public float distanceView       = 15f;
	public float rangeAttack        = 5f;
	public float attackDuration     = 1f;
	public float probeRange         = 1.0f; // how far the character can "see"
    public float turnSpeedAvoidance = 50f; // how fast to turn
	public int numberOfUnits = 1;

    public Transform probePoint; // forward probe point
    public Transform leftReference; // left probe point
    public Transform rightReference; // right probe point

	public string guiTextureName;

	public UnitAnimation unitAnimation;

	public UnitType category;

	public int AdditionalForce { get; set; }

	public bool IsAttacking { get; protected set; }
	public bool IsDead { get; protected set; }

	public Animation ControllerAnimation;
	public int TypeSoundId { get; protected set; }
	public SoundManager CharSound { get; protected set; }

	private bool canHit;
	public bool CanHit
	{
		get
		{
			if (!canHit) canHit = (IsDead);

			return canHit;
		}
	}

    private Transform obstacleInPath; // we found something!
    private bool obstacleAvoid = false; // internal var

	protected PhotonPlayer playerTargetAttack;
	protected GameObject targetAttack;
	protected bool followingTarget;
	protected float attackBuff;

	public UnitState unitState { get; set; }

	protected bool invokeCheckEnemy;

	[System.NonSerializedAttribute]
	public NavMeshAgent pathfind;
	[System.NonSerializedAttribute]
	public Vector3 pathfindTarget;

	protected FactoryController factoryController;
	protected TroopController troopController;
	protected HUDController hudController;
	protected InteractionController interactionController;

	protected HealthBar healthBar;

	protected float normalAcceleration;
	protected float normalSpeed;
	protected float normalAngularSpeed;

	public override void Init ()
	{
		base.Init();

		//CharSound = GetComponent<CharacterSound> ();

		if (ControllerAnimation == null) ControllerAnimation = gameObject.animation;
		if (ControllerAnimation == null) ControllerAnimation = GetComponentInChildren<Animation> ();

//		if (ControllerAnimation != null)
//		{
//			ControllerAnimation.SetLayer (animation.Idle, 0);
//			ControllerAnimation.SetLayer (animation.Walk, 0);
//			ControllerAnimation.SetLayer (animation.Attack, 0);
//		}

		factoryController     = ComponentGetter.Get<FactoryController> ();
		troopController       = ComponentGetter.Get<TroopController> ();
		hudController         = ComponentGetter.Get<HUDController> ();
		interactionController = ComponentGetter.Get<InteractionController>();

		pathfind = GetComponent<NavMeshAgent>();

		normalAcceleration = pathfind.acceleration;
		normalSpeed        = pathfind.speed;
		normalAngularSpeed = pathfind.angularSpeed;

		pathfindTarget = transform.position;

		this.gameObject.tag   = "Unit";
		this.gameObject.layer = LayerMask.NameToLayer ("Unit");

		if (!enabled) enabled = playerUnit;

		if (probePoint == null) probePoint = transform;

		if(leftReference == null)
		{
		    leftReference = transform;
			leftReference.position -= transform.right * collider.bounds.size.x;
		}

		if(rightReference == null)
		{
		    rightReference = transform;
			rightReference.position += transform.right * collider.bounds.size.x;
		}

		troopController.AddSoldier (this);
	}

	void Update ()
	{
		if (unitState != UnitState.Die) IAStep ();
	}

	void OnDestroy ()
	{
		if (Selected && !playerUnit) Deselect ();
		if (!IsRemoved && !playerUnit) troopController.soldiers.Remove (this);
	}

	public virtual void IAStep ()
	{
		if (!playerUnit) return;

		//MoveAvoidance (pathfindTarget);

		switch (unitState)
		{
			case UnitState.Idle:
				if (unitAnimation.Idle)
					ControllerAnimation.PlayCrossFade (unitAnimation.Idle, WrapMode.Loop);

				StartCheckEnemy ();

				if (targetAttack != null) unitState = UnitState.Walk;

				break;
			case UnitState.Walk:
				if (unitAnimation.Walk)
				{
					ControllerAnimation[unitAnimation.Walk.name].normalizedSpeed = unitAnimation.walkSpeed * Mathf.Clamp(pathfind.velocity.sqrMagnitude, 0f, 1f);
					ControllerAnimation.PlayCrossFade (unitAnimation.Walk, WrapMode.Loop);
				}

				CancelCheckEnemy ();

				if (targetAttack != null)
				{
					pathfindTarget = transform.position;

					if (IsRangeAttack(targetAttack))
					{
						unitState = UnitState.Attack;
					}
					else if (InDistanceView (targetAttack.transform.position))
					{
						//MoveAvoidance (targetAttack.transform);
						Move (targetAttack.transform.position);
					}
					else
					{
						if (followingTarget)
						{
							targetAttack    = null;
							unitState       = UnitState.Idle;
							followingTarget = false;
						}
						else
						{
							pathfindTarget = targetAttack.transform.position + (targetAttack.transform.forward * targetAttack.GetComponent<CapsuleCollider>().radius);
							Move (pathfindTarget);
						}
					}
				}
				else if (MoveComplete(pathfindTarget))
				{
					Stop ();
					unitState = UnitState.Idle;
				}
				break;
			case UnitState.Attack:

				followingTarget = true;

				if (IsAttacking) return;

				Stop ();

				pathfindTarget = transform.position;

				if (targetAttack != null)
				{
					if (IsRangeAttack (targetAttack))
					{
						StartCoroutine(Attack ());
					}
					else
					{
						unitState = UnitState.Walk;
					}
				}
				else
				{
					unitState = UnitState.Idle;
				}
				break;
		}
	}

	public virtual void SyncAnimation ()
	{
		switch (unitState)
		{
		case UnitState.Idle:
			if (unitAnimation.Idle)
				ControllerAnimation.PlayCrossFade (unitAnimation.Idle, WrapMode.Loop);

			break;
		case UnitState.Walk:
			if (unitAnimation.Walk)
			{
				ControllerAnimation[unitAnimation.Walk.name].normalizedSpeed = unitAnimation.walkSpeed;
				ControllerAnimation.PlayCrossFade (unitAnimation.Walk, WrapMode.Loop);
			}

			break;
		case UnitState.Attack:
			if (unitAnimation.Attack)
				ControllerAnimation.PlayCrossFade (unitAnimation.Attack, WrapMode.Once);

			break;
		case UnitState.Die:
			if (unitAnimation.DieAnimation)
				ControllerAnimation.PlayCrossFade (unitAnimation.DieAnimation, WrapMode.ClampForever);

			break;
		}
	}

	#region Move Pathfind w/ Avoidance
	public void Move (Vector3 destination)
	{
		if (!pathfind.updatePosition) pathfind.updatePosition = true;

		if (pathfindTarget != destination) pathfind.SetDestination (destination);

		pathfindTarget = destination;
		unitState = UnitState.Walk;
	}

	void MoveAvoidance (Transform target)
	{
        RaycastHit hit;
        Vector3 dir = (target.position - transform.position).normalized;

        bool previousCastMissed = true;

        if(Physics.Raycast(probePoint.position, transform.forward, out hit, probeRange))
		{
            if(obstacleInPath != target.transform)
			{ // ignore our target
                Debug.Log("Found an object in path! - " + gameObject.name);
                Debug.DrawLine(transform.position, hit.point, Color.green);
                previousCastMissed = false;
                obstacleAvoid = true;
                pathfind.Stop(true);
                pathfind.ResetPath();
                if(hit.transform != transform) {
                    obstacleInPath = hit.transform;
                    Debug.Log("I hit: " + hit.transform.gameObject.name);
                    dir += hit.normal * turnSpeedAvoidance;
                    Debug.Log("moving around an object - " + gameObject.name);
                }
            }
        }

        if(obstacleAvoid && previousCastMissed && Physics.Raycast(leftReference.position, transform.forward,out hit, probeRange))
		{
            if(obstacleInPath != target.transform)
			{ // ignore our target
                Debug.DrawLine(leftReference.position, hit.point, Color.red);
                obstacleAvoid = true;
                pathfind.Stop();
                if(hit.transform != transform) {
                    obstacleInPath = hit.transform;
                    previousCastMissed = false;
                    //Debug.Log("moving around an object");
                    dir += hit.normal * turnSpeedAvoidance;
                }
            }
        }

        // check the other side :)
        if(obstacleAvoid && previousCastMissed && Physics.Raycast(rightReference.position, transform.forward,out hit, probeRange)) {
            if(obstacleInPath != target.transform) { // ignore our target
                Debug.DrawLine(rightReference.position, hit.point, Color.green);
                obstacleAvoid = true;
                pathfind.Stop();
                if(hit.transform != transform) {
                    obstacleInPath = hit.transform;
                    dir += hit.normal * turnSpeedAvoidance;
                }
            }
        }

		// turn Nav back on when obstacle is behind the character!!
		if (obstacleInPath != null) {
			Vector3 forward = transform.TransformDirection(Vector3.forward);
			Vector3 toOther = obstacleInPath.position - transform.position;
			if (Vector3.Dot(forward, toOther) < 0) {
			    Debug.Log("Back on Navigation! unit - " + gameObject.name);
			    obstacleAvoid = false; // don't let Unity nav and our avoidance nav fight, character does odd things
			    obstacleInPath = null; // Hakuna Matata
			    pathfind.ResetPath();
			    pathfind.SetDestination(target.position);
			    pathfind.Resume(); // Unity nav can resume movement control
			}
		}

        // this is what actually moves the character when under avoidance control
        if(obstacleAvoid) {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime);
            transform.position += transform.forward * pathfind.speed * Time.deltaTime;
        }
    }

	void MoveAvoidance (Vector3 destination)
	{
        RaycastHit hit;
        Vector3 dir = (destination - transform.position).normalized;
        bool previousCastMissed = true; // no need to keep testing if something already hit

        // this is the main forward raycast
        if(Physics.Raycast(probePoint.position, transform.forward, out hit, probeRange))
		{
            Debug.DrawLine(transform.position, hit.point, Color.green);

            previousCastMissed = false;

            obstacleAvoid = true;

            pathfind.Stop(true);

            pathfind.ResetPath();

            if(hit.transform != transform)
			{
                obstacleInPath = hit.transform;

                dir += hit.normal * turnSpeedAvoidance;
            }
        }

        // if we did see something before, but now the forward raycast is turned out of range, check the sides
        // without this, the character bumps into the object and sort of bounces (usually) until it gets
        // past.  This is a better approach :)
        if(obstacleAvoid && previousCastMissed && Physics.Raycast(leftReference.position, transform.forward, out hit, probeRange))
		{
            if(obstacleInPath != transform)
			{
                Debug.DrawLine(leftReference.position, hit.point, Color.red);
                obstacleAvoid = true;
                pathfind.Stop();
                if(hit.transform != transform) {

                    obstacleInPath = hit.transform;

                    previousCastMissed = false;

                    dir += hit.normal * turnSpeedAvoidance;
                }
            }
        }

        // check the other side :)
        if(obstacleAvoid && previousCastMissed && Physics.Raycast(rightReference.position, transform.forward,out hit, probeRange))
		{
            Debug.DrawLine(rightReference.position, hit.point, Color.green);
            obstacleAvoid = true;
            pathfind.Stop();
            if(hit.transform != transform) {
                obstacleInPath = hit.transform;
                dir += hit.normal * turnSpeedAvoidance;
            }
        }

        // turn Nav back on when obstacle is behind the character!!
		if (obstacleInPath != null)
		{
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 toOther = obstacleInPath.position - transform.position;

            if (Vector3.Dot(forward, toOther) < 0)
			{
                Debug.Log("Back on Navigation! unit - " + gameObject.name);

                obstacleAvoid = false; // don't let Unity nav and our avoidance nav fight, character does odd things
                obstacleInPath = null;
                pathfind.ResetPath();
                pathfind.SetDestination(destination);
                pathfind.Resume(); // Unity nav can resume movement control

				unitState = UnitState.Walk;
            }
        }

        // this is what actually moves the character when under avoidance control
        if(obstacleAvoid)
		{
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime);
            transform.position += transform.forward * pathfind.speed * Time.deltaTime;

			unitState = UnitState.Walk;
        }
    }

	private void Stop ()
	{
		pathfind.Stop ();
	}
	#endregion

	private IEnumerator Attack ()
	{
		Quaternion rotation = Quaternion.LookRotation(targetAttack.transform.position - transform.position);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * pathfind.angularSpeed);

		if (unitAnimation.Attack)
		{
//			if (targetAttack.GetComponent<Unit>()) targetAttack.GetComponent<Unit>().ReceiveAttack(force + AdditionalForce);
//			else if (targetAttack.GetComponent<FactoryBase>()) targetAttack.GetComponent<FactoryBase>().ReceiveAttack(force + AdditionalForce);
//
//			if (!PhotonNetwork.offlineMode)
//			{
//				if (targetAttack.GetComponent<Unit>())
//					photonView.RPC ("AttackUnit", PhotonTargets.OthersBuffered, targetAttack.name, force + AdditionalForce);
//				else if (targetAttack.GetComponent<FactoryBase>())
//					photonView.RPC ("AttackFactory", PhotonTargets.OthersBuffered, targetAttack.name, force + AdditionalForce);
//			}

			ControllerAnimation.PlayCrossFade (unitAnimation.Attack, WrapMode.Once);

			IsAttacking = true;

			if (targetAttack == null) return true;
			
			if (targetAttack.GetComponent<IStats>().IsRemoved)
			{
				TargetingEnemy (null);
				return true;
			}
			
			yield return StartCoroutine (ControllerAnimation.WhilePlaying (unitAnimation.Attack));

			IsAttacking = false;

			if (!PhotonNetwork.offlineMode)
			{
				if (targetAttack.GetComponent<Unit>())
				{
					photonView.RPC ("AttackUnit", playerTargetAttack, targetAttack.name, force + AdditionalForce);
//					photonView.RPC ("AttackUnit", targetAttack.GetPhotonView().owner, targetAttack.name, force + AdditionalForce);
//					photonView.RPC ("AttackUnit", PhotonTargets.AllBuffered, targetAttack.name, force + AdditionalForce);
				}
				else if (targetAttack.GetComponent<FactoryBase>())
				{
					photonView.RPC ("AttackFactory", playerTargetAttack, targetAttack.name, force + AdditionalForce);
				}
			}
			else
			{
				if (targetAttack.GetComponent<Unit>()) targetAttack.GetComponent<Unit>().ReceiveAttack(force + AdditionalForce);
				else if (targetAttack.GetComponent<FactoryBase>()) targetAttack.GetComponent<FactoryBase>().ReceiveAttack(force + AdditionalForce);
			}
		}
		else
		{
			if(attackBuff < attackDuration)
			{
                attackBuff += Time.deltaTime;
	        }
			else
			{
				if (PhotonNetwork.offlineMode)
				{
					if (targetAttack.GetComponent<Unit>()) targetAttack.GetComponent<Unit>().ReceiveAttack(force + AdditionalForce);
					else if (targetAttack.GetComponent<FactoryBase>()) targetAttack.GetComponent<FactoryBase>().ReceiveAttack(force + AdditionalForce);
				}
				else
				{
					if (targetAttack.GetComponent<Unit>())
					{
						photonView.RPC ("AttackUnit", playerTargetAttack, targetAttack.name, force + AdditionalForce);
	//					photonView.RPC ("AttackUnit", targetAttack.GetPhotonView().owner, targetAttack.name, force + AdditionalForce);
	//					photonView.RPC ("AttackUnit", PhotonTargets.AllBuffered, targetAttack.name, force + AdditionalForce);
					}
					else if (targetAttack.GetComponent<FactoryBase>())
					{
						photonView.RPC ("AttackFactory", playerTargetAttack, targetAttack.name, force + AdditionalForce);
					}
				}

				GameObject attackObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
				attackObj.transform.position = transform.position + transform.forward;
				Destroy (attackObj, 0.5f);

                attackBuff = 0;
            }
		}
	}

	[RPC]
	public virtual void AttackUnit (string nameUnit, int force)
	{
		Unit unit = troopController.FindUnit (nameUnit);
		if (unit != null)
		{
			unit.ReceiveAttack (force);
		}
	}

	[RPC]
	public virtual void AttackFactory (string nameFactory, int force)
	{
		FactoryBase factory = factoryController.FindFactory (nameFactory);
		if (factory != null) factory.ReceiveAttack (force);
	}

	public virtual void Select ()
	{
		Selected = true;
		healthBar = hudController.CreateHealthBar (transform, MaxHealth, "Health Reference");
		healthBar.SetTarget (this);

		hudController.CreateSelected (transform, sizeOfSelected, gameplayManager.GetColorTeam (Team));
		hudController.CreateEnqueuedButtonInInspector ( this.name,
														Unit.UnitGroupQueueName,
														null,
														this.guiTextureName,
														(hud_ht) =>
														{
															troopController.DeselectAllSoldiers();
															troopController.SelectSoldier(this, true);
														});

		Hashtable ht;
		foreach (MovementAction ma in movementActions)
		{
			ht = new Hashtable();
			ht["actionType"] = ma.actionType;

			hudController.CreateButtonInInspector ( ma.buttonAttributes.name,
													ma.buttonAttributes.gridItemAttributes.Position,
													ht,
													ma.buttonAttributes.spriteName,
													(ht_hud) =>
													{
														switch((MovementAction.ActionType)ht["actionType"])
														{
														case MovementAction.ActionType.Move:
															interactionController.AddCallback(TouchController.IdTouch.Id0,
																								(position) =>
																								{
																									troopController.MoveTroop(position);
																								});
															break;
														case MovementAction.ActionType.Patrol:

															break;
														case MovementAction.ActionType.CancelMovement:
															Stop();
															break;
														case MovementAction.ActionType.Follow: //Rally Point

															break;
														case MovementAction.ActionType.Attack:
															break;
														}
													});
		}
	}

	public void Deselect (bool isGroupDelesection = false)
	{
		Debug.Log ("isGroupDelesection: ");
		Selected = false;
		hudController.DestroySelected (transform);

		if(isGroupDelesection)
		{
			hudController.DestroyInspector ();
		}
		else
		{
			hudController.RemoveEnqueuedButtonInInspector(this.name, Unit.UnitGroupQueueName);
		}
	}

	public bool IsRangeAttack (GameObject soldier)
	{
		return Vector3.Distance(transform.position, soldier.transform.position) <= (rangeAttack + soldier.GetComponent<CapsuleCollider>().radius);
	}

	public bool InDistanceView (Vector3 position)
	{
		return Vector3.Distance(transform.position, position) <= distanceView;
	}

	public bool MoveComplete (Vector3 destination)
	{
		return (Vector3.Distance(transform.position, pathfind.destination) <= 2) &&
				pathfind.velocity.sqrMagnitude < 0.1f;
//		return Vector3.Distance(transform.position, destination) <= 2;
	}

//	bool start = false;
	public bool MoveComplete ()
	{
//		if (pathfind.desiredVelocity.sqrMagnitude < 0.001f) start = !start;
//		return pathfind.desiredVelocity.sqrMagnitude < 0.001f || !start;
		return (Vector3.Distance(transform.position, pathfind.destination) <= 2) &&
				pathfind.velocity.sqrMagnitude < 0.1f;
//		return Vector3.Distance(transform.position, pathfind.destination) <= 2;
	}

	public void TargetingEnemy (GameObject enemy)
	{
		if (enemy != null)
		{
			PhotonPlayer[] pp = (from pps in PhotonNetwork.playerList
	        where (int)pps.customProperties["team"] == enemy.GetComponent<IStats>().Team
	        select pps).ToArray ();
			playerTargetAttack = pp[0];
		}
		else playerTargetAttack = null;
		
		targetAttack = enemy;
	}

	private Unit BinarySearch(Unit[] units, Unit unit, int first, int last)
	{
		if (first > last)
			return null;

		int mid = (first + last) / 2;  // compute mid point.

		//selecionar unidade
		Unit testedUnit = units[mid];

		//verificar se é do mesmo time
		if (testedUnit.Team == unit.Team)
		{
			if(Random.value % 2 == 0)
				return BinarySearch(units, unit, first, mid - 1);
			else
				return BinarySearch(units, unit, mid + 1, last);
		}

		Debug.Log (testedUnit.Team + " == " + unit.Team);

		//obtendo posição x
		float testedUnitX = testedUnit.transform.position.x;
		float unitX 	  = unit.transform.position.x;

		if(unit.distanceView > Mathf.Abs(testedUnitX - unitX))
		{
			float testedUnitZ = testedUnit.transform.position.z;
			float unitZ 	  = unit.transform.position.z;

			if(unit.distanceView > Mathf.Abs(testedUnitZ - unitZ))
			{
				//unidade verificada está dentro da visão da unidade atual
				return testedUnit;
			}
			//não está dentro da visão, continua verificando
		}

		if(testedUnitX > unitX + unit.distanceView)
		{
			// Call ourself for the lower part of the array
			return BinarySearch(units, unit, first, mid - 1);
		}
		else
		{
			// Call ourself for the upper part of the array
			return BinarySearch(units, unit, mid + 1, last);
		}
	}

	private void StartCheckEnemy ()
	{
		if (!invokeCheckEnemy)
		{
			InvokeRepeating ("CheckEnemyIsClose", 0.3f, 0.3f);
			invokeCheckEnemy = true;
		}
	}

	private void CancelCheckEnemy ()
	{
		if (invokeCheckEnemy)
		{
			CancelInvoke ("CheckEnemyIsClose");
			invokeCheckEnemy = false;
		}
	}

	private void CheckEnemyIsClose ()
	{
		/*
		Unit[] soldiers = ComponentGetter.Get<TroopController>().soldiers.ToArray();

		Unit nearestUnit = BinarySearch (soldiers, this, 0, soldiers.Length - 1);

		if(nearestUnit != null)
		{
			TargetingEnemy (nearestUnit.gameObject);
			unitState = UnitState.Walk;
		}
		*/

		Collider[] nearbyUnits = Physics.OverlapSphere (transform.position, distanceView, 1<<LayerMask.NameToLayer ("Unit"));

		if (nearbyUnits.Length == 0) return;

		GameObject unitSelected = null;
        for (int i = 0; i != nearbyUnits.Length; i++)
		{
			if (nearbyUnits[i].GetComponent<Unit> ())
			{
				if (nearbyUnits[i].GetComponent<Unit> ().Team != Team)
				{
					if (unitSelected == null) unitSelected = nearbyUnits[i].gameObject;
					else
					{
						if (Vector3.Distance (transform.position, nearbyUnits[i].transform.position) <
							Vector3.Distance (transform.position, unitSelected.transform.position))
						{
							unitSelected = nearbyUnits[i].gameObject;
						}
					}
				}
			}
			else
			{
				if (nearbyUnits[i].GetComponent<FactoryBase> ().Team != Team)
				{
					if (unitSelected == null) unitSelected = nearbyUnits[i].gameObject;
					else
					{
						if (Vector3.Distance (transform.position, nearbyUnits[i].transform.position) <
							Vector3.Distance (transform.position, unitSelected.transform.position))
						{
							unitSelected = nearbyUnits[i].gameObject;
						}
					}
				}
			}
        }

		if (unitSelected == null) return;
		else
		{
			TargetingEnemy (unitSelected);
			followingTarget = true;
		}
	}

	public virtual IEnumerator OnDie ()
	{
		IsDead = true;
		
		pathfind.Stop ();

		unitState = UnitState.Die;

		troopController.RemoveSoldier(this);
		
		if (Selected) Deselect ();

		if (unitAnimation.DieAnimation)
		{
			ControllerAnimation.PlayCrossFade (unitAnimation.DieAnimation, WrapMode.ClampForever, PlayMode.StopAll);
			yield return StartCoroutine (ControllerAnimation.WaitForAnimation (unitAnimation.DieAnimation, 2f));
		}

		if (IsNetworkInstantiate)
		{
			if (photonView.isMine) PhotonNetwork.Destroy(gameObject);
		}
		else Destroy (gameObject);
	}

	internal void ResetPathfindValue ()
	{
		pathfind.acceleration = normalAcceleration;
		pathfind.speed = normalSpeed;
		pathfind.angularSpeed = normalAngularSpeed;
	}

	// GIZMOS
	void OnDrawGizmosSelected ()
	{
		DrawGizmosSelected ();
	}

	public virtual void DrawGizmosSelected ()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere (this.transform.position, distanceView);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (this.transform.position, rangeAttack);
	}

	public override void SetVisible(bool isVisible)
	{
		ComponentGetter
			.Get<TroopController>()
				.ChangeVisibility (this, isVisible);

		model.SetActive(isVisible);
	}

	public override bool IsVisible
	{
		get
		{
			return model.activeSelf;
		}
	}
}
