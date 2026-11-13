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
        private const float B1UpperTerraceInset = 0.6f;

        // Retained for existing editor automation; all rebuild paths use the latest terrace outline.
        public static void ExpandB1FourthFloorAndCapture()
        {
            RefineB1UpperTerraceSetbackAndCapture();
        }

        [MenuItem("UEH/Campus B/35. Recess third and fourth floors slightly")]
        public static void RefineB1UpperTerraceSetbackAndCapture()
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
                var terrace = frame.Find("06_Stepped_open_terraces");
                if (terrace != null) UnityEngine.Object.DestroyImmediate(terrace.gameObject);
                BuildB1SteppedTerraces(frame, B1FrameRightX, B1FrameLeftX, B1FrameRearZ);
                BuildB1FloorClosures(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupB1FourthFloorViewpoints(GameObject.Find("Review viewpoints [select and align view]").transform);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            CaptureB1FacadePreviews();
            CaptureB1FourthFloorPreviews();
            float width = B1FrameLeftX - B1FrameRightX - 2 * B1UpperTerraceInset;
            float depth = B1FrameFrontZ - B1FrameRearZ - 2 * B1UpperTerraceInset;
            Directory.CreateDirectory(Path.GetFullPath("Docs/UEH-CampusB"));
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/b1-upper-terrace-setback-layout.json"), JsonUtility.ToJson(new B1FourthFloorLayout
            {
                rightEdgeX = B1FrameRightX + B1UpperTerraceInset, leftEdgeX = B1FrameLeftX - B1UpperTerraceInset,
                frontEdgeZ = B1FrameFrontZ - B1UpperTerraceInset, rearEdgeZ = B1FrameRearZ + B1UpperTerraceInset,
                width = width, depth = depth, floorArea = width * depth,
                lowerPodiumWidth = B1FrameLeftX - B1FrameRightX, lowerPodiumDepth = B1FrameFrontZ - B1FrameRearZ,
                towerWidth = dimensions.frontage - 3.5f,
                thirdFloorY = B1FrameTop, fourthFloorY = B1FrameTop + 3.6f, roofY = B1FrameTop + 7.2f
            }, true));
            Debug.Log("UEH_B1_UPPER_TERRACE_SETBACK_COMPLETE Both open floors recessed 0.6 m on each edge, remaining broader than the tower; columns, glazing, rails and upper cornices follow the new outline.");
        }

        private static void SetupB1FourthFloorViewpoints(Transform views)
        {
            foreach (Transform child in new List<Transform>(GetDirectChildren(views)))
            { int n; if (child.name.Length > 3 && int.TryParse(child.name.Substring(0, 3), out n) && (n >= 100 && n <= 103 || n >= 113 && n <= 116)) UnityEngine.Object.DestroyImmediate(child.gameObject); }
            Viewpoint(views, "113_B1_mild_setback_front", new Vector3(1.1f, 17, 36), new Vector3(1.1f, 14, -0.4f));
            Viewpoint(views, "114_B1_mild_setback_corner", new Vector3(34, 19, 22), new Vector3(0, 13, -4));
            Viewpoint(views, "115_B1_mild_setback_side", new Vector3(60, 19, -24), new Vector3(12, 13, -24));
            Viewpoint(views, "116_B1_mild_setback_rear", new Vector3(36, 23, -66), new Vector3(0, 12, -42));
        }

        private static void CaptureB1FourthFloorPreviews()
        {
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            foreach (Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                int number; if (point.name.Length < 4 || !int.TryParse(point.name.Substring(0, 3), out number) || number < 113 || number > 116) continue;
                var go = new GameObject("Temporary fourth-floor review camera"); var camera = go.AddComponent<Camera>(); camera.enabled = false;
                camera.transform.SetPositionAndRotation(point.position, point.rotation);
                camera.fieldOfView = number == 113 ? 52 : 68;
                camera.nearClipPlane = 0.08f; camera.farClipPlane = 220;
                var target = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                var prior = RenderTexture.active; camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                var texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = prior; target.Release();
                UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Serializable] private sealed class B1FourthFloorLayout
        {
            public string status = "Third and fourth floors mildly recessed from lower podium, remaining broader than upper tower; image-based estimated dimensions.";
            public float rightEdgeX, leftEdgeX, frontEdgeZ, rearEdgeZ, width, depth, floorArea;
            public float lowerPodiumWidth, lowerPodiumDepth, towerWidth, thirdFloorY, fourthFloorY, roofY;
            public float insetEachEdge = B1UpperTerraceInset;
            public bool sameFootprintAsLowerPodium = false;
        }
    }
}
#endif
