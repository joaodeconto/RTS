using UnityEngine;
using System.Collections;

using System;

using Newtonsoft.Json;

namespace DB {
	public class Player
	{
		[JsonProperty(PropertyName = "IdPlayer",Required=Required.Default)]
		public string IdPlayer;
		[JsonProperty(PropertyName = "SzEmail",Required=Required.Default)]
		public string SzEmail;
		[JsonProperty(PropertyName = "SzName",Required=Required.Default)]
		public string SzName;
		[JsonProperty(PropertyName = "IdFacebookAccount",Required=Required.Default)]
		public string IdFacebookAccount;
		[JsonProperty(PropertyName = "SzPassword",Required=Required.Default)]
		public string SzPassword;
		[JsonProperty(PropertyName = "TmTimePlayed",Required=Required.Default)]
		public string TmTimePlayed;
		[JsonProperty(PropertyName = "DtAccountCreated",Required=Required.Default)]
		public string DtAccountCreated;
		[JsonProperty(PropertyName = "DtAccountUpdated",Required=Required.Default)]
		public string DtAccountUpdated;
		[JsonProperty(PropertyName = "IdCountry",Required=Required.Default)]
		public string IdCountry;

		public Player ()
		{
		}

		public Model.Player ToModel ()
		{
			Model.Player player = new Model.Player ();

			player.IdPlayer = int.Parse(this.IdPlayer);
			player.SzEmail  = this.SzEmail;
			player.SzName   = this.SzName;

			player.IdFacebookAccount = this.IdFacebookAccount;
			player.SzPassword        = this.SzPassword;
			//player.TmTimePlayed      = TimeSpan.FromSeconds(long.Parse(this.TmTimePlayed));
			player.DtAccountCreated  = DateTime.Parse (this.DtAccountCreated);
			player.DtAccountUpdated  = DateTime.Parse (this.DtAccountUpdated);
			player.IdCountry         = !string.IsNullOrEmpty(this.IdCountry) ? int.Parse(this.IdCountry) : 0;

			return player;
		}

		public override string ToString ()
		{
			return JsonConvert.SerializeObject (this);
		}
	}
}
