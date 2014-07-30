using UnityEngine;
using System.Collections;

namespace Model {
	public class DataScoreRanking
	{
		public int IdPlayer;
		public string SzName;
		public int NrPointsVictory;
		public int NrPointsDefeat;
		public int NrPoints;
		public int IdLastBattle;

		public DataScoreRanking () { }

		public DataScoreRanking (int IdPlayer, string SzName, int NrPointsVictory, int NrPointsDefeat, int IdLastBattle)
		{
			this.IdPlayer    = IdPlayer;
			this.SzName    = SzName;
			this.NrPointsVictory = NrPointsVictory;
			this.NrPointsDefeat  = NrPointsDefeat;
			this.NrPoints    = (NrPointsVictory + NrPointsDefeat);
			this.IdLastBattle    = IdLastBattle;
		}
		
		public DB.DataScoreRanking ToDatabaseModel ()
		{
			DB.DataScoreRanking ds = new DB.DataScoreRanking ();
			
			ds.IdPlayer           = this.IdPlayer.ToString();
			ds.SzName              = this.SzName;
			ds.NrPointsVictory           = this.NrPointsVictory.ToString();
			ds.NrPointsDefeat           = this.NrPointsDefeat.ToString();
			ds.NrPoints           = this.NrPoints.ToString();
			ds.IdLastBattle           = this.IdLastBattle.ToString();			
            return ds;
        }		

	}
}
