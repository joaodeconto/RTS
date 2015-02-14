using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;
using Visiorama.Extension;

public class CaveFactory : FactoryBase

{
	protected EnemyCluster enemyCluster;


	public override void Init ()
	{
		string factoryName = buttonName;
		base.Init();
		enabled = true;
		enemyCluster      = ComponentGetter.Get<EnemyCluster>();


	}

	public void BuildEnemy(Unit unit, int clusterNumber)
	{
			unit.unitCluster = clusterNumber;	
			lUnitsToCreate.Add (unit);
											
	}


	public override void EnqueueUnitToCreate (Unit unit)            
	{
			lUnitsToCreate.Add (unit);
			Hashtable ht = new Hashtable();
			ht["unit"] = unit;
			ht["name"] = "button-" + Time.time;
			
	}


	public override void InvokeUnit (Unit unit)
	{
		string unitName = "";

		timer = 0;

		lUnitsToCreate.RemoveAt (0);

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
			GameObject u = PhotonNetwork.Instantiate(unit.gameObject.name, transformParticleDamageReference.position, Quaternion.identity, 0);
			newUnit = u.GetComponent<Unit> ();
		}

		newUnit.m_Team = 8;
		newUnit.team = 8;
		newUnit.playerUnit = false;

		newUnit.Init ();
						
		RallyPoint rallypoint = goRallypoint.GetComponent<RallyPoint> ();
		
		if (rallypoint.observedUnit != null)
		{
			newUnit.Follow (rallypoint.observedUnit);
		}
						
		newUnit.Move (goRallypoint.position);
		newUnit.transform.parent = GameObject.Find("GamePlay/" + newUnit.team).transform;
		Debug.Log(newUnit.team + " " + " " + gameplayManager.MyTeam +" "+unit.team);
		enemyCluster.clusterModels[newUnit.unitCluster].clusterUnits.Add(newUnit);
	//	newUnit = newUnit.gameObject.AddComponent("EnemyIA") as Unit;
	}


	void Update ()
	{
		Debug.Log(lUnitsToCreate.Count);
		if (lUnitsToCreate.Count == 0 && lUpgradesToCreate.Count == 0)
		return;
				
		if (lUnitsToCreate.Count >= 1)
		{
			if (unitToCreate == null)
			{
				unitToCreate = lUnitsToCreate[0];
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
					InvokeUnit (unitToCreate);
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
