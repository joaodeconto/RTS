using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Collections;
using EveryplayMiniJSON;

public class Everyplay : MonoBehaviour {
	public enum FaceCamPreviewOrigin { TopLeft = 0, TopRight, BottomLeft, BottomRight };
	public enum UserInterfaceIdiom { Phone = 0, Tablet, iPhone = Phone, iPad = Tablet };

	public delegate void WasClosedDelegate();
	public event WasClosedDelegate WasClosed {
	    add {
	        RealWasClosed += value;
	        wasClosedDelegates.Add(value);
	    }

	    remove {
	        RealWasClosed -= value;
	        wasClosedDelegates.Remove(value);
	    }
	}
	private List<WasClosedDelegate> wasClosedDelegates = new List<WasClosedDelegate>();
	private event WasClosedDelegate RealWasClosed;

	public delegate void ReadyForRecordingDelegate(bool granted);
	public event ReadyForRecordingDelegate ReadyForRecording {
	    add {
	        RealReadyForRecording += value;
			ReadyForRecordingDelegates.Add(value);
	    }

	    remove {
	        RealReadyForRecording -= value;
	        ReadyForRecordingDelegates.Remove(value);
	    }
	}

	private List<ReadyForRecordingDelegate> ReadyForRecordingDelegates = new List<ReadyForRecordingDelegate>();
	private event ReadyForRecordingDelegate RealReadyForRecording;

	public delegate void RecordingStartedDelegate();
	public event RecordingStartedDelegate RecordingStarted {
	    add {
	        RealRecordingStarted += value;
	        recordingStartedDelegates.Add(value);
	    }

	    remove {
	        RealRecordingStarted -= value;
	        recordingStartedDelegates.Remove(value);
	    }
	}

	private List<RecordingStartedDelegate> recordingStartedDelegates = new List<RecordingStartedDelegate>();
	private event RecordingStartedDelegate RealRecordingStarted;

	public delegate void RecordingStoppedDelegate();
	public event RecordingStoppedDelegate RecordingStopped {
	    add {
	        RealRecordingStopped += value;
	        recordingStoppedDelegates.Add(value);
	    }

	    remove {
	        RealRecordingStopped -= value;
	        recordingStoppedDelegates.Remove(value);
	    }
	}

	private List<RecordingStoppedDelegate> recordingStoppedDelegates = new List<RecordingStoppedDelegate>();
	private event RecordingStoppedDelegate RealRecordingStopped;

	public delegate void FaceCamSessionStartedDelegate();
	public event FaceCamSessionStartedDelegate FaceCamSessionStarted {
	    add {
	        RealFaceCamSessionStarted += value;
	       	faceCamSessionStartedDelegates.Add(value);
	    }

	    remove {
	        RealFaceCamSessionStarted -= value;
	        faceCamSessionStartedDelegates.Remove(value);
	    }
	}

	private List<FaceCamSessionStartedDelegate> faceCamSessionStartedDelegates = new List<FaceCamSessionStartedDelegate>();
	private event FaceCamSessionStartedDelegate RealFaceCamSessionStarted;

	public delegate void FaceCamRecordingPermissionDelegate(bool granted);
	public event FaceCamRecordingPermissionDelegate FaceCamRecordingPermission {
	    add {
	        RealFaceCamRecordingPermission += value;
			faceCamRecordingPermissionDelegates.Add(value);
	    }

	    remove {
	        RealFaceCamRecordingPermission -= value;
	        faceCamRecordingPermissionDelegates.Remove(value);
	    }
	}

	private List<FaceCamRecordingPermissionDelegate> faceCamRecordingPermissionDelegates = new List<FaceCamRecordingPermissionDelegate>();
	private event FaceCamRecordingPermissionDelegate RealFaceCamRecordingPermission;

	public delegate void FaceCamSessionStoppedDelegate();
	public event FaceCamSessionStoppedDelegate FaceCamSessionStopped {
	    add {
	        RealFaceCamSessionStopped += value;
	       	faceCamSessionStoppedDelegates.Add(value);
	    }

	    remove {
	        RealFaceCamSessionStopped -= value;
	        faceCamSessionStoppedDelegates.Remove(value);
	    }
	}

	private List<FaceCamSessionStoppedDelegate> faceCamSessionStoppedDelegates = new List<FaceCamSessionStoppedDelegate>();
	private event FaceCamSessionStoppedDelegate RealFaceCamSessionStopped;

	public delegate void ThumbnailReadyAtFilePathDelegate(string filePath);
	public event ThumbnailReadyAtFilePathDelegate ThumbnailReadyAtFilePath {
	    add {
	        RealThumbnailReadyAtFilePath += value;
	        thumbnailReadyAtFilePathDelegates.Add(value);
	    }

	    remove {
	        RealThumbnailReadyAtFilePath -= value;
	        thumbnailReadyAtFilePathDelegates.Remove(value);
	    }
	}

