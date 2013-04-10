using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama.Utils;

using Visiorama;

public class TroopController : MonoBehaviour
{
	public const int MAX_NUMBER_OF_GROUPS = 9;
	public const string buttonIdleWorkersName = "IdleWorkers";

	public bool keepFormation {get; set;}

	public Vector3 idleButtonPosition;
	public List<Unit> soldiers = new List<Unit> ();
	internal List<Unit> selectedSoldiers;

	internal Dictionary<int, List<Unit>> troopGroups = new Dictionary<int, List<Unit>>();

	protected bool enemySelected = false;
	protected Vector3 centerOfTroop;

	protected GameplayManager gameplayManager;
	protected SoundManager soundManager;
	protected HUDController hudController;

	protected List<Worker> idleWorkers;

	public void Init ()
	{
		gameplayManager = ComponentGetter.Get<GameplayManager> ();
		soundManager    = ComponentGetter.Get<SoundManager> ();
		hudController   = ComponentGetter.Get<HUDController> ();

		selectedSoldiers = new List<Unit> ();
		idleWorkers      = new List<Worker>();

		keepFormation = false;

		InvokeRepeating("CheckWorkersInIdle",1.0f,1.0f);
		InvokeRepeating("OrganizeUnits",1.0f,1.0f);
	}

	public void MoveTroop (Vector3 destination)
	{
		if (enemySelected) return;
		
		hudController.CreateFeedback (HUDController.Feedbacks.Move, destination, 1f);

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

		hudController.CreateFeedback (HUDController.Feedbacks.Attack, enemy.transform.position, enemy.GetComponent<IStats> ().sizeOfSelected);
		
		foreach (Unit soldier in selectedSoldiers)
		{
			soldier.TargetingEnemy (enemy);
		}
	}

	public void AddSoldier (Unit soldier)
	{
		soldiers.Add (soldier);
		ComponentGetter.Get<MiniMapController> ().AddUnit (soldier.transform, soldier.team);
		ComponentGetter.Get<FogOfWar> ().AddEntity (soldier.transform, soldier);

		gameplayManager.IncrementUnit (soldier.team, soldier.numberOfUnits);
	}

	public void RemoveSoldier (Unit soldier)
	{
		if (selectedSoldiers.Contains (soldier))
		{
			soldier.Deselect ();
			selectedSoldiers.Remove (soldier);
		}

		ComponentGetter.Get<MiniMapController> ().RemoveUnit (soldier.transform, soldier.team);
		ComponentGetter.Get<FogOfWar> ().RemoveEntity (soldier.transform, soldier);
		soldiers.Remove (soldier);

		gameplayManager.DecrementUnit (soldier.team, soldier.numberOfUnits);
	}

	public void SoldierToogleSelection (Unit soldier)
	{
		SelectSoldier (soldier, !selectedSoldiers.Contains (soldier));
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
				soldier.Deselect (true);
			}
		}

		selectedSoldiers.Clear ();
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
				if (soldier.Group == numberGroup)
					continue;

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
	
	public bool WorkerCheckFactory (FactoryBase factory)
	{
		bool feedback = false;
		
		foreach (Unit unit in selectedSoldiers)
		{
			if (unit.GetType() == typeof(Worker))
			{
				Worker w = unit as Worker;

				if (!factory.wasBuilt)
				{
					w.SetMoveToFactory(factory);
					feedback = true;
				}
				else if (w.hasResource)
				{
					if (factory.receiveResource == w.resource.type)
					{
						w.SetMoveToFactory(factory);
						feedback = true;
					}
					else if (factory.gameObject.GetComponent<MainFactory>() != null)
					{
						w.SetMoveToFactory(factory);
						feedback = true;
					}
				}
				else if (factory.IsNeededRepair)
				{
					w.SetMoveToFactory(factory);
					feedback = true;
				}
			}
		}
		
		if (feedback) 
			hudController.CreateFeedback (HUDController.Feedbacks.Self, factory.transform.position, factory.sizeOfSelected);
		
		return feedback;
	}
	
	public void ChangeVisibility (Unit soldier, bool visibility)
	{
		ComponentGetter.Get<MiniMapController> ().SetVisibilityUnit (soldier.transform, soldier.team, visibility);
	}

	public void PlaySelectSound ()
	{
		if (selectedSoldiers.Count == 1)
		{
			soundManager.PlayRandom ("SoldierSelected");
		}
		else
		{
			soundManager.Play ("TroopSelected");
			soundManager.Play ("StructureSelected");
		}
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

	void CheckWorkersInIdle()
	{
		HUDController hud = ComponentGetter.Get<HUDController>();

		foreach (Unit u in soldiers)
		{
			Worker w = u as Worker;

			if ( (w == null) || (!gameplayManager.IsSameTeam(w.team)) ) continue;

			idleWorkers.Remove(w);

			switch (w.workerState)
			{
				case Worker.WorkerState.None:
					if (w.unitState == Unit.UnitState.Idle &&
						!selectedSoldiers.Contains(w))
						idleWorkers.Add(w);
					break;
				case Worker.WorkerState.CarryingIdle:
					if (!selectedSoldiers.Contains(w))
						idleWorkers.Add(w);
					break;
				default:
					break;
			}
		}

		if(idleWorkers.Count == 0)
		{
			hud.RemoveButtonInInspector(buttonIdleWorkersName);
		}
		else
		{
			Hashtable ht = new Hashtable();

			ht["currentIdleWorker"] = 0;
			ht["counter"] = idleWorkers.Count.ToString();

			hud.CreateOrChangeButtonInInspector(buttonIdleWorkersName,
												idleButtonPosition,
												ht,
												idleWorkers[0].guiTextureName,
												(hud_ht) =>
												{
													int currentIdleWorker = (int)hud_ht["currentIdleWorker"];

													if(currentIdleWorker < idleWorkers.Count)
													{
														Vector3 idlePos = idleWorkers[currentIdleWorker].transform.position;
														Vector3 pos = idlePos;
														pos.y = 0.0f;

														Transform trnsCamera = Camera.main.transform;

														if(!Mathf.Approximately(trnsCamera.localEulerAngles.x, 45))
															Debug.Log("Centralizacao da camera so funciona com a camera em 45 graus");
														trnsCamera.position += pos - (Vector3.right * trnsCamera.position.x)
																				   - (Vector3.forward * trnsCamera.position.z)
																				   - (Vector3.forward
																						* (trnsCamera.position.y - idlePos.y)
																						* Mathf.Tan(trnsCamera.localEulerAngles.x * Mathf.Deg2Rad));

														//Deselect anything was selected
														DeselectAllSoldiers();
														ComponentGetter.Get<FactoryController>().DeselectFactory();

														SelectSoldier(idleWorkers[currentIdleWorker], true);

														idleWorkers.RemoveAt(currentIdleWorker);
													}

													if((++currentIdleWorker) >= idleWorkers.Count)
														currentIdleWorker = 0;

													hud_ht["currentIdleWorker"] = currentIdleWorker;
												},
												null,
												null,
												null,
												true);
		}
	}
}
