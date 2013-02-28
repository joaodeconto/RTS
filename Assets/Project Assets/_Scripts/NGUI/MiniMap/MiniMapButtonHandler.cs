using UnityEngine;
using System.Collections;
using Visiorama;

public class MiniMapButtonHandler : MonoBehaviour
{
	void OnClick()
	{
		ComponentGetter
			.Get<MiniMapController>()
				.UpdateCameraPosition();
	}
}
