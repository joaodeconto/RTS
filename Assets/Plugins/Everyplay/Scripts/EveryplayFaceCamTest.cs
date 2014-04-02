using UnityEngine;
using System.Collections;

public class EveryplayFaceCamTest : MonoBehaviour {
	private bool recordingPermissionGranted = false;
	private GameObject debugMessage = null;

	void Start() {
		Everyplay.SharedInstance.FaceCamRecordingPermission += CheckFaceCamRecordingPermission;
	}

	void Destroy() {
		Everyplay.SharedInstance.FaceCamRecordingPermission -= CheckFaceCamRecordingPermission;
	}

	private void CheckFaceCamRecordingPermission(bool granted) {
		recordingPermissionGranted = granted;

		if(!granted && !debugMessage) {
			debugMessage = new GameObject("FaceCamDebugMessage", typeof(GUIText));
			debugMessage.transform.position = new Vector3(0.5f, 0.5f, 0.0f);

			if(debugMessage != null) {
				GUIText debugMessageGuiText = debugMessage.GetComponent<GUIText>();

				if(debugMessageGuiText) {
					debugMessageGuiText.text = "Microphone access denied. FaceCam requires access to the microphone.\nPlease enable Microphone access from Settings / Privacy / Microphone.";
					debugMessageGuiText.alignment = TextAlignment.Center;
					debugMessageGuiText.anchor = TextAnchor.MiddleCenter;
				}
			}
		}
	}

	void OnGUI() {
		if(recordingPermissionGranted) {
			if(GUI.Button(new Rect(Screen.width - 10 - 158, 10, 158, 48), Everyplay.SharedInstance.FaceCamIsSessionRunning() ? "Stop FaceCam session" : "Start FaceCam session")) {
				if(Everyplay.SharedInstance.FaceCamIsSessionRunning()) {
					Everyplay.SharedInstance.FaceCamStopSession();
				}
				else {
					Everyplay.SharedInstance.FaceCamStartSession();
				}
	#if UNITY_EDITOR
				Debug.Log("Everyplay FaceCam is not available in the Unity editor. Please compile and run on a device.");
	#endif
			}
		}
		else {
			if(GUI.Button(new Rect(Screen.width - 10 - 158, 10, 158, 48), "Request REC permission")) {
				Everyplay.SharedInstance.FaceCamRequestRecordingPermission();
	#if UNITY_EDITOR
				Debug.Log("Everyplay FaceCam is not available in the Unity editor. Please compile and run on a device.");
	#endif
			}
		}
	}
}