using UnityEngine;
using System.Collections;

using System;

public class Battle
{
	public int IdBattle;
	public int IdBattleType;
	public BattleType battleType;

	public DateTime DtDateBattle;

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

public class PlayerBattle : MonoBehaviour
{
	public int IdPlayerBattle;

	public int IdPlayer;
	public int IdBattle;
	public Battle battle;

	public int BlWin;
	//public int NrUnitKillsScore;
	//public int NrUnitKilledScore;
	//public int NrUnitCreatedScore;
	//public int NrBuildingCreatedScore;
	//public int NrBuildingKillsScore;
	//public int NrBuildingKilledScore;
	//public int NrGatheredResources;
	//public int NrAverageUnusedResources;
	//public int NrWorkersCreated;

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
