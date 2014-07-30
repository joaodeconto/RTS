using UnityEngine;
using System.Collections;

namespace DB
{
		public class DataScoreRanking
		{
				public string IdPlayer;
				public string SzName;
				public string NrPointsVictory;
				public string NrPointsDefeat;
				public string NrPoints;
				public string IdLastBattle;

				public Model.DataScoreRanking ToModel ()
				{
						Model.DataScoreRanking ds = new Model.DataScoreRanking ();
						ds.IdPlayer = int.Parse (this.IdPlayer);
						ds.SzName = this.SzName;
						ds.NrPointsVictory = int.Parse (this.NrPointsVictory);
						ds.NrPointsDefeat = int.Parse (this.NrPointsDefeat);
						ds.NrPoints = int.Parse (this.NrPoints);
						ds.IdLastBattle = int.Parse (this.IdLastBattle);
						return ds;
				}
		
				public override string ToString ()
				{	
						return "IdPlayer: " + IdPlayer +
								" - SzName: " + SzName +
								" - NrPointsVictory: " + NrPointsVictory +
								" - NrPointsDefeat: " + NrPointsDefeat +
								" - NrPoints: " + NrPoints +
								" - IdLastBattle: " + IdLastBattle;
				}
		}
}
