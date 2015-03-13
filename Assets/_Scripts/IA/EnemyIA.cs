using UnityEngine;
using System.Collections;
using Visiorama;

public class EnemyIA : MonoBehaviour
{

	public int IAClusterNumber;
	private Unit unit;
	public Transform movementTarget;
	private bool initialized = false; 
	public bool isMoving = false; 
	public bool ScoutType;
	private float helperColl;
	protected EnemyCluster enemyCluster;


	void Start()
	{
		enemyCluster = ComponentGetter.Get<EnemyCluster>();
		initialized = true; 
		unit = gameObject.GetComponent<Unit>(); 
		if(!ScoutType) unit.moveAttack = true;
		InvokeRepeating("UpdateIATarget",1,1);
	}


	public void EnemyMovement ()
	{
		unit.Move (movementTarget.position);
		isMoving = true;		
	}

	void UpdateIATarget()
	{
		if(!movementTarget) return;

		if (unit.unitState == Unit.UnitState.Idle)
		{
			EnemyMovement ();
		}

		if (isMoving)
		{						 
			if ( unit.MoveComplete(movementTarget.position) || Vector3.Distance(transform.position, movementTarget.position) <= 4)
			{
				isMoving = false;
				movementTarget = null;
				if (ScoutType) enemyCluster.ReachPoint(IAClusterNumber);
			}
		}
	}
}


