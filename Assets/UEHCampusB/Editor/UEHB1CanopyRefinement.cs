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
        private const float B1CanopyProjection = 1.25f;
        private const float B1CanopyThickness = 0.36f;

        [MenuItem("UEH/Campus B/34. Restrict timber soffits to the central bays")]
        public static void RefineB1CentralTimberAndCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.wrappedB1Frame) throw new InvalidOperationException("Build the B1 frame first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[material.name] = material; }
            string path = Root + "/Prefabs/UEH_Exterior.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var frame = contents.transform.Find("18_B1_structural_frame");
                if (frame == null) throw new InvalidOperationException("The B1 structural frame is missing.");
                var deck = frame.Find("02_Podium_decks_and_soffits");
                if (deck != null) UnityEngine.Object.DestroyImmediate(deck.gameObject);
                BuildB1PodiumDecks(frame, dimensions.lobbyHeight, B1FrameRightX, B1FrameLeftX, B1FrameRearZ);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupB1CanopyViewpoints(GameObject.Find("Review viewpoints [select and align view]").transform);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            CaptureB1FacadePreviews();
            CaptureB1CanopyPreviews();
            var axes = B1FrontPierAxes;
            Directory.CreateDirectory(Path.GetFullPath("Docs/UEH-CampusB"));
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/b1-central-timber-layout.json"), JsonUtility.ToJson(new B1CentralTimberLayout
            {
                panelLeftEdgesX = new[] { axes[1] + 0.55f, axes[2] + 0.55f },
                panelRightEdgesX = new[] { axes[2] - 0.55f, axes[3] - 0.55f },
                panelWidths = new[] { axes[2] - axes[1] - 1.1f, axes[3] - axes[2] - 1.1f },
                ceilingHeights = new[] { dimensions.lobbyHeight - 0.23f, B1FrameTop - 0.23f },
                innerZ = EntranceGlassZ(dimensions) - 0.02f, frontZ = B1FrameFrontZ - 0.16f
            }, true));
            Debug.Log("UEH_B1_CENTRAL_TIMBER_COMPLETE Four timber panels confined to the two central bays at both heights; no timber above either outer wing or the outer eave.");
        }

        [Serializable] private sealed class B1CentralTimberLayout
        {
            public string status = "Timber soffits restricted to the two central bays; four panels across two heights.";
            public int panelsPerHeight = 2, heightCount = 2;
            public float[] panelLeftEdgesX, panelRightEdgesX, panelWidths, ceilingHeights;
            public float innerZ, frontZ;
        }

        [MenuItem("UEH/Campus B/32. Extend canopy and remove deep fascia wall")]
        public static void RefineB1CanopyAndCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.wrappedB1Frame) throw new InvalidOperationException("Build the B1 frame first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[material.name] = material; }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            string path = Root + "/Prefabs/UEH_Exterior.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var frame = contents.transform.Find("18_B1_structural_frame");
                if (frame == null) throw new InvalidOperationException("The B1 structural frame is missing.");
                var deck = frame.Find("02_Podium_decks_and_soffits");
                if (deck != null) UnityEngine.Object.DestroyImmediate(deck.gameObject);
                foreach (string name in new[] { "05_University_identity", "University_name" })
                { var old = contents.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                BuildB1PodiumDecks(frame, dimensions.lobbyHeight, B1FrameRightX, B1FrameLeftX, B1FrameRearZ);
                BuildB1CanopyName(contents.transform, B1FrameRightX, B1FrameLeftX);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupB1CanopyViewpoints(GameObject.Find("Review viewpoints [select and align view]").transform);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            CaptureB1FacadePreviews();
            CaptureB1CanopyPreviews();
            Directory.CreateDirectory(Path.GetFullPath("Docs/UEH-CampusB"));
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/b1-canopy-layout.json"), JsonUtility.ToJson(new B1CanopyLayout
            {
                slabBottomY = B1FrameTop, slabTopY = B1FrameTop + B1CanopyThickness,
                outerFrontZ = B1FrameFrontZ + B1CanopyProjection,
                outerRightX = B1FrameRightX - B1CanopyProjection,
                outerLeftX = B1FrameLeftX + B1CanopyProjection,
                rearEdgeZ = B1FrameRearZ - 0.65f,
                nameCenterY = B1FrameTop + B1CanopyThickness + 1.08f
            }, true));
            Debug.Log("UEH_B1_CANOPY_COMPLETE Continuous projected canopy slab, flush side returns, old deep fascia wall removed, freestanding blue and white university lettering above the slab.");
        }

        private static void BuildB1PodiumDecks(Transform frame, float h, float right, float left, float rear)
        {
            var decks = new Part("02_Podium_decks_and_soffits", frame, "B1Frame");
            float front = B1FrameFrontZ, span = left - right, center = (left + right) / 2;
            foreach (float y in new[] { h, B1FrameTop })
            {
                bool canopy = Mathf.Abs(y - B1FrameTop) < 0.01f;
                if (canopy)
                {
                    float r = right - B1CanopyProjection, l = left + B1CanopyProjection;
                    float f = front + B1CanopyProjection, back = rear - 0.65f;
                    // One continuous horizontal slab replaces the old slab plus independent deep front wall.
                    decks.Box("Ivory", new Vector3(center, y + B1CanopyThickness / 2, (f + back) / 2), new Vector3(l - r, B1CanopyThickness, f - back));
                    decks.Box("White", new Vector3(center, y + B1CanopyThickness + 0.04f, f - 0.10f), new Vector3(l - r + 0.10f, 0.08f, 0.30f));
                    decks.Box("B1Sandstone", new Vector3(center, y + 0.035f, f - 0.06f), new Vector3(l - r, 0.07f, 0.13f));
                    foreach (float x in new[] { r + 0.10f, l - 0.10f })
                        decks.Box("White", new Vector3(x, y + B1CanopyThickness + 0.04f, (f + back) / 2), new Vector3(0.30f, 0.08f, f - back + 0.10f));
                    decks.Box("White", new Vector3(center, y + B1CanopyThickness + 0.04f, back + 0.10f), new Vector3(l - r + 0.10f, 0.08f, 0.30f));
                }
                else B1Cornice(decks, right, left, front, rear, y);
                // Four independent timber panels: two central bays at each of the two shown heights.
                // Each panel ends inside its boundary columns and behind the outer canopy edge.
                float inner = EntranceGlassZ(dimensions) - 0.02f;
                float soffitFront = front - 0.16f;
                var axes = B1FrontPierAxes;
                for (int bay = 1; bay <= 2; bay++)
                {
                    float start = axes[bay] + 0.55f, end = axes[bay + 1] - 0.55f;
                    float middle = (start + end) / 2;
                    decks.Box("Wood", new Vector3(middle, y - 0.23f, (soffitFront + inner) / 2), new Vector3(end - start, 0.05f, soffitFront - inner));
                    for (float x = start + 0.06f; x < end - 0.04f; x += 0.24f)
                        decks.Box("Wood", new Vector3(x, y - 0.28f, (soffitFront + inner) / 2), new Vector3(0.065f, 0.07f, soffitFront - inner));
                }
            }
            decks.Finish();
        }

        private static void BuildB1CanopyName(Transform parent, float right, float left)
        {
            BuildB1RaisedUniversitySign(parent, (right + left) / 2);
        }

        private static void SetupB1CanopyViewpoints(Transform views)
        {
            foreach (Transform child in new List<Transform>(GetDirectChildren(views)))
            { int n; if (child.name.Length > 3 && int.TryParse(child.name.Substring(0, 3), out n) && ((n >= 104 && n <= 107) || (n >= 110 && n <= 112))) UnityEngine.Object.DestroyImmediate(child.gameObject); }
            Viewpoint(views, "110_B1_timber_only_central_bays", new Vector3(2, 5.2f, 26), new Vector3(3, 8.0f, 0));
            Viewpoint(views, "111_B1_central_timber_ceiling_closeup", new Vector3(5, 6.2f, 1.8f), new Vector3(5, 10.5f, 0.3f));
            Viewpoint(views, "112_B1_plain_soffit_at_side_window_wing", new Vector3(-12.95f, 6.1f, 5.2f), new Vector3(-12.95f, 10.65f, 1));
            Viewpoint(views, "104_B1_extended_canopy_grazing_edge", new Vector3(28, 12.2f, 5), new Vector3(0, 10.9f, 3.8f));
            Viewpoint(views, "105_B1_canopy_without_deep_fascia", new Vector3(28, 8.8f, 13), new Vector3(9, 10.9f, 3.5f));
            Viewpoint(views, "106_B1_thin_continuous_canopy_front", new Vector3(1.1f, 11.8f, 26), new Vector3(1.1f, 10.8f, 2));
            Viewpoint(views, "107_B1_canopy_flush_return_corner", new Vector3(35, 18, 11), new Vector3(18, 11, -2));
        }

        private static void CaptureB1CanopyPreviews()
        {
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            foreach (Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                int n; if (point.name.Length < 4 || !int.TryParse(point.name.Substring(0, 3), out n) || !((n >= 104 && n <= 107) || (n >= 110 && n <= 112))) continue;
                var go = new GameObject("Temporary B1 canopy review camera"); var camera = go.AddComponent<Camera>(); camera.enabled = false;
                camera.transform.SetPositionAndRotation(point.position, point.rotation);
                camera.fieldOfView = n == 104 ? 62 : n == 110 ? 70 : n == 111 ? 94 : 68; camera.nearClipPlane = 0.06f; camera.farClipPlane = 220;
                var target = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                var prior = RenderTexture.active; camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                var texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = prior; target.Release();
                UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Serializable] private sealed class B1CanopyLayout
        {
            public string status = "Canopy refined from marked user images; dimensions are image-based estimates.";
            public float slabBottomY, slabTopY, outerFrontZ, outerRightX, outerLeftX, rearEdgeZ, nameCenterY;
            public float projectionFromColumnAxis = B1CanopyProjection, previousProjection = 0.65f, addedFrontAndSideProjection = 0.60f;
            public float slabThickness = B1CanopyThickness, removedFasciaWallHeight = 1.18f;
            public bool separateFasciaWallRemoved = true, letteringMountedOnSlabEdge = false, letteringStandingAboveSlab = true;
        }
    }
}
#endif
