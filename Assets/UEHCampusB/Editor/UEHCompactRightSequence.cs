#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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
        // Estimated from the supplied tour views; keep all right-side builders on one layout.
        public static float RightServiceZ(UEHDimensions s) => s != null && s.compactRightSequence ? -17.2f : -19.5f;
        public static float RightLiftBankZ(UEHDimensions s) => s != null && s.compactRightSequence ? -25 : -27;
        public static float RightOfficeZ(UEHDimensions s) => s != null && s.extendedGlassOfficeAndClosedRear
            ? -39.3f : s != null && s.compactRightSequence ? -36.2f : -38.5f;
        private const float WelcomeGreenRearDepth = 14.15f;
        private const float ReservedFireWallWidth = 1.3f;

        [MenuItem("UEH/Campus B/21. Compact right areas reserve fire wall and center railing joint")]
        public static void CompactRightSequence()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.rightGlassOffice || !dimensions.refinedLobbyLayout)
                throw new InvalidOperationException("Build the existing right services, glass office and central columns first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[mat.name] = mat; }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.compactRightSequence = true;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var areas = contents.transform.Find("09_Four_expansion_areas");
                PopulateRightLiftsAndServices(areas);
                PopulateRightGlassOffice(areas);
                foreach (string name in new[] { "01_Floor_and_walls", "10_Lift_side_glass_railing" })
                { var old = contents.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                BuildExpandedShell(contents.transform);
                BuildLiftSideGlassRailing(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            RebuildCompactRightExteriorOpening();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupCompactRightViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_COMPACT_RIGHT_UPDATED Empty stone fire-equipment wall 1.3m beside WELCOME greenery; service Z=-17.2, lifts Z=-25, office Z=-36.2. L railing starts at column front-face center and returns to lift-C/corridor dividing pier. Dimensions estimated from images.");
        }

        private static void BuildReservedFireWall(Transform lobby)
        {
            if (!dimensions.compactRightSequence) return;
            var old = lobby.Find("13_Reserved_fire_equipment_wall");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var root = RightGroup(lobby, "13_Reserved_fire_equipment_wall",
                new Vector3(RightFacadeX(dimensions), 0, -WelcomeGreenRearDepth - ReservedFireWallWidth / 2));
            root.localRotation = Quaternion.Euler(0, 90, 0);
            var wall = new Part("Empty_stone_wall", root, "CompactRight");
            // Front face matches the greenery plane. Leave it bare for later equipment placement.
            wall.Box("Stone", new Vector3(0, 2.1f, -0.11f), new Vector3(ReservedFireWallWidth, 4.2f, 0.22f), true);
            wall.Box("Border", new Vector3(0, 0.055f, 0.015f), new Vector3(ReservedFireWallWidth, 0.11f, 0.06f));
            wall.Finish();
            var marker = RightGroup(root, "Reserved_fire_equipment_mount", new Vector3(0, 1.6f, 0.16f));
            var space = marker.gameObject.AddComponent<UEHReservedSpace>();
            space.size = new Vector3(1.1f, 2.8f, 0.3f);
            space.furnished = false;
            space.purpose = "Tường đá trống dành cho bình/tủ chữa cháy cạnh cây xanh WELCOME. Chưa lắp thiết bị; kích thước ước lượng theo ảnh.";
        }

        // Replace only the existing ground-floor right cladding boxes. The original full box
        // was already split by earlier edits, so the older one-shot trim cannot move this opening.
        private static void RebuildCompactRightExteriorOpening()
        {
            string path = Root + "/Prefabs/UEH_Exterior.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var filter = contents.transform.Find("02_Podium_structure/Ivory").GetComponent<MeshFilter>();
                var source = filter.sharedMesh;
                var vertices = source.vertices; var normals = source.normals; var uv = source.uv;
                var positions = new List<Vector3>(); var keptNormals = new List<Vector3>();
                var keptUV = new List<Vector2>(); var triangles = new List<int>();
                var remap = new int[vertices.Length];
                for (int i = 0; i < remap.Length; i++) remap[i] = -1;
                for (int start = 0; start < vertices.Length; start += 24)
                {
                    int count = Mathf.Min(24, vertices.Length - start);
                    var bounds = new Bounds(vertices[start], Vector3.zero);
                    for (int i = 1; i < count; i++) bounds.Encapsulate(vertices[start + i]);
                    bool replace = Mathf.Abs(bounds.center.x + dimensions.frontage / 2) < 0.02f
                        && bounds.size.x < 0.5f && bounds.min.y >= 0.74f && bounds.max.y <= dimensions.lobbyHeight;
                    if (replace) continue;
                    for (int i = 0; i < count; i++)
                    {
                        int index = start + i; remap[index] = positions.Count;
                        positions.Add(vertices[index]); keptNormals.Add(normals[index]); keptUV.Add(uv[index]);
                    }
                }
                var original = source.triangles;
                for (int i = 0; i < original.Length; i += 3)
                    if (remap[original[i]] >= 0 && remap[original[i + 1]] >= 0 && remap[original[i + 2]] >= 0)
                    { triangles.Add(remap[original[i]]); triangles.Add(remap[original[i + 1]]); triangles.Add(remap[original[i + 2]]); }
                var kept = new Mesh { indexFormat = IndexFormat.UInt32 };
                kept.SetVertices(positions); kept.SetNormals(keptNormals); kept.SetUVs(0, keptUV); kept.SetTriangles(triangles, 0);
                var geometry = new Geometry(); geometry.Append(kept, Matrix4x4.identity);
                foreach (var fragment in ServiceExteriorWallFragments(dimensions.frontage, dimensions.exteriorDepth, dimensions.lobbyHeight))
                    geometry.Box(fragment.Key, fragment.Value);
                UnityEngine.Object.DestroyImmediate(kept);
                var mesh = geometry.ToMesh("CompactRight_Exterior_podium_Ivory");
                string meshPath = Root + "/Meshes/CompactRight_Exterior_podium_Ivory.asset";
                var saved = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if (saved == null) { AssetDatabase.CreateAsset(mesh, meshPath); saved = mesh; }
                else
                {
                    saved.Clear(); saved.indexFormat = mesh.indexFormat; saved.vertices = mesh.vertices;
                    saved.normals = mesh.normals; saved.uv = mesh.uv; saved.triangles = mesh.triangles; saved.tangents = mesh.tangents;
                    saved.RecalculateBounds(); saved.UploadMeshData(false); EditorUtility.SetDirty(saved);
                    UnityEngine.Object.DestroyImmediate(mesh);
                }
                filter.sharedMesh = saved;
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }

        private static void SetupCompactRightViewpoints()
        {
            var views = GameObject.Find("Review viewpoints [select and align view]"); if (views == null) return;
            foreach (string name in new[] { "55_Compact_right_WELCOME_services", "56_Empty_fire_equipment_wall", "57_Railing_middle_column_joint", "58_Compact_right_lifts_office" })
            { var old = views.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Viewpoint(views.transform, "55_Compact_right_WELCOME_services", new Vector3(0.5f, 2, -9), new Vector3(-13.4f, 1.9f, -19.5f));
            Viewpoint(views.transform, "56_Empty_fire_equipment_wall", new Vector3(-7, 1.8f, -14.5f), new Vector3(-13.4f, 2, -14.8f));
            Viewpoint(views.transform, "57_Railing_middle_column_joint", new Vector3(-0.6f, 2.8f, -18.2f), new Vector3(-6.5f, 0.9f, -22.8f));
            Viewpoint(views.transform, "58_Compact_right_lifts_office", new Vector3(-5.7f, 1.8f, -29), new Vector3(-13.4f, 1.9f, -30.5f));
        }

        // Native asset generation and image capture only; no Play Mode or automated tests.
        public static void CompactRightAndCapture()
        {
            CompactRightSequence();
            CaptureReflections();
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            var views = GameObject.Find("Review viewpoints [select and align view]").transform;
            foreach (Transform point in views)
            {
                if (!point.name.StartsWith("55_") && !point.name.StartsWith("56_") && !point.name.StartsWith("57_") && !point.name.StartsWith("58_")) continue;
                var cameraGO = new GameObject("Temporary compact-right review camera");
                var camera = cameraGO.AddComponent<Camera>(); camera.enabled = false;
                camera.transform.SetPositionAndRotation(point.position, point.rotation);
                camera.fieldOfView = point.name.StartsWith("56_") ? 62 : 82;
                camera.nearClipPlane = 0.08f; camera.farClipPlane = 240;
                var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                var prior = RenderTexture.active; camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                var texture = new Texture2D(1600, 1000, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = prior; rt.Release();
                UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(cameraGO);
            }
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/compact-right-layout.json"), JsonUtility.ToJson(new CompactRightLayout
            {
                serviceZ = ServiceCorridorZ, liftBankZ = LiftBankZ, glassOfficeZ = GlassOfficeZ,
                fireWallWidth = ReservedFireWallWidth, columnJoint = RailingColumnAnchor(dimensions),
                railingElbow = RailingElbow(dimensions), wallJoint = RailingWallAnchor(dimensions)
            }, true));
            Debug.Log("UEH_COMPACT_RIGHT_ASSETS_AND_PREVIEWS_SAVED");
        }

        [Serializable] private sealed class CompactRightLayout
        {
            public string status = "Native Unity asset generation and four rendered previews; no Play Mode tests run for this revision.";
            public float serviceZ, liftBankZ, glassOfficeZ, fireWallWidth;
            public Vector3 columnJoint, railingElbow, wallJoint;
        }
    }
}
#endif
