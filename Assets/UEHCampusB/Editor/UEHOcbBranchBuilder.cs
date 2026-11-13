#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UEH.CampusB.Editor
{
    public static partial class UEHCampusBuilder
    {
        private const float OcbWidth = 7.2f;
        private const float OcbDepth = 6f;
        private const float OcbHeight = 4.2f;
        private static float OcbCenterX => LeftGateColumnX(dimensions) + dimensions.lobbyColumnWidth / 2 + 0.1f + OcbWidth / 2;
        private static float OcbFrontZ => EntranceGlassZ(dimensions);
        private static float OcbBackZ => OcbFrontZ - OcbDepth;

        [MenuItem("UEH/Campus B/27. Add recessed OCB bank branch at left entrance")]
        public static void BuildOcbBranchAndCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.expandedLobby) throw new InvalidOperationException("Expand the lobby before placing the OCB branch.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[m.name] = m; }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.ocbBranch = true;
            EditorUtility.SetDirty(dimensions);
            string lobbyPath = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(lobbyPath);
            try
            {
                BuildOcbBranch(contents.transform);
                RebuildOfficeWallOpening(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, lobbyPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            string exteriorPath = Root + "/Prefabs/UEH_Exterior.prefab";
            contents = PrefabUtility.LoadPrefabContents(exteriorPath);
            try
            {
                var oldGate = contents.transform.Find("08_Retracted_gate_and_planters");
                if (oldGate != null) UnityEngine.Object.DestroyImmediate(oldGate.gameObject);
                BuildForecourtGate(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, exteriorPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            TrimCompactLeftExteriorWall();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupOcbBranchViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            CaptureOcbBranchPreviews();
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/ocb-branch-layout.json"), JsonUtility.ToJson(new OcbBranchLayout
            {
                centerX = OcbCenterX, backZ = OcbBackZ, frontZ = OcbFrontZ, width = OcbWidth, depth = OcbDepth,
                areaSquareMeters = OcbWidth * OcbDepth, entranceColumnX = LeftGateColumnX(dimensions),
                entranceColumnZ = LeftColumnZ(dimensions), entranceWalkwayKeptOpen = true
            }, true));
            Debug.Log("UEH_OCB_BRANCH_COMPLETE Side bay outside the retained entrance pier; glass aligned with entrance facade; original forecourt planters restored.");
        }

        private static void BuildOcbBranch(Transform lobbyRoot)
        {
            var old = lobbyRoot.Find("17_OCB_bank_branch");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            Mat("OcbGreen", "00A651", 0.28f);
            var room = new GameObject("17_OCB_bank_branch").transform;
            room.SetParent(lobbyRoot, false);
            room.localPosition = new Vector3(OcbCenterX, 0, OcbBackZ);
            // Occupy the red-marked bay beyond the corner pier, recessed to the entrance glass plane.

            var interior = new Part("01_Room_shell_and_floor", room, "OcbBranch");
            interior.Box("Floor", new Vector3(0, -0.12f, OcbDepth / 2), new Vector3(OcbWidth, 0.24f, OcbDepth), true);
            interior.Box("OfficeInterior", new Vector3(-OcbWidth / 2, OcbHeight / 2, OcbDepth / 2), new Vector3(0.16f, OcbHeight, OcbDepth), true);
            interior.Box("OfficeInterior", new Vector3(OcbWidth / 2, OcbHeight / 2, OcbDepth / 2), new Vector3(0.16f, OcbHeight, OcbDepth), true);
            interior.Box("OfficeInterior", new Vector3(0, OcbHeight / 2, 0), new Vector3(OcbWidth, OcbHeight, 0.16f), true);
            interior.Box("White", new Vector3(0, OcbHeight + 0.04f, OcbDepth / 2), new Vector3(OcbWidth + 0.16f, 0.16f, OcbDepth + 0.16f), true);
            interior.Finish();

            var front = new Part("02_Glass_storefront_and_open_door", room, "OcbBranch");
            const float doorX = 0.0f, doorWidth = 1.25f;
            const float half = OcbWidth / 2;
            float doorA = doorX - doorWidth / 2, doorB = doorX + doorWidth / 2;
            // Three continuous clear-glass bays; the entrance is kept open and the leaf rests inside.
            AddOcbPane(front, -half, doorA, 1.92f);
            AddOcbPane(front, doorB, half, 1.92f);
            foreach (float x in new[] { -half, -1.8f, doorA, doorB, 1.8f, half })
                front.Box("Metal", new Vector3(x, 1.95f, OcbDepth), new Vector3(0.055f, 3.9f, 0.09f));
            foreach (float y in new[] { 0.055f, 3.9f })
                front.Box("Metal", new Vector3(0, y, OcbDepth), new Vector3(OcbWidth, 0.08f, 0.09f));
            front.Box("Metal", new Vector3(0, 2.75f, OcbDepth), new Vector3(OcbWidth, 0.04f, 0.08f));
            front.Box("OcbGreen", new Vector3(0, 3.51f, OcbDepth + 0.065f), new Vector3(OcbWidth - 0.08f, 0.58f, 0.08f));
            front.Finish();
            AddExteriorOcbWordmark(room);
            var openLeaf = new Part("Open_door_leaf_parked_inside", room, "OcbBranch");
            openLeaf.Box("ClearGlass", new Vector3(doorX + 0.60f, 1.32f, OcbDepth - 0.60f), new Vector3(0.055f, 2.6f, doorWidth), true);
            openLeaf.Box("Metal", new Vector3(doorX + 0.60f, 1.32f, OcbDepth - 0.60f), new Vector3(0.07f, 2.65f, 0.055f));
            openLeaf.Finish();
            Text(room, "OCB_opening_label", "LỐI VÀO", new Vector3(doorX, 2.1f, OcbDepth + 0.14f), 0.78f, 0.14f, "Teal");

            var branding = new Part("03_Interior_brand_and_teller", room, "OcbBranch");
            branding.Box("OcbGreen", new Vector3(0, 3.15f, 0.21f), new Vector3(3.35f, 0.72f, 0.07f));
            branding.Box("White", new Vector3(0, 3.15f, 0.255f), new Vector3(3.25f, 0.62f, 0.025f));
            branding.Box("OcbGreen", new Vector3(0, 3.15f, 0.28f), new Vector3(3.12f, 0.50f, 0.02f));
            branding.Box("OfficeInterior", new Vector3(0, 0.55f, 1.2f), new Vector3(4.7f, 1.1f, 0.78f), true);
            branding.Box("Wood", new Vector3(0, 1.13f, 1.17f), new Vector3(4.8f, 0.08f, 0.84f));
            branding.Box("OcbGreen", new Vector3(0, 1.43f, 1.6f), new Vector3(4.35f, 0.045f, 0.08f));
            for (int i = 0; i < 3; i++)
            {
                float x = -1.45f + i * 1.45f;
                branding.Box("OfficeCharcoal", new Vector3(x, 2.06f, 1.54f), new Vector3(0.68f, 0.42f, 0.06f));
                branding.Box("BlueGlass", new Vector3(x, 2.07f, 1.50f), new Vector3(0.59f, 0.31f, 0.018f));
                branding.Box("OfficeCharcoal", new Vector3(x, 1.82f, 1.52f), new Vector3(0.06f, 0.12f, 0.05f));
            }
            branding.Finish();
            Text(room, "OCB_interior_wordmark", "OCB", new Vector3(0, 3.15f, 0.3f), 1.15f, 0.4f, "White");

            // Two small consultation desks plus waiting seats fit without blocking the open entry.
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -2.25f : 2.25f;
                var desk = new Part("Customer_service_desk_0" + (i + 1), room, "OcbBranch");
                desk.Box("Wood", new Vector3(x, 0.76f, 3.25f), new Vector3(1.45f, 0.10f, 0.64f), true);
                desk.Box("Metal", new Vector3(x - 0.56f, 0.37f, 3.25f), new Vector3(0.055f, 0.74f, 0.5f));
                desk.Box("Metal", new Vector3(x + 0.56f, 0.37f, 3.25f), new Vector3(0.055f, 0.74f, 0.5f));
                desk.Box("OfficeCharcoal", new Vector3(x, 1.04f, 3.02f), new Vector3(0.48f, 0.32f, 0.045f));
                desk.Box("BlueGlass", new Vector3(x, 1.05f, 2.99f), new Vector3(0.4f, 0.23f, 0.012f));
                desk.Finish();
                AddOcbChair(room, "Staff_chair_0" + (i + 1), new Vector3(x, 0, 3.9f), false);
                AddOcbChair(room, "Visitor_chair_0" + (i + 1), new Vector3(x, 0, 2.58f), true);
            }
            AddOcbChair(room, "Visitor_waiting_chair", new Vector3(2.8f, 0, 4.8f), true);
            var lights = new Part("04_Ceiling_downlights", room, "OcbBranch");
            foreach (float x in new[] { -2.2f, 0, 2.2f })
            {
                lights.Cylinder("Silver", new Vector3(x, 4.0f, 3.0f), new Vector3(0.21f, 0.025f, 0.21f));
                lights.Cylinder("WarmLight", new Vector3(x, 3.965f, 3.0f), new Vector3(0.16f, 0.012f, 0.16f));
            }
            lights.Finish();
        }

        private static void AddOcbPane(Part front, float a, float b, float y)
        {
            if (b - a < 0.1f) return;
            front.Box("ClearGlass", new Vector3((a + b) / 2, y, OcbDepth + 0.014f), new Vector3(b - a - 0.04f, 3.65f, 0.025f), true);
            front.Box("White", new Vector3((a + b) / 2, 1.12f, OcbDepth + 0.03f), new Vector3(b - a - 0.08f, 0.04f, 0.004f));
        }

        private static void AddExteriorOcbWordmark(Transform room)
        {
            var logo = new Part("Exterior_OCB_wordmark", room, "OcbBranch");
            logo.Box("White", new Vector3(2.3f, 3.51f, OcbDepth + 0.12f), new Vector3(2.3f, 0.55f, 0.018f));
            logo.Finish();
            Text(room, "OCB_exterior_name", "OCB", new Vector3(2.3f, 3.51f, OcbDepth + 0.15f), 1.6f, 0.38f, "OcbGreen");
            Text(room, "OCB_exterior_bank_name", "NG\u00c2N H\u00c0NG PH\u01af\u01a0NG \u0110\u00d4NG", new Vector3(-1.0f, 3.51f, OcbDepth + 0.15f), 3.9f, 0.2f, "White");
        }

        private static void AddOcbChair(Transform parent, string name, Vector3 p, bool visitor)
        {
            var chair = new Part(name, parent, "OcbBranch");
            string color = visitor ? "CreamSeat" : "OfficeCharcoal";
            chair.Box(color, p + new Vector3(0, 0.46f, 0), new Vector3(0.52f, 0.13f, 0.52f));
            chair.Box(color, p + new Vector3(0, 0.78f, -0.22f), new Vector3(0.52f, 0.54f, 0.09f));
            foreach (float x in new[] { -0.2f, 0.2f })
                foreach (float z in new[] { -0.2f, 0.2f }) chair.Box("Metal", p + new Vector3(x, 0.22f, z), new Vector3(0.035f, 0.44f, 0.035f));
            chair.Finish();
        }

        private static void SetupOcbBranchViewpoints()
        {
            var views = GameObject.Find("Review viewpoints [select and align view]"); if (views == null) return;
            foreach (string name in new[] { "78_OCB_branch_frontage", "79_OCB_branch_from_gate", "80_OCB_branch_interior", "81_OCB_corrected_side_bay" })
            { var old = views.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            float centerX = OcbCenterX;
            Viewpoint(views.transform, "78_OCB_branch_frontage", new Vector3(centerX, 1.8f, 7.5f), new Vector3(centerX, 2.0f, OcbFrontZ));
            Viewpoint(views.transform, "79_OCB_branch_from_gate", new Vector3(28, 2.2f, 9), new Vector3(16, 2, -2.2f));
            Viewpoint(views.transform, "80_OCB_branch_interior", new Vector3(centerX, 1.7f, OcbFrontZ - 1.0f), new Vector3(centerX, 1.7f, OcbBackZ + 1.0f));
            Viewpoint(views.transform, "81_OCB_corrected_side_bay", new Vector3(26, 2.4f, 7), new Vector3(13, 2, -1.5f));
        }

        private static void CaptureOcbBranchPreviews()
        {
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            var views = GameObject.Find("Review viewpoints [select and align view]").transform;
            foreach (Transform point in views)
            {
                if (!point.name.StartsWith("78_") && !point.name.StartsWith("79_") && !point.name.StartsWith("80_") && !point.name.StartsWith("81_")) continue;
                var cameraGO = new GameObject("Temporary OCB review camera"); var camera = cameraGO.AddComponent<Camera>(); camera.enabled = false;
                camera.transform.SetPositionAndRotation(point.position, point.rotation);
                camera.fieldOfView = point.name.StartsWith("80_") ? 80 : 76; camera.nearClipPlane = 0.08f; camera.farClipPlane = 240;
                var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                var prior = RenderTexture.active; camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                var texture = new Texture2D(1600, 1000, TextureFormat.RGB24, false); texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = prior; rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
                UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(cameraGO);
            }
        }

        [Serializable] private sealed class OcbBranchLayout
        {
            public string status = "Native Unity generated geometry and rendered previews; dimensions estimated from supplied references; no Play Mode or automated tests.";
            public float centerX, backZ, frontZ, width, depth, areaSquareMeters, entranceColumnX, entranceColumnZ;
            public bool entranceWalkwayKeptOpen;
        }
    }
}
#endif
