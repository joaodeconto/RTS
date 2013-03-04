using UnityEngine;
using System.Collections;
using Visiorama;

public class MiniMapButtonHandler : MonoBehaviour
{
	void OnPress(bool isPressed)
	{
		if(isPressed)
			UpdateCameraPosition();
	}

	void OnClick()
	{
		UpdateCameraPosition();
	}

	void UpdateCameraPosition()
	{
		ComponentGetter
			.Get<MiniMapController>()
				.UpdateCameraPosition();
	}
}
