using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;
using PathologicalGames;

public class MiniMapController : MonoBehaviour
{
	#region Declares
	public Vector3 mapSize { get; private set; }	
	public float MiniMapRefreshInterval = 0.4f;
	public GameObject pref_StructureMiniMap;
	public GameObject beingAttackedMiniMap;
	public GameObject CamPositionMiniMap;
	public Vector3 visualizationPosition;
	public GameObject pref_UnitMiniMap;
	public Vector3 visualizationSize;
	public GameObject miniMapPanel;
	public Transform miniMapButton;
	public Transform mapTransform;
	public GameObject fogMiniMap;
	public UIAtlas minimapAtlas;	
	public Camera MapGUICamera;	
	public UIRoot MiniMapRoot;

	private List<GameObject>[] StructureMiniMapList;
	private List<bool>[] WasStructureAlreadyVisible;
	private List<GameObject>[] UnitMiniMapList;
	private List<Transform>[] structureList;
	private List<Transform>[] unitList;
	private UITexture textureFogOfWar;
	private GameObject mainCameraGO;
	private bool WasInitialized;
	private Vector2 miniMapSize;
	private UISprite us;

	protected InteractionController ic;	
	protected TouchController tc;
	protected GameplayManager gm;
	protected FogOfWar fogOfWar;
	protected CameraBounds cb;
	#endregion

	#region Init
	public void Init()
	{
		if (WasInitialized) return;			
		WasInitialized = true;		
		mainCameraGO = Camera.main.gameObject;	
		gm = ComponentGetter.Get<GameplayManager>();
		tc = ComponentGetter.Get <TouchController> ();
		cb = mainCameraGO.GetComponent<CameraBounds>();
		fogOfWar = ComponentGetter.Get<FogOfWar>();
		int nTeams = gm.teams.Length;
		unitList      		 	= new List<Transform>[nTeams];
		structureList 		 	= new List<Transform>[nTeams];
		UnitMiniMapList      	= new List<GameObject>[nTeams];
		StructureMiniMapList 	= new List<GameObject>[nTeams];
		WasStructureAlreadyVisible = new List<bool>[nTeams];

		for(int i = structureList.Length - 1; i != -1; --i)
		{
			unitList[i]      		= new List<Transform>();
			structureList[i] 		= new List<Transform>();
			UnitMiniMapList[i]      = new List<GameObject>();
			StructureMiniMapList[i] = new List<GameObject>();
			WasStructureAlreadyVisible[i] = new List<bool>();
		}

		InvokeRepeating("UpdateMiniMap", MiniMapRefreshInterval, MiniMapRefreshInterval);
		mapSize = fogOfWar.terrain.terrainData.size;

		if (fogOfWar.UseFog){
			textureFogOfWar = NGUITools.AddWidget<UITexture> (fogMiniMap);
			textureFogOfWar.pivot = UIWidget.Pivot.BottomLeft;
			textureFogOfWar.transform.localPosition    = new Vector2 (0,0);
		    textureFogOfWar.height = mapTransform.GetComponent<UISprite>().width;
		    textureFogOfWar.width =  mapTransform.GetComponent<UISprite>().height;
			textureFogOfWar.material = new Material (Shader.Find ("Unlit/Transparent Colored"));
			textureFogOfWar.material.mainTexture = fogOfWar.FogTexture;
		}
		us = NGUITools.AddWidget<UISprite> (CamPositionMiniMap);
		us.atlas = minimapAtlas;
		us.spriteName = "camera-mini-mapa";
        us.depth = 10;
		us.width = (int)visualizationSize.x;
		us.height = (int)visualizationSize.y;
		us.type = UISprite.Type.Sliced;	
		DefaultCallbackButton dcb;
		dcb = ComponentGetter.Get <DefaultCallbackButton> (miniMapButton, false);		
		dcb.Init(null,(Hashtable ht_dcb) => { UpdateCameraPosition (); },null,null,null,null,
			(Hashtable ht_dcb, Vector2 delta) => { UpdateCameraPosition (); }
		);		
		RefreshMiniMapSize();
	}

