using Spine.Unity;
using UnityEngine;

public static class RongThanNamekSpineController
{
    private const string AssetPath = "Spine/RongThanNamek/rong_than_namek";
    private const string AnimStart = "rong_than_start";
    private const string AnimLoop = "rong_than_loop";
    private const string AnimEnd = "rong_end";

    private const float SkeletonScale = 0.385f;
    private const int OverlayLayer = 31;
    private const int OffsetX = 0;
    private const int OffsetY = 0;
    private const float GameCameraFocusDuration = 180f;
    private const float StartAnimationSpeed = 0.7f;
    private const float EndAnimationSpeed = 0.7f;

    private static GameObject go;
    private static SkeletonAnimation skeletonAnimation;
    private static SkeletonDataAsset skeletonDataAsset;
    private static SpineAtlasAsset atlasAsset;
    private static Material material;
    private static Camera overlayCamera;
    private static RenderTexture overlayTexture;
    private static int textureW;
    private static int textureH;
    private static int mapX;
    private static int mapY;
    private static bool loaded;
    private static bool failed;
    private static bool hiding;
    private static bool focusingGameCamera;
    private static long gameCameraFocusStart;

    public static bool IsAvailable()
    {
        EnsureLoaded();
        return loaded && !failed;
    }

    public static void Show(int x, int y)
    {
        mapX = x;
        mapY = y;
        hiding = false;

        if (!IsAvailable())
        {
            return;
        }

        if (go == null || skeletonAnimation == null)
        {
            CreateInstance();
        }

        if (go == null || skeletonAnimation == null)
        {
            failed = true;
            return;
        }

        go.SetActive(true);
        UpdatePosition();
        BeginGameCameraFocus();

        skeletonAnimation.AnimationState.ClearTracks();
        var startEntry = skeletonAnimation.AnimationState.SetAnimation(0, AnimStart, false);
        startEntry.TimeScale = StartAnimationSpeed;
        skeletonAnimation.AnimationState.AddAnimation(0, AnimLoop, true, 0f);
        Debug.Log("[RongThanNamekSpine] Show Namek dragon Spine at " + x + "," + y);
    }

    public static void Hide()
    {
        if (go == null || skeletonAnimation == null)
        {
            return;
        }

        hiding = true;
        focusingGameCamera = false;
        skeletonAnimation.AnimationState.ClearTracks();
        var entry = skeletonAnimation.AnimationState.SetAnimation(0, AnimEnd, false);
        entry.TimeScale = EndAnimationSpeed;
        entry.Complete += delegate
        {
            if (hiding && go != null)
            {
                go.SetActive(false);
            }
        };
    }

    public static void ForceDispose()
    {
        if (go != null)
        {
            Object.Destroy(go);
        }
        if (overlayCamera != null)
        {
            Object.Destroy(overlayCamera.gameObject);
        }
        if (overlayTexture != null)
        {
            overlayTexture.Release();
            Object.Destroy(overlayTexture);
        }
        go = null;
        skeletonAnimation = null;
        overlayCamera = null;
        overlayTexture = null;
    }

    public static void Update()
    {
        if (go != null && go.activeSelf)
        {
            UpdatePosition();
        }
    }

