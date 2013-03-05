using UnityEngine;
using System.Collections;
using Visiorama.Extension;

public class Constructor : Unit
{
	[System.Serializable]
	public class ConstructorAnimation
	{
		public AnimationClip Extracting;
		public AnimationClip Carrying;
	}

	public enum ConstructorState
	{
		Extracting = 0,
		Carrying = 1,
		None = 2
	}
	
	public int forceToExtract;
	public int numberMaxGetResources;
	public float distanceToExtract = 5f;
	public ConstructorAnimation constructorAnimation;
	
	public ConstructorState constructorState {get; protected set;}
	
	public Resource resource {get; protected set;}
	
	public bool IsExtracting {get; protected set;}
	
	public Resource.Type resourceType {get; protected set;}
	public int currentNumberOfResources {get; protected set;}
	public bool hasResource {get; protected set;}
	
	protected TouchController touchController;
	
	protected FactoryBase mainFactory;
	protected bool MovingToMainFactory;
	
	public override void Init ()
	{
		constructorState = ConstructorState.None;
		
		touchController = Visiorama.ComponentGetter.Get<TouchController>();
		
		base.Init ();
	}
	
	public override void UnitStatus ()
	{
		if (playerUnit)
		{
			switch (constructorState)
			{
			case ConstructorState.Extracting:
				if (!IsExtracting) StartCoroutine (Extract ());
				break;
				
			case ConstructorState.Carrying:
				if (resource != null) resource.SetBuilder (null);
				
				if (!MovingToMainFactory)
				{
					SetResourceInMainBuilding ();
				}
				
				if (constructorAnimation.Carrying)
				{
					ControllerAnimation[constructorAnimation.Carrying.name].normalizedSpeed = unitAnimation.walkSpeed * Mathf.Clamp(pathfind.velocity.sqrMagnitude, 0f, 1f);
					ControllerAnimation.PlayCrossFade (constructorAnimation.Carrying, WrapMode.Loop);
				}
				
				if (Vector3.Distance (transform.position, mainFactory.transform.position) < transform.GetComponent<CapsuleCollider>().radius + mainFactory.GetComponent<CapsuleCollider>().radius)
				{
//					Debug.DrawRay (resource.transform.position, Vector3.up * 5f, Color.red);
//					Debug.Break ();
//					
					if (resource != null) Move (resource.transform.position);
//					gameplayManager.resources.Set (resource.type, currentNumberOfResources);
//					currentNumberOfResources = 0;
					hasResource = false;
					constructorState = ConstructorState.None;
					MovingToMainFactory = false;
				}
				Debug.Log ("currentNumberOfResources: " + currentNumberOfResources);
				break;
				
			case ConstructorState.None:
				base.UnitStatus ();
				
				if (resource != null)
				{
					if (Vector3.Distance (transform.position, resource.transform.position) < distanceToExtract + resource.collider.radius)
					{
						pathfind.Stop ();
						resource.SetBuilder (this);
						constructorState = ConstructorState.Extracting;
					}
				}
				break;
			}
			
			if (touchController.touchType == TouchController.TouchType.Ended)
			{
				if (touchController.idTouch == TouchController.IdTouch.Id1)
				{
					if (touchController.GetFinalRaycastHit.transform.GetComponent<Resource>() != null)
					{
						if (resource == null)
						{
							resource = touchController.GetFinalRaycastHit.transform.GetComponent<Resource>();
						}
					}
					else
					{
						resource = null;
						if (constructorState != ConstructorState.None) constructorState = ConstructorState.None; 
					}
				}
			}
		}
	}
	
	IEnumerator Extract ()
	{
		IsExtracting = true;
		
		ControllerAnimation.PlayCrossFade (constructorAnimation.Extracting, WrapMode.Once);
		yield return StartCoroutine (ControllerAnimation.WhilePlaying (constructorAnimation.Extracting));
		
		if (resource != null) resource.ExtractResource (forceToExtract);
		else constructorState = ConstructorState.None;
		
		IsExtracting = false;
	}
	
	public void GetResource ()
	{
		GetResource (numberMaxGetResources);
	}
	
	public void GetResource (int gotNumberResources)
	{
		currentNumberOfResources = gotNumberResources;
		hasResource = true;
		constructorState = ConstructorState.Carrying;
	}
	
	void SetResourceInMainBuilding ()
	{
		if (mainFactory == null)SetFactory ();
		
		if (mainFactory != null)
		{
			Move (mainFactory.transform.position);
			MovingToMainFactory = true;
		}
	}
	
	void SetFactory ()
	{
		foreach (FactoryBase fb in factoryController.factorys)
		{
			if (fb.GetType ().ToString ().Equals ("MainFactory"))
			{
				if (gameplayManager.IsSameTeam (fb))
				{
					mainFactory = fb;
				}
			}
		}
	}
	
	// GIZMOS
	
	public override void DrawGizmosSelected ()
	{
		base.DrawGizmosSelected ();
		
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere (this.transform.position, distanceToExtract);
	}
}