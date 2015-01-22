using UnityEngine;
using System.Collections;

public class EnemyIA : MonoBehaviour
{
	public int enemyCluster;
	public Transform offensiveTarget;
	private Unit unit;
	public bool initiated = false; 


	public void EnemyOffensive ()
	{
		unit = GetComponent<Unit>(); 
		unit.moveAttack = true;
		unit.Move (offensiveTarget.position);
//		unit.Follow(enemyLeader);
	}

}


