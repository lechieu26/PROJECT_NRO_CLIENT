using UnityEngine;
using Spine.Unity;
using Spine;

/// <summary>
/// Component render Spine cho 1 nhân vật sử dụng SkeletonAnimation (3D).
/// Được thiết kế để chạy với SpineCamera riêng biệt để đè lên OnGUI.
/// </summary>
public class SpineCharacterRenderer : MonoBehaviour
{
    [Header("Spine Components")]
    public SkeletonAnimation skeletonAnimation;
    
    [Header("State")]
    public string currentSkeletonName = "";
    public string currentSkin = "default";
    public string currentAnimation = "Idle";
    public bool isLoop = true;
    public int direction = 1; // 1 = phải, -1 = trái
    public float timeScale = 0.6f; // Tốc độ animation (mặc định 0.6f cho NRO)

    private bool isInitialized;

    /// <summary>
    /// Khởi tạo Spine renderer với SkeletonDataAsset
    /// </summary>
    public void Initialize(SkeletonDataAsset skeletonDataAsset, string skeletonName, string skinName = "default")
    {
        if (skeletonDataAsset == null)
        {
            Debug.LogError("[SpineCharacterRenderer] SkeletonDataAsset is null!");
            return;
        }

        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            skeletonAnimation = gameObject.AddComponent<SkeletonAnimation>();
        }

        skeletonAnimation.skeletonDataAsset = skeletonDataAsset;
        currentSkeletonName = skeletonName;
        
        // Cấu hình mesh renderer để hiển thị đúng
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 32767; // Ưu tiên hiển thị
        }

        skeletonAnimation.Initialize(true);

        // Set skin nếu có
        if (!string.IsNullOrEmpty(skinName) && skinName != "default")
        {
            try
            {
                skeletonAnimation.Skeleton.SetSkin(skinName);
                skeletonAnimation.Skeleton.SetSlotsToSetupPose();
                currentSkin = skinName;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SpineCharacterRenderer] Skin '{skinName}' not found: {e.Message}");
            }
        }

        // Animation event callback
        skeletonAnimation.AnimationState.Complete += OnAnimationComplete;

        // Play Idle by default
        SetAnimation("Idle", true);
        isInitialized = true;

        Debug.Log($"[SpineCharacterRenderer] Initialized with SkeletonAnimation, skin: {skinName}");
    }

    public void SetAnimation(string animName, bool loop, float speed = -1f)
    {
        if (!isInitialized || skeletonAnimation == null) return;
        if (string.IsNullOrEmpty(animName)) return;

        // Ánh xạ các animation chung chung từ Server sang đúng tên bộ skin hỗ trợ
        animName = GetMappedAnimationName(animName);

        if (currentAnimation == animName && isLoop == loop) return;

        currentAnimation = animName;
        isLoop = loop;

        try
        {
            skeletonAnimation.timeScale = (speed > 0) ? speed : timeScale;
            skeletonAnimation.AnimationState.SetAnimation(0, animName, loop);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SpineCharacterRenderer] Animation '{animName}' not found: {e.Message}");
            if (animName != "Idle")
            {
                try
                {
                    skeletonAnimation.AnimationState.SetAnimation(0, "Idle", true);
                    currentAnimation = "Idle";
                    isLoop = true;
                }
                catch { }
            }
        }
    }

    private string GetMappedAnimationName(string animName)
    {
        switch (animName)
        {
            case "Attack":
                return "Combo1"; // Mặc định chuyển Attack sang Combo1
            case "Injured":
            case "Hit":
                return "Hit";    // Broly dùng Hit
            case "Die":
                return "Die";    // Broly dùng Die
            case "Run":
                return "Run";
            case "Jump":
                return "Jump";
            case "Fall":
                return "Fall";
            case "Fly":
                return "Fly";
            default:
                // Nếu là dải Skill2_X thì giữ nguyên để SpineManager xử lý
                return animName;
        }
    }

    public void SetDirection(int dir)
    {
        if (!isInitialized || skeletonAnimation == null) return;
        direction = dir;
        skeletonAnimation.Skeleton.ScaleX = dir;
    }

    public void SetVisible(bool visible)
    {
        if (gameObject != null)
        {
            gameObject.SetActive(visible);
        }
    }

    public bool IsVisible()
    {
        return gameObject != null && gameObject.activeSelf;
    }

    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        if (trackEntry.TrackIndex == 0 && !trackEntry.Loop)
        {
            SetAnimation("Idle", true);
        }
    }

    private void OnDestroy()
    {
        if (skeletonAnimation != null && skeletonAnimation.AnimationState != null)
        {
            skeletonAnimation.AnimationState.Complete -= OnAnimationComplete;
        }
    }

    public void ChangeSkin(string skinName)
    {
        if (!isInitialized || skeletonAnimation == null) return;
        if (!string.IsNullOrEmpty(skinName) && currentSkin != skinName)
        {
            try
            {
                skeletonAnimation.Skeleton.SetSkin(skinName);
                skeletonAnimation.Skeleton.SetSlotsToSetupPose();
                currentSkin = skinName;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SpineCharacterRenderer] Skin '{skinName}' not found: {e.Message}");
            }
        }
    }
}
