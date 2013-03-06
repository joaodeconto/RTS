using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class FogOfWar : MonoBehaviour
{
	public bool UseFog;

	public GameObject pref_plane;

	public bool DarkFog = true;
	public Color visibleAreaColor = new Color(1.0f,1.0f,1.0f,0.0f);
	public Color knownAreaColor   = new Color(0.5f,0.5f,0.5f,0.5f);

	public Texture2D texture;

	public List<Transform> allies;
	public List<IStats> entityAllies;
	public List<GameObject> enemies;

	public Vector3 mapSize;

	Renderer r;

	private const int SIZE_TEXTURE = 128;

	public FogOfWar Init()
	{
		if(!UseFog)
			return this;

		texture = new Texture2D(SIZE_TEXTURE,SIZE_TEXTURE, TextureFormat.ARGB32, false);

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
		enemies      = new List<GameObject>();

		return this;
	}

	void Update()
	{
		if(!UseFog)
			return;

		//if(Time.time % 200)
			//return;

		Transform trns = null;
		int maxX, maxY, minX, minY, range, xRange = 0;
		Vector2 pos;

		for(int i = allies.Count - 1; i != -1; --i)
		{
			trns = allies[i];

			pos = new Vector2(SIZE_TEXTURE * (trns.position.x / mapSize.x),
							  SIZE_TEXTURE * (trns.position.z / mapSize.z));

			range = (int)(SIZE_TEXTURE * (entityAllies[i].RangeView / mapSize.x));

			//maxX = Mathf.CeilToInt (pos.x + range);
			//maxY = Mathf.CeilToInt (pos.y + range);
			//minX = Mathf.FloorToInt(pos.x - range);
			//minY = Mathf.FloorToInt(pos.y - range);

			//for(int j = minX; j != maxX; ++j)
			//{
				//for(int k = minY; k != maxY; ++k)
				//{
					//texture.SetPixel(j, k, visibleAreaColor);
				//}
			//}

			maxY = (int)Mathf.Clamp(pos.y + range, 0, SIZE_TEXTURE);
			minY = (int)Mathf.Clamp(pos.y - range, 0, SIZE_TEXTURE);

			//float changeAngleRate = 180.0f / (2.0f * range);
			//float angle = 0.0f;

			for(int k = minY; k != maxY; ++k)//, angle += changeAngleRate)
			{
				//xRange = (int)(Mathf.Sin(Mathf.Deg2Rad * angle) * (float)range);

				xRange = (int)Mathf.Sqrt((range * range) - ((k - minY - range) * (k - minY - range)) );
					 //_________
				//x = V r² + y² `

				//Debug.Log("xRange: " + xRange);
				//Debug.Log("angle: " + angle);
				//Debug.Log("changeAngleRate: " + changeAngleRate);
				//Debug.Log("Mathf.Deg2Rad * angle: " + (Mathf.Deg2Rad * angle));

				maxX = (int)Mathf.Clamp(pos.x + xRange, 0, SIZE_TEXTURE);
				minX = (int)Mathf.Clamp(pos.x - xRange, 0, SIZE_TEXTURE);

				//Debug.LogError("hmm");

				//Debug.Log("(k - pos.y): " + (k - pos.y));
				//Debug.Log("minX: " + minX);
				//Debug.Log("maxX: " + maxX);

				for(int j = minX; j != maxX; ++j)
				{
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
			enemies.Add(trnsEntity.gameObject);
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
			allies.Remove(trnsEntity);
		}

		return this;
	}
}
