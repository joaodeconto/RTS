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
			EnemyUnits,
			Collect
		}
	
		public bool IsActive;
		public bool objectiveCompleted;
		public string Name;
		public int valueToCompare;
		public EnumCondition enumCondition;
		public EnumComparingType enumComparingType;
	}

	private GameplayManager gm;
	private StatsController sc;

	public Challenge[] ChallengesToWin;

	public bool Ativar;
	public bool FoiAtivado;

	public void Update ()
	{
		if (Ativar && !FoiAtivado)
		{
			FoiAtivado = true;
			ActiveAllChallenges ();
		}
	}

	public void ActiveAllChallenges ()
	{
		foreach (Challenge ch in ChallengesToWin)
		{
			ch.IsActive = true;
		}
	}
		
	public void Start ()
	{
		gm = ComponentGetter.Get<GameplayManager> ();

		InvokeRepeating ("CheckVictory", 1.0f, 1.0f);
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

	bool CheckCondtion (Challenge.EnumComparingType enumComparingType, Challenge.EnumCondition enumCondition, int valueToCompare, string name)
	{
		switch (enumComparingType)
		{
			//Teams condition
			case Challenge.EnumComparingType.Teams:

				//Teams que estao jogando
				int nTeams = gm.teams.Length;

				return CheckValue (enumCondition, nTeams, valueToCompare);

			break;
			
			case Challenge.EnumComparingType.CurrentPopulation: 
			
			//Populaçao atual do jogador
			int nPopulation = gm.numberOfUnits;
			
			return CheckValue (enumCondition, nPopulation, valueToCompare);
			
			break;
			case Challenge.EnumComparingType.MaxPopulation: 

				//Populaçao maxima atual do jogador
				int nMaxPopulation = gm.TotalPopulation;
				
				return CheckValue (enumCondition, nMaxPopulation, valueToCompare);
			
			break;
			case Challenge.EnumComparingType.Time_:
			
				//Tempo de jogo atual
				int nTime = 10000;
				
				return CheckValue (enumCondition, nTime, valueToCompare);
			
			break;
			case Challenge.EnumComparingType.EnemyUnits:
			
				//Unidades inimigas em jogo
				int nEnemies = sc.otherStats.Count;
				
				return CheckValue (enumCondition, nEnemies, valueToCompare);

			break;
			case Challenge.EnumComparingType.Collect:
			
				//Quantidade de recursos de ouro coletados
				int nGoldCOllected = gm.resources.Rocks;
				
				return CheckValue (enumCondition, nGoldCOllected, valueToCompare);
			break;

			default: return true;
		}
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

			success = CheckCondtion (ch.enumComparingType, ch.enumCondition, ch.valueToCompare, ch.Name);

			if (success)
			{
				ch.objectiveCompleted = true;

				//Faz alguma animaçao para que o usuario veja
				Debug.Log ("Challenge " + ch.Name + "completado");
				
				++nSuccess;
			}
		}

		if (nSuccess == ChallengesToWin.Length)
		{
			gm.DefeatingEnemyTeamsByObjectives ();

			//Tudo feito
			Debug.Log ("Feedback de fim de jogo");
			Debug.Break ();
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
}
