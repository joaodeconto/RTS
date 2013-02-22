using UnityEngine;
using System.Collections;

public class Unit : Photon.MonoBehaviour
{
	[System.Serializable]
	public class UnitAnimation
	{
		public AnimationClip Idle;
		public AnimationClip Walk;
		public AnimationClip Attack;
		public AnimationClip DieAnimation;
		public AnimationClip[] SpecialAttack;
	}
	
	public enum UnitState
	{
		Idle = 0,
		Walk = 1,
		Attack = 2,
		Die = 3
	}
	
	public int MaxHealth = 20;
	public int Force;
	public int Defense;
	public float distanceView = 15f;
	public float rangeAttack = 5f;
	public float attackDuration = 1f;
	
	public bool playerUnit;
	
	public UnitAnimation animation;
	
	public int Health { get; set; }
	public int AdditionalForce { get; set; }
	
	public bool IsAttacking { get; protected set; }
	public bool IsDead { get; protected set; }

	public Animation ControllerAnimation;
	public int TypeSoundId { get; protected set; }
	public CharacterSound CharSound { get; protected set; }
	
	private bool canHit;
	public bool CanHit { 
		get {
			if (!canHit)
			{
				canHit = (IsDead);
			}
			return canHit;
		}
	}
	
	public int team = 0;
	
	protected GameObject targetAttack;
	protected float attackBuff;
	
	public UnitState unitState { get; set; }
	
	protected bool invokeCheckEnemy;
	
	[System.NonSerializedAttribute]
	public NavMeshAgent pathfind;
	[System.NonSerializedAttribute]
	public Vector3 pathfindTarget;
	
	void Awake ()
	{
//		Init ();
		
		enabled = false;
		Invoke ("Init", 0.1f);
	}
	
