using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Visiorama.Extension;
using Visiorama;

public class Unit : IStats, IMovementObservable, IMovementObserver
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

	public int force;
	public float distanceView       = 15f;
	public float attackRange        = 5f;
	public float attackDuration     = 1f;
	public float probeRange         = 1f; // how far the character can "see"
    public float turnSpeedAvoidance = 50f; // how fast to turn
	public int numberOfUnits = 1;
	public float speed { get { return Pathfind.speed; } }

	public string guiTextureName;

	public UnitAnimation unitAnimation;
	
	public float timeToSpawn;

	public int AdditionalForce { get; set; }

	public bool IsAttacking { get; protected set; }
	public bool IsDead { get; protected set; }

	public Animation ControllerAnimation;

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

	protected NavMeshAgent Pathfind;

	public float GetPathFindRadius
	{
		get	{
			return Pathfind.radius;
		}
	}

	protected Vector3 PathfindTarget
	{
		get {
			return m_pathFindTarget;
		}

		set {
			m_lastSavedPosition = m_pathFindTarget;
			m_pathFindTarget = value;
		}
	}

	private Vector3 m_pathFindTarget;
	private Vector3 m_lastSavedPosition;

	protected HUDController hudController;
	protected InteractionController interactionController;

	protected HealthBar healthBar;

	protected float normalAcceleration;
	protected float normalSpeed;
	protected float normalAngularSpeed;

	// IMovementObservable
	List<IMovementObserver> observers = new List<IMovementObserver> ();

	//IMovementObserver
	IMovementObservable followedUnit = null;

	public override void Init ()
	{
		base.Init();

		if (ControllerAnimation == null) ControllerAnimation = gameObject.animation;
		if (ControllerAnimation == null) ControllerAnimation = GetComponentInChildren<Animation> ();
		if (ControllerAnimation != null)
		{
			if (gameplayManager.IsSameTeam (team)) ControllerAnimation.cullingType = AnimationCullingType.AlwaysAnimate;
			else ControllerAnimation.cullingType = AnimationCullingType.BasedOnRenderers;
		}

		statsController       = ComponentGetter.Get<StatsController> ();
		hudController         = ComponentGetter.Get<HUDController> ();
		interactionController = ComponentGetter.Get<InteractionController>();

		Pathfind = GetComponent<NavMeshAgent>();

		normalAcceleration = Pathfind.acceleration;
		normalSpeed        = Pathfind.speed;
		normalAngularSpeed = Pathfind.angularSpeed;

		PathfindTarget = transform.position;

		this.gameObject.tag   = "Unit";
		this.gameObject.layer = LayerMask.NameToLayer ("Unit");

		GameController gc = ComponentGetter.Get<GameController> ();

		float timeToNotifyMovementObservers = 1.0f / gc.targetFPS;

		InvokeRepeating ("NotifyMovement", timeToNotifyMovementObservers, timeToNotifyMovementObservers);

		if (!enabled) enabled = playerUnit;
	}

	void Update ()
	{
		if (unitState != UnitState.Die) IAStep ();
	}

	void OnDestroy ()
	{
		if (gameplayManager.IsBoot (team)) return;
		
		if (Selected && !playerUnit)
		{
			hudController.RemoveEnqueuedButtonInInspector (this.name, Unit.UnitGroupQueueName);

			Deselect ();
		}
		if (!IsRemoved && !playerUnit)
		{
			statsController.RemoveStats (this);
		}
	}

	public virtual void IAStep ()
	{
		if (gameplayManager.IsBoot (team))
		{
			if (!PhotonNetwork.isMasterClient)
				return;
		}
		else
		{
			if (!playerUnit)
				return;
		}

//		MoveAvoidance ();

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
					ControllerAnimation[unitAnimation.Walk.name].normalizedSpeed = unitAnimation.walkSpeed * Mathf.Clamp(Pathfind.velocity.sqrMagnitude, 0f, 1f);
					ControllerAnimation.PlayCrossFade (unitAnimation.Walk, WrapMode.Loop);
					
			}
				
				CancelCheckEnemy ();

				if (targetAttack != null)
				{
					PathfindTarget = transform.position;

					if (IsRangeAttack(targetAttack))
					{
						unitState = UnitState.Attack;
					}
//					else if (InDistanceView (targetAttack.transform.position))
					else if (targetAttack.GetComponent<IStats> ().IsVisible)
					{
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
							PathfindTarget = targetAttack.transform.position + (targetAttack.transform.forward * targetAttack.GetComponent<CapsuleCollider>().radius);
							Move (PathfindTarget);
						}
					}
				}
				else if (MoveComplete(PathfindTarget))
				{
					StopMove ();
					unitState = UnitState.Idle;
				}
				break;
			case UnitState.Attack:

				followingTarget = true;

				if (targetAttack != null)
				{
					if (targetAttack.GetComponent<IStats>().IsRemoved)
					{
						TargetingEnemy (null);
						IsAttacking = false;
					}
				}

				if (IsAttacking) return;

				StopMove ();

				PathfindTarget = transform.position;

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
		if (IsVisible)
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
	}

	#region Move Pathfind w/ Avoidance
	public void Move (Vector3 destination)
	{
		if (!Pathfind.updatePosition) Pathfind.updatePosition = true;

		if (PathfindTarget != destination) Pathfind.SetDestination (destination);

		PathfindTarget = destination;

		unitState = UnitState.Walk;
	}

	void MoveAvoidance ()
	{
		if (unitState != UnitState.Walk) return;

        RaycastHit hit;
        Vector3 dir = (PathfindTarget - transform.position).normalized;

        bool previousCastMissed = true;

		Vector3 probePoint = transform.position;
		probePoint.y += 0.1f;

        if(Physics.Raycast(probePoint, transform.forward, out hit, probeRange))
		{
			if (targetAttack != null)
			{
	            if(obstacleInPath != targetAttack)
				{
					// ignore our target
	                Debug.Log("Found an object in path! - " + gameObject.name);
	                Debug.DrawLine(transform.position, hit.point, Color.green);
	                previousCastMissed = false;
	                obstacleAvoid = true;
	                Pathfind.Stop(true);
	                Pathfind.ResetPath();
	                if(hit.transform != transform)
					{
	                    obstacleInPath = hit.transform;
	                    Debug.Log("I hit: " + hit.transform.gameObject.name);
	                    dir += hit.normal * turnSpeedAvoidance;
	                    Debug.Log("moving around an object - " + gameObject.name);
	                }
	            }
			}
			else
			{
				// ignore our target
                Debug.Log("Found an object in path! - " + gameObject.name);
                Debug.DrawLine(transform.position, hit.point, Color.green);
                previousCastMissed = false;
                obstacleAvoid = true;
                Pathfind.Stop(true);
                Pathfind.ResetPath();
                if(hit.transform != transform)
				{
                    obstacleInPath = hit.transform;
                    Debug.Log("I hit: " + hit.transform.gameObject.name);
                    dir += hit.normal * turnSpeedAvoidance;
                    Debug.Log("moving around an object - " + gameObject.name);
                }
			}
        }

		Vector3 leftReference = probePoint;
		leftReference.x -= (Pathfind.radius);

        if (obstacleAvoid &&
			previousCastMissed &&
			Physics.Raycast(leftReference, transform.forward, out hit, probeRange))
		{
			if (targetAttack != null)
			{
	            if(obstacleInPath != targetAttack)
				{
					// ignore our target
	                Debug.DrawLine(leftReference, hit.point, Color.red);
	                obstacleAvoid = true;
	                Pathfind.Stop();
	                if(hit.transform != transform) {
	                    obstacleInPath = hit.transform;
	                    previousCastMissed = false;
	                    Debug.Log("moving around an object");
	                    dir += hit.normal * turnSpeedAvoidance;
	                }
	            }
			}
			else
			{
				// ignore our target
                Debug.DrawLine(leftReference, hit.point, Color.red);
                obstacleAvoid = true;
                Pathfind.Stop();
                if(hit.transform != transform) {
                    obstacleInPath = hit.transform;
                    previousCastMissed = false;
                    Debug.Log("moving around an object");
                    dir += hit.normal * turnSpeedAvoidance;
                }
			}
        }

		Vector3 rightReference = probePoint;
		rightReference.x += (Pathfind.radius);

        // check the other side :)
        if (obstacleAvoid &&
			previousCastMissed &&
			Physics.Raycast(rightReference, transform.forward, out hit, probeRange))
		{
			if (targetAttack != null)
			{
	            if (obstacleInPath != targetAttack)
				{
					// ignore our target
	                Debug.DrawLine(rightReference, hit.point, Color.green);
	                obstacleAvoid = true;
	                Pathfind.Stop();
	                if(hit.transform != transform) {
	                    obstacleInPath = hit.transform;
	                    dir += hit.normal * turnSpeedAvoidance;
	                }
	            }
			}
			else
			{
				// ignore our target
                Debug.DrawLine(rightReference, hit.point, Color.green);
                obstacleAvoid = true;
                Pathfind.Stop();
                if(hit.transform != transform) {
                    obstacleInPath = hit.transform;
                    dir += hit.normal * turnSpeedAvoidance;
                }
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
			    obstacleInPath = null; // Hakuna Matata
			    Pathfind.ResetPath();
			    Pathfind.SetDestination(PathfindTarget);
			    Pathfind.Resume(); // Unity nav can resume movement control
			}
		}

        // this is what actually moves the character when under avoidance control
        if(obstacleAvoid) {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime);
            transform.position += transform.forward * Pathfind.speed * Time.deltaTime;
        }
    }

	public void StopMove ()
	{
		Pathfind.Stop ();
	}
	#endregion

	private IEnumerator Attack ()
	{
		Quaternion rotation = Quaternion.LookRotation(targetAttack.transform.position - transform.position);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * Pathfind.angularSpeed);

		if (unitAnimation.Attack)
		{
			ControllerAnimation.PlayCrossFade (unitAnimation.Attack, WrapMode.Once);

			IsAttacking = true;

			if (!PhotonNetwork.offlineMode)
			{
				photonView.RPC ("AttackStat", playerTargetAttack, targetAttack.name, force + AdditionalForce);
			}
			else
			{
				targetAttack.GetComponent<IStats>().ReceiveAttack(force + AdditionalForce);
			}

			yield return StartCoroutine (ControllerAnimation.WhilePlaying (unitAnimation.Attack));

			IsAttacking = false;
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
					targetAttack.GetComponent<IStats>().ReceiveAttack(force + AdditionalForce);
				}
				else
				{
					photonView.RPC ("AttackStat", playerTargetAttack, targetAttack.name, force + AdditionalForce);
//					photonView.RPC ("AttackUnit", targetAttack.GetPhotonView().owner, targetAttack.name, force + AdditionalForce);
//					photonView.RPC ("AttackUnit", PhotonTargets.AllBuffered, targetAttack.name, force + AdditionalForce);
				}

				GameObject attackObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
				attackObj.transform.position = transform.position + transform.forward;
				Destroy (attackObj, 0.5f);

                attackBuff = 0;
            }
		}
	}

	public override void Select ()
	{
		base.Select ();

		healthBar = hudController.CreateHealthBar (transform, MaxHealth, "Health Reference");
		healthBar.SetTarget (this);

		Hashtable ht = new Hashtable();

		ht["item"] = this;

		hudController.CreateSelected (transform, sizeOfSelected, gameplayManager.GetColorTeam (team));
		hudController.CreateEnqueuedButtonInInspector ( this.name,
														Unit.UnitGroupQueueName,
														ht,
														this.guiTextureName,
														(hud_ht) =>
														{
															statsController.DeselectAllStats();
															statsController.SelectStat(this, true);
														});

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
																									statsController.MoveTroop(position);
																								});
															break;
														case MovementAction.ActionType.Patrol:

															break;
														case MovementAction.ActionType.CancelMovement:
															StopMove();
															break;
														case MovementAction.ActionType.Follow: //Rally Point

															break;
														case MovementAction.ActionType.Attack:
															break;
														}
													});
		}
	}

	public override void Deselect ()
	{
		base.Deselect ();

		hudController.DestroySelected (transform);
	}

	public bool IsRangeAttack (GameObject target)
	{
		if (target.GetComponent<FactoryBase> () != null)
		{
			return Vector3.Distance(transform.position, target.transform.position) <= (attackRange + target.GetComponent<FactoryBase> ().helperCollider.radius);
		}
		else
		{
			return Vector3.Distance(transform.position, target.transform.position) <= (attackRange + target.GetComponent<CapsuleCollider> ().radius);
		}
	}

	public bool InDistanceView (Vector3 position)
	{
		return Vector3.Distance(transform.position, position) <= distanceView;
	}

	public bool MoveComplete (Vector3 destination)
	{
		return (Vector3.Distance(transform.position, Pathfind.destination) <= 2) &&
				Pathfind.velocity.sqrMagnitude < 0.1f;
//		return Vector3.Distance(transform.position, destination) <= 2;
	}

