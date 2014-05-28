using UnityEngine;
using System.Collections;

public class GraphicQuality : MonoBehaviour
{
	private bool wasInitialized = false;

	public UIPopupList presets;
	public UIPopupList texture;
	public UIPopupList shadow;


	public void OnEnable ()
	{
		Open ();
	}

	public void OnDisable ()
	{
		Close ();
	}


	public void Open ()
	{
		if (wasInitialized)
			return;

		wasInitialized = true;

		DefaultCallbackButton dcb;

		QualityIntConversion(PlayerPrefs.GetInt("GraphicQuality"));


		Transform close = this.transform.FindChild ("Menu").FindChild ("Resume");
		
		if (close != null)
		{
			dcb = close.gameObject.AddComponent<DefaultCallbackButton> ();
			dcb.Init(null,
			         (ht_dcb) => 
			         {
				gameObject.SetActive (false);
			});
		}
	}

	public void QualityStringConversion (string qualiSelect)
	{
		
			if ( qualiSelect =="Fastest")
			{
				PlayerPrefs.SetInt("GraphicQuality",0);
				QualitySettings.SetQualityLevel (0);

			}
			
			if ( qualiSelect == "Fast")
			{
				PlayerPrefs.SetInt("GraphicQuality",1);
				QualitySettings.SetQualityLevel (1);
				
			}
			
			if ( qualiSelect == "Good")
			{
				PlayerPrefs.SetInt("GraphicQuality",2);
				QualitySettings.SetQualityLevel (2);
				
			}
			
			if ( qualiSelect == "High")
			{
				PlayerPrefs.SetInt("GraphicQuality",3);
				QualitySettings.SetQualityLevel (3);
				
			}
			
			if ( qualiSelect == "Ultra")
			{
				PlayerPrefs.SetInt("GraphicQuality",4);
				QualitySettings.SetQualityLevel (4);
			}
				

	}

	public void QualityIntConversion (int qualiInt)

	{
			
			if ( qualiInt == 0)
			{

				presets.value = "Fastest";
				
			}
			
			if ( qualiInt == 1)
			{

				presets.value = "Fast";
				
			}
			
			if ( qualiInt == 2)
			{

				presets.value = "Good";
				
			}
			
			if ( qualiInt == 3)
			{
				
				presets.value = "High";
				
			}
			
			if ( qualiInt == 4)
			{
				
				presets.value = "Ultra";
				
			}

		
	}

	public void Close ()
	{

	}
}
