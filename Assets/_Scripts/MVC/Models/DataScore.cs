using UnityEngine;
using System.Collections;

namespace Model {
	public class DataScore
	{
		public int IdDataScore;
		public int IdPlayer;
		public int IdBattle;
		public int NrPoints;
		public string SzScoreName;
		public string SzDetailedInformation;

		public DataScore () { }

		public DataScore (int IdPlayer, string ScoreName, int points)
		{
			this.IdPlayer    = IdPlayer;
			this.SzScoreName = ScoreName;
			this.NrPoints    = points;
		}

		public DB.DataScore ToDatabaseModel ()
		{
			DB.DataScore ds = new DB.DataScore ();
			ds.IdDataScore           = this.IdDataScore.ToString ();
			ds.IdPlayer              = this.IdPlayer.ToString ();
			ds.IdBattle              = this.IdBattle.ToString ();
			ds.NrPoints              = this.NrPoints.ToString ();
			ds.SzScoreName           = this.SzScoreName;
			ds.SzDetailedInformation = this.SzDetailedInformation;
			return ds;
		}
	}
}
