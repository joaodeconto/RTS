using UnityEngine;
using System.Collections;

using Visiorama;
using Visiorama.Utils;

public class CameraMovement : MonoBehaviour
{
	public float speed;
	public Vector2 minimum = Vector2.one * 0.01f;
	public Vector2 maximum = Vector2.one * 0.99f;
	public float smoothFactor;
	public Vector3 velocity = Vector3.zero;
	public float timeToDamp = 0.3f;
	private float startAccelY, startAccelX, startAccelZ;
	
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
		speed = PlayerPrefs.GetFloat("TouchSense");		
		scenario = bounds.scenario;		
		thisCamera = gameObject.camera;
		startAccelY = Input.acceleration.y;
		startAccelX = Input.acceleration.x;
		startAccelZ = Input.acceleration.z;
		
		foreach (Camera camera in touchController.zoomSettings.cameras)
		{
			camera.fieldOfView = thisCamera.fieldOfView;
		}
	}

	void FixedUpdate ()
	{

#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR

		smoothFactor = 8;
		if (touchController.touchType == TouchController.TouchType.Press)
		{
			Vector3 newPos = transform.position - touchController.RelativeTwoFingersPosition * (speed*2);
			//transform.position = Vector3.SmoothDamp(transform.position, newPos, ref velocity, smoothFactor, Mathf.Infinity, timeToDamp);
			transform.position = Vector3.Lerp(transform.position, newPos, smoothFactor * Time.deltaTime);
		}
		

#else	

		smoothFactor = speed/8;
		Vector3 dir = Vector3.zero;
		dir.z = Input.acceleration.z - startAccelZ;
		dir.x = Input.acceleration.x - startAccelX;
		if (dir.sqrMagnitude > 1
		    ){

			dir.Normalize();
			transform.position += dir;
		}


		float v = Input.GetAxis ("Vertical");
		float h = Input.GetAxis ("Horizontal");
		PanCamera (v * speed, h * speed);
		
		if (touchController.touchType == TouchController.TouchType.Press ||
			h != 0 || v != 0) return;

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
	}

	// Adicionar metodo na biblioteca  
	public void PanCamera (float dForward, float dRight)
    {
      Transform transform = Camera.main.transform;
      Vector3 normalized = Vector3.Cross(Vector3.up, transform.forward).normalized;
      Vector3 vector3_1 = Vector3.Cross(normalized, Vector3.up);
      Vector3 vector3_2 = transform.position + normalized * dRight + vector3_1 * dForward;
	  transform.position = Vector3.SmoothDamp(transform.position, vector3_2, ref velocity, smoothFactor, Mathf.Infinity, timeToDamp);
    }
}
