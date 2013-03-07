using UnityEngine;
using System.Collections;

[System.Serializable]
public class Team
{
	public string name;
	public Color color = Color.white;
	public Texture2D colorTexture;
	public Transform initialPosition;
}



public class GameplayManager : MonoBehaviour
{
	
	public Team[] teams;
	
	public int MyTeam {get; protected set;}
	
	// Resources
	public ResourcesManager resources;
	
	public void Init ()
	{
		if (!PhotonNetwork.offlineMode)
		{
			MyTeam = (int)PhotonNetwork.player.customProperties["team"];
		}
		else
		{
			MyTeam = 0;
		}
		
		for (int i = 0; i != teams.Length; i++)
		{
			if (MyTeam == i)
			{
				Camera.mainCamera.transform.position = teams[i].initialPosition.position;
			}
		}
	}
	
	public Color GetColorTeam (int teamID)
	{
		if (teamID < teams.Length) return teams[teamID].color;
		else
		{
			Debug.LogError ("Team ID not exist. ID: " + teamID + ". Number of teams: " + teams.Length);
			return Color.black;
		}
	}
	
	public bool IsSameTeam (int team)
	{
		return team == MyTeam;
	}
	
	public bool IsSameTeam (Unit soldier)
	{
		return soldier.Team == MyTeam;
	}
	
	public bool IsSameTeam (FactoryBase factory)
	{
		return factory.Team == MyTeam;
	}
	
	// TODO: Mostrando só os valores na tela
	
	void OnGUI ()
	{
		GUI.Label (new Rect(10, 10, 110, 50), "Pedra foderosa:");
		GUI.Label (new Rect(110, 10, 150, 50), resources.NumberOfRocks.ToString ());
	}
}