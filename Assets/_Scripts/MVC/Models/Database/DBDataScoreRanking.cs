using UnityEngine;
using System.Collections;

namespace DB
{
		public class DataScoreRanking
		{
				public string IdPlayer;
				public string SzName;
				public string NrVictory;
				public string NrDefeat;
				public string NrPoints;
				public string IdLastBattle;

				public Model.DataScoreRanking ToModel ()
				{
						Model.DataScoreRanking ds = new Model.DataScoreRanking ();
						ds.IdPlayer = int.Parse (this.IdPlayer);
						ds.SzName = this.SzName;
						ds.NrVictory = int.Parse (this.NrVictory);
						ds.NrDefeat = int.Parse (this.NrDefeat);
						ds.NrPoints = int.Parse (this.NrPoints);
            			ds.IdLastBattle = int.Parse (this.IdLastBattle);
						return ds;
				}
		
				public override string ToString ()
				{	
						return "IdPlayer: " + IdPlayer +
								" - SzName: " + SzName +
								" - NrVictory: " + NrVictory +
								" - NrDefeat: " + NrDefeat +
								" - NrPoints: " + NrPoints +
								" - IdLastBattle: " + IdLastBattle;
				}
		}
}
