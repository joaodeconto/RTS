using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	
	protected TouchController touchController;
	protected SelectionController selectionController;
	protected TroopController troopController;
	protected BuildingController buildingController;
	protected InteractionController interactionController;
	
	void Awake ()
	{
		PhotonNetwork.isMessageQueueRunning = true;
		
		GetTouchController ().Init ();
		GetSelectionController ().Init ();
		GetTroopController ().Init ();
		GetBuildingController ().Init ();
		GetInteractionController ().Init ();
	}
	
	public TouchController GetTouchController ()
	{
		if(this.touchController == null)
		{
			GameObject tc = GetInstance ().transform.FindChild ("TouchController").gameObject;
			if (tc == null)
			{
				tc = new GameObject ("TouchController");
				tc.AddComponent <TouchController> ();
			}

			GameController.AppendController (tc);

			this.touchController = tc.GetComponent <TouchController> ();
		}
		return this.touchController;
	}
	
	public SelectionController GetSelectionController ()
	{
		if(this.selectionController == null)
		{
			GameObject tc = GetInstance ().transform.FindChild ("SelectionController").gameObject;
			if (tc == null)
			{
				tc = new GameObject ("SelectionController");
				tc.AddComponent <SelectionController> ();
			}

			GameController.AppendController (tc);

			this.selectionController = tc.GetComponent <SelectionController> ();
		}
		return this.selectionController;
	}
	
	public TroopController GetTroopController ()
	{
		if(this.troopController == null)
		{
			GameObject tc = GetInstance ().transform.FindChild ("TroopController").gameObject;
			if (tc == null)
			{
				tc = new GameObject ("TroopController");
				tc.AddComponent <TroopController> ();
			}

			GameController.AppendController (tc);

			this.troopController = tc.GetComponent <TroopController> ();
		}
		return this.troopController;
	}
	
	public BuildingController GetBuildingController ()
	{
		if(this.buildingController == null)
		{
			GameObject bc = GetInstance ().transform.FindChild ("BuildingController").gameObject;
			if (bc == null)
			{
				bc = new GameObject ("BuildingController");
				bc.AddComponent <BuildingController> ();
			}

			GameController.AppendController (bc);

			this.buildingController = bc.GetComponent <BuildingController> ();
		}
		return this.buildingController;
	}
	
	public InteractionController GetInteractionController ()
	{
		if(this.interactionController == null)
		{
			GameObject ic = GetInstance ().transform.FindChild ("InteractionController").gameObject;
			if (ic == null)
			{
				ic = new GameObject ("InteractionController");
				ic.AddComponent <InteractionController> ();
			}

			GameController.AppendController (ic);

			this.interactionController = ic.GetComponent <InteractionController> ();
		}
		return this.interactionController;
	}
	
	/* Estático */
	private static GameController instance;
	public static GameController GetInstance ()
	{
		if (instance == null)
		{
			GameObject gameController = GameObject.Find ("_GameController");
			if (gameController == null)
			{
				gameController = new GameObject ("_GameController");
				gameController.AddComponent <GameController> ();
			}
			instance = gameController.GetComponent <GameController> ();
		}
		return instance;
	}

	private static void AppendController (GameObject controller)
	{
		controller.transform.parent = GetInstance ().transform;
	}
}
