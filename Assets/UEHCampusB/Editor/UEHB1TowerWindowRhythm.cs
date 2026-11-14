#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UEH.CampusB.Editor
{
    public static partial class UEHCampusBuilder
    {
        private static readonly int[] B1TowerPaneRhythm = { 3, 3, 3, 2, 2 };
        private const int B1TowerProjectingLevels = 5;
        private const float B1TowerMainPierWidth = .8f;

        // Ordered from viewer-left to viewer-right, i.e. decreasing local X.
        // The three left apertures are wider and each has three glass lights.
        private static Vector2[] B1TowerFiveBays()
        {
            float width = dimensions.frontage - 3.5f;
            float unit = (width - 6 * B1TowerMainPierWidth) / 13;
            float edge = width / 2 - B1TowerMainPierWidth;
            var bays = new Vector2[5];
            for (int i = 0; i < bays.Length; i++)
            {
                float clear = unit * B1TowerPaneRhythm[i];
                bays[i] = new Vector2(edge - clear, edge);
                edge -= clear + B1TowerMainPierWidth;
            }
            return bays;
        }

        private static void BuildB1FiveBayTower(Transform parent)
        {
            float bottom = B1FrameTop + 7.65f, top = dimensions.towerHeight;
            float width = dimensions.frontage - 3.5f, half = width / 2;
            float front = B1TowerFrontZ, rear = B1TowerRearZ, depth = B1TowerDepth;
            int floors = dimensions.upperFloors;
            float step = (top - bottom) / floors;
            var bays = B1TowerFiveBays();
            float split = bays[2].x - B1TowerMainPierWidth / 2;
            var shell = new Part("06_Tower_shell_reference", parent, "B1TowerRhythm");
            // The projected five-storey zone has recessed glass; upper left floors are flush.
            float transition = bottom + Mathf.Min(B1TowerProjectingLevels, floors) * step;
            shell.Box("BlueGlass", new Vector3((-half + split) / 2, (bottom + top) / 2, (front - .1f + rear) / 2), new Vector3(split + half, top - bottom, depth - .1f));
            shell.Box("BlueGlass", new Vector3((split + half) / 2, (bottom + transition) / 2, (front - 1.5f + rear) / 2), new Vector3(half - split, transition - bottom, depth - 1.5f));
            if (transition < top)
                shell.Box("BlueGlass", new Vector3((split + half) / 2, (transition + top) / 2, (front - .1f + rear) / 2), new Vector3(half - split, top - transition, depth - .1f));
            shell.Box("Ivory", new Vector3(0, top + .25f, front - depth / 2), new Vector3(width + .8f, .5f, depth + 1.4f));
            shell.Finish();

            var facadeRoot = RightGroup(parent, "07_Tower_horizontal_bands_and_vertical_window_bays", Vector3.zero);
            var structure = new Part("Five_bay_main_piers_and_floor_edges", facadeRoot, "B1TowerRhythm");
            var piers = new List<float> { half - B1TowerMainPierWidth / 2 };
            for (int bay = 0; bay < bays.Length; bay++) piers.Add(bays[bay].x - B1TowerMainPierWidth / 2);
            foreach (float x in piers)
                structure.Box("Ivory", new Vector3(x, (bottom + top) / 2, front - .13f), new Vector3(B1TowerMainPierWidth, top - bottom, .5f));
            for (int floor = 0; floor < floors; floor++)
            {
                float y = bottom + floor * step;
                // Floor bands on the flat facade stay shallow, without balcony parapets.
                structure.Box("Ivory", new Vector3(0, y + .18f, front + .015f), new Vector3(width, .36f, .35f));
                foreach (float x in new[] { -half, half })
                {
                    structure.Box("Ivory", new Vector3(x, y + .15f, front - depth / 2), new Vector3(.25f, .3f, depth));
                    for (float z = rear; z < front; z += 3.2f)
                        structure.Box("White", new Vector3(x, y + step / 2, z), new Vector3(.18f, step, .22f));
                }
            }
            structure.Finish();

            for (int floor = 0; floor < floors; floor++)
            {
                float y = bottom + floor * step;
                bool projected = floor < B1TowerProjectingLevels;
                var level = RightGroup(facadeRoot, "Tower_level_" + (floor + 5).ToString("00") + (projected ? "_LEFT_projected" : "_flat_five_bays"), Vector3.zero);
                for (int bay = 0; bay < bays.Length; bay++)
                {
                    bool recessed = projected && bay < 3;
                    float z = recessed ? front - 1.2f : front + .02f;
                    var glass = new Part("Level_" + (floor + 5).ToString("00") + "_bay_" + (bay + 1) + "_" + B1TowerPaneRhythm[bay] + "_lights", level, "B1TowerRhythm");
                    BuildB1TowerExactLights(glass, bays[bay], y + .45f, y + step - .3f, z, B1TowerPaneRhythm[bay]);
                    glass.Finish();
                }
                if (projected)
                {
                    var projection = new Part("Level_" + (floor + 5).ToString("00") + "_LEFT_parapet_and_projecting_coping", level, "B1TowerRhythm");
                    float leftSpan = half - split, center = (half + split) / 2;
                    projection.Box("Ivory", new Vector3(center, y + .2f, front - .6f), new Vector3(leftSpan, .4f, 3.5f));
                    projection.Box("Ivory", new Vector3(center, y + .8f, front + 1.2f), new Vector3(leftSpan, .85f, .22f));
                    projection.Box("White", new Vector3(center, y + 1.27f, front + 1.29f), new Vector3(leftSpan + .24f, .12f, .45f));
                    // Short solid returns and piers close the ends of the recessed left strip.
                    foreach (float x in new[] { split + .12f, half - .12f })
                        projection.Box("Ivory", new Vector3(x, y + .8f, front + .025f), new Vector3(.24f, .85f, 2.35f));
                    for (int bay = 0; bay < 3; bay++)
                        projection.Box("Ivory", new Vector3(bays[bay].x - B1TowerMainPierWidth / 2, y + step / 2, front - .65f), new Vector3(.3f, step, 1.3f));
                    projection.Finish();
                }
            }
            // Preserve the complete side/rear envelope and existing floor heights/footprint.
            var back = new Part("Rear_window_grid_and_floor_bands", facadeRoot, "B1TowerRhythm");
            for (int floor = 0; floor < floors; floor++)
            {
                float y = bottom + floor * step;
                B1WindowGrid(back, -half + .45f, half - .45f, y + .4f, y + step - .2f, rear - .05f, 2.4f, "White", -1f);
                back.Box("Ivory", new Vector3(0, y + .15f, rear - .08f), new Vector3(width + .25f, .3f, .35f));
            }
            back.Finish();
            ApplyB1TowerAlignment(parent);
        }

        private static void BuildB1TowerExactLights(Part p, Vector2 bay, float bottom, float top, float z, int count)
        {
            float width = bay.y - bay.x, center = (bay.x + bay.y) / 2, height = top - bottom;
            p.Box("BlueGlass", new Vector3(center, (top + bottom) / 2, z - .12f), new Vector3(width, height, .09f));
            for (int i = 0; i <= count; i++)
                p.Box("Silver", new Vector3(Mathf.Lerp(bay.x, bay.y, (float)i / count), (top + bottom) / 2, z), new Vector3(.07f, height, .12f));
            for (int row = 0; row <= 2; row++)
                p.Box("Silver", new Vector3(center, Mathf.Lerp(bottom, top, row / 2f), z), new Vector3(width, .065f, .12f));
        }

        [MenuItem("UEH/Campus B/42. Redesign upper tower five bay window rhythm")]
        public static void RefineB1TowerWindowRhythmAndCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.wrappedB1Frame) throw new InvalidOperationException("Build B1 frame first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[m.name] = m; }
            string path = Root + "/Prefabs/UEH_Exterior.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (string name in new[] { "06_Tower_shell_reference", "07_Tower_horizontal_bands_and_vertical_window_bays" })
                { var old = contents.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                BuildB1ReferenceTower(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupB1TowerRhythmViewpoints(GameObject.Find("Review viewpoints [select and align view]").transform);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            CaptureB1TowerRhythmPreviews();
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/b1-tower-window-rhythm-layout.json"), JsonUtility.ToJson(new B1TowerRhythmLayout(), true));
            Debug.Log("UEH_B1_TOWER_RHYTHM_COMPLETE Five front bays: 3-3-3-2-2 lights; larger three viewer-left bays; projected left parapets only at five lowest tower floors (5-9). Saved exterior prefab and study scene.");
        }

        [Serializable] private sealed class B1TowerRhythmLayout
        {
            public string status = "Reconstructed from supplied Virtual Tour screenshots; estimated dimensions.";
            public string ordering = "Viewer-left to viewer-right from entrance street (+Z); local X decreases.";
            public int[] glassLightsPerBay = B1TowerPaneRhythm;
            public Vector2[] clearBayBoundsLocalX = B1TowerFiveBays();
            public int firstTowerFloor = 5, projectedLeftFloorCount = B1TowerProjectingLevels, lastProjectedFloor = 9;
            public int totalTowerFloors = dimensions.upperFloors;
            public float leftToRightBayWidthRatio = 1.5f, towerWidth = dimensions.frontage - 3.5f, towerDepth = B1TowerDepth, centerX = B1TowerOffsetX;
            public float mainPierWidth = B1TowerMainPierWidth;
        }

        private static void SetupB1TowerRhythmViewpoints(Transform views)
        {
            foreach (Transform child in new List<Transform>(GetDirectChildren(views)))
            { int n; if (child.name.Length > 3 && int.TryParse(child.name.Substring(0, 3), out n) && n >= 143 && n <= 147) UnityEngine.Object.DestroyImmediate(child.gameObject); }
            Viewpoint(views, "143_B1_tower_five_bays_front", new Vector3(B1TowerOffsetX, 36.8f, 80), new Vector3(B1TowerOffsetX, 36.8f, 0));
            Viewpoint(views, "144_B1_tower_virtual_tour_perspective", new Vector3(18, 1.7f, 28), new Vector3(4, 32, -1.4f));
            Viewpoint(views, "145_B1_tower_upper_3_3_3_2_2_glass", new Vector3(B1TowerOffsetX, 49, 37), new Vector3(B1TowerOffsetX, 49, -1.4f));
            Viewpoint(views, "146_B1_tower_only_five_projected_levels", new Vector3(34, 31, 32), new Vector3(11, 29, -1));
            Viewpoint(views, "147_B1_tower_projection_to_flat_transition", new Vector3(28, 36, 14), new Vector3(12, 35, -1));
        }

        private static void CaptureB1TowerRhythmPreviews()
        {
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            foreach (Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                int n; if (point.name.Length < 4 || !int.TryParse(point.name.Substring(0, 3), out n) || n < 143 || n > 147) continue;
                var go = new GameObject("Temporary tower rhythm review camera"); var camera = go.AddComponent<Camera>(); camera.enabled = false;
                camera.transform.SetPositionAndRotation(point.position, point.rotation); camera.fieldOfView = n == 144 ? 80 : 62; camera.nearClipPlane = .05f; camera.farClipPlane = 250;
                if (n == 143) { camera.orthographic = true; camera.orthographicSize = 20; }
                var target = new RenderTexture(1800, 1200, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 }; var prior = RenderTexture.active;
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                var texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); texture.Apply(); File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = prior; target.Release();
                UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
#endif
