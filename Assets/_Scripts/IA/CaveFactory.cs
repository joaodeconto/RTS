using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;
using Visiorama.Extension;

public class CaveFactory : FactoryBase

{
	protected EnemyCluster enemyCluster;
	private List<int> lUnitCluster = new List<int>();
	private int unitToCreateCluster;


	public override void Init ()
	{
		base.Init();
		wasBuilt = true;
		enabled = true;
		enemyCluster      = ComponentGetter.Get<EnemyCluster>();


	}

	public void BuildEnemy(Unit unit, int clusterNumber)
	{

		List<CaveFactory> factories = new List<CaveFactory> ();
		
		foreach (IStats stat in statsController.otherStats)
		{
			if (stat.GetType() == typeof(CaveFactory))
			{

			CaveFactory factory = stat as CaveFactory;
			
			if (factory == null) continue;
			
			factories.Add (factory);
			}
			

		}
		
		int i = 0, factoryChoose = 0, numberToCreate = -1;
		
		foreach (CaveFactory factory in factories)
		{
			if (numberToCreate == -1)
			{
				numberToCreate = factory.lUnitsToCreate.Count;
				factoryChoose = i;
			}
			else if (numberToCreate > factory.lUnitsToCreate.Count)
			{
				numberToCreate = factory.lUnitsToCreate.Count;
				factoryChoose = i;
			}
			i++;
		}
		

			factories[factoryChoose].lUnitsToCreate.Add (unit);
			factories[factoryChoose].lUnitCluster.Add(clusterNumber);

	}


	public void InvokeUnit (Unit unit, int unitCluster)
	{

		timer = 0;

		lUnitsToCreate.RemoveAt (0);
		lUnitCluster.RemoveAt (0);

		string unitName = "";


		foreach(UnitFactory uf in unitsToCreate)
		{
			if(uf.unit == unit)
			{
				unitName = uf.buttonName;
				break;
			}
		}
								
		if(string.IsNullOrEmpty(unitName))
		{
			Debug.LogError("Eh necessario colocar um nome no UnitFactory.Utilizando nome padrao");

		}
	

		if (!hasRallypoint) return;

		Vector3 difference = goRallypoint.position - transform.position;
		Quaternion rotation = Quaternion.LookRotation (difference);
		Vector3 forward = rotation * Vector3.forward;
		
		Vector3 unitSpawnPosition = transform.position + (forward * helperCollider.radius);
		
		// Look At
//		Vector3 difference = goRallypoint.position - transformParticleDamageReference.transform.position;
//		Quaternion rotation = Quaternion.LookRotation (difference);
//		Vector3 forward = rotation * Vector3.forward;
//		
//		Vector3 unitSpawnPosition = transformParticleDamageReference.transform.position + (forward * helperCollider.radius);
//		
		Unit newUnit = null;

		if (PhotonNetwork.offlineMode)
		{

			Unit u = Instantiate (unit, transformParticleDamageReference.position, Quaternion.identity) as Unit;
			newUnit = u;

		}
		else
		{
			int teamInt = this.team;
			unit.SetTeam(teamInt,teamInt);
			GameObject u = PhotonNetwork.Instantiate(unit.gameObject.name, unitSpawnPosition, Quaternion.identity, 0);
			newUnit = u.GetComponent<Unit> ();
			unit.SetTeam(0,0);
		}

		newUnit.SetTeam(8,8);
		newUnit.Init ();
						
		RallyPoint rallypoint = goRallypoint.GetComponent<RallyPoint> ();

		rallypoint.transform.position = transformParticleDamageReference.position;
										
		newUnit.Move (goRallypoint.position);
		newUnit.moveAttack = true;

		newUnit.transform.parent = GameObject.Find("GamePlay/" + newUnit.team).transform;
//		Debug.Log(newUnit.team + " " + " " + gameplayManager.MyTeam +" "+unit.team);
		enemyCluster.clusterModels[unitCluster].clusterUnits.Add(newUnit);
		newUnit = newUnit.gameObject.AddComponent("EnemyIA") as Unit;

	}


	void Update ()
	{
//		Debug.Log(lUnitsToCreate.Count);
		if (lUnitsToCreate.Count == 0 && lUpgradesToCreate.Count == 0)
		return;
				
		if (lUnitsToCreate.Count >= 1)
		{
			if (unitToCreate == null)
			{
				unitToCreate = lUnitsToCreate[0];
				unitToCreateCluster = lUnitCluster[0];
				timeToCreate = lUnitsToCreate[0].timeToSpawn;
				inUpgrade = true;
			
			}
			
		}

		if (inUpgrade)
		{
			if (timer > timeToCreate)
			{
				if (unitToCreate != null) 
				{
					InvokeUnit (unitToCreate, unitToCreateCluster);
					unitToCreate = null;
				}
								
				inUpgrade = false;
				timer = 0;
			}

			else
			{
		
				timer += Time.deltaTime;
							
				
			}
		}
	}

}
