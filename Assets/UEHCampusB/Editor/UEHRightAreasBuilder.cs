#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UEH.CampusB.Editor
{
    public static partial class UEHCampusBuilder
    {
        public const float ServiceCorridorZ = -19.5f;
        public const float ServiceCorridorWidth = 2.8f;
        public const float ServicePortalHeight = 3.4f;
        public const float ServiceCorridorDepth = 8.6f;
        public const float LiftBankZ = -27;

        [MenuItem("UEH/Campus B/11. Build right lifts and service corridor")]
        public static void FurnishRightAreas()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before furnishing the right areas.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.expandedLobby) throw new InvalidOperationException("Expand the lobby first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                materials[material.name] = material;
            }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.rightLiftsAndServices = true;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                PopulateRightLiftsAndServices(contents.transform.Find("09_Four_expansion_areas"));
                RebuildOfficeWallOpening(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            TrimServiceExteriorGroundWall();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupRightAreaViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_RIGHT_SERVICES_UPDATED Three silver lift doors; recessed 2.8x8.6m corridor between WELCOME and lifts. Looking into corridor: stairs up/down on left, male/female toilets on right. Static closed doors, no lift or multi-floor simulation.");
        }

        private static void PopulateRightLiftsAndServices(Transform areas)
        {
            if (areas == null) throw new InvalidOperationException("Missing expansion bays.");
            Mat("LiftStainless", "D7DFE2", 0.46f, 0.3f);
            Mat("LiftGrain", "C8D1D5", 0.42f, 0.3f);
            var serviceBay = areas.Find("RIGHT_01 [12m x 6m]");
            var liftBay = areas.Find("RIGHT_02 [12m x 6m]");
            foreach (var bay in new[] { serviceBay, liftBay })
            {
                if (bay == null) throw new InvalidOperationException("Missing right bay.");
                foreach (string name in new[] { "Bay_frame_RIGHT_01", "Bay_frame_RIGHT_02", "Floor_inlay_RIGHT_01", "Floor_inlay_RIGHT_02", "Service_corridor", "Three_lift_bank" })
                { var old = bay.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                var space = bay.GetComponentInChildren<UEHReservedSpace>();
                space.furnished = true;
                space.purpose = bay == serviceBay ? "Hành lang lùi sau vách: trái thang bộ lên/xuống hầm, phải WC nam/nữ. Kích thước ước lượng."
                    : "Cụm 3 cửa thang máy A/B/C, mặt inox và vách đá theo ảnh tham khảo. Cửa mô hình tĩnh.";
            }
            // Earlier plan reserved a different pocket near the entrance. Keep that pocket clear,
            // but remove the obsolete lift designation now that the bank has its actual location.
            var oldLift = areas.parent.Find("05_WELCOME_RIGHT_from_gate/05_Reception_and_welcome_wall/03_Future_lift_space_RIGHT");
            if (oldLift != null) oldLift.name = "03_Clear_entrance_pocket";
            var pocket = areas.parent.Find("05_WELCOME_RIGHT_from_gate/05_Reception_and_welcome_wall/03_Clear_entrance_pocket");
            if (pocket != null && !dimensions.facultyLiftEntrance) pocket.GetComponent<UEHReservedSpace>().purpose = "Khoảng trống gần cửa vào được giữ thoáng. Cụm thang máy nằm ở hai khoang mở rộng phía trong.";
            BuildServiceCorridor(serviceBay);
            BuildThreeLifts(liftBay);
            if (dimensions.rightGlassOffice)
                liftBay.GetComponentInChildren<UEHReservedSpace>().purpose = "Cụm thang máy phía trước và office kính lùi sau vách ở phần trống phía trong. Kích thước ước lượng.";
        }

        private static Transform RightGroup(Transform parent, string name, Vector3 position)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false); group.localPosition = position;
            return group;
        }

        private static void BuildThreeLifts(Transform bay)
        {
            // The two original bays share one continuous program. Bring the lift bank next to
            // the service portal rather than retaining a blank 10m stretch between their centers.
            var root = RightGroup(bay, "Three_lift_bank", new Vector3(bay.localPosition.z - LiftBankZ, 0, 0));
            var wall = new Part("Lift_stone_and_dark_panels", root, "RightServices");
            // Flat facade aligned with the existing right side-wall. No mass projects into the hall.
            wall.Box("Dark", new Vector3(0, 2.1f, -0.13f), new Vector3(12, 4.2f, 0.18f), true);
            foreach (float x in new[] { -5.75f, -1.8f, 1.8f, 5.75f })
            {
                wall.Box("Stone", new Vector3(x, 2.1f, 0.015f), new Vector3(0.65f, 4.2f, 0.13f));
                wall.Box("Border", new Vector3(x, 0.14f, 0.04f), new Vector3(0.65f, 0.28f, 0.16f));
            }
            wall.Box("Metal", new Vector3(0, 4.15f, 0.035f), new Vector3(12, 0.1f, 0.12f));
            wall.Finish();
            for (int i = 0; i < 3; i++)
            {
                // Number from the viewer's left to right (local +X to -X).
                string letter = ((char)('A' + i)).ToString();
                var lift = RightGroup(root, "Lift_" + letter, new Vector3(3.6f - i * 3.6f, 0, 0));
                var frame = new Part("Lift_" + letter + "_frame", lift, "RightServices");
                frame.Box("Metal", new Vector3(0, 1.32f, 0.02f), new Vector3(2.03f, 2.64f, 0.12f));
                foreach (float x in new[] { -0.94f, 0.94f })
                    frame.Box("LiftStainless", new Vector3(x, 1.32f, 0.11f), new Vector3(0.12f, 2.64f, 0.1f));
                frame.Box("LiftStainless", new Vector3(0, 2.58f, 0.11f), new Vector3(2, 0.12f, 0.1f));
                frame.Box("LiftStainless", new Vector3(0, 0.018f, 0.11f), new Vector3(2, 0.03f, 0.2f));
                frame.Box("Dark", new Vector3(0, 2.92f, 0.09f), new Vector3(0.64f, 0.23f, 0.08f));
                frame.Finish();
                for (int side = -1; side <= 1; side += 2)
                {
                    var leaf = RightGroup(lift, side < 0 ? "Sliding_leaf_left" : "Sliding_leaf_right", new Vector3(side * 0.436f, 0, 0));
                    var geometry = new Part("Lift_" + letter + (side < 0 ? "_left_leaf" : "_right_leaf"), leaf, "RightServices");
                    geometry.Box("LiftStainless", new Vector3(0, 1.29f, 0.1f), new Vector3(0.866f, 2.51f, 0.06f), true);
                    // Fine vertical lines suggest brushed stainless steel without adding a raster decal.
                    for (int line = 0; line < 5; line++)
                        geometry.Box("LiftGrain", new Vector3(-0.36f + line * 0.18f, 1.3f, 0.131f), new Vector3(0.002f, 2.42f, 0.001f));
                    geometry.Finish();
                }
                var buttons = new Part("Lift_" + letter + "_call_panel", lift, "RightServices");
                buttons.Box("LiftStainless", new Vector3(-1.24f, 1.2f, 0.1f), new Vector3(0.19f, 0.48f, 0.08f));
                foreach (float y in new[] { 1.11f, 1.29f })
                {
                    buttons.Box("Dark", new Vector3(-1.24f, y, 0.145f), new Vector3(0.1f, 0.1f, 0.015f));
                    buttons.Box("CoolLight", new Vector3(-1.24f, y, 0.156f), new Vector3(0.045f, 0.045f, 0.006f));
                }
                buttons.Finish();
                Text(lift, "Lift_identifier", letter, new Vector3(0, 3.3f, 0.18f), 0.33f, 0.38f, "Gold");
                Text(lift, "Floor_indicator", "0", new Vector3(0, 2.92f, 0.143f), 0.1f, 0.15f, "CoolLight");
                ServiceLight(lift, "Lift ceiling wash", new Vector3(0, 3.8f, 1.3f), 4.8f, 0.8f);
            }
            Text(root, "Lift_bank_sign", "THANG MÁY  /  ELEVATORS", new Vector3(0, 3.85f, 0.13f), 3.8f, 0.2f, "Silver");
            var waiting = new Part("Lift_waiting_floor_inlay", root, "RightServices");
            waiting.Box("Border", new Vector3(0, 0.009f, 3.6f), new Vector3(12, 0.014f, 0.1f));
            foreach (float x in new[] { -6f, 6f })
                waiting.Box("Border", new Vector3(x, 0.009f, 1.85f), new Vector3(0.1f, 0.014f, 3.5f));
            waiting.Finish();
        }

        private static void BuildServiceCorridor(Transform bay)
        {
            // Right-bay local +X runs deeper into the building; local -Z goes behind its side wall.
            var root = RightGroup(bay, "Service_corridor", new Vector3(bay.localPosition.z - ServiceCorridorZ, 0, 0));
            var shell = new Part("Service_corridor_shell", root, "RightServices");
            float width = ServiceCorridorWidth, depth = ServiceCorridorDepth;
            // Main lobby floor ends 0.6m behind the display line. This slab abuts it at X=-18.
            RightRoomFloor(shell, root, width + 0.4f, depth + 0.1f);
            shell.Box("White", new Vector3(0, 3.45f, -depth / 2), new Vector3(width + 0.4f, 0.1f, depth), true);
            shell.Box("Ivory", new Vector3(0, 1.7f, -depth), new Vector3(width + 0.4f, 3.4f, 0.2f), true);
            foreach (int side in new[] { -1, 1 })
            {
                // Solid segments around two 1.1m door openings on each side.
                float previous = 0;
                foreach (float doorDepth in new[] { 2.4f, 5.8f })
                {
                    float next = doorDepth - 0.58f;
                    shell.Box("Ivory", new Vector3(side * (width / 2 + 0.1f), 1.7f, -(previous + next) / 2), new Vector3(0.2f, 3.4f, next - previous), true);
                    shell.Box("Ivory", new Vector3(side * (width / 2 + 0.1f), 2.9f, -doorDepth), new Vector3(0.2f, 1, 1.16f), true);
                    previous = doorDepth + 0.58f;
                }
                shell.Box("Ivory", new Vector3(side * (width / 2 + 0.1f), 1.7f, -(previous + depth) / 2), new Vector3(0.2f, 3.4f, depth - previous), true);
            }
            shell.Finish();
            var portal = new Part("Service_stone_portal", root, "RightServices");
            foreach (float x in new[] { -1.56f, 1.56f })
                portal.Box("Stone", new Vector3(x, 1.7f, -0.12f), new Vector3(0.32f, 3.4f, 0.24f));
            portal.Box("Metal", new Vector3(0, 3.55f, -0.03f), new Vector3(3.44f, 0.3f, 0.15f));
            portal.Box("Teal", new Vector3(0, 3.2f, -0.12f), new Vector3(1.6f, 0.25f, 0.06f));
            portal.Finish();
            Text(root, "Corridor_sign", "THANG BỘ  /  WC", new Vector3(0, 3.55f, 0.06f), 2.5f, 0.17f, "White");
            Text(root, "Exit_sign", "EXIT  /  LỐI THOÁT", new Vector3(0, 3.2f, -0.075f), 1.35f, 0.13f, "White");
            AddServiceDoor(root, "LEFT_01_Stairs_UP", 1, 2.4f, "THANG BỘ LÊN", "TẦNG TRÊN", true, false);
            AddServiceDoor(root, "LEFT_02_Stairs_DOWN_Parking", 1, 5.8f, "THANG BỘ XUỐNG", "TẦNG GỬI XE", true, true);
            AddServiceDoor(root, "RIGHT_01_Toilet_MEN", -1, 2.4f, "TOILET NAM", "MEN", false, false);
            AddServiceDoor(root, "RIGHT_02_Toilet_WOMEN", -1, 5.8f, "TOILET NỮ", "WOMEN", false, true);
            var ceiling = new Part("Service_ceiling_lights", root, "RightServices");
            foreach (float z in new[] { -1.4f, -4.2f, -7.2f })
            {
                ceiling.Box("Metal", new Vector3(0, 3.38f, z), new Vector3(1.1f, 0.04f, 0.32f));
                ceiling.Box("WarmLight", new Vector3(0, 3.35f, z), new Vector3(1, 0.02f, 0.26f));
                ServiceLight(root, "Corridor ceiling " + z, new Vector3(0, 3.05f, z), 4.4f, 1.3f);
            }
            ceiling.Finish();
        }

        private static void AddServiceDoor(Transform corridor, string name, int side, float depth, string title, string subtitle, bool stairs, bool variant)
        {
            var door = RightGroup(corridor, name, new Vector3(side * ServiceCorridorWidth / 2, 0, -depth));
            door.localRotation = Quaternion.Euler(0, side > 0 ? -90 : 90, 0);
            var frame = new Part(name + "_frame", door, "RightServices");
            foreach (float x in new[] { -0.55f, 0.55f })
                frame.Box("Metal", new Vector3(x, 1.2f, 0.015f), new Vector3(0.08f, 2.4f, 0.13f));
            frame.Box("Metal", new Vector3(0, 2.38f, 0.015f), new Vector3(1.18f, 0.08f, 0.13f));
            frame.Box("Silver", new Vector3(0, 0.012f, 0), new Vector3(1.16f, 0.02f, 0.22f));
            frame.Box("Teal", new Vector3(0, 2.73f, 0.05f), new Vector3(1.34f, 0.36f, 0.06f));
            frame.Finish();
            var leafRoot = RightGroup(door, "Door_leaf_closed", Vector3.zero);
            var leaf = new Part(name + "_leaf", leafRoot, "RightServices");
            leaf.Box(stairs ? "OfficeCharcoal" : "Wood", new Vector3(0, 1.18f, 0), new Vector3(1.02f, 2.34f, 0.06f), true);
            leaf.Box("Silver", new Vector3(0, 0.16f, 0.036f), new Vector3(0.96f, 0.28f, 0.012f));
            leaf.Box("Silver", new Vector3(-0.37f, 1.05f, 0.08f), new Vector3(0.19f, 0.035f, 0.05f));
            leaf.Box("Dark", new Vector3(0, 1.74f, 0.043f), new Vector3(0.44f, 0.57f, 0.018f));
            if (stairs)
            {
                for (int i = 0; i < 4; i++)
                    leaf.Box("White", new Vector3(-0.13f + i * 0.075f, 1.6f + (variant ? 3 - i : i) * 0.065f, 0.058f), new Vector3(0.072f, 0.03f, 0.012f));
            }
            else
            {
                leaf.Sphere("White", new Vector3(0, 1.9f, 0.068f), new Vector3(0.095f, 0.095f, 0.025f));
                leaf.Box("White", new Vector3(0, 1.75f, 0.065f), new Vector3(variant ? 0.16f : 0.12f, 0.2f, 0.02f));
                foreach (float x in new[] { -0.043f, 0.043f })
                    leaf.Box("White", new Vector3(x, 1.59f, 0.065f), new Vector3(0.035f, 0.15f, 0.02f));
            }
            leaf.Finish();
            Text(door, "Door_title", title, new Vector3(0, 2.73f, 0.09f), 1.18f, 0.17f, "White");
            Text(door, "Door_destination", subtitle, new Vector3(0, 1.3f, 0.05f), 0.8f, 0.13f, "White");
        }

        private static void ServiceLight(Transform root, string name, Vector3 position, float range, float intensity)
        {
            var light = RightGroup(root, name, position).gameObject.AddComponent<Light>();
            light.type = LightType.Point; light.range = range; light.intensity = intensity;
            light.color = new Color(1, 0.94f, 0.83f); light.shadows = LightShadows.None;
        }

        private static void SetupRightAreaViewpoints()
        {
            var root = GameObject.Find("Review viewpoints [select and align view]");
            if (root == null) return;
            foreach (string name in new[] { "21_Right_services_sequence", "22_Three_elevators", "23_Service_corridor", "24_Stairs_doors_LEFT", "25_Toilets_doors_RIGHT" })
            { var old = root.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Viewpoint(root.transform, "21_Right_services_sequence", new Vector3(-3, 2.2f, -18.5f), new Vector3(-17.4f, 2, -18.5f));
            Viewpoint(root.transform, "22_Three_elevators", new Vector3(-6.5f, 1.85f, LiftBankZ), new Vector3(-17.4f, 2, LiftBankZ));
            Viewpoint(root.transform, "23_Service_corridor", new Vector3(-16, 1.7f, ServiceCorridorZ), new Vector3(-26, 1.7f, ServiceCorridorZ));
            Viewpoint(root.transform, "24_Stairs_doors_LEFT", new Vector3(-21.5f, 1.65f, ServiceCorridorZ + 0.9f), new Vector3(-21.5f, 1.65f, ServiceCorridorZ - 1.4f));
            Viewpoint(root.transform, "25_Toilets_doors_RIGHT", new Vector3(-21.5f, 1.65f, ServiceCorridorZ - 0.9f), new Vector3(-21.5f, 1.65f, ServiceCorridorZ + 1.4f));
        }

        private static List<KeyValuePair<Vector3, Vector3>> ServiceExteriorWallFragments(float w, float d, float h)
        {
            var result = new List<KeyValuePair<Vector3, Vector3>>();
            float start = Mathf.Min(d, -ServiceCorridorZ - ServiceCorridorWidth / 2);
            float end = Mathf.Min(d, -ServiceCorridorZ + ServiceCorridorWidth / 2);
            float first = dimensions.facultyLiftsBehindWelcome ? 5.9f : 0;
            if (start > first) result.Add(new KeyValuePair<Vector3, Vector3>(new Vector3(-w / 2, h - 1.9f, -(start + first) / 2), new Vector3(0.42f, 3.7f, start - first)));
            if (d > end) result.Add(new KeyValuePair<Vector3, Vector3>(new Vector3(-w / 2, h - 1.9f, -(d + end) / 2), new Vector3(0.42f, 3.7f, d - end)));
            float header = h - 0.05f - ServicePortalHeight;
            if (end > start && header > 0) result.Add(new KeyValuePair<Vector3, Vector3>(new Vector3(-w / 2, ServicePortalHeight + header / 2, -(start + end) / 2), new Vector3(0.42f, header, end - start)));
            return result;
        }

        private static void TrimServiceExteriorGroundWall()
        {
            string path = Root + "/Prefabs/UEH_Exterior.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var filter = contents.transform.Find("02_Podium_structure/Ivory").GetComponent<MeshFilter>();
                var source = filter.sharedMesh;
                var vertices = source.vertices; var normals = source.normals; var uv = source.uv;
                Vector3 target = new Vector3(-dimensions.frontage / 2, dimensions.lobbyHeight - 1.9f, -dimensions.exteriorDepth / 2);
                int remove = -1;
                for (int start = 0; start + 24 <= vertices.Length; start += 24)
                {
                    Vector3 center = Vector3.zero;
                    for (int i = 0; i < 24; i++) center += vertices[start + i] / 24;
                    if (Vector3.Distance(center, target) < 0.02f) { remove = start; break; }
                }
                if (remove < 0) return;
                var positions = new List<Vector3>(); var keptNormals = new List<Vector3>();
                var keptUV = new List<Vector2>(); var triangles = new List<int>();
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (i >= remove && i < remove + 24) continue;
                    positions.Add(vertices[i]); keptNormals.Add(normals[i]); keptUV.Add(uv[i]);
                }
                var originalTriangles = source.triangles;
                for (int i = 0; i < originalTriangles.Length; i += 3)
                {
                    if (originalTriangles[i] >= remove && originalTriangles[i] < remove + 24) continue;
                    for (int j = 0; j < 3; j++) triangles.Add(originalTriangles[i + j] >= remove + 24 ? originalTriangles[i + j] - 24 : originalTriangles[i + j]);
                }
                var kept = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                kept.SetVertices(positions); kept.SetNormals(keptNormals); kept.SetUVs(0, keptUV); kept.SetTriangles(triangles, 0);
                var geometry = new Geometry(); geometry.Append(kept, Matrix4x4.identity);
                foreach (var fragment in ServiceExteriorWallFragments(dimensions.frontage, dimensions.exteriorDepth, dimensions.lobbyHeight)) geometry.Box(fragment.Key, fragment.Value);
                var mesh = geometry.ToMesh("RightServices_Exterior_podium_Ivory");
                UnityEngine.Object.DestroyImmediate(kept);
                string meshPath = Root + "/Meshes/RightServices_Exterior_podium_Ivory.asset";
                var saved = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if (saved == null) { AssetDatabase.CreateAsset(mesh, meshPath); saved = mesh; }
                else
                {
                    saved.Clear(); saved.indexFormat = mesh.indexFormat; saved.vertices = mesh.vertices;
                    saved.normals = mesh.normals; saved.uv = mesh.uv; saved.triangles = mesh.triangles; saved.tangents = mesh.tangents;
                    saved.RecalculateBounds(); saved.UploadMeshData(false); EditorUtility.SetDirty(saved);
                    UnityEngine.Object.DestroyImmediate(mesh);
                }
                filter.sharedMesh = saved;
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }
    }
}
#endif
