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
        private const float B1CentralGlassWidth = 5.2f;
        private const float B1CentralGlassHeight = 4.35f;
        private const float B1RightGlassWidth = 2.55f;
        private const float B1RightGlassHeight = 2.8f;

        [MenuItem("UEH/Campus B/33. Match central frames and surrounding wall to wall colour")]
        public static void MatchB1CentralWindowFrameColour()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string path = Root + "/Prefabs/UEH_Exterior.prefab";
            var ivory = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/Ivory.mat");
            if (ivory == null) throw new InvalidOperationException("The existing wall material is missing.");
            var contents = PrefabUtility.LoadPrefabContents(path);
            int changed = 0;
            try
            {
                var windows = contents.transform.Find("18_B1_structural_frame/08_Narrow_tall_central_glazing");
                if (windows == null) throw new InvalidOperationException("The two central glass walls are missing.");
                foreach (var renderer in windows.GetComponentsInChildren<MeshRenderer>())
                {
                    if (renderer.name == "BlueGlass") continue;
                    renderer.sharedMaterial = ivory;
                    changed++;
                }
                var surroundingWall = contents.transform.Find("18_B1_structural_frame/03_Balconies_louvres_and_windows/Wood");
                if (surroundingWall != null)
                {
                    var renderer = surroundingWall.GetComponent<MeshRenderer>();
                    if (renderer != null) { renderer.sharedMaterial = ivory; changed++; }
                }
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var views = GameObject.Find("Review viewpoints [select and align view]").transform;
            var old = views.Find("108_B1_central_windows_wall_colour");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            Viewpoint(views, "108_B1_central_windows_wall_colour", new Vector3(4.8f, 7.2f, 1.5f), new Vector3(4.8f, 7.15f, -0.8f));
            old = views.Find("109_B1_complete_central_surround_wall_colour");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            Viewpoint(views, "109_B1_complete_central_surround_wall_colour", new Vector3(5, 7.5f, 18), new Vector3(5, 7.7f, -0.6f));
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            CaptureB1BalconyDetailPreviews();
            CaptureB1FacadePreviews();
            CaptureB1WallColourPreview(views.Find("108_B1_central_windows_wall_colour"));
            CaptureB1WallColourPreview(views.Find("109_B1_complete_central_surround_wall_colour"));
            Debug.Log("UEH_B1_WINDOW_COLOUR_COMPLETE Central frames, mullions and the entire former brown wall surrounding both windows use the existing Ivory wall material; meshes and glazing unchanged. Renderers assigned: " + changed);
        }

        private static void CaptureB1WallColourPreview(Transform point)
        {
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            var go = new GameObject("Temporary window colour camera"); var camera = go.AddComponent<Camera>(); camera.enabled = false;
            camera.transform.SetPositionAndRotation(point.position, point.rotation);
            camera.fieldOfView = point.name.StartsWith("109_") ? 64 : 128; camera.nearClipPlane = 0.06f; camera.farClipPlane = 100;
            var target = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            var prior = RenderTexture.active; camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
            var texture = new Texture2D(1600, 1000, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); texture.Apply();
            File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
            camera.targetTexture = null; RenderTexture.active = prior; target.Release();
            UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(go);
        }

        [MenuItem("UEH/Campus B/30. Refine central balcony walls and four right windows")]
        public static void RefineB1BalconyDetailsAndCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.wrappedB1Frame) throw new InvalidOperationException("Build the B1 reference frame first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[material.name] = material; }
            string path = Root + "/Prefabs/UEH_Exterior.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var frame = contents.transform.Find("18_B1_structural_frame");
                if (frame == null) throw new InvalidOperationException("B1 structural frame is missing.");
                foreach (string name in new[] { "03_Balconies_louvres_and_windows", "07_Central_balcony_end_walls", "08_Narrow_tall_central_glazing", "09_Suspended_horizontal_screens", "10_Right_four_even_window_stacks" })
                { var old = frame.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                BuildB1LowerFacade(frame, dimensions.lobbyHeight, B1FrameRightX, B1FrameLeftX, B1FrameRearZ);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var views = GameObject.Find("Review viewpoints [select and align view]");
            SetupB1BalconyDetailViewpoints(views.transform);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            CaptureB1FacadePreviews();
            CaptureB1BalconyDetailPreviews();
            Directory.CreateDirectory(Path.GetFullPath("Docs/UEH-CampusB"));
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/b1-balcony-details-layout.json"), JsonUtility.ToJson(new B1BalconyDetailsLayout
            {
                centralWindowCentersX = new[] { (B1FrontPierAxes[1] + B1FrontPierAxes[2]) / 2, (B1FrontPierAxes[2] + B1FrontPierAxes[3]) / 2 },
                centralGlassBottom = dimensions.lobbyHeight + 0.45f,
                centralGlassTop = dimensions.lobbyHeight + 0.45f + B1CentralGlassHeight,
                endWallCentersX = new[] { B1FrontPierAxes[1] + 0.4f, B1FrontPierAxes[3] - 0.4f },
                hangingScreenBottom = B1FrameTop - 1.55f,
                hangingScreenTop = B1FrameTop - 0.35f,
                rightWindowCentersX = B1RightWindowCenters(B1FrontPierAxes[0], B1FrontPierAxes[1]),
                rightWindowBottoms = new[] { dimensions.lobbyHeight + 0.15f, dimensions.lobbyHeight + (B1FrameTop - dimensions.lobbyHeight) / 2 + 0.15f }
            }, true));
            Debug.Log("UEH_B1_BALCONY_DETAILS_COMPLETE Two solid end walls, two narrower/taller central glass walls, two suspended louvre screens, four evenly spaced right window stacks on two floors.");
        }

        private static void BuildB1BalconyClosuresAndGlazing(Transform root, float h, float front, float[] axes)
        {
            float bottom = h + 0.36f, top = B1FrameTop - 0.3f;
            float back = front - 4.43f, outer = front - 0.13f;
            var ends = new Part("07_Central_balcony_end_walls", root, "B1BalconyDetail");
            foreach (float x in new[] { axes[1] + 0.4f, axes[3] - 0.4f })
            {
                ends.Box("Ivory", new Vector3(x, (bottom + top) / 2, (back + outer) / 2), new Vector3(0.24f, top - bottom, outer - back), true);
                ends.Box("White", new Vector3(x, bottom + 0.10f, (back + outer) / 2), new Vector3(0.28f, 0.20f, outer - back));
                ends.Box("White", new Vector3(x, top - 0.08f, (back + outer) / 2), new Vector3(0.28f, 0.16f, outer - back));
            }
            ends.Finish();
            var glazing = new Part("08_Narrow_tall_central_glazing", root, "B1BalconyDetail");
            var screens = new Part("09_Suspended_horizontal_screens", root, "B1BalconyDetail");
            for (int bay = 1; bay <= 2; bay++)
            {
                float center = (axes[bay] + axes[bay + 1]) / 2;
                float a = center - B1CentralGlassWidth / 2, b = center + B1CentralGlassWidth / 2;
                float sill = h + 0.45f, head = sill + B1CentralGlassHeight;
                B1WindowGrid(glazing, a, b, sill, head, front - 4.0f, B1CentralGlassWidth / 4, "Ivory");
                foreach (float x in new[] { a, b })
                    glazing.Box("Ivory", new Vector3(x, (sill + head) / 2, front - 3.96f), new Vector3(0.10f, head - sill + 0.10f, 0.15f));
                foreach (float y in new[] { sill, head })
                    glazing.Box("Ivory", new Vector3(center, y, front - 3.96f), new Vector3(B1CentralGlassWidth + 0.1f, 0.10f, 0.15f));
                // Short suspended screens use the same silver blades and orange header as the OCB screen.
                a = axes[bay] + 0.51f; b = axes[bay + 1] - 0.51f;
                float screenBottom = B1FrameTop - 1.55f, screenTop = B1FrameTop - 0.35f;
                float z = front - 0.25f;
                for (float y = screenBottom + 0.05f; y < screenTop; y += 0.145f)
                    screens.Box("Silver", new Vector3((a + b) / 2, y, z), new Vector3(b - a, 0.065f, 0.20f));
                for (int support = 0; support <= 3; support++)
                    screens.Box("Metal", new Vector3(Mathf.Lerp(a, b, support / 3f), (screenBottom + screenTop) / 2, z - 0.055f), new Vector3(0.055f, screenTop - screenBottom + 0.12f, 0.12f));
                screens.Box("Orange", new Vector3((a + b) / 2, screenTop - 0.03f, z + 0.02f), new Vector3(b - a, 0.13f, 0.23f));
                screens.Box("Silver", new Vector3((a + b) / 2, screenBottom - 0.025f, z), new Vector3(b - a, 0.055f, 0.22f));
            }
            glazing.Finish(); screens.Finish();
        }

        private static float[] B1RightWindowCenters(float a, float b)
        {
            var centers = new float[4];
            float pitch = (b - a) / centers.Length;
            for (int i = 0; i < centers.Length; i++) centers[i] = a + pitch * (i + 0.5f);
            return centers;
        }

        private static void BuildB1FourRightWindowStacks(Transform root, float h, float front, float a, float b)
        {
            var windows = new Part("10_Right_four_even_window_stacks", root, "B1BalconyDetail");
            var centers = B1RightWindowCenters(a, b);
            float z = front - 0.5f, middle = h + (B1FrameTop - h) / 2;
            // Four equal-width wall openings per floor, each with a tall two-leaf window and a high transom.
            foreach (float floor in new[] { h, middle })
            {
                float sill = floor + 0.15f, head = sill + B1RightGlassHeight;
                float cursor = a + 0.4f;
                foreach (float center in centers)
                {
                    float x0 = center - B1RightGlassWidth / 2, x1 = center + B1RightGlassWidth / 2;
                    if (x0 > cursor)
                        windows.Box("Ivory", new Vector3((cursor + x0) / 2, (sill + head) / 2, z - 0.06f), new Vector3(x0 - cursor, head - sill, 0.3f));
                    windows.Box("BlueGlass", new Vector3(center, (sill + head) / 2, z - 0.10f), new Vector3(B1RightGlassWidth, head - sill, 0.09f));
                    foreach (float x in new[] { x0, x1 })
                        windows.Box("White", new Vector3(x, (sill + head) / 2, z + 0.05f), new Vector3(0.18f, head - sill + 0.16f, 0.30f));
                    foreach (float y in new[] { sill, head })
                        windows.Box("White", new Vector3(center, y, z + 0.05f), new Vector3(B1RightGlassWidth + 0.18f, 0.16f, 0.30f));
                    windows.Box("Silver", new Vector3(center, (sill + head) / 2, z + 0.08f), new Vector3(0.055f, head - sill, 0.10f));
                    windows.Box("Silver", new Vector3(center, head - 0.55f, z + 0.08f), new Vector3(B1RightGlassWidth, 0.055f, 0.10f));
                    cursor = x1;
                }
                if (cursor < b - 0.4f)
                    windows.Box("Ivory", new Vector3((cursor + b - 0.4f) / 2, (sill + head) / 2, z - 0.06f), new Vector3(b - 0.4f - cursor, head - sill, 0.3f));
            }
            foreach (float y in new[] { h + 0.2f, middle + 0.2f, B1FrameTop + 0.05f })
                windows.Box("White", new Vector3((a + b) / 2, y, z + 0.05f), new Vector3(b - a - 0.7f, 0.25f, 0.4f));
            // Solid spandrels close the joints between the two rows and the surrounding floor/roof beams.
            windows.Box("Ivory", new Vector3((a + b) / 2, h + 0.07f, z - 0.06f), new Vector3(b - a - 0.7f, 0.22f, 0.30f));
            windows.Box("Ivory", new Vector3((a + b) / 2, middle, z - 0.06f), new Vector3(b - a - 0.7f, 0.60f, 0.30f));
            windows.Box("Ivory", new Vector3((a + b) / 2, B1FrameTop - 0.08f, z - 0.06f), new Vector3(b - a - 0.7f, 0.30f, 0.30f));
            windows.Finish();
        }

        private static void SetupB1BalconyDetailViewpoints(Transform views)
        {
            foreach (Transform child in new List<Transform>(GetDirectChildren(views)))
            { int n; if (child.name.Length > 2 && int.TryParse(child.name.Substring(0, 2), out n) && n >= 94 && n <= 99) UnityEngine.Object.DestroyImmediate(child.gameObject); }
            Viewpoint(views, "94_B1_central_balcony_finished", new Vector3(5, 7.5f, 18), new Vector3(5, 7.7f, -0.6f));
            Viewpoint(views, "95_B1_left_balcony_end_wall", new Vector3(8, 6.2f, 2.0f), new Vector3(14.3f, 7.4f, 0.4f));
            Viewpoint(views, "96_B1_right_balcony_end_wall", new Vector3(2, 6.2f, 2.0f), new Vector3(-4.1f, 7.4f, 0.1f));
            Viewpoint(views, "97_B1_narrow_taller_central_glass", new Vector3(6, 7, 2.4f), new Vector3(9.6f, 7.5f, -0.8f));
            Viewpoint(views, "98_B1_two_suspended_louvre_screens", new Vector3(5, 9, 18), new Vector3(5, 9.5f, 2.8f));
            Viewpoint(views, "99_B1_four_even_right_window_stacks", new Vector3(-12.95f, 12, 15), new Vector3(-12.95f, 7.7f, 2.6f));
        }

        private static void CaptureB1BalconyDetailPreviews()
        {
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            foreach (Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                int number; if (point.name.Length < 3 || !int.TryParse(point.name.Substring(0, 2), out number) || number < 94 || number > 99) continue;
                var go = new GameObject("Temporary B1 balcony detail camera"); var camera = go.AddComponent<Camera>(); camera.enabled = false;
                camera.transform.SetPositionAndRotation(point.position, point.rotation);
                camera.fieldOfView = number == 95 || number == 96 ? 72 : number == 97 ? 65 : number == 99 ? 58 : 64;
                camera.nearClipPlane = 0.06f; camera.farClipPlane = 200;
                var target = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                var prior = RenderTexture.active; camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                var texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = prior; target.Release();
                UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Serializable] private sealed class B1BalconyDetailsLayout
        {
            public string status = "Native Unity facade refinement from user reference images; dimensions are visual estimates.";
            public int centralEndWallCount = 2, centralGlassWallCount = 2, suspendedScreenCount = 2, rightWindowStacks = 4, rightWindowRows = 2;
            public float centralGlassWidth = B1CentralGlassWidth, centralGlassHeight = B1CentralGlassHeight;
            public float rightGlassWidth = B1RightGlassWidth, rightGlassHeight = B1RightGlassHeight;
            public float centralGlassBottom, centralGlassTop, hangingScreenBottom, hangingScreenTop;
            public float[] centralWindowCentersX, endWallCentersX, rightWindowCentersX, rightWindowBottoms;
        }
    }
}
#endif
