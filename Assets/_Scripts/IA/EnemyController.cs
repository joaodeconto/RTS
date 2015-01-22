using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;

public class EnemyController : MonoBehaviour
{
	public List<EnemyIA> enemys = new List<EnemyIA>();
	public float offensiveTimer = 30.0f;
	private bool startedOffensive;
	public Transform teamNine;
	public Transform teamZero;
	protected GameplayManager gameplayManager;

	// Use this for initialization
	public void Init () 
	{
		gameplayManager = ComponentGetter.Get<GameplayManager>();
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
				enemys.Add (eIA);
		
			}

		}
	}

	public void SendOffensive()
	{
		foreach (EnemyIA e in enemys)
		{
			e.offensiveTarget = teamZero;
			e.EnemyOffensive ();
		}
	}
	// Update is called once per frame
	
	void Update ()
	{

		if (!startedOffensive && gameplayManager.myTimer>offensiveTimer)
		{

			SendOffensive();
			startedOffensive = true;
			Debug.Log("rolou o update");
		}

	
	}
}
