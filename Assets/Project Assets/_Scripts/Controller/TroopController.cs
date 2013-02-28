using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama.Utils;

using Visiorama;

public class TroopController : MonoBehaviour
{
	public const int MAX_NUMBER_OF_GROUPS = 9;
	
	public bool keepFormation {get; set;}

	internal List<Unit> soldiers = new List<Unit> ();
	internal List<Unit> selectedSoldiers;
	
	internal Dictionary<int, List<Unit>> troopGroups = new Dictionary<int, List<Unit>>();

	protected bool enemySelected = false;
	protected Vector3 centerOfTroop;

	protected GameplayManager gameplayManager;

	public void Init ()
	{
		gameplayManager = ComponentGetter.Get<GameplayManager> ();

		selectedSoldiers = new List<Unit> ();
		
		keepFormation = true;
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
		ComponentGetter.Get<MiniMapController> ().AddUnit (soldier.transform, soldier.Team);
	}

	public void RemoveSoldier (Unit soldier)
	{
		if (selectedSoldiers.Contains (soldier))
		{
			selectedSoldiers.Remove (soldier);
		}

		ComponentGetter.Get<MiniMapController> ().RemoveUnit (soldier.transform, soldier.Team);
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
	
	public void CreateGroup (int numberGroup)
	{
		if (selectedSoldiers.Count != 0)
		{
			if (troopGroups.Count != 0)
			{
				foreach (KeyValuePair<int, List<Unit>> group in troopGroups)
				{
					if (group.Key == numberGroup)
					{
						foreach (Unit soldier in group.Value)
						{
							soldier.Group = -1;
						}
						group.Value.Clear ();
						break;
					}
				}
			}
			
			if (!troopGroups.ContainsKey (numberGroup))
			{
				troopGroups.Add (numberGroup, new List<Unit>());
			}
			
			foreach (Unit soldier in selectedSoldiers)
			{
				if (soldier.Group != numberGroup)
				{
					if (soldier.Group != -1)
					{
						foreach (KeyValuePair<int, List<Unit>> group in troopGroups)
						{
							if (group.Key == soldier.Group)
							{
								group.Value.Remove (soldier);
							}
							break;
						}
					}
					soldier.Group = numberGroup;
					troopGroups[numberGroup].Add (soldier);
				}
			}
		}
		else VDebug.LogError ("Hasn't unit selected.");
	}
	
	public void SelectGroup (int numberGroup)
	{
		if (troopGroups.Count == 0) return;
		
		foreach (KeyValuePair<int, List<Unit>> group in troopGroups)
		{
			if (group.Key == numberGroup)
			{
				DeselectAllSoldiers ();
				
				Debug.Log ("selectedSoldiers.Count: " + group.Value.Count);
				foreach (Unit soldier in group.Value)
				{
					SelectSoldier (soldier, true);
				}
				break;
			}
		}
//		DeselectAllSoldiers ();
//		
//		foreach (Unit soldier in soldiers)
//		{
//			if (gameplayManager.IsSameTeam (soldier))
//			{
//				if (soldier.Group == numberGroup)
//				{
//					SelectSoldier (soldier, true);
//				}
//			}
//		}
	}
	
	public Unit FindUnit (string name)
	{
		foreach (Unit unit in soldiers)
		{
			if (unit.name.Equals(name))
			{
				return unit;
			}
		}
		
		return null;
	}

	//TODO Só para testes
//	void OnGUI ()
//	{
//		GUILayout.BeginHorizontal ();
//		GUILayout.Space (10f);
//		GUI.color = Color.red;
//		keepFormation = GUILayout.Toggle (keepFormation, "Keep Formation");
//		GUILayout.EndHorizontal ();
//	}

	void OrganizeUnits()
	{
		soldiers.Sort((unit1, unit2) =>
		{
			return unit1.transform.position.x.CompareTo(unit2.transform.position.x);
		});
	}

}
