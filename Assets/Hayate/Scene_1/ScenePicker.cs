using UnityEngine;
using System.Collections;

public class ScenePicker : MonoBehaviour {
	
	private int currentScene = 0;
	
	void Awake() {
        DontDestroyOnLoad(gameObject);
    }
	
	void OnGUI()
	{
		if(GUI.Button(new Rect(Screen.width / 2-50, 50, 100, 20), "Next Scene"))
		{
			if(currentScene == 8)
			{
				currentScene = 0;
				Application.LoadLevel(currentScene);
				Destroy (gameObject);
			}else{
				currentScene++;
			}
			
			Application.LoadLevel(currentScene);
		}
	}
}
