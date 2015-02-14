using UnityEngine;
using Visiorama;
using System.Collections;
using System.Collections.Generic;

public class EnemyCluster : MonoBehaviour {

	[System.Serializable]
	public class ClusterModel 
	{
		public bool clusterComplete = false;
		public int clusterNumber = 1;
		public Transform defendTarget;
		public Transform exploreTarget;
		public Transform attackTarget;
		public Transform clusterTarget;
		public CaveFactory factory;
		public ClusterBehaviour desiredBehaviour;
		public ClusterBehaviour clusterBehaviour { get; set; }
		public List<Unit> clusterDesiredUnits = new List<Unit>();
		public List<Unit> clusterUnits = new List<Unit>();
		public bool clusterIsAttacking = false;

	}

	private bool startedOffensive;
	public Transform teamNine;
	public Transform teamZero;
	protected GameplayManager gameplayManager;
	protected StatsController statsController;

	public ClusterModel[] clusterModels;
	public enum ClusterBehaviour
	{
		spawning,
		attack,
		defend,
		explore
	}

	public void Init()
	{	 	


		gameplayManager = ComponentGetter.Get<GameplayManager>();
		statsController = ComponentGetter.Get<StatsController>();
		teamNine = GameObject.Find("GamePlay/" + "8").transform;
		teamZero = GameObject.Find("GamePlay/" + "0").transform;
		startedOffensive = false;
		int a = 0;

		foreach (ClusterModel enemyCluster in clusterModels)
		{
			a++;

			enemyCluster.clusterNumber = a;
			ClusterDemmand(enemyCluster);
			Debug.Log ("Init Cluster demmand");
		}
	}

	public void ClusterDemmand (ClusterModel cluster)
	{
		foreach (Unit desiredUnit in cluster.clusterDesiredUnits)
		{
			cluster.factory.BuildEnemy(desiredUnit, cluster.clusterNumber);
		}

	}

	public void AddEnemy(Unit newUnit, int clusterNumber)
	{
		newUnit = gameObject.AddComponent("EnemyIA") as Unit;
		clusterModels[0].clusterUnits.Add(newUnit);
	
	}

	public void SendOffensive(ClusterModel cluster)
	{
		foreach (Unit e in cluster.clusterUnits)
		{
			EnemyIA eIA = e.gameObject.GetComponent<EnemyIA>();
			eIA.offensiveTarget = cluster.attackTarget;
			eIA.EnemyOffensive ();
		}
	}


	void IABehaviour ()
	{
		foreach (ClusterModel enemyCluster in clusterModels)
		{


			if( enemyCluster.clusterComplete)
			{
				enemyCluster.clusterUnits.RemoveAll(item => item == null);
				enemyCluster.clusterBehaviour = enemyCluster.desiredBehaviour;
				int clusterMinimum = enemyCluster.clusterDesiredUnits.Count/8;
				
				if (enemyCluster.clusterUnits.Count <= clusterMinimum)
				{
					enemyCluster.clusterComplete = false;
					enemyCluster.clusterIsAttacking = false;
					ClusterDemmand(enemyCluster);
					enemyCluster.clusterBehaviour = ClusterBehaviour.defend;
				}
			}
			else 
			{
				if (enemyCluster.clusterUnits.Count == enemyCluster.clusterDesiredUnits.Count)
				{
					enemyCluster.clusterComplete = true;
				}
			}
			
			switch (enemyCluster.clusterBehaviour)
			{
				
			case ClusterBehaviour.spawning:
				break;
				
			case ClusterBehaviour.defend:
				if (enemyCluster.clusterTarget != null)
				{
					enemyCluster.clusterTarget = enemyCluster.defendTarget;
				}
				break;
				
			case ClusterBehaviour.explore:
				
				if (enemyCluster.clusterTarget != null)
				{
					enemyCluster.clusterTarget = enemyCluster.exploreTarget;
				}
				
				break;
				
			case ClusterBehaviour.attack:
				
				if (!enemyCluster.clusterIsAttacking && enemyCluster.attackTarget != null)
				{
					SendOffensive(enemyCluster);
					enemyCluster.clusterIsAttacking = true;

				}					
				break;
			}

			Debug.Log (enemyCluster.clusterBehaviour);
		}
				
	}

	// Use this for initialization


	
	void Update ()
	{
		IABehaviour ();
	
	}

}
