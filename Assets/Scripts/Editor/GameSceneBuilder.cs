#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Tái tạo cảnh quan chuẩn nguyên tác "Qua Tháng Này Đã":
///   - Lòng lề đường & vỉa hè được làm chi tiết tỉ mỉ:
///       + Gạch vỉa hè sáng màu có rãnh xẻ dọc và khe co giãn ngang (như ảnh)
///       + Gờ đá bó vỉa hè vát khối 3m có khe hở
///       + Vạch sơn trắng mép đường và rãnh thoát nước lòng lề đường
///       + Vạch ngựa vằn qua đường
///   - HOÀN TOÀN KHÔNG CÓ XE CỘ (cả xe đỗ và xe đang chạy đã được xóa sạch)
///   - HOÀN TOÀN KHÔNG CÓ NPC
///   - HOÀN TOÀN KHÔNG CÓ NỘI THẤT (không bàn, ghế, giường, tủ, laptop, ghế băng)
///   - Giữ lại cảnh quan thuần túy: nhà phố Sài Gòn, hàng cây xanh tán tròn, cột đèn đường
/// </summary>
public static class GameSceneBuilder
{
    [MenuItem("Qua Thang Nay Da/Thiết kế lại cảnh vật (Lòng lề đường chi tiết - Không xe cộ - Không nội thất)")]
    public static void BuildFullScene()
    {
        // 1. Tạo Scene mới tinh
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2. Bầu trời xanh trong vắt & Ánh sáng Sài Gòn
        SetupSaigonLighting();

        // 3. Toàn bộ cảnh vật khu phố chuẩn nguyên tác (Lòng lề đường làm kĩ)
        var envRoot = new GameObject("Environment");
        BuildDetailedAvenueAndSidewalks(envRoot);
        BuildSaigonShophouses(envRoot);
        BuildCampusSide(envRoot);
        BuildIconicTrees(envRoot);
        BuildStreetLamps(envRoot);
        BuildStreetVendors(envRoot);

        // (LƯU Ý: Đã lược bỏ hoàn toàn xe cộ, NPC và toàn bộ nội thất theo yêu cầu)

        // 4. Tạo Player tại vị trí & góc nhìn chuẩn như trong ảnh
        var playerGO = CreateFirstPersonPlayer();

        // 5. Camera góc nhìn thứ nhất hướng dọc theo đại lộ
        CreateFirstPersonCamera(playerGO);

        // 6. Tạo Game Core (GameManager, TimeManager, InteractionHandler, WorldSetup)
        var coreGO = new GameObject("[GameCore]");
        coreGO.AddComponent<GameManager>();
        var tm = coreGO.AddComponent<TimeManager>();
        tm.isRunning = true;
        tm.secondsPerGameMinute = 2f;
        coreGO.AddComponent<InteractionHandler>();
        coreGO.AddComponent<WorldSetup>();

        // 7. Tạo UI HUD tối giản (chấm trắng hồng tâm giữa màn hình + thanh chỉ số)
        CreateMinimalHUD();

        // 8. Lưu Scene
        if (!Directory.Exists("Assets/Scenes"))
            Directory.CreateDirectory("Assets/Scenes");

        string scenePath = "Assets/Scenes/GameWorld.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("==================================================================");
        Debug.Log("🎉 [QuaThangNayDa] Đã thiết kế lại cảnh quan chuẩn theo yêu cầu:");
        Debug.Log("👉 ĐÃ XÓA 100% NỘI THẤT (không bàn, ghế, giường, tủ, laptop, ghế băng).");
        Debug.Log("👉 ĐÃ XÓA 100% XE CỘ (không có xe đỗ, không có xe chạy).");
        Debug.Log("👉 LÒNG LỀ ĐƯỜNG & VỈA HÈ LÀM KĨ: gạch lát chỉ dọc, khe ngang, gờ đá khối 3m, vạch mép lề đường.");
        Debug.Log("👉 Bấm nút PLAY (▶) để trải nghiệm ngay!");
        Debug.Log("==================================================================");

        EditorUtility.DisplayDialog("Thiết kế lại hoàn tất!", 
            "Đã thực hiện xong các yêu cầu của bạn:\n\n" +
            "1. XÓA HOÀN TOÀN NỘI THẤT: Không còn bàn, ghế, giường, tủ, laptop, ghế băng...\n" +
            "2. XÓA HOÀN TOÀN XE CỘ: Không còn bất kỳ xe máy hay ô tô nào.\n" +
            "3. LÒNG LỀ ĐƯỜNG LÀM KĨ (Y NHƯ ẢNH GỐC):\n" +
            "   • Mặt vỉa hè có rãnh xẻ dọc chạy dài song song\n" +
            "   • Khe co giãn ngang định kỳ\n" +
            "   • Gờ đá bó vỉa hè đúc từng khối 3m có khe hở\n" +
            "   • Vạch sơn trắng dọc mép lòng đường\n\n" +
            "Nhấn nút PLAY (▶) trên đỉnh Unity để bắt đầu trải nghiệm!", "Tuyệt vời!");
    }

