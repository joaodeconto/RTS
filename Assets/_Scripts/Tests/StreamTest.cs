using UnityEngine;
using System.Collections;

public class StreamTest : MonoBehaviour {
	
	public int level = 1;
	public float percentageLoaded = 0;
	
    void Update ()
	{
        if (Application.GetStreamProgressForLevel (level) == 1)
            guiText.text = "Level at index \"" + level + "\" has been fully streamed!";
        else
		{
            percentageLoaded = Application.GetStreamProgressForLevel (level) * 100;
            guiText.text = percentageLoaded.ToString();
        }
    }
}
