using UnityEngine;

using System;
using System.Collections.Generic;

using Visiorama;

public class DAOPlayerBattle : MonoBehaviour
{
	public delegate void DAOBattleTypeDelegate (BattleType battleType);
	public delegate void DAOBattleDelegate (Battle battle);
	public delegate void DAOPlayerBattleDelegate (PlayerBattle battle, string message);
	public delegate void DAOPlayerBattleDelegateList (List<PlayerBattle> battles, string message);

	public void GetAllPlayerBattlesFromPlayer (int idPlayer, DAOPlayerBattleDelegateList callbacks)
	{
		throw new NotImplementedException ();
		//Database db                    = ComponentGetter.Get<Database>();
		//DB.PlayerBattle dbPlayerBattle = new DB.PlayerBattle () { SzName = username };

		//db.Read (dbPlayerBattle,
		//(response) =>
		//{
			//PlayerBattle battle = null;
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

	private void GetBattleType (string battleTypeName, DAOBattleTypeDelegate callback)
	{
		Database db = ComponentGetter.Get<Database>();
		DB.BattleType dbBattleType = new DB.BattleType () { SzBattleTypeName = battleTypeName };

		db.Read (dbBattleType,
		(response) =>
		{
			BattleType battleType = null;
			if ((response as DB.BattleType) != null)
			{
				dbBattleType = response as DB.BattleType;
				callback (ConvertBattleType (dbBattleType));
			}
			else
			{
				db.Create (dbBattleType,
				(read_response) =>
				{
					dbBattleType = read_response as DB.BattleType;
					callback (ConvertBattleType (dbBattleType));
				});
			}
		});
	}

	private void GetBattle (int idPlayer, DateTime DtDateBattle, int nPlayers, BattleType DAOBattleTypeDelegate callback)
	{
		Database db = ComponentGetter.Get<Database>();
		DB.Battle dbBattle = new DB.Battle () { nrPlayers = nPlayers, blWin = win };

		db.Read (dbBattleType,
		(response) =>
		{
			BattleType battleType = null;
			if ((response as DB.BattleType) != null)
			{
				dbBattleType = response as DB.BattleType;
				callback (ConvertBattleType (dbBattleType));
			}
			else
			{
				db.Create (dbBattleType,
				(read_response) =>
				{
					dbBattleType = read_response as DB.BattleType;
					callback (ConvertBattleType (dbBattleType));
				});
			}
		});
	}

	public void CreatePlayerBattle (int idPlayer, int nPlayers, bool win, string battleTypeName, DAOPlayerBattleDelegate callback)
	{
		Database db = ComponentGetter.Get <Database> ();

		DB.BattleType bt = ConvertBattleType (battleType);

		DB.PlayerBattle dbPlayerBattle = new DB.PlayerBattle () { };

		db.Create (dbPlayerBattle,
		(response) =>
		{
			PlayerBattle battle = null;
			dbPlayerBattle = response as DB.PlayerBattle;

			if (dbPlayerBattle == null)
			{
				callback (null, "User already exists");
			}
			else
			{
				battle = ConvertPlayerBattle (dbPlayerBattle);
				callback (battle, "New account created!");
			}
		});
	}

	public void SavePlayerBattle (PlayerBattle battle)
	{
		throw new NotImplementedException ();
	}

	private BattleType ConvertBattleType (DB.BattleType bt)
	{
		BattleType battleType = new BattleType ();

		battleType.IdBattleType     = int.Parse(bt.IdBattleType);
		battleType.SzBattleTypeName = bt.SzBattleTypeName;

		return battleType;
	}

	private DB.BattleType ConvertBattleType (BattleType bt)
	{
		DB.BattleType battleType = new DB.BattleType ();

		battleType.IdBattleType     = bt.IdBattleType.ToString ();
		battleType.SzBattleTypeName = bt.SzBattleTypeName;

		return battleType;
	}

	private DB.PlayerBattle ConvertPlayerBattle (PlayerBattle bt)
	{
		return null;
	}

	private PlayerBattle ConvertPlayerBattle (DB.PlayerBattle bt)
	{
		return null;
	}
}
