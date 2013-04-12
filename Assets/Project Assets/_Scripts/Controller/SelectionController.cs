using UnityEngine;
using System.Collections;
using Visiorama;
using Visiorama.Utils;

public class SelectionController : MonoBehaviour
{
	protected TouchController touchController;
	protected TroopController troopController;
	protected FactoryController factoryController;
	protected GameplayManager gameplayManager;
	protected InteractionController interactionController;

	public void Init ()
	{
		touchController       = ComponentGetter.Get<TouchController>();
		troopController       = ComponentGetter.Get<TroopController>();
		factoryController     = ComponentGetter.Get<FactoryController>();
		gameplayManager       = ComponentGetter.Get<GameplayManager>();
		interactionController = ComponentGetter.Get<InteractionController>();
	}

	bool WebPlayerAndPcSelection()
	{
		//EDITOR ou PC
		if ((touchController.touchType != TouchController.TouchType.Ended) ||
			  (touchController.idTouch != TouchController.IdTouch.Id0))
			return true;

		factoryController.DeselectFactory ();
		
		if (touchController.DragOn)
		{
			troopController.DeselectAllSoldiers ();

			Bounds b = touchController.GetTouchBounds();

			//VDebug.DrawCube (b, Color.green);

			//Verificando seleção de unidades
			Unit enemySoldier = null;

			troopController.DeselectAllSoldiers ();
			
			foreach (Unit soldier in troopController.soldiers)
			{
				if (!gameplayManager.IsSameTeam (soldier))
				{
					if (troopController.selectedSoldiers.Count != 0 || enemySoldier != null)
						continue;
				}

				if (soldier.collider == null)
				{
					Debug.Log("soldado sem colisor!");
					Debug.Break();
				}

				if (b.Intersects (soldier.collider.bounds))
				{
					if (!gameplayManager.IsSameTeam (soldier))
						enemySoldier = soldier;
					else
					{
						troopController.SelectSoldier (soldier, true);
					}
				}
//				else
//					troopController.SelectSoldier (soldier, false);
			}
			
			//Verificando se foram selecionadas unidades
			if (troopController.selectedSoldiers.Count != 0)
			{
				troopController.PlaySelectSound ();
				return true;
			}

			if (enemySoldier != null)
			{
				troopController.SelectSoldier (enemySoldier, true);
				return true;
			}

			foreach (FactoryBase factory in factoryController.factorys)
			{
				if (factory.collider == null)
				{
					Debug.Log("estrutura sem colisor!");
					Debug.Break();
				}

//				if (b.Intersects (factory.collider.bounds))
				if (touchController.GetDragRect ().Contains (factory.transform.position))
				{
					factoryController.SelectFactory (factory);
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
					troopController.DeselectAllSoldiers ();
					troopController.SelectSoldier (selectedUnit, true);
					return true;
				}
				else //return true
				{
					if (Input.GetKey (KeyCode.LeftControl)) //return true
					{
						troopController.DeselectAllSoldiers ();

						Unit.UnitType category = selectedUnit.category;
						foreach (Unit soldier in troopController.soldiers)
						{
							if (gameplayManager.IsSameTeam (soldier.team))
							{
								//TODO pegar somente da mesma categoria dentro da tela
								if (soldier.category == category)
								{
									troopController.SelectSoldier (soldier, true);
								}
							}
						}
						return true; //selecionou unidades da mesma categoria da unidade selecionada
					}

					if (Input.GetKey (KeyCode.LeftShift))
					{
						troopController.SoldierToogleSelection (selectedUnit);
					}
					else
					{
						troopController.DeselectAllSoldiers ();
						troopController.SelectSoldier (selectedUnit, true);
						troopController.PlaySelectSound ();
					}
				}
				return true;
			}

			if(!interactionController.HaveCallbacksForTouchId(TouchController.IdTouch.Id0))
				troopController.DeselectAllSoldiers ();

			if (hit.transform.CompareTag ("Factory"))
			{
				factoryController.SelectFactory (hit.transform.GetComponent<FactoryBase>());
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
			factoryController.DeselectFactory ();
			troopController.DeselectAllSoldiers ();
			return true;
		}
		if (touchController.idTouch == TouchController.IdTouch.Id0) //return
		{
			if (touchController.DragOn)
			{
				factoryController.DeselectFactory ();
				troopController.DeselectAllSoldiers ();

				Bounds b = touchController.GetTouchBounds();
				Unit enemySoldier = null;

				foreach (Unit soldier in troopController.soldiers)
				{
					if (!gameplayManager.IsSameTeam (soldier))
					{
						continue;
					}

					if (soldier.collider == null)
					{
						Debug.Log("soldado sem colisor!");
						Debug.Break();
					}

					if (b.Intersects (soldier.collider.bounds))
					{
						troopController.SelectSoldier (soldier, true);
					}
					else
						troopController.SelectSoldier (soldier, false);
				}

				if (troopController.selectedSoldiers.Count != 0) return true;
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
							factoryController.DeselectFactory ();
							troopController.DeselectAllSoldiers ();
							troopController.SelectSoldier (selectedUnit, true);
							return true;
						}
					}

//					if(!interactionController.HaveCallbacksForTouchId(TouchController.IdTouch.Id0))
//						troopController.DeselectAllSoldiers ();
					
					if (hit.transform.CompareTag ("Factory"))
					{
						factoryController.DeselectFactory ();
						FactoryBase factory = hit.transform.GetComponent<FactoryBase>();
						if (!troopController.WorkerCheckFactory (factory))
						{
							troopController.DeselectAllSoldiers ();
							factoryController.SelectFactory (factory);
						}
						else
							troopController.DeselectAllSoldiers ();
						return true;
					}
				}
				
				if (factoryController.selectedFactory == null)
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
			troopController.CreateGroup (0);
		}
		if (Input.GetKeyDown (KeyCode.Keypad1))
		{
			troopController.CreateGroup (1);
		}
		if (Input.GetKeyDown (KeyCode.Keypad2))
		{
			troopController.CreateGroup (2);
		}
		if (Input.GetKeyDown (KeyCode.Keypad3))
		{
			troopController.CreateGroup (3);
		}
		if (Input.GetKeyDown (KeyCode.Keypad4))
		{
			troopController.CreateGroup (4);
		}
		if (Input.GetKeyDown (KeyCode.Keypad5))
		{
			troopController.CreateGroup (5);
		}
		if (Input.GetKeyDown (KeyCode.Keypad6))
		{
			troopController.CreateGroup (6);
		}
		if (Input.GetKeyDown (KeyCode.Keypad7))
		{
			troopController.CreateGroup (7);
		}
		if (Input.GetKeyDown (KeyCode.Keypad8))
		{
			troopController.CreateGroup (8);
		}
		if (Input.GetKeyDown (KeyCode.Keypad9))
		{
			troopController.CreateGroup (9);
		}
#endif
#if !UNITY_IPHONE && !UNITY_ANDROID
		bool leftCtrl = Input.GetKey(KeyCode.LeftControl);
		