	private List<ThumbnailReadyAtFilePathDelegate> thumbnailReadyAtFilePathDelegates = new List<ThumbnailReadyAtFilePathDelegate>();
	private event ThumbnailReadyAtFilePathDelegate RealThumbnailReadyAtFilePath;

	public delegate void ThumbnailReadyAtTextureIdDelegate(int textureId, bool portrait);
	public event ThumbnailReadyAtTextureIdDelegate ThumbnailReadyAtTextureId {
	    add {
	        RealThumbnailReadyAtTextureId += value;
	        thumbnailReadyAtTextureIdDelegates.Add(value);
	    }

	    remove {
	        RealThumbnailReadyAtTextureId -= value;
	        thumbnailReadyAtTextureIdDelegates.Remove(value);
	    }
	}

	private List<ThumbnailReadyAtTextureIdDelegate> thumbnailReadyAtTextureIdDelegates = new List<ThumbnailReadyAtTextureIdDelegate>();
	private event ThumbnailReadyAtTextureIdDelegate RealThumbnailReadyAtTextureId;

	public delegate void UploadDidStartDelegate(int videoId);
	public event UploadDidStartDelegate UploadDidStart {
	    add {
	        RealUploadDidStart += value;
	        uploadDidStartDelegates.Add(value);
	    }

	    remove {
	        RealUploadDidStart -= value;
	        uploadDidStartDelegates.Remove(value);
	    }
	}

	private List<UploadDidStartDelegate> uploadDidStartDelegates = new List<UploadDidStartDelegate>();
	private event UploadDidStartDelegate RealUploadDidStart;

	public delegate void UploadDidProgressDelegate(int videoId, float progress);
	public event UploadDidProgressDelegate UploadDidProgress {
	    add {
	        RealUploadDidProgress += value;
	        uploadDidProgressDelegates.Add(value);
	    }

	    remove {
	        RealUploadDidProgress -= value;
	        uploadDidProgressDelegates.Remove(value);
	    }
	}

	private List<UploadDidProgressDelegate> uploadDidProgressDelegates = new List<UploadDidProgressDelegate>();
	private event UploadDidProgressDelegate RealUploadDidProgress;

	public delegate void UploadDidCompleteDelegate(int videoId);
	public event UploadDidCompleteDelegate UploadDidComplete {
	    add {
	        RealUploadDidComplete += value;
	        uploadDidCompleteDelegates.Add(value);
	    }

	    remove {
	        RealUploadDidComplete -= value;
	        uploadDidCompleteDelegates.Remove(value);
	    }
	}

	private List<UploadDidCompleteDelegate> uploadDidCompleteDelegates = new List<UploadDidCompleteDelegate>();
	private event UploadDidCompleteDelegate RealUploadDidComplete;

	public delegate void ThumbnailLoadReadyDelegate(Texture2D texture);
	public delegate void ThumbnailLoadFailedDelegate(string error);

	public delegate void RequestReadyDelegate(string response);
	public delegate void RequestFailedDelegate(string error);

	public string clientId;
	public string clientSecret;
	public string redirectURI;

	private static Everyplay sharedInstance = null;
	private static bool appIsClosing = false;

	void Awake() {
		// Do not remove
		bool supported = Everyplay.SharedInstance.IsSupported();
		if (supported == false) {
			Debug.Log("Everyplay isSupported: " + supported);
		}
	}

	public static Everyplay SharedInstance {
		get {
			if(!sharedInstance) {
				sharedInstance = (Everyplay) FindObjectOfType(typeof(Everyplay));

				if(!sharedInstance) {
					bool shouldCreateInstance = !appIsClosing;

					if(Application.isEditor && !Application.isPlaying) {
						shouldCreateInstance = false;
					}

					if(shouldCreateInstance) {
						GameObject tmp = new GameObject("Everyplay");
						sharedInstance = tmp.AddComponent<Everyplay>();

						Debug.Log("Everyplay was not found on this scene. This is not a problem if you have added it to the first scene and you are currently editing some other scene. Just make sure you add it to the scene where you use it for the first time.");
					}
				}

				#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
				if(sharedInstance) {
					InitEveryplay(sharedInstance.clientId, sharedInstance.clientSecret, sharedInstance.redirectURI);
				}
				#endif
			}

			return sharedInstance;
		}
	}

	void Start() {
		Everyplay[] allInstances = FindObjectsOfType(typeof(Everyplay)) as Everyplay[];

		foreach(Everyplay ins in allInstances) {
			if(ins == Everyplay.SharedInstance) {
				DontDestroyOnLoad(gameObject);
			}
			else if(Everyplay.SharedInstance) {
				Destroy(ins.gameObject);
			}
		}
	}

