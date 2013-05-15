using UnityEngine;

using System;
using System.Collections;

using Visiorama;

public class PlayerDAO : MonoBehaviour
{
	public delegate void PlayerDAODelegate (Model.Player player, string message);

	public void GetPlayer (string username, string password, string idFacebook, PlayerDAODelegate callback)
	{
		//TODO usar facebook no futuro
		Database db        = ComponentGetter.Get<Database>();
		DB.Player dbPlayer = new DB.Player () { SzName = username,
												SzPassword = password };

		db.Read (dbPlayer,
		(response) =>
		{
			dbPlayer = response as DB.Player;

			if (dbPlayer == null ||
				string.IsNullOrEmpty(dbPlayer.SzName) ||
				string.IsNullOrEmpty(dbPlayer.SzPassword))
			{
				callback (null, "Wrong user or login");
			}
			else
			{
				Model.Player player = dbPlayer.ToModel ();
				callback (player, "User and passwords matches!");
			}
		});
	}

	public void CreatePlayer (string username, string password, string idFacebook, string email, PlayerDAODelegate callback)
	{
		Database db        = ComponentGetter.Get<Database>();
		DB.Player dbPlayer = new DB.Player () { SzName     = username,
												SzPassword = password,
												SzEmail    = email};
		db.Create (dbPlayer,
		(response) =>
		{
			Model.Player player = null;
			dbPlayer = response as DB.Player;

			if (dbPlayer == null)
			{
				callback (null, "User already exists");
			}
			else
			{
				player = dbPlayer.ToModel ();
				callback (player, "New account created!");
			}
		});
	}

	public void SavePlayer (Model.Player player)
	{
		throw new NotImplementedException ();
	}
}
