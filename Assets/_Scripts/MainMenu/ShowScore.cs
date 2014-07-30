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
		
	//	public int Total { get { return ResourcesPoints + UnitsPoints +	StructurePoints; } }
		
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
		}
		
		public void AddScorePlayer (string scoreName, int points)
		{
			if (scoreName.Equals (DataScoreEnum.ResourcesGathered))
			{
				GoldCollectedPoints += points;
			}
			else if (scoreName.Equals (DataScoreEnum.UnitsCreated))
			{
				UnitsCreatedPoints += points;
			}
			else if (scoreName.Equals (DataScoreEnum.UnitsKilled))
			{
				UnitsDestroyedPoints += points;
			}
			else if (scoreName.Equals (DataScoreEnum.UnitsLost))
			{
				UnitsLostPoints += points;
			}
			else if (scoreName.Equals (DataScoreEnum.BuildingsCreated))
			{
				StructureCreatedPoints += points;
			}
			else if (scoreName.Equals (DataScoreEnum.DestroyedBuildings))
			{
				StructureDestroyedPoints += points;
			}
			else if (scoreName.Equals (DataScoreEnum.BuildingsLost))
			{
				StructureLostPoints += points;
			}
		}
		
//		public string DebugPoints ()
//		{
//			return  "Resources: " + ResourcesPoints.ToString () +
//					"\n - Units: " + UnitsPoints.ToString () +
//					"\n - Structures: " + StructurePoints.ToString ();
//		}
	}
	
	public Transform scoreMenuObject;
	public GameObject scorePlayerPrefab;
	public Login login;
	public InternalMainMenu imm;
	
	public float startLabelPoisition;
	public float diferrenceBetweenLabels;
	private int battleTotalGold;
	private int battleTotalMana;
	private int battleTotalSpent;
	private int battleTotalUnitsCreated;
	private int battleTotalUnitsDestroyed;
	private int battleTotalStructuresBuild;
	private int battleTotalStructuresDestroyed;
		
	// Use this for initialization
	public void Init ()
	{


		LoginIndex index = login.GetComponentInChildren<LoginIndex> ();
		index.SetActive (false);
		
		ActiveScoreMenu (true);

		BattleTotals(battleTotalGold, battleTotalMana, battleTotalSpent, battleTotalUnitsCreated, battleTotalUnitsDestroyed, battleTotalStructuresBuild, battleTotalStructuresDestroyed );

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
				
				float positionYInitial = startLabelPoisition;
				foreach (KeyValuePair<int, ScorePlayer> sp in players)
				{
					GameObject scorePlayerObject = NGUITools.AddChild (scoreMenuObject.gameObject, scorePlayerPrefab);
					scorePlayerObject.transform.localPosition = Vector3.up * positionYInitial;
					
					scorePlayerObject.GetComponent<ScoreRow>().playerName.text 		 = sp.Key.ToString ();
					scorePlayerObject.GetComponent<ScoreRow>().ressourceGold.GetComponentInChildren<UILabel>().text  = sp.Value.GoldCollectedPoints.ToString();
					scorePlayerObject.GetComponent<ScoreRow>().ressourceMana.GetComponentInChildren<UILabel>().text  = sp.Value.ManaCollectedPoints.ToString();
					scorePlayerObject.GetComponent<ScoreRow>().ressourceSpent.GetComponentInChildren<UILabel>().text  = sp.Value.ResourcesSpentPoints.ToString();
					scorePlayerObject.GetComponent<ScoreRow>().unitsBuild.GetComponentInChildren<UILabel>().text  = sp.Value.UnitsCreatedPoints.ToString ();
					scorePlayerObject.GetComponent<ScoreRow>().unitsLost.GetComponentInChildren<UILabel>().text  = sp.Value.UnitsLostPoints.ToString ();
					scorePlayerObject.GetComponent<ScoreRow>().unitsDestroyed.GetComponentInChildren<UILabel>().text  = sp.Value.UnitsDestroyedPoints.ToString ();
					scorePlayerObject.GetComponent<ScoreRow>().StructuresBuild.GetComponentInChildren<UILabel>().text  = sp.Value.StructureCreatedPoints.ToString ();
					scorePlayerObject.GetComponent<ScoreRow>().StructuresLost.GetComponentInChildren<UILabel>().text  = sp.Value.StructureLostPoints.ToString ();
					scorePlayerObject.GetComponent<ScoreRow>().StructuresDestroyed.GetComponentInChildren<UILabel>().text  = sp.Value.StructureDestroyedPoints.ToString ();

					scorePlayerObject.GetComponent<ScoreRow>().ressourceGold.value  = battleTotalGold / sp.Value.GoldCollectedPoints;
//					scorePlayerObject.GetComponent<ScoreRow>().ressourceMana.value  = battleTotalMana / sp.Value.ManaCollectedPoints;
//					scorePlayerObject.GetComponent<ScoreRow>().ressourceSpent.value  = battleTotalSpent / sp.Value.ResourcesSpentPoints;
					scorePlayerObject.GetComponent<ScoreRow>().StructuresBuild.value  = sp.Value.StructureCreatedPoints / battleTotalStructuresBuild;
					scorePlayerObject.GetComponent<ScoreRow>().StructuresLost.value  = sp.Value.StructureLostPoints / battleTotalStructuresDestroyed;
					scorePlayerObject.GetComponent<ScoreRow>().StructuresDestroyed.value  = sp.Value.StructureDestroyedPoints / battleTotalStructuresDestroyed;
					scorePlayerObject.GetComponent<ScoreRow>().unitsBuild.value  = sp.Value.UnitsCreatedPoints / battleTotalUnitsCreated;
					scorePlayerObject.GetComponent<ScoreRow>().unitsLost.value  = sp.Value.UnitsLostPoints / battleTotalUnitsDestroyed;
					scorePlayerObject.GetComponent<ScoreRow>().unitsDestroyed.value  = sp.Value.UnitsDestroyedPoints / battleTotalUnitsDestroyed;
					
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
}