	void Update ()
	{
		if (playerUnit)
		{
			switch (unitState)
			{
			
			case UnitState.Idle:
				if (animation.Idle)
					ControllerAnimation.PlayCrossFade (animation.Idle, WrapMode.Loop);
				
				StartCheckEnemy ();
				if (targetAttack != null)
				{
					unitState = UnitState.Walk;
				}
				break;
				
			case UnitState.Walk:
				if (animation.Walk)
				{
					ControllerAnimation[animation.Walk.name].normalizedSpeed = Mathf.Clamp(pathfind.velocity.sqrMagnitude, 0f, 1f);
					ControllerAnimation.PlayCrossFade (animation.Walk, WrapMode.Loop);
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
						Move (targetAttack.transform.position);
					}
					else
					{
						unitState = UnitState.Idle;
					}
				}
				else if (MoveComplete()) unitState = UnitState.Idle;
				break;
				
			case UnitState.Attack:
				
				if (IsAttacking) return;
				
				Stop ();
				
				pathfindTarget = transform.position;
				
				if (targetAttack != null)
				{
					if (IsRangeAttack (targetAttack))
					{
						StartCoroutine(Attack ());
						pathfindTarget = transform.position;
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
	}
	
	void Init ()
	{
		Health = MaxHealth;
		
		CharSound = GetComponent<CharacterSound> ();
		
		if (ControllerAnimation == null) ControllerAnimation = gameObject.animation;
		if (ControllerAnimation == null) ControllerAnimation = GetComponentInChildren<Animation> ();
		
//		if (ControllerAnimation != null)
//		{
//			ControllerAnimation.SetLayer (animation.Idle, 0);
//			ControllerAnimation.SetLayer (animation.Walk, 0);
//			ControllerAnimation.SetLayer (animation.Attack, 0);
//		}
		
		GameController.GetInstance ().GetTroopController ().AddSoldier (this);
		
		pathfind = GetComponent<NavMeshAgent>();
		
		pathfindTarget = transform.position;
		
		if (!PhotonNetwork.offlineMode) playerUnit = photonView.isMine;
		
		if (playerUnit)
		{
			this.gameObject.tag = "Player";
		}
		else
		{
			this.gameObject.tag = "Enemy";
		}
		
		if (!PhotonNetwork.offlineMode)
		{
			team = (int)PhotonNetwork.player.customProperties["team"];
		}
		else
		{
			if (playerUnit)
			{
				team = 0;
			}
			else
			{
				team = 1;
			}
		}
		
		this.gameObject.layer = LayerMask.NameToLayer ("Unit");
		
		if (!enabled) enabled = true; 
	}
	
	public void SyncAnimation ()
	{
		switch (unitState)
		{
		case UnitState.Idle:
			if (animation.Idle)
				ControllerAnimation.PlayCrossFade (animation.Idle, WrapMode.Loop);
			
			break;
		case UnitState.Walk:
			if (animation.Walk)
				ControllerAnimation.PlayCrossFade (animation.Walk, WrapMode.Loop);
			
			break;
		case UnitState.Attack:
			if (animation.Attack)
				ControllerAnimation.PlayCrossFade (animation.Attack, WrapMode.Once);
			
			break;
		}
	}
	
	public void Move (Vector3 destination)
	{
		if (!pathfind.updatePosition) pathfind.updatePosition = true;
		pathfindTarget = destination;
		pathfind.SetDestination (destination);
		
		unitState = UnitState.Walk;
	}
	
	private void Stop ()
	{
		pathfind.Stop ();
	}
	
	private IEnumerator Attack ()
	{
		Quaternion rotation = Quaternion.LookRotation(targetAttack.transform.position - transform.position);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * pathfind.angularSpeed);
		
		if (PhotonNetwork.offlineMode)
		{
			if (targetAttack.GetComponent<Unit>()) targetAttack.GetComponent<Unit>().ReceiveAttack(Force + AdditionalForce);
		}
		else
		{
			if (targetAttack.GetComponent<Unit>())
				photonView.RPC ("AttackUnit", PhotonTargets.AllBuffered, targetAttack.name, Force + AdditionalForce);
		}
		
		if (animation.Attack)
		{
			ControllerAnimation.PlayCrossFade (animation.Attack, WrapMode.Once);
			
			IsAttacking = true;
			yield return StartCoroutine (ControllerAnimation.WhilePlaying (animation.Attack));
			IsAttacking = false;
		}
		else
		{
			GameObject attackObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
			attackObj.transform.position = transform.position + transform.forward;
			Destroy (attackObj, 0.5f);
		}
		
		attackBuff = 0;
	}
	
	[RPC]
	void AttackUnit (string nameUnit, int force)
	{
		GameObject.Find(nameUnit).GetComponent<Unit> ().ReceiveAttack(force);
	}
	
	public void Active ()
	{
//		ChangeLayersRecursively (transform, "Selected");
//		GetComponentInChildren<Projector> ().enabled = true;
//		
//		if (playerUnit) GetComponentInChildren<Projector> ().material.color = new Color(0, 0, 1, 1);	
//		else GetComponentInChildren<Projector> ().material.color = new Color(1, 0, 0, 1);
	}
	
	public void Deactive ()
	{
//		ChangeLayersRecursively (transform, "Default");
//		GetComponentInChildren<Projector> ().enabled = false;
	}
	
	public void ReceiveAttack (int Damage)
	{
		if (IsDead) return;

		int newDamage = Mathf.Max (0, Damage - Defense);

		Health -= newDamage;
		Health = Mathf.Clamp (Health, 0, MaxHealth);

		if (Health == 0)
		{
			SendMessage ("OnDead", SendMessageOptions.DontRequireReceiver);
			StartCoroutine (DieAnimation ());
		}
	}
	
	public bool IsRangeAttack (GameObject soldier)
	{
		return Vector3.Distance(transform.position, soldier.transform.position) <= rangeAttack;
	}
	
	public bool InDistanceView (Vector3 position)
	{
		return Vector3.Distance(transform.position, position) <= distanceView;
	}
	
	public bool MoveComplete (Vector3 destination)
	{
		return Vector3.Distance(transform.position, destination) <= 2;
	}
	
	bool start = false;
	public bool MoveComplete ()
	{
//		if (pathfind.desiredVelocity.sqrMagnitude < 0.001f) start = !start;
//		return pathfind.desiredVelocity.sqrMagnitude < 0.001f || !start;
		return (Vector3.Distance(transform.position, pathfind.destination) <= 2) &&
				pathfind.velocity.sqrMagnitude < 0.1f;
	}
	
	public void TargetingEnemy (GameObject enemy)
	{
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
			InvokeRepeating ("CheckEnemyToClose", 0.3f, 0.3f);
			invokeCheckEnemy = true;
		}
	}
	
	private void CancelCheckEnemy ()
	{
		if (invokeCheckEnemy)
		{
			CancelInvoke ("CheckEnemyToClose");
			invokeCheckEnemy = false;
		}
	}
	
	private void CheckEnemyToClose ()
	{
		/*
		Unit[] soldiers = GameController.GetInstance().GetTroopController().soldiers.ToArray();
		
		Unit nearestUnit = BinarySearch (soldiers, this, 0, soldiers.Length - 1);
		
		if(nearestUnit != null)
		{
			TargetingEnemy (nearestUnit.gameObject);
			unitState = UnitState.Walk;
		}
		*/
		
		Collider[] nearbyUnits = Physics.OverlapSphere (transform.position, distanceView, 1<<LayerMask.NameToLayer ("Unit"));
		
//		if (nearbyUnits.Length == 0) return false;
		if (nearbyUnits.Length == 0) return;
		
		Unit unitSelected = null;
        for (int i = 0; i != nearbyUnits.Length; i++)
		{
			if (nearbyUnits[i].GetComponent<Unit> ().team != team)
			{
				Debug.Log (nearbyUnits[i].GetComponent<Unit> ().team);
				if (unitSelected == null) unitSelected = nearbyUnits[i].GetComponent<Unit> ();
				else
				{
					if (Vector3.Distance (transform.position, nearbyUnits[i].transform.position) <
						Vector3.Distance (transform.position, unitSelected.transform.position))
					{
						unitSelected = nearbyUnits[i].GetComponent<Unit> ();
					}
				}
			}
        }
		
//		if (unitSelected == null) return false;
		if (unitSelected == null) return;
		else
		{
			TargetingEnemy (unitSelected.gameObject);
//			return true;
		}
	}

	private IEnumerator DieAnimation ()
	{
		IsDead = true;

		if (CharSound != null)
		{
			if (CharSound.DeathAudioSource.isPlaying)
			{
				if (CharSound.DeathAudioSource.clip != CharSound.deathSoundClips[TypeSoundId])
				{
					CharSound.DeathAudioSource.Stop ();
					CharSound.DeathAudioSource.clip = CharSound.deathSoundClips[TypeSoundId];
					CharSound.DeathAudioSource.Play ();
				}
			}
			else
			{
				CharSound.DeathAudioSource.clip = CharSound.deathSoundClips[TypeSoundId];
				CharSound.DeathAudioSource.Play ();
			}
		}
		
		GameController.GetInstance ().GetTroopController ().RemoveSoldier (this);
		
		if (animation.DieAnimation)
		{
			ControllerAnimation.PlayCrossFade (animation.DieAnimation, WrapMode.ClampForever, PlayMode.StopAll);
			yield return StartCoroutine (ControllerAnimation.WaitForAnimation (animation.DieAnimation, 2f));
		}
		
		Destroy (gameObject);
	}
	
	// Add nos códigos
	
	void ChangeLayersRecursively (Transform transform, string name)
	{
	    foreach (Transform child in transform)
	    {
	        child.gameObject.layer = LayerMask.NameToLayer(name);
	        ChangeLayersRecursively(child, name);
	    }
	}
	
	// GIZMOS
	
	void OnDrawGizmosSelected ()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere (this.transform.position, distanceView);
		
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (this.transform.position, rangeAttack);
	}
}