		if (leftCtrl)
		{
			if (Input.GetKeyDown (KeyCode.Alpha0))
			{
				troopController.CreateGroup (0);
			}
			if (Input.GetKeyDown (KeyCode.Alpha1))
			{
				troopController.CreateGroup (1);
			}
			if (Input.GetKeyDown (KeyCode.Alpha2))
			{
				troopController.CreateGroup (2);
			}
			if (Input.GetKeyDown (KeyCode.Alpha3))
			{
				troopController.CreateGroup (3);
			}
			if (Input.GetKeyDown (KeyCode.Alpha4))
			{
				troopController.CreateGroup (4);
			}
			if (Input.GetKeyDown (KeyCode.Alpha5))
			{
				troopController.CreateGroup (5);
			}
			if (Input.GetKeyDown (KeyCode.Alpha6))
			{
				troopController.CreateGroup (6);
			}
			if (Input.GetKeyDown (KeyCode.Alpha7))
			{
				troopController.CreateGroup (7);
			}
			if (Input.GetKeyDown (KeyCode.Alpha8))
			{
				troopController.CreateGroup (8);
			}
			if (Input.GetKeyDown (KeyCode.Alpha9))
			{
				troopController.CreateGroup (9);
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
		bool hasGroup = troopController.SelectGroup (numberOfGroup);

		if (!hasGroup) return;
		
		factoryController.DeselectFactory ();		
		
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
