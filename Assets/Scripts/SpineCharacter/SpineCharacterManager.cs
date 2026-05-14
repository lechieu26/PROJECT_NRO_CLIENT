using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

/// <summary>
/// Manager quản lý tất cả SpineCharacterRenderer sử dụng SkeletonAnimation (3D).
/// Tạo một Camera riêng biệt để render Spine đè lên trên OnGUI.
/// </summary>
public class SpineCharacterManager : MonoBehaviour
{
    private static SpineCharacterManager instance;
    public static SpineCharacterManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SpineCharacterManager");
                instance = go.AddComponent<SpineCharacterManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private Dictionary<string, SkeletonDataAsset> skeletonCache = new Dictionary<string, SkeletonDataAsset>();
    private Dictionary<int, SpineCharacterRenderer> spineCharacters = new Dictionary<int, SpineCharacterRenderer>();
    
    private Camera spineCamera;
    private RenderTexture spinetexture;
    private const int SPINE_LAYER = 31; // Lớp dành riêng cho Spine thế giới
    private const int PREVIEW_SPINE_LAYER = 30; // Lớp dành riêng cho Spine xem trước (UI)

    private Camera previewSpineCamera;
    private RenderTexture previewSpineTexture;
    private const int PREVIEW_TEX_SIZE = 256; // Kích thước texture preview (pixels)

