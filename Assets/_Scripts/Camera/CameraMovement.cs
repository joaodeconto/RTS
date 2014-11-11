using UnityEngine;
using System.Collections;

using Visiorama;
using Visiorama.Utils;

public class CameraMovement : MonoBehaviour
{
	public float speedMobile;
	public float speed;
	public Vector2 minimum = Vector2.one * 0.01f;
	public Vector2 maximum = Vector2.one * 0.99f;
	
	public float zoomSpeed;
	public MinMaxFloat zoom;
	
	protected Camera thisCamera;
	
	protected TouchController touchController;
	
	protected Vector3MinMax scenario;
	protected CameraBounds bounds;
	
	void Start ()
	{
		touchController = ComponentGetter.Get<TouchController>();
		bounds = GetComponent<CameraBounds> ();

		speedMobile = PlayerPrefs.GetFloat("TouchSense");
		speed = PlayerPrefs.GetFloat("TouchSense");
		
		scenario = bounds.scenario;
		
		thisCamera = gameObject.camera;
		
		foreach (Camera camera in touchController.zoomSettings.cameras)
		{
			camera.fieldOfView = thisCamera.fieldOfView;
		}
	}

	void Update ()
	{
#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR

		if (touchController.touchType == TouchController.TouchType.Press)
		{
			transform.position -= (touchController.RelativeTwoFingersPosition * speedMobile);
		}

#else
		float v = Input.GetAxis ("Vertical");
		float h = Input.GetAxis ("Horizontal");
		PanCamera (v * speed, h * speed);
		
		if (touchController.touchType == TouchController.TouchType.Press ||
			h != 0 || v != 0) return;
		
//		if (Input.GetAxis ("Mouse ScrollWheel") != 0)
//		{
//			float size = thisCamera.orthographicSize;
//			
//			size -= Input.GetAxis ("Mouse ScrollWheel") * zoomSpeed;
//			
//			thisCamera.fieldOfView = Mathf.Clamp (size, zoom.min, zoom.max);
//						
//		
////			float movementForceDirection = Input.GetAxis ("Mouse ScrollWheel") * zoomSpeed;
////			Vector3 cameraPosition = thisCamera.transform.position;
////			Vector3 impulse = thisCamera.transform.forward.normalized * movementForceDirection;
////			
////
////			
////			if ((movementForceDirection > 0 && ((impulse + cameraPosition).y > scenario.y.min)) ||
////			    (movementForceDirection < 0 && ((impulse + cameraPosition).y < scenario.y.max)))
////			{
////				cameraPosition += impulse;
////				
////				float screenRatio = (float)Screen.width / (float)Screen.height;
////				float angleRatio = thisCamera.transform.eulerAngles.x / 180f; 
////				float fieldOfViewRatio = (thisCamera.orthographicSize / 150f);
////				
////				scenario.x.min -= movementForceDirection * fieldOfViewRatio * screenRatio;
////				scenario.x.max += movementForceDirection * fieldOfViewRatio * screenRatio;
////				scenario.z.min += 2f * movementForceDirection * (1f / angleRatio) * fieldOfViewRatio * (1f / screenRatio);
////				scenario.z.max += 2f * movementForceDirection * (1f / angleRatio) * fieldOfViewRatio * (1f / screenRatio);
////			
////				thisCamera.transform.position = cameraPosition;
////				foreach (Camera camera in touchController.zoomSettings.cameras)
////				{
////					camera.transform.position = cameraPosition;
////				}
////			}
//		}

		if (touchController.RelativePosition.x <= minimum.x && touchController.RelativePosition.x >= 0f)
			PanCamera (0f, -speed * (1f - touchController.RelativePosition.x));
		if (touchController.RelativePosition.x >= maximum.x-0.01f && touchController.RelativePosition.x <= 1f)
			PanCamera (0f, speed * touchController.RelativePosition.x);
		if (touchController.RelativePosition.y <= minimum.y && touchController.RelativePosition.y >= 0f)
			PanCamera (-speed * (1f - touchController.RelativePosition.y), 0f);
		if (touchController.RelativePosition.y >= maximum.y-0.01f && touchController.RelativePosition.y <= 1f)
			PanCamera (speed * touchController.RelativePosition.y, 0f);
#endif
	}

	public void SetSpeed () 

	{
		speed = PlayerPrefs.GetFloat("TouchSense");
		speedMobile = PlayerPrefs.GetFloat("TouchSense");
	
	}

	// Adicionar metodo na biblioteca
	public void PanCamera (float dForward, float dRight)
    {
      Transform transform = Camera.main.transform;
      Vector3 normalized = Vector3.Cross(Vector3.up, transform.forward).normalized;
      Vector3 vector3_1 = Vector3.Cross(normalized, Vector3.up);
      Vector3 vector3_2 = transform.position + normalized * dRight + vector3_1 * dForward;
      transform.position = vector3_2;
    }
}
