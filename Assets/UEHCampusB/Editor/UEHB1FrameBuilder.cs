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
        private const float B1FrameFrontZ = 0.8f;
        private static float B1FrameRearZ => -Mathf.Max(dimensions.lobbyDepth, dimensions.exteriorDepth) - dimensions.b1FrameRearOverrun;
        private static float B1FrameTop => dimensions.lobbyHeight + 7.8f;
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
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_Exterior.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (string name in new[] { "02_Podium_structure", "03_Balcony_1", "03_Balcony_2", "03_Balcony_3", "04_Horizontal_louvres", "05_University_identity", "University_name", "Campus_address", "18_B1_structural_frame" })
                { var old = contents.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                BuildB1FrameEnvelope(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
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
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/b1-frame-layout.json"), JsonUtility.ToJson(new B1FrameLayout
            {
                frontPierAxes = B1FrontPierAxes, sidePierZ = B1SidePierDepths,
                minX = B1FrameRightX - 0.4f, maxX = B1FrameLeftX + 0.4f,
                frontZ = B1FrameFrontZ, rearZ = B1FrameRearZ, topY = B1FrameTop,
                lobbyDepth = dimensions.lobbyDepth, rearOverrun = -B1FrameRearZ - dimensions.lobbyDepth,
                centralEntranceClearOfFrame = centralClear, sinceEntranceClearOfFrame = sinceClear, coversLobby = containsLobby
            }, true));
            Debug.Log("UEH_B1_FRAME_COMPLETE Five front structural piers; continuous side and rear frame to Z=" + B1FrameRearZ + "; full lobby enclosed with " + dimensions.b1FrameRearOverrun + "m rear overrun; both entrance lanes clear.");
        }

        private static void BuildB1FrameEnvelope(Transform exteriorRoot)
        {
            Mat("B1Sandstone", "DCC8B5", 0.22f);
            Mat("B1Basalt", "3F4140", 0.16f);
            float h = dimensions.lobbyHeight, right = B1FrameRightX, left = B1FrameLeftX;
            float span = left - right, centerX = (left + right) / 2;
            float rear = B1FrameRearZ, depth = B1FrameFrontZ - rear, centerZ = (B1FrameFrontZ + rear) / 2;

            // Keep the existing ground-floor service/office wall openings; replace the old short upper podium.
            var cladding = new Part("02_Podium_structure", exteriorRoot, "B1Frame");
            foreach (var f in ServiceExteriorWallFragments(dimensions.frontage, dimensions.exteriorDepth, h)) cladding.Box("Ivory", f.Key, f.Value);
            foreach (var f in OfficeExteriorWallFragments(dimensions.frontage, dimensions.exteriorDepth, h)) cladding.Box("Ivory", f.Key, f.Value);
            cladding.Finish();

            var frame = RightGroup(exteriorRoot, "18_B1_structural_frame", Vector3.zero);
            var members = new Part("01_Frame_columns_and_ring_beams", frame, "B1Frame");
            foreach (float x in B1FrontPierAxes) AddB1FramePier(members, x, B1FrameFrontZ, true, 0);
            var depths = B1SidePierDepths;
            for (int i = 1; i < depths.Length; i++)
            {
                AddB1FramePier(members, right, depths[i], false, -1);
                AddB1FramePier(members, left, depths[i], false, 1);
            }
            for (int i = 1; i < B1FrontPierAxes.Length - 1; i++)
                AddB1FramePier(members, B1FrontPierAxes[i], rear, false, 0);

            var slabs = new Part("02_Podium_decks_and_soffits", frame, "B1Frame");
            for (int level = 0; level < 3; level++)
            {
                float y = h + level * 3.9f;
                // Full-length slabs join every front pier to the side/rear frame, including beyond the lobby end.
                slabs.Box("Ivory", new Vector3(centerX, y + 0.14f, centerZ), new Vector3(span + 1.4f, 0.28f, depth + 1.5f));
                members.Box("White", new Vector3(centerX, y + 0.05f, B1FrameFrontZ), new Vector3(span + 0.7f, 0.5f, 0.75f));
                members.Box("White", new Vector3(centerX, y + 0.05f, rear), new Vector3(span + 0.7f, 0.5f, 0.75f));
                foreach (float x in new[] { right, left })
                    members.Box("White", new Vector3(x, y + 0.05f, centerZ), new Vector3(0.75f, 0.5f, depth));
                foreach (float x in new[] { -dimensions.frontage * 0.125f, dimensions.frontage * 0.125f, LeftGateColumnX(dimensions) })
                    members.Box("Ivory", new Vector3(x, y + 0.1f, centerZ), new Vector3(0.38f, 0.36f, depth));
                for (int bay = 1; bay < depths.Length - 1; bay++)
                    members.Box("Ivory", new Vector3(centerX, y + 0.1f, depths[bay]), new Vector3(span, 0.3f, 0.38f));
                // Thin layered cornices, front and on both returns.
                slabs.Box("White", new Vector3(centerX, y + 0.31f, B1FrameFrontZ + 0.66f), new Vector3(span + 1.6f, 0.09f, 0.3f));
                slabs.Box("White", new Vector3(centerX, y + 0.31f, rear - 0.66f), new Vector3(span + 1.6f, 0.09f, 0.3f));
                foreach (float x in new[] { right - 0.66f, left + 0.66f })
                    slabs.Box("White", new Vector3(x, y + 0.31f, centerZ), new Vector3(0.3f, 0.09f, depth + 1.5f));
                slabs.Box("Wood", new Vector3(centerX, y - 0.205f, 0.15f), new Vector3(span - 0.8f, 0.045f, 2.5f));
            }
            members.Finish(); slabs.Finish();

            BuildB1BalconiesAndInfill(frame, h, right, left, rear);
            var paving = new Part("04_Side_and_rear_paving", frame, "B1Frame");
            float outerRight = right - 0.9f, outerLeft = left + 0.9f;
            float lobbyHalf = dimensions.frontage / 2;
            paving.Box("Pavement", new Vector3((outerRight - lobbyHalf) / 2, -0.15f, centerZ), new Vector3(-lobbyHalf - outerRight, 0.3f, depth + 1.8f), true);
            paving.Box("Pavement", new Vector3((outerLeft + lobbyHalf) / 2, -0.15f, centerZ), new Vector3(outerLeft - lobbyHalf, 0.3f, depth + 1.8f), true);
            float extra = -rear - dimensions.lobbyDepth + 0.9f;
            paving.Box("Pavement", new Vector3(centerX, -0.15f, -dimensions.lobbyDepth - extra / 2), new Vector3(span + 1.8f, 0.3f, extra), true);
            paving.Finish();

            var identity = new Part("05_University_identity", exteriorRoot, "B1Frame");
            identity.Box("Ivory", new Vector3(centerX, h + 5.5f, 1.10f), new Vector3(span + 0.4f, 1.2f, 0.23f));
            identity.Box("White", new Vector3(centerX, h + 6.15f, 1.10f), new Vector3(span + 0.65f, 0.1f, 0.32f));
            identity.Finish();
            Text(exteriorRoot, "University_name", "ĐẠI HỌC KINH TẾ TP. HỒ CHÍ MINH", new Vector3(centerX, h + 5.5f, 1.24f), span - 1.8f, 0.82f, "Teal");
            Text(exteriorRoot, "Campus_address", "UEH  ·  279 NGUYỄN TRI PHƯƠNG", new Vector3(0, h - 0.49f, 1.22f), 11.2f, 0.3f, "Teal");
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

        private static void BuildB1BalconiesAndInfill(Transform frame, float h, float right, float left, float rear)
        {
            var balcony = new Part("03_Balconies_louvres_and_windows", frame, "B1Frame");
            var axes = B1FrontPierAxes;
            // Two open central balcony bays with recessed glazing, metal balusters and flowering planters.
            for (int bay = 1; bay < axes.Length - 2; bay++)
            {
                float a = axes[bay] + 0.6f, b = axes[bay + 1] - 0.6f, mid = (a + b) / 2;
                for (int level = 0; level < 2; level++)
                {
                    float y = h + level * 3.9f;
                    balcony.Box("BlueGlass", new Vector3(mid, y + 1.85f, -1.8f), new Vector3(b - a, 2.8f, 0.08f));
                    balcony.Box("Metal", new Vector3(mid, y + 1.22f, 0.48f), new Vector3(b - a, 0.065f, 0.07f));
                    balcony.Box("White", new Vector3(mid, y + 0.46f, 0.48f), new Vector3(b - a, 0.16f, 0.2f));
                    for (float x = a; x <= b; x += 0.18f)
                        balcony.Box("Silver", new Vector3(x, y + 0.86f, 0.48f), new Vector3(0.028f, 0.74f, 0.038f));
                    foreach (float x in new[] { mid - 1.65f, mid + 1.65f })
                    {
                        balcony.Box("Stone", new Vector3(x, y + 0.56f, 0.11f), new Vector3(1.55f, 0.32f, 0.54f));
                        for (int i = 0; i < 7; i++)
                        {
                            Vector3 pos = new Vector3(x - 0.58f + i * 0.19f, y + 0.88f + i % 3 * 0.07f, 0.1f);
                            balcony.Sphere("Leaf", pos, new Vector3(0.42f, 0.5f, 0.48f));
                            balcony.Sphere("Flower", pos + new Vector3(0, 0.21f, 0.09f), new Vector3(0.39f, 0.26f, 0.38f));
                        }
                    }
                }
            }
            // The OCB bay carries the horizontal screen; the opposite wing is a glazed office bay.
            float screenA = axes[3] + 0.6f, screenB = axes[4] - 0.6f;
            balcony.Box("Dark", new Vector3((screenA + screenB) / 2, h + 1.95f, -0.15f), new Vector3(screenB - screenA, 3.35f, 0.10f));
            for (int i = 0; i < 22; i++)
                balcony.Box("Silver", new Vector3((screenA + screenB) / 2, h + 0.5f + i * 0.135f, 0.32f), new Vector3(screenB - screenA, 0.065f, 0.18f));
            balcony.Box("Orange", new Vector3((screenA + screenB) / 2, h + 3.53f, 0.34f), new Vector3(screenB - screenA, 0.09f, 0.2f));
            float windowA = axes[0] + 0.6f, windowB = axes[1] - 0.6f;
            for (int level = 0; level < 2; level++)
            {
                float y = h + level * 3.9f;
                balcony.Box("BlueGlass", new Vector3((windowA + windowB) / 2, y + 1.95f, -0.2f), new Vector3(windowB - windowA, 3.0f, 0.09f));
                for (float x = windowA; x <= windowB; x += 2.45f)
                    balcony.Box("White", new Vector3(x, y + 1.95f, -0.1f), new Vector3(0.13f, 3.2f, 0.16f));
                balcony.Box("White", new Vector3((windowA + windowB) / 2, y + 1.3f, -0.1f), new Vector3(windowB - windowA, 0.13f, 0.16f));
            }
            // Continue window bays down both returns to make the long ring frame legible from the side.
            var depths = B1SidePierDepths;
            for (int i = 0; i < depths.Length - 1; i++)
            {
                float a = depths[i] - 0.6f, b = depths[i + 1] + 0.6f;
                if (a <= b) continue;
                for (int level = 0; level < 2; level++)
                {
                    float y = h + level * 3.9f;
                    foreach (int side in new[] { -1, 1 })
                    {
                        float x = side < 0 ? right + 0.15f : left - 0.15f;
                        balcony.Box("BlueGlass", new Vector3(x, y + 1.94f, (a + b) / 2), new Vector3(0.08f, 3.0f, a - b));
                        for (float z = b; z < a; z += 2.1f)
                            balcony.Box("White", new Vector3(x + side * 0.08f, y + 1.94f, z), new Vector3(0.16f, 3.2f, 0.1f));
                        balcony.Box("White", new Vector3(x + side * 0.08f, y + 1.3f, (a + b) / 2), new Vector3(0.16f, 0.1f, a - b));
                    }
                }
            }
            // Upper terrace rail on the whole perimeter; ground rear stays open.
            float terrace = B1FrameTop + 0.32f;
            balcony.Box("Silver", new Vector3((left + right) / 2, terrace + 1.05f, B1FrameFrontZ + 0.4f), new Vector3(left - right + 0.7f, 0.055f, 0.065f));
            balcony.Box("Silver", new Vector3((left + right) / 2, terrace + 1.05f, rear - 0.4f), new Vector3(left - right + 0.7f, 0.055f, 0.065f));
            for (float x = right - 0.3f; x < left + 0.4f; x += 0.25f)
                foreach (float z in new[] { B1FrameFrontZ + 0.4f, rear - 0.4f })
                    balcony.Box("Silver", new Vector3(x, terrace + 0.57f, z), new Vector3(0.025f, 0.96f, 0.025f));
            foreach (float x in new[] { right - 0.4f, left + 0.4f })
            {
                balcony.Box("Silver", new Vector3(x, terrace + 1.05f, (B1FrameFrontZ + rear) / 2), new Vector3(0.065f, 0.055f, B1FrameFrontZ - rear + 0.8f));
                for (float z = rear - 0.4f; z < B1FrameFrontZ + 0.4f; z += 0.25f)
                    balcony.Box("Silver", new Vector3(x, terrace + 0.57f, z), new Vector3(0.025f, 0.96f, 0.025f));
            }
            balcony.Finish();
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
            public float minX, maxX, frontZ, rearZ, topY, lobbyDepth, rearOverrun;
            public bool centralEntranceClearOfFrame, sinceEntranceClearOfFrame, coversLobby;
        }
    }
}
#endif
