using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Visiorama;
using Soomla.Store;
using I2.Loc;

public class VictoryCondition : MonoBehaviour
{	
	[System.Serializable ()]
	public class Challenge
	{
		public enum EnumCondition { LessThan, SameThan, GreaterThan }
		
		public enum EnumComparingType
		{
			Teams,
			CurrentPopulation,
			MaxPopulation,
			Time_,
			Collect,
			Build,
			Train,
			EnemyBuildings,
			EnemyUnits,
			AllEnemies, 
			Protect,
			Scout
		}

		public enum EnumConsequense {win, lose, bonus}	
		public bool IsActive;
		public bool objectiveCompleted;
		public bool objectiveFailed = false;
		public string Name;
		public EnumComparingType enumComparingType;		
		public EnumConsequense enumConsequense = EnumConsequense.win;
		public string specificStat;
		public int valueToCompare;
		public EnumCondition enumCondition;
		public string objDescription;
		public string objSprite;
		public int timeToComplete = 0;
		public string itemIdBonus; 
		public int itemQuantBonus;
		public string bonusSprite = "Achiv_Battle1"; //TODO adicionar bonus sprites
	}

	GameplayManager gm;
	StatsController sc;
	EventController em;
	public Challenge[] ChallengesToWin;		 
	public bool Ativar;
	bool FoiAtivado;
	public GameObject objectiveLog;
	public UITable objSubpanel;
	public GameObject objectiveRow;
	int nEnemies 		= 0;
	int nPopulation 	= 0;
	int nTeams 			= 0;
	int nMaxPopulation  = 0;
	float nTime 		= 0;
	int nBuilds 		= 0;
	int nUnits 			= 0;
	int eneBuilds 		= 0;
	int nCollected 		= 0;
	int chForVictory 	= 0;
	IStats choseenOne 	= null;	
	bool choseenOneFound = false;

	void Start()
	{
		if(PhotonNetwork.offlineMode) Invoke("Init",2f);
		else {
			objectiveLog.SetActive(false);
			enabled = false;
		}
	}

	private void Init ()
	{
		gm = ComponentGetter.Get<GameplayManager> ();
		em = ComponentGetter.Get<EventController> ();
		sc = ComponentGetter.Get<StatsController>();
		if (Ativar && !FoiAtivado)	ActiveAllChallenges ();	
		InvokeRepeating ("CheckVictory", 15.0f, 3.0f);
	}

	bool CheckValue (Challenge.EnumCondition enumCondition, int value, int valueToCompare)
	{
		switch (enumCondition)
		{
			case Challenge.EnumCondition.GreaterThan: return (value > valueToCompare);
			case Challenge.EnumCondition.LessThan: return (value < valueToCompare);
			case Challenge.EnumCondition.SameThan: return (value == valueToCompare);
			default: return true;
		}
	}

