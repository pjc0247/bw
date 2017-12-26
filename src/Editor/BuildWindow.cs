using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BuildWindow : EditorWindow
{
    private enum State
    {
        SetupBuild,
        UploadToApplivery
    }

    private Texture2D rocket, alpha;
    private Texture2D selected;
    private Texture2D applivery;
    private Font moonBold;

    private int idx = 0;
    private State state = State.SetupBuild;
    private bool uploadDone = false;
    private bool uploadResult = false;

    [MenuItem("Build/Build")]
	static void Start ()
    {
        var window = EditorWindow.GetWindow(typeof(BuildWindow));
        window.Show();
    }

    void Awake()
    {
        rocket = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Script/Editor/Build/Img/rocket.png");
        alpha = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Script/Editor/Build/Img/alpha.png");
        applivery = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Script/Editor/Build/Img/applivery.png");
        moonBold = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Font/Moon Bold.otf");
        selected = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/UI/FaceDetect.png");

        titleContent.text = "Build";
        titleContent.image = rocket;

        minSize = maxSize = new Vector2(600, 600);
    }

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(Vector2.zero, maxSize), EditorGUIUtility.whiteTexture);

        if (state == State.SetupBuild)
            OnDrawSetupBuild();
        if (state == State.UploadToApplivery)
            OnDrawUploadToApplivery();
    }
    void OnDrawSetupBuild()
    { 
        if (GUI.Button(new Rect(100, 100, 150, 150), alpha, new GUIStyle()))
            idx = 0;
        if (GUI.Button(new Rect(350, 100, 150, 150), rocket, new GUIStyle()))
            idx = 1;

        if (idx == 0)
            GUI.DrawTexture(new Rect(100-20, 100-20, 150+40, 150+40), selected, ScaleMode.ScaleToFit, true, 1, Color.red, 0, 0);
        else
            GUI.DrawTexture(new Rect(350-20, 100-20, 150+40, 150+40), selected, ScaleMode.ScaleToFit, true, 1, Color.red, 0, 0);

        var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        centeredStyle.font = moonBold;
        centeredStyle.fontSize = 20;
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(100, 260, 150, 50), "ALPHA", centeredStyle);
        GUI.Label(new Rect(350, 260, 150, 50), "PRODUCTION", centeredStyle);

        var appInfoStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        appInfoStyle.font = moonBold;
        appInfoStyle.fontSize = 20;
        appInfoStyle.alignment = TextAnchor.MiddleLeft;
        GUI.Label(new Rect(100, 350, 200, 50), "BUNDLE-ID:", appInfoStyle);
        GUI.Label(new Rect(230, 350, 300, 50), PlayerSettings.applicationIdentifier, appInfoStyle);
        GUI.Label(new Rect(100, 380, 200, 50), "VERSION:", appInfoStyle);
        PlayerSettings.bundleVersion = GUI.TextField(new Rect(230, 380, 300, 50), PlayerSettings.bundleVersion, appInfoStyle);
        GUI.Label(new Rect(100, 410, 200, 50), "BUILD-NO:", appInfoStyle);
        PlayerSettings.Android.bundleVersionCode = EditorGUI.IntField(new Rect(230, 410, 300, 50), PlayerSettings.Android.bundleVersionCode, appInfoStyle);

        var largeStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        largeStyle.font = moonBold;
        largeStyle.fontSize = 34;
        largeStyle.normal.textColor = new Color32(100, 240, 100, 255);
        largeStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(125, 450, 350, 150), "BUILD & DISTRIBUTE", largeStyle);
        if (GUI.Button(new Rect(125, 450, 350, 150), "", new GUIStyle()))
            UploadBuild();
    }
    void OnDrawUploadToApplivery()
    {
        GUI.DrawTexture(new Rect(200, 200, 200, 200), applivery);

        var largeStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        largeStyle.font = moonBold;
        largeStyle.fontSize = 34;
        largeStyle.normal.textColor = new Color32(100, 240, 100, 255);
        largeStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(125, 450, 350, 150), uploadDone ? "DONE" : "UPLOADING", largeStyle);
        if (uploadDone)
        {
            if (GUI.Button(new Rect(125, 450, 350, 150), "", new GUIStyle()))
            {
                state = State.UploadToApplivery;
                Repaint();
            }
        }
    }

    private void SetupKeystore()
    {
    }
    private void BuildAlpha()
    {
        
    }
    private void UploadBuild()
    {
        state = State.UploadToApplivery;

        Applivery.UploadBuild("", new Applivery.AppData()
        {
            appId = "",
            versionName = PlayerSettings.bundleVersion,
            os = "android",
            notes = "test",
            tags = ""
        },
        (success) =>
        {
            uploadDone = true;
            uploadResult = success;

            Repaint();
        });
    }
}