	void RefreshMiniMapSize()
	{
		miniMapSize = mapTransform.GetComponent<UISprite>().localSize;  				
		Vector3 newScale = new Vector3( visualizationSize.x / mapSize.x,
										visualizationSize.y / mapSize.z,
										1f);
		
		newScale.x *= mapTransform.localScale.x;
		newScale.y *= mapTransform.localScale.y;
		newScale.z *= mapTransform.localScale.z;
	}
	#endregion

	#region Minimap updates	
	void UpdateMiniMap()
	{
		UpdateMiniMapCameraPosition();
		us.width = (int)(visualizationSize.x*(1-cb.zoomModifier));
		us.height = (int)(visualizationSize.y*(1-cb.zoomModifier));

		for(int i = structureList.Length - 1; i != -1; --i)
		{
			for(int j = structureList[i].Count - 1; j != -1; --j)
			{
				if (structureList[i][j] == null){
					structureList[i].RemoveAt(j);
					StructureMiniMapList[i].RemoveAt(j);
				}
				else{
					UpdatePosition(StructureMiniMapList[i][j], structureList[i][j]);
				}
			}

			for(int j = unitList[i].Count - 1; j != -1; --j)
			{
				if (unitList[i][j] == null){
					unitList[i].RemoveAt(j);
					UnitMiniMapList[i].RemoveAt(j);
				}
				else{
					UpdatePosition(UnitMiniMapList[i][j], unitList[i][j]);
				}
			}
		}
	}

	void UpdateMiniMapCameraPosition()
	{
		Vector3 percentPos = new Vector3 (  (mainCameraGO.transform.position.x) / mapSize.x,
											(visualizationPosition.z + mainCameraGO.transform.position.z) / mapSize.z,
											0);


		CamPositionMiniMap.transform.localPosition = new Vector3((mapTransform.localPosition.x + (miniMapSize.x * percentPos.x)),
																 (mapTransform.localPosition.y + (miniMapSize.y * percentPos.y)),0);
	}

	void UpdatePosition(GameObject miniMapObject, Transform referenceTrns, float transformZ = 0)
	{
		Vector3 percentPos = new Vector3(referenceTrns.position.x / mapSize.x,
										 referenceTrns.position.z / mapSize.z,
										 -transformZ);
		miniMapObject.transform.localPosition = new Vector3((mapTransform.localPosition.x + (miniMapSize.x * percentPos.x)),
															(mapTransform.localPosition.y + (miniMapSize.y * percentPos.y)),
															(-transformZ));
	}

	public void UpdateCameraPosition()
	{        
		Vector3 camBoundsSize = mapSize;				
		float touchLocalPointX = UICamera.lastTouchPosition.x;
		float touchLocalPointY = UICamera.lastTouchPosition.y;
		Vector3 vecTouchLocalPosition = Vector3.zero;
		vecTouchLocalPosition.x = touchLocalPointX;
		vecTouchLocalPosition.y = touchLocalPointY;		
		Vector3 vecScreen = MapGUICamera.WorldToScreenPoint (mapTransform.transform.position);
		touchLocalPointX -= (vecScreen.x );
        touchLocalPointY -= (vecScreen.y );
		Vector2 percentPos = new Vector2 ((MiniMapRoot.pixelSizeAdjustment * touchLocalPointX),(MiniMapRoot.pixelSizeAdjustment * touchLocalPointY));
		percentPos.x /= miniMapSize.x;
		percentPos.y /= miniMapSize.y;
		Vector3 newCameraPosition = new Vector3((camBoundsSize.x * percentPos.x)         - (visualizationPosition.x),
											    (mainCameraGO.transform.localPosition.y) - (visualizationPosition.y),
											    (camBoundsSize.z * percentPos.y)         - (visualizationPosition.z));

		mainCameraGO.transform.position = mainCameraGO.GetComponent<CameraBounds>().ClampScenario(newCameraPosition);
	}