	void OnApplicationQuit() {
		RemoveAllEvents();
		appIsClosing = true;
		sharedInstance = null;
	}

	public void Show() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayShow();
		#endif
	}

	public void ShowWithPath(string path) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayShowWithPath(path);
		#endif
	}

	public void PlayVideoWithURL(string url) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayPlayVideoWithURL(url);
		#endif
	}

	public void PlayVideoWithDictionary(Dictionary<string,object> dict) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayPlayVideoWithDictionary(Json.Serialize(dict));
		#endif
	}

	public void MakeRequest(string method, string url, Dictionary<string, object> data, RequestReadyDelegate readyDelegate, RequestFailedDelegate failedDelegate) {
		StartCoroutine(MakeRequestEnumerator(method, url, data, readyDelegate, failedDelegate));
	}

	public string AccessToken() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplayAccountAccessToken();
		#else
		return null;
		#endif
	}

	public void ShowSharingModal() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayShowSharingModal();
		#endif
	}

	public void StartRecording() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayStartRecording();
		#endif
	}

	public void StopRecording() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayStopRecording();
		#endif
	}

	public void PauseRecording() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayPauseRecording();
		#endif
	}

	public void ResumeRecording() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayResumeRecording();
		#endif
	}

	public bool IsRecording() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplayIsRecording();
		#else
		return false;
		#endif
	}

	public bool IsRecordingSupported() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplayIsRecordingSupported();
		#else
		return false;
		#endif
	}

	public bool IsPaused() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplayIsPaused();
		#else
		return false;
		#endif
	}

	public bool SnapshotRenderbuffer() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplaySnapshotRenderbuffer();
		#else
		return false;
		#endif
	}

	public bool IsSupported() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplayIsSupported();
		#else
		return false;
		#endif
	}

	public bool IsSingleCoreDevice() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplayIsSingleCoreDevice();
		#else
		return false;
		#endif
	}

	public int GetUserInterfaceIdiom() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplayGetUserInterfaceIdiom();
		#else
		return 0;
		#endif
	}

	public void PlayLastRecording() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayPlayLastRecording();
		#endif
	}

	public void SetMetadata(string key, object val) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		if(key != null && val != null) {
			Dictionary<string,object> dict = new Dictionary<string, object>();
			dict.Add(key, val);
			EveryplaySetMetadata(Json.Serialize(dict));
		}
		#endif
	}

	public void SetMetadata(Dictionary<string,object> dict) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		if(dict != null) {
			if(dict.Count > 0) {
				EveryplaySetMetadata(Json.Serialize(dict));
			}
		}
		#endif
	}

	public void SetTargetFPS(int fps) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplaySetTargetFPS(fps);
		#endif
	}

	public void SetMotionFactor(int factor) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplaySetMotionFactor(factor);
		#endif
	}

	public void SetMaxRecordingMinutesLength(int minutes) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplaySetMaxRecordingMinutesLength(minutes);
		#endif
	}

	public void SetLowMemoryDevice(bool state) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplaySetLowMemoryDevice(state);
		#endif
	}

	public void SetDisableSingleCoreDevices(bool state) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplaySetDisableSingleCoreDevices(state);
		#endif
	}

	public void LoadThumbnailFromFilePath(string filePath, ThumbnailLoadReadyDelegate readyDelegate, ThumbnailLoadFailedDelegate failedDelegate) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		if(filePath != null) {
				StartCoroutine(LoadThumbnailEnumerator(filePath, readyDelegate, failedDelegate));
		}
		else {
			failedDelegate("Everyplay error: Thumbnail is not ready.");
		}
		#endif
	}

	public bool FaceCamIsVideoRecordingSupported() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplayFaceCamIsVideoRecordingSupported();
		#else
		return false;
		#endif
	}

	public bool FaceCamIsAudioRecordingSupported() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplayFaceCamIsAudioRecordingSupported();
		#else
		return false;
		#endif
	}

	public bool FaceCamIsHeadphonesPluggedIn() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplayFaceCamIsHeadphonesPluggedIn();
		#else
		return false;
		#endif
	}

	public bool FaceCamIsSessionRunning() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplayFaceCamIsSessionRunning();
		#else
		return false;
		#endif
	}

	public bool FaceCamIsRecordingPermissionGranted() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplayFaceCamIsRecordingPermissionGranted();
		#else
		return false;
		#endif
	}

	public float FaceCamAudioPeakLevel() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplayFaceCamAudioPeakLevel();
		#else
		return 0.0f;
		#endif
	}

	public float FaceCamAudioPowerLevel() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		return EveryplayFaceCamAudioPowerLevel();
		#else
		return 0.0f;
		#endif
	}

	public void FaceCamSetMonitorAudioLevels(bool enabled) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamSetMonitorAudioLevels(enabled);
		#endif
	}

	public void FaceCamSetAudioOnly(bool audioOnly) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamSetAudioOnly(audioOnly);
		#endif
	}

	public void FaceCamSetPreviewVisible(bool visible) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamSetPreviewVisible(visible);
		#endif
	}

	public void FaceCamSetPreviewScaleRetina(bool autoScale) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamSetPreviewScaleRetina(autoScale);
		#endif
	}

	public void FaceCamSetPreviewSideWidth(int width) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamSetPreviewSideWidth(width);
		#endif
	}

	public void FaceCamSetPreviewBorderWidth(int width) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamSetPreviewBorderWidth(width);
		#endif
	}

	public void FaceCamSetPreviewPositionX(int x) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamSetPreviewPositionX(x);
		#endif
	}

	public void FaceCamSetPreviewPositionY(int y) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamSetPreviewPositionY(y);
		#endif
	}

	public void FaceCamSetPreviewBorderColor(float r, float g, float b, float a) {
		#if UNITY_IPHONE && !UNITY_EDITOR
		EveryplayFaceCamSetPreviewBorderColor(r, g, b, a);
		#endif
	}

	public void FaceCamSetPreviewOrigin(FaceCamPreviewOrigin origin) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamSetPreviewOrigin((int) origin);
		#endif
	}

	public void SetThumbnailWidth(int thumbnailWidth) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplaySetThumbnailWidth(thumbnailWidth);
		#endif
	}

	public void FaceCamSetTargetTextureId(int textureId) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamSetTargetTextureId(textureId);
		#endif
	}

	public void FaceCamSetTargetTextureWidth(int textureWidth) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamSetTargetTextureWidth(textureWidth);
		#endif
	}

	public void FaceCamSetTargetTextureHeight(int textureHeight) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamSetTargetTextureHeight(textureHeight);
		#endif
	}

	public void FaceCamStartSession() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamStartSession();
		#endif
	}

	public void FaceCamRequestRecordingPermission() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamRequestRecordingPermission();
		#endif
	}

	public void FaceCamStopSession() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayFaceCamStopSession();
		#endif
	}

	public void SetThumbnailTargetTextureId(int textureId) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplaySetThumbnailTargetTextureId(textureId);
		#endif
	}

	public void SetThumbnailTargetTextureWidth(int textureWidth) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplaySetThumbnailTargetTextureWidth(textureWidth);
		#endif
	}

	public void SetThumbnailTargetTextureHeight(int textureHeight) {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplaySetThumbnailTargetTextureHeight(textureHeight);
		#endif
	}

	public void TakeThumbnail() {
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		EveryplayTakeThumbnail();
		#endif
	}

	private void RemoveAllEvents() {
	    foreach(ReadyForRecordingDelegate del in ReadyForRecordingDelegates) {
	        RealReadyForRecording -= del;
	    }
	    ReadyForRecordingDelegates.Clear();

	    foreach(RecordingStartedDelegate del in recordingStartedDelegates) {
	        RealRecordingStarted -= del;
	    }
	    recordingStartedDelegates.Clear();

	    foreach(RecordingStoppedDelegate del in recordingStoppedDelegates) {
	        RealRecordingStopped -= del;
	    }
	    recordingStoppedDelegates.Clear();

	    foreach(FaceCamSessionStartedDelegate del in faceCamSessionStartedDelegates) {
	        RealFaceCamSessionStarted -= del;
	    }
	    faceCamSessionStartedDelegates.Clear();

	    foreach(FaceCamRecordingPermissionDelegate del in faceCamRecordingPermissionDelegates) {
	        RealFaceCamRecordingPermission -= del;
	    }
	    faceCamRecordingPermissionDelegates.Clear();

	    foreach(FaceCamSessionStoppedDelegate del in faceCamSessionStoppedDelegates) {
	        RealFaceCamSessionStopped -= del;
	    }
	    faceCamSessionStoppedDelegates.Clear();

	    foreach(ThumbnailReadyAtFilePathDelegate del in thumbnailReadyAtFilePathDelegates) {
	        RealThumbnailReadyAtFilePath -= del;
	    }
	    thumbnailReadyAtFilePathDelegates.Clear();

	    foreach(ThumbnailReadyAtTextureIdDelegate del in thumbnailReadyAtTextureIdDelegates) {
	        RealThumbnailReadyAtTextureId -= del;
	    }
	    thumbnailReadyAtTextureIdDelegates.Clear();

	    foreach(UploadDidStartDelegate del in uploadDidStartDelegates) {
	        RealUploadDidStart -= del;
	    }
	    uploadDidStartDelegates.Clear();

	    foreach(UploadDidProgressDelegate del in uploadDidProgressDelegates) {
	        RealUploadDidProgress -= del;
	    }
	    uploadDidProgressDelegates.Clear();

	    foreach(UploadDidCompleteDelegate del in uploadDidCompleteDelegates) {
	        RealUploadDidComplete -= del;
	    }
	    uploadDidCompleteDelegates.Clear();
	}

	private IEnumerator LoadThumbnailEnumerator(string fileName, ThumbnailLoadReadyDelegate readyDelegate, ThumbnailLoadFailedDelegate failedDelegate) {
		WWW www = new WWW("file://" + fileName);

		yield return www;

		if(!string.IsNullOrEmpty(www.error)) {
			failedDelegate("Everyplay error: " + www.error);
		}
		else {
			if(www.texture) {
				 readyDelegate(www.texture);
			}
			else {
				failedDelegate("Everyplay error: Loading thumbnail failed.");
			}
		}
	}

	private IEnumerator MakeRequestEnumerator(string method, string url, Dictionary<string, object> data, RequestReadyDelegate readyDelegate, RequestFailedDelegate failedDelegate) {

		if (data == null) {
		  data = new Dictionary<string, object>();
		}

		if (url.IndexOf("http") != 0) {
			if (url.IndexOf("/") != 0) {
				url = "/" + url;
			}

			url = "https://api.everyplay.com" + url;
		}

		method = method.ToLower();

		Hashtable headers = new Hashtable();

		string accessToken = AccessToken();
		if (accessToken != null) {
			headers["Authorization"] = "Bearer " + accessToken;
		} else {
			if(url.IndexOf("client_id") == -1) {
				if (url.IndexOf("?") == -1) {
					url += "?";
				} else {
					url += "&";
				}
				url += "client_id=" + clientId;
			}
		}

		data.Add("_method", method);


		string dataString = Json.Serialize(data);
		byte[] dataArray = System.Text.Encoding.UTF8.GetBytes(dataString);

		headers["Accept"] = "application/json";
		headers["Content-Type"] = "application/json";
		headers["Data-Type"] = "json";
		headers["Content-Length"] = dataArray.Length;

		WWW www = new WWW(url, dataArray, headers);

		yield return www;

		if(!string.IsNullOrEmpty(www.error) && failedDelegate != null) {
			failedDelegate("Everyplay error: " + www.error);
		}
		else if(string.IsNullOrEmpty(www.error) && readyDelegate != null) {
			readyDelegate(www.text);
		}
	}

	private void EveryplayHidden(string msg) {
		if(RealWasClosed != null) {
			RealWasClosed();
		}
	}

	private void EveryplayReadyForRecording(string jsonMsg) {
		if(jsonMsg != null && jsonMsg.Length > 0) {
			Dictionary<string,object> dict = Json.Deserialize(jsonMsg) as Dictionary<string,object>;
			if(dict != null && dict.ContainsKey("enabled")) {
				int enabled = Convert.ToInt32(dict["enabled"]);
				if(RealReadyForRecording != null) {
					RealReadyForRecording(enabled == 1);
				}
			}
		}
	}

	private void EveryplayRecordingStarted(string msg) {
		if(RealRecordingStarted != null) {
			RealRecordingStarted();
		}
	}

	private void EveryplayRecordingStopped(string msg) {
		if(RealRecordingStopped != null) {
			RealRecordingStopped();
		}
	}

	private void EveryplayFaceCamSessionStarted(string msg) {
		if(RealFaceCamSessionStarted != null) {
			RealFaceCamSessionStarted();
		}
	}

	private void EveryplayFaceCamRecordingPermission(string jsonMsg) {
		if(jsonMsg != null && jsonMsg.Length > 0) {
			Dictionary<string,object> dict = Json.Deserialize(jsonMsg) as Dictionary<string,object>;
			if(dict != null && dict.ContainsKey("granted")) {
				int granted = Convert.ToInt32(dict["granted"]);
				if(RealFaceCamRecordingPermission != null) {
					RealFaceCamRecordingPermission(granted == 1);
				}
			}
		}
	}

	private void EveryplayFaceCamSessionStopped(string msg) {
		if(RealFaceCamSessionStopped != null) {
			RealFaceCamSessionStopped();
		}
	}

	private void EveryplayThumbnailReadyAtFilePath(string jsonMsg) {
		if(jsonMsg != null && jsonMsg.Length > 0) {
			Dictionary<string,object> dict = Json.Deserialize(jsonMsg) as Dictionary<string,object>;
			if(dict != null && dict.ContainsKey("thumbnailFilePath")) {
				string realFilePath = (string) dict["thumbnailFilePath"];
				if(realFilePath != null) {
					if(RealThumbnailReadyAtFilePath != null) {
						RealThumbnailReadyAtFilePath(realFilePath);
					}
				}
			}
		}
	}

	private void EveryplayThumbnailReadyAtTextureId(string jsonMsg) {
		if(jsonMsg != null && jsonMsg.Length > 0) {
			Dictionary<string,object> dict = Json.Deserialize(jsonMsg) as Dictionary<string,object>;
			if(dict != null && dict.ContainsKey("textureId") && dict.ContainsKey("portrait")) {
				int realTextureId = Convert.ToInt32(dict["textureId"]);
				bool realPortrait = Convert.ToInt32(dict["portrait"]) > 0 ? true : false;
				if(RealThumbnailReadyAtTextureId != null) {
					RealThumbnailReadyAtTextureId(realTextureId, realPortrait);
				}
			}
		}
	}

	private void EveryplayUploadDidStart(string jsonMsg) {
		if(jsonMsg != null && jsonMsg.Length > 0) {
			Dictionary<string,object> dict = Json.Deserialize(jsonMsg) as Dictionary<string,object>;
			if(dict != null && dict.ContainsKey("videoId")) {
				int videoId = Convert.ToInt32(dict["videoId"]);
				if(RealUploadDidStart != null) {
					RealUploadDidStart(videoId);
				}
			}
		}
	}

	private void EveryplayUploadDidProgress(string jsonMsg) {
		if(jsonMsg != null && jsonMsg.Length > 0) {
			Dictionary<string,object> dict = Json.Deserialize(jsonMsg) as Dictionary<string,object>;
			if(dict != null && dict.ContainsKey("videoId") && dict.ContainsKey("progress")) {
				int videoId = Convert.ToInt32(dict["videoId"]);
				double progress = Convert.ToDouble(dict["progress"]);
				if(RealUploadDidProgress != null) {
					RealUploadDidProgress(videoId, (float) progress);
				}
			}
		}
	}

	private void EveryplayUploadDidComplete(string jsonMsg) {
		if(jsonMsg != null && jsonMsg.Length > 0) {
			Dictionary<string,object> dict = Json.Deserialize(jsonMsg) as Dictionary<string,object>;
			if(dict != null && dict.ContainsKey("videoId")) {
				int videoId = Convert.ToInt32(dict["videoId"]);
				if(RealUploadDidComplete != null) {
					RealUploadDidComplete(videoId);
				}
			}
		}
	}

	#if UNITY_IPHONE && !UNITY_EDITOR

	[DllImport("__Internal")]
	private static extern void InitEveryplay(string clientId, string clientSecret, string redirectURI);

	[DllImport("__Internal")]
	private static extern void EveryplayShow();

	[DllImport("__Internal")]
	private static extern void EveryplayShowWithPath(string path);

	[DllImport("__Internal")]
	private static extern void EveryplayPlayVideoWithURL(string url);

	[DllImport("__Internal")]
	private static extern void EveryplayPlayVideoWithDictionary(string dic);

	[DllImport("__Internal")]
	private static extern string EveryplayAccountAccessToken();

	[DllImport("__Internal")]
	private static extern void EveryplayShowSharingModal();

	[DllImport("__Internal")]
	private static extern void EveryplayStartRecording();

	[DllImport("__Internal")]
	private static extern void EveryplayStopRecording();

	[DllImport("__Internal")]
	private static extern void EveryplayPauseRecording();

	[DllImport("__Internal")]
	private static extern void EveryplayResumeRecording();

	[DllImport("__Internal")]
	private static extern bool EveryplayIsRecording();

	[DllImport("__Internal")]
	private static extern bool EveryplayIsRecordingSupported();

	[DllImport("__Internal")]
	private static extern bool EveryplayIsPaused();

	[DllImport("__Internal")]
	private static extern bool EveryplaySnapshotRenderbuffer();

	[DllImport("__Internal")]
	private static extern void EveryplayPlayLastRecording();

	[DllImport("__Internal")]
	private static extern void EveryplaySetMetadata(string json);

	[DllImport("__Internal")]
	private static extern void EveryplaySetTargetFPS(int fps);

	[DllImport("__Internal")]
	private static extern void EveryplaySetMotionFactor(int factor);

	[DllImport("__Internal")]
	private static extern void EveryplaySetMaxRecordingMinutesLength(int minutes);

	[DllImport("__Internal")]
	private static extern void EveryplaySetLowMemoryDevice(bool state);

	[DllImport("__Internal")]
	private static extern void EveryplaySetDisableSingleCoreDevices(bool state);

	[DllImport("__Internal")]
	private static extern bool EveryplayIsSupported();

	[DllImport("__Internal")]
	private static extern bool EveryplayIsSingleCoreDevice();

	[DllImport("__Internal")]
	private static extern int EveryplayGetUserInterfaceIdiom();

	[DllImport("__Internal")]
	private static extern bool EveryplayFaceCamIsVideoRecordingSupported();

	[DllImport("__Internal")]
	private static extern bool EveryplayFaceCamIsAudioRecordingSupported();

	[DllImport("__Internal")]
	private static extern bool EveryplayFaceCamIsHeadphonesPluggedIn();

	[DllImport("__Internal")]
	private static extern bool EveryplayFaceCamIsSessionRunning();

	[DllImport("__Internal")]
	private static extern bool EveryplayFaceCamIsRecordingPermissionGranted();

	[DllImport("__Internal")]
	private static extern float EveryplayFaceCamAudioPeakLevel();

	[DllImport("__Internal")]
	private static extern float EveryplayFaceCamAudioPowerLevel();

	[DllImport("__Internal")]
	private static extern void EveryplayFaceCamSetMonitorAudioLevels(bool enabled);

	[DllImport("__Internal")]
	private static extern void EveryplayFaceCamSetAudioOnly(bool audioOnly);

	[DllImport("__Internal")]
	private static extern void EveryplayFaceCamSetPreviewVisible(bool visible);

	[DllImport("__Internal")]
	private static extern void EveryplayFaceCamSetPreviewScaleRetina(bool autoScale);

	[DllImport("__Internal")]
	private static extern void EveryplayFaceCamSetPreviewSideWidth(int width);

	[DllImport("__Internal")]
	private static extern void EveryplayFaceCamSetPreviewBorderWidth(int width);

	[DllImport("__Internal")]
	private static extern void EveryplayFaceCamSetPreviewPositionX(int x);

	[DllImport("__Internal")]
	private static extern void EveryplayFaceCamSetPreviewPositionY(int y);

	[DllImport("__Internal")]
	private static extern void EveryplayFaceCamSetPreviewBorderColor(float r, float g, float b, float a);

	[DllImport("__Internal")]
	private static extern void EveryplayFaceCamSetPreviewOrigin(int origin);

	[DllImport("__Internal")]
	private static extern void EveryplayFaceCamSetTargetTextureId(int textureId);

	[DllImport("__Internal")]
	private static extern void EveryplayFaceCamSetTargetTextureWidth(int textureWidth);

	[DllImport("__Internal")]
	private static extern void EveryplayFaceCamSetTargetTextureHeight(int textureHeight);

	[DllImport("__Internal")]
	private static extern bool EveryplayFaceCamStartSession();

	[DllImport("__Internal")]
	private static extern void EveryplayFaceCamRequestRecordingPermission();

	[DllImport("__Internal")]
	private static extern bool EveryplayFaceCamStopSession();

	[DllImport("__Internal")]
	private static extern void EveryplaySetThumbnailWidth(int thumbnailWidth);

	[DllImport("__Internal")]
	private static extern void EveryplaySetThumbnailTargetTextureId(int textureId);

	[DllImport("__Internal")]
	private static extern void EveryplaySetThumbnailTargetTextureWidth(int textureWidth);

	[DllImport("__Internal")]
	private static extern void EveryplaySetThumbnailTargetTextureHeight(int textureHeight);

	[DllImport("__Internal")]
	private static extern void EveryplayTakeThumbnail();

	#elif UNITY_ANDROID && !UNITY_EDITOR

	private static AndroidJavaObject everyplayUnity;

	public static void InitEveryplay(string clientId, string clientSecret, string redirectURI) {
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
		everyplayUnity = new AndroidJavaObject("com.everyplay.Everyplay.unity.EveryplayUnity3DWrapper");
		everyplayUnity.Call("initEveryplay", activity, clientId, clientSecret, redirectURI);
	}

	public static void EveryplayShow() {
		everyplayUnity.Call<bool>("showEveryplay");
	}

	public static void EveryplayShowWithPath(string path) {
		everyplayUnity.Call<bool>("showEveryplay", path);
	}

	public static void EveryplayPlayVideoWithURL(string url) {
		everyplayUnity.Call("playVideoWithURL", url);
	}

	public static void EveryplayPlayVideoWithDictionary(string dic) {
		everyplayUnity.Call("playVideoWithDictionary", dic);
	}

	public static string EveryplayAccountAccessToken() {
		return everyplayUnity.Call<string>("getAccessToken");
	}

	public static void EveryplayShowSharingModal() {
		everyplayUnity.Call("showSharingModal");
	}

	public static void EveryplayStartRecording() {
		everyplayUnity.Call("startRecording");
	}

	public static void EveryplayStopRecording() {
		everyplayUnity.Call("stopRecording");
	}

	public static void EveryplayPauseRecording() {
		everyplayUnity.Call("pauseRecording");
	}

	public static void EveryplayResumeRecording() {
		everyplayUnity.Call("resumeRecording");
	}

	public static bool EveryplayIsRecording() {
		return everyplayUnity.Call<bool>("isRecording");
	}

	public static bool EveryplayIsRecordingSupported() {
		return everyplayUnity.Call<bool>("isRecordingSupported");
	}

	public static bool EveryplayIsPaused() {
		return everyplayUnity.Call<bool>("isPaused");
	}

	public static bool EveryplaySnapshotRenderbuffer() {
		return everyplayUnity.Call<bool>("snapshotRenderbuffer");
	}

	public static void EveryplayPlayLastRecording() {
		everyplayUnity.Call("playLastRecording");
	}

	public static void EveryplaySetMetadata(string json) {
		everyplayUnity.Call("setMetadata", json);
	}

	public static void EveryplaySetTargetFPS(int fps) {
		everyplayUnity.Call("setTargetFPS", fps);
	}

	public static void EveryplaySetMotionFactor(int factor) {
		everyplayUnity.Call("setMotionFactor", factor);
	}

	public static void EveryplaySetMaxRecordingMinutesLength(int minutes) {
		everyplayUnity.Call("setMaxRecordingMinutesLength", minutes);
	}

	public static void EveryplaySetLowMemoryDevice(bool state) {
		everyplayUnity.Call("setLowMemoryDevice", state ? 1 : 0);
	}

	public static void EveryplaySetDisableSingleCoreDevices(bool state) {
		everyplayUnity.Call("setDisableSingleCoreDevices", state ? 1 : 0);
	}

	public static bool EveryplayIsSupported() {
		return everyplayUnity.Call<bool>("isSupported");
	}

	public static bool EveryplayIsSingleCoreDevice() {
		return everyplayUnity.Call<bool>("isSingleCoreDevice");
	}

	public static int EveryplayGetUserInterfaceIdiom() {
		return everyplayUnity.Call<int>("getUserInterfaceIdiom");
	}

	public static bool EveryplayFaceCamIsVideoRecordingSupported() {
		return false;
	}

	public static bool EveryplayFaceCamIsAudioRecordingSupported() {
		return false;
	}

	public static bool EveryplayFaceCamIsHeadphonesPluggedIn() {
		return false;
	}

	public static bool EveryplayFaceCamIsSessionRunning() {
		return false;
	}

	public static bool EveryplayFaceCamIsRecordingPermissionGranted() {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
		return false;
	}

	public static float EveryplayFaceCamAudioPeakLevel() {
		return 0.0f;
	}

	public static float EveryplayFaceCamAudioPowerLevel() {
		return 0.0f;
	}

	public static void EveryplayFaceCamSetMonitorAudioLevels(bool enabled) {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
	}

	public static void EveryplayFaceCamSetAudioOnly(bool audioOnly) {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
	}

	public static void EveryplayFaceCamSetPreviewVisible(bool visible) {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
	}

	public static void EveryplayFaceCamSetPreviewScaleRetina(bool autoScale) {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
	}

	public static void EveryplayFaceCamSetPreviewSideWidth(int width) {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
	}

	public static void EveryplayFaceCamSetPreviewBorderWidth(int width) {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
	}

	public static void EveryplayFaceCamSetPreviewPositionX(int x) {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
	}

	public static void EveryplayFaceCamSetPreviewPositionY(int y) {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
	}

	public static void EveryplayFaceCamSetPreviewBorderColor(float r, float g, float b, float a) {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
	}

	public static void EveryplayFaceCamSetPreviewOrigin(int origin) {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
	}

	public static void EveryplayFaceCamSetTargetTextureId(int textureId) {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
	}

	public static void EveryplayFaceCamSetTargetTextureWidth(int textureHeight) {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
	}

	public static void EveryplayFaceCamSetTargetTextureHeight(int textureWidth) {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
	}

	public static bool EveryplayFaceCamStartSession() {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
		return false;
	}

	public static void EveryplayFaceCamRequestRecordingPermission() {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
	}

	public static bool EveryplayFaceCamStopSession() {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + " not yet implemented");
		return false;
	}

	public static void EveryplaySetThumbnailWidth(int thumbnailWidth) {
		everyplayUnity.Call("setThumbnailWidth", thumbnailWidth);
	}

	public static void EveryplaySetThumbnailTargetTextureId(int textureId) {
		everyplayUnity.Call("setThumbnailTargetTextureId", textureId);
	}

	public static void EveryplaySetThumbnailTargetTextureWidth(int textureWidth) {
		everyplayUnity.Call("setThumbnailTargetTextureWidth", textureWidth);
	}

	public static void EveryplaySetThumbnailTargetTextureHeight(int textureHeight) {
		everyplayUnity.Call("setThumbnailTargetTextureHeight", textureHeight);
	}

	public static void EveryplayTakeThumbnail() {
		everyplayUnity.Call("takeThumbnail");
	}

	#endif
}
