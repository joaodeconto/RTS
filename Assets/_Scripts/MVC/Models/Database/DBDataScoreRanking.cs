using UnityEngine;
using System.Collections;

namespace DB
{
		public class DataScoreRanking
		{
				public int IdPlayer;
				public string SzName;
				public int NrPointsVictory;
				public int NrPointsDefeat;
				public int NrPoints;
				public int IdLastBattle;

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
