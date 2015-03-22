using UnityEngine;
using System.Collections.Generic;
using Visiorama;

public class ShowScore : MonoBehaviour
{
	public class ScorePlayer
	{
		public int GoldCollectedPoints { get; private set; }
		public int ManaCollectedPoints { get; private set; }
		public int ResourcesSpentPoints { get; private set; }
		public int UnitsCreatedPoints { get; private set; }
		public int UnitsLostPoints { get; private set; }
		public int UnitsDestroyedPoints { get; private set; }
		public int StructureCreatedPoints { get; private set; }
		public int StructureLostPoints { get; private set; }
		public int StructureDestroyedPoints { get; private set; }
		public int VictoryPoints { get; private set; }
		public int TotalTimeElapsed { get; private set; }

		public int TotalScore
		{ 
			get 
			{
				return GoldCollectedPoints/100 + ManaCollectedPoints/100 + UnitsCreatedPoints + UnitsDestroyedPoints +StructureCreatedPoints +StructureDestroyedPoints;
			} 
		}
		
		public ScorePlayer ()
		{
			GoldCollectedPoints      = 0;
			ManaCollectedPoints      = 0;
			ResourcesSpentPoints     = 0;
			UnitsCreatedPoints     	 = 0;
			UnitsLostPoints 		 = 0;
			UnitsDestroyedPoints     = 0;
			StructureCreatedPoints   = 0;
			StructureLostPoints 	 = 0;
			StructureDestroyedPoints = 0;
			VictoryPoints            = 0;
			TotalTimeElapsed         = 0;
		}
		
		public void AddScorePlayer (string scoreName, int points)
		{
			if (scoreName.Equals (DataScoreEnum.ResourcesGathered))			GoldCollectedPoints += points;
			else if (scoreName.Equals (DataScoreEnum.UnitsCreated))			UnitsCreatedPoints += points;
			else if (scoreName.Equals (DataScoreEnum.UnitsKilled))			UnitsDestroyedPoints += points;
			else if (scoreName.Equals (DataScoreEnum.UnitsLost))			UnitsLostPoints += points;
			else if (scoreName.Equals (DataScoreEnum.BuildingsCreated))		StructureCreatedPoints += points;
			else if (scoreName.Equals (DataScoreEnum.DestroyedBuildings))	StructureDestroyedPoints += points;
			else if (scoreName.Equals (DataScoreEnum.BuildingsLost)) 		StructureLostPoints += points;
			else if (scoreName.Equals (DataScoreEnum.Victory))				VictoryPoints += points;
			else if (scoreName.Equals (DataScoreEnum.Defeat))				VictoryPoints -= points;
			else if (scoreName.Equals (DataScoreEnum.TotalTimeElapsed))		TotalTimeElapsed = points;
		}		
	}
	
	public Transform scoreMenuObject;
	public GameObject scorePlayerPrefab;
	public Login login;
	public InternalMainMenu imm;
	public UILabel modeLabel;
	public UILabel mapLabel;
	public UILabel timeLabel;
	private float playerTime;
	public string showGameTime 
	{	get
		{  
			int minutes = Mathf.FloorToInt(playerTime / 60F);
			int seconds = Mathf.FloorToInt(playerTime - minutes * 60);
			string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
			return niceTime;
		}
	}
		
	public float startLabelPoisition = 0f;
	public float diferrenceBetweenLabels = 0f;
	private int battleTotalGold = 0;
	private int battleTotalMana = 0;
	private int battleTotalSpent = 0;
	private int battleTotalUnitsCreated = 0;
	private int battleTotalUnitsDestroyed = 0;
	private int battleTotalStructuresBuild = 0;
	private int battleTotalStructuresDestroyed = 0;
		
