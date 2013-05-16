using UnityEngine;
using System.Collections;

using System.Collections.Generic;

using Visiorama;

public class Score : MonoBehaviour
{
	//private Dictionary <string, DB.DataScore> listDataScore;
	//private Model.Player player;

	//void Start ()
	//{
		//PhotonWrapper pw = ComponentGetter.Get <PhotonWrapper> ();
		//player = new Model.Player ((string)pw.GetPropertyOnPlayer ("player"));

		//listDataScore = new Dictionary <DB.DataScore> ();
	//}

	//public void AddScorePoints (string ScoreName, int points)
	//{
		//if(!listDataScore.ContainsKey (ScoreName))
			//listDataScore.Add (ScoreName, new Model.DataScore (player.IdPlayer, ScoreName, points));
		//else
			//listDataScore[ScoreName].NrPoints += points;
	//}

	//public void SubtractScorePoints (string ScoreName, int points)
	//{
		//if(!listDataScore.ContainsKey (ScoreName))
			//listDataScore.Add (ScoreName, new Model.DataScore (player.IdPlayer, ScoreName, points));
		//else
			//listDataScore[ScoreName].NrPoints -= points;
	//}

	//public void SetScorePoints (string ScoreName, int points)
	//{
		//if(!listDataScore.ContainsKey (ScoreName))
			//listDataScore.Add (ScoreName, new Model.DataScore (player.IdPlayer, ScoreName, points));
		//else
			//listDataScore[ScoreName].NrPoints = points;
	//}

	//public void SaveScore ()
	//{

	//}

	//private static Score instance;
	//public static Score Instance
	//{
		//get
		//{
			//if (instance == null) {
				//instance = ComponentGetter.Get <Score> ();
			//}

			//return instance;
		//}
	//}

	////public static void AddScorePoints (string ScoreName, int points)
	////{
		////Instance.AddScorePoints (ScoreName, points);
	////}

	////public static void SubtractScorePoints (string ScoreName, int points)
	////{
		////Instance.SubtractScorePoints (ScoreName, points);
	////}

	////public static void SetScorePoints (string ScoreName, int points)
	////{
		////Instance.SetScorePoints (ScoreName, points);
	////}

	////public static void SaveScore ()
	////{

	////}
}
