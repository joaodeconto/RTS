using UnityEngine;
using System.Collections;
using Visiorama.Extension;
using Visiorama;

public class RangedStructure : RangeUnit
{
	public override void Init ()
	{		
		base.Init();
		GameObject factoryParent = transform.parent.gameObject;
		FactoryBase fb = factoryParent.GetComponent<FactoryBase>();	
		fb.upgradesToCreate[1].techAvailable = false;
		fb.upgradesToCreate[3].techAvailable = true;
		fb.RestoreOptionsMenu();
		moveAttack = false;
	//	this.gameObject.layer = LayerMask.NameToLayer ("Unit");	
	}
		
	public override void IAStep ()
	{

		if (TargetAttack != null)
		{
			if (InProjectileRange (TargetAttack)) projectileAttacking = true;	
			else TargetAttack = null;

			
			Debug.Log(TargetAttack);
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

	private IEnumerator Attack ()
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

}