using UnityEngine;

[AddComponentMenu("NGUI/Examples/HUD Root")]
public class HUDRoot : MonoBehaviour
{
	static public GameObject go;
	void Awake () { go = gameObject; }
}