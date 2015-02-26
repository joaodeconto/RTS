﻿using UnityEngine;
using Visiorama;
using System;
using System.Collections;
using System.Collections.Generic;

public class TechTreeController : MonoBehaviour {

	protected StatsController statsController;

	public List<Upgrade> prefabUpgrade = new List<Upgrade>();
	public List<FactoryBase> prefabFactory = new List<FactoryBase>();
	public List<Unit> prefabUnit = new List<Unit>();
	public Dictionary<string, int> techTreeManager;

	public int unitAtkBonus;
	public int unitDefBonus;
	public int buildingDefBonus;

	// Use this for initialization
	public void Init ()
	{
		techTreeManager = new Dictionary<string, int>();
		statsController = ComponentGetter.Get<StatsController>();

		InitialiseCategoryList();
	}

	public void InitialiseCategoryList()               						//sao adicionadas todas unidades possiveis de serem construidas
	{
		foreach(FactoryBase fb in prefabFactory)
		{
			techTreeManager.Add(fb.category,0);
			fb.InitFactoryTechAvailability();								// inicializa techs no factory para zerar o prefab;
		}

		foreach(Upgrade up in prefabUpgrade)
		{
			techTreeManager.Add(up.name,0);
		}

		foreach(Unit u in prefabUnit)
		{
			techTreeManager.Add(u.category,0);

			if (u.GetType() == typeof(Worker))
			{
				Worker worker = u as Worker;
				worker.InitWorkerTechAvailability(); 						// inicializa techs no worker para zerar o prefab;
				break;
			}
		}

		foreach (KeyValuePair<string, int> pair in techTreeManager)
		{
			string entry = pair.Key + " = " + pair.Value;
//			Debug.Log(entry);
		}
	}
		
	public void TechBoolOperator(string category, bool techAvailality) 		//adiciona ou subtrai tokens de tech na lista
	{
		Debug.Log ("Called TechBoolOp for " +category );
		int a = techTreeManager[category];

		if (techAvailality) a++; 
		else a--;

		techTreeManager[category] = a;

		if (a<=0) 
		{
			TechFactoriesBool(category, false);
			TechWorkersBool (category, false);
		}
		if (a == 1)
		{
			TechFactoriesBool(category, true);
			TechWorkersBool (category, true);
		}
//		foreach (KeyValuePair<string, int> pair in techTreeManager)
//		{
//			string entry = pair.Key + " = " + pair.Value;
//			Debug.Log(entry);
//		}
		
	}

	public void TechFactoriesBool (string category, bool techAvailality)    //envia boleana para as factories
	{	
		List<FactoryBase> statsFactories = new List<FactoryBase>();

		foreach (IStats stat in statsController.myStats) //muda tech nos stats
		{
			if (stat.GetType() == typeof(FactoryBase))
			{
				FactoryBase factory = stat as FactoryBase;
				
				statsFactories.Add(factory);
			}
		}
		foreach (FactoryBase fb in prefabFactory) // muda Tech no prefab
		{
				statsFactories.Add(fb);
		}

		foreach (FactoryBase factory in statsFactories) // aplica mudança de tech
		{		
			factory.TechBool(category, techAvailality);
		}


	}

	public void TechWorkersBool (string category, bool isAvailable) 		//envia boleana para todos workers
	{
		List<Worker> statsWorkers = new List<Worker>();
		
		foreach (IStats stat in statsController.myStats) //muda tech nos stats
		{
			if (stat.GetType() == typeof(Worker))
			{
				Worker worker = stat as Worker;
				
				statsWorkers.Add(worker);
			}
		}
		foreach (Unit unit in prefabUnit) // muda Tech no prefab
		{
			if (unit.GetType() == typeof(Worker))
			{
				Worker worker = unit as Worker;
			
				statsWorkers.Add(worker);
			}
		}
		
		foreach (Worker worker in statsWorkers) // aplica mudança de tech
		{
			worker.StructureTechBool(category, isAvailable);
		}
		
	}

	public void GetTechOnStat()
	{

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
