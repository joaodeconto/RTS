using UnityEngine;
using System.Collections;
using Visiorama;
using Visiorama.Utils;

public class SelectionController : MonoBehaviour
{
	protected TouchController touchController;
	protected StatsController statsController;
	protected GameplayManager gameplayManager;
	protected InteractionController interactionController;

	public void Init ()
	{
		touchController       = ComponentGetter.Get<TouchController>();
		statsController       = ComponentGetter.Get<StatsController>();
		gameplayManager       = ComponentGetter.Get<GameplayManager>();
		interactionController = ComponentGetter.Get<InteractionController>();
	}

	bool WebPlayerAndPcSelection()
	{
		//EDITOR ou PC
		if ((touchController.touchType != TouchController.TouchType.Ended) ||
			  (touchController.idTouch != TouchController.IdTouch.Id0))
			return true;

		if (touchController.DragOn)
		{
			
			statsController.DeselectAllStats ();

			Bounds b = touchController.GetTouchBounds();

			//VDebug.DrawCube (b, Color.green);

			foreach (IStats stat in statsController.myStats)
			{
				Unit unit = stat as Unit;
				
				if (unit == null) continue;
				
				if (unit.collider == null)
				{
					Debug.Log("soldado sem colisor!");
					Debug.Break();
				}

				if (b.Intersects (unit.collider.bounds))
				{
					statsController.SelectStat (unit, true);
				}
			}
			
			//Verificando se foram selecionadas unidades
			if (statsController.selectedStats.Count != 0)
			{
				statsController.PlaySelectSound ();
				return true;
			}

			foreach (IStats stat in statsController.myStats)
			{
				FactoryBase factory = stat as FactoryBase;
				
				if (factory == null) continue;
				
				if (factory.collider == null)
				{
					Debug.Log("estrutura sem colisor!");
					Debug.Break();
				}

//				if (touchController.GetDragRect ().Contains (factory.transform.position))
				if (b.Intersects (factory.collider.bounds))
				{
					statsController.SelectStat (factory, true);
				}
			}
			
			foreach (IStats stat in statsController.otherStats)
			{
				if (b.Intersects (stat.collider.bounds))
				{
					statsController.SelectStat (stat, true);
					break;
				}
			}
		}
		else
		{
			RaycastHit hit;

			if (!Physics.Raycast (touchController.GetFinalRay, out hit))
			{
				return true;
			}

			if (hit.transform.CompareTag ("Unit")) //return true
			{
				Unit selectedUnit = hit.transform.GetComponent<Unit> ();
				if (!gameplayManager.IsSameTeam (selectedUnit)) //return true
				{
					statsController.DeselectAllStats ();
					statsController.SelectStat (selectedUnit, true);
					return true;
				}
				else //return true
				{
					if (Input.GetKey (KeyCode.LeftControl)) //return true
					{
						statsController.DeselectAllStats ();

						Unit.UnitType category = selectedUnit.category;
						foreach (Unit soldier in statsController.myStats)
						{
							if (gameplayManager.IsSameTeam (soldier.team))
							{
								//TODO pegar somente da mesma categoria dentro da tela
								if (soldier.category == category)
								{
									statsController.SelectStat (soldier, true);
								}
							}
						}
						return true; //selecionou unidades da mesma categoria da unidade selecionada
					}

					if (Input.GetKey (KeyCode.LeftShift))
					{
						statsController.ToogleSelection (selectedUnit);
					}
					else
					{
						statsController.DeselectAllStats ();
						statsController.SelectStat (selectedUnit, true);
						statsController.PlaySelectSound ();
					}
				}
				return true;
			}

			if(!interactionController.HaveCallbacksForTouchId(TouchController.IdTouch.Id0))
				statsController.DeselectAllStats ();
			
			if (hit.transform.CompareTag ("Factory"))
			{
				statsController.SelectStat (hit.transform.GetComponent<FactoryBase>(), true);
				return true;
			}
		}
		return false;
	}

