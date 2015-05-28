using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Advertisements;
using Visiorama;

public class ShowScore : MonoBehaviour
{
	public Transform scoreMenuObject;
	public GameObject scorePlayerPrefab;
	public Login login;
	public InternalMainMenu imm;
	public UILabel modeLabel;
	public UILabel mapLabel;
	public UILabel timeLabel;
	private float playerTime;
	public string showGameTime 
	{
		get
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
	private int battleTotalUpgradePoints = 0;

	// Use this for initialization
	public void Init ()
	{
		if(ConfigurationData.addPass || ConfigurationData.multiPass) {}
		else Advertisement.Show(null, new ShowOptions{pause = false,resultCallback = result => {}});
	
		LoginIndex index = login.GetComponentInChildren<LoginIndex> ();
		index.SetActive (false);		
		ActiveScoreMenu (true);		
		Dictionary<int, ScorePlayer> players = new Dictionary<int, ScorePlayer>();
		mapLabel.text = VersusScreen.mapLabelString;
		modeLabel.text = VersusScreen.modeLabelString;


		if (VersusScreen.modeLabelString == "Single Player")
		{
			OfflineScore oScore = ComponentGetter.Get<OfflineScore>();
			float positionYInitial = startLabelPoisition;
			foreach (KeyValuePair<int, ScorePlayer> sp in oScore.oPlayers)
			{
				battleTotalGold						+= sp.Value.GoldCollectedPoints;
				battleTotalMana 					+= sp.Value.ManaCollectedPoints;
				battleTotalSpent					+= sp.Value.ResourcesSpentPoints;
				battleTotalUnitsCreated 			+= sp.Value.UnitsCreatedPoints;
				battleTotalUnitsDestroyed 			+= sp.Value.UnitsKillsPoints;
				battleTotalStructuresBuild 			+= sp.Value.StructureCreatedPoints;
				battleTotalStructuresDestroyed 		+= sp.Value.StructureDestroyedPoints;
				battleTotalUpgradePoints 			+= sp.Value.UpgradePoints;
				
				GameObject scorePlayerObject = NGUITools.AddChild (scoreMenuObject.gameObject, scorePlayerPrefab);
				scorePlayerObject.transform.localPosition = Vector3.up * positionYInitial;
				ScoreRow sr = scorePlayerObject.GetComponent<ScoreRow>();					
				
				playerTime = (float)sp.Value.TotalTimeElapsed;
				if(sp.Key == 0) timeLabel.text = showGameTime;
				sr.playerScoreModifier.text = "+" + sp.Value.TotalScore.ToString();								
				if (sp.Value.VictoryPoints > 0)  sr.gameResult.text = "Victory";
				else sr.gameResult.text = "Defeat";
				if (sp.Key == 8) sr.playerName.text = "AI";
				else sr.playerName.text = "Human";
				
				sr.ressourceGold.GetComponentInChildren<UILabel>().text 		= sp.Value.GoldCollectedPoints.ToString();
				sr.ressourceMana.GetComponentInChildren<UILabel>().text  		= sp.Value.ManaCollectedPoints.ToString();
				//					sr.ressourceSpent.GetComponentInChildren<UILabel>().text 		= sp.Value.ResourcesSpentPoints.ToString();
				sr.unitsBuild.GetComponentInChildren<UILabel>().text 	 		= sp.Value.UnitsCreatedPoints.ToString ();
				sr.unitsLost.GetComponentInChildren<UILabel>().text		 		= sp.Value.UnitsLostPoints.ToString ();
				sr.unitsDestroyed.GetComponentInChildren<UILabel>().text 		= sp.Value.UnitsKillsPoints.ToString ();
				sr.StructuresBuild.GetComponentInChildren<UILabel>().text		= sp.Value.StructureCreatedPoints.ToString ();
				sr.StructuresLost.GetComponentInChildren<UILabel>().text 		= sp.Value.StructureLostPoints.ToString ();
				sr.StructuresDestroyed.GetComponentInChildren<UILabel>().text   = sp.Value.StructureDestroyedPoints.ToString ();
				sr.techsResearched.GetComponentInChildren<UILabel>().text       = sp.Value.UpgradePoints.ToString();
				
//					sr.ressourceGold.value 	      = ((float)sp.Value.GoldCollectedPoints 		/ (float)battleTotalGold);
//					sr.ressourceMana.value		  = ((float)sp.Value.ManaCollectedPoints 		/ (float)battleTotalMana);
//					sr.ressourceSpent.value 	  = ((float)sp.Value.ResourcesSpentPoints 		/ (float)battleTotalSpent);
//					sr.StructuresBuild.value  	  = ((float)sp.Value.StructureCreatedPoints 	/ (float)battleTotalStructuresBuild);
//					sr.StructuresLost.value 	  = ((float)sp.Value.StructureLostPoints 		/ (float)battleTotalStructuresDestroyed);
//					sr.StructuresDestroyed.value  = ((float)sp.Value.StructureDestroyedPoints	/ (float)battleTotalStructuresDestroyed);
//					sr.unitsBuild.value  		  = ((float)sp.Value.UnitsCreatedPoints 		/ (float)battleTotalUnitsCreated);
//					sr.unitsLost.value  		  = ((float)sp.Value.UnitsLostPoints 			/ (float)battleTotalUnitsDestroyed);
//					sr.unitsDestroyed.value  	  = ((float)sp.Value.UnitsKillsPoints 			/ (float)battleTotalUnitsDestroyed);	
//					sr.techsResearched.value  	  = ((float)sp.Value.UpgradePoints 				/ (float)battleTotalUpgradePoints);	
				
				positionYInitial -= diferrenceBetweenLabels;
			}
			oScore.DestroyMe();

			if(ConfigurationData.Offline)
			{

				DefaultCallbackButton dcb = scoreMenuObject.FindChild ("Button Main Menu").gameObject.AddComponent<DefaultCallbackButton> ();
				dcb.Init (null,
				          (ht_dcb) =>
				          {
							ActiveScoreMenu (false);
							login.Index();					
						  });
			}
			else
			{
				DefaultCallbackButton dcb = scoreMenuObject.FindChild ("Button Main Menu").gameObject.AddComponent<DefaultCallbackButton> ();
				dcb.Init (null,
				          (ht_dcb) =>
				          {
								ActiveScoreMenu (false);
								imm.Init ();
								
							});
			}
		}

		else
		{				
			int myPlayerId = ConfigurationData.player.IdPlayer;
			Score.LoadBattleScore
			(
				(dicScore) =>
				{
					for (int i = 0; i != dicScore.Count; i++)
					{
						if (!players.ContainsKey (dicScore[i].IdPlayer)) players.Add (dicScore[i].IdPlayer, new ScorePlayer ());
						
						players[dicScore[i].IdPlayer].AddScorePlayer (dicScore[i].SzScoreName, dicScore[i].NrPoints);

					}											
											
					float positionYInitial = startLabelPoisition;
					foreach (KeyValuePair<int, ScorePlayer> sp in players)
					{
						battleTotalGold						+= sp.Value.GoldCollectedPoints;
						battleTotalMana 					+= sp.Value.ManaCollectedPoints;
						battleTotalSpent					+= sp.Value.ResourcesSpentPoints;
						battleTotalUnitsCreated 			+= sp.Value.UnitsCreatedPoints;
						battleTotalUnitsDestroyed 			+= sp.Value.UnitsKillsPoints;
						battleTotalStructuresBuild 			+= sp.Value.StructureCreatedPoints;
						battleTotalStructuresDestroyed 		+= sp.Value.StructureDestroyedPoints;
						battleTotalUpgradePoints 			+= sp.Value.UpgradePoints;

						GameObject scorePlayerObject = NGUITools.AddChild (scoreMenuObject.gameObject, scorePlayerPrefab);
						scorePlayerObject.transform.localPosition = Vector3.up * positionYInitial;
						ScoreRow sr = scorePlayerObject.GetComponent<ScoreRow>();					

						playerTime = (float)sp.Value.TotalTimeElapsed;
						timeLabel.text = showGameTime;
						sr.playerScoreModifier.text = "+" + sp.Value.TotalScore.ToString();

						if (sp.Key == myPlayerId)
						{
							SaveMyTotal(sp.Value);
						}
											
						if (sp.Value.VictoryPoints > 0)  sr.gameResult.text = "Victory";
						else sr.gameResult.text = "Defeat";

						sr.ressourceGold.GetComponentInChildren<UILabel>().text 		= sp.Value.GoldCollectedPoints.ToString();
						sr.ressourceMana.GetComponentInChildren<UILabel>().text  		= sp.Value.ManaCollectedPoints.ToString();
	//					sr.ressourceSpent.GetComponentInChildren<UILabel>().text 		= sp.Value.ResourcesSpentPoints.ToString();
						sr.unitsBuild.GetComponentInChildren<UILabel>().text 	 		= sp.Value.UnitsCreatedPoints.ToString ();
						sr.unitsLost.GetComponentInChildren<UILabel>().text		 		= sp.Value.UnitsLostPoints.ToString ();
						sr.unitsDestroyed.GetComponentInChildren<UILabel>().text 		= sp.Value.UnitsKillsPoints.ToString ();
						sr.StructuresBuild.GetComponentInChildren<UILabel>().text		= sp.Value.StructureCreatedPoints.ToString ();
						sr.StructuresLost.GetComponentInChildren<UILabel>().text 		= sp.Value.StructureLostPoints.ToString ();
						sr.StructuresDestroyed.GetComponentInChildren<UILabel>().text   = sp.Value.StructureDestroyedPoints.ToString ();
						sr.techsResearched.GetComponentInChildren<UILabel>().text       = sp.Value.UpgradePoints.ToString();

	//					sr.ressourceGold.value 	      = ((float)sp.Value.GoldCollectedPoints / (float)battleTotalGold);
	//					sr.ressourceMana.value		  = ((float)sp.Value.ManaCollectedPoints / (float)battleTotalMana);
	//					sr.ressourceSpent.value 	  = ((float)sp.Value.ResourcesSpentPoints / (float)battleTotalSpent);
	//					sr.StructuresBuild.value  	  = ((float)sp.Value.StructureCreatedPoints / (float)battleTotalStructuresBuild);
	//					sr.StructuresLost.value 	  = ((float)sp.Value.StructureLostPoints / (float)battleTotalStructuresDestroyed);
	//					sr.StructuresDestroyed.value  = ((float)sp.Value.StructureDestroyedPoints / (float)battleTotalStructuresDestroyed);
	//					sr.unitsBuild.value  		  = ((float)sp.Value.UnitsCreatedPoints / (float)battleTotalUnitsCreated);
	//					sr.unitsLost.value  		  = ((float)sp.Value.UnitsLostPoints / (float)battleTotalUnitsDestroyed);
	//					sr.unitsDestroyed.value  	  = ((float)sp.Value.UnitsKillsPoints / (float)battleTotalUnitsDestroyed);	
	//					sr.techsResearched.value  	  = ((float)sp.Value.UpgradePoints / (float)battleTotalUpgradePoints);	

						positionYInitial -= diferrenceBetweenLabels;
						SetPlayerRank(sp.Key, sr);
					}
				});

			DefaultCallbackButton dcb = scoreMenuObject.FindChild ("Button Main Menu").gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init (null,
			          (ht_dcb) =>
			          {
							ActiveScoreMenu (false);
							imm.Init ();
							
						});
		}


	}

	protected void BattleTotals (int totalGold, int totalMana, int totalSpent, int totalUnitsCreated, int totalUnitsDestroyed, int totalStructuresBuild, int totalStructuresDestroyed, int totalUpgradePoints)
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
				totalGold					+= sp.Value.GoldCollectedPoints;
				totalMana 					+= sp.Value.ManaCollectedPoints;
				totalSpent					+= sp.Value.ResourcesSpentPoints;
				totalUnitsCreated 			+= sp.Value.UnitsCreatedPoints;
				totalUnitsDestroyed 		+= sp.Value.UnitsKillsPoints;
				totalStructuresBuild 		+= sp.Value.StructureCreatedPoints;
				totalStructuresDestroyed 	+= sp.Value.StructureDestroyedPoints;
				totalUpgradePoints 			+= sp.Value.UpgradePoints;
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
		Score.LoadRanking((List<Model.DataScoreRanking> ranking) => 	
		{
			foreach (Model.DataScoreRanking r in ranking)
			{
				i++;
				if (r.IdPlayer == playerId)
				{
					int rankDif = (PlayerPrefs.GetInt("Rank")- i);
					scoreRow.playerName.text  = r.SzName;
					scoreRow.playerNewRank.text  = i.ToString();
					scoreRow.rankLadder.text  = rankDif.ToString();

//						if (rankDif < 0)
//							scoreRow.rankLadderSignal.spriteName  = "Minus";
//						if (rankDif > 0)
//							scoreRow.rankLadderSignal.spriteName  = "Plus";
//						else
//							scoreRow.rankLadderSignal.enabled = false;

					PlayerPrefs.SetInt("Rank", i);

					foreach (KeyValuePair <string,string> name in VersusScreen.opponentSprite)
					{

						if (r.SzName == name.Key)
						{
							scoreRow.playerAvatar.spriteName = name.Value;
							break;
						}
					}
					break;													
				}
			}
		});		
	}

	private void SaveMyTotal(ScorePlayer sp)
	{
		PlayerBattleDAO pbDAO = ComponentGetter.Get <PlayerBattleDAO> ();
		
		pbDAO.CreatePlayerBattle (ConfigurationData.player, ConfigurationData.battle,
		                          (playerBattle, message) =>
		                          {
										playerBattle.PScore = sp.TotalScore;
			
										pbDAO.UpdatePlayerBattle (playerBattle,
										                          (playerBattle_update, message_update) =>
										                          {
										
										});
									});
	}
}
