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
		public float triggerDemmand = 60f; // ativa demanda de determinado cluster
		public int bravery = 5; // numero dividido pelo total de unidades ate demandar novas unidades.
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
		public bool hasDemanded;
	}

	public enum ClusterBehaviour
	{
		spawning,
		attack,
		defend,
		explore
	}

	public Transform teamNine;
	public Transform teamZero;
	protected GameplayManager gameplayManager;
	protected StatsController statsController;
	public ClusterModel[] clusterModels;


	public void Init()
	{	 	
		gameplayManager = ComponentGetter.Get<GameplayManager>();
		statsController = ComponentGetter.Get<StatsController>();
		teamNine = gameplayManager.teams[8].initialPosition;
		teamZero = gameplayManager.teams[0].initialPosition;
		teamNine.gameObject.SetActive(true);
		InitInicialEnemies ();
		Invoke("InitClusters",5f);
		InvokeRepeating ("IABehaviour",2,1);
	}
		
	public void InitClusters()
	{
		int a = 0;
		foreach (ClusterModel cluster in clusterModels)
		{
			cluster.clusterNumber = a;
			cluster.clusterTarget = null;
			cluster.hasDemanded = false;
			if(cluster.triggerDemmand <= gameplayManager.myTimer)
					CheckClusterFactory(cluster); 
			else
				{
					float minusT = cluster.triggerDemmand - gameplayManager.myTimer +1f;
					Invoke ("CheckClusterTrigger", minusT);
				}
			a++;
		}
	}

	private void CheckClusterTrigger()
	{
		foreach (ClusterModel cluster in clusterModels)
		{
			if(!cluster.hasDemanded && cluster.triggerDemmand <= gameplayManager.myTimer)
			{
				CheckClusterFactory(cluster); 
			}
		}
		
	}

	private void CheckClusterFactory(ClusterModel cluster)
	{				
		foreach (IStats stat in statsController.otherStats)
		{
			if (stat.GetType() == typeof(CaveFactory))
			{
				CaveFactory factory = stat as CaveFactory;
				cluster.factory = factory;
				ClusterDemand(cluster);
			}			
			
		}
	}

	public void ClusterDemand (ClusterModel cluster)
	{
		if (!cluster.hasDemanded)
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

				cluster.hasDemanded = true;
			}
		}
		else Debug.Log(cluster +"  ja demandou");
	}

	void IABehaviour ()
	{
		foreach (ClusterModel cluster in clusterModels)
		{
			if(cluster.clusterComplete)
			{
				cluster.clusterUnits.RemoveAll(item => item == null);
				cluster.clusterBehaviour = cluster.desiredBehaviour;
				int clusterMinimum = cluster.clusterDesiredUnits.Count/cluster.bravery;
				
				if (cluster.clusterUnits.Count <= clusterMinimum)
				{
					cluster.clusterComplete = false;
					cluster.clusterIsBusy = false;
					cluster.hasDemanded = false;
					ClusterDemand(cluster);
					cluster.clusterBehaviour = ClusterBehaviour.defend;
				}
			}
			else 
			{
				if (cluster.clusterUnits.Count >= cluster.clusterDesiredUnits.Count)
				{
					cluster.clusterComplete = true;
				}
			}
			
			switch (cluster.clusterBehaviour)
			{
				
			case ClusterBehaviour.spawning:
				break;
				
			case ClusterBehaviour.defend:
				if (!cluster.clusterIsBusy && cluster.clusterTarget != null)
				{
					cluster.clusterIsBusy = true;
					cluster.clusterTarget = cluster.defendTarget;
					MoveCluster(cluster);
				}
				break;
				
			case ClusterBehaviour.explore:
				
				if (!cluster.clusterIsBusy && cluster.clusterTarget != null)
				{
					cluster.clusterIsBusy = true;
					cluster.clusterTarget = cluster.exploreTarget;
					MoveCluster(cluster);
				}
				
				break;
				
			case ClusterBehaviour.attack:
				
				if (!cluster.clusterIsBusy && cluster.attackTarget != null)
				{

					cluster.clusterTarget = cluster.attackTarget;
					cluster.clusterIsBusy = true;
					MoveCluster(cluster);

				}					
				break;
			}

//			Debug.Log (cluster.clusterBehaviour);
		}				
	}
	
	private void InitInicialEnemies ()            // instancia Enemies ataraves do initInstanciateEnemy
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
	private void MoveCluster(ClusterModel cluster)
	{
		foreach (Unit e in cluster.clusterUnits)
		{
			EnemyIA eIA = e.gameObject.GetComponent<EnemyIA>();
			eIA.movementTarget = cluster.clusterTarget;
			eIA.EnemyMovement ();
		}
	}
	
	// Use this for initialization


	
//	void Update ()
//	{
//		IABehaviour ();
//	
//	}

}
