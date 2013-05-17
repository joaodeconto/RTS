using UnityEngine;
using System.Collections;

namespace DB {
	public class DataScore : MonoBehaviour
	{
		public string IdScore;
		public string IdPlayer;
		public string IdPlayerBattle;
		public string SzScoreName;
		public string SzDetailedInformation;
		public string NrPoints;

		public Model.DataScore ToModel ()
		{
			Model.DataScore ds = new Model.DataScore ();
			ds.IdScore               = int.Parse(this.IdScore);
			ds.IdPlayer              = int.Parse(this.IdPlayer);
			ds.IdPlayerBattle        = int.Parse(this.IdPlayerBattle);
			ds.NrPoints              = int.Parse(this.NrPoints);
			ds.SzScoreName           = this.SzScoreName;
			ds.SzDetailedInformation = this.SzDetailedInformation;
			return ds;
		}
	}
}
