using UnityEngine;

using System;
using System.Collections;

using Visiorama;

public class DAOPlayer : MonoBehaviour
{
	public delegate void DAOPlayerDelegate (Player player, string message);

	public void GetPlayer (string username, string password, string idFacebook, DAOPlayerDelegate callback)
	{
		//TODO usar facebook no futuro
		Database db        = ComponentGetter.Get<Database>();
		DB.Player dbPlayer = new DB.Player () { SzName = username,
												SzPassword = password };

		db.Read (dbPlayer,
		(response) =>
		{
			Player player = null;
			dbPlayer = response as DB.Player;

			if (dbPlayer == null ||
				string.IsNullOrEmpty(dbPlayer.SzName) ||
				string.IsNullOrEmpty(dbPlayer.SzPassword))
			{
				callback (null, "Wrong user or login");
			}
			else
			{
				player = ConvertPlayer (dbPlayer);
				callback (player, "User and passwords matches!");
			}
		});
	}

	public void CreatePlayer (string username, string password, string idFacebook, string email, DAOPlayerDelegate callback)
	{
		Database db        = ComponentGetter.Get<Database>();
		DB.Player dbPlayer = new DB.Player () { SzName     = username,
												SzPassword = password,
												SzEmail    = email};
		db.Create (dbPlayer,
		(response) =>
		{
			Player player = null;
			dbPlayer = response as DB.Player;

			if (dbPlayer == null)
			{
				callback (null, "User already exists");
			}
			else
			{
				player = ConvertPlayer (dbPlayer);
				callback (player, "New account created!");
			}
		});
	}

	public void SavePlayer (Player player)
	{
		throw new NotImplementedException ();
	}

	private Player ConvertPlayer (DB.Player p)
	{
		Player player = new Player ();

		player.IdPlayer = int.Parse(p.IdPlayer);
		player.SzEmail  = p.SzEmail;
		player.SzName   = p.SzName;

		player.IdFacebookAccount = p.IdFacebookAccount;
		player.SzPassword        = p.SzPassword;
		player.TmTimePlayed      = p.TmTimePlayed;
		player.DtAccountCreated  = DateTime.Parse (p.DtAccountCreated);
		player.DtAccountUpdated  = DateTime.Parse (p.DtAccountUpdated);
		player.IdCountry         = int.Parse(p.IdCountry);

		return player;
	}
}
