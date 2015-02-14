using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;

public class EnemyController : MonoBehaviour
{
	public List<EnemyIA> enemiesDic = new List<EnemyIA>();
	public float offensiveTimer = 30.0f;
	private bool startedOffensive;
	public Transform teamNine;
	public Transform teamZero;
	protected GameplayManager gameplayManager;
	protected EnemyCluster enemyCluster;
	protected CaveFactory caveFactory;

	// Use this for initialization
	public void Init () 
	{
		gameplayManager = ComponentGetter.Get<GameplayManager>();

		caveFactory = ComponentGetter.Get<CaveFactory>();
		teamNine = GameObject.Find("GamePlay/" + "8").transform;
		teamZero = GameObject.Find("GamePlay/" + "0").transform;
		startedOffensive = false;
		CountEnemys();

	}
			
	public void CountEnemys()
	{
		foreach (Transform e in teamNine)
		{
			if (e.gameObject.activeSelf && e.GetComponent<EnemyIA>() != null)
			{
				EnemyIA eIA = e.GetComponent<EnemyIA>();
				enemiesDic.Add (eIA);
		
			}

		}
	}

	// Update is called once per frame
	
	void Update ()
	{

		if (!startedOffensive && gameplayManager.myTimer>offensiveTimer)
		{
			startedOffensive = true;
			Debug.Log("rolou o update");
		}

	
	}
}
