using UnityEngine;
using System.Collections;

namespace Model {
	public class DataScoreRanking
	{
		public int IdPlayer;
		public string SzName;
		public int NrVictory;
		public int NrDefeat;
		public int NrPoints;
		public int IdLastBattle;

		public DataScoreRanking () { }

		public DataScoreRanking (int IdPlayer, string SzName, int NrVictory, int NrDefeat, int NrPoints, int IdLastBattle)
		{
			this.IdPlayer    = IdPlayer;
			this.SzName    = SzName;
			this.NrVictory = NrVictory;
			this.NrDefeat  = NrDefeat;
			this.NrPoints    = NrPoints;
			this.IdLastBattle    = IdLastBattle;
		}
		
		public DB.DataScoreRanking ToDatabaseModel ()
		{
			DB.DataScoreRanking ds = new DB.DataScoreRanking ();
			
			ds.IdPlayer           = this.IdPlayer.ToString();
			ds.SzName              = this.SzName;
			ds.NrVictory           = this.NrVictory.ToString();
			ds.NrDefeat           = this.NrDefeat.ToString();
			ds.NrPoints           = this.NrPoints.ToString();
			ds.IdLastBattle           = this.IdLastBattle.ToString();			
            return ds;
        }		

	}
}
