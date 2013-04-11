using UnityEngine;
using System.Collections;

public class GraphicQuality : MonoBehaviour
{
	private bool wasInitialized = false;

	Transform buttons;

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

		buttons = this.transform.FindChild ("Menu").FindChild ("Buttons");

		GameObject lowQuality = buttons.FindChild ("Low").gameObject;

		dcb = lowQuality.AddComponent<DefaultCallbackButton> ();
		dcb.Init ().Show (null, (ht_hud) =>
							{
								QualitySettings.SetQualityLevel (0);
							});

		GameObject midQuality = buttons.FindChild ("Mid").gameObject;

		dcb = midQuality.AddComponent<DefaultCallbackButton> ();
		dcb.Init ().Show (null, (ht_hud) =>
							{
								QualitySettings.SetQualityLevel (1);
							});

		GameObject highQuality = buttons.FindChild ("High").gameObject;

		dcb = highQuality.AddComponent<DefaultCallbackButton> ();
		dcb.Init ().Show (null, (ht_hud) =>
							{
								QualitySettings.SetQualityLevel (2);
							});
	}

	public void Close ()
	{

	}
}
