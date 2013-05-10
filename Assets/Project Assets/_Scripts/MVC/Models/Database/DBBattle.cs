using UnityEngine;
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
		public BattleType battleType;

		[JsonProperty(PropertyName = "DtDateBattle",Required=Required.Default)]
		public string DtDateBattle;
	}

	public class BattleType
	{
		[JsonProperty(PropertyName = "IdBattleType",Required=Required.Default)]
		public string IdBattleType;
		[JsonProperty(PropertyName = "SzBattleTypeName",Required=Required.Default)]
		public string SzBattleTypeName;
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
	}
}
