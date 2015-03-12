using UnityEngine;
using System.Collections;

public class EnemyIA : MonoBehaviour
{

	public int IAClusterNumber;
	private Unit unit;
	public Transform movementTarget;
	private bool initialized = false; 
	public bool isMoving = false; 
	public bool ScoutType;


	void Start()
	{
		initialized = true; 
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
				if (ScoutType)SendMessage ("ReachPoint", IAClusterNumber, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}


