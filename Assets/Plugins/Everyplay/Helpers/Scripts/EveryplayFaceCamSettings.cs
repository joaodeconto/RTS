using UnityEngine;
using System.Collections;

public class EveryplayFaceCamSettings : MonoBehaviour {

	public bool previewVisible = true;

	public int iPhonePreviewSideWidth = 64;
	public int iPhonePreviewPositionX = 16;
	public int iPhonePreviewPositionY = 16;
	public int iPhonePreviewBorderWidth = 2;
	
	public int iPadPreviewSideWidth = 96;
	public int iPadPreviewPositionX = 24;
	public int iPadPreviewPositionY = 24;
	public int iPadPreviewBorderWidth = 2;

	public Color previewBorderColor = Color.white;

	public Everyplay.FaceCamPreviewOrigin previewOrigin = Everyplay.FaceCamPreviewOrigin.BottomRight;

	public bool previewScaleRetina = true;

	public bool audioOnly = false;

	void Start () {
		if(Everyplay.SharedInstance.GetUserInterfaceIdiom() == (int) Everyplay.UserInterfaceIdiom.iPad) {
			Everyplay.SharedInstance.FaceCamSetPreviewSideWidth(iPadPreviewSideWidth);
			Everyplay.SharedInstance.FaceCamSetPreviewBorderWidth(iPadPreviewBorderWidth);
			Everyplay.SharedInstance.FaceCamSetPreviewPositionX(iPadPreviewPositionX);
			Everyplay.SharedInstance.FaceCamSetPreviewPositionY(iPadPreviewPositionY);
		}
		else {
			Everyplay.SharedInstance.FaceCamSetPreviewSideWidth(iPhonePreviewSideWidth);
			Everyplay.SharedInstance.FaceCamSetPreviewBorderWidth(iPhonePreviewBorderWidth);
			Everyplay.SharedInstance.FaceCamSetPreviewPositionX(iPhonePreviewPositionX);
			Everyplay.SharedInstance.FaceCamSetPreviewPositionY(iPhonePreviewPositionY);
		}
		
		Everyplay.SharedInstance.FaceCamSetPreviewBorderColor(previewBorderColor.r, previewBorderColor.g, previewBorderColor.b, previewBorderColor.a);
		Everyplay.SharedInstance.FaceCamSetPreviewOrigin(previewOrigin);
		Everyplay.SharedInstance.FaceCamSetPreviewScaleRetina(previewScaleRetina);
		Everyplay.SharedInstance.FaceCamSetPreviewVisible(previewVisible);
		
		Everyplay.SharedInstance.FaceCamSetAudioOnly(audioOnly);
	}
}