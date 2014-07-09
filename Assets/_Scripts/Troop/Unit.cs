using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Visiorama.Extension;
using Visiorama;

public class Unit : IStats, IMovementObservable,
							IMovementObserver, 
							IAttackObservable,
							IAttackObserver,
							IDeathObservable
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
	public bool CanHit {
		get {
			if (!canHit) canHit = (IsDead);

			return canHit;
		}
	}

    private Transform obstacleInPath; 	// we found something!
    private bool obstacleAvoid = false; // internal var

	protected PhotonPlayer playerTargetAttack;

	protected GameObject m_targetAttack;
	protected GameObject TargetAttack {
		get { return m_targetAttack; }
		set {
			if (m_targetAttack != value)
			{
				m_targetAttack = value;
				NotifyBeginAttack ();
			}
		}
	}
	protected bool followingTarget;
	protected float attackBuff;

	public UnitState unitState { get; set; }

	protected bool invokeCheckEnemy;

	protected NavMeshAgent Pathfind;

	public float GetPathFindRadius {
		get	{
			return Pathfind.radius;
		}
	}

	protected Vector3 PathfindTarget {
		get { return m_pathFindTarget; }
		set {
			m_lastSavedPosition = m_pathFindTarget;
			m_pathFindTarget = value;
		}
	}

	private Vector3 m_pathFindTarget;
	private Vector3 m_lastSavedPosition;

