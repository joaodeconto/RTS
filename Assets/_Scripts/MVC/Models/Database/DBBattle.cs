using UnityEngine;

using System;
using System.Collections;

using Newtonsoft.Json;

namespace DB {
	public class Battle
	{
		[JsonProperty(PropertyName = "IdBattle",Required=Required.Default)]
		public string IdBattle;
		[JsonProperty(PropertyName = "IdBattleType",Required=Required.Default)]
		public string IdBattleType;
		[JsonProperty(PropertyName = "BattleType",Required=Required.Default)]
		public DB.BattleType battleType;
		
		[JsonProperty(PropertyName = "NrBid",Required=Required.Default)]
		public string Bid;

		[JsonProperty(PropertyName = "DtDateBattle",Required=Required.Default)]
		public string DtDateBattle;
		[JsonProperty(PropertyName = "NrPlayers",Required=Required.Default)]
		public string NrPlayers;

		public Model.Battle ToModel ()
		{
			Model.Battle battle = new Model.Battle ();
			battle.IdBattle     = !string.IsNullOrEmpty(this.IdBattle) ? int.Parse(this.IdBattle) : -1;
			battle.IdBattleType = int.Parse(this.IdBattleType);
			battle.battleType   = this.battleType.ToModel ();
			
			battle.Bid 			= int.Parse (this.Bid);

			battle.DtDateBattle = Convert.ToDateTime (this.DtDateBattle);
			battle.NrPlayers    = int.Parse(this.NrPlayers);

			return battle;
		}

		public override string ToString ()
		{
			return JsonConvert.SerializeObject (this);
		}
	}

	public class BattleType
	{
		[JsonProperty(PropertyName = "IdBattleType",Required=Required.Default)]
		public string IdBattleType;
		[JsonProperty(PropertyName = "SzBattleTypeName",Required=Required.Default)]
		public string SzBattleTypeName;

		public Model.BattleType ToModel ()
		{
			Model.BattleType battleType = new Model.BattleType ();

			battleType.IdBattleType     = int.Parse(this.IdBattleType);
			battleType.SzBattleTypeName = this.SzBattleTypeName;

			return battleType;
		}

		public override string ToString ()
		{
			return JsonConvert.SerializeObject (this);
		}
	}

	public class PlayerBattle
	{
		[JsonProperty(PropertyName = "IdPlayerBattle",Required=Required.Default)]
		public string IdPlayerBattle;

		[JsonProperty(PropertyName = "IdPlayer",Required=Required.Default)]
		public string IdPlayer;
		[JsonProperty(PropertyName = "IdBattle",Required=Required.Default)]
		public string IdBattle;
		[JsonProperty(PropertyName = "battle",Required=Required.Default)]
		public Battle battle;

		[JsonProperty(PropertyName = "BlWin",Required=Required.Default)]
		public string BlWin;

		public Model.PlayerBattle ToModel ()
		{
			Model.PlayerBattle playerBattle = new Model.PlayerBattle ();

			playerBattle.IdPlayer = int.Parse(this.IdPlayer);
			playerBattle.IdBattle = int.Parse(this.IdBattle);

			if (this.battle != null)
				playerBattle.battle   = this.battle.ToModel ();

			return playerBattle;
		}

		public override string ToString ()
		{
			return JsonConvert.SerializeObject (this);
		}
	}
}
