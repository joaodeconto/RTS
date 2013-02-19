using UnityEngine;
using System.Collections;

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
	
	public bool DragOn {get; private set;}
	
	public Vector2 RelativePosition {get; private set;}
	
	public Vector3 FirstPosition {get; private set;} // primeiro toque
	public Vector3 CurrentPosition {get; private set;} // toque atual
	public Vector3 FinalPosition {get; private set;} // ultimo toque
	
	public Ray GetFirstRaycast {get; private set;}
	public Ray GetFinalRaycast {get; private set;}
	
	public Vector3 GetFirstPoint {get; private set;}
	public Vector3 GetFinalPoint {get; private set;}
	
	public TouchType touchType {get; private set;}
	public IdTouch idTouch {get; private set;}
	
#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR
	protected bool multitouch = false;
#endif
	
	public void Init ()
	{
		if (mainCamera == null) mainCamera = Camera.mainCamera;
		touchType = TouchType.None;
		DragOn = false;
	}
	
	void Update ()
	{
		RelativePosition = new Vector2 (Mathf.Clamp (Input.mousePosition.x / Screen.width, 0f, 1f), 
											 Mathf.Clamp (Input.mousePosition.y / Screen.height, 0f, 1f));
		
		if (Input.GetMouseButtonDown(0) || 
			Input.GetMouseButtonDown(1))
		{
			FirstPosition = new Vector3(Input.mousePosition.x,
										Screen.height - Input.mousePosition.y,
										Input.mousePosition.z);
			
			GetFirstRaycast = mainCamera.ScreenPointToRay (Input.mousePosition);
			
			RaycastHit hit;
			if (Physics.Raycast (GetFirstRaycast, out hit))
			{
				GetFirstPoint = hit.point;
			}
			
			touchType = TouchType.First;
			
			VerifyTouchID ();
		}
		else
		if (Input.GetMouseButton(0) || 
			Input.GetMouseButton(1))
		{
			CurrentPosition = new Vector2(Input.mousePosition.x,
										Screen.height - Input.mousePosition.y);
			
			if (Mathf.Abs (CurrentPosition.magnitude - FirstPosition.magnitude) > 1f)
			{
				DragOn = true;
			}
			
			touchType = TouchType.Press;
			
			VerifyTouchID ();
		}
		else
		if (Input.GetMouseButtonUp(0) || 
			Input.GetMouseButtonUp(1))
		{
			FinalPosition = new Vector3(Input.mousePosition.x,
										Screen.height - Input.mousePosition.y,
										Input.mousePosition.z);
			
			GetFinalRaycast = mainCamera.ScreenPointToRay (Input.mousePosition);
			
			RaycastHit hit;
			if (Physics.Raycast (GetFinalRaycast, out hit))
			{
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
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)) idTouch = IdTouch.Id0;
		else idTouch = IdTouch.Id1;
#endif
	}
}
