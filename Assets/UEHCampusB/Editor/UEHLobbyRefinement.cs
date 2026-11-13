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
        public static float ExpandedColumnZ(UEHDimensions settings) => settings.refinedLobbyLayout ? -settings.lobbyDepth / 2 : RetainedColumnZ;
        public static float ExpandedEntranceZ(UEHDimensions settings) => settings.refinedLobbyLayout ? -(settings.lobbyColumnWidth + 0.24f) / 2 : EntranceColumnCenterZ;
        public static float ExpandedCornerX(UEHDimensions settings) => settings.refinedLobbyLayout ? settings.frontage / 2 - 0.3f - settings.lobbyColumnWidth / 2 : EntranceCornerColumnX(settings.frontage);
        public static Vector3 RailingWallAnchor(UEHDimensions settings) => new Vector3(RightFacadeX(settings) + 0.08f, 0.6f, RightLiftBankZ(settings) + 5.75f);
        public static Vector3 RailingColumnAnchor(UEHDimensions settings) => new Vector3(
            -settings.frontage * 0.125f - (settings.compactRightSequence ? 0 : settings.lobbyColumnWidth / 2), 0.6f,
            ExpandedColumnZ(settings) + settings.lobbyColumnWidth / 2);
        public static Vector3 RailingElbow(UEHDimensions settings) => new Vector3(
            RailingColumnAnchor(settings).x, 0.6f, RailingWallAnchor(settings).z);
        public static int ConnectedRailingPanelCount(UEHDimensions settings)
        {
            var elbow = RailingElbow(settings);
            return Mathf.CeilToInt(Vector3.Distance(RailingColumnAnchor(settings), elbow) / 1.25f)
                + Mathf.CeilToInt(Vector3.Distance(elbow, RailingWallAnchor(settings)) / 1.25f);
        }

        [MenuItem("UEH/Campus B/15. Center timber only wider columns and right angle railing")]
        public static void RefineCeilingColumnsAndLRailing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before refining the ceiling, columns and railing.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.refinedLobbyLayout || !dimensions.rightLiftsAndServices)
                throw new InvalidOperationException("Build the refined lobby and lift/toilet areas first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                materials[material.name] = material;
            }
            dimensions.centerTimberOnly = true;
            dimensions.lobbyColumnWidth = 2;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (string name in new[] { "02_Stone_columns", "04_Timber_ceiling_and_downlights", "10_Lift_side_glass_railing" })
                { var old = contents.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                BuildExpandedColumns(contents.transform);
                BuildExpandedCeiling(contents.transform);
                BuildLiftSideGlassRailing(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupRightAngleRailingViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_CEILING_COLUMNS_L_UPDATED Removed both side timber strips; central strip/chandeliers/30 round lamps retained. Six columns 2x2m, center positions retained and front columns realigned to glass/corners. Two railing segments meet at 90 degrees, connecting column front corner to lift-C/toilet pier.");
        }

        [MenuItem("UEH/Campus B/14. Connect glass railing to lift and toilet dividing wall")]
        public static void ConnectRailingToDividingWall()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before editing the railing.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.refinedLobbyLayout || !dimensions.rightLiftsAndServices)
                throw new InvalidOperationException("Refine the lobby and build the lifts/services before connecting the railing.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                materials[material.name] = material;
            }
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var old = contents.transform.Find("10_Lift_side_glass_railing");
                if (old == null) throw new InvalidOperationException("Missing lift-side railing.");
                UnityEngine.Object.DestroyImmediate(old.gameObject);
                BuildLiftSideGlassRailing(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupRailingWallViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_RAILING_WALL_CONNECTED Right-angle glass railing connects the central column front corner to the stone pier between lift C and the toilet corridor. Wall anchor=" + RailingWallAnchor(dimensions));
        }

        [MenuItem("UEH/Campus B/13. Open academic hall and refine columns lights railing")]
        public static void RefineLobbyLayout()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before refining the lobby.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.expandedLobby)
                throw new InvalidOperationException("Expand the lobby before this refinement.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                materials[material.name] = material;
            }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.refinedLobbyLayout = true;
            dimensions.lobbyColumnWidth = Mathf.Max(1.6f, dimensions.lobbyColumnWidth);
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (string name in new[] { "01_Floor_and_walls", "02_Stone_columns", "07_Inner_passage_inferred", "Inner_direction", "15_Rear_lobby_closure", "10_Lift_side_glass_railing", "11_Central_round_spotlights" })
                { var old = contents.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                BuildExpandedShell(contents.transform);
                BuildExpandedColumns(contents.transform);
                BuildOpenAcademicGeometry(contents.transform, dimensions.lobbyDepth, dimensions.lobbyHeight);
                if (!dimensions.extendedGlassOfficeAndClosedRear)
                    Text(contents.transform, "Inner_direction", "KHÔNG GIAN HỌC THUẬT", new Vector3(0, 3.95f, -dimensions.lobbyDepth + 0.3f), 5.4f, 0.28f, "Teal");
                else RemoveAcademicConnection(contents.transform);
                BuildLiftSideGlassRailing(contents.transform);
                BuildCentralRoundSpotlights(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupLobbyRefinementViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_LOBBY_REFINED Full-width academic connection; six columns " + dimensions.lobbyColumnWidth + "m wide with central pair at half lobby depth; right-angle glass railing and central round spotlights retained.");
        }

        private static void BuildOpenAcademicGeometry(Transform root, float d, float h)
        {
            if (dimensions.extendedGlassOfficeAndClosedRear)
            {
                BuildRearLobbyClosure(root, d, h);
                return;
            }
            float w = dimensions.frontage;
            var hall = new Part("07_Inner_passage_inferred", root, "LobbyRefine");
            hall.Box("Floor", new Vector3(0, -0.14f, -d - 3), new Vector3(w, 0.28f, 6), true);
            foreach (int side in new[] { -1, 1 })
                hall.Box("Ivory", new Vector3(side * (w / 2 - 0.15f), h / 2, -d - 3), new Vector3(0.3f, h, 6), true);
            hall.Box("White", new Vector3(0, h - 0.1f, -d - 3), new Vector3(w, 0.2f, 6));
            // A suspended sign at ceiling height replaces the former low stone header.
            hall.Box("White", new Vector3(0, 3.96f, -d + 0.13f), new Vector3(6.2f, 0.45f, 0.14f));
            hall.Box("Pavement", new Vector3(0, -0.14f, -d - 10), new Vector3(w, 0.28f, 8), true);
            hall.Box("Stone", new Vector3(0, 0.4f, -d - 14), new Vector3(w, 0.8f, 0.4f), true);
            foreach (int side in new[] { -1, 1 })
            {
                hall.Box("Stone", new Vector3(side * (w / 2 - 0.2f), 0.4f, -d - 10), new Vector3(0.4f, 0.8f, 8), true);
                hall.Box("Stone", new Vector3(side * 6, 0.35f, -d - 10), new Vector3(5, 0.7f, 3), true);
            }
            hall.Finish();
        }

        private static void BuildLiftSideGlassRailing(Transform root)
        {
            string path = Root + "/Materials/RailingGlass.mat";
            var glass = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (glass == null)
            {
                glass = new Material(materials["ClearGlass"]) { name = "RailingGlass" };
                AssetDatabase.CreateAsset(glass, path);
            }
            glass.color = new Color(0.53f, 0.76f, 0.74f, 0.27f);
            glass.SetFloat("_Glossiness", 0.8f); glass.SetFloat("_Metallic", 0.12f);
            materials["RailingGlass"] = glass; EditorUtility.SetDirty(glass);
            var rail = RightGroup(root, "10_Lift_side_glass_railing", Vector3.zero);
            var start = RailingColumnAnchor(dimensions); start.y = 0;
            var elbow = RailingElbow(dimensions); elbow.y = 0;
            var wall = RailingWallAnchor(dimensions); wall.y = 0;
            BuildRailingSection(rail, "Column_to_elbow", start, elbow, true);
            BuildRailingSection(rail, "Elbow_to_dividing_wall", elbow, wall, false);
            RightGroup(rail, "Column_connection", start + Vector3.up * 0.6f);
            RightGroup(rail, "Elbow_connection", elbow + Vector3.up * 0.6f);
            RightGroup(rail, "Wall_connection", wall + Vector3.up * 0.6f);
        }

        private static void BuildRailingSection(Transform rail, string name, Vector3 start, Vector3 end, bool includeStartPost)
        {
            Vector3 delta = end - start;
            float length = delta.magnitude;
            int panels = Mathf.CeilToInt(length / 1.25f);
            float step = length / panels;
            var section = RightGroup(rail, name, (start + end) / 2);
            section.localRotation = Quaternion.Euler(0, Mathf.Atan2(-delta.z, delta.x) * Mathf.Rad2Deg, 0);
            var geometry = new Part("Railing_" + name, section, "RailingConnection");
            for (int i = includeStartPost ? 0 : 1; i <= panels; i++)
                geometry.Box("Silver", new Vector3(-length / 2 + i * step, 0.57f, 0), new Vector3(0.045f, 1.1f, 0.045f));
            for (int i = 0; i < panels; i++)
                geometry.Box("RailingGlass", new Vector3(-length / 2 + (i + 0.5f) * step, 0.6f, 0), new Vector3(step - 0.04f, 0.94f, 0.018f), true);
            geometry.Box("Silver", new Vector3(0, 1.12f, 0), new Vector3(length + 0.04f, 0.04f, 0.055f));
            geometry.Finish();
        }

        private static void BuildCentralRoundSpotlights(Transform root)
        {
            var group = RightGroup(root, "11_Central_round_spotlights", Vector3.zero);
            var lights = new Part("Paired_round_ceiling_lamps", group, "LobbyRefine");
            int index = 0;
            for (float z = -2.5f; z > -dimensions.lobbyDepth + 1; z -= 3)
                foreach (float x in new[] { -1.1f, 1.1f })
                {
                    float y = dimensions.lobbyHeight - 0.42f;
                    lights.Cylinder("Metal", new Vector3(x, y, z), new Vector3(0.17f, 0.065f, 0.17f));
                    lights.Cylinder("WarmLight", new Vector3(x, y - 0.075f, z), new Vector3(0.115f, 0.012f, 0.115f));
                    RightGroup(group, "Round_spot_" + (++index).ToString("00"), new Vector3(x, y, z));
                }
            lights.Finish();
        }

        private static void SetupLobbyRefinementViewpoints()
        {
            var root = GameObject.Find("Review viewpoints [select and align view]");
            if (root == null) return;
            foreach (string name in new[] { "30_Open_academic_connection", "31_Centered_wider_columns", "32_Column_glass_railing", "33_Central_round_lights", "34_Academic_side_walkway" })
            { var old = root.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Viewpoint(root.transform, "30_Open_academic_connection", new Vector3(0, 1.8f, -32), new Vector3(0, 2, -54));
            Viewpoint(root.transform, "31_Centered_wider_columns", new Vector3(0, 1.7f, -8), new Vector3(0, 2.1f, -24));
            Viewpoint(root.transform, "32_Column_glass_railing", new Vector3(-2, 1.8f, -18), new Vector3(-7.6f, 1.1f, -24));
            Viewpoint(root.transform, "33_Central_round_lights", new Vector3(0, 3, -15.4f), new Vector3(0, 4.05f, -18));
            Viewpoint(root.transform, "34_Academic_side_walkway", new Vector3(12, 1.7f, -42), new Vector3(12, 1.7f, -53));
            SetupRailingWallViewpoints();
        }

        private static void SetupRailingWallViewpoints()
        {
            var root = GameObject.Find("Review viewpoints [select and align view]");
            if (root == null) return;
            foreach (string name in new[] { "35_Railing_connected_to_dividing_wall", "36_Railing_wall_joint" })
            { var old = root.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Viewpoint(root.transform, "35_Railing_connected_to_dividing_wall", new Vector3(-2, 1.85f, -17), new Vector3(-12, 1.1f, -22));
            Viewpoint(root.transform, "36_Railing_wall_joint", new Vector3(-14.4f, 1.55f, -18.2f), new Vector3(-17.32f, 0.9f, -21.25f));
            SetupRightAngleRailingViewpoints();
        }

        private static void SetupRightAngleRailingViewpoints()
        {
            var root = GameObject.Find("Review viewpoints [select and align view]");
            if (root == null) return;
            foreach (string name in new[] { "37_Right_angle_glass_railing", "38_Central_timber_and_wider_columns" })
            { var old = root.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Viewpoint(root.transform, "37_Right_angle_glass_railing", new Vector3(-2, 3.65f, -18.5f), new Vector3(-10, 0.8f, -22.2f));
            Viewpoint(root.transform, "38_Central_timber_and_wider_columns", new Vector3(0, 1.8f, -8), new Vector3(0, 3.6f, -32));
        }
    }
}
#endif
