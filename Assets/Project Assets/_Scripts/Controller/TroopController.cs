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

		keepFormation = false;
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
			int i = 0;
			int k = 1;
			foreach (Unit soldier in selectedSoldiers)
			{
				if (soldier != null)
				{
					soldier.TargetingEnemy (null);

					//Vector3 newDestination = destination + (Random.insideUnitSphere * soldier.pathfind.radius * selectedSoldiers.Count);
					Vector3 newDestination = destination;
					if (i != 0)
					{
						if (i-1 % 3 == 0)
						{
							newDestination += Vector3.left * soldier.pathfind.radius * k;
						}
						if (i-1 % 3 == 1)
						{
							newDestination += Vector3.right * soldier.pathfind.radius * k;
						}
						if (i-1 % 3 == 2)
						{
							newDestination += Vector3.back * soldier.pathfind.radius * k;
							k++;
						}
					}

					soldier.Move (newDestination);

					i++;
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
		ComponentGetter.Get<FogOfWar> ().AddEntity (soldier.transform, soldier);
	}

	public void RemoveSoldier (Unit soldier)
	{
		if (selectedSoldiers.Contains (soldier))
		{
			soldier.Deselect ();
			selectedSoldiers.Remove (soldier);
		}

		ComponentGetter.Get<MiniMapController> ().RemoveUnit (soldier.transform, soldier.Team);
		ComponentGetter.Get<FogOfWar> ().RemoveEntity (soldier.transform, soldier);
		soldiers.Remove (soldier);
	}

	public void SelectSoldier (Unit soldier, bool select)
	{
		if(!soldier.IsVisible)
			return;

		if (select)
		{
			enemySelected = !gameplayManager.IsSameTeam (soldier);
			selectedSoldiers.Add (soldier);

			soldier.Select ();
		}
		else
		{
			selectedSoldiers.Remove (soldier);
			soldier.Deselect ();
		}
	}

	public void DeselectAllSoldiers ()
	{
		foreach (Unit soldier in selectedSoldiers)
		{
			if (soldier != null)
			{
				soldier.Deselect ();
			}
		}

		selectedSoldiers.Clear ();
		hudController.DestroyInspector();
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
						foreach (int key in troopGroups.Keys)
						{
							if (key == soldier.Group)
							{
								troopGroups[key].Remove (soldier);
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

	public bool SelectGroup (int numberGroup)
	{
		if (troopGroups.Count == 0) return false;

		foreach (KeyValuePair<int, List<Unit>> group in troopGroups)
		{
			if (group.Key == numberGroup)
			{
				DeselectAllSoldiers ();

				foreach (Unit soldier in group.Value)
				{
					SelectSoldier (soldier, true);
				}
				return true;
				break;
			}
		}

		return false;
	}

	public Unit FindUnit (string name)
	{
		int i = 0;
		foreach (Unit unit in soldiers)
		{
			if (unit == null)
				soldiers.RemoveAt (i);

			if (unit.name.Equals(name))
			{
				return unit;
			}
			i++;
		}

		return null;
	}

	public void ChangeVisibility (Unit soldier, bool visibility)
	{
		ComponentGetter.Get<MiniMapController> ().SetVisibilityUnit (soldier.transform, soldier.Team, visibility);
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
