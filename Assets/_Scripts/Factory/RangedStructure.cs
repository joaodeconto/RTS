using UnityEngine;
using System.Collections;
using Visiorama.Extension;
using Visiorama;

public class RangedStructure : RangeUnit
{
	public override void Init ()
	{
		dummyRangeObject.SetActive(true);
		dummyRotation = dummyRangeObject.transform.localRotation;
		gameplayManager 	= ComponentGetter.Get<GameplayManager> ();
		IStats factoryParent = transform.parent.gameObject.GetComponentInParent<IStats>();
		SetTeam(factoryParent.team, factoryParent.ally);
		playerUnit = factoryParent.playerUnit;
		FactoryBase fb = factoryParent.GetComponent<FactoryBase>();
		fb.upgradesToCreate[1].techAvailable = false;
		fb.upgradesToCreate[3].techAvailable = true;
		fb.RestoreOptionsMenu();
	//	this.gameObject.layer = LayerMask.NameToLayer ("Unit");	
	}
		
	public override void IAStep ()
	{

		if (TargetAttack != null)
		{
			if (InProjectileRange (TargetAttack)) projectileAttacking = true;	
			else TargetAttack = null;
		}
		else
		{

			if (projectileAttacking) projectileAttacking = false;
		//	unitState = UnitState.Idle;
			CheckEnemyIsClose();
		}
		
		if (projectileAttacking)
		{			
			if (TargetAttack.GetComponent<IStats>().WasRemoved)
			{
				dummyRangeObject.SetActive(true);
				TargetingEnemy (null);
				IsAttacking = false;
				projectileAttacking = false;
				unitState = UnitState.Idle;
				Debug.Log(TargetAttack);
				return;
			}
			
			if (IsAttacking)
			{
				dummyRangeObject.transform.localRotation = Quaternion.Euler(0f,-3.34f,0f);		//reajusta direçao da lança
				return;
			}			

			StartCoroutine(Attack ());
		}
		else
		{
			ControllerAnimation.PlayCrossFade (unitAnimation.Idle, WrapMode.Loop);
			DummyReset();
		}
	}

	public IEnumerator Attack ()
	{	
		SfxAtk();
		Quaternion rotation = Quaternion.LookRotation(TargetAttack.transform.position - transform.position);
		transform.rotation =  rotation;
		
		
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

	public override void OnDestroy ()
	{
	}
}