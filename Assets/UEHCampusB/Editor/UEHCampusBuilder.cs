#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UEH.CampusB.Editor
{
    /// <summary>Builds persistent, editable architecture in a separate additive scene.</summary>
    public static partial class UEHCampusBuilder
    {
        public const string Root = "Assets/UEHCampusB";
        public const string ScenePath = Root + "/Scenes/UEH_CampusB_Study.unity";
        // Half of the 1.12m capital depth: the entrance-side face meets the glazing plane at Z = 0.
        public const float EntranceColumnCenterZ = -0.56f;
        // Body outer face meets the inner face of the 0.30m side wall.
        public static float EntranceCornerColumnX(float frontage) => frontage / 2 - 0.30f - 0.45f;
        private static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        private static readonly Dictionary<PrimitiveType, Mesh> primitives = new Dictionary<PrimitiveType, Mesh>();
        private static Font signFont;
        private static UEHDimensions dimensions;
        private static int meshCounter;
        private static Transform exterior, lobby;

        [MenuItem("UEH/Campus B/1. Open walkthrough scene")]
        public static void OpenScene()
        {
            if (!File.Exists(ScenePath)) { Build(); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var existing = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SceneManager.SetActiveScene(existing);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("UEH: Scene tham quan độc lập đã mở. Nhấn Play để đi bộ.");
        }

        [MenuItem("UEH/Campus B/2. Rebuild study scene from estimated dimensions")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before building UEH.");
            var previous = SceneManager.GetSceneByPath(ScenePath);
            if (previous.IsValid() && previous.isLoaded)
            {
                if (previous.isDirty && !Application.isBatchMode)
                    if (!EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] { previous })) return;
                EditorSceneManager.CloseScene(previous, true);
            }
            foreach (string sub in new[] { "Scenes", "Prefabs", "Meshes", "Materials", "Textures", "Lighting", "Fonts", "Settings" })
                Directory.CreateDirectory(Root + "/" + sub);
            AssetDatabase.Refresh();
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null)
            {
                dimensions = ScriptableObject.CreateInstance<UEHDimensions>();
                AssetDatabase.CreateAsset(dimensions, Root + "/Settings/EstimatedDimensions.asset");
            }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            if (signFont == null) throw new FileNotFoundException("Missing UEH sign font. See Assets/UEHCampusB/README.vi.md.");
            meshCounter = 0;
            materials.Clear();
            CreatePalette();
            bool hasUntitledScene = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
                hasUntitledScene |= string.IsNullOrEmpty(SceneManager.GetSceneAt(i).path);
            if (hasUntitledScene && !Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var mode = Application.isBatchMode || hasUntitledScene ? NewSceneMode.Single : NewSceneMode.Additive;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, mode);
            SceneManager.SetActiveScene(scene);
            SetupLighting();

            exterior = new GameObject("UEH_Exterior [estimated 36m frontage]").transform;
            lobby = new GameObject("UEH_LobbyB1 [estimated dimensions]").transform;
            BuildExterior();
            BuildLobby();
            PlaceEntranceSideZones(lobby, dimensions);
            SavePrefab(exterior.gameObject, "UEH_Exterior");
            SavePrefab(lobby.gameObject, "UEH_LobbyB1");

            var campus = new GameObject("UEH_CampusB [1 unit = 1 metre]");
            exterior.SetParent(campus.transform, false);
            lobby.SetParent(campus.transform, false);
            SavePrefab(campus, "UEH_CampusB");
            UnityEngine.Object.DestroyImmediate(campus);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/UEH_CampusB.prefab"), scene);
            SetupWalkthrough();
            SetupViewpoints();
            SetupReflectionProbes();
            if (dimensions.expandedLobby) ConfigureExpandedScene();
            Physics.SyncTransforms();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.LookAt(new Vector3(0, 15, -6), Quaternion.Euler(15, 205, 0), 75);
            Selection.activeGameObject = instance;
            Debug.Log("UEH_BUILD_COMPLETE " + ScenePath);
        }

        /// <summary>Moves the two existing displays without regenerating the architecture or materials.</summary>
        [MenuItem("UEH/Campus B/4. Place 1976 left and WELCOME right from gate")]
        public static void ApplyRequestedLayout()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before changing the UEH layout.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null) throw new FileNotFoundException("Missing UEH estimated dimensions.");
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                PlaceEntranceSideZones(contents.transform, dimensions);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var oldViews = GameObject.Find("Review viewpoints [select and align view]");
            if (oldViews != null) UnityEngine.Object.DestroyImmediate(oldViews);
            SetupViewpoints();
            if (dimensions.expandedLobby) ConfigureExpandedScene();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_LAYOUT_UPDATED 1976=left side wall; WELCOME=right side wall, viewed from the gate.");
        }

        private static void PlaceEntranceSideZones(Transform lobbyRoot, UEHDimensions settings)
        {
            // Gate is at +Z and the visitor looks toward -Z: screen-left is +X, screen-right is -X.
            PlaceSideZone(lobbyRoot, "05_WELCOME_RIGHT_from_gate",
                new Vector3(-settings.frontage / 2 + 0.6f, 0, -7.5f), 90,
                new Vector3(-settings.frontage * 0.30f, 0, -settings.lobbyDepth + 0.4f),
                new[] { "05_Reception_and_welcome_wall" });
            PlaceSideZone(lobbyRoot, "06_1976_LEFT_from_gate",
                new Vector3(settings.frontage / 2 - 0.6f, 0, -7.5f), -90,
                new Vector3(settings.frontage * 0.30f, 0, -settings.lobbyDepth + 0.6f),
                new[] { "06_Heritage_1976_and_gallery" });
        }

        private static void PlaceSideZone(Transform lobbyRoot, string name, Vector3 position, float heading, Vector3 sourceCenter, string[] childNames)
        {
            var zone = lobbyRoot.Find(name);
            if (zone == null)
            {
                zone = new GameObject(name).transform;
                zone.SetParent(lobbyRoot, false);
                foreach (string childName in childNames)
                {
                    var child = lobbyRoot.Find(childName);
                    if (child == null) throw new InvalidOperationException("Missing UEH display element: " + childName);
                    child.SetParent(zone, false);
                    child.localPosition -= sourceCenter;
                }
            }
            // Assign absolute transforms, so applying the correction again never mirrors or rotates twice.
            zone.localPosition = position;
            zone.localRotation = Quaternion.Euler(0, heading, 0);
            zone.localScale = Vector3.one;
        }

        [MenuItem("UEH/Campus B/5. Place front columns at glazing and outer corners")]
        public static void MoveFrontColumnsToGate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before moving the entrance columns.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var settings = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (settings == null) throw new FileNotFoundException("Missing UEH estimated dimensions.");
            if (settings.expandedLobby)
            {
                Debug.Log("UEH: Expanded lobby column layout is active. Use menu 8 to apply that layout.");
                return;
            }
            float cornerX = EntranceCornerColumnX(settings.frontage);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var group = contents.transform.Find("02_Stone_columns");
                if (group == null) throw new InvalidOperationException("Missing lobby column group.");
                var plans = new List<KeyValuePair<Mesh, Vector3[]>>();
                foreach (var filter in group.GetComponentsInChildren<MeshFilter>(true))
                {
                    var mesh = filter.sharedMesh;
                    var vertices = mesh.vertices;
                    if (vertices.Length != 12 * 24 && vertices.Length != 11 * 24)
                        throw new InvalidOperationException("Unexpected column mesh layout; expected 11 or 12 box modules: " + mesh.name);
                    float frontZ = float.NegativeInfinity;
                    for (int start = 0; start < vertices.Length; start += 24)
                    {
                        float center = 0;
                        for (int i = 0; i < 24; i++) center += vertices[start + i].z / 24;
                        frontZ = Mathf.Max(frontZ, center);
                    }
                    int moved = 0, corners = 0;
                    for (int start = 0; start < vertices.Length; start += 24)
                    {
                        float center = 0;
                        for (int i = 0; i < 24; i++) center += vertices[start + i].z / 24;
                        if (Mathf.Abs(center - frontZ) > 0.02f) continue;
                        float centerX = 0;
                        for (int i = 0; i < 24; i++) centerX += vertices[start + i].x / 24;
                        float shiftX = Mathf.Abs(centerX) > settings.frontage * 0.25f
                            ? Mathf.Sign(centerX) * cornerX - centerX : 0;
                        if (Mathf.Abs(centerX) > settings.frontage * 0.25f) corners++;
                        for (int i = 0; i < 24; i++)
                        {
                            vertices[start + i].z += EntranceColumnCenterZ - center;
                            vertices[start + i].x += shiftX;
                        }
                        moved++;
                    }
                    if (moved != 4 || corners != 2) throw new InvalidOperationException("Expected four entrance modules including two outer corners in " + mesh.name);
                    plans.Add(new KeyValuePair<Mesh, Vector3[]>(mesh, vertices));
                }
                if (plans.Count != 3) throw new InvalidOperationException("Expected body, base and capital meshes.");
                var colliders = group.GetComponentsInChildren<BoxCollider>(true);
                float frontColliderZ = float.NegativeInfinity;
                foreach (var collider in colliders) frontColliderZ = Mathf.Max(frontColliderZ, collider.transform.localPosition.z);
                var frontColliders = new List<BoxCollider>();
                foreach (var collider in colliders)
                    if (Mathf.Abs(collider.transform.localPosition.z - frontColliderZ) < 0.02f) frontColliders.Add(collider);
                if (frontColliders.Count != 4) throw new InvalidOperationException("Expected four entrance column colliders.");
                int cornerColliders = 0;
                foreach (var collider in frontColliders)
                    if (Mathf.Abs(collider.transform.localPosition.x) > settings.frontage * 0.25f) cornerColliders++;
                if (cornerColliders != 2) throw new InvalidOperationException("Expected two outer entrance column colliders.");

                // Apply only after every body/base/capital and collider has passed the preflight.
                foreach (var plan in plans)
                {
                    plan.Key.vertices = plan.Value;
                    plan.Key.RecalculateBounds();
                    plan.Key.UploadMeshData(false);
                    EditorUtility.SetDirty(plan.Key);
                }
                foreach (var collider in frontColliders)
                {
                    var position = collider.transform.localPosition;
                    position.z = EntranceColumnCenterZ;
                    if (Mathf.Abs(position.x) > settings.frontage * 0.25f)
                        position.x = Mathf.Sign(position.x) * cornerX;
                    collider.transform.localPosition = position;
                }
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var views = GameObject.Find("Review viewpoints [select and align view]");
            if (views != null && views.transform.Find("07_Entrance_columns_from_inside") == null)
                Viewpoint(views.transform, "07_Entrance_columns_from_inside", new Vector3(0, 1.65f, -13), new Vector3(0, 1.9f, 0));
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_COLUMNS_UPDATED Four front columns at Z=" + EntranceColumnCenterZ + "; outer pair at X=+/-" + cornerX + "; bodies, bases, capitals and colliders aligned.");
        }

        private static void SavePrefab(GameObject go, string name)
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, Root + "/Prefabs/" + name + ".prefab", InteractionMode.AutomatedAction);
        }

        [MenuItem("UEH/Campus B/6. Refine 1976 wall and three ordered sections")]
        public static void RefineHeritageWall()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before editing the heritage wall.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                materials[material.name] = material;
            }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            if (signFont == null) throw new FileNotFoundException("Missing UEH sign font.");
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var zone = contents.transform.Find("06_1976_LEFT_from_gate");
                if (zone == null) throw new InvalidOperationException("Missing left heritage zone.");
                foreach (string name in new[] { "06_Heritage_1976_and_gallery", "Since_1976", "Since", "Gallery_caption" })
                {
                    var old = zone.Find(name);
                    if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                }
                var group = new GameObject("06_Heritage_1976_and_gallery").transform;
                group.SetParent(zone, false);
                PopulateHeritage(group);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var views = GameObject.Find("Review viewpoints [select and align view]");
            if (views != null)
            {
                var point = views.transform.Find("08_Heritage_front");
                if (point != null) UnityEngine.Object.DestroyImmediate(point.gameObject);
                Viewpoint(views.transform, "08_Heritage_front", new Vector3(7, 1.8f, -8), new Vector3(17.4f, 2.1f, -8));
            }
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_HERITAGE_UPDATED Viewer left-to-right: blank wall, SINCE 1976 greenery, nine colour swatches, five varied-size circles.");
        }

        private static void CreatePalette()
        {
            MakeTexture("Marble", 512, 0);
            MakeTexture("Stone", 512, 1);
            MakeTexture("Pavement", 512, 2);
            MakeTexture("Wood", 512, 3);
            Mat("Ivory", "E9E6DE", 0.22f);
            Mat("White", "F6F2E9", 0.3f);
            Mat("Stone", "D2BDA3", 0.36f, 0, "Stone");
            Mat("Floor", "F0E4D0", 0.78f, 0, "Marble").mainTextureScale = Vector2.one * 1.25f;
            Mat("Border", "A38B6B", 0.55f, 0, "Stone");
            Mat("Pavement", "B9BCB7", 0.16f, 0, "Pavement");
            Mat("Asphalt", "434C50", 0.06f);
            Mat("Metal", "455458", 0.5f, 0.65f);
            Mat("Silver", "A6ADB0", 0.65f, 0.78f);
            Mat("Gold", "BD9752", 0.6f, 0.68f);
            Mat("Wood", "8E522D", 0.3f, 0, "Wood");
            Mat("Teal", "008D9B", 0.4f, 0.1f);
            Mat("BlueGlass", "6F9BA9", 0.78f, 0.18f);
            Mat("Dark", "26373B", 0.25f);
            Mat("Leaf", "3C6B37", 0.12f);
            Mat("LeafLight", "628B41", 0.1f);
            Mat("Flower", "CB4280", 0.25f);
            Mat("Orange", "EF7C24", 0.28f);
            Mat("CreamSeat", "C6C4B9", 0.25f);
            Mat("Red", "B9382E", 0.22f);
            var glass = Mat("ClearGlass", "BED6D5", 0.88f, 0.04f);
            glass.color = new Color(0.73f, 0.86f, 0.85f, 0.10f);
            glass.SetFloat("_Mode", 3);
            glass.SetInt("_SrcBlend", (int)BlendMode.One);
            glass.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            glass.SetInt("_ZWrite", 0);
            glass.DisableKeyword("_ALPHATEST_ON");
            glass.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            glass.renderQueue = 3000;
            Emissive("WarmLight", new Color(1, 0.83f, 0.52f), 2.0f);
            Emissive("CoolLight", new Color(0.25f, 0.73f, 1), 2.0f);
            var worldText = Mat("WorldText", "FFFFFF", 0);
            var textShader = Shader.Find("UEH/World Text");
            if (textShader == null) throw new InvalidOperationException("Missing depth-tested world text shader.");
            worldText.shader = textShader;
        }

        private static Material Mat(string name, string hex, float smoothness, float metallic = 0, string texture = null)
        {
            var path = Root + "/Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(mat, path); }
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            mat.color = color;
            mat.SetFloat("_Glossiness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            mat.enableInstancing = true;
            if (texture != null) mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Textures/" + texture + ".png");
            materials[name] = mat;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void Emissive(string name, Color color, float intensity)
        {
            var m = Mat(name, ColorUtility.ToHtmlStringRGB(color), 0.45f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", color * intensity);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
        }

        private static void MakeTexture(string name, int size, int type)
        {
            string path = Root + "/Textures/" + name + ".png";
            if (File.Exists(path)) return;
            var texture = new Texture2D(size, size, TextureFormat.RGB24, true);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size, v = y / (float)size;
                    float n = Mathf.PerlinNoise(u * 13 + 7, v * 13 + 11);
                    float tone;
                    if (type == 0)
                    {
                        float vein = Mathf.Pow(Mathf.Abs(Mathf.Sin(u * 15 + v * 9 + n * 8)), 18);
                        tone = 0.96f - vein * 0.055f + n * 0.025f;
                        if (x < 2 || y < 2) tone = 0.72f;
                    }
                    else if (type == 1) tone = 0.80f + n * 0.15f + Mathf.PerlinNoise(u * 85, v * 85) * 0.04f;
                    else if (type == 2)
                    {
                        float dx = Mathf.Abs(u - 0.5f), dy = Mathf.Abs(v - 0.5f);
                        float arc = Mathf.Repeat(Mathf.Sqrt(dx * dx + dy * dy) * 8, 1);
                        tone = 0.82f + n * 0.08f;
                        if (arc < 0.055f || x < 3 || y < 3) tone *= 0.72f;
                    }
                    else tone = 0.65f + Mathf.PerlinNoise(u * 5, v * 110) * 0.27f;
                    pixels[y * size + x] = new Color(tone, tone, tone);
                }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.maxTextureSize = 1024;
            importer.anisoLevel = 8;
            importer.SaveAndReimport();
        }

        private static void BuildExterior()
        {
            float w = dimensions.frontage, d = dimensions.exteriorDepth, h = dimensions.lobbyHeight;
            var ground = new Part("01_Forecourt_and_pavement", exterior);
            ground.Box("Pavement", new Vector3(0, -0.15f, 8), new Vector3(62, 0.3f, 17), true);
            ground.Box("Asphalt", new Vector3(0, -0.23f, 23), new Vector3(75, 0.16f, 13), true);
            ground.Box("Ivory", new Vector3(0, -0.1f, 16.55f), new Vector3(62, 0.25f, 0.22f));
            for (int i = -6; i <= 6; i++)
                ground.Box("White", new Vector3(i * 5, -0.139f, 23), new Vector3(2.7f, 0.012f, 0.13f));
            for (int i = 0; i < 7; i++)
                ground.Box("White", new Vector3(0, -0.139f, 17.8f + i * 0.95f), new Vector3(4.6f, 0.012f, 0.45f));
            ground.Finish();

            var podium = new Part("02_Podium_structure", exterior);
            for (int floor = 0; floor < 3; floor++)
            {
                float y = h + floor * 3.9f;
                podium.Box("Ivory", new Vector3(0, y + 0.2f, -d / 2), new Vector3(w + 0.8f, 0.4f, d + 1));
                podium.Box("White", new Vector3(0, y + 0.02f, 1.0f), new Vector3(w + 1.2f, 0.16f, 2.4f));
                if (floor < 2)
                {
                    podium.Box("Ivory", new Vector3(0, y + 1, -d + 0.2f), new Vector3(w, 1.9f, 0.35f));
                    for (int i = -3; i <= 3; i++)
                    {
                        podium.Box("Ivory", new Vector3(i * w / 7, y + 1.9f, -1.6f), new Vector3(0.48f, 3.6f, 0.6f));
                        podium.Box("BlueGlass", new Vector3(i * w / 7, y + 1.9f, -2.0f), new Vector3(w / 7 - 0.5f, 2.8f, 0.09f));
                    }
                }
                if (floor == 0 && dimensions.rightLiftsAndServices)
                    foreach (var fragment in ServiceExteriorWallFragments(w, d, h)) podium.Box("Ivory", fragment.Key, fragment.Value);
                else podium.Box("Ivory", new Vector3(-w / 2, y - 1.9f, -d / 2), new Vector3(0.42f, 3.7f, d));
                if (floor == 0 && dimensions.leftOfficeAndLounge)
                    foreach (var fragment in OfficeExteriorWallFragments(w, d, h)) podium.Box("Ivory", fragment.Key, fragment.Value);
                else podium.Box("Ivory", new Vector3(w / 2, y - 1.9f, -d / 2), new Vector3(0.42f, 3.7f, d));
            }
            podium.Finish();

            for (int level = 0; level < 3; level++)
            {
                var balcony = new Part("03_Balcony_" + (level + 1), exterior);
                float floorY = h + level * 3.9f;
                balcony.Box("Silver", new Vector3(0, floorY + 1.12f, 2.1f), new Vector3(w - 0.6f, 0.055f, 0.065f));
                balcony.Box("Ivory", new Vector3(0, floorY + 0.25f, 2.1f), new Vector3(w, 0.3f, 0.25f));
                for (float x = -w / 2 + 0.5f; x < w / 2; x += 0.24f)
                    balcony.Box("Silver", new Vector3(x, floorY + 0.69f, 2.1f), new Vector3(0.025f, 0.82f, 0.025f));
                if (level == 0)
                {
                    for (int bay = 0; bay < 6; bay++)
                    {
                        float x = -w / 2 + 3 + bay * (w - 6) / 5;
                        balcony.Box("Stone", new Vector3(x, floorY + 0.5f, 1.7f), new Vector3(1.5f, 0.38f, 0.5f));
                        for (int i = 0; i < 4; i++)
                        {
                            balcony.Sphere("Leaf", new Vector3(x - 0.5f + i * 0.33f, floorY + 0.8f, 1.7f), new Vector3(0.5f, 0.55f, 0.6f));
                            balcony.Sphere("Flower", new Vector3(x - 0.5f + i * 0.33f, floorY + 0.93f, 1.98f), Vector3.one * 0.27f);
                        }
                    }
                }
                balcony.Finish();
            }

            var screen = new Part("04_Horizontal_louvres", exterior);
            for (int bay = 0; bay < 6; bay++)
            {
                float x = -w / 2 + w / 12 + bay * w / 6;
                if (bay == 2 || bay == 3) continue;
                for (int row = 0; row < 14; row++)
                    screen.Box("Silver", new Vector3(x, h + 0.8f + row * 0.18f, 1.97f), new Vector3(w / 6 - 0.42f, 0.07f, 0.2f));
                screen.Box("Orange", new Vector3(x, h + 3.36f, 2.0f), new Vector3(w / 6 - 0.35f, 0.12f, 0.22f));
            }
            screen.Finish();

            var identity = new Part("05_University_identity", exterior);
            identity.Box("Ivory", new Vector3(0, h + 5.5f, 2.16f), new Vector3(w - 0.8f, 1.5f, 0.22f));
            identity.Finish();
            Text(exterior, "University_name", "ĐẠI HỌC KINH TẾ TP. HỒ CHÍ MINH", new Vector3(0, h + 5.5f, 2.31f), w - 2.0f, 0.95f, "Teal");
            Text(exterior, "Campus_address", "UEH  ·  279 NGUYỄN TRI PHƯƠNG", new Vector3(0, 4.12f, 0.42f), 12, 0.38f, "Teal");

            float towerBase = h + 8.2f, tw = w - 3.5f, td = d - 5;
            float floorHeight = (dimensions.towerHeight - towerBase) / dimensions.upperFloors;
            var core = new Part("06_Tower_shell_estimated", exterior);
            core.Box("BlueGlass", new Vector3(0, (dimensions.towerHeight + towerBase) / 2, -d / 2 - 1.0f), new Vector3(tw, dimensions.towerHeight - towerBase, td));
            core.Box("Ivory", new Vector3(0, dimensions.towerHeight + 0.22f, -d / 2 - 1.0f), new Vector3(tw + 0.5f, 0.44f, td + 0.5f));
            core.Finish();
            for (int floor = 0; floor <= dimensions.upperFloors; floor++)
            {
                var frame = new Part("07_Tower_floor_" + floor.ToString("00"), exterior);
                float y = towerBase + floor * floorHeight;
                float z = -d / 2 - 1.0f;
                frame.Box("Ivory", new Vector3(0, y, z), new Vector3(tw + 0.4f, 0.32f, td + 0.4f));
                if (floor < dimensions.upperFloors)
                {
                    for (int bay = 0; bay <= 8; bay++)
                    {
                        float x = -tw / 2 + bay * tw / 8;
                        frame.Box(bay % 2 == 0 ? "Ivory" : "Silver", new Vector3(x, y + floorHeight / 2, z + td / 2 + 0.055f), new Vector3(bay % 2 == 0 ? 1.1f : 0.09f, floorHeight, 0.15f));
                    }
                    frame.Box("Silver", new Vector3(0, y + floorHeight * 0.62f, z + td / 2 + 0.07f), new Vector3(tw, 0.06f, 0.12f));
                    for (int bay = 0; bay <= 4; bay++)
                        foreach (int side in new[] { -1, 1 })
                            frame.Box("Ivory", new Vector3(side * (tw / 2 + 0.05f), y + floorHeight / 2, z - td / 2 + bay * td / 4), new Vector3(0.15f, floorHeight, 0.35f));
                }
                frame.Finish();
            }
            BuildGateAndLandscape(w);
        }

        private static void BuildGateAndLandscape(float w)
        {
            var gate = new Part("08_Retracted_gate_and_planters", exterior);
            foreach (int side in new[] { -1, 1 })
            {
                gate.Box("Stone", new Vector3(side * 4.5f, 0.6f, 8.2f), new Vector3(1.3f, 1.2f, 1.3f), true);
                gate.Box("Dark", new Vector3(side * 4.5f, 1.22f, 8.2f), new Vector3(1.4f, 0.08f, 1.4f));
                gate.Box("Stone", new Vector3(side * 13, 0.45f, 6.5f), new Vector3(11, 0.9f, 1.3f), true);
                for (int i = 0; i < 14; i++)
                    gate.Sphere(i % 3 == 0 ? "LeafLight" : "Leaf", new Vector3(side * (8.0f + i * 0.75f), 1.0f, 6.5f), new Vector3(1.1f, 0.65f, 1.0f));
                for (int i = 0; i < 8; i++)
                    gate.Box("Silver", new Vector3(side * (5.35f + i * 0.13f), 0.98f, 8.2f), new Vector3(0.05f, 1.9f, 0.6f));
            }
            gate.Finish();
            Tree(exterior, "09_Tree_left", new Vector3(-12, 0, 9.4f), 6.5f);
            Tree(exterior, "09_Tree_right", new Vector3(12.5f, 0, 10), 7.5f);
            Tree(exterior, "09_Tree_edge_left", new Vector3(-24, 0, 11), 8);
            Tree(exterior, "09_Tree_edge_right", new Vector3(24, 0, 10), 8);
            var limits = new Part("10_Site_boundaries", exterior);
            foreach (int side in new[] { -1, 1 })
                limits.Box("Stone", new Vector3(side * 30, 0.45f, -6), new Vector3(0.6f, 0.9f, 43), true);
            limits.Finish();
        }

        private static void BuildLobby()
        {
            if (dimensions.expandedLobby)
            {
                BuildExpandedCore(lobby);
                BuildReception(dimensions.frontage, dimensions.lobbyDepth, dimensions.lobbyHeight);
                BuildHeritage(dimensions.frontage, dimensions.lobbyDepth, dimensions.lobbyHeight);
                BuildInnerHall(dimensions.lobbyDepth, dimensions.lobbyHeight);
                foreach (float x in new[] { -dimensions.frontage * 0.375f, dimensions.frontage * 0.375f })
                {
                    Plant(lobby, "08_Indoor_plant_" + x, new Vector3(x - 1.1f, 0, -12));
                    Plant(lobby, "08_Entrance_plant_" + x, new Vector3(x, 0, -1.2f));
                }
                BuildExpandedEntrance(lobby);
                return;
            }
            float w = dimensions.frontage, d = dimensions.lobbyDepth, h = dimensions.lobbyHeight, entry = dimensions.entryWidth;
            var shell = new Part("01_Floor_and_walls", lobby);
            shell.Box("Floor", new Vector3(0, -0.14f, -d / 2), new Vector3(w, 0.28f, d), true);
            shell.Box("White", new Vector3(0, h - 0.1f, -d / 2), new Vector3(w - 0.5f, 0.2f, d - 0.1f), true);
            foreach (int side in new[] { -1, 1 })
            {
                shell.Box("Ivory", new Vector3(side * (w / 2 - 0.15f), h / 2, -d / 2), new Vector3(0.3f, h, d), true);
                shell.Box("Ivory", new Vector3(side * (w / 4 + 1.5f), h / 2, -d), new Vector3(w / 2 - 3, h, 0.3f), true);
                shell.Box("Border", new Vector3(side * (w / 2 - 1.6f), 0.008f, -d / 2), new Vector3(0.16f, 0.014f, d - 1.8f));
            }
            shell.Box("Border", new Vector3(0, 0.008f, -1), new Vector3(w - 3.2f, 0.014f, 0.16f));
            shell.Box("Border", new Vector3(0, 0.008f, -d + 1), new Vector3(w - 3.2f, 0.014f, 0.16f));
            shell.Finish();

            var columns = new Part("02_Stone_columns", lobby);
            foreach (float x in new[] { -w * 0.375f, -w * 0.125f, w * 0.125f, w * 0.375f })
                foreach (float z in new[] { EntranceColumnCenterZ, -d * 0.5f, -d + 2 })
                {
                    if (Mathf.Abs(x + w * 0.375f) < 0.02f && Mathf.Abs(z + d * 0.5f) < 0.02f) continue;
                    float columnX = z == EntranceColumnCenterZ && Mathf.Abs(x) > w * 0.25f
                        ? Mathf.Sign(x) * EntranceCornerColumnX(w) : x;
                    columns.Box("Stone", new Vector3(columnX, h / 2, z), new Vector3(0.9f, h, 0.9f), true);
                    columns.Box("Border", new Vector3(columnX, 0.12f, z), new Vector3(0.98f, 0.24f, 0.98f));
                    columns.Box("Ivory", new Vector3(columnX, h - 0.13f, z), new Vector3(1.12f, 0.24f, 1.12f));
                }
            columns.Finish();

            var entrance = new Part("03_Glazing_with_open_doorway", lobby);
            float bayWidth = (w - entry) / 6;
            foreach (int side in new[] { -1, 1 })
                for (int bay = 0; bay < 3; bay++)
                {
                    float x = side * (entry / 2 + bayWidth * (bay + 0.5f));
                    entrance.Box("ClearGlass", new Vector3(x, 2.0f, 0), new Vector3(bayWidth - 0.09f, 3.8f, 0.025f), true);
                    entrance.Box("Metal", new Vector3(side * (entry / 2 + bayWidth * bay), 2.05f, 0), new Vector3(0.065f, 4.1f, 0.12f), true);
                    entrance.Box("Metal", new Vector3(x, 3.25f, 0), new Vector3(bayWidth, 0.055f, 0.12f));
                    entrance.Box("Metal", new Vector3(x, 0.15f, 0), new Vector3(bayWidth, 0.08f, 0.12f));
                }
            entrance.Box("Metal", new Vector3(0, 3.3f, 0), new Vector3(entry, 0.13f, 0.25f));
            entrance.Box("ClearGlass", new Vector3(0, 3.86f, 0), new Vector3(entry, 0.95f, 0.025f));
            // Glass leaves parked beside the clear 5.6m opening. No collider spans the doorway.
            foreach (int side in new[] { -1, 1 })
            {
                entrance.Box("ClearGlass", new Vector3(side * (entry / 2 + 0.65f), 1.65f, -0.12f), new Vector3(1.2f, 3.2f, 0.03f));
                entrance.Box("Silver", new Vector3(side * (entry / 2 + 0.18f), 1.5f, -0.19f), new Vector3(0.045f, 0.7f, 0.045f));
            }
            entrance.Finish();
            Text(lobby, "Entrance_label", "UEH  /  CƠ SỞ B", new Vector3(0, 3.65f, 0.11f), 4.3f, 0.36f, "Teal");

            var ceiling = new Part("04_Timber_ceiling_and_downlights", lobby);
            foreach (float centerX in new[] { -w * 0.34f, 0, w * 0.34f })
            {
                float stripWidth = centerX == 0 ? 3.8f : 2.3f;
                ceiling.Box("Dark", new Vector3(centerX, h - 0.2f, -d / 2), new Vector3(stripWidth, 0.14f, d - 1));
                for (float x = centerX - stripWidth / 2; x < centerX + stripWidth / 2; x += 0.105f)
                    ceiling.Box("Wood", new Vector3(x, h - 0.29f, -d / 2), new Vector3(0.065f, 0.12f, d - 1));
            }
            for (float x = -w / 2 + 2; x < w / 2 - 1; x += 3)
                for (float z = -2; z > -d + 1; z -= 3.4f)
                {
                    ceiling.Cylinder("Silver", new Vector3(x, h - 0.22f, z), new Vector3(0.22f, 0.025f, 0.22f));
                    ceiling.Cylinder("WarmLight", new Vector3(x, h - 0.255f, z), new Vector3(0.18f, 0.01f, 0.18f));
                }
            for (int i = 0; i < 4; i++)
            {
                float z = -4 - i * 5.0f;
                ceiling.Box("Gold", new Vector3(0, h - 0.5f, z), new Vector3(1.8f, 0.08f, 0.85f));
                for (int crystal = 0; crystal < 18; crystal++)
                    ceiling.Box("WarmLight", new Vector3(-0.8f + crystal * 0.095f, h - 0.66f, z), new Vector3(0.045f, 0.3f, 0.65f));
                ceiling.Box("Ivory", new Vector3(7, h - 0.23f, z), new Vector3(1.3f, 0.04f, 0.45f));
                for (int vent = 0; vent < 8; vent++)
                    ceiling.Box("Metal", new Vector3(6.5f + vent * 0.14f, h - 0.26f, z), new Vector3(0.035f, 0.015f, 0.3f));
            }
            ceiling.Finish();
            BuildReception(w, d, h);
            BuildHeritage(w, d, h);
            BuildInnerHall(d, h);
            foreach (float x in new[] { -dimensions.frontage * 0.375f, dimensions.frontage * 0.375f })
            {
                Plant(lobby, "08_Indoor_plant_" + x, new Vector3(x - 1.1f, 0, -dimensions.lobbyDepth * 0.5f));
                Plant(lobby, "08_Entrance_plant_" + x, new Vector3(x, 0, -1.2f));
            }
        }

        private static void BuildReception(float w, float d, float h)
        {
            var group = new GameObject("05_Reception_and_welcome_wall").transform;
            group.SetParent(lobby, false);
            group.localPosition = new Vector3(-w * 0.30f, 0, -d + 0.4f);
            PopulateReception(group);
            for (int i = 0; i < 4; i++) Chair(lobby, new Vector3(-w / 2 + 2.1f, 0, -7.5f - i * 1.3f));
        }

        private static void BuildHeritage(float w, float d, float h)
        {
            var group = new GameObject("06_Heritage_1976_and_gallery").transform;
            group.SetParent(lobby, false);
            group.localPosition = new Vector3(w * 0.30f, 0, -d + 0.6f);
            PopulateHeritage(group);
        }

        // Display coordinates: u increases to the viewer's RIGHT, so local X = -u.
        // The parent zone rotates -90 degrees onto the left side wall. Local +Z faces into the lobby.
        private static Transform HeritageSection(Transform parent, string name, float u)
        {
            var section = new GameObject(name).transform;
            section.SetParent(parent, false);
            section.localPosition = new Vector3(-u, 0, 0);
            return section;
        }

        private static void PopulateHeritage(Transform parent)
        {
            Mat("HeritageOrange", "F46A08", 0.22f);
            Mat("HeritageYellow", "F4C51B", 0.22f);
            Mat("HeritagePink", "D4919F", 0.18f);
            Mat("HeritageGreen", "719D1B", 0.18f);
            Mat("HeritageBlue", "0C5973", 0.18f);
            Mat("HeritageLeafDark", "234E2D", 0.1f);

            var blank = HeritageSection(parent, "01_Plain_wall_left", -5.65f);
            var wall = new Part("Plain_wall", blank, "Heritage");
            wall.Box("Ivory", new Vector3(0, 2.1f, -0.32f), new Vector3(1.9f, 4.2f, 0.08f));
            wall.Finish();

            var frame = new Part("Timber_frame", parent, "Heritage");
            frame.Box("Wood", new Vector3(-0.5f, 2.05f, -0.16f), new Vector3(10.2f, 3.2f, 0.24f));
            // Shared frame surrounds three adjoining panels, with narrow timber mullions.
            foreach (float x in new[] { 4.55f, 1.0f, -2.5f, -5.55f })
                frame.Box("Wood", new Vector3(x, 2.05f, 0.055f), new Vector3(0.07f, 3.1f, 0.11f));
            foreach (float y in new[] { 0.5f, 3.6f })
                frame.Box("Wood", new Vector3(-0.5f, y, 0.055f), new Vector3(10.2f, 0.07f, 0.11f));
            frame.Finish();

            var green = HeritageSection(parent, "02_SINCE_1976_green", -2.75f);
            var foliage = new Part("Greenery_backing_and_leaves", green, "Heritage");
            foliage.Box("HeritageLeafDark", new Vector3(0, 2.05f, 0), new Vector3(3.43f, 3.01f, 0.09f));
            for (int row = 0; row < 12; row++)
                for (int col = 0; col < 13; col++)
                {
                    int i = row * 13 + col;
                    float x = -1.51f + col * 0.25f + Mathf.Sin(i * 2.3f) * 0.035f;
                    float y = 0.72f + row * 0.24f + Mathf.Cos(i * 1.7f) * 0.025f;
                    string color = i % 5 == 0 ? "LeafLight" : i % 3 == 0 ? "HeritageLeafDark" : "Leaf";
                    foliage.Leaf(color, new Vector3(x, y, 0.065f + (i % 3) * 0.012f),
                        new Vector2(0.32f, 0.42f), i * 137.5f);
                }
            foliage.Finish();
            Text(green, "Since_1976", "1976", new Vector3(0, 2.15f, 0.23f), 3.0f, 1.05f, "White");
            Text(green, "Since", "SINCE", new Vector3(1.12f, 3.22f, 0.23f), 0.8f, 0.17f, "White");

            var nine = HeritageSection(parent, "03_Nine_colour_panel", 0.75f);
            var neutral = new Part("Ivory_panel", nine, "Heritage");
            neutral.Box("Ivory", new Vector3(0, 2.05f, 0), new Vector3(3.43f, 3.01f, 0.09f));
            neutral.Finish();
            string[] colors = { "White", "HeritagePink", "HeritageBlue", "HeritageOrange", "White", "HeritageBlue", "HeritagePink", "HeritageOrange", "White" };
            for (int i = 0; i < colors.Length; i++)
            {
                var swatch = HeritageSection(nine, "Colour_" + (i + 1).ToString("00"), -1.32f + i * 0.33f);
                var tile = new Part("Tile_" + (i + 1).ToString("00"), swatch, "Heritage");
                tile.Box(colors[i], new Vector3(0, 0.97f, 0.09f), new Vector3(0.25f, 0.36f, 0.045f));
                tile.Finish();
            }

            var circles = HeritageSection(parent, "04_Circles_panel", 4.0f);
            var timber = new Part("Timber_panel", circles, "Heritage");
            timber.Box("Wood", new Vector3(0, 2.05f, 0), new Vector3(2.93f, 3.01f, 0.09f));
            timber.Finish();
            HeritageCircle(circles, "Circle_01_large_orange", -0.35f, 2.91f, 1.05f, "HeritageOrange");
            HeritageCircle(circles, "Circle_02_small_yellow", 0.83f, 3.03f, 0.55f, "HeritageYellow");
            HeritageCircle(circles, "Circle_03_medium_pink", -0.96f, 2.07f, 0.75f, "HeritagePink");
            HeritageCircle(circles, "Circle_04_large_green", 0.62f, 1.86f, 0.9f, "HeritageGreen");
            HeritageCircle(circles, "Circle_05_small_blue", -0.36f, 1.46f, 0.6f, "HeritageBlue");

            // The original service-area proxy is superseded by the furnished office in the next bay.
            if (dimensions != null && dimensions.leftOfficeAndLounge) return;
            // Retain the blue service-area motif until that office is furnished.
            var service = HeritageSection(parent, "05_Inner_service_blue_outline", 7.4f);
            var outline = new Part("Service_outline", service, "Heritage");
            outline.Box("Dark", new Vector3(0, 1.8f, -0.12f), new Vector3(2.6f, 3.6f, 0.2f));
            outline.Box("CoolLight", new Vector3(0, 3.62f, 0.03f), new Vector3(2.7f, 0.055f, 0.06f));
            foreach (int side in new[] { -1, 1 })
                outline.Box("CoolLight", new Vector3(side * 1.34f, 1.8f, 0.03f), new Vector3(0.055f, 3.6f, 0.06f));
            outline.Finish();
        }

        private static void HeritageCircle(Transform parent, string name, float u, float y, float diameter, string color)
        {
            var circle = HeritageSection(parent, name, u);
            var part = new Part(name, circle, "Heritage");
            part.Sphere(color, new Vector3(0, y, 0.1f), new Vector3(diameter, diameter, 0.045f));
            part.Finish();
        }

        private static void BuildInnerHall(float d, float h)
        {
            var hall = new Part("07_Inner_passage_inferred", lobby);
            hall.Box("Floor", new Vector3(0, -0.14f, -d - 3), new Vector3(6.1f, 0.28f, 6), true);
            foreach (int side in new[] { -1, 1 })
                hall.Box("Ivory", new Vector3(side * 3.1f, h / 2, -d - 3), new Vector3(0.2f, h, 6), true);
            hall.Box("White", new Vector3(0, h - 0.1f, -d - 3), new Vector3(6.2f, 0.2f, 6));
            hall.Box("Stone", new Vector3(0, 2.7f, -d + 0.1f), new Vector3(6, 0.35f, 0.4f));
            hall.Box("Pavement", new Vector3(0, -0.14f, -d - 10), new Vector3(20, 0.28f, 8), true);
            hall.Box("Stone", new Vector3(0, 0.4f, -d - 14), new Vector3(20, 0.8f, 0.4f), true);
            foreach (int side in new[] { -1, 1 })
            {
                hall.Box("Stone", new Vector3(side * 10, 0.4f, -d - 10), new Vector3(0.4f, 0.8f, 8), true);
                hall.Box("Stone", new Vector3(side * 6, 0.35f, -d - 10), new Vector3(5, 0.7f, 3), true);
            }
            hall.Finish();
            Tree(lobby, "Courtyard_context_left", new Vector3(-6, 0.7f, -d - 10), 5.8f);
            Tree(lobby, "Courtyard_context_right", new Vector3(6, 0.7f, -d - 10), 5.2f);
            Text(lobby, "Inner_direction", "KHÔNG GIAN HỌC THUẬT", new Vector3(0, 3.3f, -d + 0.3f), 5.4f, 0.28f, "Teal");
        }

        private static void Plant(Transform parent, string name, Vector3 p, string assetPrefix = null)
        {
            var part = new Part(name, parent, assetPrefix);
            part.Cylinder("White", p + Vector3.up * 0.35f, new Vector3(0.55f, 0.35f, 0.55f), true);
            part.Cylinder("Wood", p + Vector3.up * 0.98f, new Vector3(0.06f, 0.4f, 0.06f));
            for (int i = 0; i < 7; i++)
            {
                float a = i * 2.4f;
                part.Sphere(i % 2 == 0 ? "Leaf" : "LeafLight", p + new Vector3(Mathf.Cos(a) * 0.23f, 1.1f + (i % 3) * 0.18f, Mathf.Sin(a) * 0.23f), new Vector3(0.65f, 0.42f, 0.6f));
            }
            part.Finish();
        }

        private static void Tree(Transform parent, string name, Vector3 p, float height)
        {
            var part = new Part(name, parent);
            part.Cylinder("Wood", p + Vector3.up * height * 0.35f, new Vector3(0.32f, height * 0.35f, 0.32f), true);
            for (int i = 0; i < 11; i++)
            {
                float a = i * 2.4f;
                part.Sphere(i % 3 == 0 ? "LeafLight" : "Leaf", p + new Vector3(Mathf.Cos(a) * height * 0.21f, height * (0.72f + (i % 3) * 0.07f), Mathf.Sin(a) * height * 0.21f), new Vector3(height * 0.42f, height * 0.34f, height * 0.4f));
            }
            part.Finish();
        }

        private static void Chair(Transform parent, Vector3 p)
        {
            var part = new Part("Lounge_seat_" + p.z, parent);
            part.Box("CreamSeat", p + new Vector3(0, 0.46f, 0), new Vector3(0.85f, 0.22f, 0.85f), true);
            part.Box("CreamSeat", p + new Vector3(-0.3f, 0.85f, 0), new Vector3(0.18f, 0.7f, 0.9f));
            foreach (int side in new[] { -1, 1 })
            {
                part.Box("Metal", p + new Vector3(0, 0.22f, side * 0.32f), new Vector3(0.65f, 0.4f, 0.04f));
                part.Box("CreamSeat", p + new Vector3(0, 0.72f, side * 0.42f), new Vector3(0.85f, 0.22f, 0.12f));
            }
            part.Finish();
        }

        private static void Text(Transform parent, string name, string content, Vector3 position, float width, float height, string color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(0, 180, 0);
            var tm = go.AddComponent<TextMesh>();
            tm.font = signFont;
            tm.fontSize = 96;
            tm.fontStyle = FontStyle.Bold;
            tm.characterSize = 1;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = materials[color].color;
            tm.text = content;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = signFont.material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            signFont.RequestCharactersInTexture(content, 96, FontStyle.Bold);
            var size = renderer.localBounds.size;
            float factor = Mathf.Min(width / Mathf.Max(size.x, 0.01f), height / Mathf.Max(size.y, 0.01f));
            go.transform.localScale = Vector3.one * factor;
            var worldText = go.AddComponent<UEHWorldText>();
            worldText.worldMaterial = materials["WorldText"];
            worldText.Refresh();
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.ReflectionProbeStatic);
        }

        private static void SetupLighting()
        {
            var sky = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Lighting/DaylightSky.mat");
            if (sky == null) { sky = new Material(Shader.Find("Skybox/Procedural")); AssetDatabase.CreateAsset(sky, Root + "/Lighting/DaylightSky.mat"); }
            sky.SetFloat("_SunSize", 0.025f);
            sky.SetFloat("_AtmosphereThickness", 0.8f);
            sky.SetFloat("_Exposure", 1.1f);
            sky.SetColor("_SkyTint", new Color(0.53f, 0.64f, 0.75f));
            RenderSettings.skybox = sky;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.88f, 0.86f, 0.82f);
            RenderSettings.ambientEquatorColor = new Color(0.79f, 0.77f, 0.72f);
            RenderSettings.ambientGroundColor = new Color(0.57f, 0.55f, 0.50f);
            RenderSettings.ambientIntensity = 1;
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.73f, 0.81f, 0.85f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 100;
            RenderSettings.fogEndDistance = 220;
            var sun = new GameObject("Daylight / Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(42, -32, 0);
            sun.color = new Color(1, 0.95f, 0.85f);
            sun.intensity = 1.05f;
            sun.shadows = LightShadows.Soft;
            sun.shadowBias = 0.035f;
            RenderSettings.sun = sun;
            QualitySettings.shadowDistance = 85;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.antiAliasing = 4;
            var fillRoot = new GameObject("Lobby lighting").transform;
            foreach (float x in new[] { -11f, 0, 11f })
                foreach (float z in new[] { -5f, -15f, -23f })
                {
                    var light = new GameObject("Warm ceiling fill").AddComponent<Light>();
                    light.transform.SetParent(fillRoot, false);
                    light.transform.position = new Vector3(x, 3.85f, z);
                    light.type = LightType.Point;
                    light.range = 14;
                    light.intensity = 1.25f;
                    light.color = new Color(1, 0.90f, 0.72f);
                    light.shadows = LightShadows.None;
                    light.renderMode = LightRenderMode.Auto;
                }
        }

        private static void SetupWalkthrough()
        {
            var player = new GameObject("Walkthrough Player [independent of game saves]");
            player.transform.position = new Vector3(0, 0.08f, 13);
            player.transform.rotation = Quaternion.Euler(0, 180, 0);
            var body = player.AddComponent<CharacterController>();
            body.height = 1.8f;
            body.radius = 0.35f;
            body.center = new Vector3(0, 0.9f, 0);
            body.stepOffset = 0.3f;
            var cameraGO = new GameObject("UEH Main Camera");
            cameraGO.tag = "MainCamera";
            var camera = cameraGO.AddComponent<Camera>();
            cameraGO.AddComponent<AudioListener>();
            camera.fieldOfView = 68;
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = 240;
            camera.allowHDR = true;
            camera.transform.position = player.transform.position + Vector3.up * 1.65f;
            camera.transform.rotation = Quaternion.Euler(0, 180, 0);
            var movement = player.AddComponent<UEHWalkthroughController>();
            movement.viewCamera = camera;
            movement.lobbySpawn = new Vector3(0, 0.08f, -dimensions.lobbyDepth / 2);
        }

        private static void SetupViewpoints()
        {
            var root = new GameObject("Review viewpoints [select and align view]").transform;
            Viewpoint(root, "01_Exterior", new Vector3(40, 25, 66), new Vector3(0, 26, -6));
            Viewpoint(root, "02_Entrance", new Vector3(0, 1.65f, 12), new Vector3(0, 3.2f, -10));
            Viewpoint(root, "03_Lobby", new Vector3(0, 1.65f, 10), new Vector3(0, 1.8f, -7.5f));
            Viewpoint(root, "04_Reception", new Vector3(-6, 1.65f, -5), new Vector3(-17.0f, 1.9f, -7.5f));
            Viewpoint(root, "05_Lobby_toward_entrance", new Vector3(0, 1.65f, -21), new Vector3(0, 2, 0));
            Viewpoint(root, "06_Heritage", new Vector3(6, 1.65f, -5), new Vector3(17.0f, 1.9f, -7.5f));
            Viewpoint(root, "07_Entrance_columns_from_inside", new Vector3(0, 1.65f, -13), new Vector3(0, 1.9f, 0));
            Viewpoint(root, "08_Heritage_front", new Vector3(7, 1.8f, -8), new Vector3(17.4f, 2.1f, -8));
            SetupReceptionViewpoints(root);
        }

        private static void Viewpoint(Transform root, string name, Vector3 position, Vector3 target)
        {
            var point = new GameObject(name).transform;
            point.SetParent(root, false);
            point.position = position;
            point.LookAt(target);
        }

        private static void SetupReflectionProbes()
        {
            var root = new GameObject("Local reflection captures").transform;
            foreach (float x in new[] { -10f, 10f })
            {
                var go = new GameObject("Lobby reflection " + x);
                go.transform.SetParent(root, false);
                go.transform.position = new Vector3(x, 1.9f, -12);
                var probe = go.AddComponent<ReflectionProbe>();
                probe.mode = ReflectionProbeMode.Custom;
                probe.size = new Vector3(23, 5.2f, dimensions.lobbyDepth + 1);
                probe.center = new Vector3(0, 0.3f, 0);
                probe.boxProjection = true;
                probe.blendDistance = 3;
                probe.resolution = 128;
                probe.intensity = 0;
            }
        }

        public static void CaptureReflections()
        {
            var probes = UnityEngine.Object.FindObjectsByType<ReflectionProbe>();
            foreach (var probe in probes) probe.intensity = 0;
            foreach (var probe in probes)
            {
                var cameraGO = new GameObject("Temporary reflection camera");
                var camera = cameraGO.AddComponent<Camera>();
                camera.transform.position = probe.transform.position;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 180;
                camera.enabled = false;
                var cube = new Cubemap(128, TextureFormat.RGBAHalf, true);
                var target = new RenderTexture(128, 128, 24, RenderTextureFormat.ARGBHalf) { dimension = TextureDimension.Cube };
                target.Create();
                if (!camera.RenderToCubemap(target)) throw new InvalidOperationException("Reflection capture failed.");
                var prior = RenderTexture.active;
                var readback = new Texture2D(128, 128, TextureFormat.RGBAHalf, false);
                for (int face = 0; face < 6; face++)
                {
                    Graphics.SetRenderTarget(target, 0, (CubemapFace)face);
                    readback.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
                    readback.Apply();
                    cube.SetPixels(readback.GetPixels(), (CubemapFace)face);
                }
                cube.Apply(true, false);
                RenderTexture.active = prior;
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(readback);
                string path = Root + "/Lighting/" + (probe.transform.position.x < 0 ? "LobbyWest" : "LobbyEast") +
                    (probe.name.Contains("rear") ? "Rear" : "") + ".cubemap";
                var existing = AssetDatabase.LoadAssetAtPath<Cubemap>(path);
                if (existing != null)
                {
                    for (int face = 0; face < 6; face++) existing.SetPixels(cube.GetPixels((CubemapFace)face), (CubemapFace)face);
                    existing.Apply(true, false);
                    EditorUtility.SetDirty(existing);
                    UnityEngine.Object.DestroyImmediate(cube);
                    cube = existing;
                }
                else AssetDatabase.CreateAsset(cube, path);
                probe.customBakedTexture = cube;
                UnityEngine.Object.DestroyImmediate(cameraGO);
            }
            foreach (var probe in probes) probe.intensity = 0.85f;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }

        [MenuItem("UEH/Campus B/3. Capture review images and local reflections")]
        public static void CaptureReviewImages()
        {
            if (SceneManager.GetActiveScene().path != ScenePath) throw new InvalidOperationException("Activate the UEH study scene first.");
            CaptureReflections();
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews");
            Directory.CreateDirectory(output);
            var root = GameObject.Find("Review viewpoints [select and align view]");
            if (root == null) throw new InvalidOperationException("Missing review viewpoints.");
            foreach (Transform point in root.transform)
            {
                var cameraGO = new GameObject("Temporary review camera");
                var camera = cameraGO.AddComponent<Camera>();
                camera.enabled = false;
                camera.transform.SetPositionAndRotation(point.position, point.rotation);
                camera.fieldOfView = point.name.StartsWith("01") ? 58 :
                    point.name.StartsWith("22") ? 62 : point.name.StartsWith("23") ? 85 :
                    (point.name.StartsWith("24") || point.name.StartsWith("25")) ? 70 :
                    point.name.StartsWith("15") ? 65 :
                    point.name.StartsWith("19") ? 65 : point.name.StartsWith("20") ? 75 :
                    (point.name.StartsWith("16") || point.name.StartsWith("17")) ? 58 :
                    point.name.StartsWith("09") ? 58 : point.name.StartsWith("10") ? 48 :
                    (point.name.StartsWith("03") || point.name.StartsWith("07") ||
                     point.name.StartsWith("11") || point.name.StartsWith("13") || point.name.StartsWith("14") || point.name.StartsWith("18")) ? 90 : 75;
                camera.nearClipPlane = 0.08f;
                camera.farClipPlane = 240;
                var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32);
                rt.antiAliasing = 4;
                var prior = RenderTexture.active;
                camera.targetTexture = rt;
                camera.Render();
                RenderTexture.active = rt;
                var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                camera.targetTexture = null;
                RenderTexture.active = prior;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(cameraGO);
            }
            Debug.Log("UEH_PREVIEWS_COMPLETE " + output);
        }

        /// <summary>Each architectural part is editable; tiny repeated details share one mesh per material.</summary>
        private sealed class Part
        {
            private readonly Transform root;
            private readonly string assetPrefix;
            private readonly Dictionary<string, Geometry> groups = new Dictionary<string, Geometry>();
            public Part(string name, Transform parent, string assetPrefix = null)
            {
                this.assetPrefix = assetPrefix;
                root = new GameObject(name).transform;
                root.SetParent(parent, false);
            }
            private Geometry Group(string material)
            {
                if (!groups.TryGetValue(material, out var geometry)) { geometry = new Geometry(); groups.Add(material, geometry); }
                return geometry;
            }
            public void Box(string mat, Vector3 p, Vector3 size, bool collide = false)
            {
                Group(mat).Box(p, size);
                if (collide)
                {
                    var go = new GameObject("Collision / " + mat);
                    go.transform.SetParent(root, false);
                    go.transform.localPosition = p;
                    go.AddComponent<BoxCollider>().size = size;
                }
            }
            public void Sphere(string mat, Vector3 p, Vector3 size) { Primitive(mat, PrimitiveType.Sphere, p, size); }
            public void Leaf(string mat, Vector3 p, Vector2 size, float angle) { Group(mat).Leaf(p, size, angle); }
            public void AppendMesh(string mat, Mesh mesh, Matrix4x4 matrix) { Group(mat).Append(mesh, matrix); }
            public void Cylinder(string mat, Vector3 p, Vector3 size, bool collide = false)
            {
                Primitive(mat, PrimitiveType.Cylinder, p, size);
                if (collide)
                {
                    var go = new GameObject("Collision / planter or trunk");
                    go.transform.SetParent(root, false);
                    go.transform.localPosition = p;
                    var collider = go.AddComponent<BoxCollider>();
                    collider.size = new Vector3(size.x, size.y * 2, size.z);
                }
            }
            private void Primitive(string mat, PrimitiveType type, Vector3 p, Vector3 size)
            {
                if (!primitives.TryGetValue(type, out var mesh))
                {
                    var temporary = GameObject.CreatePrimitive(type);
                    mesh = temporary.GetComponent<MeshFilter>().sharedMesh;
                    primitives[type] = mesh;
                    UnityEngine.Object.DestroyImmediate(temporary);
                }
                Group(mat).Append(mesh, Matrix4x4.TRS(p, Quaternion.identity, size));
            }
            public void Finish()
            {
                foreach (var pair in groups)
                {
                    var mesh = pair.Value.ToMesh(root.name + "_" + pair.Key);
                    string path = Root + "/Meshes/" + (assetPrefix ?? (meshCounter++).ToString("000")) + "_" + Sanitize(mesh.name) + ".asset";
                    var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if (existing != null)
                    {
                        // CopySerialized alone can leave a stale GPU vertex buffer after an in-place rebuild.
                        existing.Clear();
                        existing.indexFormat = mesh.indexFormat;
                        existing.vertices = mesh.vertices;
                        existing.normals = mesh.normals;
                        existing.uv = mesh.uv;
                        existing.triangles = mesh.triangles;
                        existing.tangents = mesh.tangents;
                        existing.RecalculateBounds();
                        existing.UploadMeshData(false);
                        EditorUtility.SetDirty(existing);
                        UnityEngine.Object.DestroyImmediate(mesh);
                        mesh = existing;
                    }
                    else AssetDatabase.CreateAsset(mesh, path);
                    var go = new GameObject(pair.Key);
                    go.transform.SetParent(root, false);
                    go.AddComponent<MeshFilter>().sharedMesh = mesh;
                    var renderer = go.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = materials[pair.Key];
                    renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
                    if (pair.Key == "ClearGlass" || pair.Key == "OfficeGlass") renderer.shadowCastingMode = ShadowCastingMode.Off;
                    GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic);
                }
            }
            private static string Sanitize(string name)
            {
                foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
                return name.Replace(' ', '_');
            }
        }

        private sealed class Geometry
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<Vector3> normals = new List<Vector3>();
            private readonly List<Vector2> uv = new List<Vector2>();
            private readonly List<int> triangles = new List<int>();
            public void Leaf(Vector3 p, Vector2 size, float angle)
            {
                var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                Vector2[] rim = { new Vector2(0, 0.5f), new Vector2(-0.4f, 0.2f), new Vector2(-0.35f, -0.2f),
                    new Vector2(0, -0.5f), new Vector2(0.35f, -0.2f), new Vector2(0.4f, 0.2f) };
                for (int i = 0; i < rim.Length; i++)
                {
                    Vector3 a = p + new Vector3(0, 0, 0.025f);
                    Vector3 b = p + rotation * new Vector3(rim[i].x * size.x, rim[i].y * size.y, 0);
                    Vector2 next = rim[(i + 1) % rim.Length];
                    Vector3 c = p + rotation * new Vector3(next.x * size.x, next.y * size.y, 0);
                    int start = vertices.Count;
                    vertices.AddRange(new[] { a, b, c });
                    Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
                    normals.AddRange(new[] { normal, normal, normal });
                    uv.AddRange(new[] { new Vector2(0.5f, 0.5f), rim[i] + Vector2.one * 0.5f, next + Vector2.one * 0.5f });
                    triangles.AddRange(new[] { start, start + 1, start + 2 });
                }
            }
            private void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                int n = vertices.Count;
                vertices.AddRange(new[] { a, b, c, d });
                var normal = Vector3.Cross(b - a, c - a).normalized;
                for (int i = 0; i < 4; i++) normals.Add(normal);
                float width = Vector3.Distance(a, b), height = Vector3.Distance(b, c);
                uv.AddRange(new[] { Vector2.zero, new Vector2(width, 0), new Vector2(width, height), new Vector2(0, height) });
                triangles.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3 });
            }
            public void Box(Vector3 p, Vector3 size)
            {
                Vector3 a = p - size / 2, b = p + size / 2;
                Quad(new Vector3(a.x, a.y, b.z), new Vector3(b.x, a.y, b.z), new Vector3(b.x, b.y, b.z), new Vector3(a.x, b.y, b.z));
                Quad(new Vector3(b.x, a.y, a.z), new Vector3(a.x, a.y, a.z), new Vector3(a.x, b.y, a.z), new Vector3(b.x, b.y, a.z));
                Quad(new Vector3(a.x, b.y, b.z), new Vector3(b.x, b.y, b.z), new Vector3(b.x, b.y, a.z), new Vector3(a.x, b.y, a.z));
                Quad(new Vector3(a.x, a.y, a.z), new Vector3(b.x, a.y, a.z), new Vector3(b.x, a.y, b.z), new Vector3(a.x, a.y, b.z));
                Quad(new Vector3(a.x, a.y, a.z), new Vector3(a.x, a.y, b.z), new Vector3(a.x, b.y, b.z), new Vector3(a.x, b.y, a.z));
                Quad(new Vector3(b.x, a.y, b.z), new Vector3(b.x, a.y, a.z), new Vector3(b.x, b.y, a.z), new Vector3(b.x, b.y, b.z));
            }
            public void Append(Mesh mesh, Matrix4x4 matrix)
            {
                int offset = vertices.Count;
                foreach (Vector3 v in mesh.vertices) vertices.Add(matrix.MultiplyPoint3x4(v));
                var normalMatrix = matrix.inverse.transpose;
                foreach (Vector3 n in mesh.normals) normals.Add(normalMatrix.MultiplyVector(n).normalized);
                uv.AddRange(mesh.uv);
                foreach (int i in mesh.triangles) triangles.Add(offset + i);
            }
            public Mesh ToMesh(string name)
            {
                var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uv);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateTangents();
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
#endif
