using UnityEngine;
using System.Collections;

public class ScorePlayer : MonoBehaviour
{
	public int GoldCollectedPoints { get; private set; }
	public int ManaCollectedPoints { get; private set; }
	public int ResourcesSpentPoints { get; private set; }
	public int UnitsCreatedPoints { get; private set; }
	public int UnitsLostPoints { get; private set; }
	public int UnitsKillsPoints { get; private set; }
	public int StructureCreatedPoints { get; private set; }
	public int StructureLostPoints { get; private set; }
	public int StructureDestroyedPoints { get; private set; }
	public int VictoryPoints { get; private set; }
	public int TotalTimeElapsed { get; private set; }
	public int UpgradePoints { get; private set; }
	
	public int XUnitsCreatedPoints { get; private set; }
	public int XUnitsLostPoints { get; private set; }
	public int XUnitsKillsPoints { get; private set; }
	public int XStructureCreatedPoints { get; private set; }
	public int XStructureLostPoints { get; private set; }
	public int XStructureDestroyedPoints { get; private set; }
	public int XUpgradePoints { get; private set; }
	
	public int TotalScore
	{ 
		get
		{
			return (GoldCollectedPoints + 
			        ManaCollectedPoints + 
			        XUnitsCreatedPoints + 
			        XUnitsKillsPoints*3 +
			        XStructureCreatedPoints +
			        XStructureDestroyedPoints*3 +
			        XUpgradePoints + (VictoryPoints * 1000))/10;
		}
	}
	
	public ScorePlayer ()
	{
		GoldCollectedPoints      	= 0;
		ManaCollectedPoints      	= 0;
		ResourcesSpentPoints     	= 0;
		UnitsCreatedPoints     	 	= 0;
		UnitsLostPoints 		 	= 0;
		UnitsKillsPoints     	 	= 0;
		StructureCreatedPoints   	= 0;
		StructureLostPoints 	 	= 0;
		StructureDestroyedPoints 	= 0;
		VictoryPoints            	= 0;
		TotalTimeElapsed         	= 0;
		UpgradePoints			 	= 0;
		XUnitsCreatedPoints     	= 0;
		XUnitsLostPoints 		 	= 0;
		XUnitsKillsPoints     		= 0;
		XStructureCreatedPoints   	= 0;
		XStructureLostPoints 	 	= 0;
		XStructureDestroyedPoints 	= 0;
		XUpgradePoints			 	= 0;
	}
	
	public void AddScorePlayer (string scoreName, int points)
	{
		if (scoreName.Equals (DataScoreEnum.GoldGathered))				GoldCollectedPoints += points;
		else if (scoreName.Equals (DataScoreEnum.ManaGathered))			ManaCollectedPoints += points;
		else if (scoreName.Equals (DataScoreEnum.UnitsCreated))			UnitsCreatedPoints += points;
		else if (scoreName.Equals (DataScoreEnum.UnitsKilled))			UnitsKillsPoints += points;
		else if (scoreName.Equals (DataScoreEnum.UnitsLost))			UnitsLostPoints += points;
		else if (scoreName.Equals (DataScoreEnum.BuildingsCreated))		StructureCreatedPoints += points;
		else if (scoreName.Equals (DataScoreEnum.BuildingsDestroyed))	StructureDestroyedPoints += points;
		else if (scoreName.Equals (DataScoreEnum.BuildingsLost)) 		StructureLostPoints += points;
		else if (scoreName.Equals (DataScoreEnum.Victory))				VictoryPoints += points;
		else if (scoreName.Equals (DataScoreEnum.Defeat))				VictoryPoints -= points;
		else if (scoreName.Equals (DataScoreEnum.TotalTimeElapsed))		TotalTimeElapsed += points;
		else if (scoreName.Equals (DataScoreEnum.UpgradesCreated))		UpgradePoints += points;  
		else if (scoreName.Contains (DataScoreEnum.XCreated)) 			XUnitsCreatedPoints += points;
		else if (scoreName.Contains (DataScoreEnum.XKilled)) 			XUnitsKillsPoints += points;
		else if (scoreName.Contains (DataScoreEnum.XUnitLost)) 			XUnitsLostPoints += points;
		else if (scoreName.Contains (DataScoreEnum.XBuilt)) 			XStructureCreatedPoints += points;
		else if (scoreName.Contains (DataScoreEnum.XDestroyed)) 		XStructureDestroyedPoints += points;
		else if (scoreName.Contains (DataScoreEnum.XBuildLost)) 		XStructureLostPoints += points;
		else if (scoreName.Contains (DataScoreEnum.XUpgraded)) 			XUpgradePoints += points;
	}	
}
