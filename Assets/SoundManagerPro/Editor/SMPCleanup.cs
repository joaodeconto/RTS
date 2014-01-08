using UnityEngine;
using UnityEditor;
using System.Collections;

#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1
public class SMPCleanup : AssetModificationProcessor {
#else
public class SMPCleanup : UnityEditor.AssetModificationProcessor {
#endif
	static string[] OnWillSaveAssets (string[] paths) {
        SoundManager SMP = GameObject.FindObjectOfType(typeof(SoundManager)) as SoundManager;
		if(SMP != null && SMP.storage != null)
			SMP.storage.ClearStorage();
        return paths;
    }
}