	bool AndroidAndIphoneSelection()
	{
		if (touchController.touchType != TouchController.TouchType.Ended) //return
			return true;

		if (!touchController.DragOn && touchController.idTouch == TouchController.IdTouch.Id1) //return
		{
			statsController.DeselectAllStats ();
			return true;
		}
		if (touchController.idTouch == TouchController.IdTouch.Id0) //return
		{
			if (touchController.DragOn)
			{
				statsController.DeselectAllStats ();

				Bounds b = touchController.GetTouchBounds();

				foreach (IStats stat in statsController.myStats)
				{
					Unit unit = stat as Unit;
					
					if (unit == null)
					{
						continue;
					}

					if (unit.collider == null)
					{
						Debug.Log("soldado sem colisor!");
						Debug.Break();
					}

					if (b.Intersects (unit.collider.bounds))
					{
						statsController.SelectStat (unit, true);
					}
				}

				if (statsController.selectedStats.Count != 0)
				{
					statsController.PlaySelectSound ();
					return true;
				}
				
				foreach (IStats stat in statsController.otherStats)
				{
					if (b.Intersects (stat.collider.bounds))
					{
						statsController.SelectStat (stat, true);
						break;
					}
				}
			}
			else
			{
				RaycastHit hit;

				if (Physics.Raycast (touchController.GetFinalRay, out hit))
				{
					if (hit.transform.CompareTag ("Unit"))
					{
						Unit selectedUnit = hit.transform.GetComponent<Unit> ();
						
						if (gameplayManager.IsSameTeam (selectedUnit))
						{
							statsController.DeselectAllStats ();
							statsController.SelectStat (selectedUnit, true);
							statsController.PlaySelectSound ();
							return true;
						}
					}

//					if(!interactionController.HaveCallbacksForTouchId(TouchController.IdTouch.Id0))
//						troopController.DeselectAllSoldiers ();
					
					if (hit.transform.CompareTag ("Factory"))
					{
						FactoryBase factory = hit.transform.GetComponent<FactoryBase>();
						
						if (gameplayManager.IsSameTeam (factory))
						{
							if (!statsController.WorkerCheckFactory (factory))
							{
								statsController.DeselectAllStats ();
								statsController.SelectStat (factory, true);
							}
							else
								statsController.DeselectAllStats ();
							
							return true;
						}
					}
				}
				
				interactionController.Interaction (touchController.GetFinalRaycastHit.transform);
			}
		}
		
		return false;
	}

