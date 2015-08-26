using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama.Utils;
using Visiorama;

public class EventController : MonoBehaviour
{
	[System.Serializable]
	public class Event
	{
		public string Name;
		public string Message;
		public string spriteName;
		public Vector3 eventPosition;
		public GameObject hudFeedback;
		public string sfxFeedback;
	}

	public float timer = 0f;
	public bool setTimerCount = false;
	public List<EventController.Event> events;

	HUDController hudController;

	public EventController Init()
	{
		hudController = ComponentGetter.Get<HUDController> ();

		return this;
	}
	public void AddEvent (string eventName, string param = "", string spriteName = "" )
	{
		EventController.Event e = GetEvent(eventName);
		
		Hashtable ht = new Hashtable();
		ht["message"] = string.Format(e.Message, param);
		
		if(string.IsNullOrEmpty(spriteName))
			spriteName = e.spriteName;

		if(e.hudFeedback)e.hudFeedback.SetActive(true);

		if(e.sfxFeedback != null)
		{
			AudioClip sfxFbk = SoundManager.Load(e.sfxFeedback);				
			SoundManager.PlaySFX(sfxFbk);
		}

		hudController.CreateEnqueuedButtonInInspector ( "event-" + Time.time, "Events", ht,spriteName);
	}

	public void AddEvent (string eventName, Vector3 eventPosition, string param = "", string spriteName = "" )
	{
		EventController.Event e = GetEvent(eventName);

		Hashtable ht = new Hashtable();
		ht["message"] = string.Format(e.Message, param);

		if(string.IsNullOrEmpty(spriteName))
			spriteName = e.spriteName;

		if(e.hudFeedback)e.hudFeedback.SetActive(true);
		
		if(e.sfxFeedback != null)
		{
			AudioClip sfxFbk = SoundManager.Load(e.sfxFeedback);				
			SoundManager.PlaySFX(sfxFbk);
		}


		hudController.CreateEnqueuedButtonInInspector ( "event-" + Time.time, "Events", ht, spriteName,
		                                                (ht_dcb) => 
		                                               {
															if (eventPosition != Vector3.zero)
															eventPosition.y = 0.0f;
																											
															Math.CenterCameraInObject (Camera.main, eventPosition);
														});
	}

	public void AddEvent (string eventName, GameObject hudFeedback, string param = "", string spriteName = "" )
	{
		EventController.Event e = GetEvent(eventName);
		
		Hashtable ht = new Hashtable();
		ht["message"] = string.Format(e.Message, param);
		
		if(string.IsNullOrEmpty(spriteName))
			spriteName = e.spriteName;

		if(e.hudFeedback)e.hudFeedback.SetActive(true);
		
		if(e.sfxFeedback != null)
		{
			AudioClip sfxFbk = SoundManager.Load(e.sfxFeedback);				
			SoundManager.PlaySFX(sfxFbk);
		}

		hudController.CreateEnqueuedButtonInInspector ( "event-" + Time.time, "Events", ht, spriteName);
	}

	protected EventController.Event GetEvent(string name)
	{
		foreach(EventController.Event e in events) //return
		{
			if(e.Name.Equals(name))
				return e;
		}

		EventController.Event newEvent = new EventController.Event();
		newEvent.Name    = name;
		newEvent.Message = "standard message : argument => {0}";
		events.Add(newEvent);
		return newEvent;
	}
}
