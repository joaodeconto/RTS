using UnityEngine;
using System.Collections;
using System.Collections.Generic;


using Visiorama;

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
			EnemyBuildings,
			EnemyUnits,
			AllEnemies
		}
	
		public bool IsActive;
		public bool objectiveCompleted;
		public string Name;
		public EnumComparingType enumComparingType;
		public string specificStat;
		public int valueToCompare;
		public EnumCondition enumCondition;
		public string objDescription;
		public string objSprite;
	}

	private GameplayManager gm;
	private StatsController sc;
	private EventController em;
	public Challenge[] ChallengesToWin;		 
	public bool Ativar;
	private bool FoiAtivado;
	public GameObject objectiveLog;
	public UITable objSubpanel;
	public GameObject objectiveRow;
	private int nEnemies = 0;
	private int nPopulation = 0;
	private int nTeams = 0;
	private int nMaxPopulation = 0;
	private float nTime = 0;
	private int nBuilds = 0;
	private int eneBuilds = 0;
	private int nCollected = 0;


	public void Init ()
	{
		gm = ComponentGetter.Get<GameplayManager> ();
		em = ComponentGetter.Get<EventController> ();
		sc = ComponentGetter.Get<StatsController>();

		if (Ativar && !FoiAtivado)	ActiveAllChallenges ();	

		InvokeRepeating ("CheckVictory", 10.0f, 2.0f);
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

						if (stats.tag == specificStat && fb.wasBuilt) _nBuilds++;continue;
					}
					nBuilds = _nBuilds;
														
					return CheckValue (enumCondition, nBuilds, valueToCompare);

			case Challenge.EnumComparingType.EnemyBuildings:						

					int _eneBuilds = 0;


					foreach (IStats stats in sc.otherStats)
					{
						if  (stats.tag == specificStat) _eneBuilds++;continue;
					}
					eneBuilds = _eneBuilds;

									
					return CheckValue (enumCondition, eneBuilds, valueToCompare);			

			case Challenge.EnumComparingType.EnemyUnits:
				
					int _nEnemies = 0;
					
					foreach (IStats stats in sc.otherStats)
					{
						if (stats.tag == "Unit" && !gm.IsNotEnemy(stats.team,stats.ally)) _nEnemies++; continue;
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
								
			default: return true;
		}
	}

	public void ActiveAllChallenges ()
	{
		if (!FoiAtivado) 
			foreach (Challenge ch in ChallengesToWin)
		{
			ch.IsActive = true;
			AddToObjectiveLog(ch.Name, ch.objSprite, ch.valueToCompare, ch.objDescription, ch.objectiveCompleted);
		}
		FoiAtivado = true;
	}

	public void CheckVictory ()
	{
		bool success = false;
		int nSuccess = 0;

		foreach (Challenge ch in ChallengesToWin)
		{
			if (!ch.IsActive)
				continue;

			if (ch.objectiveCompleted)
			{
				++nSuccess;
				continue;
			}

			success = CheckCondition (ch.enumComparingType, ch.enumCondition, ch.valueToCompare, ch.Name, ch.specificStat);

			if (success)
			{
				ch.objectiveCompleted = true;
				em.AddEvent("objective completed", ch.Name, ch.objSprite);
				Transform completeTrns = objSubpanel.transform.FindChild (ch.Name).FindChild("ObjectiveCompleted");
				completeTrns.GetComponent<UIToggle>().value = true;								
				++nSuccess;
			}
		}

		if (nSuccess == ChallengesToWin.Length) 	//Tudo feito
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

	public void AddToObjectiveLog (string challengeName, string spriteName = "", int paramValue = 0, string description = "", bool complete = false)
	{

		Transform name = objectiveRow.transform.FindChild ("label (objName)");
		name.GetComponent<UILabel>().text = challengeName;

		Transform desc = objectiveRow.transform.FindChild ("Tween").FindChild ("Label - Description");
		desc.GetComponent<UILabel>().text = description;

		Transform sprite = objectiveRow.transform.FindChild ("Sprite (objSprite)");
		sprite.GetComponent<UISprite>().spriteName = spriteName;

		Transform completed = objectiveRow.transform.FindChild ("ObjectiveCompleted");
		completed.GetComponent<UIToggle>().value = complete;

		GameObject objline = NGUITools.AddChild (objSubpanel.gameObject, objectiveRow);

		Transform completeTrns = objSubpanel.transform.FindChild ("Objective Row(Clone)");
		completeTrns.name = challengeName;

		objSubpanel.repositionNow = true;						
	
	}

}
