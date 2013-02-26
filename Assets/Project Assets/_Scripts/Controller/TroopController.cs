using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama.Utils;

public class TroopController : MonoBehaviour
{
	public bool keepFormation {get; set;}
	
	internal List<Unit> soldiers = new List<Unit> ();
	internal List<Unit> selectedSoldiers;
	
	protected bool enemySelected = false;
	protected Vector3 centerOfTroop;
	
	protected GameplayManager gameplayManager;
	
	public void Init ()
	{
		gameplayManager = GameController.GetInstance ().GetGameplayManager ();
		
		selectedSoldiers = new List<Unit> ();
		//InvokeRepeating("OrganizeUnits",1.0f,1.0f);
	}
	
	public void MoveTroop (Vector3 destination)
	{
		if (enemySelected) return;
		
		if (keepFormation)
		{
			centerOfTroop = Math.CenterOfObjects (selectedSoldiers.ToArray ());
			foreach (Unit soldier in selectedSoldiers)
			{
				if (soldier != null)
				{
					soldier.TargetingEnemy (null);
					
					Vector3 t = centerOfTroop - soldier.transform.position;
					soldier.Move (destination - t);
				}
			}
		}
		else
		{
			foreach (Unit soldier in selectedSoldiers)
			{
				if (soldier != null)
				{
					soldier.TargetingEnemy (null);
					
					Vector3 newDestination = destination + (Random.insideUnitSphere * soldier.pathfind.radius * selectedSoldiers.Count);
					
					soldier.Move (newDestination);
				}
			}
		}
	}
	
	public void AttackTroop (GameObject enemy)
	{
		if (enemy == null) return;
		
		foreach (Unit soldier in selectedSoldiers)
		{
			soldier.TargetingEnemy (enemy);
		}
	}
	
	public void AddSoldier (Unit soldier)
	{
		soldiers.Add (soldier);
	}
	
	public void RemoveSoldier (Unit soldier)
	{
		if (selectedSoldiers.Contains (soldier))
		{
			selectedSoldiers.Remove (soldier);
		}
		soldiers.Remove (soldier);
	}
	
	public void SelectSoldier (Unit soldier, bool select)
	{
		if (select)
		{
			enemySelected = !gameplayManager.IsSameTeam (soldier);
			
			selectedSoldiers.Add (soldier);
			
			soldier.Active ();
		}
		else
		{
			selectedSoldiers.Remove (soldier);
			soldier.Deactive ();
		}
	}
	
	public void DeselectAllSoldiers ()
	{
		foreach (Unit soldier in selectedSoldiers)
		{
			if (soldier != null)
			{
				soldier.Deactive ();
			}
		}
		
		selectedSoldiers.Clear ();
		
		foreach (Transform child in HUDRoot.go.transform)
		{
			Destroy (child.gameObject);
		}
	}
	
	//TODO Só para testes
	void OnGUI ()
	{
		GUILayout.BeginHorizontal ();
		GUILayout.Space (10f);
		GUI.color = Color.red;
		keepFormation = GUILayout.Toggle (keepFormation, "Keep Formation");
		GUILayout.EndHorizontal ();
	}
	
	void OrganizeUnits()
	{
		soldiers.Sort((unit1, unit2) =>
		{
			return unit1.transform.position.x.CompareTo(unit2.transform.position.x);
		});
	}
	
}