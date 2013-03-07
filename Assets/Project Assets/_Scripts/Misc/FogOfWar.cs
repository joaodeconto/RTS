using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class FogOfWar : MonoBehaviour
{
	public bool UseFog  = true;
	public bool DarkFog = true;

	public GameObject pref_plane;

	public Color visibleAreaColor = new Color(1.0f,1.0f,1.0f,0.0f);
	public Color knownAreaColor   = new Color(0.5f,0.5f,0.5f,0.5f);

	public Texture2D texture;

	public List<Transform> allies;
	public List<IStats> entityAllies;
	public List<Transform> enemies;
	public List<IStats> entityEnemies;

	public Vector3 mapSize;

	Renderer r;

	private const int SIZE_TEXTURE = 128;

	private enum FogFlag
	{
		VISITED,
		NOT_VISIBLE,
		VISIBLE,
	}

	bool[,] fogNodeVisited;

	public FogOfWar Init()
	{
		if(!UseFog)
			return this;

		texture        = new Texture2D(SIZE_TEXTURE, SIZE_TEXTURE, TextureFormat.ARGB32, false);
		fogNodeVisited = new bool[SIZE_TEXTURE,SIZE_TEXTURE];

		for(int i = 0; i != SIZE_TEXTURE; ++i)
			for(int j = 0; j != SIZE_TEXTURE; ++j)
			{
				if(DarkFog)
					texture.SetPixel(i,j, Color.black);
				else
					texture.SetPixel(i,j, knownAreaColor);
			}

		texture.Apply();

		GameObject poly = Instantiate(pref_plane, Vector3.zero, Quaternion.identity) as GameObject;

		poly.layer = LayerMask.NameToLayer("FogOfWar");

		Transform polyTrns = poly.transform;

		polyTrns.parent = this.transform;
		polyTrns.localPosition    = Vector3.zero;
		polyTrns.localScale       = Vector3.one * 105f;
		polyTrns.localEulerAngles = new Vector3(270,180,0);

		r = poly.renderer;//.GetComponent<MeshRenderer>();

		//GameplayManager
		TerrainData td = ComponentGetter.Get<Terrain>("Terrain").terrainData;
		mapSize = td.size;

		allies       = new List<Transform>();
		entityAllies = new List<IStats>();
		enemies      = new List<Transform>();
		entityEnemies= new List<IStats>();

		return this;
	}

	void Update()
	{
		if(!UseFog)
			return;

		//if(Time.time % 200)
			//return;

		for(int i = 0; i != SIZE_TEXTURE; ++i)
			for(int j = 0; j != SIZE_TEXTURE; ++j)
				if(fogNodeVisited[i,j])
				{
					texture.SetPixel(i, j, knownAreaColor);
				}

		Transform trns = null;
		int maxX, maxY, minX, minY,
			range, xRange = 0,
			posX, posY;

		for(int i = allies.Count - 1; i != -1; --i)
		{
			trns = allies[i];

			posX = (int)(SIZE_TEXTURE * (trns.position.x / mapSize.x));
			posY = (int)(SIZE_TEXTURE * (trns.position.z / mapSize.z));

			range = (int)(SIZE_TEXTURE * (entityAllies[i].RangeView / mapSize.x));

			//maxX = Mathf.CeilToInt (posX + range);
			//maxY = Mathf.CeilToInt (posY + range);
			//minX = Mathf.FloorToInt(posX - range);
			//minY = Mathf.FloorToInt(posY - range);

			//for(int j = minX; j != maxX; ++j)
			//{
				//for(int k = minY; k != maxY; ++k)
				//{
					//texture.SetPixel(j, k, visibleAreaColor);
				//}
			//}

			maxY = (int)Mathf.Clamp(posY + range, 0, SIZE_TEXTURE);
			minY = (int)Mathf.Clamp(posY - range, 0, SIZE_TEXTURE);

			//float changeAngleRate = 180.0f / (2.0f * range);
			//float angle = 0.0f;

			for(int k = minY; k != maxY; ++k)//, angle += changeAngleRate)
			{
				//xRange = (int)(Mathf.Sin(Mathf.Deg2Rad * angle) * (float)range);

				xRange = (int)Mathf.Sqrt((range * range) - ((k - posY) * (k - posY)) );
					 //_________
				//x = V r² + y² `

				//Debug.Log("xRange: " + xRange);
				//Debug.Log("angle: " + angle);
				//Debug.Log("changeAngleRate: " + changeAngleRate);
				//Debug.Log("Mathf.Deg2Rad * angle: " + (Mathf.Deg2Rad * angle));

				maxX = (int)Mathf.Clamp(posX + xRange, 0, SIZE_TEXTURE);
				minX = (int)Mathf.Clamp(posX - xRange, 0, SIZE_TEXTURE);

				for(int j = minX; j != maxX; ++j)
				{
					fogNodeVisited[j,k] = true;
					texture.SetPixel(j, k, visibleAreaColor);
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

		texture.Apply();
		r.material.mainTexture = texture;

		for(int i = enemies.Count - 1; i != -1; --i)
		{
			trns = enemies[i];

			posX = (int)(SIZE_TEXTURE * (trns.position.x / mapSize.x));
			posY = (int)(SIZE_TEXTURE * (trns.position.z / mapSize.z));

			entityEnemies[i].SetVisible(fogNodeVisited[posX,posY]);
		}
	}

	public FogOfWar AddEntity(Transform trnsEntity, IStats entity)
	{
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
		if(ComponentGetter.Get<GameplayManager>().IsSameTeam(entity.Team))
		{
			int index = allies.IndexOf(trnsEntity);

				  allies.RemoveAt(index);
			entityAllies.RemoveAt(index);
		}
		else
		{
			int index = enemies.IndexOf(trnsEntity);

				  enemies.RemoveAt(index);
			entityEnemies.RemoveAt(index);
		}

		return this;
	}
}
