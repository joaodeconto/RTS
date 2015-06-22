using UnityEngine;
using System.Collections;
using Visiorama;
using Visiorama.Utils;

public class TouchController : MonoBehaviour
{
	[System.Serializable]
	public class ZoomSettings
	{
		public Camera[] cameras;
		public float zoomSpeed;
		public MinMaxFloat zoom;
	}
	
	public enum TouchType
	{
		First,
		Press,
		Drag,
		Drop,
		Ended,
		None,
	}
	
	public enum IdTouch
	{
		Id0,
		Id1,
		None
	}
	
	public Camera mainCamera;
	public string[] layersToIgnore;
	public ZoomSettings zoomSettings;
	public float doubleClickSpeed;
	
	protected Camera[] camerasUI;
	
	public bool DragOn {get; private set;}
	public bool DisableDragOn {get; set;}
	public bool holdCounting{get; set;}
	
	public bool DoubleClick {get; private set;}
	public bool touchHold {get; private set;}
	public float touchTimer {get; private set;}
	
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
	
	//        protected SoundSource soundSource;
	
	protected Vector3 windowSize = Vector3.zero;
	
	protected bool ignoreTouch = false;
	
	#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR
	protected bool multitouch = false;
	#endif
	
	public void Init ()
	{
		if (mainCamera == null) mainCamera = Camera.main;
		doubleClickSpeed = PlayerPrefs.GetFloat("DoubleClickSpeed");
		touchType = TouchType.None;
		DragOn = false;
		camerasUI = new Camera[layersToIgnore.Length];
		for (int i = 0; i != layersToIgnore.Length; i++)
		{
			camerasUI[i] = NGUITools.FindCameraForLayer(LayerMask.NameToLayer (layersToIgnore[i]));
		}
		//                soundSource = GetComponent<SoundSource> ();
	}
	
	void Update ()
	{
		RelativePosition = new Vector2 (Mathf.Clamp (Input.mousePosition.x / Screen.width, 0f, 1f),
		                                Mathf.Clamp (Input.mousePosition.y / Screen.height, 0f, 1f));
		if(holdCounting) touchTimer += Time.deltaTime;
		
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
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
			touchHold = false;
			holdCounting = true;
			VerifyTouchID ();
		}
		else if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
		{
			if (ignoreTouch) return;
			
			CurrentPosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			
			if (!DisableDragOn)
			{
				if (!DragOn)
				{
					if (Mathf.Abs (CurrentPosition.magnitude - FirstPosition.magnitude) > 8f)

					{
						DragOn = true;
					}
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
			
			//                if (!DragOn) soundSource.Play ("Click");
			
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
			
			VerifyTouchID ();			
			DoubleClick = GetDoubleClick (doubleClickSpeed);		
			touchType = TouchType.Ended;
			if(touchTimer > 0.3f) touchHold = true;
			holdCounting = false;
			touchTimer = 0;
		}
		else
		{
			CurrentPosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);			
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
	
	protected float   lastTimeDoubleClick = 0f;
	protected int           countClick = 0;
	protected IdTouch lastTouch;
	
	protected bool GetDoubleClick (float maxTimeToReclick)
	{
		#if UNITY_IPHONE && !UNITY_EDITOR

		if(Input.GetTouch(0).tapCount == 2) return true;

		#else

		if (idTouch != lastTouch)
		{
			countClick = 0;
			
			lastTouch = idTouch;
		}
		
		if (countClick == 0)
		{
			countClick++;
			lastTimeDoubleClick = Time.time;
		}
		else
		{
			countClick = 0;
			
			if (Time.time - lastTimeDoubleClick < maxTimeToReclick)
			{
				return true;
			}
		}
		#endif
		
		return false;
	}
	
	public bool IsInCamera (Vector3 position)
	{
		Vector3 pos = mainCamera.WorldToViewportPoint (position);
		
		return (pos.z > 0f && pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f);
	}
	
	public Rect GetDragInvertedRect ()
	{
		return new Rect(Mathf.Min(FirstPosition.x, CurrentPosition.x),  Screen.height - Mathf.Min(FirstPosition.y, CurrentPosition.y),
		                Mathf.Abs(FirstPosition.x - CurrentPosition.x), Mathf.Abs(FirstPosition.y - CurrentPosition.y));
	}
	
	public Rect GetDragRect ()
	{
		return new Rect(FirstPosition.x - CurrentPosition.x, FirstPosition.y - CurrentPosition.y,
		                Mathf.Abs(FirstPosition.x - CurrentPosition.x), Mathf.Abs(FirstPosition.y - CurrentPosition.y));
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

	public void SetDoubleClick (float dc)
	{
			doubleClickSpeed = dc;
	}

}