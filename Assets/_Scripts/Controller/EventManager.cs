using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;

public class EventManager : MonoBehaviour
{
	[System.Serializable]
	public class Event
	{
		public string Name;
		public string Message;
		public string spriteName;
	}

	public List<EventManager.Event> events;

	HUDController hudController;

	public EventManager Init()
	{
		hudController = ComponentGetter.Get<HUDController> ();

		return this;
	}

	public void AddEvent (string eventName, string param = "", string spriteName = "")
	{
		EventManager.Event e = GetEvent(eventName);

		Hashtable ht = new Hashtable();
		ht["message"] = string.Format(e.Message, param);

		if(string.IsNullOrEmpty(spriteName))
			spriteName = e.spriteName;

		hudController.CreateEnqueuedButtonInInspector ( "event-" + Time.time,
														"Events",
														ht,
														spriteName);
	}

	protected EventManager.Event GetEvent(string name)
	{
		foreach(EventManager.Event e in events) //return
		{
			if(e.Name.Equals(name))
				return e;
		}

		EventManager.Event newEvent = new EventManager.Event();

		newEvent.Name    = name;
		newEvent.Message = "standard message : argument => {0}";

		events.Add(newEvent);

		return newEvent;
	}
}
