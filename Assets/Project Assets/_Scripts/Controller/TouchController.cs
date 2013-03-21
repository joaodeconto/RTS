using UnityEngine;
using System.Collections;
using Visiorama;

public class TouchController : MonoBehaviour
{
	public enum TouchType
	{
		First,
		Press,
		Drag,
		Drop,
		Ended,
		None
	}

	public enum IdTouch
	{
		Id0,
		Id1,
		None
	}

	public Camera mainCamera;
	public string[] layersToIgnore;

	protected Camera[] camerasUI;

	public bool DragOn {get; private set;}
	public bool DisableDragOn {get; set;}

	public Vector2 RelativePosition {get; private set;}

	public Vector3 FirstPosition {get; private set;} // primeiro toque
	public Vector3 CurrentPosition {get; private set;} // toque atual
	public Vector3 FinalPosition {get; private set;} // ultimo toque

	public Ray GetFirstRay {get; private set;}
	public Ray GetFinalRay {get; private set;}

	public RaycastHit GetFirstRaycastHit {get; private set;}
	public RaycastHit GetFinalRaycastHit {get; private set;}

	public Vector3 GetFirstPoint {get; private set;}
	public Vector3 GetFinalPoint {get; private set;}

	public TouchType touchType {get; private set;}
	public IdTouch idTouch {get; private set;}

	protected Vector3 windowSize = Vector3.zero;

	protected bool ignoreTouch = false;

#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR
	protected bool multitouch = false;
#endif

	public void Init ()
	{
		if (mainCamera == null) mainCamera = Camera.mainCamera;
		touchType = TouchType.None;
		DragOn = false;
		camerasUI = new Camera[layersToIgnore.Length];
		for (int i = 0; i != layersToIgnore.Length; i++)
		{
			camerasUI[i] = NGUITools.FindCameraForLayer(LayerMask.NameToLayer (layersToIgnore[i]));
		}
	}

	void Update ()
	{
		RelativePosition = new Vector2 (Mathf.Clamp (Input.mousePosition.x / Screen.width, 0f, 1f),
											 Mathf.Clamp (Input.mousePosition.y / Screen.height, 0f, 1f));

		if (Input.GetMouseButtonDown(0) ||
			Input.GetMouseButtonDown(1))
		{
			if (NGUIUtils.ClickedInGUI (camerasUI, "GUI"))
			{
				ignoreTouch = true;
				return;
			}

			FirstPosition = new Vector3(Input.mousePosition.x,
										Screen.height - Input.mousePosition.y,
										Input.mousePosition.z);

			GetFirstRay = mainCamera.ScreenPointToRay (Input.mousePosition);

			RaycastHit hit;
			if (Physics.Raycast (GetFirstRay, out hit))
			{
				GetFirstRaycastHit = hit;
				GetFirstPoint = hit.point;
			}

			touchType = TouchType.First;

			VerifyTouchID ();
		}
		else
		if (Input.GetMouseButton(0) ||
			Input.GetMouseButton(1))
		{
			if (ignoreTouch) return;

			CurrentPosition = new Vector2(Input.mousePosition.x,
										Screen.height - Input.mousePosition.y);

			if (!DisableDragOn)
			{
				if (Mathf.Abs (CurrentPosition.magnitude - FirstPosition.magnitude) > 1f)
				{
					DragOn = true;
				}
			}

			touchType = TouchType.Press;

			VerifyTouchID ();
		}
		else
		if (Input.GetMouseButtonUp(0) ||
			Input.GetMouseButtonUp(1))
		{
			DisableDragOn = false;

			if (ignoreTouch)
			{
				ignoreTouch = false;
				return;
			}
			else
			if (NGUIUtils.ClickedInGUI (camerasUI, "GUI")) return;

			FinalPosition = new Vector3(Input.mousePosition.x,
										Screen.height - Input.mousePosition.y,
										Input.mousePosition.z);

			GetFinalRay = mainCamera.ScreenPointToRay (Input.mousePosition);

			RaycastHit hit;
			if (Physics.Raycast (GetFinalRay, out hit))
			{
				GetFinalRaycastHit = hit;
				GetFinalPoint = hit.point;
			}

			touchType = TouchType.Ended;

			VerifyTouchID ();
		}
		else
		{
			CurrentPosition = new Vector2(Input.mousePosition.x,
										Screen.height - Input.mousePosition.y);

			touchType = TouchType.None;
#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR
			VerifyTouchID ();
#else
			idTouch = IdTouch.None;
#endif
			DragOn = false;
		}
	}

	public Vector3 RelativeTwoFingersPosition
	{
		get
		{
			float x = ((Input.GetTouch(0).deltaPosition.x+Input.GetTouch(1).deltaPosition.x));
			float z = ((Input.GetTouch(0).deltaPosition.y+Input.GetTouch(1).deltaPosition.y));

			return new Vector3(x, 0, z);
		}
	}

	public Bounds GetTouchBounds()
	{
		Vector3 first = GetFirstPoint;
		Vector3 final = GetFinalPoint;

		windowSize.x = Mathf.Abs(first.x - final.x);
		windowSize.y = Mathf.Abs(first.y - final.y);
		windowSize.z = Mathf.Abs(first.z - final.z);

		return new Bounds((GetFirstPoint+GetFinalPoint)/2, windowSize + (Vector3.up * 999999f) );
	}

	void VerifyTouchID ()
	{
#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR
		if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)) &&
			(Input.GetMouseButtonDown(1) || Input.GetMouseButton(1) || Input.GetMouseButtonUp(1)))
			idTouch = IdTouch.Id1;
		else if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
		{
			if (idTouch != IdTouch.Id1)
			{
				idTouch = IdTouch.Id0;
			}
		}
		else idTouch = IdTouch.None;
#else
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
			idTouch = IdTouch.Id0;
		else if (Input.GetMouseButtonDown(1) || Input.GetMouseButton(1) || Input.GetMouseButtonUp(1))
			idTouch = IdTouch.Id1;
#endif
	}
}
