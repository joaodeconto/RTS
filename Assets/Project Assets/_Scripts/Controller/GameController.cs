using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	
	protected GameplayManager gameplayManager;
	protected TouchController touchController;
	protected SelectionController selectionController;
	protected TroopController troopController;
	protected FactoryController factoryController;
	protected InteractionController interactionController;
	protected HUDController hudController;
	protected NetworkManager networkManager;
	
	void Awake ()
	{
		PhotonNetwork.offlineMode = true;
		
		GetNetworkManager ().Init ();
		GetGameplayManager ().Init ();
		GetTouchController ().Init ();
		GetSelectionController ().Init ();
		GetTroopController ().Init ();
		GetFactoryController ().Init ();
		GetInteractionController ().Init ();
	}
	
	public GameplayManager GetGameplayManager ()
	{
		if(this.gameplayManager == null)
		{
			GameObject gm = GetInstance ().transform.FindChild ("GameplayManager").gameObject;
			if (gm == null)
			{
				gm = new GameObject ("GameplayManager");
				gm.AddComponent <GameplayManager> ();
			}

			GameController.AppendController (gm);

			this.gameplayManager = gm.GetComponent <GameplayManager> ();
		}
		return this.gameplayManager;
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
	
	public FactoryController GetFactoryController ()
	{
		if(this.factoryController == null)
		{
			GameObject fc = GetInstance ().transform.FindChild ("FactoryController").gameObject;
			if (fc == null)
			{
				fc = new GameObject ("FactoryController");
				fc.AddComponent <FactoryController> ();
			}

			GameController.AppendController (fc);

			this.factoryController = fc.GetComponent <FactoryController> ();
		}
		return this.factoryController;
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
	
	public HUDController GetHUDController ()
	{
		if(this.hudController == null)
		{
			GameObject hc = GetInstance ().transform.FindChild ("HUDController").gameObject;
			if (hc == null)
			{
				hc = new GameObject ("HUDController");
				hc.AddComponent <HUDController> ();
			}

			GameController.AppendController (hc);

			this.hudController = hc.GetComponent <HUDController> ();
		}
		return this.hudController;
	}
	
	public NetworkManager GetNetworkManager ()
	{
		if(this.networkManager == null)
		{
			GameObject nm = GetInstance ().transform.FindChild ("NetworkManager").gameObject;
			if (nm == null)
			{
				nm = new GameObject ("NetworkManager");
				nm.AddComponent <NetworkManager> ();
			}

			GameController.AppendController (nm);

			this.networkManager = nm.GetComponent <NetworkManager> ();
		}
		return this.networkManager;
	}
	
	// Estático
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
	
	// Adicionar Controle
	private static void AppendController (GameObject controller)
	{
		controller.transform.parent = GetInstance ().transform;
	}
}