	// Use this for initialization
	public void Init ()
	{
		LoginIndex index = login.GetComponentInChildren<LoginIndex> ();
		index.SetActive (false);		
		ActiveScoreMenu (true);		
		Dictionary<int, ScorePlayer> players = new Dictionary<int, ScorePlayer>();

		BattleTotals(battleTotalGold, battleTotalMana, battleTotalSpent, battleTotalUnitsCreated, battleTotalUnitsDestroyed, battleTotalStructuresBuild, battleTotalStructuresDestroyed);
						
		Score.LoadBattleScore
		(
			(dicScore) =>
			{
				for (int i = 0; i != dicScore.Count; i++)
				{
					if (!players.ContainsKey (dicScore[i].IdPlayer))	players.Add (dicScore[i].IdPlayer, new ScorePlayer ());
					
					players[dicScore[i].IdPlayer].AddScorePlayer (dicScore[i].SzScoreName, dicScore[i].NrPoints);
				}
				
				float positionYInitial = startLabelPoisition;
				foreach (KeyValuePair<int, ScorePlayer> sp in players)
				{
					GameObject scorePlayerObject = NGUITools.AddChild (scoreMenuObject.gameObject, scorePlayerPrefab);
					scorePlayerObject.transform.localPosition = Vector3.up * positionYInitial;
					ScoreRow sr = scorePlayerObject.GetComponent<ScoreRow>();					
					SetPlayerRank(sp.Key, sr);

					playerTime = (float)sp.Value.TotalTimeElapsed;     //TODO timer do proprio player
					timeLabel.text = showGameTime;

					sr.playerScoreModifier.text = "+" + sp.Value.TotalScore.ToString();
										
					if (sp.Value.VictoryPoints > 0)  sr.gameResult.text = "Victory";
					else sr.gameResult.text = "Defeat";

					sr.ressourceGold.GetComponentInChildren<UILabel>().text 		 = sp.Value.GoldCollectedPoints.ToString();
					sr.ressourceMana.GetComponentInChildren<UILabel>().text  		= sp.Value.ManaCollectedPoints.ToString();
					sr.ressourceSpent.GetComponentInChildren<UILabel>().text 		= sp.Value.ResourcesSpentPoints.ToString();
					sr.unitsBuild.GetComponentInChildren<UILabel>().text 	 		= sp.Value.UnitsCreatedPoints.ToString ();
					sr.unitsLost.GetComponentInChildren<UILabel>().text		 		= sp.Value.UnitsLostPoints.ToString ();
					sr.unitsDestroyed.GetComponentInChildren<UILabel>().text 		= sp.Value.UnitsDestroyedPoints.ToString ();
					sr.StructuresBuild.GetComponentInChildren<UILabel>().text		= sp.Value.StructureCreatedPoints.ToString ();
					sr.StructuresLost.GetComponentInChildren<UILabel>().text 		= sp.Value.StructureLostPoints.ToString ();
					sr.StructuresDestroyed.GetComponentInChildren<UILabel>().text   = sp.Value.StructureDestroyedPoints.ToString ();

					sr.ressourceGold.value 	      = ((float)sp.Value.GoldCollectedPoints / (float)battleTotalGold);
					sr.ressourceMana.value		  = ((float)sp.Value.ManaCollectedPoints / (float)battleTotalMana);
					sr.ressourceSpent.value 	  = ((float)sp.Value.ResourcesSpentPoints / (float)battleTotalSpent);
					sr.StructuresBuild.value  	  = ((float)sp.Value.StructureCreatedPoints / (float)battleTotalStructuresBuild);
					sr.StructuresLost.value 	  = ((float)sp.Value.StructureLostPoints / (float)battleTotalStructuresDestroyed);
					sr.StructuresDestroyed.value  = ((float)sp.Value.StructureDestroyedPoints / (float)battleTotalStructuresDestroyed);
					sr.unitsBuild.value  		  = ((float)sp.Value.UnitsCreatedPoints / (float)battleTotalUnitsCreated);
					sr.unitsLost.value  		  = ((float)sp.Value.UnitsLostPoints / (float)battleTotalUnitsDestroyed);
					sr.unitsDestroyed.value  	  = ((float)sp.Value.UnitsDestroyedPoints / (float)battleTotalUnitsDestroyed);			

					positionYInitial -= diferrenceBetweenLabels;
				}
			}
		);
		
		
		DefaultCallbackButton dcb = scoreMenuObject.FindChild ("Button Main Menu").gameObject.AddComponent<DefaultCallbackButton> ();
		dcb.Init (null,
			(ht_dcb) =>
			{
				ActiveScoreMenu (false);
				imm.Init ();
				
			}
		);
	}

	protected void BattleTotals (int totalGold, int totalMana, int totalSpent, int totalUnitsCreated, int totalUnitsDestroyed, int totalStructuresBuild, int totalStructuresDestroyed)
	{

		Dictionary<int, ScorePlayer> players = new Dictionary<int, ScorePlayer>();

		Score.LoadBattleScore
		(
		(dicScore) =>
		{
			for (int i = 0; i != dicScore.Count; i++)
			{
				if (!players.ContainsKey (dicScore[i].IdPlayer))
				{
					players.Add (dicScore[i].IdPlayer, new ScorePlayer ());				
				}
				
				players[dicScore[i].IdPlayer].AddScorePlayer (dicScore[i].SzScoreName, dicScore[i].NrPoints);

			}

			foreach (KeyValuePair<int, ScorePlayer> sp in players)
			{
				totalGold +=  sp.Value.GoldCollectedPoints;
				totalMana +=  sp.Value.ManaCollectedPoints;
				totalSpent +=  sp.Value.ResourcesSpentPoints;
				totalUnitsCreated +=  sp.Value.UnitsCreatedPoints;
				totalUnitsDestroyed +=  sp.Value.UnitsDestroyedPoints;
				totalStructuresBuild +=  sp.Value.StructureCreatedPoints;
				totalStructuresDestroyed +=  sp.Value.StructureDestroyedPoints;
			}							
		}
		);

	}
	
	protected void ActiveScoreMenu (bool boolean)
	{
		scoreMenuObject.gameObject.SetActive (boolean);
	}

	public void SetPlayerRank(int playerId, ScoreRow scoreRow)
	{
		int i = 0;
		Score.LoadRanking 
			(
				(List<Model.DataScoreRanking> ranking) => 	
				{
				foreach (Model.DataScoreRanking r in ranking)
				{
					i++;
					if (r.IdPlayer == playerId)
					{
						int rankDif = (PlayerPrefs.GetInt("Rank")- i);
						scoreRow.playerName.text  = r.SzName.ToString ();
						scoreRow.playerNewRank.text  = i.ToString();
						scoreRow.rankLadder.text  = rankDif.ToString();

						if (rankDif < 0)
							scoreRow.rankLadderSignal.spriteName  = "Minus";
						if (rankDif > 0)
							scoreRow.rankLadderSignal.spriteName  = "Plus";
						else
							scoreRow.rankLadderSignal.enabled = false;

						PlayerPrefs.SetInt("Rank", i);
						break;													
					}
				}
			}
			);		
	}
}
