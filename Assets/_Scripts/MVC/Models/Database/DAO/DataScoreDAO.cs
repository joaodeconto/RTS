using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using Visiorama;

using Newtonsoft.Json;

public class DataScoreDAO : MonoBehaviour
{
	private Database db;

	void Awake () { db = ComponentGetter.Get <Database> (); }

	public delegate void DataScoreDAODelegateList (Dictionary <string, Model.DataScore> scores);
	public void LoadScoresFromPlayer (DB.Player player, DataScoreDAODelegateList callback)
	{
		List <DB.DataScore> lDataScore = new List <DB.DataScore> ();
		db.Read (player, lDataScore.GetType (), "read-list-datascore",
		(response) =>
		{
			Dictionary <string, Model.DataScore> dic = new Dictionary <string, Model.DataScore> ();

			if ((response as List <DB.DataScore>) != null)
			{
				lDataScore = (response as List <DB.DataScore>);
			}

			for (int i = lDataScore.Count - 1; i != -1; --i)
			{
				dic.Add (lDataScore[i].SzScoreName, lDataScore[i].ToModel ());
			}

			callback (dic);
		});
	}
}
