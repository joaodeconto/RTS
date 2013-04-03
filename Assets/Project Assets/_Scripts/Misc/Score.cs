using UnityEngine;
using System.Collections;
using Visiorama.Utils;
using Visiorama;

public class Score : MonoBehaviour
{
	private static Score instance;
	public static Score GetInstance
	{
		get
		{
			if (instance == null)
				instance = ComponentGetter.Get<Score> ();

			return instance;
		}
	}

	//public static void AddScore
}