    [MenuItem("Qua Thang Nay Da/Xóa tất cả nội thất khỏi Scene hiện tại")]
    public static void RemoveAllFurnitureFromCurrentScene()
    {
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        int count = 0;
        foreach (var go in allObjects)
        {
            if (go == null) continue;
            string n = go.name.ToLower();
            bool isFurniture = n.Contains("bench")
                            || n.Contains("bed")
                            || n.Contains("desk")
                            || n.Contains("chair")
                            || n.Contains("table")
                            || n.Contains("laptop")
                            || n.Contains("furniture")
                            || n.Contains("ghế")
                            || n.Contains("bàn")
                            || n.Contains("giường")
                            || n.StartsWith("prop_");

            if (isFurniture)
            {
                Undo.DestroyObjectImmediate(go);
                count++;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[QuaThangNayDa] Đã xóa {count} đối tượng nội thất khỏi Scene.");
        EditorUtility.DisplayDialog("Đã xóa hết nội thất!", $"Đã xóa thành công {count} đối tượng nội thất (bàn, ghế, giường, tủ, laptop, ghế băng...) khỏi Scene!", "OK");
    }

    [MenuItem("Qua Thang Nay Da/Xóa hết xe cộ khỏi Scene hiện tại")]
    public static void RemoveAllVehiclesFromCurrentScene()
    {
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        int count = 0;
        foreach (var go in allObjects)
        {
            if (go == null) continue;
            bool isVehicle = go.GetComponent<VehicleController>() != null
                          || go.name.Contains("Vespa")
                          || go.name.Contains("Mercedes")
                          || go.name.Contains("Traffic")
                          || go.name.Contains("Car_")
                          || go.name.Contains("car")
                          || go.name.Contains("Fadil")
                          || go.name.Contains("Vehicles");

            if (isVehicle)
            {
                Undo.DestroyObjectImmediate(go);
                count++;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[QuaThangNayDa] Đã xóa {count} đối tượng xe cộ khỏi Scene.");
        EditorUtility.DisplayDialog("Đã xóa hết xe cộ!", $"Đã xóa thành công {count} đối tượng xe cộ (cả xe đỗ và xe đang chạy) khỏi Scene!", "OK");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CÁC CÔNG CỤ TẠO BASEPLATE NHÁP / XƯỞNG THIẾT KẾ TÒA NHÀ
    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("Qua Thang Nay Da/Xưởng Thiết Kế/1. Tạo Scene Nháp Riêng (Khuyên Dùng)")]
    public static void CreateBuildingDraftSandbox()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SetupDraftLighting();

        // 1. Sàn Baseplate kẻ ô lưới tỉ lệ chuẩn 1m (120m x 120m)
        var baseplate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseplate.name = "Draft_Baseplate_Grid1m";
        baseplate.transform.position = new Vector3(0, -0.5f, 0);
        baseplate.transform.localScale = new Vector3(120f, 1f, 120f);
        baseplate.GetComponent<MeshRenderer>().material = GetOrCreateGridMaterial();

        // 2. Cột mốc tỉ lệ người thật cao 1.7m (màu cam) để căn cửa và tầng nhà
        var scaleRef = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        scaleRef.name = "[ThamKhảo]_TiLeNguoiThat_1m7";
        scaleRef.transform.position = new Vector3(0, 0.85f, 0);
        scaleRef.transform.localScale = new Vector3(0.5f, 0.85f, 0.5f);
        var refMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        refMat.color = HexColor("#e67e22");
        scaleRef.GetComponent<MeshRenderer>().material = refMat;

        // 3. Camera góc nhìn xưởng thiết kế
        var camGO = new GameObject("Studio_Camera");
        var cam = camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();
        camGO.tag = "MainCamera";
        camGO.transform.position = new Vector3(0, 8f, -16f);
        camGO.transform.rotation = Quaternion.Euler(20f, 0, 0);

        // 4. Tạo thư mục chứa Prefabs nhà tự dựng
        if (!Directory.Exists("Assets/Prefabs/Buildings"))
            Directory.CreateDirectory("Assets/Prefabs/Buildings");

        string sandboxPath = "Assets/Scenes/BuildingDraftSandbox.unity";
        EditorSceneManager.SaveScene(scene, sandboxPath);
        AddSceneToBuildSettings(sandboxPath);

        SceneView.lastActiveSceneView?.Frame(new Bounds(Vector3.zero, new Vector3(25, 15, 25)), false);

        EditorUtility.DisplayDialog("Xưởng Thiết Kế Đã Sẵn Sàng!",
            "Đã mở Scene nháp: BuildingDraftSandbox.unity\n\n" +
            "• Sàn Baseplate rộng 120m x 120m có ô lưới tỉ lệ (mỗi ô vuông nhỏ đúng 1m x 1m).\n" +
            "• Cột mốc người thật 1.7m màu cam ở giữa để bạn đối chiếu chiều cao cửa và tầng.\n" +
            "• Thoải mái dựng các khối nhà (Cube, Quads, Vật liệu...).\n" +
            "• Khi xong, gom vào 1 GameObject cha và kéo xuống thư mục 'Assets/Prefabs/Buildings' để tạo Prefab.\n\n" +
            "Để quay lại scene chính: Chọn menu 'Qua Thang Nay Da -> Xưởng Thiết Kế -> Quay Lại Scene Chính (GameWorld)'.", "Bắt đầu thiết kế!");
    }

    [MenuItem("Qua Thang Nay Da/Xưởng Thiết Kế/2. Thêm Baseplate Nháp Ngay Trong Scene Hiện Tại")]
    public static void AddDraftBaseplateToCurrentScene()
    {
        var existing = GameObject.Find("Draft_Baseplate_Local");
        if (existing != null)
        {
            Selection.activeGameObject = existing;
            SceneView.lastActiveSceneView?.Frame(existing.GetComponent<Collider>().bounds, false);
            EditorUtility.DisplayDialog("Thông báo", "Baseplate nháp đã có sẵn trong Scene! Đã chuyển camera tới vị trí đó.", "OK");
            return;
        }

        var baseplate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseplate.name = "Draft_Baseplate_Local";
        // Đặt ở vị trí cách đại lộ 70m về phía sau dãy nhà để không vướng đường phố
        baseplate.transform.position = new Vector3(0, -0.45f, 65f);
        baseplate.transform.localScale = new Vector3(80f, 1f, 80f);
        baseplate.GetComponent<MeshRenderer>().material = GetOrCreateGridMaterial();

        var scaleRef = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        scaleRef.name = "[ThamKhảo]_TiLeNguoiThat_1m7";
        scaleRef.transform.SetParent(baseplate.transform);
        scaleRef.transform.localPosition = new Vector3(0, 1.35f, 0);
        scaleRef.transform.localScale = new Vector3(0.5f / 80f, 0.85f, 0.5f / 80f);
        var refMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        refMat.color = HexColor("#e67e22");
        scaleRef.GetComponent<MeshRenderer>().material = refMat;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = baseplate;
        SceneView.lastActiveSceneView?.Frame(baseplate.GetComponent<Collider>().bounds, false);

        EditorUtility.DisplayDialog("Đã thêm Baseplate!", 
            "Đã tạo một sàn Baseplate 80m x 80m kẻ ô lưới 1m tại tọa độ (Z = 65m, phía sau dãy nhà phố).\n\n" +
            "Bạn có thể thiết kế nhà tại đây rồi kéo trực tiếp sang mặt đường phố khi ưng ý!", "Đã rõ");
    }

    [MenuItem("Qua Thang Nay Da/Xưởng Thiết Kế/Quay Lại Scene Chính (GameWorld)")]
    public static void ReturnToMainGameWorld()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        string scenePath = "Assets/Scenes/GameWorld.unity";
        if (File.Exists(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
        else
        {
            EditorUtility.DisplayDialog("Thông báo", "Không tìm thấy file GameWorld.unity. Hãy bấm 'Thiết kế lại cảnh vật' để tạo mới.", "OK");
        }
    }

    static Material GetOrCreateGridMaterial()
    {
        string dir = "Assets/Materials/Textures";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string texPath = $"{dir}/Draft_Grid.png";
        if (!File.Exists(texPath))
        {
            int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color bg = HexColor("#eef2f6");
            Color line = HexColor("#cbd5e1");
            Color border = HexColor("#64748b");

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = (x <= 2 || y <= 2 || x >= size - 3 || y >= size - 3);
                    bool isSubLine = (x == size / 2 || y == size / 2);
                    Color c = isBorder ? border : (isSubLine ? line : bg);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            File.WriteAllBytes(texPath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(texPath);

            var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer != null)
            {
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        var loadedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (loadedTex != null)
        {
            mat.mainTexture = loadedTex;
            mat.mainTextureScale = new Vector2(120f, 120f); // Mỗi ô vuông = đúng 1m x 1m
        }
        mat.color = Color.white;
        return mat;
    }

    static void SetupDraftLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = HexColor("#93c5fd");
        RenderSettings.ambientEquatorColor = HexColor("#cbd5e1");
        RenderSettings.ambientGroundColor = HexColor("#e2e8f0");

        var sunGO = new GameObject("Studio_SunLight");
        var sun = sunGO.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = HexColor("#ffffff");
        sun.intensity = 1.3f;
        sun.shadows = LightShadows.Soft;
        sunGO.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

        RenderSettings.fog = false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. ÁNH SÁNG & BẦU TRỜI XANH TRONG SÀI GÒN
    // ─────────────────────────────────────────────────────────────────────────
    static void SetupSaigonLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = HexColor("#4fa6e8");
        RenderSettings.ambientEquatorColor = HexColor("#8bc4eb");
        RenderSettings.ambientGroundColor = HexColor("#cbd5dc");

        var sunGO = new GameObject("SunLight_Directional");
        var sun = sunGO.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = HexColor("#fff8e7");
        sun.intensity = 1.8f;
        sun.shadows = LightShadows.Soft;
        sunGO.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

        RenderSettings.fog = true;
        RenderSettings.fogColor = HexColor("#91cbec");
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 110f;
        RenderSettings.fogEndDistance = 280f;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. LÒNG LỀ ĐƯỜNG & VỈA HÈ LÀM KĨ (THEO ĐÚNG ẢNH MẪU)
    // ─────────────────────────────────────────────────────────────────────────
    static void BuildDetailedAvenueAndSidewalks(GameObject root)
    {
        var streetRoot = new GameObject("Street_And_Sidewalks");
        streetRoot.transform.SetParent(root.transform);

        // 1. Mặt đất bao quanh (Cỏ nền)
        CreateBox("Terrain_Base", 0, -0.3f, -20, 260, 0.5f, 240, HexColor("#8eb37a"), streetRoot);

        // 2. Mặt đường nhựa màu xám đen
        CreateBox("Road_Asphalt", 0, 0.01f, 2, 220, 0.06f, 11.5f, HexColor("#32373e"), streetRoot);

        // 3. Vạch phân làn giữa đường đứt khúc màu vàng
        for (float x = -100; x < 100; x += 5.0f)
        {
            CreateBox("RoadDivider", x, 0.045f, 2, 2.5f, 0.02f, 0.22f, HexColor("#fceda2"), streetRoot);
        }

        // 4. Vạch qua đường cho người đi bộ (Zebra Crossings)
        for (float z = -2.6f; z < 6.8f; z += 1.05f)
        {
            CreateBox("Zebra_1", 11, 0.042f, z, 3.2f, 0.025f, 0.55f, Color.white, streetRoot);
            CreateBox("Zebra_2", -32, 0.042f, z, 3.2f, 0.025f, 0.55f, Color.white, streetRoot);
        }

        // 5. Vỉa hè bên phải (z = 9.7) và bên trái (z = -5.6)
        // Tạo với texture vỉa hè kẻ rãnh chỉ dọc và khe ngang như trong ảnh
        Material sidewalkMat = GetOrCreateSidewalkMaterial();

        BuildDetailedSidewalk("Sidewalk_Right_Detailed", 9.7f, 3.8f, 7.8f, 1, sidewalkMat, streetRoot);
        BuildDetailedSidewalk("Sidewalk_Left_Detailed", -5.6f, 3.8f, -3.7f, -1, sidewalkMat, streetRoot);
    }

    // Xây dựng một vỉa hè chi tiết: mặt gạch rãnh dọc, khe ngang, gờ đá khối 3m, vạch mép lề
    static void BuildDetailedSidewalk(string name, float centerZ, float width, float curbZ, int sideSign, Material mat, GameObject parent)
    {
        var sRoot = new GameObject(name);
        sRoot.transform.SetParent(parent.transform);

        // Mặt vỉa hè chính (Lát gạch sáng màu, có texture rãnh dọc)
        var walkBox = CreateBox("Pavement_Surface", 0, 0.12f, centerZ, 220, 0.20f, width, HexColor("#d4dade"), sRoot);
        if (mat != null)
        {
            walkBox.GetComponent<MeshRenderer>().material = mat;
        }

        // Các đường rãnh khe co giãn ngang (Transverse Joint Lines) cắt ngang vỉa hè mỗi 4 mét
        // (Đây chính là đường rãnh ngang đậm nét vắt ngang vỉa hè trong ảnh của bạn)
        for (float x = -100; x <= 100; x += 4.0f)
        {
            CreateBox($"Joint_Line_{x}", x, 0.222f, centerZ, 0.04f, 0.005f, width * 0.98f, HexColor("#84919a"), sRoot);
        }

        // Gờ đá bó vỉa hè (Curb Stones) đúc từng viên khối 3m có khe hở kỹ thuật (như mép lề trong ảnh)
        for (float x = -100; x < 100; x += 3.0f)
        {
            // Viên đá bó vỉa hè màu trắng ngà
            CreateBox($"CurbStone_{x}", x + 1.47f, 0.165f, curbZ, 2.92f, 0.21f, 0.22f, HexColor("#f4f2e5"), sRoot);
            // Khe hở giữa 2 viên đá
            CreateBox($"CurbGap_{x}", x + 2.97f, 0.165f, curbZ, 0.08f, 0.21f, 0.22f, HexColor("#6b767f"), sRoot);
        }

        // Vạch sơn trắng kẻ dọc theo mép mép lòng đường (White Edge Line)
        float edgeLineZ = curbZ - sideSign * 0.22f;
        CreateBox("Road_White_EdgeLine", 0, 0.042f, edgeLineZ, 220, 0.02f, 0.14f, Color.white, sRoot);

        // Rãnh thoát nước lòng lề đường (Gutter Channel) màu xám sẫm chạy dọc sát mép bó vỉa hè
        float gutterZ = curbZ - sideSign * 0.12f;
        CreateBox("Road_Gutter_Channel", 0, 0.038f, gutterZ, 220, 0.02f, 0.16f, HexColor("#2a2e34"), sRoot);
    }

    // Tạo Texture gạch vỉa hè tự động với đường rãnh dọc và hạt bê tông nhám nhẹ chuẩn như ảnh
    static Material GetOrCreateSidewalkMaterial()
    {
        string dir = "Assets/Materials/Textures";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string texPath = $"{dir}/Sidewalk_Tiles.png";
        if (!File.Exists(texPath))
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color baseCol = HexColor("#d4dade");
            Color grooveCol = HexColor("#a0abb3");
            Color shadowCol = HexColor("#bdc6cc");

            Random.InitState(1481);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float noise = (Random.value - 0.5f) * 0.045f;
                    tex.SetPixel(x, y, new Color(baseCol.r + noise, baseCol.g + noise, baseCol.b + noise, 1f));
                }
            }

            // Vẽ các đường rãnh xẻ dọc chạy dài theo vỉa hè (Longitudinal grooves)
            // Chia vỉa hè thành các dải thanh dài chuẩn tỉ lệ như trong ảnh
            for (int k = 1; k <= 7; k++)
            {
                int gx = k * (size / 8);
                for (int y = 0; y < size; y++)
                {
                    tex.SetPixel(gx, y, grooveCol);
                    tex.SetPixel(gx + 1, y, shadowCol);
                }
            }

            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(texPath, bytes);
            AssetDatabase.ImportAsset(texPath);

            var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer != null)
            {
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        var loadedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (loadedTex != null)
        {
            mat.mainTexture = loadedTex;
            mat.mainTextureScale = new Vector2(30f, 1f); // Tiled dọc theo đại lộ
        }
        mat.color = HexColor("#e4e8ec");
        return mat;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. DÃY NHÀ PHỐ RỰC RỠ CÓ BAN CÔNG & LAN CAN (BÊN PHẢI ĐƯỜNG)
    // ─────────────────────────────────────────────────────────────────────────
    static void BuildSaigonShophouses(GameObject root)
    {
        var shophousesRoot = new GameObject("Shophouses_Right");
        shophousesRoot.transform.SetParent(root.transform);

        (float x, float h, string wallColor, string signText, string signColor)[] shopDefs = {
            (21f,  13f, "#26de81", "THE BOOK CORNER", "#a55eea"),
            (2f,   16f, "#15a89b", "BAKERY & TEA",    "#ff9f43"),
            (-15f, 15f, "#11998e", "LAUNDRY 24/7",    "#a855f7"),  // Đúng trong ảnh!
            (-31f, 16f, "#e58e26", "CƠM NHÀ",         "#3867d6"),  // Đúng trong ảnh!
            (-48f, 14f, "#8854d0", "STUDIO & LIVING",  "#20bf6b"),
            (-68f, 17f, "#fa8231", "PHỞ SÀI GÒN",     "#eb3b5a"),
            (-88f, 15f, "#2d98da", "SAIGON COWORK",    "#f7b731")
        };

        foreach (var def in shopDefs)
        {
            CreateShophouseWithBalconies(def.x, 19.5f, 13f, def.h, HexColor(def.wallColor), def.signText, HexColor(def.signColor), -1, shophousesRoot);
        }
    }

    static void CreateShophouseWithBalconies(float x, float z, float w, float h, Color wallColor, string signText, Color signColor, int front, GameObject parent)
    {
        var b = new GameObject($"Shophouse_{signText}");
        b.transform.SetParent(parent.transform);

        CreateBox("BuildingBody", x, h * 0.5f, z, w, h, 10.0f, wallColor, b);

        float frontZ = z + front * 5.05f;
        int floorCount = Mathf.FloorToInt(h / 3.0f);

        CreateBox("ShopWindow_Ground", x, 1.4f, frontZ, w * 0.8f, 2.2f, 0.08f, HexColor("#68b4d4"), b);

        for (int floor = 1; floor < floorCount; floor++)
        {
            float floorY = floor * 3.0f;
            CreateBox($"Balcony_Floor_{floor}", x, floorY + 0.1f, frontZ + front * 0.7f, w * 0.96f, 0.16f, 1.4f, HexColor("#e4e8ec"), b);
            CreateBox($"Handrail_Top_{floor}", x, floorY + 0.95f, frontZ + front * 1.35f, w * 0.94f, 0.04f, 0.04f, HexColor("#222831"), b);

            int barCount = Mathf.FloorToInt(w / 0.6f);
            for (int i = 0; i <= barCount; i++)
            {
                float bx = x - w * 0.45f + i * 0.6f;
                CreateBox($"RailBar_{floor}_{i}", bx, floorY + 0.52f, frontZ + front * 1.35f, 0.035f, 0.82f, 0.035f, HexColor("#222831"), b);
            }

            for (int k = -1; k <= 1; k++)
            {
                CreateBox($"Window_{floor}_{k}", x + k * w * 0.28f, floorY + 1.6f, frontZ, w * 0.22f, 1.8f, 0.06f, HexColor("#79bad4"), b);
            }
        }

        var signBoard = CreateBox("SignBoard", x, 3.2f, frontZ + front * 0.2f, w * 0.88f, 0.85f, 0.12f, signColor, b);
        var textGO = new GameObject("SignText");
        textGO.transform.SetParent(signBoard.transform);
        textGO.transform.localPosition = new Vector3(0, 0, front > 0 ? 0.55f : -0.55f);
        textGO.transform.localRotation = Quaternion.Euler(0, front > 0 ? 0 : 180, 0);
        var tmp = textGO.AddComponent<TextMeshPro>();
        tmp.text = signText;
        tmp.fontSize = 4.2f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        CreateBox("Awning", x, 2.6f, frontZ + front * 0.8f, w * 0.94f, 0.10f, 1.6f, HexColor("#eee8de"), b);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 4. KHU VỰC BÊN TRÁI ĐƯỜNG (TRỌ 08, CHẠM COFFEE, ĐẠI HỌC UEH)
    // ─────────────────────────────────────────────────────────────────────────
    static void BuildCampusSide(GameObject root)
    {
        var campusRoot = new GameObject("Campus_Left");
        campusRoot.transform.SetParent(root.transform);

        CreateShophouseWithBalconies(22f,  -17.0f, 18f, 14f, HexColor("#0984e3"), "UEH · ĐẠI HỌC KINH TẾ", HexColor("#027c8d"), 1, campusRoot);
        CreateShophouseWithBalconies(-0.5f, -16.2f, 13f, 12f, HexColor("#f1c40f"), "CHẠM · COFFEE & STUDY", HexColor("#e67e22"), 1, campusRoot);
        CreateShophouseWithBalconies(-18f, -15.0f, 10f, 10f, HexColor("#00cec9"), "TRỌ 08 · THANH XUÂN", HexColor("#0984e3"), 1, campusRoot);
        CreateShophouseWithBalconies(-34f, -17.0f, 11f, 10f, HexColor("#2ecc71"), "MINI MART · TẠP HÓA", HexColor("#27ae60"), 1, campusRoot);
        CreateShophouseWithBalconies(66f,  -16.0f, 13f, 12f, HexColor("#74b9ff"), "GARAGE 08 · SỬA XE", HexColor("#0984e3"), 1, campusRoot);

        // (Đã xóa toàn bộ ghế ngồi / Bench theo yêu cầu không có nội thất)
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 5. CÂY XANH TÁN TRÒN (8 Khối cầu cụm lại)
    // ─────────────────────────────────────────────────────────────────────────
    static void BuildIconicTrees(GameObject root)
    {
        var treesRoot = new GameObject("Iconic_Trees");
        treesRoot.transform.SetParent(root.transform);

        float[] treeX = { -80, -60, -43, -26, -9, 10, 33, 55, 78 };
        foreach (var x in treeX)
        {
            CreateIconicTree(x, -5.5f, 1.0f, treesRoot);
            CreateIconicTree(x, 9.8f, 1.05f, treesRoot);
        }
    }

    static void CreateIconicTree(float x, float z, float s, GameObject parent)
    {
        var treeGO = new GameObject($"Tree_{x}_{z}");
        treeGO.transform.SetParent(parent.transform);

        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.position = new Vector3(x, 1.8f * s, z);
        trunk.transform.localScale = new Vector3(0.32f * s, 1.8f * s, 0.32f * s);
        trunk.transform.SetParent(treeGO.transform);
        var trunkMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        trunkMat.color = HexColor("#7f7664");
        trunk.GetComponent<MeshRenderer>().material = trunkMat;

        Color[] foliageColors = { HexColor("#1db552"), HexColor("#22b96e"), HexColor("#7dcc39") };
        Vector3[] offsets = {
            new Vector3(0, 4.0f * s, 0),
            new Vector3(0.9f * s, 4.2f * s, 0.4f * s),
            new Vector3(-0.9f * s, 4.1f * s, 0.3f * s),
            new Vector3(0.4f * s, 4.3f * s, -0.8f * s),
            new Vector3(-0.5f * s, 4.0f * s, -0.7f * s),
            new Vector3(0.7f * s, 3.8f * s, 0.6f * s),
            new Vector3(-0.6f * s, 3.9f * s, 0.7f * s),
            new Vector3(0, 4.6f * s, 0)
        };

        for (int i = 0; i < offsets.Length; i++)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = $"LeafCluster_{i}";
            sphere.transform.position = new Vector3(x + offsets[i].x, offsets[i].y, z + offsets[i].z);
            sphere.transform.localScale = new Vector3(1.9f * s, 1.5f * s, 1.8f * s);
            sphere.transform.SetParent(treeGO.transform);

            var leafMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            leafMat.color = foliageColors[i % foliageColors.Length];
            sphere.GetComponent<MeshRenderer>().material = leafMat;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 6. CỘT ĐÈN ĐƯỜNG CHỮ L
    // ─────────────────────────────────────────────────────────────────────────
    static void BuildStreetLamps(GameObject root)
    {
        var lampsRoot = new GameObject("StreetLamps");
        lampsRoot.transform.SetParent(root.transform);

        float[] lampX = { -76, -52, -28, -5, 17, 38, 60, 83 };
        foreach (var x in lampX)
        {
            var lampGO = new GameObject($"Lamp_{x}");
            lampGO.transform.SetParent(lampsRoot.transform);

            CreateBox("Pole", x, 2.8f, 7.8f, 0.11f, 5.6f, 0.11f, HexColor("#2d3748"), lampGO);
            CreateBox("BracketArm", x, 5.5f, 7.3f, 0.09f, 0.09f, 0.9f, HexColor("#2d3748"), lampGO);
            var lampHead = CreateBox("LampHead", x, 5.45f, 6.8f, 0.35f, 0.08f, 0.6f, HexColor("#ffffff"), lampGO);

            var ptLight = lampHead.AddComponent<Light>();
            ptLight.type = LightType.Point;
            ptLight.color = HexColor("#fff1c5");
            ptLight.range = 14f;
            ptLight.intensity = 1.3f;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 7. QUẦY HÀNG RONG VEN ĐƯỜNG (XE BÁNH MÌ GỐC TRONG ẢNH)
    // ─────────────────────────────────────────────────────────────────────────
    static void BuildStreetVendors(GameObject root)
    {
        var vendorRoot = new GameObject("StreetVendors");
        vendorRoot.transform.SetParent(root.transform);

        CreateBox("Cart_Body", -30, 0.55f, 9.8f, 2.5f, 0.9f, 1.2f, HexColor("#f3f6f8"), vendorRoot);
        CreateBox("Canopy_Orange", -30, 2.3f, 9.8f, 3.2f, 0.10f, 2.2f, HexColor("#e67e22"), vendorRoot);
        CreateBox("VendorSign", -30, 1.15f, 9.15f, 2.3f, 0.45f, 0.08f, HexColor("#e67e22"), vendorRoot);

        // (Đã xóa toàn bộ nội thất: giường, bàn học, laptop theo đúng yêu cầu)
    }

    static GameObject SpawnGLB(string path, string name, Vector3 pos, Quaternion rot, GameObject parent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        GameObject inst = null;
        if (prefab != null)
        {
            inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.name = name;
        }
        else
        {
            inst = GameObject.CreatePrimitive(PrimitiveType.Cube);
            inst.name = $"{name}_Placeholder";
        }

        inst.transform.position = pos;
        inst.transform.rotation = rot;
        if (parent != null) inst.transform.SetParent(parent.transform);
        return inst;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 8. PLAYER & CAMERA GÓC NHÌN THỨ NHẤT
    // ─────────────────────────────────────────────────────────────────────────
    static GameObject CreateFirstPersonPlayer()
    {
        var playerGO = new GameObject("Player");
        // Đứng ngay trước vạch sang đường nhìn dọc theo vỉa hè & đại lộ
        playerGO.transform.position = new Vector3(18.5f, 0.15f, 2.8f);
        playerGO.transform.rotation = Quaternion.Euler(0, -90f, 0);

        var cc = playerGO.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.center = new Vector3(0, 0.9f, 0);
        cc.radius = 0.35f;

        var pc = playerGO.AddComponent<PlayerController>();
        pc.walkSpeed = 4.8f;
        pc.runSpeed = 7.8f;
        pc.isFirstPerson = true;

        return playerGO;
    }

    static void CreateFirstPersonCamera(GameObject player)
    {
        var existingCam = Camera.main;
        GameObject camGO;
        if (existingCam != null)
            camGO = existingCam.gameObject;
        else
        {
            camGO = new GameObject("Main Camera");
            camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            camGO.tag = "MainCamera";
        }

        var cam = camGO.GetComponent<Camera>();
        cam.fieldOfView = 68f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 320f;

        camGO.transform.position = player.transform.position + Vector3.up * 1.68f;
        camGO.transform.rotation = Quaternion.Euler(0, -90f, 0);

        var pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.playerCamera = camGO.transform;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 9. HUD TỐI GIẢN
    // ─────────────────────────────────────────────────────────────────────────
    static void CreateMinimalHUD()
    {
        var canvasGO = new GameObject("Canvas_HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }

        var hud = canvasGO.AddComponent<HUDController>();

        // Chấm tròn trắng hồng tâm chính giữa màn hình (Center Dot)
        CreateUIPanel("CenterDot", canvasGO.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(6, 6), Color.white);

        // Thanh chỉ số góc trên bên phải (giống góc trên bên phải trong ảnh mới của bạn!)
        var statsRoot = CreateUIPanel("StatsPanel", canvasGO.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-220, -32), new Vector2(420, 52), HexColor("#0d1737ee"));
        hud.moneyText = CreateUIText("Money", statsRoot.transform, new Vector2(0.12f, 0.5f), "₫ 1.98M", 16, TextAlignmentOptions.Left, HexColor("#ff9a24"));
        hud.healthText = CreateUIText("Health", statsRoot.transform, new Vector2(0.35f, 0.5f), "♥ 88/100", 16, TextAlignmentOptions.Left, HexColor("#ff6e9e"));
        hud.energyText = CreateUIText("Energy", statsRoot.transform, new Vector2(0.60f, 0.5f), "ϟ 96/100", 16, TextAlignmentOptions.Left, HexColor("#35efff"));
        hud.studyText = CreateUIText("Study", statsRoot.transform, new Vector2(0.85f, 0.5f), "▤ 45/100", 16, TextAlignmentOptions.Left, HexColor("#d8ff42"));

        // Prompt tương tác
        var promptPanel = CreateUIPanel("InteractPrompt", canvasGO.transform, new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), Vector2.zero, new Vector2(380, 60), HexColor("#112145ee"));
        hud.interactPrompt = promptPanel;
        hud.interactLabel = CreateUIText("Label", promptPanel.transform, new Vector2(0.5f, 0.65f), "[E] Tương tác", 18, TextAlignmentOptions.Center, HexColor("#35efff"));
        hud.interactSub = CreateUIText("Sub", promptPanel.transform, new Vector2(0.5f, 0.30f), "Nhấn E để hành động", 14, TextAlignmentOptions.Center, Color.white);
        promptPanel.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────────
    static GameObject CreateBox(string name, float x, float y, float z, float w, float h, float d, Color c, GameObject parent)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = new Vector3(x, y, z);
        go.transform.localScale = new Vector3(w, h, d);
        if (parent != null) go.transform.SetParent(parent.transform);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.color = c;
        go.GetComponent<MeshRenderer>().material = mat;
        return go;
    }

    static GameObject CreateUIPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        return go;
    }

    static TextMeshProUGUI CreateUIText(string name, Transform parent, Vector2 anchor, string content, float fontSize, TextAlignmentOptions align, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(380, 30);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color;
        return tmp;
    }

    static Color HexColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
        return Color.white;
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var originalScenes = EditorBuildSettings.scenes;
        foreach (var s in originalScenes)
            if (s.path == scenePath) return;

        var newScenes = new EditorBuildSettingsScene[originalScenes.Length + 1];
        System.Array.Copy(originalScenes, newScenes, originalScenes.Length);
        newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }
}
#endif
