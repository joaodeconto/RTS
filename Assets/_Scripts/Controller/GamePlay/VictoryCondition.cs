using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VictoryCondition 
{	
	[System.Serializable ()]
	public class Challenge
	{
		public enum EnumValue { LessThan, SameThan, GreaterThan }
	
		public string Name;
		public string FirstProperty;
		public EnumValue enumValue;
		public string SecondProperty;
	}
	
	public Challenge[] ChallengesToWin;
	
	private Achieve GameAchievesToWin = new Achieve ();
	
	public void Init ()
	{
		foreach (Challenge ch in ChallengesToWin)
		{
			GameAchievesToWin.DefineAchievement (ch.Name, new List <string> (){ ch.FirstProperty, ch.SecondProperty});

			switch (ch.enumValue)
			{
				case Challenge.EnumValue.LessThan: 	  GameAchievesToWin.DefineProperty (ch.Name, 0, Achieve.ACTIVE_IF_LESS_THAN, 0); break;
				case Challenge.EnumValue.SameThan: 	  GameAchievesToWin.DefineProperty (ch.Name, 0, Achieve.ACTIVE_IF_EQUALS_TO, 0); break;
				case Challenge.EnumValue.GreaterThan: GameAchievesToWin.DefineProperty (ch.Name, 0, Achieve.ACTIVE_IF_GREATER_THAN, 0); break;
				default: break;
			}
		}	
	}
	
	public void CheckVictory ()
	{
		//TODO continuar
	}
}
