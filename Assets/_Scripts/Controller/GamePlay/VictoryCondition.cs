using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Visiorama;
using Soomla.Store;

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
	}

	public void Init ()
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

					//Teams que estao jogando
					nTeams = gm.teams.Length;

					return CheckValue (enumCondition, nTeams, valueToCompare);
				
			case Challenge.EnumComparingType.CurrentPopulation: 
				
					//Populaçao atual do jogador
					nPopulation = gm.numberOfUnits;
					
					return CheckValue (enumCondition, nPopulation, valueToCompare);

			case Challenge.EnumComparingType.MaxPopulation: 

					//Populaçao maxima atual do jogador
					nMaxPopulation = gm.TotalPopulation;
					
					return CheckValue (enumCondition, nMaxPopulation, valueToCompare);
		
			case Challenge.EnumComparingType.Time_:
				
					//Tempo de jogo atual
					nTime = gm.gameTime;
					
					return CheckValue (enumCondition, (int)nTime, valueToCompare);
	
			case Challenge.EnumComparingType.Collect:

				if (specificStat == "Mana")
				{		    
					//Quantidade de recursos de ouro coletados
					nCollected = gm.resources.Mana;
				}
				else
				{
					nCollected = gm.resources.Rocks;
				}
					
					return CheckValue (enumCondition, nCollected, valueToCompare);

			case Challenge.EnumComparingType.Build:

					int _nBuilds = 0;
					
					foreach (IStats stats in sc.myStats)
					{
						FactoryBase fb = stats as FactoryBase;
						if (fb != null)
						{
							if (stats.category == specificStat && fb.wasBuilt) _nBuilds++; //TODO removi tags, pode dar bug
						}
					}
					nBuilds = _nBuilds;
														
					return CheckValue (enumCondition, nBuilds, valueToCompare);

			case Challenge.EnumComparingType.Train:

					int _nUnits = 0;
					
					foreach (IStats stats in sc.myStats)
					{
						Unit un = stats as Unit;
						if (un != null)
						{
							if (un.category == specificStat) _nUnits++; //TODO removi tags, pode dar bug
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
						else if (stats.category == specificStat && !gm.IsNotEnemy(stats.team,stats.ally)) _nEnemies++;
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
					
					if(choseenOne == null && choseenOneFound){ return true;}

					else if (!choseenOneFound)
					{						
						foreach (IStats stat in sc.myStats)
						{
							if (stat.category == specificStat)
							{
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
				else if (ch.enumConsequense == Challenge.EnumConsequense.lose && ch.timeToComplete != 0) chForVictory++;
				AddToObjectiveLog(ch);
			}
			FoiAtivado = true;
		}
	}

	public void InactiveAllChallenges ()
	{
		if (FoiAtivado) 
			foreach (Challenge ch in ChallengesToWin)
		{
			ch.IsActive = false;
		}
		FoiAtivado = false;
	}

	public void CheckVictory ()
	{
		bool success = false;
		int nSuccess = 0;

		if(sc.myStats.Count < 1)
		{
			gm.Defeat(0,0);
			this.enabled = false;
		}
			
		foreach (Challenge ch in ChallengesToWin)
		{
			if (!ch.IsActive || ch.objectiveFailed)
				continue;

			if (ch.objectiveCompleted )
			{			
				if(ch.enumConsequense == Challenge.EnumConsequense.win)	++nSuccess;

				continue;
			}

			if (ch.timeToComplete!=0 && (ch.timeToComplete +1) < gm.gameTime)  // Defeat by time
			{
				if(ch.enumConsequense == Challenge.EnumConsequense.bonus)
				{
					GameObject failSprite = objSubpanel.transform.FindChild (ch.Name).transform.FindChild ("fail_sprite").gameObject;
					failSprite.SetActive(true);
					ch.objectiveFailed = true; 

					continue;
				}

				if(ch.enumConsequense == Challenge.EnumConsequense.lose)
				{
					Transform completeTrns = objSubpanel.transform.FindChild (ch.Name).FindChild("ObjectiveCompleted");
					completeTrns.GetComponent<UIToggle>().value = true;	
					ch.objectiveCompleted = true;
					em.AddEvent("objective completed", ch.Name, ch.objSprite);
					++nSuccess;
					Debug.Log("tempo victory");
					
					if(ch.itemIdBonus != "")
					{
						StoreInventory.GiveItem(ch.itemIdBonus, ch.itemQuantBonus);
						//							completeTrns = objectiveLog.transform.FindChild (ch.Name).FindChild("BonusCompleted"); //TODO  criar objeto de bonus no subpanelv
						string bonusName = StoreInfo.GetItemByItemId(ch.itemIdBonus).Description;
						completeTrns.gameObject.SetActive(true);
						em.AddEvent("bonus completed", ch.itemQuantBonus.ToString() + " " + bonusName, ch.bonusSprite);
						
						
					}
				}

				else if(ch.enumConsequense == Challenge.EnumConsequense.win || ch.enumComparingType != Challenge.EnumComparingType.Protect)
				{
						Transform completeTrns = objSubpanel.transform.FindChild (ch.Name).FindChild("ObjectiveCompleted");
						completeTrns.GetComponent<UIToggle>().value = true;	
						ch.objectiveCompleted = true;
						em.AddEvent("objective completed", ch.Name, ch.objSprite);
						++nSuccess;
					Debug.Log("tempo victory");

						if(ch.itemIdBonus != "")
						{
							StoreInventory.GiveItem(ch.itemIdBonus, ch.itemQuantBonus);
//							completeTrns = objectiveLog.transform.FindChild (ch.Name).FindChild("BonusCompleted"); //TODO  criar objeto de bonus no subpanelv
							string bonusName = StoreInfo.GetItemByItemId(ch.itemIdBonus).Description;
							completeTrns.gameObject.SetActive(true);
							em.AddEvent("bonus completed", ch.itemQuantBonus.ToString() + " " + bonusName, ch.bonusSprite);
							

						}
					continue;
				}
			}


			success = CheckCondition (ch.enumComparingType, ch.enumCondition, ch.valueToCompare, ch.Name, ch.specificStat);

			if (success)
			{
				if(ch.enumConsequense == Challenge.EnumConsequense.lose)
				{
					GameObject failSprite = objSubpanel.transform.FindChild (ch.Name).transform.FindChild ("fail_sprite").gameObject;
					failSprite.SetActive(true);
					gm.Defeat(0,0);
					em.AddEvent("objective Failed", ch.Name, ch.objSprite); //TODO  Adicionar evento no event manager
					CancelInvoke("CheckVictory");
					break;
				}

				else
				{
					Transform completeTrns = objSubpanel.transform.FindChild (ch.Name).FindChild("ObjectiveCompleted");
					completeTrns.GetComponent<UIToggle>().value = true;	

					if(ch.enumConsequense == Challenge.EnumConsequense.bonus){
						ch.objectiveCompleted = true;
						StoreInventory.GiveItem(ch.itemIdBonus, ch.itemQuantBonus);
//						completeTrns = objectiveLog.transform.FindChild (ch.Name).FindChild("BonusCompleted"); //TODO  criar objeto de bonus no subpanelv
						string bonusName = StoreInfo.GetItemByItemId(ch.itemIdBonus).Description;
						completeTrns.gameObject.SetActive(true);
						em.AddEvent("bonus completed", ch.itemQuantBonus.ToString() + " " + bonusName, ch.bonusSprite);
					}

					else{
						ch.objectiveCompleted = true;
						em.AddEvent("objective completed", ch.Name, ch.objSprite);
						++nSuccess;
						if(ch.itemIdBonus != ""){
							StoreInventory.GiveItem(ch.itemIdBonus, ch.itemQuantBonus);
//							completeTrns = objectiveLog.transform.FindChild (ch.Name).FindChild("BonusCompleted"); //TODO  criar objeto de bonus no subpanelv
							string bonusName = StoreInfo.GetItemByItemId(ch.itemIdBonus).Description;
							completeTrns.gameObject.SetActive(true);
							em.AddEvent("bonus completed", ch.itemQuantBonus.ToString() + " " + bonusName, ch.bonusSprite);

						}
					}
				}
			}
		}

		if (nSuccess == chForVictory) 	//Tudo feito
		{
			gm.DefeatingEnemyTeamsByObjectives ();
			CancelInvoke("CheckVictory");
		}



		//Score.GetPlayerCurrentBattleScores
		//(
	//		(listScore) => 
	//		{
	//			foreach (Model.DataScore ds in listScore)
	//			{
	//				foreach (Challenge ch in ChallengesToWin)
	//				{
	//					GameAchievesToWin.SetValue (new List<string> () { ds.SzScoreName }, ds.NrPoints);
	//				}
	//			}
	//			
	//			//Verificar se ganhou
	//			cb (GameAchievesToWin.UnlockedAchievements.Count == ChallengesToWin.Length);
	//		}
	///	);
	}

	public void AddToObjectiveLog (Challenge ch)
	{

		Transform bg = objectiveRow.transform.FindChild ("objective_bg");

		switch (ch.enumConsequense)		{
		case Challenge.EnumConsequense.bonus: 	bg.GetComponent<UISprite>().color = Color.blue; break;	
		case Challenge.EnumConsequense.lose: 	bg.GetComponent<UISprite>().color = Color.grey; break;
		case Challenge.EnumConsequense.win: 	bg.GetComponent<UISprite>().color = Color.yellow; break;
		}
		Transform name = objectiveRow.transform.FindChild ("label (objName)");
		name.GetComponent<UILabel>().text = ch.Name;

		Transform desc = objectiveRow.transform.FindChild ("Tween").FindChild ("Label - Description");
		desc.GetComponent<UILabel>().text = ch.objDescription;

		Transform sprite = objectiveRow.transform.FindChild ("Sprite (objSprite)");
		sprite.GetComponent<UISprite>().spriteName = ch.objSprite;

		Transform completed = objectiveRow.transform.FindChild ("ObjectiveCompleted");
		completed.GetComponent<UIToggle>().value = ch.objectiveCompleted;

		GameObject failSprite = objectiveRow.transform.FindChild ("fail_sprite").gameObject;
		failSprite.SetActive(false);

		GameObject objline = NGUITools.AddChild (objSubpanel.gameObject, objectiveRow);

		Transform completeTrns = objSubpanel.transform.FindChild ("Objective Row(Clone)");
		completeTrns.name = ch.Name;

		objSubpanel.repositionNow = true;						
	
	}

}