//	bool start = false;
	public bool MoveComplete ()
	{
//		if (pathfind.desiredVelocity.sqrMagnitude < 0.001f) start = !start;
//		return pathfind.desiredVelocity.sqrMagnitude < 0.001f || !start;
		return (Vector3.Distance(transform.position, Pathfind.destination) <= 2) &&
				Pathfind.velocity.sqrMagnitude < 0.1f;
//		return Vector3.Distance(transform.position, pathfind.destination) <= 2;
	}

	public void TargetingEnemy (GameObject enemy)
	{
		if (enemy != null)
		{
			if (!gameplayManager.IsBoot (enemy.GetComponent<IStats>().team))
			{
				PhotonPlayer[] pp = (from pps in PhotonNetwork.playerList
		        where (int)pps.customProperties["team"] == enemy.GetComponent<IStats>().team
		        select pps).ToArray ();
				playerTargetAttack = pp[0];
			}
			else
			{
				playerTargetAttack = PhotonNetwork.masterClient;
			}

			followingTarget = true;
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
		if (testedUnit.team == unit.team)
		{
			if(Random.value % 2 == 0)
				return BinarySearch(units, unit, first, mid - 1);
			else
				return BinarySearch(units, unit, mid + 1, last);
		}

		Debug.Log (testedUnit.team + " == " + unit.team);

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

		GameObject enemyFound = null;
        for (int i = 0; i != nearbyUnits.Length; i++)
		{
			if (nearbyUnits[i].GetComponent<IStats> ())
			{
				if (gameplayManager.IsBoot (team))
				{
					if (gameplayManager.IsBoot (nearbyUnits[i].GetComponent<IStats> ().team))
						continue;
					
					if (enemyFound == null)
					{
						if (!nearbyUnits[i].GetComponent<IStats> ().IsRemoved)
							enemyFound = nearbyUnits[i].gameObject;
					}
					else
					{
						if (!nearbyUnits[i].GetComponent<IStats> ().IsRemoved)
						{
							if (Vector3.Distance (transform.position, nearbyUnits[i].transform.position) <
								Vector3.Distance (transform.position, enemyFound.transform.position))
							{
								enemyFound = nearbyUnits[i].gameObject;
							}
						}
					}
				}
				else
				{
					if (gameplayManager.SameEntity (nearbyUnits[i].GetComponent<IStats> ().team,
													nearbyUnits[i].GetComponent<IStats> ().ally))
						continue;
					
					if (enemyFound == null)
					{
						if (!nearbyUnits[i].GetComponent<IStats> ().IsRemoved)
							enemyFound = nearbyUnits[i].gameObject;
					}
					else
					{
						if (!nearbyUnits[i].GetComponent<IStats> ().IsRemoved)
						{
							if (Vector3.Distance (transform.position, nearbyUnits[i].transform.position) <
								Vector3.Distance (transform.position, enemyFound.transform.position))
							{
								enemyFound = nearbyUnits[i].gameObject;
							}
						}
					}
				}
			}
        }

		if (enemyFound == null) return;
		else
		{
			TargetingEnemy (enemyFound);
		}
	}

	public virtual IEnumerator OnDie ()
	{
		IsDead = true;

		Pathfind.Stop ();

		unitState = UnitState.Die;

		//IMovementObservable
		int c = observers.Count;
		while (--c != -1)
		{
			UnRegisterMovementObserver (observers[c]);
		}

		statsController.RemoveStats(this);

		hudController.RemoveEnqueuedButtonInInspector (this.name, Unit.UnitGroupQueueName);

		if (Selected)
		{
			Deselect ();
		}

		if (unitAnimation.DieAnimation)
		{
			ControllerAnimation.PlayCrossFade (unitAnimation.DieAnimation, WrapMode.ClampForever, PlayMode.StopAll);
			yield return StartCoroutine (ControllerAnimation.WaitForAnimation (unitAnimation.DieAnimation, 2f));
		}

		if (IsNetworkInstantiate)
		{
			PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
			Model.Battle battle = (new Model.Battle((string)pw.GetPropertyOnRoom ("battle")));

			if (photonView.isMine)
			{
				PhotonNetwork.Destroy(gameObject);

				Score.AddScorePoints ("Units lost", 1);
				Score.AddScorePoints ("Units lost", 1, battle.IdBattle);
				Score.AddScorePoints (this.category + " lost", 1);
				Score.AddScorePoints (this.category + " lost", 1, battle.IdBattle);
			}
			else
			{
				Score.AddScorePoints ("Units killed", 1);
				Score.AddScorePoints ("Units killed", 1, battle.IdBattle);
				Score.AddScorePoints (this.category + " killed", 1);
				Score.AddScorePoints (this.category + " killed", 1, battle.IdBattle);
			}

		}
		else Destroy (gameObject);
	}

	internal void ResetPathfindValue ()
	{
		Pathfind.acceleration = normalAcceleration;
		Pathfind.speed = normalSpeed;
		Pathfind.angularSpeed = normalAngularSpeed;
	}

	public override void DrawGizmosSelected ()
	{
		base.DrawGizmosSelected ();

		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere (this.transform.position, distanceView);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (this.transform.position, attackRange);
	}

	public override void SetVisible(bool isVisible)
	{
		ComponentGetter
			.Get<StatsController>()
				.ChangeVisibility (this, isVisible);

		model.SetActive(isVisible);
	}

	public override bool IsVisible
	{
		get {
			return model.activeSelf;
		}
	}

	#region IMovementObservable implementation

	public void RegisterMovementObserver (IMovementObserver observer)
	{
		observers.Add (observer);
	}

	public void UnRegisterMovementObserver (IMovementObserver observer)
	{
		bool success = observers.Remove (observer);

		if (success)
		{
			observer.OnUnRegisterObserver ();
		}
	}

	public void NotifyMovement ()
	{
		foreach (IMovementObserver o in observers)
		{
			o.UpdatePosition (transform.position);
		}
	}

	#endregion

	public void Follow (Unit unit)
	{
		unit.RegisterMovementObserver (this);
		followedUnit = unit;
	}

	public void UnFollow  ()
	{
		if (followedUnit == null)
			return;

		followedUnit.UnRegisterMovementObserver (this);
		followedUnit = null;
	}

	#region IMovementObserver implementation

	public void UpdatePosition (Vector3 newPosition)
	{
		Vector3 forwardVec = (transform.forward.normalized) * this.GetPathFindRadius * 2.0f;

		Move (newPosition - forwardVec);
	}

	public void OnUnRegisterObserver ()
	{
		Move (LastPosition);
	}

	public Vector3 LastPosition {
		get {
			return m_lastSavedPosition;
		}
	}

	#endregion

	// RPC
	[RPC]
	public virtual void AttackStat (string name, int force)
	{
		IStats stat = statsController.FindMyStat (name);
		if (stat != null)
		{
			stat.ReceiveAttack (force);
		}
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
