using UnityEngine;
using System.Collections;

using System;

using Newtonsoft.Json;

public class Player
{
    [JsonProperty(PropertyName = "id_player",Required=Required.Default)]
	public int idPlayer;
    [JsonProperty(PropertyName = "sz_name",Required=Required.Default)]
	public string szName;
    [JsonProperty(PropertyName = "id_facebook_account",Required=Required.Default)]
	public string idFacebookAccount;
    [JsonProperty(PropertyName = "sz_password",Required=Required.Default)]
	public string szPassword;
    [JsonProperty(PropertyName = "tm_time_played",Required=Required.Default)]
	public string tmTimePlayed;
	[JsonProperty(PropertyName = "dt_account_created",Required=Required.Default)]
	public DateTime dtAccountCreated;
    [JsonProperty(PropertyName = "dt_account_updated",Required=Required.Default)]
	public DateTime dtAccountUpdated;
    [JsonProperty(PropertyName = "id_country",Required=Required.Default)]
	public int idCountry;

	public Player ()
	{

	}
}
