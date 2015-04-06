using UnityEngine;
using System.Collections;

public class GraphicQuality : MonoBehaviour
{
	private bool wasInitialized = false;

	public UIPopupList presets;
	public UIPopupList texture;
	public UIPopupList shadow;
	public int QualiSelected;
	public GameObject confirmCG;


	public void OnEnable ()
	{
		Open ();
	}

	public void OnDisable ()
	{
		Close ();
	}

	public void BoolActiveConfirm()
	{
		if (confirmCG.activeSelf == true) confirmCG.SetActive(false);
		else	confirmCG.SetActive(true);
	}

	public void Open ()
	{
		if (wasInitialized)	return;
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
						Close();
					});
		}
	}

	public void QualityStringConversion (string qualiSelect)
	{

			if ( qualiSelect =="Fastest")
			{
				QualiSelected = 0;
				QualitySettings.SetQualityLevel (0);
			}
			
			if ( qualiSelect == "Fast")
			{
				QualiSelected = 1;
				QualitySettings.SetQualityLevel (1);
			}
			
			if ( qualiSelect == "Good")
			{
				QualiSelected = 2;
				QualitySettings.SetQualityLevel (2);
			}
			
			if ( qualiSelect == "High")
			{
				QualiSelected = 3;
				QualitySettings.SetQualityLevel (3);	
			}
			
			if ( qualiSelect == "Ultra")
			{
				QualiSelected = 4;
				QualitySettings.SetQualityLevel (4);
			}
	}

	public void QualityIntConversion (int qualiInt)
	{			
			if ( qualiInt == 0)presets.value = "Fastest";			
			if ( qualiInt == 1)presets.value = "Fast";
			if ( qualiInt == 2)presets.value = "Good";			
			if ( qualiInt == 3)presets.value = "High";				
			if ( qualiInt == 4)presets.value = "Ultra";
	}

	public void SaveGCOptions(int quali)
	{
		PlayerPrefs.SetInt("GraphicQuality",QualiSelected);
	}

	public void RevertSetting()
	{
		QualiSelected = PlayerPrefs.GetInt("GraphicQuality");
		QualitySettings.SetQualityLevel (QualiSelected);
		QualityIntConversion(PlayerPrefs.GetInt("GraphicQuality"));
	}

	public void Close ()
	{
		gameObject.SetActive(false);
	}
}
