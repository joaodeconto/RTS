using UnityEngine;
using System.Collections;
using Visiorama;

public class CameraMovement : MonoBehaviour {
	public float speedMobile = 0.25f;
	public float speed = 0.5f;
	public Vector2 minimum = Vector2.one * 0.01f;
	public Vector2 maximum = Vector2.one * 0.99f;

	public FactoryBase[] factorys;
	
	protected TouchController touchController;

	void Start ()
	{
		touchController = ComponentGetter.Get<TouchController>();
//		enabled = false;
		
		foreach (FactoryBase fb in factorys)
		{
			if (ComponentGetter.Get<GameplayManager>().IsSameTeam(fb))
			{
				transform.position = fb.transform.position;
			}
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
		if (touchController.touchType == TouchController.TouchType.Press) return;

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