	void Update ()
	{
#if (!UNITY_IPHONE && !UNITY_ANDROID) || UNITY_EDITOR
		WebPlayerAndPcSelection();
#else
		AndroidAndIphoneSelection();
#endif

#if UNITY_EDITOR
		if (Input.GetKeyDown (KeyCode.Keypad0))
		{
			statsController.CreateGroup (0);
		}
		if (Input.GetKeyDown (KeyCode.Keypad1))
		{
			statsController.CreateGroup (1);
		}
		if (Input.GetKeyDown (KeyCode.Keypad2))
		{
			statsController.CreateGroup (2);
		}
		if (Input.GetKeyDown (KeyCode.Keypad3))
		{
			statsController.CreateGroup (3);
		}
		if (Input.GetKeyDown (KeyCode.Keypad4))
		{
			statsController.CreateGroup (4);
		}
		if (Input.GetKeyDown (KeyCode.Keypad5))
		{
			statsController.CreateGroup (5);
		}
		if (Input.GetKeyDown (KeyCode.Keypad6))
		{
			statsController.CreateGroup (6);
		}
		if (Input.GetKeyDown (KeyCode.Keypad7))
		{
			statsController.CreateGroup (7);
		}
		if (Input.GetKeyDown (KeyCode.Keypad8))
		{
			statsController.CreateGroup (8);
		}
		if (Input.GetKeyDown (KeyCode.Keypad9))
		{
			statsController.CreateGroup (9);
		}
#endif
#if !UNITY_IPHONE && !UNITY_ANDROID
		bool leftCtrl = Input.GetKey(KeyCode.LeftControl);
		
		if (leftCtrl)
		{
			if (Input.GetKeyDown (KeyCode.Alpha0))
			{
				statsController.CreateGroup (0);
			}
			if (Input.GetKeyDown (KeyCode.Alpha1))
			{
				statsController.CreateGroup (1);
			}
			if (Input.GetKeyDown (KeyCode.Alpha2))
			{
				statsController.CreateGroup (2);
			}
			if (Input.GetKeyDown (KeyCode.Alpha3))
			{
				statsController.CreateGroup (3);
			}
			if (Input.GetKeyDown (KeyCode.Alpha4))
			{
				statsController.CreateGroup (4);
			}
			if (Input.GetKeyDown (KeyCode.Alpha5))
			{
				statsController.CreateGroup (5);
			}
			if (Input.GetKeyDown (KeyCode.Alpha6))
			{
				statsController.CreateGroup (6);
			}
			if (Input.GetKeyDown (KeyCode.Alpha7))
			{
				statsController.CreateGroup (7);
			}
			if (Input.GetKeyDown (KeyCode.Alpha8))
			{
				statsController.CreateGroup (8);
			}
			if (Input.GetKeyDown (KeyCode.Alpha9))
			{
				statsController.CreateGroup (9);
			}
		}
		else
		{
			if (Input.GetKeyDown (KeyCode.Alpha0))
			{
				SelectGroup (0);
			}
			if (Input.GetKeyDown (KeyCode.Alpha1))
			{
				SelectGroup (1);
			}
			if (Input.GetKeyDown (KeyCode.Alpha2))
			{
				SelectGroup (2);
			}
			if (Input.GetKeyDown (KeyCode.Alpha3))
			{
				SelectGroup (3);
			}
			if (Input.GetKeyDown (KeyCode.Alpha4))
			{
				SelectGroup (4);
			}
			if (Input.GetKeyDown (KeyCode.Alpha5))
			{
				SelectGroup (5);
			}
			if (Input.GetKeyDown (KeyCode.Alpha6))
			{
				SelectGroup (6);
			}
			if (Input.GetKeyDown (KeyCode.Alpha7))
			{
				SelectGroup (7);
			}
			if (Input.GetKeyDown (KeyCode.Alpha8))
			{
				SelectGroup (8);
			}
			if (Input.GetKeyDown (KeyCode.Alpha9))
			{
				SelectGroup (9);
			}
		}
#endif
	}
	
	float tempTime = -1f;
	
	void SelectGroup (int numberOfGroup)
	{
		bool hasGroup = statsController.SelectGroup (numberOfGroup);

		if (!hasGroup) return;
		
//		if (Time.time - tempTime < 1f)
//		{
//			Vector3 getPosition = troopController.selectedSoldiers[0].transform.position - Vector3.forward * touchController.mainCamera.orthographicSize;
//			getPosition = touchController.mainCamera.GetComponent<CameraBounds> ().ClampScenario (getPosition);
//	
//			touchController.mainCamera.transform.position = getPosition;
//		}
//		
//		tempTime = Time.time;
	}

	void OnGUI ()
	{
		if (touchController.DragOn && touchController.idTouch == TouchController.IdTouch.Id0)
		{
			GUI.Box (touchController.GetDragRect (), "");
		}
	}

	Plane[] CalculateRect (Rect r)
	{
		var c = Camera.mainCamera;

		// Project the rectangle into world space
		var c0 = c.ScreenToWorldPoint(new Vector3(r.xMin, r.yMin, c.nearClipPlane));
		var c1 = c.ScreenToWorldPoint(new Vector3(r.xMin, r.yMax, c.nearClipPlane));
		var c2 = c.ScreenToWorldPoint(new Vector3(r.xMax, r.yMin, c.nearClipPlane));
		var c3 = c.ScreenToWorldPoint(new Vector3(r.xMax, r.yMax, c.nearClipPlane));

		var c4 = c.ScreenToWorldPoint(new Vector3(r.xMin, r.yMin, c.farClipPlane));
		var c5 = c.ScreenToWorldPoint(new Vector3(r.xMax, r.yMax, c.farClipPlane));

		Plane[] planes = new Plane[4];
		// Define the planes of the rectangle projected into the world
		planes[0] = new Plane(c0, c4, c2);
		planes[1] = new Plane(c2, c5, c3);
		planes[2] = new Plane(c3, c5, c1);
		planes[3] = new Plane(c1, c4, c0);

		return planes;
	}
}
