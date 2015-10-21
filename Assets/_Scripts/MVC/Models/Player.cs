using UnityEngine;
using System.Collections;

using System;

using Newtonsoft.Json;

namespace Model {
	public class Player
	{
		//TODO colocar tempo de jogo funcionar
		public const float TicksToSecondsRate = 10000;

		public int IdPlayer;
		public string SzEmail;
		public string SzName;

		public string IdFacebookAccount;
		public string SzPassword;
		public TimeSpan TmTimePlayed;
		public DateTime DtAccountCreated;
		public DateTime DtAccountUpdated;
		public int IdCountry;

		public Player () {}
		public Player (string JSONString)
		{
			Debug.Log ("JSONString: " + JSONString);
			DB.Player p = (DB.Player)JsonConvert.DeserializeObject (JSONString, typeof(DB.Player));

			Model.Player pp = p.ToModel ();

			this.IdPlayer = pp.IdPlayer;
			this.SzEmail  = pp.SzEmail;
			this.SzName   = pp.SzName;

			this.IdFacebookAccount = pp.IdFacebookAccount;
			this.SzPassword        = pp.SzPassword;
			this.TmTimePlayed      = pp.TmTimePlayed;
			this.DtAccountCreated  = pp.DtAccountCreated;
			this.DtAccountUpdated  = pp.DtAccountUpdated;
			this.IdCountry         = pp.IdCountry;
		}

		public DB.Player ToDatabaseModel ()
		{
			DB.Player player 	= new DB.Player ();
			player.IdPlayer 	= this.IdPlayer.ToString ();
			player.SzEmail  	= this.SzEmail;
			player.SzName   	= this.SzName;

			player.IdFacebookAccount = this.IdFacebookAccount;
			player.SzPassword        = this.SzPassword;
			//player.TmTimePlayed      = TimeSpan.FromSeconds(long.Parse(this.TmTimePlayed.ticks / TicksToSecondsRate) * TicksToSecondsRate);
			player.DtAccountCreated  = this.DtAccountCreated.ToString ("yyyy-MM-dd HH:mm:ss");
			player.DtAccountUpdated  = this.DtAccountUpdated.ToString ("yyyy-MM-dd HH:mm:ss");
			//player.IdCountry         = !string.IsNullOrEmpty(this.IdCountry) ? int.Parse(this.IdCountry) : 0;

			return player;
		}

		public override string ToString ()
		{
			return JsonConvert.SerializeObject (this.ToDatabaseModel ());
		}
	}
}
