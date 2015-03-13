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

	public enum UnitSkill
	{
		none,
		Crusher,
		ManEater,
		BeastMaster,
		Charger,
		Pusher
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
	public int numberOfUnits 		= 1;
	public float speed { get { return NavAgent.speed; } }
	public string guiTextureName;
	public UnitAnimation unitAnimation;	
	public float timeToSpawn;
	public bool IsAttacking { get; protected set; }
	public bool IsDead { get; set; }
	public Animation ControllerAnimation;
	private bool canHit;
	public bool CanHit {
		get {
			if (!canHit) canHit = (IsDead);

			return canHit;
		}
	}   
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
	public bool moveAttack { get; set; }
	public UnitState unitState { get; set; }
	public UnitSkill unitSkill;
	public int skillBonus {get; set;}
	protected bool invokeCheckEnemy;
	protected NavMeshAgent NavAgent;
	public float GetAgentRadius {
		get	{
			return NavAgent.radius;
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
	protected InteractionController interactionController;
	protected float normalAcceleration;
	public float normalSpeed;
	protected float normalAngularSpeed;
	protected ObstacleAvoidanceType normalObstacleAvoidance;
	protected int normalAvoidancePriority;
	protected Hashtable loadAttribs = new Hashtable();

	List<IMovementObserver> IMOobservers = new List<IMovementObserver> ();
	List<IAttackObserver> IAOobservers   = new List<IAttackObserver> ();
	List<IDeathObserver> IDOobservers	 = new List<IDeathObserver> ();
	Unit followedUnit = null;

	public override void Init ()
	{
		base.Init();
		moveAttack = false;


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
		selectionController   = ComponentGetter.Get<SelectionController>();
		GameController gc     = ComponentGetter.Get<GameController> ();

		NavAgent = GetComponent<NavMeshAgent>();
		normalAcceleration = NavAgent.acceleration;
		normalSpeed        = NavAgent.speed;
		normalAngularSpeed = NavAgent.angularSpeed;
		normalObstacleAvoidance = NavAgent.obstacleAvoidanceType;
		normalAvoidancePriority = NavAgent.avoidancePriority;
		PathfindTarget = transform.position;
		this.gameObject.tag   = "Unit";
		this.gameObject.layer = LayerMask.NameToLayer ("Unit");
		float timeToNotifyMovementObservers = 1f / gc.targetFPS;
		InvokeRepeating ("NotifyMovement", timeToNotifyMovementObservers, timeToNotifyMovementObservers);
//		unitState = UnitState.Idle;	
		if (playerUnit)
		{	enabled = true;
			if (techTreeController.attribsHash.ContainsKey(category)) LoadStandardAttribs();
		}
	}

	void Update ()
	{
		if (!IsDead) IAStep ();
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
					ControllerAnimation[unitAnimation.Walk.name].normalizedSpeed = unitAnimation.walkSpeed * Mathf.Clamp(NavAgent.velocity.sqrMagnitude, 0f, 1f);
					ControllerAnimation.PlayCrossFade (unitAnimation.Walk, WrapMode.Loop);
				}
				
				if (!moveAttack)
				{
					CancelCheckEnemy ();
				}

				if (TargetAttack != null)
				{	
					PathfindTarget = transform.position;

					if (InMeleeRange(TargetAttack))
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

//				NavAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
					StopMove (true);
					
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
					if (InMeleeRange (TargetAttack))
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

	#region Move NavAgent w/ Avoidance
	public void Move (Vector3 destination)
	{
		NavAgent.enabled = true;

		NavAgent.avoidancePriority = normalAvoidancePriority;
	
		if (!NavAgent.updatePosition) NavAgent.updatePosition = true;

		if (PathfindTarget != destination) NavAgent.SetDestination (destination);

		PathfindTarget = destination;

		unitState = UnitState.Walk;
	}

	public void StopMove (bool changeState = false)
	{
		NavAgent.avoidancePriority = 4;

		if (changeState)
		{
			unitState = UnitState.Idle;
		}
		NavAgent.Stop ();
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
			smas.minDistance = 6.0f;
			smas.maxDistance = 60.0f;
			smas.rolloffMode = AudioRolloffMode.Logarithmic;
		
		}
		
	}

	public bool CheckNemesis(IStats targetStats)										 // Confere se o target stats e do tipo alvo de nossa habilidade
	{
		if (unitSkill == UnitSkill.Crusher && targetStats.GetType() == typeof(FactoryBase))	 // bonus vs factory
			return true;

		if (unitSkill == UnitSkill.ManEater && targetStats.GetType() == typeof(Unit) && targetStats.subCategory == "Man") // bonus vs factory - busca por subcategoria o nome "man"
			return true;

		if (unitSkill == UnitSkill.BeastMaster && targetStats.GetType() == typeof(Unit) && targetStats.subCategory == "Dino") 	// bonus vs factory - busca por subcategoria o nome "Dino" da
			return true;

		if (unitSkill == UnitSkill.Charger) 	// bonus por velocidade//TODO
			return false;

		if (unitSkill == UnitSkill.Pusher) 	// estuna ou empurra inimigos //TODO
			return false;

		else return false;
	}

		
	private IEnumerator Attack ()
	{
		Quaternion rotation = Quaternion.LookRotation(TargetAttack.transform.position - transform.position);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * NavAgent.angularSpeed);

		SfxAtk();

		if (unitAnimation.Attack)
		{
			ControllerAnimation.PlayCrossFade (unitAnimation.Attack, WrapMode.Once);

			IsAttacking = true;

			if (PhotonNetwork.offlineMode)
			{
				TargetAttack.GetComponent<IStats>().ReceiveAttack(force + bonusForce);
			}
			else
			{
				if (unitSkill != UnitSkill.none)
				{	
					if	(CheckNemesis(TargetAttack.GetComponent<IStats>()))
					{
						skillBonus  = Mathf.FloorToInt((force + bonusForce)*0.25f);                                   
					}
				}
				photonView.RPC ("AttackStat", playerTargetAttack, TargetAttack.name, force + bonusForce + skillBonus);
			}
			
			yield return StartCoroutine (ControllerAnimation.WhilePlaying (unitAnimation.Attack));

			skillBonus = 0; 
			IsAttacking = false;
		}
		else
		{
			Debug.LogError("Caiu no attack buffer estranho");

			if(attackBuff < attackDuration)
			{
                attackBuff += Time.deltaTime;
	        }
			else
			{
				if (PhotonNetwork.offlineMode)
				{
					TargetAttack.GetComponent<IStats>().ReceiveAttack(force + bonusForce);
				}
				else
				{
					photonView.RPC ("AttackStat", playerTargetAttack, TargetAttack.name, force + bonusForce);
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
		if(IsDead) return;
		base.Select ();

		Hashtable ht = new Hashtable();

		ht["observableHealth"] = this;
		ht["time"] = 0f;
		
		hudController.CreateSubstanceHealthBar (this, sizeOfHealthBar, MaxHealth, "Health Reference");
		hudController.CreateSelected (transform, sizeOfSelected, gameplayManager.GetColorTeam (team));

		if (!gameplayManager.IsSameTeam (this.team))
			return;

		hudController.CreateEnqueuedButtonInInspector ( this.name,
														Unit.UnitGroupQueueName,
														ht,
														this.guiTextureName,
														(hud_ht) =>
														{
															statsController.DeselectAllStats();
															statsController.SelectStat(this, true);
														},
														(ht_dcb, isDown) => 
														{
															if (isDown)
															{
																ht["time"] = Time.time;
															}
															else
															{
																if (Time.time - (float)ht["time"] > 0.1f)
																{	
																	selectionController.SelectSameCategory(this.category);						
																}																
															}
														});		
			
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

	public bool InMeleeRange (GameObject target)
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
		float distanceToDestination = Vector3.Distance(transform.position, destination);
	
		return (distanceToDestination <= NavAgent.stoppingDistance);
	}


	public bool MoveComplete ()
	{
//		if (pathfind.desiredVelocity.sqrMagnitude < 0.001f) start = !start;
//		return pathfind.desiredVelocity.sqrMagnitude < 0.001f || !start;
		return (Vector3.Distance(transform.position, NavAgent.destination) <= 2.0f);
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
		NavAgent.Stop ();
		NavAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

		IsDead = true;

		unitState = UnitState.Die;

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

				Score.AddScorePoints (DataScoreEnum.UnitsLost, 1);
				Score.AddScorePoints (DataScoreEnum.UnitsLost, 1, battle.IdBattle);
				Score.AddScorePoints (this.category + DataScoreEnum.XLost, 1);
				Score.AddScorePoints (this.category + DataScoreEnum.XLost, 1, battle.IdBattle);
			}
			else
			{
				Score.AddScorePoints (DataScoreEnum.UnitsKilled, 1);
				Score.AddScorePoints (DataScoreEnum.UnitsKilled, 1, battle.IdBattle);
				Score.AddScorePoints (this.category + DataScoreEnum.XKilled, 1);
				Score.AddScorePoints (this.category + DataScoreEnum.XKilled, 1, battle.IdBattle);
			}

		}
		else Destroy (gameObject);
	}

	internal void ResetPathfindValue ()
	{
		NavAgent.acceleration = normalAcceleration;
		NavAgent.speed = normalSpeed;
		NavAgent.angularSpeed = normalAngularSpeed;
	}

//	public override void DrawGizmosSelected ()
//	{
//		base.DrawGizmosSelected ();
//
//		Gizmos.color = Color.cyan;
//		Gizmos.DrawWireSphere (this.transform.position, distanceView);
//
//		Gizmos.color = Color.red;
//		Gizmos.DrawWireSphere (this.transform.position, attackRange);
//
//		Gizmos.color = Color.yellow;
//		Gizmos.DrawRay (new Ray (transform.position, PathfindTarget));
//	}

	public override void SetVisible(bool isVisible)
	{
		ComponentGetter.Get<StatsController>().ChangeVisibility (this, isVisible);
		model.SetActive(isVisible);
		if (firstDamage)
		{
			if(isVisible)hudController.CreateSubstanceHealthBar (this, sizeOfHealthBar, MaxHealth, "Health Reference");
			else hudController.DestroySelected(transform);
		}
	}

	public override bool IsVisible
	{
		get 
		{
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
		if (TargetAttack != null || followedUnit == null)
			return;

		float minDistanceBetweenFollowedUnit = (followedUnit.GetAgentRadius + this.GetAgentRadius) * 1.2f;
		
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
		if(NavAgent != null) Move (LastPosition);
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

	public virtual void LoadStandardAttribs()											// Inicializa os tributos da unidade conforme Techtree
	{
		Hashtable ht = techTreeController.attribsHash[category] as Hashtable;
		bonusForce		= (int)ht["bonusforce"];
		bonusSpeed		= (int)ht["bonusspeed"];
		bonusSight		= (int)ht["bonussight"];
		bonusDefense	= (int)ht["bonusdefense"];
	}	

	// RPC
	[RPC]
	public virtual void AttackStat (string name, int force)
	{
		IStats stat = statsController.FindMyStat (name);
		if (stat != null && unitState != UnitState.Die)
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
