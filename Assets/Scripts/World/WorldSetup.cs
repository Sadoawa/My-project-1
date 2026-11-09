using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Đặt tất cả InteractionZone trong scene GameWorld theo bản đồ từ world.js.
/// Gắn vào một GameObject rỗng trong scene.
/// Thêm NPC spawn và điểm tương tác.
/// </summary>
public class WorldSetup : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject interactionZonePrefab;

    // ─────────────────────────────────────────────────────────────────────────
    // Dữ liệu điểm tương tác — port từ interactions[] trong world.js
    // ─────────────────────────────────────────────────────────────────────────
    private static readonly InteractionDef[] INTERACTIONS = new InteractionDef[]
    {
        // Phố
        new InteractionDef("food",     -30.0f,  10.0f, "Cô Hạnh · Bánh mì",           "Mua bữa ăn để hồi phục năng lượng"),
        new InteractionDef("help",      -9.0f,  -6.0f, "Bạn sinh viên làm rơi tài liệu","Giúp bạn hay tiếp tục đi?"),
        new InteractionDef("wallet",    33.0f,  -6.0f, "Một chiếc ví trên đường",      "Quyết định phải làm gì với chiếc ví"),
        // Quán Chạm Coffee & UEH (Tương tác khu vực)
        new InteractionDef("manager",   -3.8f, -20.2f, "Anh Minh · Quản lý quán",    "Nhận ca và phục vụ khách"),
        new InteractionDef("tray",      -2.5f, -18.5f, "Khay cà phê",                 "Nhận khay ở quầy"),
        new InteractionDef("friend",    18.5f, -11.5f, "Linh · Bạn cùng nhóm",       "Trao đổi trực tiếp về bài thuyết trình"),
        new InteractionDef("teacher",   25.5f, -22.5f, "Thầy An",                     "Hỏi về bài tập và cách làm việc nhóm"),
        // Garage sửa xe
        new InteractionDef("repair",    63.0f, -12.0f, "Chú Tâm · Sửa xe",            "Sửa xe sau va chạm"),
    };

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        EnsureRuntimeWorld();
        SpawnInteractionZones();
        SetupLighting();
    }

    void EnsureRuntimeWorld()
    {
        // 1. Kiểm tra mặt đất — Nếu Scene đã có Terrain_Base hoặc Environment thì KHÔNG tự ý tạo thêm plane trắng đè lên!
        bool hasGround = GameObject.Find("Terrain_Base") != null
                      || GameObject.Find("Terrain_Ground") != null
                      || GameObject.Find("Ground") != null
                      || GameObject.Find("Environment") != null;

        if (!hasGround)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Terrain_Ground";
            ground.transform.position = new Vector3(0, -0.3f, -20);
            ground.transform.localScale = new Vector3(25, 1, 22);
            var rend = ground.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = new Color(0.55f, 0.70f, 0.48f); // Màu xanh cỏ tự nhiên
                rend.material = mat;
            }
        }

        // 2. Kiểm tra Player
        if (PlayerController.Instance == null)
        {
            var player = GameObject.Find("Player");
            if (player == null)
            {
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Player";
                player.transform.position = new Vector3(-18f, 1f, -13f);
                var cc = player.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.center = new Vector3(0, 0.9f, 0);
                player.AddComponent<PlayerController>();
            }
        }

        // 3. Kiểm tra Camera
        if (Camera.main == null)
        {
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(-18f, 3.5f, -18f);
            var p = GameObject.Find("Player");
            if (p != null) camGO.transform.LookAt(p.transform.position + Vector3.up * 1.5f);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    void SpawnInteractionZones()
    {
        foreach (var def in INTERACTIONS)
        {
            GameObject go = interactionZonePrefab != null
                ? Instantiate(interactionZonePrefab)
                : new GameObject($"Zone_{def.id}");

            go.name = $"Zone_{def.id}";
            go.transform.position = new Vector3(def.x, 0.5f, def.z);
            go.transform.parent   = transform;

            var zone = go.GetComponent<InteractionZone>() ?? go.AddComponent<InteractionZone>();
            zone.interactionId = def.id;
            zone.label         = def.label;
            zone.subLabel      = def.subLabel;
            zone.radius        = 2f;

            // Thêm SphereCollider trigger cho proximity check
            var col = go.GetComponent<SphereCollider>() ?? go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius    = zone.radius;
        }

        Debug.Log($"[WorldSetup] Spawned {INTERACTIONS.Length} interaction zones.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    void SetupLighting()
    {
        // Nếu trong Scene đã có Directional Light (như SunLight_Directional) thì không tạo thêm để tránh chói lóa
        if (GameObject.Find("SunLight_Directional") != null || GameObject.Find("SunLight") != null)
            return;

        // Ánh sáng bầu trời (Hemisphere) — tương đương THREE.HemisphereLight
        RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor    = new Color(0.79f, 0.91f, 1.00f);
        RenderSettings.ambientGroundColor = new Color(0.56f, 0.59f, 0.59f);

        // Ánh sáng mặt trời
        GameObject sunGO = new GameObject("SunLight");
        var sun = sunGO.AddComponent<Light>();
        sun.type      = LightType.Directional;
        sun.color     = new Color(1f, 0.95f, 0.87f);
        sun.intensity = 1.8f;
        sun.shadows   = LightShadows.Soft;
        sunGO.transform.SetPositionAndRotation(
            new Vector3(-26, 55, 30),
            Quaternion.Euler(45f, -30f, 0f)
        );

        // Fog
        RenderSettings.fog          = true;
        RenderSettings.fogColor     = new Color(0.69f, 0.84f, 0.91f);
        RenderSettings.fogMode      = FogMode.Linear;
        RenderSettings.fogStartDistance = 85f;
        RenderSettings.fogEndDistance   = 230f;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Data
    // ─────────────────────────────────────────────────────────────────────────
    private struct InteractionDef
    {
        public string id, label, subLabel;
        public float  x, z;
        public InteractionDef(string id, float x, float z, string label, string subLabel)
        {
            this.id       = id;
            this.x        = x;
            this.z        = z;
            this.label    = label;
            this.subLabel = subLabel;
        }
    }
}

