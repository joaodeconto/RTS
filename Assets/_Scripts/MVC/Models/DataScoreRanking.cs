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
			
			ds.IdPlayer           = this.IdPlayer;
			ds.SzName              = this.SzName.ToString();
			ds.NrPointsVictory           = this.NrPointsVictory;
			ds.NrPointsDefeat           = this.NrPointsDefeat;
			ds.NrPoints           = this.NrPoints;
			ds.IdLastBattle           = this.IdLastBattle;			
            return ds;
        }		

	}
}
