using UnityEngine;

using System;
using System.Collections.Generic;

using Visiorama;

public class PlayerBattleDAO : MonoBehaviour
{
	public delegate void BattleTypeDAODelegate (Model.BattleType battleType);
	public delegate void BattleDAODelegate (Model.Battle battle);
	public delegate void PlayerBattleDAODelegate (Model.PlayerBattle playerBattle, string message);
	public delegate void PlayerBattleDAODelegateList (List<Model.PlayerBattle> playerBattles, string message);

	private Database db;

	void Awake ()
	{
		db = ComponentGetter.Get <Database> ();
	}

	public void GetAllPlayerBattlesFromPlayer (int idPlayer, PlayerBattleDAODelegateList callbacks)
	{
		throw new NotImplementedException ();
		//Database db                    = ComponentGetter.Get<Database>();
		//DB.PlayerBattle dbPlayerBattle = new DB.PlayerBattle () { SzName = username };

		//db.Read (dbPlayerBattle,
		//(response) =>
		//{
			//Model.PlayerBattle battle = null;
			//dbPlayerBattle = response as DB.PlayerBattle;

			//if (dbPlayerBattle == null ||
				//string.IsNullOrEmpty(dbPlayerBattle.SzName) ||
				//string.IsNullOrEmpty(dbPlayerBattle.SzPassword))
			//{
				//callback (null, "Wrong user or login");
			//}
			//else
			//{
				//battle = ConvertPlayer (dbPlayerBattle);
				//callback (battle, "User and passwords matches!");
			//}
		//});
	}

	private void GetBattleType (string battleTypeName, BattleTypeDAODelegate callback)
	{
		DB.BattleType dbBattleType = new DB.BattleType () { SzBattleTypeName = battleTypeName };

		db.Read (dbBattleType,
		(response) =>
		{
		
			Debug.Log ("Chegou");
			
			Model.BattleType battleType = null;
			if ((response as DB.BattleType) != null)
			{
				dbBattleType = response as DB.BattleType;
				callback (dbBattleType.ToModel ());
			}
			else
			{
				db.Create (dbBattleType,
				(read_response) =>
				{
					dbBattleType = read_response as DB.BattleType;
					callback (dbBattleType.ToModel ());
				});
			}
		});
	}

	public void CreateBattle (string battleTypeName,
							  DateTime dateBattle,
							  int nrPlayers,
							  BattleDAODelegate callback)
	{
		GetBattleType (battleTypeName,
		(battleType) =>
		{
			Debug.Log ("Criou battle type");
			DB.Battle dbBattle = (new Model.Battle () { IdBattleType = battleType.IdBattleType,
														battleType   = battleType,
														DtDateBattle = dateBattle,
														NrPlayers    = nrPlayers
													}).ToDatabaseModel ();

			db.Create (dbBattle,
			(response) =>
			{
				if ((response as DB.Battle) != null)
				{
					dbBattle = response as DB.Battle;
					dbBattle.battleType = battleType.ToDatabaseModel ();
					callback (dbBattle.ToModel ());
				}
				else
				{
					//ahn?
				}
			});
		});
	}

	private void GetBattle (Model.Player player, DateTime dtDateBattle, int nrPlayers, BattleTypeDAODelegate callback)
	{
		throw new NotImplementedException ();
		//Database db        = ComponentGetter.Get<Database>();
		//DB.Battle dbBattle = new DB.Battle () { IdBattleType = NrPlayers = nrPlayers.ToString () };

		//db.Read (dbBattleType,
		//(response) =>
		//{
			//Model.BattleType battleType = null;
			//if ((response as DB.BattleType) != null)
			//{
				//dbBattleType = response as DB.BattleType;
				//callback (ConvertBattleType (dbBattleType));
			//}
			//else
			//{
				//db.Create (dbBattleType,
				//(read_response) =>
				//{
					//dbBattleType = read_response as DB.BattleType;
					//callback (ConvertBattleType (dbBattleType));
				//});
			//}
		//});
	}

	public void CreatePlayerBattle (Model.Player player, Model.Battle battle, PlayerBattleDAODelegate callback)
	{
		Debug.Log ("player: " + player.ToString ());

		DB.PlayerBattle dbPlayerBattle = (new Model.PlayerBattle (){IdPlayer = player.IdPlayer,
																	IdBattle = battle.IdBattle,
																	battle   = battle}).ToDatabaseModel ();
		Debug.Log ("dbPlayerBattle: " + dbPlayerBattle);
		db.Create (dbPlayerBattle,
		(response) =>
		{
			Model.PlayerBattle playerBattle = null;
			dbPlayerBattle = response as DB.PlayerBattle;

			if (dbPlayerBattle == null)
			{
				callback (null, "Could not create playerBattle");
			}
			else
			{
				playerBattle = dbPlayerBattle.ToModel ();
				playerBattle.battle = battle;
				callback (playerBattle,  "Model.PlayerBattle has been created!");
			}
		});
	}

	public void UpdatePlayerBattle (Model.PlayerBattle playerBattle, PlayerBattleDAODelegate callback)
	{
		db.Update (playerBattle,
		(response) =>
		{
			DB.PlayerBattle pb = (response as DB.PlayerBattle);
			if (pb == null)
			{
				callback (null, "Could not update playerBattle");
			}
			else
			{
				playerBattle = pb.ToModel ();
				callback (playerBattle,  "Model.PlayerBattle has been updated!");
			}
		});
	}
}
