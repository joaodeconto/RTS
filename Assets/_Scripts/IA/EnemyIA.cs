using UnityEngine;
using System.Collections;

public class EnemyIA : MonoBehaviour
{

	public int enemyClusterNumber;
	private Unit unit;
	public Transform offensiveTarget;
	public bool initiated = false; 

	void Start()
	{
		unit = gameObject.GetComponent<Unit>(); 
		unit.moveAttack = true;
	}


	public void EnemyOffensive ()
	{
		
		unit.Move (offensiveTarget.position);
		
	}

	void Update()
	{
		if (offensiveTarget !=null && unit.unitState == Unit.UnitState.Idle)
		{
			EnemyOffensive ();
		}
	}
}


