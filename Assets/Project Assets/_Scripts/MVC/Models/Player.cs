using UnityEngine;
using System.Collections;

using System;

using Newtonsoft.Json;

public class Player
{
	public int IdPlayer;
	public string SzEmail;
	public string SzName;

	public string IdFacebookAccount;
	public string SzPassword;
	public string TmTimePlayed;
	public DateTime DtAccountCreated;
	public DateTime DtAccountUpdated;
	public int IdCountry;

	public Player ()
	{
	}
}
