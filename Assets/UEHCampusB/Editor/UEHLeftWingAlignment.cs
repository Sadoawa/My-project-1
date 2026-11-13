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
        public static float LeftFacadeX(UEHDimensions s) => s.frontage / 2 - 0.6f - s.leftWingInset;

        [MenuItem("UEH/Campus B/22. Inset left wall heritage office and entrance glass")]
        public static void AlignLeftWing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.leftOfficeAndLounge || !dimensions.entranceGlassThroughColumns)
                throw new InvalidOperationException("Build the existing left areas and column-aligned gates first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[mat.name] = mat; }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.leftWingInset = dimensions.rightWingInset;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var heritage = contents.transform.Find("06_1976_LEFT_from_gate");
                float delta = LeftFacadeX(dimensions) - heritage.localPosition.x;
                heritage.localPosition = new Vector3(LeftFacadeX(dimensions), 0, LeftHeritageZ(dimensions));
                ShiftLooseLeftProps(contents.transform, delta);
                var areas = contents.transform.Find("09_Four_expansion_areas");
                foreach (Transform bay in areas)
                    if (bay.name.StartsWith("LEFT_")) bay.localPosition = new Vector3(LeftFacadeX(dimensions), 0, bay.localPosition.z);
                // The room, its door, furnishings and waiting seats move as a single group.
                // Its former floor extension is unnecessary once it fits on the main slab.
                AlignOfficeBehindWall(areas.Find("LEFT_01 [12m x 6m]/Office_room"));
                foreach (string name in new[] { "01_Floor_and_walls", "02_Stone_columns", "03_Glazing_with_open_doorway", "Entrance_label" })
                { var old = contents.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                BuildExpandedShell(contents.transform);
                BuildExpandedColumns(contents.transform);
                BuildExpandedEntrance(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            TrimCompactLeftExteriorWall();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupLeftWingViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_LEFT_WING_INSET_UPDATED Left wall, slate, Since1976, office/lounge and loose left props moved inward " + dimensions.leftWingInset + "m; facade X=" + LeftFacadeX(dimensions) + ". Since gate glass shortened to meet the moved outside column, open doorway retained at X=" + SinceGateCenterX(dimensions) + ".");
        }

        private static void ShiftLooseLeftProps(Transform root, float delta)
        {
            foreach (Transform child in root)
                if (child.name.StartsWith("Lounge_seat_") || child.name.StartsWith("08_Indoor_plant_") || child.name.StartsWith("08_Entrance_plant_"))
                {
                    var renderer = child.GetComponentInChildren<MeshRenderer>();
                    if (renderer != null && renderer.bounds.center.x > 0) child.localPosition += Vector3.right * delta;
                }
        }

        private static void SetupLeftWingViewpoints()
        {
            var views = GameObject.Find("Review viewpoints [select and align view]"); if (views == null) return;
            foreach (string name in new[] { "59_Inset_left_heritage_wall", "60_Balanced_lobby_sides_and_gates", "61_Shorter_Since_gate_glazing", "62_Inset_left_office_lounge" })
            { var old = views.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Viewpoint(views.transform, "59_Inset_left_heritage_wall", new Vector3(3, 1.75f, -9), new Vector3(LeftFacadeX(dimensions), 2, -9));
            Viewpoint(views.transform, "60_Balanced_lobby_sides_and_gates", new Vector3(0, 1.75f, -22), new Vector3(0, 2, EntranceGlassZ(dimensions)));
            Viewpoint(views.transform, "61_Shorter_Since_gate_glazing", new Vector3(18, 1.8f, 6), new Vector3(12, 1.8f, -1));
            Viewpoint(views.transform, "62_Inset_left_office_lounge", new Vector3(5.8f, 1.9f, -20.5f), new Vector3(LeftFacadeX(dimensions), 2, -20.5f));
        }

        // Apply the layout and render review images; no Play Mode tests are run here.
        public static void AlignLeftWingAndCapture()
        {
            AlignLeftWing();
            CaptureReflections();
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            var views = GameObject.Find("Review viewpoints [select and align view]").transform;
            foreach (Transform point in views)
            {
                if (!point.name.StartsWith("59_") && !point.name.StartsWith("60_") && !point.name.StartsWith("61_") && !point.name.StartsWith("62_")) continue;
                var cameraGO = new GameObject("Temporary inset-left review camera");
                var camera = cameraGO.AddComponent<Camera>(); camera.enabled = false;
                camera.transform.SetPositionAndRotation(point.position, point.rotation);
                camera.fieldOfView = point.name.StartsWith("60_") ? 100 : point.name.StartsWith("62_") ? 104 : 75;
                camera.nearClipPlane = 0.08f; camera.farClipPlane = 240;
                var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                var prior = RenderTexture.active; camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                var texture = new Texture2D(1600, 1000, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = prior; rt.Release();
                UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(cameraGO);
            }
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/left-wing-layout.json"), JsonUtility.ToJson(new LeftWingLayout
            {
                inset = dimensions.leftWingInset, leftFacadeX = LeftFacadeX(dimensions), rightFacadeX = RightFacadeX(dimensions),
                outsideColumnX = LeftGateColumnX(dimensions), glassStartX = dimensions.frontage * 0.125f + dimensions.lobbyColumnWidth / 2,
                glassEndX = LeftGateColumnX(dimensions) - dimensions.lobbyColumnWidth / 2,
                gateCenterX = SinceGateCenterX(dimensions), gateWidth = SinceGateWidth
            }, true));
            Debug.Log("UEH_LEFT_WING_ASSETS_AND_PREVIEWS_SAVED");
        }

        [Serializable] private sealed class LeftWingLayout
        {
            public string status = "Native Unity asset generation and rendered previews; no Play Mode tests for this revision.";
            public float inset, leftFacadeX, rightFacadeX, outsideColumnX, glassStartX, glassEndX, gateCenterX, gateWidth;
        }
    }
}
#endif