	bool CheckCondition (Challenge.EnumComparingType enumComparingType, Challenge.EnumCondition enumCondition, int valueToCompare, string name, string specificStat)
	{
		switch (enumComparingType)
		{
				//Teams condition
			case Challenge.EnumComparingType.Teams:
					nTeams = gm.teams.Length;
					return CheckValue (enumCondition, nTeams, valueToCompare);
				
			case Challenge.EnumComparingType.CurrentPopulation: 				
					nPopulation = gm.numberOfUnits;					
					return CheckValue (enumCondition, nPopulation, valueToCompare);

			case Challenge.EnumComparingType.MaxPopulation: 
					nMaxPopulation = gm.TotalPopulation;					
					return CheckValue (enumCondition, nMaxPopulation, valueToCompare);
		
			case Challenge.EnumComparingType.Time_:									
					nTime = gm.gameTime;
					return CheckValue (enumCondition, (int)nTime, valueToCompare);
	
			case Challenge.EnumComparingType.Collect:
				if (specificStat == "Mana")	nCollected = gm.resources.Mana;				
				else nCollected = gm.resources.Rocks;					
					return CheckValue (enumCondition, nCollected, valueToCompare);

			case Challenge.EnumComparingType.Build:
					int _nBuilds = 0;					
					foreach (IStats stats in sc.myStats)
					{
						FactoryBase fb = stats as FactoryBase;
						if (fb != null){
							if (stats.category == specificStat && fb.wasBuilt) _nBuilds++;
						}
					}
					nBuilds = _nBuilds;														
					return CheckValue (enumCondition, nBuilds, valueToCompare);

			case Challenge.EnumComparingType.Train:
					int _nUnits = 0;					
					foreach (IStats stats in sc.myStats)
					{
						Unit un = stats as Unit;
						if (un != null){
							if (un.category == specificStat) _nUnits++;
						}
					}
					nUnits = _nUnits;
					return CheckValue (enumCondition, nUnits, valueToCompare);

			case Challenge.EnumComparingType.EnemyBuildings:
					int _eneBuilds = 0;
					foreach (IStats stats in sc.otherStats)
					{
						 if (stats.category == specificStat) _eneBuilds++; //TODO removi tags, pode dar bug							
					}
					eneBuilds = _eneBuilds;									
					return CheckValue (enumCondition, eneBuilds, valueToCompare);			

			case Challenge.EnumComparingType.EnemyUnits:				
					int _nEnemies = 0;					
					foreach (IStats stats in sc.otherStats)
					{
						if (stats.tag == "Unit" && !gm.IsNotEnemy(stats.team,stats.ally)) _nEnemies++; 				
						else if (stats.category == specificStat &&
				         !gm.IsNotEnemy(stats.team,stats.ally)) _nEnemies++;
						continue;
					}	
					nEnemies = _nEnemies;					
					return CheckValue (enumCondition, nEnemies, valueToCompare);
				
			case Challenge.EnumComparingType.AllEnemies:				
					int _nEnemyTribe = 0;				
					foreach (IStats stats in sc.otherStats)
					{
						if (!gm.IsNotEnemy(stats.team,stats.ally)) _nEnemyTribe++; continue;
					}
					nEnemies = _nEnemyTribe;					
					return CheckValue (enumCondition, nEnemies, valueToCompare);

			case Challenge.EnumComparingType.Protect:					
					if(choseenOne == null && choseenOneFound) return true;
					else if (!choseenOneFound){						
						foreach (IStats stat in sc.myStats)
						{
							if (stat.category == specificStat){
								choseenOne = stat;
								choseenOneFound = true;
							}
						}
					}
					return false;

			case Challenge.EnumComparingType.Scout:
					int _eneScout = 0;
					foreach (IStats stats in sc.otherStats)
					{
						if (stats.category == specificStat && stats.wasVisible)	_eneScout++;
					}
					return CheckValue (enumCondition, _eneScout, valueToCompare);	

			default: return true;
		}
	}

	public void ActiveAllChallenges ()
	{
		if (!FoiAtivado){
			foreach (Challenge ch in ChallengesToWin)
			{
				ch.IsActive = true;
				if(ch.enumConsequense == Challenge.EnumConsequense.win) chForVictory++;
				else if (ch.enumConsequense == Challenge.EnumConsequense.lose
				         && ch.timeToComplete != 0) chForVictory++;
				AddToObjectiveLog(ch);
			}
			FoiAtivado = true;
		}
	}

	public void InactiveAllChallenges ()
	{
		if (FoiAtivado){ 
			foreach (Challenge ch in ChallengesToWin)
			{
				ch.IsActive = false;
			}
			FoiAtivado = false;
		}
	}

