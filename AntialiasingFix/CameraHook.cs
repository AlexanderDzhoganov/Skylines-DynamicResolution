using System;
using DynamicResolution;
using UnityEngine;

public class CameraHook : MonoBehaviour
{

    public static CameraHook instance = null;

    private RenderTexture rt;

    public float userSSAAFactor = 1.0f;
    public float currentSSAAFactor = 1.0f;

    private bool initialized = false;

    public Rect cameraPixelRect;

    private GameObject dummyGameObject;
    private CameraRenderer cameraRenderer;

    public bool showConfigWindow = false;
    private Rect windowRect = new Rect(64, 64, 350, 170);

    private static readonly string configPath = "DynamicResolutionConfig.xml";

    public Configuration config;
    public CameraController cameraController;

    private Texture2D bgTexture;
    private GUISkin skin;

    private float dtAccum = 0.0f;
    private int frameCount = 0;
    private float fps = 0.0f;

    void OnDestroy()
    {
        GetComponent<Camera>().enabled = true;
        Destroy(dummyGameObject);
    }

    public void Awake()
    {
        instance = this;

        config = Configuration.Deserialize(configPath);
        if (config == null)
        {
            config = new Configuration();
        }

        currentSSAAFactor = userSSAAFactor = config.ssaaFactor;
        SaveConfig();

        cameraController = FindObjectOfType<CameraController>();

        bgTexture = new Texture2D(1, 1);
        bgTexture.SetPixel(0, 0, Color.grey);
        bgTexture.Apply();
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

    public int width
    {
        get { return (int)(Screen.width * currentSSAAFactor); }
    }
    public int height
    {
        get { return (int)(Screen.height * currentSSAAFactor); }
    }

    public int internalWidth
    {
        get { return (int)(cameraPixelRect.width * currentSSAAFactor); }
    }
    public int internalHeight
    {
        get { return (int)(cameraPixelRect.height * currentSSAAFactor); }
    }

    public void SetSSAAFactor(float factor, bool lowerVRAMUsage)
    {
        var width = Screen.width * factor;
        var height = Screen.height * factor;

        Destroy(rt);
        rt = new RenderTexture((int)width, (int)height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

        var hook = dummyGameObject.GetComponent<CameraRenderer>();
        hook.fullResRT = rt;

        if (hook.halfVerticalResRT != null)
        {
            Destroy(hook.halfVerticalResRT);
        }

        if (!lowerVRAMUsage)
        {
            hook.halfVerticalResRT = new RenderTexture(Screen.width, (int)height, 0);
        }
        else
        {
            hook.halfVerticalResRT = null;
        }

        Destroy(CameraRenderer.mainCamera.targetTexture);
        CameraRenderer.mainCamera.targetTexture = rt;

        currentSSAAFactor = factor;

        initialized = true;
    }

    public void Initialize()
    {
        SetInGameAA(false);

        var camera = gameObject.GetComponent<Camera>();
        cameraPixelRect = camera.pixelRect;
        camera.enabled = false;

        var width = Screen.width * userSSAAFactor;
        var height = Screen.height * userSSAAFactor;
        rt = new RenderTexture((int)width, (int)height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

        dummyGameObject = new GameObject();
        var dummy = dummyGameObject.AddComponent<Camera>();
        dummy.cullingMask = 0;
        dummy.depth = -3;
        dummy.tag = "MainCamera";
        dummy.pixelRect = cameraPixelRect;

        cameraRenderer = dummyGameObject.AddComponent<CameraRenderer>();
        cameraRenderer.fullResRT = rt;
        cameraRenderer.halfVerticalResRT = new RenderTexture(Screen.width, (int)height, 0);

        CameraRenderer.mainCamera = camera;

        CameraRenderer.mainCamera.targetTexture = null;
        CameraRenderer.mainCamera.pixelRect = cameraPixelRect;

        currentSSAAFactor = userSSAAFactor;
        initialized = true;

        SaveConfig();
    }

    void Update()
    {
        frameCount++;
        dtAccum += Time.deltaTime;

        if (dtAccum >= 1.0f)
        {
            fps = frameCount;
            dtAccum = 0.0f;
            frameCount = 0;
        }

        if (!initialized)
        {
            Initialize();
        }
       
        if (Input.GetKeyDown(KeyCode.F10) || (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R)))
        {
            showConfigWindow = !showConfigWindow;
        }
    }

    void OnGUI()
    {
        if (skin == null)
        {
            skin = ScriptableObject.CreateInstance<GUISkin>();
            skin.box = new GUIStyle(GUI.skin.box);
            skin.button = new GUIStyle(GUI.skin.button);
            skin.horizontalScrollbar = new GUIStyle(GUI.skin.horizontalScrollbar);
            skin.horizontalScrollbarLeftButton = new GUIStyle(GUI.skin.horizontalScrollbarLeftButton);
            skin.horizontalScrollbarRightButton = new GUIStyle(GUI.skin.horizontalScrollbarRightButton);
            skin.horizontalScrollbarThumb = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
            skin.horizontalSlider = new GUIStyle(GUI.skin.horizontalSlider);
            skin.horizontalSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
            skin.label = new GUIStyle(GUI.skin.label);
            skin.scrollView = new GUIStyle(GUI.skin.scrollView);
            skin.textArea = new GUIStyle(GUI.skin.textArea);
            skin.textField = new GUIStyle(GUI.skin.textField);
            skin.toggle = new GUIStyle(GUI.skin.toggle);
            skin.verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
            skin.verticalScrollbarDownButton = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
            skin.verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
            skin.verticalScrollbarUpButton = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
            skin.verticalSlider = new GUIStyle(GUI.skin.verticalSlider);
            skin.verticalSliderThumb = new GUIStyle(GUI.skin.verticalSliderThumb);
            skin.window = new GUIStyle(GUI.skin.window);
            skin.window.normal.background = bgTexture;
            skin.window.onNormal.background = bgTexture;
        }

        if (showConfigWindow)
        {
            var oldSkin = GUI.skin;
            GUI.skin = skin;
            windowRect = GUI.Window(12412, windowRect, DoConfigWindow, "Dynamic resolution");
            GUI.skin = oldSkin;
        }
    }

    void DoConfigWindow(int wnd)
    {
        var width = cameraPixelRect.width * userSSAAFactor;
        var height = cameraPixelRect.height * userSSAAFactor;

        GUILayout.Label(String.Format("Internal resolution: {0}x{1}", (int)width, (int)height));
        GUILayout.BeginHorizontal();

        userSSAAFactor = GUILayout.HorizontalSlider(userSSAAFactor, 0.25f, 3.0f, GUILayout.Width(256));

        if (!config.unlockSlider)
        {
            if (userSSAAFactor <= 0.25f)
            {
                userSSAAFactor = 0.25f;
            }
            else if (userSSAAFactor <= 0.50f)
            {
                userSSAAFactor = 0.50f;
            }
            else if (userSSAAFactor <= 0.75f)
            {
                userSSAAFactor = 0.75f;
            }
            else if (userSSAAFactor <= 1.0f)
            {
                userSSAAFactor = 1.0f;
            }
            else if (userSSAAFactor <= 1.5f)
            {
                userSSAAFactor = 1.5f;
            }
            else if (userSSAAFactor <= 1.75f)
            {
                userSSAAFactor = 1.75f;
            }
            else if (userSSAAFactor <= 2.0f)
            {
                userSSAAFactor = 2.0f;
            }
            else if (userSSAAFactor <= 2.5f)
            {
                userSSAAFactor = 2.5f;
            }
            else if (userSSAAFactor <= 3.0f)
            {
                userSSAAFactor = 3.0f;
            }
        }

        GUILayout.Label(String.Format("{0} %", (int)(userSSAAFactor * 100.0f)));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Unlock slider (may degrade quality)");
        var unlockSlider = GUILayout.Toggle(config.unlockSlider, "");
        GUILayout.EndHorizontal();

        if (unlockSlider != config.unlockSlider)
        {
            config.unlockSlider = unlockSlider;
            SaveConfig();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Lower VRAM usage (will degrade quality)");
        var lowerRAMUsage = GUILayout.Toggle(config.lowerVRAMUsage, "");
        GUILayout.EndHorizontal();

        if (lowerRAMUsage != config.lowerVRAMUsage)
        {
            config.lowerVRAMUsage = lowerRAMUsage;
            SaveConfig();
        }

        GUILayout.Label("FPS: " + fps);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Reset"))
        {
            config.lowerVRAMUsage = false;
            SetSSAAFactor(1.0f, config.lowerVRAMUsage);
            userSSAAFactor = 1.0f;
            SaveConfig();
        }

        if (GUILayout.Button("Apply"))
        {
            SetSSAAFactor(userSSAAFactor, config.lowerVRAMUsage);
            SaveConfig();
        }

        GUILayout.EndHorizontal();
    }


}



