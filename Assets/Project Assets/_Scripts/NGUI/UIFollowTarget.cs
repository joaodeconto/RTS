using UnityEngine;

public class UIFollowTarget : MonoBehaviour
{
	public Transform target;

	public bool disableIfInvisible = true;

	public Camera mGameCamera { get; set; }
	public Camera mUICamera { get; set; }

	Transform mTrans;
	bool mIsVisible = false;

	void Awake () { mTrans = transform; }

	void Start()
	{
		if (target != null)
		{
			if (mGameCamera == null) mGameCamera = NGUITools.FindCameraForLayer(target.gameObject.layer);
			if (mUICamera == null) mUICamera = NGUITools.FindCameraForLayer(gameObject.layer);
			SetVisible(false);
		}
		else
		{
			Debug.LogError("Expected to have 'target' set to a valid transform", this);
			enabled = false;
		}
	}

	void SetVisible (bool val)
	{
		mIsVisible = val;

		for (int i = 0, imax = mTrans.childCount; i < imax; ++i)
		{
			NGUITools.SetActive(mTrans.GetChild(i).gameObject, val);
		}
	}

	void Update ()
	{
		if (target == null)
		{
			Destroy (this.gameObject);
			return;
		}

		Vector3 pos = mGameCamera.WorldToViewportPoint(target.position);

		// Determine the visibility and the target alpha
		bool isVisible = (pos.z > 0f && pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f);

		// Update the visibility flag
		if (disableIfInvisible && mIsVisible != isVisible) SetVisible(isVisible);

		// If visible, update the position
		if (isVisible)
		{
			mGameCamera.ResetAspect();
			  mUICamera.ResetAspect();

			//pos.x *= (mUICamera.pixelWidth  / mGameCamera.pixelWidth);
			//pos.y *= (mUICamera.pixelHeight / mGameCamera.pixelHeight);

			transform.position = mUICamera.ViewportToWorldPoint(pos);
			pos = mTrans.localPosition;
			pos.x = Mathf.RoundToInt(pos.x);
			pos.y = Mathf.RoundToInt(pos.y);
			pos.z = 0f;
			mTrans.localPosition = pos;
		}
	}
}
