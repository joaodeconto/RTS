using UnityEngine;
using System.Collections;
using Visiorama.Extension;
using Visiorama;

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

	public UnitAnimation unitAnimation;

	public int Health { get; set; }
	public int AdditionalForce { get; set; }

	public bool IsAttacking { get; protected set; }
	public bool IsDead { get; protected set; }

	public Animation ControllerAnimation;
	public int TypeSoundId { get; protected set; }
	public CharacterSound CharSound { get; protected set; }

	public int Team {get; protected set;}

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

	protected GameObject targetAttack;
	protected float attackBuff;

	public UnitState unitState { get; set; }

	protected bool invokeCheckEnemy;

	[System.NonSerializedAttribute]
	public NavMeshAgent pathfind;
	[System.NonSerializedAttribute]
	public Vector3 pathfindTarget;

	protected GameplayManager gameplayManager;

	protected HUDController hudController;
	protected HealthBar healthBar;

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

		gameplayManager = ComponentGetter.Get<GameplayManager> ();

		hudController = ComponentGetter.Get<HUDController> ();

		ComponentGetter.Get<TroopController> ().AddSoldier (this);

		pathfind = GetComponent<NavMeshAgent>();

		pathfindTarget = transform.position;

		if (!PhotonNetwork.offlineMode) playerUnit = photonView.isMine;

		if (!PhotonNetwork.offlineMode)
		{
			Team = (int)PhotonNetwork.player.customProperties["team"];
		}
		else
		{
			if (playerUnit)
			{
				Team = 0;
			}
			else
			{
				Team = 1;
			}
		}

		this.gameObject.tag = "Unit";
		this.gameObject.layer = LayerMask.NameToLayer ("Unit");

		if (!enabled) enabled = true;
	}

	void Awake ()
	{
//		Init ();

		enabled = false;
		Invoke ("Init", 0.1f);
	}

	void Update ()
	{
//		if (playerUnit)
		if (true)
		{
			switch (unitState)
			{

			case UnitState.Idle:
				if (unitAnimation.Idle)
					ControllerAnimation.PlayCrossFade (unitAnimation.Idle, WrapMode.Loop);

				StartCheckEnemy ();
				if (targetAttack != null)
				{
					unitState = UnitState.Walk;
				}
				break;

			case UnitState.Walk:
				if (unitAnimation.Walk)
				{
					ControllerAnimation[unitAnimation.Walk.name].normalizedSpeed = Mathf.Clamp(pathfind.velocity.sqrMagnitude, 0f, 1f);
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
						Move (targetAttack.transform.position);
					}
//					else
//					{
//						unitState = UnitState.Idle;
//					}
				}
				else if (MoveComplete(pathfindTarget))
				{
					Stop ();
					unitState = UnitState.Idle;
				}
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

	public void SyncAnimation ()
	{
		switch (unitState)
		{
		case UnitState.Idle:
			if (unitAnimation.Idle)
				ControllerAnimation.PlayCrossFade (unitAnimation.Idle, WrapMode.Loop);

			break;
		case UnitState.Walk:
			if (unitAnimation.Walk)
				ControllerAnimation.PlayCrossFade (unitAnimation.Walk, WrapMode.Loop);

			break;
		case UnitState.Attack:
			if (unitAnimation.Attack)
				ControllerAnimation.PlayCrossFade (unitAnimation.Attack, WrapMode.Once);

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

		if (unitAnimation.Attack)
		{
			if (PhotonNetwork.offlineMode)
			{
				if (targetAttack.GetComponent<Unit>()) targetAttack.GetComponent<Unit>().ReceiveAttack(Force + AdditionalForce);
			}
			else
			{
				if (targetAttack.GetComponent<Unit>())
					photonView.RPC ("AttackUnit", PhotonTargets.AllBuffered, targetAttack.name, Force + AdditionalForce);
			}

			ControllerAnimation.PlayCrossFade (unitAnimation.Attack, WrapMode.Once);

			IsAttacking = true;
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
					if (targetAttack.GetComponent<Unit>()) targetAttack.GetComponent<Unit>().ReceiveAttack(Force + AdditionalForce);
				}
				else
				{
					if (targetAttack.GetComponent<Unit>())
						photonView.RPC ("AttackUnit", PhotonTargets.AllBuffered, targetAttack.name, Force + AdditionalForce);
				}

				GameObject attackObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
				attackObj.transform.position = transform.position + transform.forward;
				Destroy (attackObj, 0.5f);

                attackBuff = 0;
            }
		}
	}

	[RPC]
	void AttackUnit (string nameUnit, int force)
	{
		GameObject.Find(nameUnit).GetComponent<Unit> ().ReceiveAttack(force);
	}

	public void Active ()
	{
		healthBar = hudController.CreateHealthBar (transform, MaxHealth, "Health Reference");
		healthBar.SetTarget (this);

		hudController.CreateSelected (transform, pathfind.radius, gameplayManager.GetColorTeam (Team));
	}

	public void Deactive ()
	{
		hudController.DestroySelected (transform);
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

//	bool start = false;
	public bool MoveComplete ()
	{
//		if (pathfind.desiredVelocity.sqrMagnitude < 0.001f) start = !start;
//		return pathfind.desiredVelocity.sqrMagnitude < 0.001f || !start;
//		return (Vector3.Distance(transform.position, pathfind.destination) <= 2) &&
//				pathfind.velocity.sqrMagnitude < 0.1f;
		return Vector3.Distance(transform.position, pathfind.destination) <= 2;
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
		Unit[] soldiers = ComponentGetter.Get<TroopController>().soldiers.ToArray();

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

//		if (unitSelected == null) return false;
		if (unitSelected == null) return;
		else
		{
			TargetingEnemy (unitSelected);
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

		ComponentGetter.Get<TroopController> ().RemoveSoldier (this);

		if (unitAnimation.DieAnimation)
		{
			ControllerAnimation.PlayCrossFade (unitAnimation.DieAnimation, WrapMode.ClampForever, PlayMode.StopAll);
			yield return StartCoroutine (ControllerAnimation.WaitForAnimation (unitAnimation.DieAnimation, 2f));
		}

		Destroy (gameObject);
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
