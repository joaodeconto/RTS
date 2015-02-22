using UnityEngine;
using Visiorama;
using System.Collections;
using System.Collections.Generic;

public class EnemyCluster : MonoBehaviour {

	[System.Serializable]
	public class ClusterModel 
	{
		public bool clusterComplete = false;
		public int clusterNumber;
		public Transform defendTarget;
		public Transform exploreTarget;
		public Transform attackTarget;
		public Transform clusterTarget;
		public CaveFactory factory;
		public ClusterBehaviour desiredBehaviour;
		public ClusterBehaviour clusterBehaviour { get; set; }
		public List<Unit> clusterDesiredUnits = new List<Unit>();
		public List<Unit> clusterUnits = new List<Unit>();
		public bool clusterIsBusy = false;
		public int bravery = 5; // numero dividido pelo total de unidades ate demandar novas unidades.

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
		teamNine = gameplayManager.teams[8].initialPosition;
		teamZero = gameplayManager.teams[0].initialPosition;
		teamNine.gameObject.SetActive(true);
		InitInicialEnemies ();

		startedOffensive = false;
		Invoke("ActiveClusters",5f);
		InvokeRepeating ("IABehaviour",2,1);


	}

	public void InitInicialEnemies ()
	{
		foreach (Transform trns in teamNine)
		{
			
			if(trns.gameObject.activeSelf == true)
			{
				InitInstantiateEnemy toInit = trns.GetComponent<InitInstantiateEnemy>();
				if (toInit.GetType() == typeof(InitInstantiateEnemy))
				{
					
					toInit.Init();
					Debug.Log("transform  "+ trns.name);
					
				}
			}
			
		}
	}
		
		public void ActiveClusters()
	{
		int a = 0;
		foreach (ClusterModel cluster in clusterModels)
		{

			cluster.clusterNumber = a;
			cluster.clusterTarget = null;
			CheckClusterFactory(cluster);
			a++;
		}
	}
	public void CheckClusterFactory(ClusterModel cluster)
	{
		List<CaveFactory> factories = new List<CaveFactory> ();
		
		foreach (IStats stat in statsController.otherStats)
		{
			if (stat.GetType() == typeof(CaveFactory))
			{
				CaveFactory factory = stat as CaveFactory;
				cluster.factory = factory;
				ClusterDemmand(cluster);
			}			
			
		}
	}

	public void ClusterDemmand (ClusterModel cluster)
	{
		if(cluster.factory == null)
		{
			CheckClusterFactory(cluster);		
		}
		else
		{	
			foreach (Unit desiredUnit in cluster.clusterDesiredUnits)
			{
				cluster.factory.BuildEnemy(desiredUnit, cluster.clusterNumber);
			}
		}
	}

//	public void AddEnemy(Unit newUnit, int clusterNumber)
//	{
//		newUnit = gameObject.AddComponent("EnemyIA") as Unit;
//		clusterModels[0].clusterUnits.Add(newUnit);
//	
//	}

	public void MoveCluster(ClusterModel cluster)
	{
		foreach (Unit e in cluster.clusterUnits)
		{
			EnemyIA eIA = e.gameObject.GetComponent<EnemyIA>();
			eIA.movementTarget = cluster.clusterTarget;
			eIA.EnemyMovement ();
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
				int clusterMinimum = enemyCluster.clusterDesiredUnits.Count/enemyCluster.bravery;
				
				if (enemyCluster.clusterUnits.Count <= clusterMinimum)
				{
					enemyCluster.clusterComplete = false;
					enemyCluster.clusterIsBusy = false;
					ClusterDemmand(enemyCluster);
					enemyCluster.clusterBehaviour = ClusterBehaviour.defend;
				}
			}
			else 
			{
				if (enemyCluster.clusterUnits.Count >= enemyCluster.clusterDesiredUnits.Count)
				{
					enemyCluster.clusterComplete = true;
				}
			}
			
			switch (enemyCluster.clusterBehaviour)
			{
				
			case ClusterBehaviour.spawning:
				break;
				
			case ClusterBehaviour.defend:
				if (!enemyCluster.clusterIsBusy && enemyCluster.clusterTarget != null)
				{
					enemyCluster.clusterIsBusy = true;
					enemyCluster.clusterTarget = enemyCluster.defendTarget;
					MoveCluster(enemyCluster);
				}
				break;
				
			case ClusterBehaviour.explore:
				
				if (!enemyCluster.clusterIsBusy && enemyCluster.clusterTarget != null)
				{
					enemyCluster.clusterIsBusy = true;
					enemyCluster.clusterTarget = enemyCluster.exploreTarget;
					MoveCluster(enemyCluster);
				}
				
				break;
				
			case ClusterBehaviour.attack:
				
				if (!enemyCluster.clusterIsBusy && enemyCluster.attackTarget != null)
				{

					enemyCluster.clusterTarget = enemyCluster.attackTarget;
					enemyCluster.clusterIsBusy = true;
					MoveCluster(enemyCluster);

				}					
				break;
			}

//			Debug.Log (enemyCluster.clusterBehaviour);
		}
				
	}

	// Use this for initialization


	
//	void Update ()
//	{
//		IABehaviour ();
//	
//	}

}
