using UnityEngine;
using System.Collections;
using Visiorama;
using Visiorama.Utils;

public class CameraMovement : MonoBehaviour {
	public float speedMobile = 0.25f;
	public float speed = 0.5f;
	public Vector2 minimum = Vector2.one * 0.01f;
	public Vector2 maximum = Vector2.one * 0.99f;
	
	public float zoomSpeed;
	public MinMaxFloat zoom;
		
	protected Camera thisCamera;
	
	protected TouchController touchController;

	void Start ()
	{
		touchController = ComponentGetter.Get<TouchController>();
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
		
		if (Input.GetAxis ("Mouse ScrollWheel") != 0)
		{
			float size = thisCamera.fieldOfView;
			size -= Input.GetAxis ("Mouse ScrollWheel") * zoomSpeed;
			thisCamera.fieldOfView = Mathf.Clamp (size, zoom.min, zoom.max);
			foreach (Camera camera in touchController.zoomSettings.cameras)
			{
				camera.fieldOfView = thisCamera.fieldOfView;
			}
		}

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

	// Add Códigos na Framework
	public void PanCamera (float dForward, float dRight)
    {
      Transform transform = Camera.main.transform;
      Vector3 normalized = Vector3.Cross(Vector3.up, transform.forward).normalized;
      Vector3 vector3_1 = Vector3.Cross(normalized, Vector3.up);
      Vector3 vector3_2 = transform.position + normalized * dRight + vector3_1 * dForward;
      transform.position = vector3_2;
    }
}