    public static void DrawOverlay()
    {
        if (go == null || !go.activeSelf || overlayCamera == null)
        {
            return;
        }

        EnsureOverlayTexture();
        if (overlayTexture == null)
        {
            return;
        }

        overlayCamera.Render();
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture, ScaleMode.StretchToFill, true);
    }

    private static void EnsureLoaded()
    {
        if (loaded || failed)
        {
            return;
        }

        TextAsset skel = Resources.Load<TextAsset>(AssetPath + ".skel");
        TextAsset atlas = Resources.Load<TextAsset>(AssetPath + ".atlas");
        Texture2D texture = Resources.Load<Texture2D>(AssetPath);

        if (skel == null || atlas == null || texture == null)
        {
            Debug.LogWarning("[RongThanNamekSpine] Missing Spine assets, fallback to Effect 25. skel=" + (skel != null) + " atlas=" + (atlas != null) + " tex=" + (texture != null));
            failed = true;
            return;
        }

        Shader shader = Shader.Find("Spine/Skeleton");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        material = new Material(shader);
        material.mainTexture = texture;
        atlasAsset = SpineAtlasAsset.CreateRuntimeInstance(atlas, new Texture2D[] { texture }, material, true);
        skeletonDataAsset = SkeletonDataAsset.CreateRuntimeInstance(skel, atlasAsset, true, SkeletonScale);

        if (skeletonDataAsset == null || skeletonDataAsset.GetSkeletonData(true) == null)
        {
            Debug.LogWarning("[RongThanNamekSpine] Could not load skeleton data, fallback to Effect 25.");
            failed = true;
            return;
        }

        loaded = true;
        Debug.Log("[RongThanNamekSpine] Runtime assets loaded OK.");
    }

    private static void CreateInstance()
    {
        EnsureOverlayCamera();

        go = new GameObject("RongThanNamekSpine");
        Object.DontDestroyOnLoad(go);
        go.layer = OverlayLayer;
        skeletonAnimation = SkeletonAnimation.AddToGameObject(go, skeletonDataAsset);
        skeletonAnimation.Initialize(true);

        MeshRenderer renderer = go.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 101;
        }
    }

    private static void EnsureOverlayCamera()
    {
        if (overlayCamera != null)
        {
            return;
        }

        GameObject camGo = new GameObject("RongThanNamekOverlayCamera");
        Object.DontDestroyOnLoad(camGo);
        overlayCamera = camGo.AddComponent<Camera>();
        overlayCamera.enabled = false;
        overlayCamera.orthographic = true;
        overlayCamera.clearFlags = CameraClearFlags.SolidColor;
        overlayCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        overlayCamera.cullingMask = 1 << OverlayLayer;
        overlayCamera.nearClipPlane = -1000f;
        overlayCamera.farClipPlane = 1000f;
        overlayCamera.transform.position = new Vector3(0f, 0f, -10f);
        EnsureOverlayTexture();
    }

    private static void EnsureOverlayTexture()
    {
        int w = Mathf.Max(1, Screen.width);
        int h = Mathf.Max(1, Screen.height);
        if (overlayTexture != null && textureW == w && textureH == h)
        {
            return;
        }

        if (overlayTexture != null)
        {
            overlayTexture.Release();
            Object.Destroy(overlayTexture);
        }

        textureW = w;
        textureH = h;
        overlayTexture = new RenderTexture(w, h, 16, RenderTextureFormat.ARGB32);
        overlayTexture.Create();
        overlayCamera.targetTexture = overlayTexture;
        overlayCamera.orthographicSize = h * 0.5f;
    }

    public static bool UpdateGameCameraFocus()
    {
        if (!focusingGameCamera || go == null || !go.activeSelf)
        {
            return false;
        }

        int targetX = ClampCameraX(mapX - GameScr.gW2);
        int targetY = ClampCameraY(mapY - GameScr.gH23);
        GameScr.cmtoX = targetX;
        GameScr.cmtoY = targetY;

        if (mSystem.currentTimeMillis() - gameCameraFocusStart >= (long)(GameCameraFocusDuration * 1000f))
        {
            focusingGameCamera = false;
            return false;
        }
        return true;
    }

    private static void BeginGameCameraFocus()
    {
        focusingGameCamera = true;
        gameCameraFocusStart = mSystem.currentTimeMillis();
        GameScr.cmtoX = ClampCameraX(mapX - GameScr.gW2);
        GameScr.cmtoY = ClampCameraY(mapY - GameScr.gH23);
    }

    private static int ClampCameraX(int value)
    {
        if (value < 24)
        {
            return 24;
        }
        if (value > GameScr.cmxLim)
        {
            return GameScr.cmxLim;
        }
        return value;
    }

    private static int ClampCameraY(int value)
    {
        if (value < 0)
        {
            return 0;
        }
        if (value > GameScr.cmyLim)
        {
            return GameScr.cmyLim;
        }
        return value;
    }

    private static void UpdatePosition()
    {
        float screenX = (mapX + OffsetX - GameScr.cmx) * mGraphics.zoomLevel;
        float screenY = (mapY + OffsetY - GameScr.cmy) * mGraphics.zoomLevel;
        float centeredX = screenX - Screen.width * 0.5f;
        float centeredY = Screen.height * 0.5f - screenY;
        go.transform.position = new Vector3(centeredX, centeredY, 0f);
    }
}
