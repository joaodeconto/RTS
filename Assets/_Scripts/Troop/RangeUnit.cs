using UnityEngine;
using System.Collections;
using Visiorama.Extension;
using Visiorama;

public class RangeUnit : Unit
{
	public GameObject prefabRange;
	public Transform trnsInstantiateLocalPrefab;
	public float highRangeAttack;
	public AnimationClip highRangeAnimation;
	
	public bool InHighRange { get; set; }
	
	public override void Init ()
	{
		base.Init ();
		
		if (trnsInstantiateLocalPrefab == null)
			trnsInstantiateLocalPrefab = transform;
	}
	
	public override void IAStep ()
	{
		if (targetAttack != null)
		{
			if (!InHighRange)
			{
				if (IsHighRangeAttack (targetAttack))
					InHighRange = true;
			}
			else
			{
				if (IsRangeAttack (targetAttack))
					InHighRange = false;
			}
		}
		else
		{
			if (InHighRange)
				InHighRange = false;
		}
		
		if (InHighRange)
		{
			followingTarget = true;

			if (targetAttack.GetComponent<IStats>().IsRemoved)
			{
				TargetingEnemy (null);
				IsAttacking = false;
				return;
			}

			if (IsAttacking) return;
			
			pathfind.Stop ();

			pathfindTarget = transform.position;

			StartCoroutine(Attack ());
		}
		else
		{
			base.IAStep ();
		}
	}
	
	IEnumerator Attack ()
	{
		if (highRangeAnimation != null)
		{
			Quaternion rotation = Quaternion.LookRotation(targetAttack.transform.position - transform.position);
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * pathfind.angularSpeed);
			
			ControllerAnimation.PlayCrossFade (highRangeAnimation, WrapMode.Once);

			IsAttacking = true;
			
			GameObject pRange;
			
			if (!PhotonNetwork.offlineMode)
			{
				 pRange = PhotonNetwork.Instantiate  (prefabRange.name, 
													  trnsInstantiateLocalPrefab.position,
													  trnsInstantiateLocalPrefab.rotation,
													  0, null);
			}
			else
			{
				 pRange = Instantiate (prefabRange, 
							 trnsInstantiateLocalPrefab.position,
							 trnsInstantiateLocalPrefab.rotation) as GameObject;
			}
			
			pRange.GetComponent<RangeObject> ().Init (targetAttack, 5f,
			(ht) => 
			{
				if (!PhotonNetwork.offlineMode)
				{
					photonView.RPC ("AttackStat", playerTargetAttack, targetAttack.name, force + AdditionalForce);
				}
				else
				{
					targetAttack.GetComponent<IStats>().ReceiveAttack(force + AdditionalForce);
				}
			}
			);

			yield return StartCoroutine (ControllerAnimation.WhilePlaying (unitAnimation.Attack));

			IsAttacking = false;
		}
	}
	
	public override void SyncAnimation ()
	{
		if (InHighRange)
		{
			ControllerAnimation.PlayCrossFade (highRangeAnimation, WrapMode.Loop);
		}
		else
		{
			base.SyncAnimation ();
		}
	}
	
	public bool IsHighRangeAttack (GameObject target)
	{
		return Vector3.Distance(transform.position, target.transform.position) <=
							   (highRangeAttack + target.GetComponent<CapsuleCollider>().radius);
	}
		
	public override void Select ()
	{
		base.Select ();
	}
	
	public override void Deselect ()
	{
		base.Deselect ();
	}
	
	public override void DrawGizmosSelected ()
	{
		base.DrawGizmosSelected ();
		
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere (this.transform.position, highRangeAttack);
	}
	
	public override void SetVisible (bool isVisible)
	{
		ComponentGetter
			.Get<StatsController>()
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
}