	public void CheckVictory ()
	{
		bool success = false;
		int nSuccess = 0;

		if(sc.myStats.Count < 1){
			gm.Defeat(0,0);
			this.enabled = false;
		}
			
		foreach (Challenge ch in ChallengesToWin)
		{
			if (!ch.IsActive || ch.objectiveFailed)	continue;
			if (ch.objectiveCompleted ){			
				if(ch.enumConsequense == Challenge.EnumConsequense.win)	++nSuccess;
				continue;
			}
			if (ch.timeToComplete!=0 && (ch.timeToComplete +1) < gm.gameTime){
				if(ch.enumConsequense == Challenge.EnumConsequense.bonus){
					GameObject failSprite = objSubpanel.transform.FindChild(ch.Name).transform.FindChild ("fail_sprite").gameObject;
					em.AddEvent("objective Failed", ScriptLocalization.Get("Challenges/" +ch.Name), ch.objSprite);
					failSprite.SetActive(true);
					ch.objectiveFailed = true; 
					continue;
				}
				if(ch.enumConsequense == Challenge.EnumConsequense.lose){
					++nSuccess;
					ChCompleteMission(ch);
					continue;
				}
				else if(ch.enumConsequense == Challenge.EnumConsequense.win || 
				        ch.enumComparingType != Challenge.EnumComparingType.Protect){
					ChCompleteMission(ch);						
					++nSuccess;						
					continue;
				}
			}

			success = CheckCondition (ch.enumComparingType, ch.enumCondition, ch.valueToCompare, ch.Name, ch.specificStat);

			if (success){
				if(ch.enumConsequense == Challenge.EnumConsequense.lose){
					GameObject failSprite = objSubpanel.transform.FindChild(ch.Name).transform.FindChild ("fail_sprite").gameObject;
					failSprite.SetActive(true);
					gm.Defeat(0,0);
					em.AddEvent("objective Failed", ScriptLocalization.Get("Challenges/" +ch.Name), ch.objSprite);
					CancelInvoke("CheckVictory");
					break;
				}

				else{
					if(ch.enumConsequense == Challenge.EnumConsequense.bonus)	
						ChCompleteBonus(ch);
					else{
						ChCompleteMission(ch);
						++nSuccess;
					}
				}
			}
		}
		if (nSuccess == chForVictory) {
			gm.DefeatingEnemyTeamsByObjectives ();
			CancelInvoke("CheckVictory");
		}
	}

	public void AddToObjectiveLog (Challenge ch)
	{
		Transform bg = objectiveRow.transform.FindChild ("objective_bg");

		switch (ch.enumConsequense)		{
		case Challenge.EnumConsequense.bonus: 	bg.GetComponent<UISprite>().color = Color.grey; break;	
		case Challenge.EnumConsequense.lose: 	bg.GetComponent<UISprite>().color = Color.grey; break;
		case Challenge.EnumConsequense.win: 	bg.GetComponent<UISprite>().color = Color.grey; break;
		}
		Transform refTrns;
		string localizedString;
		refTrns = objectiveRow.transform.FindChild ("label (objName)");
		refTrns.GetComponent<UILabel>().text = ScriptLocalization.Get("Challenges/" +ch.Name);

		refTrns = objectiveRow.transform.FindChild ("Tween").FindChild ("Label - Description");
		refTrns.GetComponent<UILabel>().text = ScriptLocalization.Get("Challenges/" +ch.objDescription);

		refTrns = objectiveRow.transform.FindChild ("Sprite (objSprite)");
		refTrns.GetComponent<UISprite>().spriteName = ch.objSprite;

		refTrns = objectiveRow.transform.FindChild ("ObjectiveCompleted");
		refTrns.GetComponent<UIToggle>().value = ch.objectiveCompleted;

		refTrns = objectiveRow.transform.FindChild ("fail_sprite");
		refTrns.gameObject.SetActive(false);

		GameObject objline = NGUITools.AddChild (objSubpanel.gameObject, objectiveRow);

		refTrns = objSubpanel.transform.FindChild ("Objective Row(Clone)");
		refTrns.name = ch.Name;

		objSubpanel.repositionNow = true;						
	
	}

	void ChCompleteMission(Challenge ch)
	{
		Transform completeTrns = objSubpanel.transform.FindChild(ch.Name).FindChild("ObjectiveCompleted");
		completeTrns.GetComponent<UIToggle>().value = true;	
		ch.objectiveCompleted = true;
		em.AddEvent("objective completed", ScriptLocalization.Get("Challenges/" +ch.Name), ch.objSprite);		
		if(ch.itemIdBonus != "")	ChCompleteBonus(ch);
	}
	void ChCompleteBonus(Challenge ch)
	{
		Transform completeTrns = objSubpanel.transform.FindChild (ch.Name).FindChild("ObjectiveCompleted");
		completeTrns.GetComponent<UIToggle>().value = true;	
		ch.objectiveCompleted = true;
		StoreInventory.GiveItem(ch.itemIdBonus, ch.itemQuantBonus);
		string bonusName = StoreInfo.GetItemByItemId(ch.itemIdBonus).Description;
		em.AddEvent("bonus completed", ch.itemQuantBonus.ToString() + " " + bonusName, ch.bonusSprite);
	}
}
