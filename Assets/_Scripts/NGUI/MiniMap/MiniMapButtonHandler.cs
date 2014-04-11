using UnityEngine;
using System.Collections;
using Visiorama;

public class MiniMapButtonHandler : MonoBehaviour
{
	void OnDrag (Vector2 position)
	{
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
				
		// SOH PRA TER COMMIT, DEPOIS APAGAR
	}
}
