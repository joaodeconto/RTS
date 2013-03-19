using UnityEngine;
using System.Collections;
using Visiorama;

public class EventManager : MonoBehaviour
{
	HUDController hudController;

	public EventManager Init()
	{
		hudController = ComponentGetter.Get<HUDController> ();

		return this;
	}

	public void AddEvent (string message, string spriteName = "")
	{
		Hashtable ht = new Hashtable();
		ht["message"] = message;

		hudController.CreateEnqueuedButtonInInspector ( "event-" + Time.time,
														"Events",
														ht,
														spriteName);
	}
}
