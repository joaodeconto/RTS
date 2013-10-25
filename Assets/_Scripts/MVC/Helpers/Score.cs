using UnityEngine;
using System.Collections;

using System.Collections.Generic;

using Visiorama;

public class Score : MonoBehaviour
{
	private Dictionary <string, Model.DataScore> dicDataScore;
	private List <Model.DataScore> dicCurrentBattleScore;
	private Model.Player player;
	private DataScoreDAO dataScoreDAO;
	
	public delegate void A (List <Model.DataScore> dicScore);
	public A b;

	Score Init ()
	{
		PhotonWrapper pw = ComponentGetter.Get <PhotonWrapper> ();
		player = new Model.Player ((string)pw.GetPropertyOnPlayer ("player"));

		dataScoreDAO = ComponentGetter.Get <DataScoreDAO> ();
		dicDataScore = new Dictionary <string, Model.DataScore> ();
		dicCurrentBattleScore = new List<Model.DataScore> ();

		return this;
	}

	void CreateDataScore (string ScoreName, int points, int battleId)
	{
		Model.DataScore ds = new Model.DataScore (player.IdPlayer, ScoreName, points);

		if (battleId != -1) ds.IdBattle = battleId;

		string scoreKey = GetScoreKey (ScoreName, battleId);

		dicDataScore.Add (scoreKey, ds);
	}

	private string GetScoreKey (string ScoreName, int battleId)
	{
		return ScoreName + " - " + battleId;
	}

	public void _AddScorePoints (string ScoreName, int points = 1, int battleId = -1)
	{
		string scoreKey = GetScoreKey (ScoreName, battleId);

		if(!dicDataScore.ContainsKey (scoreKey))
			CreateDataScore (ScoreName, points, battleId);
		else
			dicDataScore[scoreKey].NrPoints += points;
	}

	public void _SubtractScorePoints (string ScoreName, int points, int battleId = -1)
	{
		string scoreKey = GetScoreKey (ScoreName, battleId);

		if(!dicDataScore.ContainsKey (scoreKey))
			CreateDataScore (ScoreName, points, battleId);
		else
			dicDataScore[scoreKey].NrPoints -= points;
	}

	public void _SetScorePoints (string ScoreName, int points, int battleId = -1)
	{
		string scoreKey = GetScoreKey (ScoreName, battleId);

		if(!dicDataScore.ContainsKey (scoreKey))
			CreateDataScore (ScoreName, points, battleId);
		else
			dicDataScore[scoreKey].NrPoints = points;
	}
	
	public void _LoadScore ()
	{
		//TODO ler apenas scores totais, ou seja, sem que tenham IdBattle's
		dataScoreDAO.LoadAllPlayerScores (player.ToDatabaseModel (),
											(scores) =>
											{
												Debug.Log ("Llego!");
												dicDataScore = scores;
											});
	}
	
	public void _LoadBattleScore (A b)
	{
		dataScoreDAO.LoadAllBattleScores (ConfigurationData.battle.ToDatabaseModel (),
											(scores) =>
											{
												dicCurrentBattleScore = scores;
			
												b (dicCurrentBattleScore);
											});
	}


	public void _SaveScore ()
	{
		dataScoreDAO.SaveScores (dicDataScore);
	}
	
	public List<Model.DataScore> CurrentBattle ()
	{
		return dicCurrentBattleScore;
	}

	/* Static */
	private static Score instance;
	public static Score Instance
	{
		get
		{
			if (instance == null)
			{
				instance = ComponentGetter.Get <Score> ().Init ();
			}

			return instance;
		}
	}

	public static void AddScorePoints (string ScoreName, int points, int battleId = -1)
	{
		Instance._AddScorePoints (ScoreName, points, battleId);
	}

	public static void SubtractScorePoints (string ScoreName, int points, int battleId = -1)
	{
		Instance._SubtractScorePoints (ScoreName, points, battleId);
	}

	public static void SetScorePoints (string ScoreName, int points, int battleId = -1)
	{
		Instance._SetScorePoints (ScoreName, points, battleId);
	}

	public static void Load ()
	{
		Instance._LoadScore ();
	}
	
	public static void LoadBattle (A b)
	{
		Instance._LoadBattleScore (b);
	}

	public static void Save ()
	{
		Instance._SaveScore ();
	}
}