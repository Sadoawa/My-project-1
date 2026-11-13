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
        private static float B1FrameFrontZ => dimensions.b1FrameFrontZ;
        private static float B1FrameRearZ => -Mathf.Max(dimensions.lobbyDepth, dimensions.exteriorDepth) - dimensions.b1FrameRearOverrun;
        private static float B1FrameTop => dimensions.lobbyHeight + 6.3f;
        private static float B1FrameLeftX => Mathf.Max(dimensions.frontage / 2, OcbCenterX + OcbWidth / 2) + 0.6f;
        private static float B1FrameRightX => Mathf.Min(RightCornerColumnX(dimensions), RightFacadeX(dimensions) - FacultyRoomDepth - 0.8f);
        private static float[] B1FrontPierAxes => new[] { B1FrameRightX, -dimensions.frontage * 0.125f, dimensions.frontage * 0.125f, LeftGateColumnX(dimensions), B1FrameLeftX };
        private static float[] B1SidePierDepths
        {
            get
            {
                var depths = new List<float> { B1FrameFrontZ };
                for (float z = -8; z > B1FrameRearZ + 0.1f; z -= 8) depths.Add(z);
                depths.Add(B1FrameRearZ);
                return depths.ToArray();
            }
        }

        [MenuItem("UEH/Campus B/28. Rebuild B1 structural frame around full lobby")]
        public static void RefineB1FrameAndCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.ocbBranch || !dimensions.expandedLobby)
                throw new InvalidOperationException("Build the current expanded lobby and aligned OCB bay first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[m.name] = m; }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.wrappedB1Frame = true;
            dimensions.b1FrameFrontZ = 3.2f;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_Exterior.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (string name in new[] { "02_Podium_structure", "03_Balcony_1", "03_Balcony_2", "03_Balcony_3", "04_Horizontal_louvres", "05_University_identity", "University_name", "Campus_address", "18_B1_structural_frame" })
                { var old = contents.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                foreach (Transform child in new List<Transform>(GetDirectChildren(contents.transform)))
                    if (child.name.StartsWith("06_Tower_") || child.name.StartsWith("07_Tower_")) UnityEngine.Object.DestroyImmediate(child.gameObject);
                BuildB1FrameEnvelope(contents.transform);
                BuildB1ReferenceTower(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            RemoveFloatingB1EntranceLabel();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupB1FrameViewpoints();
            Physics.SyncTransforms();
            var frame = GameObject.Find("18_B1_structural_frame").transform;
            bool centralClear = B1FrameGateClear(frame, 0);
            bool sinceClear = B1FrameGateClear(frame, SinceGateCenterX(dimensions));
            bool containsLobby = B1FrameRightX - 0.4f <= -dimensions.frontage / 2 && B1FrameLeftX + 0.4f >= dimensions.frontage / 2 && B1FrameRearZ < -dimensions.lobbyDepth;
            if (!centralClear || !sinceClear || !containsLobby) throw new InvalidOperationException("B1 frame does not enclose the lobby or obstructs an entrance.");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            CaptureB1FramePreviews();
            CaptureB1FacadePreviews();
            CaptureB1BalconyDetailPreviews();
            CaptureB1FourthFloorPreviews();
            CaptureB1CanopyPreviews();
            CaptureB1RaisedSignPreviews();
            CaptureB1TowerProportionPreviews();
            CaptureB1FloorClosurePreviews();
            CaptureB1TowerAlignmentPreviews();
            CaptureB1LandscapePreviews();
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/b1-frame-layout.json"), JsonUtility.ToJson(new B1FrameLayout
            {
                frontPierAxes = B1FrontPierAxes, sidePierZ = B1SidePierDepths,
                minX = B1FrameRightX - 0.4f, maxX = B1FrameLeftX + 0.4f,
                frontZ = B1FrameFrontZ, rearZ = B1FrameRearZ, topY = B1FrameTop,
                porchDepthFromEntranceGlass = B1FrameFrontZ - EntranceGlassZ(dimensions),
                upperTerraceFloors = new[] { B1FrameTop, B1FrameTop + 3.6f, B1FrameTop + 7.2f },
                lobbyDepth = dimensions.lobbyDepth, rearOverrun = -B1FrameRearZ - dimensions.lobbyDepth,
                centralEntranceClearOfFrame = centralClear, sinceEntranceClearOfFrame = sinceClear, coversLobby = containsLobby
            }, true));
            Debug.Log("UEH_B1_FRAME_COMPLETE Five front structural piers; continuous side and rear frame to Z=" + B1FrameRearZ + "; full lobby enclosed with " + dimensions.b1FrameRearOverrun + "m rear overrun; both entrance lanes clear.");
        }

        private static void BuildB1FrameEnvelope(Transform exteriorRoot)
        {
            EnsureB1LandscapeMaterials();
            Mat("B1Sandstone", "DCC8B5", 0.22f);
            Mat("B1Basalt", "3F4140", 0.16f);
            float h = dimensions.lobbyHeight, right = B1FrameRightX, left = B1FrameLeftX;
            float span = left - right, centerX = (left + right) / 2;
            float rear = B1FrameRearZ, depth = B1FrameFrontZ - rear, centerZ = (B1FrameFrontZ + rear) / 2;

            BuildB1ReferenceEnvelope(exteriorRoot, h, right, left, rear);
        }

        private static void AddB1FramePier(Part p, float x, float z, bool front, int outwardSide)
        {
            float top = B1FrameTop;
            p.Box("Ivory", new Vector3(x, top / 2, z), new Vector3(0.76f, top, 0.76f), true);
            p.Box("B1Basalt", new Vector3(x, 0.3f, z), new Vector3(0.83f, 0.6f, 0.83f));
            p.Box("Ivory", new Vector3(x, 0.08f, z), new Vector3(1.02f, 0.16f, 1.02f));
            if (front)
            {
                p.Box("B1Sandstone", new Vector3(x, (top + 0.65f) / 2, z + 0.399f), new Vector3(0.24f, top - 0.65f, 0.055f));
                foreach (float offset in new[] { -0.32f, 0.32f })
                    p.Box("White", new Vector3(x + offset, (top + 0.65f) / 2, z + 0.41f), new Vector3(0.1f, top - 0.65f, 0.09f));
            }
            else if (outwardSide != 0)
            {
                p.Box("B1Sandstone", new Vector3(x + outwardSide * 0.399f, (top + 0.65f) / 2, z), new Vector3(0.055f, top - 0.65f, 0.24f));
                foreach (float offset in new[] { -0.32f, 0.32f })
                    p.Box("White", new Vector3(x + outwardSide * 0.41f, (top + 0.65f) / 2, z + offset), new Vector3(0.09f, top - 0.65f, 0.1f));
            }
        }

        private static bool B1FrameGateClear(Transform frame, float x)
        {
            var lane = new Bounds(new Vector3(x, 1.05f, 0), new Vector3(0.95f, 1.85f, 6));
            foreach (var collider in frame.Find("01_Frame_columns_and_ring_beams").GetComponentsInChildren<BoxCollider>())
                if (collider.bounds.Intersects(lane)) return false;
            return true;
        }

        private static void SetupB1FrameViewpoints()
        {
            var views = GameObject.Find("Review viewpoints [select and align view]"); if (views == null) return;
            foreach (string name in new[] { "82_B1_frame_front", "83_B1_frame_virtual_tour_angle", "84_B1_frame_OCB_porch", "85_B1_frame_full_side_depth", "86_B1_frame_rear_overrun", "87_B1_frame_lobby_cutaway" })
            { var old = views.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            SetupB1FacadeViewpoints(views.transform);
            Viewpoint(views.transform, "82_B1_frame_front", new Vector3(2, 7.2f, 37), new Vector3(1, 6, -1));
            Viewpoint(views.transform, "83_B1_frame_virtual_tour_angle", new Vector3(29, 2, 20), new Vector3(3, 9, -1));
            Viewpoint(views.transform, "84_B1_frame_OCB_porch", new Vector3(30, 2.1f, 9), new Vector3(18, 4.3f, -5));
            Viewpoint(views.transform, "85_B1_frame_full_side_depth", new Vector3(61, 20, -26), new Vector3(15, 5.5f, -26));
            Viewpoint(views.transform, "86_B1_frame_rear_overrun", new Vector3(31, 8, -65), new Vector3(4, 5, -45));
            Viewpoint(views.transform, "87_B1_frame_lobby_cutaway", new Vector3(58, 44, 18), new Vector3(2, 1.5f, -24));
        }

        private static void CaptureB1FramePreviews()
        {
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            foreach (Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                int n; if (point.name.Length < 3 || !int.TryParse(point.name.Substring(0, 2), out n) || n < 82 || n > 87) continue;
                var disabled = new List<Renderer>();
                try
                {
                    if (n == 87)
                    {
                        var exteriorObject = GameObject.Find("18_B1_structural_frame").transform.parent;
                        foreach (var r in exteriorObject.GetComponentsInChildren<Renderer>())
                        {
                            bool keep = r.transform.IsChildOf(GameObject.Find("01_Frame_columns_and_ring_beams").transform) || r.transform.IsChildOf(exteriorObject.Find("01_Forecourt_and_pavement"));
                            if (!keep && r.enabled) { r.enabled = false; disabled.Add(r); }
                        }
                        foreach (var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                        {
                            string path = AnimationUtility.CalculateTransformPath(r.transform, null);
                            bool ceiling = path.Contains("04_Timber_ceiling_and_downlights") || path.Contains("11_Central_round_spotlights") || (r.name == "White" && (path.Contains("01_Floor_and_walls/") || path.Contains("17_OCB_bank_branch/01_Room_shell")));
                            if (ceiling && r.enabled) { r.enabled = false; disabled.Add(r); }
                        }
                    }
                    var go = new GameObject("Temporary B1 frame review camera"); var camera = go.AddComponent<Camera>(); camera.enabled = false;
                    camera.transform.SetPositionAndRotation(point.position, point.rotation);
                    camera.fieldOfView = n == 82 ? 50 : n == 84 ? 67 : n == 87 ? 62 : 68;
                    camera.nearClipPlane = 0.08f; camera.farClipPlane = 250;
                    var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                    var prior = RenderTexture.active; camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                    var texture = new Texture2D(1600, 1000, TextureFormat.RGB24, false); texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); texture.Apply();
                    File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                    camera.targetTexture = null; RenderTexture.active = prior; rt.Release();
                    UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(go);
                }
                finally { foreach (var r in disabled) if (r != null) r.enabled = true; }
            }
        }

        [Serializable] private sealed class B1FrameLayout
        {
            public string status = "Native Unity geometry and rendered previews; dimensions estimated from user-provided Virtual Tour images; cutaway 87 hides roofs for review only.";
            public float[] frontPierAxes, sidePierZ;
            public float minX, maxX, frontZ, rearZ, topY, lobbyDepth, rearOverrun, porchDepthFromEntranceGlass;
            public float[] upperTerraceFloors;
            public bool centralEntranceClearOfFrame, sinceEntranceClearOfFrame, coversLobby;
        }
    }
}
#endif
