#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UEH.CampusB.Editor
{
    public static partial class UEHCampusBuilder
    {
        public const float ExpandedLobbyDepth = 48;
        public const float RetainedColumnZ = -12;

        [MenuItem("UEH/Campus B/8. Expand lobby for four side areas")]
        public static void ExpandLobby()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before expanding the lobby.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null) throw new FileNotFoundException("Missing estimated dimensions.");
            float oldDepth = dimensions.lobbyDepth;
            dimensions.lobbyDepth = Mathf.Max(ExpandedLobbyDepth, oldDepth);
            dimensions.expandedLobby = true;
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                materials[material.name] = material;
            }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (string name in new[] { "01_Floor_and_walls", "02_Stone_columns", "04_Timber_ceiling_and_downlights", "09_Four_expansion_areas", "10_Lift_side_glass_railing", "11_Central_round_spotlights" })
                {
                    var old = contents.transform.Find(name);
                    if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                }
                BuildExpandedCore(contents.transform);
                foreach (string name in new[] { "07_Inner_passage_inferred", "Courtyard_context_left", "Courtyard_context_right", "Inner_direction" })
                {
                    var item = contents.transform.Find(name);
                    if (item != null) item.localPosition += Vector3.back * (dimensions.lobbyDepth - oldDepth);
                }
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            EditorUtility.SetDirty(dimensions);
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ConfigureExpandedScene();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_EXPANSION_UPDATED Lobby " + dimensions.frontage + "x" + dimensions.lobbyDepth +
                "m, four 12x6m side areas; central pair Z=" + ExpandedColumnZ(dimensions) + ". Entrance columns retained=" + dimensions.keepEntranceColumnsWhenExpanded);
        }

        private static void BuildExpandedShell(Transform root)
        {
            float w = dimensions.frontage, d = dimensions.lobbyDepth, h = dimensions.lobbyHeight;
            var shell = new Part("01_Floor_and_walls", root, "Expanded");
            shell.Box("Floor", new Vector3(0, -0.14f, -d / 2), new Vector3(w, 0.28f, d), true);
            shell.Box("White", new Vector3(0, h - 0.1f, -d / 2), new Vector3(w - 0.5f, 0.2f, d - 0.1f), true);
            foreach (int side in new[] { -1, 1 })
            {
                if (side > 0 && dimensions.leftOfficeAndLounge)
                {
                    const float openingTop = 4.2f;
                    float officeDepth = -LeftOfficeZ(dimensions);
                    float openingStart = officeDepth - 5.3f, openingEnd = officeDepth + 5.3f;
                    shell.Box("Ivory", new Vector3(LeftFacadeX(dimensions) + 0.45f, h / 2, -openingStart / 2), new Vector3(0.3f, h, openingStart), true);
                    shell.Box("Ivory", new Vector3(LeftFacadeX(dimensions) + 0.45f, h / 2, -(openingEnd + d) / 2), new Vector3(0.3f, h, d - openingEnd), true);
                    shell.Box("Ivory", new Vector3(LeftFacadeX(dimensions) + 0.45f, (openingTop + h) / 2, LeftOfficeZ(dimensions)), new Vector3(0.3f, h - openingTop, openingEnd - openingStart), true);
                }
                else if (side < 0 && dimensions.rightGlassOffice)
                    BuildRightWallWithGlassOffice(shell, w, d, h);
                else if (side < 0 && dimensions.rightLiftsAndServices)
                {
                    float start = ServiceCorridorZ * -1 - ServiceCorridorWidth / 2;
                    float end = ServiceCorridorZ * -1 + ServiceCorridorWidth / 2;
                    shell.Box("Ivory", new Vector3(-w / 2 + 0.15f, h / 2, -start / 2), new Vector3(0.3f, h, start), true);
                    shell.Box("Ivory", new Vector3(-w / 2 + 0.15f, h / 2, -(end + d) / 2), new Vector3(0.3f, h, d - end), true);
                    shell.Box("Ivory", new Vector3(-w / 2 + 0.15f, (ServicePortalHeight + h) / 2, ServiceCorridorZ), new Vector3(0.3f, h - ServicePortalHeight, ServiceCorridorWidth), true);
                }
                else shell.Box("Ivory", new Vector3(side * (w / 2 - 0.15f), h / 2, -d / 2), new Vector3(0.3f, h, d), true);
                if (!dimensions.refinedLobbyLayout)
                    shell.Box("Ivory", new Vector3(side * (w / 4 + 1.5f), h / 2, -d), new Vector3(w / 2 - 3, h, 0.3f), true);
                if (side > 0 || !dimensions.facultyLiftsBehindWelcome)
                    shell.Box("Border", new Vector3(side * (w / 2 - 1.6f) - (side > 0 ? dimensions.leftWingInset : 0), 0.008f, -d / 2), new Vector3(0.16f, 0.014f, d - 1.8f));
            }
            foreach (float z in new[] { -1f, -d + 1 })
                shell.Box("Border", new Vector3(0, 0.008f, z), new Vector3(w - 3.2f, 0.014f, 0.16f));
            if (dimensions.leftWingInset > 0 && dimensions.keepEntranceColumnsWhenExpanded && !dimensions.ocbBranch)
            {
                // Close the frontage left behind by the shorter glass bay. The inner end
                // meets the moved pier at its mid-depth, like the glazing on the other face.
                float start = LeftGateColumnX(dimensions) + dimensions.lobbyColumnWidth / 2;
                if (start < w / 2)
                    shell.Box("Ivory", new Vector3((start + w / 2) / 2, h / 2, EntranceGlassZ(dimensions)),
                        new Vector3(w / 2 - start, h, 0.3f), true);
            }
            shell.Finish();
        }

        private static void BuildExpandedCore(Transform root)
        {
            float w = dimensions.frontage, d = dimensions.lobbyDepth, h = dimensions.lobbyHeight;
            BuildExpandedShell(root);
            BuildExpandedColumns(root);
            BuildExpandedCeiling(root);
            if (dimensions.refinedLobbyLayout)
            {
                BuildLiftSideGlassRailing(root);
                BuildCentralRoundSpotlights(root);
            }
            BuildFourSideAreas(root);
            if (dimensions.ocbBranch) BuildOcbBranch(root);
            BuildRearSouvenirAndPlants(root);
        }

        private static void BuildExpandedCeiling(Transform root)
        {
            float w = dimensions.frontage, d = dimensions.lobbyDepth, h = dimensions.lobbyHeight;
            var ceiling = new Part("04_Timber_ceiling_and_downlights", root, "Expanded");
            foreach (float centerX in dimensions.centerTimberOnly ? new[] { 0f } : new[] { -w * 0.34f, 0, w * 0.34f })
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
            for (float z = -4; z > -d + 2; z -= 5)
            {
                ceiling.Box("Gold", new Vector3(0, h - 0.5f, z), new Vector3(1.8f, 0.08f, 0.85f));
                for (int crystal = 0; crystal < 18; crystal++)
                    ceiling.Box("WarmLight", new Vector3(-0.8f + crystal * 0.095f, h - 0.66f, z), new Vector3(0.045f, 0.3f, 0.65f));
                ceiling.Box("Ivory", new Vector3(7, h - 0.23f, z), new Vector3(1.3f, 0.04f, 0.45f));
                for (int vent = 0; vent < 8; vent++)
                    ceiling.Box("Metal", new Vector3(6.5f + vent * 0.14f, h - 0.26f, z), new Vector3(0.035f, 0.015f, 0.3f));
            }
            ceiling.Finish();
        }

        private static void BuildExpandedColumns(Transform root)
        {
            float w = dimensions.frontage, h = dimensions.lobbyHeight;
            var columns = new Part("02_Stone_columns", root, "Expanded");
            foreach (float x in new[] { -w * 0.125f, w * 0.125f }) AddExpandedColumn(columns, x, ExpandedColumnZ(dimensions), h);
            if (dimensions.rearColumnsAndSouvenir)
                foreach (float x in new[] { -w * 0.125f, w * 0.125f }) AddExpandedColumn(columns, x, RearColumnZ(dimensions), h);
            if (dimensions.keepEntranceColumnsWhenExpanded)
                foreach (float x in new[] { RightCornerColumnX(dimensions), -w * 0.125f, w * 0.125f, LeftGateColumnX(dimensions) })
                    AddExpandedColumn(columns, x, x > w * 0.25f ? LeftColumnZ(dimensions) : ExpandedEntranceZ(dimensions), h);
            columns.Finish();
        }

        private static void AddExpandedColumn(Part columns, float x, float z, float h)
        {
            float width = dimensions.refinedLobbyLayout ? dimensions.lobbyColumnWidth : 0.9f;
            float baseWidth = width + (dimensions.refinedLobbyLayout ? 0.12f : 0.08f);
            float capitalWidth = width + (dimensions.refinedLobbyLayout ? 0.24f : 0.22f);
            columns.Box("Stone", new Vector3(x, h / 2, z), new Vector3(width, h, width), true);
            columns.Box("Border", new Vector3(x, 0.12f, z), new Vector3(baseWidth, 0.24f, baseWidth));
            columns.Box("Ivory", new Vector3(x, h - 0.13f, z), new Vector3(capitalWidth, 0.24f, capitalWidth));
        }

        private static void BuildFourSideAreas(Transform lobbyRoot)
        {
            var areas = new GameObject("09_Four_expansion_areas").transform;
            areas.SetParent(lobbyRoot, false);
            foreach (int side in new[] { 1, -1 })
                for (int i = 0; i < 2; i++)
                {
                    string name = (side > 0 ? "LEFT_" : "RIGHT_") + (i + 1).ToString("00");
                    var zone = new GameObject(name + " [12m x 6m]").transform;
                    zone.SetParent(areas, false);
                    zone.localPosition = new Vector3(side < 0 ? RightFacadeX(dimensions) : LeftFacadeX(dimensions), 0, -23 - i * 14);
                    zone.localRotation = Quaternion.Euler(0, side > 0 ? -90 : 90, 0);
                    var volume = new GameObject("Reserved_area_" + name).transform;
                    volume.SetParent(zone, false);
                    volume.localPosition = new Vector3(0, 2.1f, 3.15f);
                    var reserved = volume.gameObject.AddComponent<UEHReservedSpace>();
                    reserved.size = new Vector3(12, 4.1f, 6);
                    reserved.purpose = "Khu mở rộng " + name + ": mặt bằng 12 x 6 m, đủ vách tương đương mảng Since 1976. Nội dung trang trí để bố trí tiếp.";
                    // Unassigned wall panels and stone inlays show the four bays without inventing exhibit content.
                    var frame = new Part("Bay_frame_" + name, zone, "Expanded");
                    frame.Box("Wood", new Vector3(0, 2.05f, -0.08f), new Vector3(10.2f, 3.2f, 0.16f));
                    frame.Box("Ivory", new Vector3(0, 2.05f, 0.025f), new Vector3(10.04f, 3.04f, 0.05f));
                    frame.Finish();
                    var border = new Part("Floor_inlay_" + name, zone, "Expanded");
                    foreach (float x in new[] { -6f, 6f })
                        border.Box("Border", new Vector3(x, 0.009f, 3.15f), new Vector3(0.1f, 0.014f, 6));
                    border.Box("Border", new Vector3(0, 0.009f, 6.15f), new Vector3(12, 0.014f, 0.1f));
                    border.Finish();
                }
            if (dimensions.leftOfficeAndLounge) PopulateLeftOfficeAndLounge(areas);
            if (dimensions.rightLiftsAndServices) PopulateRightLiftsAndServices(areas);
            if (dimensions.rightGlassOffice) PopulateRightGlassOffice(areas);
            if (dimensions.compactLeftSequence) CompactLeftBays(areas);
        }

        private static void ConfigureExpandedScene()
        {
            var player = UnityEngine.Object.FindFirstObjectByType<UEHWalkthroughController>();
            if (player != null) player.lobbySpawn = new Vector3(0, 0.08f, -dimensions.lobbyDepth / 2);
            var fillRoot = GameObject.Find("Lobby lighting");
            if (fillRoot != null)
            {
                var old = fillRoot.transform.Find("Expanded rear lighting");
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                var rear = new GameObject("Expanded rear lighting").transform;
                rear.SetParent(fillRoot.transform, false);
                foreach (float x in new[] { -11f, 0, 11f })
                    foreach (float z in new[] { -33f, -43f })
                    {
                        var light = new GameObject("Rear ceiling fill").AddComponent<Light>();
                        light.transform.SetParent(rear, false);
                        light.transform.position = new Vector3(x, 3.85f, z);
                        light.type = LightType.Point; light.range = 14; light.intensity = 1.25f;
                        light.color = new Color(1, 0.90f, 0.72f); light.shadows = LightShadows.None;
                    }
            }
            var reflections = GameObject.Find("Local reflection captures");
            if (reflections != null)
            {
                foreach (var probe in reflections.GetComponentsInChildren<ReflectionProbe>())
                    if (!probe.name.Contains("rear"))
                    { probe.transform.position = new Vector3(probe.transform.position.x, 1.9f, -12); probe.size = new Vector3(23, 5.2f, 25); }
                foreach (float x in new[] { -10f, 10f })
                {
                    string name = "Lobby reflection rear " + x;
                    var item = reflections.transform.Find(name);
                    if (item == null)
                    { item = new GameObject(name).transform; item.SetParent(reflections.transform, false); item.gameObject.AddComponent<ReflectionProbe>(); }
                    item.position = new Vector3(x, 1.9f, -36);
                    var probe = item.GetComponent<ReflectionProbe>();
                    probe.mode = ReflectionProbeMode.Custom; probe.size = new Vector3(23, 5.2f, 25);
                    probe.center = new Vector3(0, 0.3f, 0); probe.boxProjection = true;
                    probe.blendDistance = 3; probe.resolution = 128; probe.intensity = 0;
                }
            }
            var views = GameObject.Find("Review viewpoints [select and align view]");
            if (views != null)
            {
                foreach (string name in new[] { "11_Expanded_lobby_forward", "12_Expanded_lobby_reverse", "13_Expanded_left_areas", "14_Expanded_right_areas" })
                { var old = views.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                Viewpoint(views.transform, "11_Expanded_lobby_forward", new Vector3(0, 1.65f, -3), new Vector3(0, 1.9f, -40));
                Viewpoint(views.transform, "12_Expanded_lobby_reverse", new Vector3(0, 1.65f, -44), new Vector3(0, 1.9f, -12));
                Viewpoint(views.transform, "13_Expanded_left_areas", new Vector3(3, 2.2f, -30), new Vector3(17.4f, 2.2f, -30));
                Viewpoint(views.transform, "14_Expanded_right_areas", new Vector3(-3, 2.2f, -30), new Vector3(-17.4f, 2.2f, -30));
            }
            if (dimensions.leftOfficeAndLounge) SetupLeftAreaViewpoints();
            if (dimensions.rightLiftsAndServices) SetupRightAreaViewpoints();
            if (dimensions.rightGlassOffice) SetupGlassOfficeViewpoints();
            if (dimensions.refinedLobbyLayout) SetupLobbyRefinementViewpoints();
        }

        private static void BuildExpandedEntrance(Transform root)
        {
            if (dimensions.facultyLiftEntrance) { BuildReferenceEntrance(root); return; }
            float w = dimensions.frontage, entry = dimensions.entryWidth;
            var entrance = new Part("03_Glazing_with_open_doorway", root);
            float bayWidth = (w - entry) / 6;
            foreach (int side in new[] { -1, 1 })
            {
                for (int bay = 0; bay < 3; bay++)
                {
                    float x = side * (entry / 2 + bayWidth * (bay + 0.5f));
                    entrance.Box("ClearGlass", new Vector3(x, 2, 0), new Vector3(bayWidth - 0.09f, 3.8f, 0.025f), true);
                    entrance.Box("Metal", new Vector3(side * (entry / 2 + bayWidth * bay), 2.05f, 0), new Vector3(0.065f, 4.1f, 0.12f), true);
                    entrance.Box("Metal", new Vector3(x, 3.25f, 0), new Vector3(bayWidth, 0.055f, 0.12f));
                    entrance.Box("Metal", new Vector3(x, 0.15f, 0), new Vector3(bayWidth, 0.08f, 0.12f));
                }
                entrance.Box("ClearGlass", new Vector3(side * (entry / 2 + 0.65f), 1.65f, -0.12f), new Vector3(1.2f, 3.2f, 0.03f));
                entrance.Box("Silver", new Vector3(side * (entry / 2 + 0.18f), 1.5f, -0.19f), new Vector3(0.045f, 0.7f, 0.045f));
            }
            entrance.Box("Metal", new Vector3(0, 3.3f, 0), new Vector3(entry, 0.13f, 0.25f));
            entrance.Box("ClearGlass", new Vector3(0, 3.86f, 0), new Vector3(entry, 0.95f, 0.025f));
            entrance.Finish();
            Text(root, "Entrance_label", "UEH  /  CƠ SỞ B", new Vector3(0, 3.65f, 0.11f), 4.3f, 0.36f, "Teal");
        }
    }
}
#endif
