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
	}
}
