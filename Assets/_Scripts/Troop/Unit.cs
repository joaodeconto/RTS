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
	#region Declares, Serializables
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
	public const string UnitGroupQueueName = "Unit Group";
	public bool CanHit {  get {	if (!canHit) canHit = (IsDead);	return canHit;}}   
	public bool followingTarget;
	public bool moveAttack					{get; set;}
	public Vector3 moveAttackDestination	{get; set;}
	public bool hasMoveAttackDestination	{get; set;}
	public UnitState unitState 				{get; set;}
	public UnitSkill unitSkill;
	public int skillBonus 					{get; set;}
	public float normalSpeed;	
	public NavMeshPath newPath;
	public float GetAgentRadius {
		get	{if(NavAgent != null)
			return NavAgent.radius;
			else		return 1f;
		}
	}
	protected Vector3 PathfindTarget {
		get { return m_pathFindTarget; }
		set {
			m_lastSavedPosition = m_pathFindTarget;
			m_pathFindTarget = value;
		}
	}
	protected PhotonPlayer playerTargetAttack;
	protected GameObject m_targetAttack;
	protected bool invokeCheckEnemy;
	protected NavMeshAgent NavAgent;
	protected float attackBuff;
	protected float normalAcceleration;
	protected float normalAngularSpeed;
	protected ObstacleAvoidanceType normalObstacleAvoidance;
	protected int normalAvoidancePriority;
	protected Hashtable loadAttribs = new Hashtable();
	protected InteractionController interactionController;

	private bool canHit;
	private Vector3 m_pathFindTarget;
	private Vector3 m_lastSavedPosition;
	private bool unitInitialized = false;
	private bool isFollowed = false;
	private Unit followedUnit = null;

	List<IMovementObserver> IMOobservers = new List<IMovementObserver> ();
	List<IAttackObserver> IAOobservers   = new List<IAttackObserver> ();
	List<IDeathObserver> IDOobservers	 = new List<IDeathObserver> ();
	#endregion

	#region Init
	public override void Init ()
	{
		if (unitInitialized)	return;		
		unitInitialized = true;
		base.Init();
		moveAttack = true;

		if (ControllerAnimation == null) ControllerAnimation = gameObject.animation;
		if (ControllerAnimation == null) ControllerAnimation = GetComponentInChildren<Animation> ();
		if (ControllerAnimation != null)
		{
			if (gameplayManager.IsSameTeam (team)) ControllerAnimation.cullingType = AnimationCullingType.AlwaysAnimate;
			else ControllerAnimation.cullingType = AnimationCullingType.BasedOnRenderers;
		}

		if (gameplayManager.IsBotTeam(this)|| playerUnit) enabled = true;
		if (GetComponent<RangedStructure>() != null) return;

		if (PhotonNetwork.offlineMode && gameplayManager.scoreCounting)
		{
			OfflineScore oScore = ComponentGetter.Get<OfflineScore>();
			oScore.oPlayers[team].AddScorePlayer("units-created",  1);			
			oScore.oPlayers[team].AddScorePlayer(category + DataScoreEnum.XCreated, totalResourceCost);
		}

		NavAgent = GetComponent<NavMeshAgent>();
		normalAcceleration = NavAgent.acceleration;
		normalSpeed        = NavAgent.speed;
		normalAngularSpeed = NavAgent.angularSpeed;
		normalObstacleAvoidance = NavAgent.obstacleAvoidanceType;
		normalAvoidancePriority = NavAgent.avoidancePriority;
		PathfindTarget = transform.position;
		this.gameObject.tag   = "Unit";
		this.gameObject.layer = LayerMask.NameToLayer ("Unit");		

		if (playerUnit)
		{
			interactionController = ComponentGetter.Get<InteractionController>();
			if (techTreeController.attribsHash.ContainsKey(category)) LoadStandardAttribs();			
			if (!PhotonNetwork.offlineMode)
			{
				Model.Battle battle = ConfigurationData.battle;
				Score.AddScorePoints (DataScoreEnum.UnitsCreated, 1, battle.IdBattle);
				Score.AddScorePoints (category + DataScoreEnum.XCreated, totalResourceCost, battle.IdBattle);
			}
		}
	}

	public virtual void LoadStandardAttribs()											// Inicializa os tributos da unidade conforme Techtree
	{
		Hashtable ht = techTreeController.attribsHash[category] as Hashtable;
		bonusForce		= (int)ht["bonusforce"];
		bonusSpeed		= (int)ht["bonusspeed"];
		bonusSight		= (int)ht["bonussight"];
		bonusDefense	= (int)ht["bonusdefense"];
		bonusProjectile = (int)ht["bonusprojectile"];
	}
	#endregion

	#region Update, IAstep, Animation Sync

	void Update ()
	{
		if (!IsDead && ConfigurationData.InGame) IAStep ();
	}

	public virtual void IAStep ()
	{
		if (gameplayManager.IsBotTeam (this)){
			if (!PhotonNetwork.isMasterClient)
				return;
		}
		else{
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
				if (unitAnimation.Walk){
					ControllerAnimation[unitAnimation.Walk.name].normalizedSpeed = unitAnimation.walkSpeed * Mathf.Clamp(NavAgent.velocity.sqrMagnitude, 0f, 1f);
					ControllerAnimation.PlayCrossFade (unitAnimation.Walk, WrapMode.Loop);
				}
				if(isFollowed) NotifyMovement();
				if(!moveAttack)	CancelCheckEnemy();	
				if (TargetAttack != null){
					PathfindTarget = transform.position;
					if (InMeleeRange(TargetAttack))
						unitState = UnitState.Attack;					
					else if (TargetAttack.GetComponent<IStats> ().IsVisible)
						Move (TargetAttack.transform.position);					
					else{
						if (followingTarget){
							StopMove(true);
							TargetAttack    = null;
							unitState       = UnitState.Idle;
							followingTarget = false;
						}
					}
				}

				else if (MoveComplete(PathfindTarget)){
					if (hasMoveAttackDestination && MoveComplete(moveAttackDestination))		
						hasMoveAttackDestination = false;
					
					StopMove (true);					
				}
				
				if(NavAgent.isPathStale){
			
					if (NavAgent.pathStatus != NavMeshPathStatus.PathComplete){						
						if (NavAgent.pathStatus != NavMeshPathStatus.PathInvalid){
							Vector3 randomVector = Random.onUnitSphere;
							Vector3 position = PathfindTarget - randomVector;
							position.y = PathfindTarget.y;
							NavMeshHit hit;
							if(NavMesh.SamplePosition(position,out hit,1,1))
								Move (position);
						}
						else 
							Move (PathfindTarget);
					}					
				}
				break;
			case UnitState.Attack:
				followingTarget = true;
				if (TargetAttack != null){
					if (TargetAttack.GetComponent<IStats>().WasRemoved){
						TargetingEnemy (null);
						IsAttacking = false;
					}
				}

				if (IsAttacking) return;
				StopMove (false);

				if (TargetAttack != null){
					if (InMeleeRange (TargetAttack))	
						StartCoroutine(Attack ());
					
					else
						unitState = UnitState.Walk;					
				}
				else{
					if(hasMoveAttackDestination)  // depois que batalha no moveAttack checa o destination pendente.
					{
						Move(moveAttackDestination);
					}
					else
					{
						unitState = UnitState.Idle;
					}
				}
				break;
		}
	}

	public virtual void SyncAnimation ()
	{
		if (IsVisible){
			switch (unitState)
			{
			case UnitState.Idle:
				if (unitAnimation.Idle)
					ControllerAnimation.PlayCrossFade (unitAnimation.Idle, WrapMode.Loop);

				break;
			case UnitState.Walk:
				if (unitAnimation.Walk){
					ControllerAnimation[unitAnimation.Walk.name].normalizedSpeed = unitAnimation.walkSpeed;
					ControllerAnimation.PlayCrossFade (unitAnimation.Walk, WrapMode.Loop);
				}

				break;
			case UnitState.Attack:
				if (unitAnimation.Attack)
					if (!gameplayManager.IsSameTeam (team))ControllerAnimation.cullingType = AnimationCullingType.AlwaysAnimate;
					ControllerAnimation.PlayCrossFade (unitAnimation.Attack, WrapMode.Once);					

				break;
			case UnitState.Die:
				if (unitAnimation.DieAnimation)
					ControllerAnimation.PlayCrossFade (unitAnimation.DieAnimation, WrapMode.ClampForever);					

				break;
			}
		}
	}
	#endregion

	#region Move NavAgent, Follow
	public void Move (Vector3 destination)
	{
		newPath = new NavMeshPath();
		NavAgent.avoidancePriority = normalAvoidancePriority;	
		if (!NavAgent.updatePosition) NavAgent.updatePosition = true;
		if (NavAgent.SetDestination (destination))	PathfindTarget = destination;
		if (PhotonNetwork.offlineMode || playerUnit) unitState = UnitState.Walk;
	}

	public void StopMove (bool changeState = false)
	{
		NavAgent.avoidancePriority = 4;
		if (changeState)	unitState = UnitState.Idle;
		NavAgent.Stop ();
	}

	internal void ResetNavAgentValues ()
	{
		NavAgent.acceleration = normalAcceleration;
		NavAgent.speed = normalSpeed;
		NavAgent.angularSpeed = normalAngularSpeed;
		NavAgent.avoidancePriority = normalAvoidancePriority;
	}
	
	public void Follow (Unit followed)
	{
		TargetAttack    = null;
		followingTarget = false;
		followedUnit = followed;		
		if (followed.followedUnit == this)	followed.UnFollow ();
		Move(followed.transform.position);
		followed.RegisterMovementObserver (this);
		followed.RegisterAttackObserver (this);
	}
	
	public void UnFollow  ()
	{
		if (followedUnit == null)	return;		
		followedUnit.UnRegisterMovementObserver (this);
		followedUnit.UnRegisterAttackObserver (this);
		followedUnit = null;
	}
	#endregion

	#region Attack, Targeting 
	[RPC]
	public void SfxAtk (Vector3 sourcePosition)
	{
		AudioClip sfxAtk = SoundManager.LoadFromGroup("Attack");
		AudioSource smas = SoundManager.PlayCappedSFX (sfxAtk, "Attack", 1f, 1f, sourcePosition);

		if (smas != null){
			smas.dopplerLevel = 0f;
			smas.minDistance = 6.0f;
			smas.maxDistance = 60.0f;
			smas.rolloffMode = AudioRolloffMode.Logarithmic;		
		}		
	}

	[RPC]
	public void SfxDie (Vector3 sourcePosition)
	{
		AudioClip sfxDeath = SoundManager.LoadFromGroup("Death");		
		Vector3 u = this.transform.position;		
		AudioSource smas = SoundManager.PlayCappedSFX (sfxDeath, "Death", 1f, 1f, u);
		
		if (smas != null){
			smas.dopplerLevel = 0.0f;
			smas.spread = 0.3f;
			smas.minDistance = 3.0f;
			smas.maxDistance = 30.0f;
			smas.rolloffMode =AudioRolloffMode.Custom;
		}	
	}

	public void SfxDie ()
	{
		AudioClip sfxDeath = SoundManager.LoadFromGroup("Death");		
		Vector3 u = this.transform.position;		
		AudioSource smas = SoundManager.PlayCappedSFX (sfxDeath, "Death", 1f, 1f, u);
		
		if (smas != null){
			smas.dopplerLevel = 0.0f;
			smas.spread = 0.3f;
			smas.minDistance = 3.0f;
			smas.maxDistance = 30.0f;
			smas.rolloffMode =AudioRolloffMode.Custom;
		}	
	}

	public void SfxAtk ()
	{
		AudioClip sfxAtk = SoundManager.LoadFromGroup("Attack");		
		Vector3 u = this.transform.position;		
		AudioSource smas = SoundManager.PlayCappedSFX (sfxAtk, "Attack", 1f, 1f, u);
		
		if (smas != null){
			smas.dopplerLevel = 0f;
			smas.minDistance = 6.0f;
			smas.maxDistance = 60.0f;
			smas.rolloffMode = AudioRolloffMode.Logarithmic;		
		}		
	}
	

	
	private IEnumerator Attack ()
	{
		Quaternion rotation = Quaternion.LookRotation(TargetAttack.transform.position - transform.position);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * NavAgent.angularSpeed);
		SfxAtk();
		IStats targetStats = TargetAttack.GetComponent<IStats>();

		if (unitAnimation.Attack)
		{
			if (!gameplayManager.IsSameTeam (team))ControllerAnimation.cullingType = AnimationCullingType.AlwaysAnimate;
			ControllerAnimation.PlayCrossFade (unitAnimation.Attack, WrapMode.Once);
			IsAttacking = true;

			if (unitSkill != UnitSkill.none){	
				if	(CheckNemesis(targetStats))
					skillBonus  = Mathf.FloorToInt((force + bonusForce)*0.25f);
			}
			
			if (PhotonNetwork.offlineMode)	targetStats.ReceiveAttack(force + bonusForce + skillBonus);

			else{
				photonView.RPC ("AttackStat", playerTargetAttack, TargetAttack.name, force + bonusForce + skillBonus);
				photonView.RPC ("SfxAtk", PhotonTargets.All,transform.position); 
				if(!targetStats.firstDamage) targetStats.ShowHealth();
			}
			
			yield return StartCoroutine (ControllerAnimation.WhilePlaying (unitAnimation.Attack));

			if (!gameplayManager.IsSameTeam (team))ControllerAnimation.cullingType = AnimationCullingType.BasedOnRenderers;
			skillBonus = 0; 
			IsAttacking = false;
		}
	}

	public void TargetingEnemy (GameObject enemy)
	{
		if (enemy != null && !PhotonNetwork.offlineMode){
			if (!gameplayManager.IsBotTeam (enemy.GetComponent<IStats>())){
				PhotonPlayer[] pp = (from pps in PhotonNetwork.playerList
							         where (int)pps.customProperties["team"] == enemy.GetComponent<IStats>().team
							         select pps).ToArray ();
				playerTargetAttack = pp[0];
			}
			else
				playerTargetAttack = PhotonNetwork.masterClient;

			followingTarget = true;
		}

		else playerTargetAttack = null;

		TargetAttack = enemy;
	}

	private Unit BinarySearch(Unit[] units, Unit unit, int first, int last)
	{
		if (first > last)
			return null;

		int mid = (first + last) / 2;
		Unit testedUnit = units[mid];

		if (testedUnit.team == unit.team){
			if(Random.value % 2 == 0)
				return BinarySearch(units, unit, first, mid - 1);
			else
				return BinarySearch(units, unit, mid + 1, last);
		}

		float testedUnitX = testedUnit.transform.position.x;
		float unitX 	  = unit.transform.position.x;

		if(unit.distanceView > Mathf.Abs(testedUnitX - unitX)){
			float testedUnitZ = testedUnit.transform.position.z;
			float unitZ 	  = unit.transform.position.z;

			if(unit.distanceView > Mathf.Abs(testedUnitZ - unitZ)){
				return testedUnit;
			}		
		}

		if(testedUnitX > unitX + unit.distanceView){
			return BinarySearch(units, unit, first, mid - 1);
		}
		else{		
			return BinarySearch(units, unit, mid + 1, last);
		}
	}
	#endregion

	#region Unit Check's
	
	public void CheckEnemyIsClose ()
	{		
		Collider[] nearbyUnits = Physics.OverlapSphere (transform.position, distanceView, 1 << LayerMask.NameToLayer ("Unit"));		
		if (nearbyUnits.Length == 0) return;		
		GameObject enemyFound = null;
		IStats cStats = null;		
		for (int i = 0; i != nearbyUnits.Length; i++)
		{
			cStats = nearbyUnits[i].GetComponent<IStats> ();
			if (cStats){
				if (gameplayManager.IsBotTeam (this)){
					if (gameplayManager.IsBotTeam (cStats))
						continue;
					
					if (enemyFound == null){
						if (!cStats.WasRemoved)
							enemyFound = cStats.gameObject;
					}
					else{
						if (!cStats.WasRemoved){
							if (Vector3.Distance (transform.position, cStats.transform.position) <
							    Vector3.Distance (transform.position, enemyFound.transform.position))
								enemyFound = cStats.gameObject;
						}
					}
				}
				else{
					if (gameplayManager.IsNotEnemy (cStats.team, cStats.ally))
						continue;
					
					if (enemyFound == null){
						if (!cStats.WasRemoved)
							enemyFound = nearbyUnits[i].gameObject;
					}
					else{
						if (!cStats.WasRemoved){
							if (Vector3.Distance (transform.position, cStats.transform.position) <
							    Vector3.Distance (transform.position, enemyFound.transform.position))
								enemyFound = cStats.gameObject;						
						}
					}
				}
			}
		}
		
		if (enemyFound != null)	TargetingEnemy (enemyFound);
	}
	
	public void StartCheckEnemy ()
	{
		if (!invokeCheckEnemy){
			InvokeRepeating ("CheckEnemyIsClose", 0.3f, 0.2f);
			invokeCheckEnemy = true;
		}
	}
	public void CancelCheckEnemy ()
	{
		if (invokeCheckEnemy){
			CancelInvoke ("CheckEnemyIsClose");
			invokeCheckEnemy = false;
		}
	}
	public void CallInvokeCheckEnemy(float wait)
	{
		Invoke ("StartCheckEnemy",wait);
	}
	
	public bool InMeleeRange (GameObject target)
	{
		if (target.GetComponent<FactoryBase> () != null){
			return Vector3.Distance(transform.position, target.transform.position) <= (attackRange + target.GetComponent<FactoryBase> ().helperCollider.radius);
		}
		else{
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
		if (distanceToDestination <= NavAgent.stoppingDistance) return true;
		if ((distanceToDestination <= NavAgent.stoppingDistance * 2) && NavAgent.velocity.sqrMagnitude < 0.2f)  return true;
		return false;
	}
	
	public bool MoveComplete ()
	{
		return (Vector3.Distance(transform.position, NavAgent.destination) <= 2.0f);
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
	#endregion

	#region Select
	public override void Select ()
	{
		if(IsDead) return;
		base.Select ();		
		Hashtable ht = new Hashtable();		
		ht["observableHealth"] = this;
		ht["time"] = 0f;		
		if(firstDamage)hudController.CreateSubstanceHealthBar (this, sizeOfHealthBar, MaxHealth, "Health Reference");
		hudController.CreateSelected (transform, sizeOfSelected, gameplayManager.GetColorTeam (team));
		
		if (!gameplayManager.IsSameTeam (this.team))
			return;

		hudController.CreateEnqueuedButtonInInspector ( this.name,
		                                               Unit.UnitGroupQueueName,
		                                               ht,
		                                               this.guiTextureName,
		                                               (hud_ht) =>{
															statsController.DeselectAllStats();
															statsController.SelectStat(this, true);
														});	

	}
	
	public override void Deselect ()
	{		
		base.Deselect ();
		int c = IDOobservers.Count;
		while (--c != -1)
		{
			UnRegisterDeathObserver (IDOobservers[c]);
		}
	}
	#endregion

	#region Visibility
	public override void SetVisible(bool isVisible)
	{
		statsController.ChangeVisibility (this, isVisible);
		model.SetActive(isVisible);
		if (firstDamage){
			if(isVisible)hudController.CreateSubstanceHealthBar (this, sizeOfHealthBar, MaxHealth, "Health Reference");
			else {
					hudController.DestroySelected(transform);
					hudController.DestroyHealthBar(transform);
			}
		}
	}

	public override bool IsVisible
	{
		get 
		{
			return model.activeSelf;
		}
	}
	#endregion

	#region Ondie

	public virtual IEnumerator OnDie ()
	{		
		if (Selected)	Deselect ();
		if (!PhotonNetwork.offlineMode) photonView.RPC ("SfxDie", PhotonTargets.All,transform.position); 
		else SfxDie();
		NavAgent.Stop ();
		NavAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;	
		Collider col = GetComponent<Collider>();
		col.enabled = false;
		IsDead = true;		
		unitState = UnitState.Die;
		if (followedUnit != null) UnFollow();
		int c = IMOobservers.Count;
		while (--c != -1){	UnRegisterMovementObserver (IMOobservers[c]);}
		c = IAOobservers.Count;
		while (--c != -1){	UnRegisterAttackObserver (IAOobservers[c]);	}
		statsController.RemoveStats(this);		
		minimapController.RemoveUnit (this.transform, this.team);
		gameplayManager.DecrementUnit (this.team, this.numberOfUnits);
		NotifyDeath ();		
		c = IDOobservers.Count;
		while (--c != -1){	UnRegisterDeathObserver (IDOobservers[c]);}
		hudController.RemoveEnqueuedButtonInInspector (this.name, Unit.UnitGroupQueueName);		
			
		if (unitAnimation.DieAnimation){
			ControllerAnimation.PlayCrossFade (unitAnimation.DieAnimation, WrapMode.ClampForever, PlayMode.StopAll);
			yield return StartCoroutine (ControllerAnimation.WaitForAnimation (unitAnimation.DieAnimation, 2f));
		}

		if (!PhotonNetwork.offlineMode){
			Model.Battle battle = ConfigurationData.battle;
			
			if (playerUnit){
				PhotonNetwork.Destroy(gameObject);
				Score.AddScorePoints (DataScoreEnum.UnitsLost, 1, battle.IdBattle);				
				Score.AddScorePoints (this.category + DataScoreEnum.XUnitLost, this.totalResourceCost, battle.IdBattle);
			}
			else{
				if(gameplayManager.IsBotTeam (this)) PhotonNetwork.Destroy(gameObject);
				Score.AddScorePoints (DataScoreEnum.UnitsKilled, 1, battle.IdBattle);
				Score.AddScorePoints (this.category + DataScoreEnum.XKilled, this.totalResourceCost, battle.IdBattle);
			}			
		}

		else if (gameplayManager.scoreCounting){
			OfflineScore oScore = ComponentGetter.Get<OfflineScore>();
			if (playerUnit){
				PhotonNetwork.Destroy(gameObject);
				oScore.oPlayers[0].AddScorePlayer (DataScoreEnum.UnitsLost, 1);				
				oScore.oPlayers[0].AddScorePlayer (this.category + DataScoreEnum.XUnitLost, this.totalResourceCost);
				oScore.oPlayers[8].AddScorePlayer (DataScoreEnum.UnitsKilled, 1);
				oScore.oPlayers[8].AddScorePlayer (this.category + DataScoreEnum.XKilled, this.totalResourceCost);
			}
			else{
				if(gameplayManager.IsBotTeam (this)) Destroy(gameObject);
				oScore.oPlayers[8].AddScorePlayer (DataScoreEnum.UnitsLost, 1);				
				oScore.oPlayers[8].AddScorePlayer (this.category + DataScoreEnum.XUnitLost, this.totalResourceCost);
				oScore.oPlayers[0].AddScorePlayer (DataScoreEnum.UnitsKilled, 1);
				oScore.oPlayers[0].AddScorePlayer (this.category + DataScoreEnum.XKilled, this.totalResourceCost);
			}

		}
	}

	#endregion

	#region IMovementObservable implementation

	public void RegisterMovementObserver (IMovementObserver observer)
	{
		IMOobservers.Add (observer);
		isFollowed = true;
	}

	public void UnRegisterMovementObserver (IMovementObserver observer)
	{
		bool success = IMOobservers.Remove (observer);

		if (success)
			observer.OnUnRegisterMovementObserver ();
	}

	public void NotifyMovement ()
	{
		if (transform.position != LastPosition){
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

		if(followedUnit.NavAgent != null){
			float minDistanceBetweenFollowedUnit = (followedUnit.GetAgentRadius + this.GetAgentRadius)*3f;			
			Vector3 forwardVec = (this.transform.position.normalized - (followedUnit.transform.position.normalized))* minDistanceBetweenFollowedUnit;
			newPosition = newPosition + forwardVec;
			if (Vector3.Distance (this.transform.position, newPosition) < minDistanceBetweenFollowedUnit)
				return;
		}
		else UnFollow();

		Move (newPosition);
	}

	public void OnUnRegisterMovementObserver ()
	{
		if (IMOobservers.Count == 0) isFollowed = false;
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
			observer.OnUnRegisterAttackObserver ();
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

	public void WarpToPos(Vector3 warpPos)
	{
		NavAgent.Warp(warpPos);
	}


	#endregion

	#region RPC's
	[RPC]
	public virtual void AttackStat (string name, int force)
	{
		IStats stat = statsController.FindMyStat (name);
		if (stat != null && unitState != UnitState.Die){
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
	#endregion
}
