using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(FlowController))]

public class FlowController_Editor : Editor
{
    FlowController _target;

    static bool bAbout;
    Texture2D icon;
    Texture2D facebookIcon;
    Texture2D youtubeIcon;

    int[] materialArray;
    string[] str;

    void OnEnable()
    {
        _target = (FlowController)target;

        if (EditorGUIUtility.isProSkin)
            icon = Resources.Load("Flow_Icon_Editor_Pro") as Texture2D;
        else
            icon = Resources.Load("Flow_Icon_Editor_Free") as Texture2D;

        facebookIcon = Resources.Load("facebook_icon") as Texture2D;
        youtubeIcon = Resources.Load("youtube_icon") as Texture2D;

        RebuildMaterialIndexArray();
    }

    public override void OnInspectorGUI()
    {
        RebuildMaterialIndexArray();        

        _target.materialIndex = EditorGUILayout.IntPopup("Material index:", _target.materialIndex, str, materialArray);
        _target.speed = EditorGUILayout.Slider("Speed: ", _target.speed, -1.0f, 1.0f);
        _target.phaseLength = EditorGUILayout.Slider("Stretching: ", _target.phaseLength, 0.3f, 5.0f);
        _target.bAnimateFlowRevealing = false;
        _target.flowType = FlowController.FLOW_TYPE.Continuous;

        About();
    }

    void OnDisable()
    {
        _target = null;
    }

    void RebuildMaterialIndexArray()
    {
        materialArray = new int[_target.GetComponent<MeshRenderer>().sharedMaterials.Length];
        str = new string[materialArray.Length];

        for (int i = 0; i < materialArray.Length; i++)
        {
            materialArray[i] = i;
            str[i] = i.ToString() + ". " + _target.GetComponent<MeshRenderer>().sharedMaterials[i].ToString();

            //
            if (str[i].IndexOf("null") == -1)
                str[i] = str[i].Remove(str[i].IndexOf("("));
        }

        if (_target.materialIndex > materialArray.Length - 1)
            _target.materialIndex = materialArray.Length - 1;
    }

    void About()
    {
        GUILayout.Space(5);

        //bAbout = EditorGUILayout.Foldout(bAbout, "About");
        //if (bAbout)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(10);
            GUILayout.Box(icon, new GUIStyle());

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Version 1.3.5 Free");
            EditorGUILayout.LabelField("by Davit Naskidashvili");
            EditorGUILayout.LabelField("2013");
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();


            GUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Flow Forum", GUILayout.Width(100), GUILayout.Height(28)))
            {
                Application.OpenURL("http://forum.unity3d.com/threads/156119-Flow");
            }

            GUILayout.Space(10);
            if (GUILayout.Button("My Assets", GUILayout.Width(100), GUILayout.Height(28)))
            {
                Application.OpenURL("http://u3d.as/publisher/davit-naskidashvili/2RW");
            }

           

            GUILayout.Space(10);
            GUI.backgroundColor = new Color(0.275f, 0.424f, 0.690f);
            if (GUILayout.Button(facebookIcon, new GUIStyle(), GUILayout.Width(32), GUILayout.Height(32)))
            {
                Application.OpenURL("https://www.facebook.com/pages/Vacuum/645071998850267?ref=hl");
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            if (GUILayout.Button(youtubeIcon, new GUIStyle(), GUILayout.Width(32), GUILayout.Height(32)))
            {
                Application.OpenURL("http://www.youtube.com/user/Arxivrag/videos");
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
        }
    }

}
