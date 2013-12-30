using UnityEngine;
using System.Collections;

public class Cheats : MonoBehaviour
{
	[System.Serializable]
	public class CheatData
	{
		public Cheat CheatCallback; 
		public KeyCode[] Pressed;
		public KeyCode[] Hold;
	}

	public delegate void Cheat ();

	public CheatData[] cheats;
	
	// Update is called once per frame
	void Update ()
	{
		bool success = true;
		foreach (CheatData cd in cheats)
		{
			success = true;

			foreach (KeyCode k in cd.Hold)
			{
				if (Input.GetKey(k) == false)
				{
					success = false;
					break;
				}
			}
			
			foreach (KeyCode k in cd.Pressed)
			{
				if (Input.GetKeyDown(k) == false)
				{
					success = false;
					break;
				}
			}

			if (success)
			{
				cd.CheatCallback ();
			}
		}
	}

	public void asd ()
	{
		#if UNITY_EDITOR
		if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown (KeyCode.K))
		{
			GameplayManager gm = Visiorama.ComponentGetter.Get <GameplayManager> ();
			gm.resources.NumberOfRocks += 100;
		}
		#endif
	}
}
