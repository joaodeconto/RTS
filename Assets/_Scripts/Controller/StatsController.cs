using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Visiorama.Utils;

using Visiorama;

public class StatsController : MonoBehaviour
{
	public const int MAX_NUMBER_OF_GROUPS = 9;
	public const string buttonIdleWorkersName = "IdleWorkers";
	
	public enum StatsTypeSelected
	{
		None,
		Unit,
		Factory
	}

	public StatsTypeSelected statsTypeSelected {get; protected set;}
	
	public bool keepFormation {get; set;}

	public Vector3 idleButtonPosition;
	public List<IStats> myStats = new List<IStats> ();
	public List<IStats> otherStats = new List<IStats> ();
	
	internal List<IStats> selectedStats;

	internal Dictionary<int, List<IStats>> statsGroups = new Dictionary<int, List<IStats>>();

	protected bool otherSelected = false;
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

		selectedStats 	= new List<IStats> ();
		idleWorkers     = new List<Worker>();

		keepFormation = false;
		
		statsTypeSelected = StatsTypeSelected.None;

		InvokeRepeating("CheckWorkersInIdle",1.0f,1.0f);
//		InvokeRepeating("OrganizeUnits",1.0f,1.0f);
	}

	public void MoveTroop (Vector3 destination)
	{
		if (otherSelected) return;
		
		int i = 0;
		int k = 1;
		
		bool feedback = false;
		
		foreach (IStats stat in selectedStats)
		{
			Unit unit = stat as Unit;
			
			if (unit == null) continue;
			
			if (keepFormation)
			{
				centerOfTroop = Math.CenterOfObjects (selectedStats.ToArray ());
					
				unit.TargetingEnemy (null);

				Vector3 t = centerOfTroop - unit.transform.position;
				unit.Move (destination - t);
				
				feedback = true;
			}
			else
			{
				unit.TargetingEnemy (null);

				//Vector3 newDestination = destination + (Random.insideUnitSphere * soldier.pathfind.radius * selectedSoldiers.Count);
				Vector3 newDestination = destination;
				if (i != 0)
				{
					if (i-1 % 3 == 0)
					{
						newDestination += Vector3.left * unit.GetPathFindRadius * k;
					}
					if (i-1 % 3 == 1)
					{
						newDestination += Vector3.right * unit.GetPathFindRadius * k;
					}
					if (i-1 % 3 == 2)
					{
						newDestination += Vector3.back * unit.GetPathFindRadius * k;
						k++;
					}
				}

				unit.Move (newDestination);
				
				feedback = true;

				i++;
			}
		}
		
		if (feedback)
		

			hudController.CreateFeedback (HUDController.Feedbacks.Move, destination, 1f, gameplayManager.GetColorTeam ());
			soundManager.PlayRandom ("ConfirmMov");
	}

	public void AttackTroop (GameObject enemy)
	{
		if (enemy == null ||
			otherSelected) return;
		
		bool feedback = false;
		
		foreach (IStats stat in selectedStats)
		{
			Unit unit = stat as Unit;
			
			if (unit == null) continue;
			
			unit.TargetingEnemy (enemy);
			
			feedback = true;
		}
		
		if (feedback)
			soundManager.PlayRandom ("Confirm");
			hudController.CreateFeedback (HUDController.Feedbacks.Attack, enemy.transform,
										  enemy.GetComponent<IStats> ().sizeOfSelected, gameplayManager.GetColorTeam(enemy.GetComponent<IStats> ().team));
	}

	public void AddStats (IStats stat)
	{
		Debug.Log ("gameplay is null: " + gameplayManager);

		if (gameplayManager.IsSameTeam (stat.team))
		{
			myStats.Add (stat);
		}
		else
		{
			otherStats.Add (stat);
		}
		
		Unit unit = stat as Unit;
		
		if (unit != null)
		{
			ComponentGetter.Get<MiniMapController> ().AddUnit (stat.transform, stat.team);
			gameplayManager.IncrementUnit (stat.team, unit.numberOfUnits);
		}
		
		FactoryBase factory = stat as FactoryBase;
		
		if (factory != null)
		{
			ComponentGetter.Get<MiniMapController> ().AddStructure (stat.transform, stat.team);
		}
			
		ComponentGetter.Get<FogOfWar> ().AddEntity (stat.transform, stat);
	}

	public void RemoveStats (IStats stat)
	{
		if (selectedStats.Contains (stat))
		{
			stat.Deselect ();
			selectedStats.Remove (stat);
		}
		
		if (stat.GetType() == typeof(Unit))
		{
			Unit unit = stat as Unit;
			
			ComponentGetter.Get<MiniMapController> ().RemoveUnit (unit.transform, unit.team);
			gameplayManager.DecrementUnit (unit.team, unit.numberOfUnits);
		}
		else
		{
			ComponentGetter.Get<MiniMapController> ().RemoveStructure (stat.transform, stat.team);
		}
		
		ComponentGetter.Get<FogOfWar> ().RemoveEntity (stat.transform, stat);
		
		if (gameplayManager.IsSameTeam (stat.team))
		{
			myStats.Remove (stat);
		}
		else
		{
			otherStats.Remove (stat);
		}
	}
	
	public void DestroyAllStatsTeam (int teamID)
	{
		IStats[] allStats;
		
		if (gameplayManager.IsSameTeam (teamID))
		{
			allStats = myStats.ToArray ();
		}
		else
		{
			allStats = (from stat in otherStats
		        where teamID == stat.team
		        select stat).ToArray ();
		}
		
		foreach (IStats stat in allStats)
		{
			stat.SendRemove ();
		}
	}

	public void ToogleSelection (IStats stat)
	{
		SelectStat (stat, !selectedStats.Contains (stat));
	}

	public void SelectStat (IStats stat, bool select)
	{
		if(!stat.IsVisible)
			return;

		if (select)
		{
			if (stat.Selected) return;
			
			otherSelected = !gameplayManager.IsSameTeam (stat);
			selectedStats.Add (stat);
			
			if (selectedStats.Count == 1 &&
				!otherSelected)
			{
				Unit unit = stat as Unit;
				
				if (unit != null)
				{
					statsTypeSelected = StatsTypeSelected.Unit;
				}
				else
				{
					statsTypeSelected = StatsTypeSelected.Factory;
				}
			}
			
			stat.Select ();
		}
		else
		{
			if (!stat.Selected) return;
			
			selectedStats.Remove (stat);
			
			hudController.RemoveEnqueuedButtonInInspector(this.name, Unit.UnitGroupQueueName);
			
			stat.Deselect ();
			
			if (selectedStats.Count == 0) statsTypeSelected = StatsTypeSelected.None;
		}
	}

	public void DeselectAllStats ()
	{
		if (selectedStats.Count == 0) return;
		
		foreach (IStats stat in selectedStats)
		{
			if (stat != null)
			{
				stat.Deselect ();
			}
		}
		
		hudController.DestroyInspector ("all");

		selectedStats.Clear ();
		
		statsTypeSelected = StatsTypeSelected.None;
	}

	public void CreateGroup (int numberGroup)
	{
		if (selectedStats.Count != 0)
		{
			if (statsGroups.Count != 0)
			{
				foreach (KeyValuePair<int, List<IStats>> group in statsGroups)
				{
					if (group.Key == numberGroup)
					{
						foreach (IStats stat in group.Value)
						{
							stat.Group = -1;
						}
						group.Value.Clear ();
						break;
					}
				}
			}

			if (!statsGroups.ContainsKey (numberGroup))
			{
				statsGroups.Add (numberGroup, new List<IStats>());
			}

			foreach (IStats stat in selectedStats)
			{
				if (stat.Group == numberGroup)
					continue;

				if (stat.Group != -1)
				{
					foreach (int key in statsGroups.Keys)
					{
						if (key == stat.Group)
						{
							statsGroups[key].Remove (stat);
						}
						break;
					}
				}
				stat.Group = numberGroup;
				statsGroups[numberGroup].Add (stat);
			}
		}
		else VDebug.LogError ("Hasn't unit selected.");
	}

	public bool SelectGroup (int numberGroup)
	{
		if (statsGroups.Count == 0) return false;

		foreach (KeyValuePair<int, List<IStats>> group in statsGroups)
		{
			if (group.Key == numberGroup)
			{
				DeselectAllStats ();

				foreach (IStats stat in group.Value)
				{
					SelectStat (stat, true);
				}
				return true;
				break;
			}
		}

		return false;
	}
	
	public bool IsUnit (IStats stat)
	{
		Unit unit = stat as Unit;
		
		return unit != null;
	}

	public IStats FindMyStat (string name)
	{
		int i = 0;
		foreach (IStats stat in myStats)
		{
			if (stat == null)
			{
				myStats.RemoveAt (i);
				continue;
			}

			if (stat.name.Equals(name))
			{
				return stat;
			}
			i++;
		}
		
		i = 0;
		foreach (IStats stat in otherStats)
		{
			if (stat == null)
			{
				otherStats.RemoveAt (i);
				continue;
			}

			if (stat.name.Equals(name))
			{
				return stat;
			}
			i++;

		}

		return null;
	}

	public bool WorkerCheckFactory (FactoryBase factory)
	{
		bool feedback = false;

		foreach (IStats stat in selectedStats)
		{
			if (stat.GetType() == typeof(Worker))
			{
				Worker w = stat as Worker;

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
					else if (factory.IsNeededRepair)
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
			hudController.CreateFeedback (HUDController.Feedbacks.Self, factory.transform.position, factory.sizeOfSelected, gameplayManager.GetColorTeam ());

		return feedback;
	}

	public void ChangeVisibility (IStats stat, bool visibility)
	{
		ComponentGetter.Get<MiniMapController> ().SetVisibilityUnit (stat.transform, stat.team, visibility);
	}
	
	public void PlaySelectSound ()
	{
		if (selectedStats.Count == 1)
		{
			IStats statSelected = selectedStats[0];
			
			if (IsUnit (statSelected))
			{
				soundManager.PlayRandom (statSelected.category);
			}
			else
			{
				soundManager.PlayRandom ("BuildingSelected");
			}
		}
		else
		{
			soundManager.PlayRandom ("TroopSelected");
		}
	}

	void OrganizeUnits()
	{
		myStats.Sort((unit1, unit2) =>
		{
			return unit1.transform.position.x.CompareTo(unit2.transform.position.x);
		});
	}

	void CheckWorkersInIdle()
	{
		HUDController hud = ComponentGetter.Get<HUDController>();

		foreach (IStats stat in myStats)
		{
			Worker w = stat as Worker;

			if ( (w == null) || (!gameplayManager.IsSameTeam(w.team)) ) continue;

			idleWorkers.Remove(w);

			switch (w.workerState)
			{
				case Worker.WorkerState.None:
					if (w.unitState == Unit.UnitState.Idle &&
						!selectedStats.Contains(w))
						idleWorkers.Add(w);
					break;
				case Worker.WorkerState.CarryingIdle:
					if (!selectedStats.Contains(w))
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
			ht["counter"] = idleWorkers.Count;
			ht["time"] = 0f;

			hud.CreateOrChangeButtonInInspector(buttonIdleWorkersName,
												idleButtonPosition,
												ht,
												idleWorkers[0].guiTextureName,
												(ht_dcb) =>
												{
													int currentIdleWorker = (int)ht_dcb["currentIdleWorker"];

													if(currentIdleWorker < idleWorkers.Count)
													{
														Vector3 idlePos = idleWorkers[currentIdleWorker].transform.position;
														idlePos.y = 0.0f;

														Math.CenterCameraInObject (Camera.main, idlePos);

														//Deselect anything was selected
														DeselectAllStats();

														SelectStat(idleWorkers[currentIdleWorker], true);

														idleWorkers.RemoveAt(currentIdleWorker);
													}

													if((++currentIdleWorker) >= idleWorkers.Count)
														currentIdleWorker = 0;

													ht_dcb["currentIdleWorker"] = currentIdleWorker;
												},
												(ht_dcb, isDown) => 
												{
													if (isDown)
													{
														ht["time"] = Time.time;
													}
													else
													{
														if (Time.time - (float)ht["time"] > 2f)
														{
															Vector3 pos = Math.CenterOfObjects (idleWorkers.ToArray ());
															
															Math.CenterCameraInObject (Camera.main, pos);
															
															//Deselect anything was selected
															DeselectAllStats();
															
															foreach (Worker iw in idleWorkers)
															{
																SelectStat (iw, true);
															}
										
															idleWorkers.Clear ();
														}
													}
												}
												,
												null,
												null,
												true);
		}
	}
	
	/////// FACTORY CONTROLLER (OBSOLETE)
	/*
	public void AddFactory (FactoryBase factory)
	{
		factorys.Add (factory);
		ComponentGetter.Get<MiniMapController> ().AddStructure (factory.transform, factory.team);
		ComponentGetter.Get<FogOfWar> ().AddEntity (factory.transform, factory);
	}

	public void RemoveFactory (FactoryBase factory)
	{
		factorys.Remove (factory);
		ComponentGetter.Get<MiniMapController> ().RemoveStructure (factory.transform, factory.team);
		ComponentGetter.Get<FogOfWar> ().RemoveEntity (factory.transform, factory);
	}

	public void SelectFactory (FactoryBase factory)
	{
		if(!factory.IsVisible)
			return;

		selectedFactory = factory;
		
		PlaySelectSound ();

		factory.Select ();
	}

	public void DeselectFactory ()
	{
		if (selectedFactory != null)
		{
			if (selectedFactory.Deselect ())
				selectedFactory = null;
		}
	}
	
	public void DestroyFactorysTeam (int teamID)
	{
		FactoryBase[] fcs = (from factory in factorys
	        where teamID == factory.team
	        select factory).ToArray ();
		
		foreach (FactoryBase fc in fcs)
		{
//			gameplayManager.RemoveStatTeamID (teamID);
			StartCoroutine (fc.OnDie ());
		}

	}

	public FactoryBase FindFactory (string name)
	{
		foreach (FactoryBase factory in factorys)
		{
			if (factory.name.Equals(name))
			{
				return factory;
			}
		}

		return null;
	}
	
	public void PlaySelectSound ()
	{
		soundManager.PlayRandom ("BuildingSelected");
	}

	public void ChangeVisibility (FactoryBase factory, bool visibility)
	{
		ComponentGetter.Get<MiniMapController> ().SetVisibilityStructure (factory.transform, factory.team, visibility);
	}
	*/
}
