using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class FogOfWar : MonoBehaviour
{
	private const int SIZE_TEXTURE = 256;

	public bool UseFog  = true;
	public bool DarkFog = true;

	public GameObject pref_plane;

	public Color visibleAreaColor = new Color(1.0f,1.0f,1.0f,0.0f);
	public Color knownAreaColor   = new Color(0.5f,0.5f,0.5f,0.5f);

	public float fogHeight = 8.0f;

	public Texture2D FogTexture { get; private set; }

	private List<Transform> allies;
	private List<Transform> enemies;
	private List<IStats> entityAllies;
	private List<IStats> entityEnemies;

	private Vector3 mapSize;

	private enum FogFlag
	{
		NOT_KNOWN_AREA,
		VISIBLE,
		KNOWN_AREA,
	}

	//Temporary variables
	Transform trns = null;
	int maxX, maxY, minX, minY,
		range, xRange = 0,
		posX, posY;

	FogFlag[,] matrixFogFlag;

	public FogOfWar Init()
	{
		if(!UseFog)
			return this;

		FogTexture    = new Texture2D (SIZE_TEXTURE, SIZE_TEXTURE, TextureFormat.ARGB32, false);
		matrixFogFlag = new FogFlag[SIZE_TEXTURE,SIZE_TEXTURE];

		//GameplayManager
		TerrainData td = ComponentGetter.Get<Terrain>("Terrain").terrainData;
		mapSize = td.size;

		for(int i = 0; i != SIZE_TEXTURE; ++i)
			for(int j = 0; j != SIZE_TEXTURE; ++j)
			{
				if(DarkFog)
					FogTexture.SetPixel(i,j, Color.black);
				else
					FogTexture.SetPixel(i,j, knownAreaColor);

				matrixFogFlag[i,j] = FogFlag.NOT_KNOWN_AREA;
			}

		FogTexture.Apply();

		//posicionando FogOfWar no local correto
		this.transform.position = new Vector3(mapSize.x * 0.5f , 0, mapSize.z * 0.5f);

		GameObject poly = Instantiate(pref_plane, Vector3.zero, Quaternion.identity) as GameObject;

		poly.layer = LayerMask.NameToLayer("FogOfWar");

		Transform polyTrns = poly.transform;

		Renderer r;

		polyTrns.parent = this.transform;
		polyTrns.localPosition    = Vector3.up * fogHeight;
		polyTrns.localScale       = new Vector3(mapSize.x, mapSize.z, mapSize.y) * 0.5f;
		polyTrns.localEulerAngles = new Vector3 (270,180,0);

		r = poly.renderer;
		r.material.mainTexture = FogTexture;

		allies        = new List<Transform>();
		entityAllies  = new List<IStats>();
		enemies       = new List<Transform>();
		entityEnemies = new List<IStats>();

		return this;
	}

	void Update()
	{
		if(!UseFog || allies == null)
			return;

		for(int i = allies.Count - 1; i != -1; --i)
		{
			trns = allies[i];

			posX = Mathf.RoundToInt(SIZE_TEXTURE * (trns.position.x / mapSize.x));
			posY = Mathf.RoundToInt(SIZE_TEXTURE * (trns.position.z / mapSize.z));

			range = Mathf.RoundToInt(SIZE_TEXTURE * (entityAllies[i].RangeView / mapSize.x));

			//maxX = Mathf.CeilToInt (posX + range);
			//maxY = Mathf.CeilToInt (posY + range);
			//minX = Mathf.RoundToInt(posX - range);
			//minY = Mathf.RoundToInt(posY - range);

			//for(int j = minX; j != maxX; ++j)
			//{
				//for(int k = minY; k != maxY; ++k)
				//{
					//texture.SetPixel(j, k, visibleAreaColor);
				//}
			//}

			maxY = Mathf.RoundToInt(Mathf.Clamp(posY + range, 0, SIZE_TEXTURE));
			minY = Mathf.RoundToInt(Mathf.Clamp(posY - range, 0, SIZE_TEXTURE));

			//float changeAngleRate = 180.0f / (2.0f * range);
			//float angle = 0.0f;

			for(int k = minY; k != maxY; ++k)//, angle += changeAngleRate)
			{
				//xRange = (int)(Mathf.Sin(Mathf.Deg2Rad * angle) * (float)range);

				xRange = Mathf.RoundToInt(Mathf.Sqrt((range * range) - ((k - posY) * (k - posY)) ));
					 //_________
				//x = V r² + y² `

				//Debug.Log("xRange: " + xRange);
				//Debug.Log("angle: " + angle);
				//Debug.Log("changeAngleRate: " + changeAngleRate);
				//Debug.Log("Mathf.Deg2Rad * angle: " + (Mathf.Deg2Rad * angle));

				maxX = Mathf.RoundToInt(Mathf.Clamp(posX + xRange, 0, SIZE_TEXTURE));
				minX = Mathf.RoundToInt(Mathf.Clamp(posX - xRange, 0, SIZE_TEXTURE));

				for(int j = minX; j != maxX; ++j)
				{
					matrixFogFlag [j,k] = FogFlag.VISIBLE;
				}
			}

			//Debug.Log("(int)(SIZE_TEXTURE * (trns.position.x / mapSize.x)): "
						//+ (int)(SIZE_TEXTURE * (trns.position.x / mapSize.x)));
			//Debug.Log("(int)(SIZE_TEXTURE * (trns.position.z / mapSize.z)): "
						//+ (int)(SIZE_TEXTURE * (trns.position.z / mapSize.z)));

			//texture.SetPixel((int)(SIZE_TEXTURE * (trns.position.x / mapSize.x)),
							 //(int)(SIZE_TEXTURE * (trns.position.z / mapSize.z)),
							 //visibleAreaColor);
		}

		UpdateEnemyVisibility();

		for(int i = 0; i != SIZE_TEXTURE; ++i)
			for(int j = 0; j != SIZE_TEXTURE; ++j)
			{
				if(matrixFogFlag[i,j] == FogFlag.VISIBLE)//== FogFlag.VISIBLE)
				{
					FogTexture.SetPixel(i, j, visibleAreaColor);

					//deixar área invisivel para que seja verificada posteriormente se é visivel
					matrixFogFlag[i,j] = FogFlag.KNOWN_AREA;
				}
				else if(matrixFogFlag[i,j] == FogFlag.KNOWN_AREA)//fogNodeVisited[i,j])
					FogTexture.SetPixel(i, j, knownAreaColor);
			}

		FogTexture.Apply();
	}

	void UpdateEnemyVisibility()
	{
		for(int i = enemies.Count - 1; i != -1; --i)
		{
			trns = enemies[i];

			if (trns == null)
			{
					  enemies.RemoveAt(i);
				entityEnemies.RemoveAt(i);
				++i;
				continue;
			}

			posX = Mathf.RoundToInt(SIZE_TEXTURE * (trns.position.x / mapSize.x));
			posY = Mathf.RoundToInt(SIZE_TEXTURE * (trns.position.z / mapSize.z));

			bool positionIsVisible = (matrixFogFlag[posX,posY] == FogFlag.VISIBLE);

//			Debug.Log("positionIsVisible: " + positionIsVisible);
			//Só aplicando se mudar o estado de visibilidade do inimigo
			if(!entityEnemies[i].IsVisible && positionIsVisible)
			{
//				Debug.Log("chegou 1");
				entityEnemies[i].SetVisible(true);
			}
			else if(entityEnemies[i].IsVisible && !positionIsVisible)
			{
				entityEnemies[i].SetVisible(false);
			}

			//Debug.Log("matrixFogFlag[posX,posY]: " + matrixFogFlag[posX,posY]);
			//Debug.Log("posX: " + posX + " - posY: " + posY);
		}
	}

	public FogOfWar AddEntity(Transform trnsEntity, IStats entity)
	{
		if(!UseFog)
			return null;

		if(ComponentGetter.Get<GameplayManager>().IsSameTeam(entity.Team))
		{
			allies.Add(trnsEntity);
			entityAllies.Add(entity);
		}
		else
		{
			enemies.Add(trnsEntity);
			entityEnemies.Add(entity);
		}

		return this;
	}

	public FogOfWar RemoveEntity(Transform trnsEntity, IStats entity)
	{
		if(!UseFog || trnsEntity == null)
			return null;

		if(ComponentGetter.Get<GameplayManager>().IsSameTeam(entity.Team))
		{
			int index = allies.IndexOf(trnsEntity) != null ? allies.IndexOf(trnsEntity) : -1;

			if (index == -1) return null;

				  allies.RemoveAt(index);
			entityAllies.RemoveAt(index);
		}
		else
		{
			int index = enemies.IndexOf(trnsEntity) != null ? enemies.IndexOf(trnsEntity) : -1;

			if (index == -1) return null;

				  enemies.RemoveAt(index);
			entityEnemies.RemoveAt(index);
		}

		return this;
	}

	public bool IsVisitedPosition (Transform trans)
	{
		return true;
	}
}