	public void InstantiatePositionBeingAttacked (Transform target)
	{
		Transform miniMapObject = PoolManager.Pools["Minimap"].Spawn(beingAttackedMiniMap);
		miniMapObject.GetComponent<TweenHeight> ().Play (true);
		miniMapObject.GetComponent<UISprite> ().depth = 60;
		UpdatePosition (miniMapObject.gameObject, target);
	}
	#endregion

	#region Add and Remove Structures/Units
	Transform InstantiateMiniMapObject(GameObject pref_go, Transform trns, int teamId)
	{
		Transform _go = PoolManager.Pools["Minimap"].Spawn(pref_go, Vector3.zero, Quaternion.identity);
		Color teamColor = ComponentGetter.Get<GameplayManager>().teams[teamId].colors[0];
		_go.GetComponent<UISprite>().color = teamColor;
		UpdatePosition(_go.gameObject, trns);
		return _go;
	}

	public void AddStructure (Transform trns, int teamId)
	{
	#if UNITY_EDITOR
		if(structureList.Length <= teamId){
			Debug.Log("O numero de times eh menor do que o id enviado");
			Debug.Log("teamId: " + teamId);
			Debug.Break();
		}
#endif

		Transform miniMapObject = InstantiateMiniMapObject(pref_StructureMiniMap, trns, teamId);
		structureList[teamId].Add(trns);
		StructureMiniMapList[teamId].Add(miniMapObject.gameObject);
		WasStructureAlreadyVisible[teamId].Add(false);
	}

	public void AddUnit (Transform trns, int teamId)
	{
		//Inicializando se nao foi inicializado ainda
		if (unitList == null)	Init ();

#if UNITY_EDITOR
 		if(unitList.Length <= teamId){
			Debug.Break();
		}
#endif
		Transform miniMapObject = InstantiateMiniMapObject(pref_UnitMiniMap, trns, teamId);

		unitList[teamId].Add(trns);
		UnitMiniMapList[teamId].Add(miniMapObject.gameObject);
	}

	public void RemoveStructure (Transform trns, int teamId)
	{
		if (trns == null) return;
		int index = structureList[teamId].IndexOf(trns) != null ? structureList[teamId].IndexOf(trns) : -1;
		if (index == -1) return;
		GameObject obj = StructureMiniMapList[teamId][index];
		PoolManager.Pools["Minimap"].Despawn(obj.transform);
		structureList[teamId].RemoveAt(index);
		StructureMiniMapList[teamId].RemoveAt(index);
	}

	public void RemoveUnit (Transform trns, int teamId)
	{
		if (trns == null) return;		
		int index = unitList[teamId].IndexOf(trns) != null ? unitList[teamId].IndexOf(trns) : -1;
		if (index == -1) return;
		GameObject obj = UnitMiniMapList[teamId][index];
		PoolManager.Pools["Minimap"].Despawn(obj.transform);
		unitList[teamId].RemoveAt(index);
		UnitMiniMapList[teamId].RemoveAt(index);
	}

	public void SetVisibilityStructure(Transform trns, int teamId, bool visibility)
	{
		int index = structureList[teamId].IndexOf(trns);
		if(visibility){
			if(!StructureMiniMapList[teamId][index].activeSelf){
				StructureMiniMapList[teamId][index].SetActive(true);
				WasStructureAlreadyVisible[teamId][index] = true;
			}
		}
		else{
			if(!WasStructureAlreadyVisible[teamId][index]){
				StructureMiniMapList[teamId][index].SetActive(false);
			}
		}
	}

	public void SetVisibilityUnit(Transform trns, int teamId, bool visibility)
	{
		int index = unitList[teamId].IndexOf(trns);
		if (index != -1)
			UnitMiniMapList[teamId][index].SetActive(visibility);
	}
	#endregion
}
