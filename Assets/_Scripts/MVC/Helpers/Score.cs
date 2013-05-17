using UnityEngine;
using System.Collections;

using System.Collections.Generic;

using Visiorama;

public class Score : MonoBehaviour
{
	private Dictionary <string, Model.DataScore> dicDataScore;
	private Model.Player player;
	private DataScoreDAO dataScoreDAO;

	void Start ()
	{
		PhotonWrapper pw = ComponentGetter.Get <PhotonWrapper> ();
		player = new Model.Player ((string)pw.GetPropertyOnPlayer ("player"));

		dataScoreDAO = ComponentGetter.Get <DataScoreDAO> ();

		dataScoreDAO.LoadScoresFromPlayer (player.ToDatabaseModel (),
									(scores) =>
									{
										Debug.Log ("Llego!");
										dicDataScore = scores;
									});
	}

	public void _AddScorePoints (string ScoreName, int points)
	{
		if(!dicDataScore.ContainsKey (ScoreName))
			dicDataScore.Add (ScoreName, new Model.DataScore (player.IdPlayer, ScoreName, points));
		else
			dicDataScore[ScoreName].NrPoints += points;
	}

	public void _SubtractScorePoints (string ScoreName, int points)
	{
		if(!dicDataScore.ContainsKey (ScoreName))
			dicDataScore.Add (ScoreName, new Model.DataScore (player.IdPlayer, ScoreName, points));
		else
			dicDataScore[ScoreName].NrPoints -= points;
	}

	public void _SetScorePoints (string ScoreName, int points)
	{
		if(!dicDataScore.ContainsKey (ScoreName))
			dicDataScore.Add (ScoreName, new Model.DataScore (player.IdPlayer, ScoreName, points));
		else
			dicDataScore[ScoreName].NrPoints = points;
	}

	public void _LoadScore ()
	{

	}

	public void _SaveScore ()
	{
		//dataScoreDAO.Save ();
	}

	/* Static */
	private static Score instance;
	public static Score Instance
	{
		get
		{
			if (instance == null) {
				instance = ComponentGetter.Get <Score> ();
			}

			return instance;
		}
	}

	public static void AddScorePoints (string ScoreName, int points)
	{
		Instance._AddScorePoints (ScoreName, points);
	}

	public static void SubtractScorePoints (string ScoreName, int points)
	{
		Instance._SubtractScorePoints (ScoreName, points);
	}

	public static void SetScorePoints (string ScoreName, int points)
	{
		Instance._SetScorePoints (ScoreName, points);
	}

	public static void LoadScore ()
	{
		Instance._LoadScore ();
	}

	public static void SaveScore ()
	{
		Instance._SaveScore ();
	}
}