    // Renderer riêng cho chế độ xem trước trong UI
    private SpineCharacterRenderer previewRenderer;
    private int currentPreviewCharId = -1;
    private string lastPreviewSkeleton = "";
    private string lastPreviewSkin = "";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        EnsureCamera();
    }

    private void LateUpdate()
    {
        EnsureCamera();
        UpdatePositions();
    }

    public void PaintSpine(mGraphics g)
    {
        // Vô hiệu hóa hàm vẽ toàn cục cũ để chuyển sang vẽ theo từng nhân vật (interleaved)
        // Nếu muốn vẽ đè lên tất cả như cũ thì để lại, nhưng user muốn nó như skin bình thường.
    }

    /// <summary>
    /// Vẽ phần texture Spine tương ứng với vị trí của nhân vật này.
    /// Giúp Spine nhân vật được vẽ đúng thứ tự Z-order với các nhân vật 2D khác.
    /// </summary>
    public void PaintSpineForChar(mGraphics g, Char c, int x, int y)
    {
        if (spinetexture == null || c == null || !c.useSpine) return;
        if (c.isMonkey > 0 || c.isFusion || c.isHide) return;

        int zoom = mGraphics.zoomLevel;
        
        // Vùng clip quanh nhân vật (tọa độ logic mGraphics)
        int drawW = 150; 
        int drawH = 150;
        int drawX = x - drawW / 2;
        int drawY = y - drawH + 20;

        // Lưu lại clip cũ
        int oldCX = g.clipX / zoom;
        int oldCY = g.clipY / zoom;
        int oldCW = g.clipW / zoom;
        int oldCH = g.clipH / zoom;
        bool oldIsClip = g.isClip;

        // Thiết lập vùng hiển thị cho nhân vật này
        g.setClip(drawX, drawY, drawW, drawH);
        
        // Vẽ toàn bộ texture thế giới tại vị trí bù trừ translation của mGraphics (đơn vị pixel)
        // Truyền tọa độ LOGIC. drawRenderTexture sẽ tự nhân zoom và cộng translateX pixel.
        g.drawRenderTexture(spinetexture, -g.translateX / zoom, -g.translateY / zoom);

        // Khôi phục clip cũ
        if (oldIsClip) g.setClip(oldCX, oldCY, oldCW, oldCH);
        else g.isClip = false;
    }

    /// <summary>
    /// Vẽ nhân vật Spine xem trước như một element UI bình thường.
    /// Tọa độ x,y là vị trí trung tâm nhân vật trong hệ tọa độ mGraphics (content space).
    /// Tự động tuân thủ translate và setClip của mGraphics, giống fillRect.
    /// </summary>
    public void PaintPreviewSpine(mGraphics g, int x, int y)
    {
        if (previewSpineTexture == null || previewSpineCamera == null) return;
        if (previewRenderer == null || !previewRenderer.IsVisible()) return;

        int zoom = mGraphics.zoomLevel;
        int pw = PREVIEW_TEX_SIZE;
        int ph = PREVIEW_TEX_SIZE;
        
        // Tọa độ truyền vào x, y là logic trung tâm chân
        // Chúng ta tính logicX, logicY sao cho tâm chân texture khớp với x, y
        // drawRenderTexture sẽ tự nhân zoom
        int logicX = x - (pw / 2) / zoom;
        int logicY = (y - 70);
        int logicW = pw / zoom;
        int logicH = ph / zoom;

        g.drawRenderTexture(previewSpineTexture, logicX, logicY, logicW, logicH);
    }

    private void EnsureCamera()
    {
        float halfH = UnityEngine.Screen.height / 2f;
        float halfW = UnityEngine.Screen.width / 2f;

        // 1. Khởi tạo Camera World
        if (spineCamera == null)
        {
            GameObject camObj = GameObject.Find("SpineCamera");
            if (camObj == null)
            {
                camObj = new GameObject("SpineCamera");
                spineCamera = camObj.AddComponent<Camera>();
                spineCamera.orthographic = true;
                spineCamera.clearFlags = CameraClearFlags.SolidColor; 
                spineCamera.backgroundColor = new Color(0, 0, 0, 0); 
                spineCamera.depth = 100; 
                spineCamera.cullingMask = 1 << SPINE_LAYER; // Chỉ nhìn thấy layer 31
                spineCamera.nearClipPlane = 0.1f;
                spineCamera.farClipPlane = 100f;
                
                spinetexture = new UnityEngine.RenderTexture(UnityEngine.Screen.width, UnityEngine.Screen.height, 24, UnityEngine.RenderTextureFormat.ARGB32);
                spinetexture.Create();
                spineCamera.targetTexture = spinetexture;

                camObj.transform.position = new UnityEngine.Vector3(halfW, halfH, -10);
                DontDestroyOnLoad(camObj);
            }
            else spineCamera = camObj.GetComponent<Camera>();
        }

        // 2. Khởi tạo Camera UI Preview (texture nhỏ, camera nhìn gốc tọa độ)
        if (previewSpineCamera == null)
        {
            GameObject pCamObj = GameObject.Find("SpinePreviewCamera");
            if (pCamObj == null)
            {
                pCamObj = new GameObject("SpinePreviewCamera");
                previewSpineCamera = pCamObj.AddComponent<Camera>();
                previewSpineCamera.orthographic = true;
                previewSpineCamera.clearFlags = CameraClearFlags.SolidColor;
                previewSpineCamera.backgroundColor = new Color(0, 0, 0, 0);
                previewSpineCamera.depth = -100; // Không hiển thị trực tiếp lên màn hình
                previewSpineCamera.cullingMask = 1 << PREVIEW_SPINE_LAYER;
                previewSpineCamera.nearClipPlane = 0.1f;
                previewSpineCamera.farClipPlane = 100f;

                previewSpineTexture = new UnityEngine.RenderTexture(PREVIEW_TEX_SIZE, PREVIEW_TEX_SIZE, 24, UnityEngine.RenderTextureFormat.ARGB32);
                previewSpineTexture.Create();
                previewSpineCamera.targetTexture = previewSpineTexture;

                // Camera nhìn vào gốc (0,0) - nhân vật preview sẽ đặt tại (0,0)
                pCamObj.transform.position = new UnityEngine.Vector3(0, 0, -10);
                // orthographicSize = nửa chiều cao texture = PREVIEW_TEX_SIZE / 2
                previewSpineCamera.orthographicSize = PREVIEW_TEX_SIZE / 2f;
                DontDestroyOnLoad(pCamObj);
            }
            else previewSpineCamera = pCamObj.GetComponent<Camera>();
        }

        spineCamera.orthographicSize = halfH;
        spineCamera.transform.position = new UnityEngine.Vector3(halfW, halfH, -10);
        // Preview camera luôn nhìn gốc (0,0) với kích thước cố định
        previewSpineCamera.orthographicSize = PREVIEW_TEX_SIZE / 2f;
        previewSpineCamera.transform.position = new UnityEngine.Vector3(0, 0, -10);

        // Kiểm tra Resize cho World camera
        if (spinetexture == null || spinetexture.width != Screen.width || spinetexture.height != Screen.height)
        {
            if (spinetexture != null) spinetexture.Release();
            spinetexture = new UnityEngine.RenderTexture(Screen.width, Screen.height, 24, UnityEngine.RenderTextureFormat.ARGB32);
            spineCamera.targetTexture = spinetexture;
        }
        // Preview texture luôn cố định PREVIEW_TEX_SIZE x PREVIEW_TEX_SIZE
        if (previewSpineTexture == null)
        {
            previewSpineTexture = new UnityEngine.RenderTexture(PREVIEW_TEX_SIZE, PREVIEW_TEX_SIZE, 24, UnityEngine.RenderTextureFormat.ARGB32);
            previewSpineTexture.Create();
            previewSpineCamera.targetTexture = previewSpineTexture;
        }
    }

    public SpineCharacterRenderer AddOrUpdateCharacter(int charId, string skeletonName, string skinName, Vector2 position)
    {
        if (spineCharacters.ContainsKey(charId))
        {
            SpineCharacterRenderer existingRenderer = spineCharacters[charId];
            if (existingRenderer.currentSkin != skinName)
            {
                existingRenderer.ChangeSkin(skinName);
            }
            return existingRenderer;
        }

        SkeletonDataAsset skeletonData = LoadSkeletonData(skeletonName);
        if (skeletonData == null) return null;

        GameObject charObj = new GameObject($"SpineChar_{charId}");
        SetLayerRecursively(charObj, SPINE_LAYER);

        SpineCharacterRenderer renderer = charObj.AddComponent<SpineCharacterRenderer>();
        renderer.Initialize(skeletonData, skeletonName, skinName);
        
        // Luôn đảm bảo renderers con cũng đúng layer
        SetLayerRecursively(charObj, SPINE_LAYER);

        spineCharacters[charId] = renderer;
        Debug.Log($"[SpineCharacterManager] Created 3D Spine for {charId} at layer {SPINE_LAYER}");
        return renderer;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void RemoveCharacter(int charId)
    {
        if (spineCharacters.ContainsKey(charId))
        {
            SpineCharacterRenderer renderer = spineCharacters[charId];
            if (renderer != null) Destroy(renderer.gameObject);
            spineCharacters.Remove(charId);
        }
    }

    public SpineCharacterRenderer GetRenderer(int charId)
    {
        spineCharacters.TryGetValue(charId, out SpineCharacterRenderer renderer);
        return renderer;
    }

    public void SetCharacterAnimation(int charId, string animation, bool loop)
    {
        SpineCharacterRenderer renderer = GetRenderer(charId);
        if (renderer != null) renderer.SetAnimation(animation, loop);
    }

    private void UpdatePositions()
    {
        if (spineCharacters.Count == 0) return;

        List<int> toRemove = new List<int>();
        int zoom = mGraphics.zoomLevel;

        bool panelVisible = (GameCanvas.panel != null && GameCanvas.panel.isShow) || 
                            (GameCanvas.panel2 != null && GameCanvas.panel2.isShow) ||
                            CustomInventoryPanel.isShow;

        Char previewChar = null;

        foreach (var kvp in spineCharacters)
        {
            int charId = kvp.Key;
            SpineCharacterRenderer renderer = kvp.Value;

            Char c = GameScr.findCharInMap(charId);
            if (c == null && Char.myCharz() != null)
            {
                if (charId == Char.myCharz().charID)
                {
                    c = Char.myCharz();
                }
                else if (charId == -Char.myCharz().charID && Char.myPetz() != null)
                {
                    c = Char.myPetz();
                    c.charID = charId;
                }
            }

            if (c != null)
            {
                // Nếu Panel đã đóng, chắc chắn reset biến Preview
                if (!panelVisible) c.isPreviewSpine = false;

                if (c.isPreviewSpine && panelVisible)
                {
                    previewChar = c;
                }
                else
                {
                    // Chỉ cập nhật vị trí WORLD khi không ở chế độ Preview hoặc panel đã đóng
                    // Việc này tránh skin bị nhảy tọa độ khi OnGUI thay đổi cx, cy
                    float screenX = (float)(c.cx - GameScr.cmx) * zoom;
                    float screenY = (float)Screen.height - (float)(c.cy - GameScr.cmy + GameCanvas.transY) * zoom;
                    renderer.transform.position = new Vector3(screenX, screenY, 0);
                }

                float finalScale = 16.5f * zoom; 
                renderer.transform.localScale = new Vector3(finalScale, finalScale, 1);

                renderer.SetDirection(c.cdir);
                UpdateAnimationByCharState(renderer, c);
                
                // Ẩn Spine nếu đang biến hình (Khỉ) hoặc Hợp thể hoặc bị ẩn hoàn toàn
                bool shouldShowSpine = (c.isMonkey == 0 && !c.isFusion && !c.isHide);
                renderer.SetVisible(shouldShowSpine);
                
                // Xử lý độ trong suốt cho Tàng hình
                if (shouldShowSpine && renderer.skeletonAnimation != null)
                {
                    float alpha = 1.0f;
                    if (c.me && c.isTanHinh) alpha = 0.4f; // Player tự nhìn mình mờ mờ
                    else if (c.isTanHinh) alpha = 0f;      // Đối thủ không nhìn thấy (hoặc mờ tùy server)
                    
                    if (renderer.skeletonAnimation.skeleton != null)
                    {
                        renderer.skeletonAnimation.skeleton.A = alpha;
                    }
                }
            }
            else
            {
                toRemove.Add(charId);
            }
        }

        // Xử lý mô hình xem trước (Preview) riêng biệt
        UpdatePreviewRenderer(previewChar, panelVisible, zoom);

        foreach (int id in toRemove) RemoveCharacter(id);
    }

    private void UpdatePreviewRenderer(Char c, bool panelVisible, int zoom)
    {
        if (c == null || !panelVisible || !c.useSpine)
        {
            if (previewRenderer != null) previewRenderer.SetVisible(false);
            if (c != null && !panelVisible) c.isPreviewSpine = false;
            return;
        }

        // Lấy thông tin từ renderer thế giới của nhân vật đó
        SpineCharacterRenderer worldRenderer = GetRenderer(c.charID);
        if (worldRenderer == null) return;

        // Khởi tạo/Cập nhật previewRenderer
        if (previewRenderer == null)
        {
            GameObject go = new GameObject("SpinePreviewRenderer");
            SetLayerRecursively(go, PREVIEW_SPINE_LAYER);
            previewRenderer = go.AddComponent<SpineCharacterRenderer>();
            DontDestroyOnLoad(go);
        }

        // Đồng bộ Skeleton và Skin nếu thay đổi
        if (lastPreviewSkeleton != worldRenderer.currentSkeletonName || lastPreviewSkin != worldRenderer.currentSkin)
        {
            SkeletonDataAsset data = LoadSkeletonData(worldRenderer.currentSkeletonName);
            if (data != null)
            {
                previewRenderer.Initialize(data, worldRenderer.currentSkeletonName, worldRenderer.currentSkin);
                lastPreviewSkeleton = worldRenderer.currentSkeletonName;
                lastPreviewSkin = worldRenderer.currentSkin;
                SetLayerRecursively(previewRenderer.gameObject, PREVIEW_SPINE_LAYER);
            }
        }

        // Đặt nhân vật preview tại gốc (0,0) — camera preview nhìn vào đây
        previewRenderer.transform.position = new Vector3(0, 0, 0);
        
        // Scale chuẩn (không âm Y) cho chế độ No-Flip
        float finalScale = 16.5f * zoom;
        previewRenderer.transform.localScale = new Vector3(finalScale, finalScale, 1);

        previewRenderer.SetVisible(true);
        previewRenderer.SetDirection(1);
        UpdateAnimationByCharState(previewRenderer, c);
    }

    private void UpdateAnimationByCharState(SpineCharacterRenderer renderer, Char c)
    {
        string targetAnim = "Idle";
        bool loop = true;
        float animSpeed = 1.0f; // Mặc định tốc độ là 1.0f

        // 1. Chết hoặc Bất động (Ưu tiên cao nhất)
        if (c.statusMe == 14 || c.statusMe == 5 || c.cf == 23) 
        { 
            targetAnim = "Die"; 
            loop = false; 
        }
        // 2. Chạy (Run)
        else if (c.statusMe == 2) 
        {
            targetAnim = "Run";
            loop = true;
        }
        // 3. Nhảy (Jump)
        else if (c.statusMe == 3 || c.statusMe == 9)
        {
            targetAnim = "Jump";
            loop = false;
        }
        // 4. Rơi (Fall)
        else if (c.statusMe == 4)
        {
            targetAnim = c.isFlyUp ? "Fly" : "Fall";
            loop = !c.isFlyUp;
        }
        // 5. Bay (Fly)
        else if (c.statusMe == 10)
        {
            targetAnim = "Fly";
            loop = true;
        }
        // 6. Gồng năng lượng (Charge)
        else if (c.isCharge || c.isStandAndCharge || c.isFlyAndCharge || c.cf == 17)
        {
            targetAnim = "Skill5_5"; 
            loop = true;
            animSpeed = 1.0f; // Gồng năng lượng dùng tốc độ gốc
        }
        // 7. Tấn công / Skill (Dựa trên skillPaint và status)
        else if (c.isAttack || c.statusMe == 7 || c.isAttFly || c.skillPaint != null ||
                 c.cf == 9 || c.cf == 10 || c.cf == 11 || 
                 c.cf == 7 || c.cf == 12 || c.cf == 13 || c.statusMe == 12 || c.statusMe == 13)
        {
            targetAnim = GetAttackAnimationName(c);
            loop = false;
            
            // Nếu là chiêu bắn chưởng, giảm tốc độ (0.5f) theo yêu cầu
            if (targetAnim.StartsWith("Skill2_")) 
            {
                animSpeed = 0.5f; 
            }
            else if (targetAnim.StartsWith("Combo"))
            {
                animSpeed = 1.0f; // Đấm cận chiến dùng tốc độ gốc
            }
        }
        // 8. Bị thương (Hit)
        else if (c.statusMe == 8 || c.cf == 8)
        {
            targetAnim = "Hit";
            loop = false;
        }

        renderer.SetAnimation(targetAnim, loop, animSpeed);
    }

    private string GetAttackAnimationName(Char c)
    {
        // Lấy cấp độ kỹ năng hiện tại (mặc định 1 nếu không rõ)
        int skillLevel = (c.myskill != null) ? c.myskill.point : 1;

        if (c.skillPaint != null)
        {
            int id = c.skillPaint.id;
            
            // 0. Biến khỉ / Hóa hình (Ưu tiên cao nhất)
            if ((id >= 35 && id <= 41) || id == 105 || id == 165 || c.isWaitMonkey)
            {
                return "ZSkill7";
            }

            // 1. Nhóm đấm cận chiến (Cố định Combo1)
            bool isMelee = (id >= 0 && id <= 6) || (id >= 14 && id <= 20) || 
                          (id >= 28 && id <= 34) || (id >= 63 && id <= 69) ||
                          (id >= 107 && id <= 109) || id == 164;

            if (isMelee)
            {
                return "Combo1"; 
            }

            // 2. Nhóm bắn chưởng (Theo yêu cầu: cấp 1-5 là Skill2_4, cấp 6-7 là Skill2_1)
            if (skillLevel >= 6) return "Skill2_1";
            return "Skill2_4";
        }

        // 3. Fallback cho frame bắn chưởng cơ bản (cf 12, 13)
        if (c.cf == 12 || c.cf == 13) 
        {
            return (skillLevel >= 6) ? "Skill2_1" : "Skill2_4";
        }

        // 4. Fallback cho frame đấm cận chiến
        if (c.cf == 9 || c.cf == 10 || c.cf == 11)
        {
            return "Combo1";
        }

        return "Combo1"; 
    }

    public void ClearAll()
    {
        foreach (var r in spineCharacters.Values) if (r != null) Destroy(r.gameObject);
        spineCharacters.Clear();
    }

    private SkeletonDataAsset LoadSkeletonData(string skeletonName)
    {
        if (skeletonCache.ContainsKey(skeletonName)) return skeletonCache[skeletonName];
        
        // Sử dụng SpineSkinManager để tìm path linh hoạt
        string path = SpineSkinManager.GetResourcePath(skeletonName);
        SkeletonDataAsset asset = Resources.Load<SkeletonDataAsset>(path);
        
        if (asset != null) skeletonCache[skeletonName] = asset;
        else Debug.LogError($"[Spine] Failed to load SkeletonData at: {path}");
        
        return asset;
    }
}
