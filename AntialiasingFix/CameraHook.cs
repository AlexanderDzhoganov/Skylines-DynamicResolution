using System;
using System.CodeDom;
using System.Runtime.InteropServices;
using System.Xml.Schema;
using DynamicResolution;
using UnityEngine;

public class CameraHook : MonoBehaviour
{

    private RenderTexture rt;

    public float ssaaFactor = 1.0f;
    public float userSSAAFactor = 1.0f;
    public float currentSSAAFactor = 1.0f;

    public bool resetFactor = false;

    private bool initialized = false;

    public Rect cameraPixelRect;

    private GameObject dummyGameObject;
    private DummyHook dummyHook;

    private Camera undergroundCamera;

    public bool showConfigWindow = false;
    private Rect windowRect = new Rect(64, 64, 350, 160);

    private static readonly string configPath = "DynamicResolutionConfig.xml";

    private Configuration config;
    
    public float GetSSAAFactor()
    {
        if (undergroundCamera.cullingMask != 0)
        {
            return 1.0f;
        }

        return currentSSAAFactor;
    }

    public void Awake()
    {
        config = Configuration.Deserialize(configPath);
        if (config == null)
        {
            config = new Configuration();
        }

        ssaaFactor = config.ssaaFactor;
        userSSAAFactor = ssaaFactor;
        currentSSAAFactor = ssaaFactor;
        SaveConfig();
    }

    public void SaveConfig()
    {
        config.ssaaFactor = userSSAAFactor;
        Configuration.Serialize(configPath, config);
    }

    public void SetInGameAA(bool state)
    {
        var camera = gameObject.GetComponent<Camera>();
        if (!state)
        {
            if (camera.GetComponent<SMAA>() != null)
            {
                Destroy(camera.gameObject.GetComponent<SMAA>());
            }
        }
        else
        {
            if (camera.GetComponent<SMAA>() == null)
            {
                camera.gameObject.AddComponent<SMAA>();
            }
        }
    }

    public void SetSSAAFactor(float factor)
    {
        var width = Screen.width * factor;
        var height = Screen.height * factor;
        rt = new RenderTexture((int)width, (int)height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

        var hook = dummyGameObject.GetComponent<DummyHook>();
        hook.rt = rt;
        hook.rt2 = new RenderTexture(Screen.width, (int)height, 0);

        hook.mainCamera.targetTexture = rt;

        initialized = true;

        currentSSAAFactor = factor;
    }

    public void OnPreCull()
    {
    }

    public void OnPostRender()
    {
    }

    public void Initialize()
    {
        SetInGameAA(false);

        var camera = gameObject.GetComponent<Camera>();
        cameraPixelRect = camera.pixelRect;
        camera.depth = -100;
        camera.enabled = false;

        var width = Screen.width * ssaaFactor;
        var height = Screen.height * ssaaFactor;
        rt = new RenderTexture((int)width, (int)height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

        dummyGameObject = new GameObject();
        var dummy = dummyGameObject.AddComponent<Camera>();
        dummy.cullingMask = 0;
        dummy.depth = -3;
        dummy.tag = "MainCamera";
        dummy.pixelRect = cameraPixelRect;

        dummyHook = dummyGameObject.AddComponent<DummyHook>();
        dummyHook.rt = rt;
        dummyHook.rt2 = new RenderTexture(Screen.width, (int)height, 0);
        dummyHook.fxaaRt = null;

        dummyHook.mainCamera = camera;
        //hook.mainCamera.tag = "Player";
        dummyHook.hook = this;

        dummyHook.mainCamera.targetTexture = null;
        dummyHook.mainCamera.pixelRect = cameraPixelRect;

        var underground = FindObjectOfType<UndergroundView>();
        undergroundCamera = underground.gameObject.GetComponent<Camera>();
        undergroundCamera.backgroundColor = new Color(0, 0, 0, 1);
        undergroundCamera.depth = -110;

        initialized = true;

        SaveConfig();
    }

    public void Update()
    {
        if (!initialized)
        {
            Initialize();
        }

        if (undergroundCamera.cullingMask != 0)
        {
            if (ssaaFactor != 1.0f)
            {
                SetSSAAFactor(1.0f);
                ssaaFactor = 1.0f;
                resetFactor = true;
            }
        }
        else if(undergroundCamera.cullingMask == 0)
        {
            if (ssaaFactor != userSSAAFactor && resetFactor)
            {
                SetSSAAFactor(userSSAAFactor);
                ssaaFactor = userSSAAFactor;
                resetFactor = false;
            }
        }

        if (Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.F10))
        {
            if (ssaaFactor == 1.0f)
            {
                SetSSAAFactor(userSSAAFactor);
                ssaaFactor = userSSAAFactor;
            }
            else
            {
                SetSSAAFactor(1.0f);
                ssaaFactor = 1.0f;
            }
        }
        else if (Input.GetKeyDown(KeyCode.F10))
        {
                showConfigWindow = !showConfigWindow;
        }
    }

    void OnGUI()
    {
        if (showConfigWindow)
        {
            windowRect = GUI.Window(12412, windowRect, DoConfigWindow, "Dynamic resolution");
        }
    }

    void DoConfigWindow(int wnd)
    {
        var width = cameraPixelRect.width * ssaaFactor;
        var height = cameraPixelRect.height * ssaaFactor;

        GUILayout.Label(String.Format("Internal resolution: {0}x{1}", (int)width, (int)height));
        GUILayout.BeginHorizontal();
      
        ssaaFactor = GUILayout.HorizontalSlider(ssaaFactor, 0.25f, 3.0f, GUILayout.Width(256));
        if (ssaaFactor <= 0.25f)
        {
            ssaaFactor = 0.25f;
        }
        else if (ssaaFactor <= 0.50f)
        {
            ssaaFactor = 0.50f;
        }
        else if (ssaaFactor <= 0.75f)
        {
            ssaaFactor = 0.75f;
        }
        else if (ssaaFactor <= 1.0f)
        {
            ssaaFactor = 1.0f;
        }
        else if (ssaaFactor <= 1.5f)
        {
            ssaaFactor = 1.5f;
        }
        else if (ssaaFactor <= 1.75f)
        {
            ssaaFactor = 1.75f;
        }
        else if (ssaaFactor <= 2.0f)
        {
            ssaaFactor = 2.0f;
        }
        else if (ssaaFactor <= 2.5f)
        {
            ssaaFactor = 2.5f;
        }
        else if (ssaaFactor <= 3.0f)
        {
            ssaaFactor = 3.0f;
        }

        GUILayout.Label(String.Format("{0} %", (int)(ssaaFactor * 100.0f)));
        GUILayout.EndHorizontal();
        GUILayout.Label("FPS: " + 1.0f / Time.deltaTime);
        GUILayout.Label("dT: " + Time.deltaTime.ToString("0.000"));

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Reset"))
        {
            SetSSAAFactor(1.0f);
            ssaaFactor = 1.0f;
            userSSAAFactor = ssaaFactor;
            SaveConfig();
        }

        if (GUILayout.Button("Apply"))
        {
            SetSSAAFactor(ssaaFactor);
            userSSAAFactor = ssaaFactor;
            SaveConfig();
        }

        GUILayout.EndHorizontal();
    }


}



