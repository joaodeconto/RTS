using UnityEngine;
using System.Collections;
using Visiorama.Extension;
using Visiorama;

public class RangeUnit : Unit
{
	#region Declares e Init

	public int projectileAttackForce;
	public float projectileAttackRange;
	public GameObject prefabProjectile;
	public Transform trnsInstantiateLocalPrefab;
	public GameObject dummyRangeObject;
	private Quaternion dummyRotation;
	public AnimationClip projectileAttackAnimation;
	public float projectileAnimationSync;
	public bool projectileAttacking {get; set;}
	public int projectileAnimating 	{get; set;}		
	private bool rangeInitialized = false;

	public override void Init ()
	{
		if(rangeInitialized) return;
		rangeInitialized = true;	
		dummyRangeObject.SetActive(true);
		dummyRotation = dummyRangeObject.transform.localRotation;
		base.Init ();

	}
	#endregion
	
	#region IaStep, Attack e SyncAnimation
	public override void IAStep ()
	{
		if (TargetAttack != null)
		{
			if (InMeleeRange (TargetAttack)) projectileAttacking = false;

			else if (InProjectileRange (TargetAttack)) projectileAttacking = true;
		
		}
		else
		{
			if (projectileAttacking) projectileAttacking = false;
		}

		if (projectileAttacking)
		{

			followingTarget = true;

			if (TargetAttack.GetComponent<IStats>().WasRemoved)
			{
				dummyRangeObject.SetActive(true);
				TargetingEnemy (null);
				IsAttacking = false;
				projectileAttacking = false;
				unitState = UnitState.Idle;
				return;
			}

			if (IsAttacking)
			{
				dummyRangeObject.transform.localRotation = Quaternion.Euler(0f,-3.34f,0f);		//reajusta direçao da lança
				return;
			}
			
			NavAgent.Stop ();
			PathfindTarget = transform.position;
			StartCoroutine(Attack ());
		}
		else
		{
			DummyReset();
			base.IAStep ();
		}
	}

	IEnumerator Attack ()
	{	
		SfxAtk();
		Quaternion rotation = Quaternion.LookRotation(TargetAttack.transform.position - transform.position);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * NavAgent.angularSpeed);


		if(projectileAttacking)
		{
			projectileAnimating++;
			ControllerAnimation.PlayCrossFade (projectileAttackAnimation, WrapMode.Once);
			Invoke ("ProjectileSync", projectileAnimationSync);
			IsAttacking = true;			
			yield return StartCoroutine (ControllerAnimation.WhilePlaying (projectileAttackAnimation));
			projectileAnimating--;
		}

		else
		{		
			ControllerAnimation.PlayCrossFade (unitAnimation.Attack, WrapMode.Once);											
			IsAttacking = true;			
			yield return StartCoroutine (ControllerAnimation.WhilePlaying (unitAnimation.Attack));
		}			

		IsAttacking = false;
	}

	public override void SyncAnimation ()
	{
		if (!IsVisible) return;
		
		Debug.Log ("projectileAnimating: " + projectileAttacking + "  " + projectileAnimating);
		
		if (projectileAnimating == 1)
		{
			ControllerAnimation.PlayCrossFade (projectileAttackAnimation, WrapMode.Once);
		}
		else
		{
			base.SyncAnimation ();
		}
	}
	#endregion
		
	#region Projectile

	public bool InProjectileRange (GameObject target)
	{	
		return Vector3.Distance(transform.position, target.transform.position) <=
							   (projectileAttackRange + target.GetComponent<CapsuleCollider>().radius);
	}

	public void ProjectileSync ()
	{
		dummyRangeObject.SetActive(false);
		GameObject pRange;
		
		if (!PhotonNetwork.offlineMode)
		{
			pRange = PhotonNetwork.Instantiate  (prefabProjectile.name, 
			                                     trnsInstantiateLocalPrefab.position,
			                                     trnsInstantiateLocalPrefab.rotation,
			                                     0, null);
		}
		else
		{
			pRange = Instantiate (prefabProjectile, 
			                      trnsInstantiateLocalPrefab.position,
			                      trnsInstantiateLocalPrefab.rotation) as GameObject;
		}
		
		pRange.GetComponent<RangeObject>().Init (TargetAttack, 2f,
		                                         (ht) => 
		                                         {
			if (TargetAttack != null)
			{
				if (!PhotonNetwork.offlineMode)
				{
					photonView.RPC ("AttackStat", playerTargetAttack, TargetAttack.name, projectileAttackForce + bonusProjectile);
				}
				else
				{
					TargetAttack.GetComponent<IStats>().ReceiveAttack(projectileAttackForce + bonusProjectile);
				}
			}
			
		}
		);
		Invoke ("DummyActive",1);
	}

	private void DummyActive()
	{
		dummyRangeObject.SetActive(true);
	}
	private void DummyReset()
	{
		dummyRangeObject.SetActive(true);
		dummyRangeObject.transform.localRotation = dummyRotation;
	}
	#endregion

	#region RPC's
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
	#endregion
}