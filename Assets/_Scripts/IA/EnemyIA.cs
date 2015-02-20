using UnityEngine;
using System.Collections;

public class EnemyIA : MonoBehaviour
{

	public int enemyClusterNumber;
	private Unit unit;
	public Transform movementTarget;
	public bool initiated = false; 
	public bool isMoving = false; 


	void Start()
	{
		initiated = true; 
		unit = gameObject.GetComponent<Unit>(); 
		unit.moveAttack = true;
	}


	public void EnemyMovement ()
	{
		unit.Move (movementTarget.position);
		isMoving = true;
		
	}

	void Update()
	{
		if (movementTarget != null && unit.unitState == Unit.UnitState.Idle)
		{
			EnemyMovement ();
		}
		if (isMoving)
		{
			if (unit.MoveComplete(movementTarget.position))
			{
				isMoving = false;
				movementTarget = null;
			}
		}

	}
}


