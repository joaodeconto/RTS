#define DEBUG_DATABASE_CONNECTION

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Text;

using Newtonsoft.Json;

public class Database : MonoBehaviour
{
	public delegate void CallbackDatabaseCall (object obj);

	public const string wrapperURL = "http://visiorama.com.br/uploads/RTS2/database/access.php";

	private float timeToWait = 0.5f;

	private CallbackDatabaseCall cdc;
	private System.Type objType;

	private Dictionary<string, string> parameters;

	private bool wasInitialized = false;
	public void Init ()
	{
		if (wasInitialized)
			return;

		wasInitialized = true;

		parameters = new Dictionary<string, string>();
	}

	public void Create (object obj, CallbackDatabaseCall cdc)
	{
		Init ();

		this.objType = obj.GetType ();
		this.cdc     = cdc;

		parameters.Add ("request", "create-" + obj.GetType ().Name.ToLower ());
		parameters.Add ("data", JsonConvert.SerializeObject(obj));

#if DEBUG_DATABASE_CONNECTION
		Debug.Log ("parameters[\"request\"]: " + parameters["request"]);
		Debug.Log ("parameters[\"data\"]: "    + parameters["data"]);
#endif

		StartCoroutine (_SendData ());
	}

	public void Read (object obj, CallbackDatabaseCall cdc)
	{
		Init ();

		this.objType = obj.GetType ();
		this.cdc     = cdc;

		parameters.Add ("request", "get-" + obj.GetType ().Name.ToLower ());
		parameters.Add ("data", JsonConvert.SerializeObject(obj));

#if DEBUG_DATABASE_CONNECTION
		Debug.Log ("parameters[\"request\"]: " + parameters["request"]);
		Debug.Log ("parameters[\"data\"]: "    + parameters["data"]);
#endif

		StartCoroutine (_SendData ());
	}

	public void Update (object obj, CallbackDatabaseCall cdc)
	{
		Init ();

		this.objType = obj.GetType ();
		this.cdc = cdc;

		parameters.Add ("request", "update-" + obj.GetType ().Name.ToLower ());
		parameters.Add ("data", JsonConvert.SerializeObject(obj));

#if DEBUG_DATABASE_CONNECTION
		Debug.Log ("parameters[\"request\"]: " + parameters["request"]);
		Debug.Log ("parameters[\"data\"]: "    + parameters["data"]);
#endif

		StartCoroutine (_SendData ());
	}

	public void Delete (object obj, CallbackDatabaseCall cdc)
	{
		Init ();

		this.objType = obj.GetType ();
		this.cdc = cdc;

		parameters.Add ("request", "delete-" + obj.GetType ().Name.ToLower ());
		parameters.Add ("data", JsonConvert.SerializeObject(obj));

#if DEBUG_DATABASE_CONNECTION
		Debug.Log ("parameters[\"request\"]: " + parameters["request"]);
		Debug.Log ("parameters[\"data\"]: " + parameters["data"]);
#endif

		StartCoroutine (_SendData ());
	}

	public IEnumerator _SendData ()
	{
#if DEBUG_DATABASE_CONNECTION
		Debug.Log ("Has sent data");
#endif

		WWWForm form = new WWWForm ();

		foreach (KeyValuePair <string, string> de in parameters)
		{
			form.AddField (de.Key, de.Value);
		}

		WWW www = new WWW(wrapperURL, form);

		yield return www;

		if (www.error != null)
		{
#if DEBUG_DATABASE_CONNECTION
			Debug.LogError ("WWW Error: " + www.error);
#endif
			cdc (null);
		}
		else
		{
#if DEBUG_DATABASE_CONNECTION
			Debug.LogError ("WWW response: " + www.text);
#endif
			object obj;
			try {
				obj = JsonConvert.DeserializeObject (www.text, objType);
			} catch (JsonSerializationException ex) {
#if DEBUG_DATABASE_CONNECTION
			Debug.LogError (ex.Message);
#endif
				obj = www.text;
			}

			cdc (obj);
		}

		cdc = null;
		parameters.Clear ();
	}
}
