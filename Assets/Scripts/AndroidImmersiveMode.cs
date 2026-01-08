using UnityEngine;

public class AndroidImmersiveMode : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        AllowCutout();
        ApplyInsets();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            ApplyInsets();
    }

    void ApplyInsets()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer =
               new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaObject window =
                activity.Call<AndroidJavaObject>("getWindow");

            AndroidJavaObject controller =
                window.Call<AndroidJavaObject>("getInsetsController");

            if (controller == null) return;

            // WindowInsets.Type.statusBars() | navigationBars()
            int statusBars = 1;
            int navigationBars = 2;

            controller.Call("hide", statusBars | navigationBars);
            controller.Call("setSystemBarsBehavior", 2);
            // BEHAVIOR_SHOW_TRANSIENT_BARS_BY_SWIPE = 2
        }
#endif
    }

    void AllowCutout()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (AndroidJavaClass unityPlayer =
           new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    {
        AndroidJavaObject activity =
            unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaObject window =
            activity.Call<AndroidJavaObject>("getWindow");

        AndroidJavaObject attributes =
            window.Call<AndroidJavaObject>("getAttributes");

        // LAYOUT_IN_DISPLAY_CUTOUT_MODE_SHORT_EDGES = 1
        attributes.Set("layoutInDisplayCutoutMode", 1);

        window.Call("setAttributes", attributes);
    }
#endif
    }

}