//	protected HUDController hudController;
	protected InteractionController interactionController;

	protected float normalAcceleration;
	protected float normalSpeed;
	protected float normalAngularSpeed;

	// IObservers
	List<IMovementObserver> IMOobservers = new List<IMovementObserver> ();
	List<IAttackObserver> IAOobservers   = new List<IAttackObserver> ();
	List<IDeathObserver> IDOobservers	 = new List<IDeathObserver> ();

	//IMovementObserver
	Unit followedUnit = null;

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

		float timeToNotifyMovementObservers = 1f / gc.targetFPS;

		InvokeRepeating ("NotifyMovement", timeToNotifyMovementObservers, timeToNotifyMovementObservers);

		unitState = UnitState.Idle;
			
		if (!enabled) enabled = playerUnit;
	}

	void Update ()
	{
		if (unitState != UnitState.Die) IAStep ();
	}

	void OnDestroy ()
	{
		if (gameplayManager.IsBotTeam (this)) return;
		
		if (Selected && !playerUnit)
		{
			hudController.RemoveEnqueuedButtonInInspector (this.name, Unit.UnitGroupQueueName);

			Deselect ();
		}
		if (!WasRemoved && !playerUnit)
		{
			statsController.RemoveStats (this);
		}
	}

	public virtual void IAStep ()
	{
		if (gameplayManager.IsBotTeam (this))
		{
			if (!PhotonNetwork.isMasterClient)
				return;
		}
		else
		{
			if (!playerUnit)
				return;
		}

		switch (unitState)
		{
			case UnitState.Idle:
				if (unitAnimation.Idle)
					ControllerAnimation.PlayCrossFade (unitAnimation.Idle, WrapMode.Loop);

				StartCheckEnemy ();

				if (TargetAttack != null) unitState = UnitState.Walk;

				break;
			case UnitState.Walk:
				if (unitAnimation.Walk)
				{
					ControllerAnimation[unitAnimation.Walk.name].normalizedSpeed = unitAnimation.walkSpeed * Mathf.Clamp(Pathfind.velocity.sqrMagnitude, 0f, 1f);
					ControllerAnimation.PlayCrossFade (unitAnimation.Walk, WrapMode.Loop);
				}
				
				CancelCheckEnemy ();

				if (TargetAttack != null)
				{	
					PathfindTarget = transform.position;

					if (IsRangeAttack(TargetAttack))
					{
						unitState = UnitState.Attack;
					}
//					else if (InDistanceView (targetAttack.transform.position))
					else if (TargetAttack.GetComponent<IStats> ().IsVisible)
					{
						Move (TargetAttack.transform.position);
					}
					else
					{
						if (followingTarget)
						{
							TargetAttack    = null;
							unitState       = UnitState.Idle;
							followingTarget = false;
						}
						else
						{
							PathfindTarget = TargetAttack.transform.position + (TargetAttack.transform.forward * TargetAttack.GetComponent<CapsuleCollider>().radius);
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

				if (TargetAttack != null)
				{
					if (TargetAttack.GetComponent<IStats>().WasRemoved)
					{
						TargetingEnemy (null);
						IsAttacking = false;
					}
				}

				if (IsAttacking) return;

				StopMove ();

				PathfindTarget = transform.position;

				if (TargetAttack != null)
				{
					if (IsRangeAttack (TargetAttack))
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
		Pathfind.enabled = true;
	
		if (!Pathfind.updatePosition) Pathfind.updatePosition = true;

		if (PathfindTarget != destination) Pathfind.SetDestination (destination);

		PathfindTarget = destination;

		unitState = UnitState.Walk;
	}

	public void StopMove (bool changeState = false)
	{
		if (changeState)
		{
			unitState = UnitState.Idle;
		}
		Pathfind.Stop ();
	}
	#endregion

	public void SfxAtk ()
	{
		AudioClip sfxAtk = SoundManager.LoadFromGroup("Attack");
		
		Vector3 u = this.transform.position;
		
		AudioSource smas = SoundManager.PlayCappedSFX (sfxAtk, "Attack", 1f, 1f, u);

		if (smas != null)
		{

			smas.dopplerLevel = 0f;
			smas.minDistance = 3.0f;
			smas.maxDistance = 20.0f;
			smas.rolloffMode = AudioRolloffMode.Custom;
		
		}
		
	}
		
		//			SoundManager.PlayCappedSFX (sfxAtk, "Attack", 1f, 1f, u);
		
	private IEnumerator Attack ()
	{
		Quaternion rotation = Quaternion.LookRotation(TargetAttack.transform.position - transform.position);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * Pathfind.angularSpeed);

		SfxAtk();


		if (unitAnimation.Attack)
		{
			ControllerAnimation.PlayCrossFade (unitAnimation.Attack, WrapMode.Once);

			IsAttacking = true;

			if (PhotonNetwork.offlineMode)
			{
				TargetAttack.GetComponent<IStats>().ReceiveAttack(force + AdditionalForce);
			}
			else
			{
				photonView.RPC ("AttackStat", playerTargetAttack, TargetAttack.name, force + AdditionalForce);
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
					TargetAttack.GetComponent<IStats>().ReceiveAttack(force + AdditionalForce);
				}
				else
				{
					photonView.RPC ("AttackStat", playerTargetAttack, TargetAttack.name, force + AdditionalForce);
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

		Hashtable ht = new Hashtable();

		ht["observableHealth"] = this;
		
		hudController.CreateSubstanceHealthBar (this, sizeOfSelected, MaxHealth, "Health Reference");
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

		if (!gameplayManager.IsSameTeam (this.team))
			return;
		
		foreach (MovementAction ma in movementActions)
		{
			ht = new Hashtable();
			ht["actionType"] = ma.actionType;
			
			hudController.CreateButtonInInspector ( ma.buttonAttributes.name,
													ma.buttonAttributes.gridItemAttributes.Position,
													ht,
													ma.buttonAttributes.spriteName,
													(_ht) =>
			                                       {
														MovementAction.ActionType action = (MovementAction.ActionType)_ht["actionType"];
														switch(action)
														{
														case MovementAction.ActionType.Move:
															interactionController.AddCallback(TouchController.IdTouch.Id0,
																								(position, hit) =>
																								{
																									statsController.MoveTroop(position);
																								});
															break;
														case MovementAction.ActionType.Patrol:
//					
//															interactionController.AddCallback(TouchController.IdTouch.Id0,
//															                                 	(position, hit) =>
//															                                  	{
//																									IStats istats = hit.GetComponent <IStats> ();
//																									
//																									//Nao pode ser nulo e nao pode atacar aliados
//																									//so do mesmo time ou inimigo
//																									if (istats != null && 
//																									    (gameplayManager.IsAlly (istats) && gameplayManager.IsSameTeam (istats)))
//																									{
//																										statsController.AttackTroop (istats.gameObject);
//																									}
//																								});
															break;
														case MovementAction.ActionType.CancelMovement:
															StopMove (true);
															break;
														case MovementAction.ActionType.Follow:
															interactionController.AddCallback(TouchController.IdTouch.Id0,
															                                 	(position, hit) =>
															                                  	{
																									Unit unit = hit.GetComponent <Unit> ();
																									
																									Debug.LogWarning ("Seguindo apenas do mesmo time, nao segue aliados.");
																									if (unit != null && gameplayManager.IsSameTeam (unit))
																									{
																										statsController.FollowTroop (unit);
																									}
																								});
															break;
														case MovementAction.ActionType.Attack:
															interactionController.AddCallback(TouchController.IdTouch.Id0,
															                                  	(position, hit) =>
															                                  	{
																									IStats istats = hit.GetComponent <IStats> ();
																									
																									//Nao pode ser nulo e nao pode atacar aliados
																									//so do mesmo time ou inimigo
																									if (istats != null && 
						    																			(gameplayManager.IsAlly (istats) && gameplayManager.IsSameTeam (istats)))
																									{
																										statsController.AttackTroop (istats.gameObject);
																									}
																								});
															break;
														}
													});
		}
	}

	public override void Deselect ()
	{
		if (statsController.selectedStats.Count == 1) 
		{
			hudController.DestroyOptionsBtns ();
		}

		base.Deselect ();

		int c = IDOobservers.Count;
		while (--c != -1)
		{
			UnRegisterDeathObserver (IDOobservers[c]);
		}

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
		float distanceToDestination = Vector3.Distance(transform.position, Pathfind.destination);
	
		return (distanceToDestination <= 1.0f) && Pathfind.velocity.sqrMagnitude < 0.1f;
	}

//	bool start = false;
	public bool MoveCompleted ()
	{
//		if (pathfind.desiredVelocity.sqrMagnitude < 0.001f) start = !start;
//		return pathfind.desiredVelocity.sqrMagnitude < 0.001f || !start;
		return (Vector3.Distance(transform.position, Pathfind.destination) <= 2.0f) &&
				Pathfind.velocity.sqrMagnitude < 0.1f;
//		return Vector3.Distance(transform.position, pathfind.destination) <= 2;
	}

	public void TargetingEnemy (GameObject enemy)
	{
		if (enemy != null)
		{
			if (!gameplayManager.IsBotTeam (enemy.GetComponent<IStats>()))
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

		TargetAttack = enemy;
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
			InvokeRepeating ("CheckEnemyIsClose", 0.3f, 0.2f);
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
		
		Collider[] nearbyUnits = Physics.OverlapSphere (transform.position, distanceView, 1 << LayerMask.NameToLayer ("Unit"));

		if (nearbyUnits.Length == 0) return;

		GameObject enemyFound = null;
		IStats cStats = null;
		
        for (int i = 0; i != nearbyUnits.Length; i++)
		{
			cStats = nearbyUnits[i].GetComponent<IStats> ();
			if (cStats)
			{
				if (gameplayManager.IsBotTeam (this))
				{
					if (gameplayManager.IsBotTeam (cStats))
						continue;
					
					if (enemyFound == null)
					{
						if (!cStats.WasRemoved)
						{
							enemyFound = cStats.gameObject;
						}
					}
					else
					{
						if (!cStats.WasRemoved)
						{
							if (Vector3.Distance (transform.position, cStats.transform.position) <
								Vector3.Distance (transform.position, enemyFound.transform.position))
							{
								enemyFound = cStats.gameObject;
							}
						}
					}
				}
				else
				{
					if (gameplayManager.IsNotEnemy (cStats.team, cStats.ally))
						continue;
					
					if (enemyFound == null)
					{
						if (!cStats.WasRemoved)
							enemyFound = nearbyUnits[i].gameObject;
					}
					else
					{
						if (!cStats.WasRemoved)
						{
							if (Vector3.Distance (transform.position, cStats.transform.position) <
								Vector3.Distance (transform.position, enemyFound.transform.position))
							{
								enemyFound = cStats.gameObject;
							}
						}
					}
				}
			}
        }

		if (enemyFound != null)
		{
			TargetingEnemy (enemyFound);
		}
	}


	public virtual IEnumerator OnDie ()
	{
		IsDead = true;

		AudioClip sfxDeath = SoundManager.LoadFromGroup("Death");

		Vector3 u = this.transform.position;

		AudioSource smas = SoundManager.PlayCappedSFX (sfxDeath, "Death", 1f, 1f, u);

		if (smas != null)
		{
			smas.dopplerLevel = 0.0f;
			smas.spread = 0.3f;
			smas.minDistance = 3.0f;
			smas.maxDistance = 30.0f;
			smas.rolloffMode =AudioRolloffMode.Custom;

		}

		Pathfind.Stop ();

		unitState = UnitState.Die;

		//IMovementObservable
		int c = IMOobservers.Count;
		while (--c != -1)
		{
			UnRegisterMovementObserver (IMOobservers[c]);
		}
		
		//IAttackObservable
		c = IAOobservers.Count;
		while (--c != -1)
		{
			UnRegisterAttackObserver (IAOobservers[c]);
		}

		statsController.RemoveStats(this);

		ComponentGetter.Get<MiniMapController> ().RemoveUnit (this.transform, this.team);
		gameplayManager.DecrementUnit (this.team, this.numberOfUnits);
		
		//IDeathObservable
		NotifyDeath ();

			
		c = IDOobservers.Count;
		while (--c != -1)
		{
			UnRegisterDeathObserver (IDOobservers[c]);
		}
		
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

//				Score.AddScorePoints (DataScoreEnum.UnitsLost, 1);
//				Score.AddScorePoints (DataScoreEnum.UnitsLost, 1, battle.IdBattle);
//				Score.AddScorePoints (this.category + DataScoreEnum.XLost, 1);
//				Score.AddScorePoints (this.category + DataScoreEnum.XLost, 1, battle.IdBattle);
			}
			else
			{
//				Score.AddScorePoints (DataScoreEnum.UnitsKilled, 1);
//				Score.AddScorePoints (DataScoreEnum.UnitsKilled, 1, battle.IdBattle);
//				Score.AddScorePoints (this.category + DataScoreEnum.XKilled, 1);
//				Score.AddScorePoints (this.category + DataScoreEnum.XKilled, 1, battle.IdBattle);
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

		Gizmos.color = Color.yellow;
		Gizmos.DrawRay (new Ray (transform.position, PathfindTarget));
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
	
	public void Follow (Unit followed)
	{
		TargetAttack    = null;
		followingTarget = false;

		followed.RegisterMovementObserver (this);
		followed.RegisterAttackObserver (this);
		followedUnit = followed;
		
		if (followed.followedUnit == this)
			followed.UnFollow ();
	}
	
	public void UnFollow  ()
	{
		if (followedUnit == null)
			return;
		
		followedUnit.UnRegisterMovementObserver (this);
		followedUnit.UnRegisterAttackObserver (this);
		followedUnit = null;
	}

	#region IMovementObservable implementation

	/// <summary>
	/// Registers the movement observer.
	/// </summary>
	/// <param name="observer">Observer.</param>
	/// <description>
	/// Avoid using this method use the Follow method instead
	/// </description>

	public void RegisterMovementObserver (IMovementObserver observer)
	{
		IMOobservers.Add (observer);
	}

	/// <summary>
	/// Unregister the movement observer.
	/// </summary>
	/// <param name="observer">Observer.</param>
	/// Avoid using this method use the UnFollow method instead
	public void UnRegisterMovementObserver (IMovementObserver observer)
	{
		bool success = IMOobservers.Remove (observer);

		if (success)
		{
			observer.OnUnRegisterMovementObserver ();
		}
	}

	public void NotifyMovement ()
	{
		if (transform.position != LastPosition)
		{
			foreach (IMovementObserver o in IMOobservers)
			{
				o.UpdatePosition (transform.position);
			}
		}
	}

	#endregion

	#region IMovementObserver implementation

	public void UpdatePosition (Vector3 newPosition, GameObject go = null)
	{
		if (TargetAttack != null)
			return;

		float minDistanceBetweenFollowedUnit = (followedUnit.GetPathFindRadius + this.GetPathFindRadius) * 3f;

		
		Vector3 forwardVec = (this.transform.position.normalized - (followedUnit.transform.position.normalized * 2.0f))
								* minDistanceBetweenFollowedUnit;
		
		//		forwardVec = Vector3.Angle (followedUnit.transform.forward.normalized, 
		//		                            this.transform.forward.normalized)
		
		//		forwardVec.x += Random.Range (minDistanceBetweenFollowedUnit / 2.0f, minDistanceBetweenFollowedUnit);
		//		forwardVec.z += Random.Range (minDistanceBetweenFollowedUnit / 2.0f, minDistanceBetweenFollowedUnit);
		
		newPosition = newPosition + forwardVec;

		if (Vector3.Distance (this.transform.position, newPosition) < minDistanceBetweenFollowedUnit)
			return;

		Move (newPosition);
	}

	public void OnUnRegisterMovementObserver ()
	{
		Move (LastPosition);
	}

	public Vector3 LastPosition {
		get {
			return m_lastSavedPosition;
		}
	}

	#endregion

	#region IAttackObservable implementation

	public void RegisterAttackObserver (IAttackObserver observer)
	{
		IAOobservers.Add (observer);
	}

	public void UnRegisterAttackObserver (IAttackObserver observer)
	{
		bool success = IAOobservers.Remove (observer);
		
		if (success)
		{
			observer.OnUnRegisterAttackObserver ();
		}
	}

	public void NotifyBeginAttack ()
	{
		foreach (IAttackObserver o in IAOobservers)
		{
			o.BeginAttack (TargetAttack);
		}
	}

	#endregion

	#region IAttackObserver implementation

	public void BeginAttack (GameObject enemy)
	{
		CancelCheckEnemy ();
		TargetingEnemy (enemy);
	}
	
	public void OnUnRegisterAttackObserver ()
	{
		//this method is empty, don't worry ;)
	}
	#endregion

	#region IDeathObservable implementation

	public void RegisterDeathObserver (IDeathObserver observer)
	{
		IDOobservers.Add (observer);
	}

	public void UnRegisterDeathObserver (IDeathObserver observer)
	{
		IDOobservers.Remove (observer);
	}

	public void NotifyDeath ()
	{
		foreach (IDeathObserver o in IDOobservers)
		{
			o.OnObservableDie (this.gameObject);
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
