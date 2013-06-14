using UnityEngine;
using System.Collections;

namespace DB {
	public class DataScore
	{
		public string IdDataScore;
		public string IdPlayer;
		public string IdBattle;
		public string SzScoreName;
		public string SzDetailedInformation;
		public string NrPoints;

		public Model.DataScore ToModel ()
		{
			Model.DataScore ds = new Model.DataScore ();
			ds.IdDataScore           = int.Parse(this.IdDataScore);
			ds.IdPlayer              = int.Parse(this.IdPlayer);
			ds.IdBattle              = int.Parse(this.IdBattle);
			ds.NrPoints              = int.Parse(this.NrPoints);
			ds.SzScoreName           = this.SzScoreName;
			ds.SzDetailedInformation = this.SzDetailedInformation;
			return ds;
		}
	}
}
