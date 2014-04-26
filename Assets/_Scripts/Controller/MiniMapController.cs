using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class MiniMapController : MonoBehaviour
{
	public GameObject pref_UnitMiniMap;
	public GameObject pref_StructureMiniMap;

	public Transform miniMapButton;
	public Camera MapGUICamera;
	
	public GameObject miniMapPanel;
	public UIRoot MiniMapRoot;
	public UIAnchor minimapAnchor;

	public GameObject CamPositionMiniMap;
	public Texture2D CamPositionTexture;

	public Transform mapTransform;

	public GameObject fogMiniMap;

	public GameObject beingAttackedMiniMap;

	public UIAtlas minimapAtlas;
	
    public Vector3 visualizationSize;
	public Vector3 visualizationPosition;

	public float MiniMapRefreshInterval = 0.4f;

	private Vector2 miniMapSize;

	public Vector3 mapSize { get; private set; }

	private List<Transform>[] structureList;
	private List<Transform>[] unitList;

	private List<GameObject>[] UnitMiniMapList;
	private List<GameObject>[] StructureMiniMapList;

	private List<bool>[] WasStructureAlreadyVisible;

	private GameObject mainCameraGO;

	private bool WasInitialized;
	
	private FogOfWar fogOfWar;
	private TouchController tc;

	InteractionController ic;
	
	private UITexture textureFogOfWar;
	
	public MiniMapController Init()
	{
		if (WasInitialized)
		{
			Debug.LogError("Classe ja inicializada!");
			return this;
		}
		
		WasInitialized = true;

		GameplayManager gm = ComponentGetter.Get<GameplayManager>();

		int nTeams = gm.teams.Length;

		unitList      = new List<Transform>[nTeams];
		structureList = new List<Transform>[nTeams];

		UnitMiniMapList      = new List<GameObject>[nTeams];
		StructureMiniMapList = new List<GameObject>[nTeams];

		WasStructureAlreadyVisible = new List<bool>[nTeams];

		for(int i = structureList.Length - 1; i != -1; --i)
		{
			unitList[i]      = new List<Transform>();
			structureList[i] = new List<Transform>();

			UnitMiniMapList[i]      = new List<GameObject>();
			StructureMiniMapList[i] = new List<GameObject>();

			WasStructureAlreadyVisible[i] = new List<bool>();
		}

		InvokeRepeating("UpdateMiniMap",
						MiniMapRefreshInterval,
						MiniMapRefreshInterval);

		tc = ComponentGetter.Get <TouchController> ();
		fogOfWar = ComponentGetter.Get<FogOfWar>();

		mapSize = fogOfWar.terrain.terrainData.size;

		if (fogOfWar.UseFog)
		{
			textureFogOfWar = NGUITools.AddWidget<UITexture> (fogMiniMap);
			textureFogOfWar.pivot = UIWidget.Pivot.BottomLeft;
			textureFogOfWar.transform.localPosition    = new Vector2 (0,0);
		    textureFogOfWar.height = mapTransform.GetComponent<UISprite>().width;
		    textureFogOfWar.width =  mapTransform.GetComponent<UISprite>().height;
//		    textureFogOfWar.depth = 5;
//			textureFogOfWar.transform.localEulerAngles = Vector3.forward * 90f;
			textureFogOfWar.material = new Material (Shader.Find ("Unlit/Transparent Colored"));
			textureFogOfWar.material.mainTexture = fogOfWar.FogTexture;
//			Texture t = FOWSystem.instance.texture0;
//			Debug.Log ("Texture: " + (t != null));
//			ut.material.mainTexture = t;
		}

		UISprite us;
		us = NGUITools.AddWidget<UISprite> (CamPositionMiniMap);
		us.atlas = minimapAtlas;
		us.spriteName = "camera-mini-mapa";
        //ut.transform.localPosition = Vector3.forward * -1;
        us.depth = 10;
		us.width = (int)visualizationSize.x;
		us.height = (int)visualizationSize.y;
		us.type = UISprite.Type.Sliced;
		//ut.transform.parent.localScale = Vector3.one;
        //ut.transform.localEulerAngles = Vector3.forward * 90f;
		//ut.material = new Material (Shader.Find ("Unlit/Transparent Colored"));
		//ut.material.mainTexture = CamPositionTexture;

		mainCameraGO = Camera.main.gameObject;
		
		DefaultCallbackButton dcb;
		dcb = ComponentGetter.Get <DefaultCallbackButton> (miniMapButton, false);
		
		dcb.Init
		(
			null,
			(Hashtable ht_dcb) => { UpdateCameraPosition (); },
			null,
			null,
			null,
			null,
			(Hashtable ht_dcb, Vector2 delta) => { UpdateCameraPosition (); }
		);
		
		RefreshMiniMapSize();

		return this;
	}

	void RefreshMiniMapSize()
	{
		miniMapSize = mapTransform.GetComponent<UISprite>().localSize;   //miniMapCollider.bounds.max - miniMapCollider.bounds.min;//.size;//(miniMapMaxPoint - miniMapMinPoint);
				
		Vector3 newScale = new Vector3( visualizationSize.x / mapSize.x,
										visualizationSize.y / mapSize.z,
										1f);
		
		newScale.x *= mapTransform.localScale.x;
		newScale.y *= mapTransform.localScale.y;
		newScale.z *= mapTransform.localScale.z;
		
		//CamPositionMiniMap.transform.localScale = newScale;
	}

	void UpdateMiniMap()
	{
#if UNITY_EDITOR
		//RefreshMiniMapSize();
#endif
//		Debug.Log(FOWSystem.instance.texture1.width);
		
		//Update camera mini map position
		UpdateMiniMapCameraPosition();

		//iterate by teams
		for(int i = structureList.Length - 1; i != -1; --i)
		{
			//iterate by structures
			for(int j = structureList[i].Count - 1; j != -1; --j)
			{
				if (structureList[i][j] == null)
				{
					structureList[i].RemoveAt(j);
					StructureMiniMapList[i].RemoveAt(j);
				}
				else
				{
					UpdatePosition(StructureMiniMapList[i][j], structureList[i][j]);
				}
			}

			//iterate by unit
			for(int j = unitList[i].Count - 1; j != -1; --j)
			{
				if (unitList[i][j] == null)
				{
					unitList[i].RemoveAt(j);
					UnitMiniMapList[i].RemoveAt(j);
				}
				else
				{
					UpdatePosition(UnitMiniMapList[i][j], unitList[i][j]);
				}
			}
		}
	}

	void UpdateMiniMapCameraPosition()
	{
		//CamPositionMiniMap, mainCameraGO.transform

		Vector3 percentPos = new Vector3 (  (mainCameraGO.transform.position.x) / mapSize.x,
											(visualizationPosition.z + mainCameraGO.transform.position.z) / mapSize.z,
											0);

		//Debug.Log("percentPos (" + referenceTrns.name + "): " + percentPos);

		CamPositionMiniMap.transform.localPosition = new Vector3((mapTransform.localPosition.x + (miniMapSize.x * percentPos.x)),
																 (mapTransform.localPosition.y + (miniMapSize.y * percentPos.y)),
																 (-18));
	}

	void UpdatePosition(GameObject miniMapObject, Transform referenceTrns, float transformZ = 20f)
	{
		Vector3 percentPos = new Vector3(referenceTrns.position.x / mapSize.x,
										 referenceTrns.position.z / mapSize.z,
										 -transformZ);

		//Debug.Log("percentPos (" + referenceTrns.name + "): " + percentPos);
		//Debug.Log("miniMapSize: " + miniMapSize);

		miniMapObject.transform.localPosition = new Vector3((mapTransform.localPosition.x + (miniMapSize.x * percentPos.x)),
															(mapTransform.localPosition.y + (miniMapSize.y * percentPos.y)),
															(-transformZ));
	}

	public void UpdateCameraPosition()
	{
//		Debug.LogWarning ("TODO: UpdateCameraPosition ();");
		
//		float touchLocalPointX = Screen.width - MiniMapRoot.pixelSizeAdjustment * UICamera.lastTouchPosition.x;
//		float touchLocalPointY = Screen.height - MiniMapRoot.pixelSizeAdjustment * UICamera.lastTouchPosition.y;
		/*float touchLocalPointX = UICamera.lastTouchPosition.x - CamPositionMiniMap.transform.position.x;
		float touchLocalPointY = UICamera.lastTouchPosition.y - CamPositionMiniMap.transform.position.y;
        
		Debug.Log("touchLocalPointX: " + touchLocalPointX + " = " + UICamera.lastTouchPosition.x + " - " + CamPositionMiniMap.transform.position.x);
		Debug.Log("touchLocalPointY: " + touchLocalPointY + " = " + UICamera.lastTouchPosition.y + " - " + CamPositionMiniMap.transform.position.y);
        
		Vector2 touchPosition = mapSizeComponent.GetPositon(new Vector2(touchLocalPointX, touchLocalPointY));
        
		mainCameraGO.transform.position = new Vector3(touchPosition.x, mainCameraGO.transform.position.y, touchPosition.y);*/
        
//        return;
        
        CameraBounds camBounds = mainCameraGO.GetComponent<CameraBounds>();

		Vector3 camBoundsSize = mapSize;
								//new Vector3((camBounds.scenario.x.max - camBounds.scenario.x.min),
											//(camBounds.scenario.y.max - camBounds.scenario.y.min),
											//(camBounds.scenario.z.max - camBounds.scenario.z.min));

//		Debug.Log("mainCameraGO.transform.localPosition: " + mainCameraGO.transform.localPosition);
//		Debug.Log("UICamera.lastTouchPosition: " + UICamera.lastTouchPosition * MiniMapRoot.pixelSizeAdjustment);
//		Debug.Log("mapTransform.localPosition: " + mapTransform.localPosition);
//		Debug.Log("miniMapSize: " + miniMapSize);
				
		float touchLocalPointX = MiniMapRoot.pixelSizeAdjustment * UICamera.lastTouchPosition.x;
		float touchLocalPointY = MiniMapRoot.pixelSizeAdjustment * UICamera.lastTouchPosition.y;
		
		Vector3 vecTouchLocalPosition = Vector3.zero;
		vecTouchLocalPosition.x = touchLocalPointX;
		vecTouchLocalPosition.y = touchLocalPointY;
		
//		Debug.Log("b: touchLocalPointX: " + touchLocalPointX);
//		Debug.Log("b: touchLocalPointY: " + touchLocalPointY);
		
//		Ray ray = UICamera.mainCamera.ScreenToWorldPoint (vecTouchLocalPosition);
//		RaycastHit hit;
//		
//		if (Physics.Raycast (ray, out hit))
//		{
//			Debug.Log ("hit.point: " + hit.point);
//			Debug.Log ("that's point: " + (hit.point - miniMapButton.position));
//		}
//		
//		Debug.Log ("ray: " + ray.origin + " - " + ray.direction);
		
//		MapGUICamera.WorldToScreenPoint (miniMapButton.position);//ray.origin;
				
		Vector3 vecScreen = MapGUICamera.WorldToScreenPoint (miniMapPanel.transform.position);

		//Retirando posicao do mapa globalmente
		touchLocalPointX -= (vecScreen.x);
		touchLocalPointY -= (vecScreen.y);

		Debug.Log ("vecScreen: " + vecScreen);
		
		Debug.Log("a: touchLocalPointX: " + touchLocalPointX);
		Debug.Log("a: touchLocalPointY: " + touchLocalPointY);
		
		Vector2 percentPos = new Vector2(((MiniMapRoot.pixelSizeAdjustment
		                                   * UICamera.lastTouchPosition.x) - Screen.width + miniMapPanel.transform.localPosition.x),
		                                 ((MiniMapRoot.pixelSizeAdjustment * UICamera.lastTouchPosition.y) - minimapAnchor.transform.localPosition.y + miniMapPanel.transform.localPosition.y));
		percentPos.x /= miniMapSize.x;
		percentPos.y /= miniMapSize.y;
		
//		percentPos.x -= 7.1f;
//		percentPos.y -= 2.9f;
        
//		Debug.Break ();
		Debug.Log("percentPos: " + percentPos);
		Debug.Log("mapSize: " + mapSize);

		Vector3 newCameraPosition = new Vector3((camBoundsSize.x * percentPos.x)         - (visualizationPosition.x),
											    (mainCameraGO.transform.localPosition.y) - (visualizationPosition.y),
											    (camBoundsSize.z * percentPos.y)         - (visualizationPosition.z * 1.5f)  );

		mainCameraGO.transform.position = mainCameraGO.GetComponent<CameraBounds>().ClampScenario(newCameraPosition);

		Debug.Log("mainCameraGO.transform.localPosition: " + mainCameraGO.transform.localPosition);
	}

	public void InstantiatePositionBeingAttacked (Transform target)
	{
		GameObject miniMapObject = Instantiate (beingAttackedMiniMap) as GameObject;

		miniMapObject.transform.parent     = miniMapPanel.transform;
		miniMapObject.GetComponent <UISprite>().height = beingAttackedMiniMap.GetComponent <UISprite>().height;
		miniMapObject.GetComponent <UISprite>().width = beingAttackedMiniMap.GetComponent <UISprite>().width;

		miniMapObject.GetComponent<TweenHeight> ().Play (true);
		miniMapObject.GetComponent<TweenWidth> ().Play (true);

		miniMapObject.GetComponent<UISprite> ().depth = 10;

		UpdatePosition (miniMapObject, target);
	}

#region Add and Remove Structures/Units
	GameObject InstantiateMiniMapObject(GameObject pref_go, Transform trns, int teamId)
	{
		GameObject _go = Instantiate(pref_go, Vector3.zero, Quaternion.identity) as GameObject;

		_go.transform.parent     = miniMapPanel.transform;
		_go.transform.localScale = pref_go.GetComponent<UISprite> ().localSize;

		Color teamColor = ComponentGetter.Get<GameplayManager>().teams[teamId].colors[0];

		_go.GetComponent<UISlicedSprite>().color = teamColor;

//		_go.GetComponent<UISprite> ().depth = 10;

		UpdatePosition(_go, trns);

		return _go;
	}

	public void AddStructure (Transform trns, int teamId)
	{
#if UNITY_EDITOR
		if(structureList.Length <= teamId)
		{
			Debug.Log("O numero de times eh menor do que o id enviado");
			Debug.Log("teamId: " + teamId);
			Debug.Break();
		}
#endif

		GameObject miniMapObject = InstantiateMiniMapObject(pref_StructureMiniMap, trns, teamId);

		structureList[teamId].Add(trns);
		StructureMiniMapList[teamId].Add(miniMapObject);
		WasStructureAlreadyVisible[teamId].Add(false);
	}

	public void AddUnit (Transform trns, int teamId)
	{
		//Inicializando se nao foi inicializado ainda
		if (unitList == null)
			Init ();

#if UNITY_EDITOR
 		if(unitList.Length <= teamId)
		{
			Debug.Log("O numero de times eh menor do que o id enviado");
			Debug.Log("teamId: " + teamId);
			Debug.Break();
		}
#endif
		GameObject miniMapObject = InstantiateMiniMapObject(pref_UnitMiniMap, trns, teamId);

		unitList[teamId].Add(trns);
		UnitMiniMapList[teamId].Add(miniMapObject);
	}

	public void RemoveStructure (Transform trns, int teamId)
	{
		if (trns == null) return;

		int index = structureList[teamId].IndexOf(trns) != null ? structureList[teamId].IndexOf(trns) : -1;

		if (index == -1) return;

		GameObject obj = StructureMiniMapList[teamId][index];

		Destroy(obj);

		structureList[teamId].RemoveAt(index);
		StructureMiniMapList[teamId].RemoveAt(index);
	}

	public void RemoveUnit (Transform trns, int teamId)
	{
		if (trns == null) return;
		
		int index = unitList[teamId].IndexOf(trns) != null ? unitList[teamId].IndexOf(trns) : -1;

		if (index == -1) return;

		GameObject obj = UnitMiniMapList[teamId][index];

		Destroy(obj);

		unitList[teamId].RemoveAt(index);
		UnitMiniMapList[teamId].RemoveAt(index);
	}

	public void SetVisibilityStructure(Transform trns, int teamId, bool visibility)
	{
		int index = structureList[teamId].IndexOf(trns);

		if(visibility)
		{
			//Debug.Log("chegou");
			//Debug.Log("StructureMiniMapList[teamId][index].activeSelf: " + StructureMiniMapList[teamId][index].activeSelf);
			if(!StructureMiniMapList[teamId][index].activeSelf)
			{
				StructureMiniMapList[teamId][index].SetActive(true);
				WasStructureAlreadyVisible[teamId][index] = true;
			}
		}
		else
		{
			if(!WasStructureAlreadyVisible[teamId][index])
			{
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
