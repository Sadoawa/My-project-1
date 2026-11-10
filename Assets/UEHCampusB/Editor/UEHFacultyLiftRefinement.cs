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
        public const float FacultyRoomDepth = 7.2f;
        public const float FacultyFirstLiftDepth = 2.05f;
        public const float FacultySecondLiftDepth = 5.35f;

        [MenuItem("UEH/Campus B/18. Move faculty stone column to rear wall")]
        public static void MoveFacultyColumnToRearWall()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.facultyLiftsBehindWelcome) throw new InvalidOperationException("Build the two faculty lifts first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[m.name] = m;
            }
            dimensions.facultyColumnAtRearWall = true;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var old = contents.transform.Find("02_Stone_columns");
                if (old == null) throw new InvalidOperationException("Missing stone columns.");
                UnityEngine.Object.DestroyImmediate(old.gameObject);
                BuildExpandedColumns(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupFacultyReturnViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_FACULTY_COLUMN_REAR_UPDATED Column body/base/capital/collision moved from X=-16.7 to X=" + RightCornerColumnX(dimensions) + "; rear body face meets faculty rear wall; Z remains " + ExpandedEntranceZ(dimensions) + ". Other five columns and faculty geometry retained.");
        }

        [MenuItem("UEH/Campus B/17. Two faculty lifts behind WELCOME and recessed stone column")]
        public static void RefineFacultyLifts()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.facultyLiftEntrance) throw new InvalidOperationException("Build the faculty entrance first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[m.name] = m;
            }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.facultyLiftsBehindWelcome = true;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (string name in new[] { "01_Floor_and_walls", "02_Stone_columns", "03_Glazing_with_open_doorway", "Entrance_label" })
                { var old = contents.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                BuildExpandedShell(contents.transform);
                BuildExpandedColumns(contents.transform);
                BuildExpandedEntrance(contents.transform);
                BuildFacultyLiftEntrance(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            TrimFacultyExteriorGroundWall();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupFacultyReturnViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_FACULTY_RETURN_UPDATED Two lifts behind WELCOME, on left after entry; 7.2m deep shared vestibule, 3 access pedestals; stone corner column moved 4m away from lobby to X=" + RightCornerColumnX(dimensions) + "; entrance glazing extended to match.");
        }

        private static void BuildFacultyLiftsBehindWelcome(Transform lobbyRoot)
        {
            var old = lobbyRoot.Find("12_Faculty_lift_entrance");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var root = RightGroup(lobbyRoot, "12_Faculty_lift_entrance", new Vector3(RightFacadeX(dimensions), 0, -4));
            root.localRotation = Quaternion.Euler(0, 90, 0);
            // Entry direction is local -Z (world -X); the visitor's left is local +X
            // (world -Z). Both lift faces are therefore on the return behind WELCOME.
            var shell = new Part("Faculty_return_room", root, "FacultyReturn");
            shell.Box("OfficeCharcoal", new Vector3(2.05f, 1.85f, -(FacultyRoomDepth + 0.2f) / 2), new Vector3(0.3f, 3.7f, FacultyRoomDepth + 0.2f), true);
            shell.Box("White", new Vector3(2.05f, 4.05f, -(FacultyRoomDepth + 0.2f) / 2), new Vector3(0.3f, 0.7f, FacultyRoomDepth + 0.2f), true);
            shell.Box("Ivory", new Vector3(-1, 2.2f, -FacultyRoomDepth - 0.1f), new Vector3(6.2f, 4.4f, 0.2f), true);
            shell.Box("White", new Vector3(-1, 4.3f, -FacultyRoomDepth / 2), new Vector3(6, 0.2f, FacultyRoomDepth), true);
            RightRoomFloor(shell, root, 6, FacultyRoomDepth + 0.2f, -1);
            // Front glazing is shared with the lobby. Only the small part beyond its
            // outer edge needs a solid closure; no opaque side pier blocks the glass.
            float edgeDepth = RightFacadeX(dimensions) - RightEntranceGlassEdgeX(dimensions);
            float closure = FacultyRoomDepth - edgeDepth;
            shell.Box("Stone", new Vector3(-3.95f, 2.2f, -(FacultyRoomDepth + edgeDepth) / 2), new Vector3(0.1f, 4.4f, closure), true);
            foreach (float z in new[] { -0.4f, -3.7f, -7f })
                shell.Box("Stone", new Vector3(1.85f, 1.85f, z), new Vector3(0.12f, 3.7f, 0.3f));
            shell.Finish();
            BuildFacultyReturnLift(root, "GV1", FacultyFirstLiftDepth);
            BuildFacultyReturnLift(root, "GV2", FacultySecondLiftDepth);
            for (int i = 0; i < 3; i++)
            {
                var gate = RightGroup(root, "Access_pedestal_" + (i + 1), new Vector3(-1.3f + i * 1.3f, 0, -0.5f));
                var g = new Part("Access_" + i, gate, "FacultyReturn");
                g.Box("LiftStainless", new Vector3(0, 0.5f, 0), new Vector3(0.28f, 1, 0.72f), true);
                g.Box("Dark", new Vector3(0, 1.015f, 0.12f), new Vector3(0.2f, 0.035f, 0.24f));
                g.Box("Teal", new Vector3(0, 1.037f, 0.12f), new Vector3(0.09f, 0.008f, 0.14f));
                g.Box("CoolLight", new Vector3(0, 0.7f, 0.367f), new Vector3(0.08f, 0.08f, 0.01f));
                g.Finish();
            }
            // A small sign gives the left turn without exposing a lift door head-on.
            var sign = new Part("Entry_direction_sign", root, "FacultyReturn");
            sign.Box("Teal", new Vector3(-0.2f, 3.6f, -0.05f), new Vector3(2.9f, 0.3f, 0.06f)); sign.Finish();
            Text(root, "Faculty_entry_direction", "<  THANG MÁY GIẢNG VIÊN", new Vector3(-0.2f, 3.6f, 0), 2.65f, 0.17f, "White");
            var lamps = new Part("Faculty_return_downlights", root, "FacultyReturn");
            foreach (float z in new[] { -1.6f, -4, -6.3f })
            {
                lamps.Cylinder("Metal", new Vector3(-0.4f, 4.17f, z), new Vector3(0.15f, 0.03f, 0.15f));
                lamps.Cylinder("WarmLight", new Vector3(-0.4f, 4.13f, z), new Vector3(0.12f, 0.01f, 0.12f));
                ServiceLight(root, "Faculty vestibule light " + z, new Vector3(-0.4f, 3.7f, z), 4.5f, 0.85f);
            }
            lamps.Finish();
            var marker = lobbyRoot.Find("05_WELCOME_RIGHT_from_gate/05_Reception_and_welcome_wall/03_Clear_entrance_pocket");
            if (marker != null)
            {
                var reserved = marker.GetComponent<UEHReservedSpace>(); reserved.furnished = true;
                reserved.purpose = "Hai thang máy giảng viên sau WELCOME; đi vào rồi nhìn trái. Kích thước ước lượng, cửa tĩnh.";
                marker.position = root.TransformPoint(new Vector3(-1, 2.1f, -3.6f)); reserved.size = new Vector3(6, 4.2f, 7.2f);
            }
        }

        private static void BuildFacultyReturnLift(Transform root, string id, float depth)
        {
            var lift = RightGroup(root, "Faculty_lift_" + id, new Vector3(1.82f, 0, -depth));
            lift.localRotation = Quaternion.Euler(0, -90, 0);
            var frame = new Part(id + "_frame_and_call", lift, "FacultyReturn");
            foreach (float x in new[] { -1.02f, 1.02f })
                frame.Box("LiftStainless", new Vector3(x, 1.4f, 0), new Vector3(0.14f, 2.8f, 0.12f));
            frame.Box("LiftStainless", new Vector3(0, 2.74f, 0), new Vector3(2.18f, 0.12f, 0.12f));
            frame.Box("Silver", new Vector3(0, 0.015f, 0.14f), new Vector3(2.1f, 0.03f, 0.35f));
            frame.Box("LiftStainless", new Vector3(-1.27f, 1.2f, 0.02f), new Vector3(0.18f, 0.4f, 0.06f));
            frame.Box("CoolLight", new Vector3(-1.27f, 1.2f, 0.06f), new Vector3(0.065f, 0.065f, 0.02f));
            frame.Finish();
            for (int side = 0; side < 2; side++)
            {
                var leaf = RightGroup(lift, "Sliding_leaf_" + side, new Vector3(side == 0 ? -0.46f : 0.46f, 0, 0.04f));
                var p = new Part(id + "_leaf_" + side, leaf, "FacultyReturn");
                p.Box("LiftStainless", new Vector3(0, 1.34f, 0), new Vector3(0.91f, 2.62f, 0.06f), true);
                for (int line = 0; line < 5; line++) p.Box("LiftGrain", new Vector3(-0.36f + line * 0.18f, 1.34f, 0.031f), new Vector3(0.002f, 2.5f, 0.002f));
                p.Finish();
            }
            Text(lift, "Faculty_lift_identifier", id, new Vector3(0, 3.17f, 0.07f), 0.6f, 0.3f, "Gold");
            Text(lift, "Faculty_floor_indicator", "0", new Vector3(0, 2.94f, 0.07f), 0.12f, 0.15f, "CoolLight");
        }

        private static void SetupFacultyReturnViewpoints()
        {
            var views = GameObject.Find("Review viewpoints [select and align view]"); if (views == null) return;
            foreach (string name in new[] { "43_Faculty_entry_clear_glass", "44_Two_faculty_lifts_on_LEFT", "45_Faculty_behind_WELCOME_plan" })
            { var old = views.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Viewpoint(views.transform, "43_Faculty_entry_clear_glass", new Vector3(-8.2f, 1.65f, -10.7f), new Vector3(-13.7f, 1.9f, -3.4f));
            Viewpoint(views.transform, "44_Two_faculty_lifts_on_LEFT", new Vector3(-17, 1.7f, -2.8f), new Vector3(-17, 1.9f, -6));
            Viewpoint(views.transform, "45_Faculty_behind_WELCOME_plan", new Vector3(-14.5f, 1.7f, -2.8f), new Vector3(-19.3f, 1.8f, -5.8f));
            if (dimensions.facultyColumnAtRearWall)
            {
                var old = views.transform.Find("46_Faculty_column_at_rear_wall");
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                Viewpoint(views.transform, "46_Faculty_column_at_rear_wall", new Vector3(-15.7f, 1.7f, -3.6f), new Vector3(-19.4f, 1.9f, -1.3f));
            }
        }

        // Strip only ground-level negative-X cladding chunks. The office recess,
        // upper floors and all other facade pieces retain their original mesh data.
        private static void TrimFacultyExteriorGroundWall()
        {
            string path = Root + "/Prefabs/UEH_Exterior.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var filter = contents.transform.Find("02_Podium_structure/Ivory").GetComponent<MeshFilter>();
                var source = filter.sharedMesh; var vertices = source.vertices;
                var positions = new List<Vector3>(); var normals = new List<Vector3>(); var uv = new List<Vector2>();
                var remap = new int[vertices.Length]; var sourceNormals = source.normals; var sourceUV = source.uv;
                for (int start = 0; start + 24 <= vertices.Length; start += 24)
                {
                    var bounds = new Bounds(vertices[start], Vector3.zero);
                    for (int i = 1; i < 24; i++) bounds.Encapsulate(vertices[start + i]);
                    bool remove = Mathf.Abs(bounds.center.x + dimensions.frontage / 2) < 0.02f && Mathf.Abs(bounds.size.x - 0.42f) < 0.02f && bounds.max.y <= dimensions.lobbyHeight + 0.01f;
                    for (int i = start; i < start + 24; i++)
                    {
                        remap[i] = remove ? -1 : positions.Count;
                        if (!remove) { positions.Add(vertices[i]); normals.Add(sourceNormals[i]); uv.Add(sourceUV[i]); }
                    }
                }
                var triangles = new List<int>(); var original = source.triangles;
                for (int i = 0; i < original.Length; i += 3)
                    if (remap[original[i]] >= 0) for (int j = 0; j < 3; j++) triangles.Add(remap[original[i + j]]);
                var kept = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                kept.SetVertices(positions); kept.SetNormals(normals); kept.SetUVs(0, uv); kept.SetTriangles(triangles, 0);
                var geometry = new Geometry(); geometry.Append(kept, Matrix4x4.identity);
                foreach (var fragment in ServiceExteriorWallFragments(dimensions.frontage, dimensions.exteriorDepth, dimensions.lobbyHeight)) geometry.Box(fragment.Key, fragment.Value);
                var mesh = geometry.ToMesh("FacultyReturn_Exterior_podium_Ivory"); UnityEngine.Object.DestroyImmediate(kept);
                string meshPath = Root + "/Meshes/FacultyReturn_Exterior_podium_Ivory.asset";
                var saved = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if (saved == null) { AssetDatabase.CreateAsset(mesh, meshPath); saved = mesh; }
                else { EditorUtility.CopySerialized(mesh, saved); UnityEngine.Object.DestroyImmediate(mesh); saved.UploadMeshData(false); EditorUtility.SetDirty(saved); }
                filter.sharedMesh = saved;
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }
    }
}
#endif
