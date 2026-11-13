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
        public const float SinceGateWidth = 4.4f;
        public static float EntranceGlassZ(UEHDimensions s) => s.entranceGlassThroughColumns ? ExpandedEntranceZ(s) : 0;
        public static float LeftGateColumnX(UEHDimensions s) => (s.entranceGlassThroughColumns ? s.frontage / 2 - 0.3f + s.lobbyColumnWidth / 2 : ExpandedCornerX(s)) - s.leftWingInset;
        public static float SinceGateCenterX(UEHDimensions s) => (s.frontage * 0.125f + LeftGateColumnX(s)) / 2;

        [MenuItem("UEH/Campus B/20. Align gate glazing through columns and open Since entrance")]
        public static void AlignGlazingAndOpenSinceGate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.facultyColumnAtRearWall || !dimensions.compactLeftSequence)
                throw new InvalidOperationException("Build the current faculty and left-side layout first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[m.name] = m; }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.entranceGlassThroughColumns = true;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (string name in new[] { "02_Stone_columns", "03_Glazing_with_open_doorway", "Entrance_label" })
                { var old = contents.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                BuildExpandedColumns(contents.transform);
                BuildExpandedEntrance(contents.transform);
                // Extend the existing faculty-side closure only behind the end column;
                // the fixed glazing can now join its inside face without an opaque strip.
                BuildFacultyLiftEntrance(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupColumnGateViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_COLUMN_GATES_UPDATED Since column X=" + LeftGateColumnX(dimensions) + ", Z=" + EntranceGlassZ(dimensions) + "; three connected bays at column mid-depth; central " + dimensions.entryWidth + "m and Since " + SinceGateWidth + "m gates open and walkable.");
        }

        private static void BuildColumnAlignedEntrance(Transform lobby)
        {
            var entrance = RightGroup(lobby, "03_Glazing_with_open_doorway", new Vector3(0, 0, EntranceGlassZ(dimensions)));
            float halfColumn = dimensions.lobbyColumnWidth / 2, inner = dimensions.frontage * 0.125f;
            BuildGateBay(entrance, "BAY_RIGHT_fixed", RightCornerColumnX(dimensions) + halfColumn, -inner - halfColumn, false, 0, 0, true);
            BuildGateBay(entrance, "BAY_CENTER_walkable", -inner + halfColumn, inner - halfColumn, true, 0, dimensions.entryWidth, false);
            BuildGateBay(entrance, "BAY_SINCE_walkable", inner + halfColumn, LeftGateColumnX(dimensions) - halfColumn, true, SinceGateCenterX(dimensions), SinceGateWidth, false);
            MoveSinceEntrancePlanter(lobby);
        }

        private static void BuildGateBay(Transform entrance, string name, float start, float end, bool walkable, float doorCenter, float doorWidth, bool blinds)
        {
            var bay = RightGroup(entrance, name, Vector3.zero);
            RightGroup(bay, "Left_column_face", new Vector3(start, 2.1f, 0));
            RightGroup(bay, "Right_column_face", new Vector3(end, 2.1f, 0));
            var head = new Part(name + "_continuous_header", bay, "ColumnGates");
            head.Box("Silver", new Vector3((start + end) / 2, 3.24f, 0), new Vector3(end - start, 0.11f, 0.17f));
            head.Box("Metal", new Vector3((start + end) / 2, 4.37f, 0), new Vector3(end - start, 0.055f, 0.1f));
            head.Finish();
            if (!walkable) BuildGateFixedSegment(bay, name, start, end, blinds);
            else
            {
                float left = doorCenter - doorWidth / 2, right = doorCenter + doorWidth / 2;
                BuildGateFixedSegment(bay, name + "_left", start, left, false);
                BuildGateFixedSegment(bay, name + "_right", right, end, false);
                var transom = new Part(name + "_door_transom", bay, "ColumnGates");
                transom.Box("ClearGlass", new Vector3(doorCenter, 3.81f, 0), new Vector3(doorWidth - 0.05f, 1.08f, 0.025f), true);
                transom.Finish();
                var opening = RightGroup(bay, "Walkable_opening", new Vector3(doorCenter, 0, 0));
                foreach (int side in new[] { -1, 1 })
                {
                    var leaf = RightGroup(opening, side < 0 ? "Leaf_LEFT_open" : "Leaf_RIGHT_open", new Vector3(side * doorWidth / 2, 0, -0.07f));
                    leaf.localRotation = Quaternion.Euler(0, 90, 0);
                    float width = doorWidth / 2;
                    var p = new Part(name + (side < 0 ? "_left_open_leaf" : "_right_open_leaf"), leaf, "ColumnGates");
                    p.Box("ClearGlass", new Vector3(width / 2, 1.58f, 0), new Vector3(width - 0.05f, 3.06f, 0.025f), true);
                    foreach (float x in new[] { 0.025f, width - 0.025f })
                        p.Box("Silver", new Vector3(x, 1.58f, 0), new Vector3(0.035f, 3.14f, 0.055f));
                    foreach (float y in new[] { 0.025f, 3.135f })
                        p.Box("Silver", new Vector3(width / 2, y, 0), new Vector3(width, 0.035f, 0.055f));
                    p.Box("Silver", new Vector3(width - 0.2f, 1.45f, 0.05f), new Vector3(0.035f, 0.6f, 0.055f));
                    p.Finish();
                }
            }
        }

        private static void BuildGateFixedSegment(Transform bay, string id, float start, float end, bool blinds)
        {
            float span = end - start; if (span <= 0.01f) return;
            int count = Mathf.CeilToInt(span / 1.35f);
            var frame = new Part(id + "_fixed_frame", bay, "ColumnGates");
            for (int i = 0; i < count; i++)
            {
                float a = start + span * i / count, b = start + span * (i + 1) / count, x = (a + b) / 2;
                var pane = RightGroup(bay, "Entrance_fixed_pane_" + id + "_" + i, new Vector3(x, 0, 0));
                var p = new Part("Pane_" + id + "_" + i, pane, "ColumnGates");
                p.Box("ClearGlass", new Vector3(0, 1.64f, 0), new Vector3(b - a - 0.055f, 3.1f, 0.025f), true);
                p.Box("ClearGlass", new Vector3(0, 3.81f, 0), new Vector3(b - a - 0.055f, 1.08f, 0.025f), true);
                p.Finish();
                frame.Box("Metal", new Vector3(a, 2.2f, 0), new Vector3(0.055f, 4.4f, 0.1f), true);
                frame.Box("Metal", new Vector3(x, 0.075f, 0), new Vector3(b - a, 0.055f, 0.1f));
                if (blinds && i >= 2 && i <= 4)
                {
                    frame.Box("Ivory", new Vector3(x, 2.6f, -0.065f), new Vector3(b - a - 0.1f, 1.15f, 0.022f));
                    frame.Box("Silver", new Vector3(x, 3.2f, -0.065f), new Vector3(b - a - 0.09f, 0.06f, 0.06f));
                    frame.Box("Metal", new Vector3(x, 2.015f, -0.065f), new Vector3(b - a - 0.1f, 0.025f, 0.03f));
                }
            }
            frame.Box("Metal", new Vector3(end, 2.2f, 0), new Vector3(0.055f, 4.4f, 0.1f), true);
            frame.Finish();
        }

        private static void MoveSinceEntrancePlanter(Transform lobby)
        {
            foreach (Transform child in lobby)
            {
                if (!child.name.StartsWith("08_Entrance_plant_")) continue;
                var white = child.Find("White"); if (white == null) continue;
                var bounds = white.GetComponent<MeshRenderer>().bounds;
                if (bounds.center.x < 0) continue;
                float targetX = dimensions.leftWingInset > 0 ? LeftFacadeX(dimensions) - 0.55f : 16.1f;
                child.position += new Vector3(targetX - bounds.center.x, 0, -2.8f - bounds.center.z);
            }
        }

        private static void SetupColumnGateViewpoints()
        {
            var views = GameObject.Find("Review viewpoints [select and align view]"); if (views == null) return;
            foreach (string name in new[] { "51_Column_gates_exterior", "52_Column_gates_from_lobby", "53_Since_gate_open_walkway", "54_Glazing_mid_column_joint" })
            { var old = views.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Viewpoint(views.transform, "51_Column_gates_exterior", new Vector3(27, 2, 9), new Vector3(7, 2, -1.12f));
            Viewpoint(views.transform, "52_Column_gates_from_lobby", new Vector3(0, 1.7f, -20), new Vector3(0, 2, -1.12f));
            Viewpoint(views.transform, "53_Since_gate_open_walkway", new Vector3(SinceGateCenterX(dimensions), 1.65f, -7), new Vector3(SinceGateCenterX(dimensions), 1.8f, 2));
            Viewpoint(views.transform, "54_Glazing_mid_column_joint", new Vector3(14.8f, 1.8f, 4), new Vector3(18.3f, 2, -1.12f));
        }
    }
}
#endif
