#define DEBUG_DATABASE_CONNECTION

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using System.Text;

using Newtonsoft.Json;

public class Database : MonoBehaviour
{
	private class DatabaseCall
	{
		public CallbackDatabaseCall cdc;
		public System.Type objType;
		public Dictionary<string, string> parameters;

		public DatabaseCall Init ()
		{
			parameters = new Dictionary<string, string>();
			return this;
		}
	}

	public delegate void CallbackDatabaseCall (object obj);

	public const string wrapperURL = "http://www.visiorama.com.br/uploads/RTS2/database/access.php";

	private float timeToWait = 0.5f;

	private Stack<DatabaseCall> DatabaseCallStack;

	private bool wasInitialized = false;
	public void Init ()
	{
		if (wasInitialized)
			return;

		wasInitialized = true;

		DatabaseCallStack = new Stack<DatabaseCall> ();
	}

	public void Create (object sendObj, CallbackDatabaseCall cdc)
	{
		Create (sendObj, sendObj.GetType (), null, cdc);
	}

	public void Create (object sendObj, string requestName, CallbackDatabaseCall cdc)
	{
		Create (sendObj, sendObj.GetType (), requestName, cdc);
	}

	public void Create (object sendObj, Type dbType, string requestName, CallbackDatabaseCall cdc)
	{
		Init ();

		DatabaseCall dc = new DatabaseCall ().Init ();

		dc.objType = dbType;
		dc.cdc     = cdc;

		if (string.IsNullOrEmpty (requestName))
			requestName = "create-" + dbType.Name.ToLower ();

		dc.parameters.Add ("request", requestName);
		dc.parameters.Add ("data", JsonConvert.SerializeObject(sendObj));

#if DEBUG_DATABASE_CONNECTION
		Debug.Log ("parameters[\"request\"]: " + dc.parameters["request"]);
		Debug.Log ("parameters[\"data\"]: "    + dc.parameters["data"]);
#endif

		DatabaseCallStack.Push (dc);

		if (DatabaseCallStack.Count == 1)
			StartCoroutine (_SendData ());
	}

	public void Read (object sendObj, CallbackDatabaseCall cdc)
	{
		Read (sendObj, sendObj.GetType (), null, cdc);
	}

	public void Read (object sendObj, string requestName, CallbackDatabaseCall cdc)
	{
		Read (sendObj, sendObj.GetType (), requestName, cdc);
	}

	public void Read (object sendObj, Type dbType, string requestName, CallbackDatabaseCall cdc)
	{
		Init ();

		DatabaseCall dc = new DatabaseCall ().Init ();

		dc.objType = dbType;
		dc.cdc     = cdc;

		if (string.IsNullOrEmpty (requestName))
			requestName = "read-" + dbType.Name.ToLower ();

		dc.parameters.Add ("request", requestName);
		dc.parameters.Add ("data", JsonConvert.SerializeObject(sendObj));

#if DEBUG_DATABASE_CONNECTION
		Debug.Log ("parameters[\"request\"]: " + dc.parameters["request"]);
		Debug.Log ("parameters[\"data\"]: "    + dc.parameters["data"]);
#endif

		DatabaseCallStack.Push (dc);

		if (DatabaseCallStack.Count == 1)
			StartCoroutine (_SendData ());
	}

	public void Update (object sendObj, CallbackDatabaseCall cdc)
	{
		Update (sendObj, sendObj.GetType (), null, cdc);
	}

	public void Update (object sendObj, string requestName, CallbackDatabaseCall cdc)
	{
		Update (sendObj, sendObj.GetType (), requestName, cdc);
	}

	public void Update (object sendObj, Type dbType, string requestName, CallbackDatabaseCall cdc)
	{
		Init ();

		DatabaseCall dc = new DatabaseCall ().Init ();

		dc.objType = dbType;
		dc.cdc     = cdc;

		if (string.IsNullOrEmpty (requestName))
			requestName = "update-" + dbType.Name.ToLower ();

		dc.parameters.Add ("request", requestName);
		dc.parameters.Add ("data", JsonConvert.SerializeObject(sendObj));

#if DEBUG_DATABASE_CONNECTION
		Debug.Log ("parameters[\"request\"]: " + dc.parameters["request"]);
		Debug.Log ("parameters[\"data\"]: "    + dc.parameters["data"]);
#endif

		DatabaseCallStack.Push (dc);

		if (DatabaseCallStack.Count == 1)
			StartCoroutine (_SendData ());
	}

	public void Delete (object sendObj, CallbackDatabaseCall cdc)
	{
		Delete (sendObj, sendObj.GetType (), null, cdc);
	}

	public void Delete (object sendObj, string requestName, CallbackDatabaseCall cdc)
	{
		Delete (sendObj, sendObj.GetType (), requestName, cdc);
	}

	public void Delete (object sendObj, Type dbType, string requestName, CallbackDatabaseCall cdc)
	{
		Init ();

		DatabaseCall dc = new DatabaseCall ().Init ();

		dc.objType = dbType;
		dc.cdc     = cdc;

		if (string.IsNullOrEmpty (requestName))
			requestName = "delete-" + dbType.Name.ToLower ();

		dc.parameters.Add ("request", requestName);
		dc.parameters.Add ("data", JsonConvert.SerializeObject(sendObj));

#if DEBUG_DATABASE_CONNECTION
		Debug.Log ("parameters[\"request\"]: " + dc.parameters["request"]);
		Debug.Log ("parameters[\"data\"]: "    + dc.parameters["data"]);
#endif

		DatabaseCallStack.Push (dc);

		if (DatabaseCallStack.Count == 1)
			StartCoroutine (_SendData ());
	}

	public IEnumerator _SendData ()
	{
#if DEBUG_DATABASE_CONNECTION
		Debug.Log ("Has sent data");
#endif

		WWWForm form = new WWWForm ();

		DatabaseCall dc = DatabaseCallStack.Pop ();

		foreach (KeyValuePair <string, string> de in dc.parameters)
		{
			form.AddField (de.Key, de.Value);
		}

		dc.parameters.Clear ();

		WWW www = new WWW(wrapperURL, form);

		yield return www;

		if (www.error != null)
		{
#if DEBUG_DATABASE_CONNECTION
			Debug.LogError ("WWW Error: " + www.error);
#endif
			dc.cdc (null);
		}
		else
		{
#if DEBUG_DATABASE_CONNECTION
			Debug.Log ("WWW response: " + www.text);
#endif
			object obj;
			try {
				obj = JsonConvert.DeserializeObject (www.text, dc.objType);
			} catch (JsonSerializationException ex) {
#if DEBUG_DATABASE_CONNECTION
				Debug.LogError (ex.Message);
#endif
				obj = www.text;
			}

			//if (dc.cdc == null) { Debug.Log ("cdc é nulo?"); }

			dc.cdc (obj);
		}

		dc.cdc = null;

		if (DatabaseCallStack.Count != 0)
			StartCoroutine (_SendData ());
	}
}
