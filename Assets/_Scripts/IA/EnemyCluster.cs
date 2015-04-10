using UnityEngine;
using Visiorama;
using System.Collections;
using System.Collections.Generic;

public class EnemyCluster : MonoBehaviour
{
	#region Serializable e Declares

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

	private Transform teamNine;
	private Transform teamZero;
	private bool gotMainBase = false;
	private bool enemyFactoryExists;
	protected GameplayManager gameplayManager;
	protected StatsController statsController;
	public ClusterModel[] clusterModels;
	private int exploreIndex = 0;
	private Dictionary<int, Transform> exploreTargets = new Dictionary<int, Transform>();	
	public enum ClusterBehaviour
	{
		none,
		spawning,
		attack,
		defend,
		explore
	}
	#endregion

	#region Inits

	public void Init()
	{	 	
		gameplayManager = ComponentGetter.Get<GameplayManager>();
		statsController = ComponentGetter.Get<StatsController>();
		teamNine = gameplayManager.teams[8].initialPosition;
		teamZero = gameplayManager.teams[0].initialPosition;
		teamNine.gameObject.SetActive(true);
		InitExploreTargets();
		InitInicialEnemies ();
		Invoke("InitClusters",3f);
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
			cluster.clusterIsBusy = false;
			cluster.clusterBehaviour = ClusterBehaviour.none;

			if(cluster.triggerDemmand <= gameplayManager.gameTime)
					CheckClusterFactory(cluster); 
			else
				{
				float minusT = cluster.triggerDemmand - gameplayManager.gameTime +1f;
					Invoke ("CheckClusterTrigger", minusT);
				}
			a++;
		}
	}

	private void InitInicialEnemies ()            
	{
		foreach (Transform trns in teamNine)
		{			
			if(trns.gameObject.activeSelf == true)
			{
				InitInstantiateEnemy toInit = trns.GetComponent<InitInstantiateEnemy>();
				if (!toInit)continue;
				if (toInit.GetType() == typeof(InitInstantiateEnemy))
				{					
					toInit.Init();
					Debug.Log("transform  "+ trns.name);					
				}
			}			
		}
	}
	private void InitExploreTargets()
	{
		int i = 0;
		Transform exploreTarget = GameObject.Find("GamePlay/" + "Resources").transform;
		foreach (Transform target in exploreTarget)	
		{
			exploreTargets.Add(i,target);
			i++;
		}
	}
	#endregion

	#region Cluster Methods
	private void CheckClusterTrigger()
	{
		foreach (ClusterModel cluster in clusterModels)
		{
			if(!cluster.hasDemanded && cluster.triggerDemmand <= gameplayManager.gameTime)
			{
				CheckClusterFactory(cluster); 
			}
		}		
	}

	private void CheckClusterFactory(ClusterModel cluster)
	{			
		if (cluster.factory != null)
		{
			ClusterDemand(cluster);
		}
		else
		{
			enemyFactoryExists = true;

			foreach (IStats stat in statsController.otherStats)
			{
				if (stat.GetType() == typeof(CaveFactory))
				{
					CaveFactory factory = stat as CaveFactory;
					cluster.factory = factory;
					ClusterDemand(cluster);
					break;
				}
				if (cluster.factory == null)
					enemyFactoryExists = false;
			}

			if (cluster.factory == null) return;
		}
	}

	public void ClusterDemand (ClusterModel cluster)
	{
		if(!enemyFactoryExists) return;

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
	}

	private void MoveCluster(ClusterModel cluster)
	{
		foreach (Unit e in cluster.clusterUnits)
		{
			EnemyIA eIA = e.gameObject.GetComponent<EnemyIA>();
			eIA.IAClusterNumber = cluster.clusterNumber;
			eIA.movementTarget = cluster.clusterTarget;
			eIA.EnemyMovement ();
			if (cluster.clusterBehaviour == ClusterBehaviour.explore) eIA.ScoutType = true;
			cluster.clusterIsBusy = true;
		}
	}
	#endregion

	#region Behaviour
	void IABehaviour ()
	{
		foreach (ClusterModel cluster in clusterModels)             //chama os grupos
		{
			cluster.clusterUnits.RemoveAll(item => item == null);
			
			if(cluster.clusterComplete)
			{
				cluster.clusterBehaviour = cluster.desiredBehaviour;
				int clusterMinimum = Mathf.FloorToInt(cluster.clusterDesiredUnits.Count/cluster.bravery);
				
				if (cluster.clusterUnits.Count <= clusterMinimum)
				{
					cluster.clusterComplete = false;
					cluster.clusterIsBusy = false;
					cluster.hasDemanded = false;
					cluster.clusterBehaviour = ClusterBehaviour.none;
					ClusterDemand(cluster);

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

			case ClusterBehaviour.none:
				break;
				
			case ClusterBehaviour.spawning:
			case ClusterBehaviour.defend:

				if(cluster.defendTarget == null)
				{
					Transform defendRef = cluster.factory.goRallypoint;
					defendRef.position += Vector3.forward * 4;
					cluster.defendTarget = defendRef;
				}

				else if (!cluster.clusterIsBusy)
				{
					cluster.clusterTarget = cluster.defendTarget;
					MoveCluster(cluster);
				}
				break;
				
			case ClusterBehaviour.explore:

				if (cluster.clusterTarget == null)	UpdateClusterExploreTarget(cluster);	
				else if (!cluster.clusterIsBusy )
				{
					MoveCluster(cluster);
				}
				
				break;
				
			case ClusterBehaviour.attack:

				if (cluster.clusterTarget == null)	UpdateClusterAttackTarget(cluster);
				else if(!cluster.clusterIsBusy )
				{
					MoveCluster(cluster);
				}					
				break;
			}
		}				
	}
	#endregion

	#region Cluster Updates

	private void UpdateClusterAttackTarget(ClusterModel cluster)
	{
		foreach (IStats stats in statsController.myStats)				// Utilizando o mystats como base para facilitar (multiplayer inserir times)
		{
			if(stats.GetType() == typeof(MainFactory) && !gotMainBase)
			{
				cluster.attackTarget = stats.transform;
				gotMainBase = true;
				break;
			}
			string barracks = "Barracks";
			if(stats.category == barracks)
			{
				cluster.attackTarget = stats.transform;
				break;
			}
			if(stats != null)											//Nao sendo nullo, base ou depot pega qualquer stats
			{
				cluster.attackTarget = stats.transform;
			}
		}
		cluster.clusterTarget = cluster.attackTarget;
	}

	private void UpdateClusterExploreTarget(ClusterModel cluster)         //Observa o index e envia para o target nao explorado
	{
		cluster.clusterIsBusy = false;

		if (exploreIndex >= exploreTargets.Count) exploreIndex = 0;        // zera se todos ja foram assignalados

		cluster.exploreTarget = exploreTargets[exploreIndex];
		cluster.clusterTarget = cluster.exploreTarget;
	}

	public void ReachPoint(int clusterNumber)									//Recebe mensagem da IA de que chegou no explore point
	{
		exploreIndex++;
		
		foreach (ClusterModel cluster in clusterModels)
		{
			if(cluster.clusterNumber == clusterNumber)
			{
				UpdateClusterExploreTarget(cluster);
				break;
			}
		}
	}
	#endregion
}
