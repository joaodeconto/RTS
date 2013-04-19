using UnityEngine;
using System.Collections;

[AddComponentMenu("Animation/Instantiate Script")]
public class InstantiateInAnimation : MonoBehaviour
{
	void Instantiate (GameObject prefab)
	{
		Instantiate (prefab, transform.position, transform.rotation);
	}
}
