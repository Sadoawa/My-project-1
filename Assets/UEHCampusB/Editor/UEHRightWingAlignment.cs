#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UEH.CampusB.Editor
{
    public static partial class UEHCampusBuilder
    {
        public static float RightFacadeX(UEHDimensions s) => -s.frontage / 2 + 0.6f + s.rightWingInset;
        public static float RightCornerColumnX(UEHDimensions s) => s.facultyColumnAtRearWall
            ? RightFacadeX(s) - FacultyRoomDepth + s.lobbyColumnWidth / 2
            : -ExpandedCornerX(s) + (s.facultyLiftsBehindWelcome ? 0 : s.rightWingInset);
        public static float RightEntranceGlassEdgeX(UEHDimensions s) => s.entranceGlassThroughColumns ? RightCornerColumnX(s) + s.lobbyColumnWidth / 2 : s.facultyLiftsBehindWelcome ? -s.frontage / 2 + 0.3f : RightFacadeX(s) - 0.3f;

        [MenuItem("UEH/Campus B/16. Align right wing entrance glazing and faculty lift")]
        public static void AlignRightWing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.rightGlassOffice) throw new InvalidOperationException("Build right services/office first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[m.name] = m;
            }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.rightWingInset = 4;
            dimensions.facultyLiftEntrance = true;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var welcome = contents.transform.Find("05_WELCOME_RIGHT_from_gate");
                float delta = RightFacadeX(dimensions) - welcome.localPosition.x;
                welcome.localPosition = new Vector3(RightFacadeX(dimensions), 0, -7.5f);
                ShiftLooseRightProps(contents.transform, delta);
                var areas = contents.transform.Find("09_Four_expansion_areas");
                foreach (Transform bay in areas)
                    if (bay.name.StartsWith("RIGHT_")) bay.localPosition = new Vector3(RightFacadeX(dimensions), 0, bay.localPosition.z);
                PopulateRightLiftsAndServices(areas);
                PopulateRightGlassOffice(areas);
                foreach (string name in new[] { "01_Floor_and_walls", "02_Stone_columns", "03_Glazing_with_open_doorway", "Entrance_label", "10_Lift_side_glass_railing", "12_Faculty_lift_entrance" })
                { var old = contents.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                BuildExpandedShell(contents.transform);
                BuildExpandedColumns(contents.transform);
                BuildExpandedEntrance(contents.transform);
                BuildLiftSideGlassRailing(contents.transform);
                BuildFacultyLiftEntrance(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupAlignedRightViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_RIGHT_WING_ALIGNED inset=4m facadeX=" + RightFacadeX(dimensions) + "; narrow entrance mullions/transoms/blinds; recessed faculty lift with three access pedestals; L railing reconnected.");
        }

        private static void ShiftLooseRightProps(Transform root, float delta)
        {
            foreach (Transform child in root)
                if (child.name.StartsWith("Lounge_seat_") || child.name.StartsWith("08_Indoor_plant_") || child.name.StartsWith("08_Entrance_plant_"))
                {
                    var renderer = child.GetComponentInChildren<MeshRenderer>();
                    if (renderer != null && renderer.bounds.center.x < 0) child.localPosition += Vector3.right * delta;
                }
        }

        // Only extend private slabs beyond the main floor edge. Interior portions already
        // share the main floor, so moving the wing creates no coplanar overlapping slabs.
        private static void RightRoomFloor(Part part, Transform room, float width, float rearDepth, float localX = 0)
        {
            float edgeDepth = Mathf.Max(0, dimensions.frontage / 2 + room.position.x);
            if (rearDepth > edgeDepth + 0.001f)
                part.Box("Floor", new Vector3(localX, -0.14f, -(rearDepth + edgeDepth) / 2), new Vector3(width, 0.28f, rearDepth - edgeDepth), true);
        }

        private static void BuildReferenceEntrance(Transform root)
        {
            if (dimensions.entranceGlassThroughColumns) { BuildColumnAlignedEntrance(root); return; }
            float left = dimensions.frontage / 2, right = RightEntranceGlassEdgeX(dimensions), halfEntry = dimensions.entryWidth / 2;
            var frame = new Part("03_Glazing_with_open_doorway", root, "RightWing");
            foreach (int side in new[] { -1, 1 })
            {
                float edge = side < 0 ? -right : left;
                int count = Mathf.CeilToInt((edge - halfEntry) / 1.35f);
                float span = (edge - halfEntry) / count;
                for (int i = 0; i < count; i++)
                {
                    float x = side * (halfEntry + (i + 0.5f) * span);
                    var pane = RightGroup(root, "Entrance_fixed_pane_" + side + "_" + i, new Vector3(x, 0, 0));
                    var glass = new Part("Pane_" + side + "_" + i, pane, "RightWing");
                    glass.Box("ClearGlass", new Vector3(0, 1.69f, 0), new Vector3(span - 0.055f, 3.2f, 0.025f), true);
                    glass.Box("ClearGlass", new Vector3(0, 3.83f, 0), new Vector3(span - 0.055f, 1.02f, 0.025f), true);
                    glass.Finish();
                    // Parent panes beneath the entrance group after Finish below.
                    frame.Box("Metal", new Vector3(side * (halfEntry + i * span), 2.2f, 0), new Vector3(0.055f, 4.4f, 0.1f), true);
                    foreach (float y in new[] { 0.07f, 3.31f, 4.37f })
                        frame.Box("Metal", new Vector3(x, y, 0), new Vector3(span, 0.055f, 0.1f));
                    if (side < 0 && i >= count - 4 && i <= count - 2)
                    {
                        frame.Box("Ivory", new Vector3(x, 2.64f, -0.065f), new Vector3(span - 0.1f, 1.23f, 0.022f));
                        frame.Cylinder("Silver", new Vector3(x, 3.27f, -0.065f), new Vector3(0.06f, 0.03f, 0.06f));
                        frame.Box("Metal", new Vector3(x, 2.015f, -0.065f), new Vector3(span - 0.1f, 0.025f, 0.03f));
                    }
                }
                frame.Box("Metal", new Vector3(side * edge, 2.2f, 0), new Vector3(0.055f, 4.4f, 0.1f));
                frame.Box("ClearGlass", new Vector3(side * (halfEntry + 0.65f), 1.65f, -0.12f), new Vector3(1.2f, 3.2f, 0.03f));
                frame.Box("Silver", new Vector3(side * (halfEntry + 0.18f), 1.5f, -0.19f), new Vector3(0.045f, 0.7f, 0.045f));
            }
            frame.Box("Silver", new Vector3(0, 3.31f, 0), new Vector3(dimensions.entryWidth, 0.14f, 0.25f));
            frame.Box("ClearGlass", new Vector3(0, 3.85f, 0), new Vector3(dimensions.entryWidth, 1.02f, 0.025f));
            // Close the front of the newly inset service wing within the existing building shell.
            frame.Box("Stone", new Vector3((right - dimensions.frontage / 2) / 2, 2.2f, -0.15f), new Vector3(dimensions.frontage / 2 + right, 4.4f, 0.3f), true);
            frame.Finish();
            var entrance = root.Find("03_Glazing_with_open_doorway");
            var panes = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in root) if (child.name.StartsWith("Entrance_fixed_pane_")) panes.Add(child);
            foreach (var pane in panes) pane.SetParent(entrance, true);
            Text(root, "Entrance_label", "UEH  /  CƠ SỞ B", new Vector3(0, 3.75f, 0.11f), 4.3f, 0.3f, "Teal");
        }

        private static void BuildFacultyLiftEntrance(Transform lobbyRoot)
        {
            if (!dimensions.facultyLiftEntrance) return;
            if (dimensions.facultyLiftsBehindWelcome) { BuildFacultyLiftsBehindWelcome(lobbyRoot); return; }
            var old = lobbyRoot.Find("12_Faculty_lift_entrance");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var root = RightGroup(lobbyRoot, "12_Faculty_lift_entrance", new Vector3(RightFacadeX(dimensions), 0, -4));
            root.localRotation = Quaternion.Euler(0, 90, 0);
            var shell = new Part("Faculty_lift_alcove", root, "Faculty");
            // 3.8m wide alcove, set 3m behind the facade; all furniture stays behind it.
            foreach (float x in new[] { -1.95f, 1.95f })
                shell.Box("Stone", new Vector3(x, 1.8f, -1.5f), new Vector3(0.1f, 3.6f, 3), true);
            shell.Box("White", new Vector3(0, 3.55f, -1.5f), new Vector3(3.9f, 0.1f, 3), true);
            shell.Box("OfficeCharcoal", new Vector3(0, 1.8f, -3.05f), new Vector3(3.9f, 3.6f, 0.1f), true);
            foreach (float x in new[] { -1.02f, 1.02f })
                shell.Box("Stone", new Vector3(x, 1.4f, -2.97f), new Vector3(0.15f, 2.8f, 0.14f));
            shell.Box("Stone", new Vector3(0, 2.72f, -2.97f), new Vector3(2.19f, 0.15f, 0.14f));
            shell.Box("Silver", new Vector3(0, 0.015f, -2.8f), new Vector3(2.1f, 0.03f, 0.35f));
            shell.Finish();
            for (int i = 0; i < 2; i++)
            {
                var leaf = RightGroup(root, "Faculty_lift_leaf_" + i, new Vector3(i == 0 ? -0.46f : 0.46f, 0, -2.89f));
                var p = new Part("Faculty_leaf_" + i, leaf, "Faculty");
                p.Box("LiftStainless", new Vector3(0, 1.34f, 0), new Vector3(0.91f, 2.62f, 0.06f), true);
                for (int line = 0; line < 5; line++) p.Box("LiftGrain", new Vector3(-0.36f + line * 0.18f, 1.34f, 0.031f), new Vector3(0.002f, 2.5f, 0.002f));
                p.Finish();
            }
            Text(root, "Faculty_lift_sign", "THANG MÁY GIẢNG VIÊN", new Vector3(0, 3.07f, -2.86f), 2.9f, 0.2f, "Silver");
            Text(root, "Faculty_floor_indicator", "0", new Vector3(0, 2.84f, -2.86f), 0.12f, 0.15f, "CoolLight");
            var call = new Part("Faculty_call_panel", root, "Faculty");
            call.Box("LiftStainless", new Vector3(-1.3f, 1.22f, -2.87f), new Vector3(0.18f, 0.4f, 0.06f));
            call.Box("CoolLight", new Vector3(-1.3f, 1.22f, -2.83f), new Vector3(0.065f, 0.065f, 0.02f));
            call.Finish();
            for (int i = 0; i < 3; i++)
            {
                var gate = RightGroup(root, "Access_pedestal_" + (i + 1), new Vector3(-1.3f + i * 1.3f, 0, -0.5f));
                var g = new Part("Faculty_access_" + i, gate, "Faculty");
                g.Box("LiftStainless", new Vector3(0, 0.5f, 0), new Vector3(0.28f, 1, 0.72f), true);
                g.Box("Dark", new Vector3(0, 1.015f, 0.12f), new Vector3(0.2f, 0.035f, 0.24f));
                g.Box("Teal", new Vector3(0, 1.037f, 0.12f), new Vector3(0.09f, 0.008f, 0.14f));
                g.Cylinder("CoolLight", new Vector3(0, 0.68f, 0.368f), new Vector3(0.08f, 0.01f, 0.08f));
                g.Finish();
            }
            ServiceLight(root, "Faculty alcove fill", new Vector3(0, 3.1f, -1.5f), 4, 1.2f);
            var marker = lobbyRoot.Find("05_WELCOME_RIGHT_from_gate/05_Reception_and_welcome_wall/03_Clear_entrance_pocket");
            if (marker != null)
            {
                var reserved = marker.GetComponent<UEHReservedSpace>(); reserved.furnished = true;
                reserved.purpose = "Hốc thang máy giảng viên 3.8 x 3m, 3 trụ kiểm soát. Cửa thang máy mô hình tĩnh; kích thước ước lượng.";
                marker.position = root.TransformPoint(new Vector3(0, 1.8f, -1.5f)); reserved.size = new Vector3(3.8f, 3.6f, 3);
            }
        }

        private static void SetupAlignedRightViewpoints()
        {
            var views = GameObject.Find("Review viewpoints [select and align view]"); if (views == null) return;
            // All these views originally described the right facade at X=-17.4.
            foreach (Transform view in views.transform)
            {
                string number = view.name.Substring(0, 2);
                if (Array.IndexOf(new[] { "04", "09", "10", "14", "21", "22", "23", "24", "25", "26", "27", "28", "29", "35", "36", "37" }, number) >= 0)
                {
                    // Idempotent anchor saved on the viewpoint to avoid cumulative movement.
                    var anchor = view.GetComponent<UEHRightViewAnchor>();
                    if (anchor == null) { anchor = view.gameObject.AddComponent<UEHRightViewAnchor>(); anchor.originalPosition = view.position; }
                    view.position = anchor.originalPosition + Vector3.right * dimensions.rightWingInset;
                }
            }
            foreach (string name in new[] { "39_Right_entrance_and_faculty_lift", "40_Faculty_lift_alcove", "41_Right_wing_alignment", "42_Entrance_glazing_inside" })
            { var old = views.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Viewpoint(views.transform, "39_Right_entrance_and_faculty_lift", new Vector3(-4.2f, 1.7f, -12.8f), new Vector3(-13.6f, 1.9f, -5.8f));
            Viewpoint(views.transform, "40_Faculty_lift_alcove", new Vector3(-10.5f, 1.65f, -4), new Vector3(-16.3f, 1.75f, -4));
            Viewpoint(views.transform, "41_Right_wing_alignment", new Vector3(1, 2.1f, -14), new Vector3(-13.4f, 1.9f, -25));
            Viewpoint(views.transform, "42_Entrance_glazing_inside", new Vector3(-6.5f, 1.65f, -9), new Vector3(-8, 2.1f, 0));
            if (dimensions.facultyLiftsBehindWelcome) SetupFacultyReturnViewpoints();
            if (dimensions.entranceGlassThroughColumns) SetupColumnGateViewpoints();
        }
    }
}
#endif
