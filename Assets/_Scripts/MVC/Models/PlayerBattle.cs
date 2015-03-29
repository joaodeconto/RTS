using UnityEngine;
using System.Collections;

using System;

using Newtonsoft.Json;

namespace Model {
	public class Battle
	{
		public int IdBattle;
		public int IdBattleType;
		public BattleType battleType;

		public int Bid;

		public DateTime DtDateBattle;
		public int NrPlayers;

		public Battle () {}

		public Battle (string JSONString)
		{
			//Debug.Log ("JSONString: " + JSONString);

			DB.Battle b = (DB.Battle)JsonConvert.DeserializeObject (JSONString, typeof (DB.Battle));
			Model.Battle bb = b.ToModel ();

			this.IdBattle     = bb.IdBattle;
			this.IdBattleType = bb.IdBattleType;
			this.battleType   = bb.battleType;

			this.Bid 		  = bb.Bid;

			this.DtDateBattle = bb.DtDateBattle;
			this.NrPlayers    = bb.NrPlayers;
		}

		public DB.Battle ToDatabaseModel ()
		{
			DB.Battle battle = new DB.Battle ();

			battle.IdBattle     = this.IdBattle.ToString ();
			battle.IdBattleType = this.IdBattleType.ToString ();
			battle.battleType   = this.battleType.ToDatabaseModel ();
			
			battle.Bid 			= this.Bid.ToString ();

			//Debug.Log ("DtDateBattle: " + this.DtDateBattle.ToString ("yyyy-MM-dd HH:mm:ss"));
			battle.DtDateBattle = this.DtDateBattle.ToString ("yyyy-MM-dd HH:mm:ss");
			battle.NrPlayers    = this.NrPlayers.ToString ();

			return battle;
		}

		public override string ToString ()
		{
			return this.ToDatabaseModel ().ToString ();
		}

		private static Battle b;
		public static Battle CurrentBattle
		{
			get
			{
				if (b == null)
					b = new Battle ();

				return b;
			}
		}
	}

	public class BattleType
	{
		public int IdBattleType;
		public string SzBattleTypeName;

		public DB.BattleType ToDatabaseModel ()
		{
			DB.BattleType battleType = new DB.BattleType ();

			battleType.IdBattleType     = this.IdBattleType.ToString ();
			battleType.SzBattleTypeName = this.SzBattleTypeName;

			return battleType;
		}

		public override string ToString ()
		{
			return this.ToDatabaseModel ().ToString ();
		}

		private static BattleType bt;
		public static BattleType CurrentBattleType
		{
			get
			{
				if (bt == null)
					bt = new BattleType ();

				return bt;
			}
		}
	}

	public class PlayerBattle
	{
		public int IdPlayerBattle;

		public int IdPlayer;
		public int IdBattle;
		public Battle battle;
		public int PScore;
		public bool BlWin;

		public PlayerBattle () {}

		public PlayerBattle (string JSONString)
		{
			DB.PlayerBattle b = (DB.PlayerBattle)JsonConvert.DeserializeObject (JSONString, typeof (DB.PlayerBattle));
			Model.PlayerBattle bb = b.ToModel ();

			this.IdPlayerBattle = bb.IdPlayerBattle;
			this.IdBattle       = bb.IdBattle;
			this.BlWin          = bb.BlWin;
			this.PScore         = bb.PScore;

			this.battle = bb.battle;
		}

		public DB.PlayerBattle ToDatabaseModel ()
		{
			DB.PlayerBattle playerBattle = new DB.PlayerBattle ();

			playerBattle.IdPlayer = this.IdPlayer.ToString ();
			playerBattle.IdBattle = this.IdBattle.ToString ();
			playerBattle.battle   = this.battle.ToDatabaseModel ();

			return playerBattle;
		}

		public override string ToString ()
		{
			return this.ToDatabaseModel ().ToString ();
		}

		private static PlayerBattle pb;
		public static PlayerBattle CurrentPlayerBattle
		{
			get
			{
				if (pb == null)
					pb = new PlayerBattle ();

				return pb;
			}
		}
	}
}
