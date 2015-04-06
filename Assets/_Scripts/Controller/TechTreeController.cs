using UnityEngine;
using Visiorama;
using System;
using System.Collections;
using System.Collections.Generic;

public class TechTreeController : MonoBehaviour
{
	#region Declares e Inits
	protected StatsController statsController;
	public List<Upgrade> prefabUpgrade = new List<Upgrade>();
	public List<FactoryBase> prefabFactory = new List<FactoryBase>();
	public List<Unit> prefabUnit = new List<Unit>();
	public Dictionary<string, int> techTreeManager;
	public Hashtable attribsHash = new Hashtable();

	public void Init ()
	{
		techTreeManager = new Dictionary<string, int>();
		statsController = ComponentGetter.Get<StatsController>();
		InitCategoryList();
		InitAtribList ();
	}

	public void InitAtribList ()				//Guarda todos os atributos atributos iniciais de todas as classes;
	{
		foreach(Unit u in prefabUnit)
		{
			Hashtable ht = new Hashtable();
			ht["category"] 			= u.category;
			ht["subcategory"] 		= u.subCategory;
			ht["bonusdefense"] 		= u.bonusDefense;
			ht["bonusforce"]		= u.bonusForce;
			ht["bonusspeed"]		= u.bonusSpeed;
			ht["bonussight"]		= u.bonusSight;
			ht["bonusprojectile"]	= u.bonusProjectile;

			attribsHash.Add(u.category,ht);
		}

		foreach(FactoryBase fb in prefabFactory)
		{
			Hashtable ht = new Hashtable();
			ht["category"] 			= fb.category;
			ht["subcategory"] 		= fb.subCategory;
			ht["bonusdefense"] 		= fb.bonusDefense;
			ht["bonussight"]		= fb.bonusSight;
			ht["bonusprojectile"]	= fb.bonusProjectile;
			attribsHash.Add(fb.category,ht);
		}

	}

	public void InitCategoryList()               						//sao adicionadas todas unidades possiveis de serem construidas
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

			if (u is Worker)
			{
				Worker worker = u as Worker;
				worker.InitWorkerTechAvailability(); 						// inicializa techs no worker para zerar o prefab;
				break;
			}
		}
	}
	#endregion

	#region Tech Functions

	public void AttributeModifier(string category, string attribute, int bonusValue)       // adiciona o valor do bonus a categoria indicada
	{
		foreach(Hashtable ht in attribsHash.Values)
		{
			if (ht.ContainsValue(category))
			{
				int oldValue = (int)ht[attribute];
				ht[attribute] = (oldValue + bonusValue);
				break;
			}
		}
	}
	
	public void UpgradeChildBoolOperator(string category, bool isTechChild) 		//adiciona ou subtrai tokens de tech na lista
	{
		Debug.Log (category +"  istechchild? " + isTechChild);

		List<FactoryBase> statsFactories = new List<FactoryBase>();
		
		foreach (IStats stat in statsController.myStats) //muda tech nos stats
		{
			FactoryBase factory = stat as FactoryBase;
			if (stat is FactoryBase)statsFactories.Add(factory);
		}
		foreach (FactoryBase fb in prefabFactory) // muda Tech no prefab
		{
			statsFactories.Add(fb);
		}
		
		foreach (FactoryBase factory in statsFactories) // aplica mudança de tech
		{		
			factory.TechChildBool(category, isTechChild);
		}		
	}
		
	public void TechBoolOperator(string category, bool techAvailality) 		//adiciona ou subtrai tokens de tech na lista
	{
//		Debug.Log ("Called TechBoolOp for " + category );
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
			FactoryBase factory = stat as FactoryBase;
			if (stat is FactoryBase)statsFactories.Add(factory);
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
	#endregion
}
