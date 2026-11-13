#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UEH.CampusB.Editor
{
    public static partial class UEHCampusBuilder
    {
        public static float LeftHeritageZ(UEHDimensions s) => s.widerLeftGaps ? -9.4f : s.compactLeftSequence ? -8.6f : -7.5f;
        public static float LeftOfficeZ(UEHDimensions s) => s.widerLeftGaps ? -22 : s.compactLeftSequence ? -20.1f : -23;
        public static float LeftLoungeZ(UEHDimensions s) => s.widerLeftGaps ? -34 : s.compactLeftSequence ? -30.95f : -37;
        private static float LeftHeritageSpacingOffset(UEHDimensions s) => s.widerLeftGaps ? 0.8f : 0;
        public static float LeftColumnZ(UEHDimensions s) => s.entranceGlassThroughColumns ? EntranceGlassZ(s) : s.compactLeftSequence ? LeftHeritageZ(s) + 5.35f + LeftHeritageSpacingOffset(s) + 0.725f + s.lobbyColumnWidth / 2 : ExpandedEntranceZ(s);

        [MenuItem("UEH/Campus B/19. Compact left heritage office lounge and slate wall")]
        public static void CompactLeftSequence()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.leftOfficeAndLounge) throw new InvalidOperationException("Build the left office/lounge first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[m.name] = m; }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.compactLeftSequence = true;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var heritage = contents.transform.Find("06_1976_LEFT_from_gate");
                heritage.localPosition = new Vector3(LeftFacadeX(dimensions), 0, LeftHeritageZ(dimensions));
                BuildHeritageContrast(heritage.Find("06_Heritage_1976_and_gallery"));
                var areas = contents.transform.Find("09_Four_expansion_areas");
                CompactLeftBays(areas);
                foreach (string name in new[] { "01_Floor_and_walls", "02_Stone_columns" })
                { var old = contents.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                BuildExpandedShell(contents.transform);
                BuildExpandedColumns(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            TrimCompactLeftExteriorWall();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupLeftAreaViewpoints();
            SetupCompactLeftViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_COMPACT_LEFT_UPDATED Slate panel 1.45x4.2m on viewer-left of 1976; stone body touches slate edge. Heritage Z=" + LeftHeritageZ(dimensions) + ", office Z=" + LeftOfficeZ(dimensions) + ", lounge Z=" + LeftLoungeZ(dimensions) + "; narrow gaps, office floor/wall/exterior opening aligned. Right wing retained.");
        }

        private static void BuildHeritageContrast(Transform heritage)
        {
            if (!dimensions.compactLeftSequence) return;
            var old = heritage.Find("00_Contrasting_slate_panel");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            Mat("HeritageSlate", "879597", 0.22f);
            var panel = new GameObject("00_Contrasting_slate_panel").transform;
            panel.SetParent(heritage, false); panel.localPosition = new Vector3(5.35f + LeftHeritageSpacingOffset(dimensions), 0, 0);
            var part = new Part("Slate_wall_and_picture", panel, "CompactLeft");
            part.Box("HeritageSlate", new Vector3(0, 2.1f, 0.025f), new Vector3(1.45f, 4.2f, 0.05f));
            foreach (float x in new[] { -0.735f, 0.735f })
                part.Box("Silver", new Vector3(x, 2.1f, 0.06f), new Vector3(0.025f, 4.2f, 0.015f));
            part.Box("Silver", new Vector3(0, 0.04f, 0.06f), new Vector3(1.45f, 0.07f, 0.03f));
            part.Box("White", new Vector3(0, 1.52f, 0.075f), new Vector3(0.94f, 0.53f, 0.05f));
            part.Box("HeritageSlate", new Vector3(0, 1.52f, 0.102f), new Vector3(0.83f, 0.42f, 0.008f));
            // Small framed abstract display, built from editable geometry.
            part.Box("Ivory", new Vector3(-0.2f, 1.52f, 0.108f), new Vector3(0.18f, 0.33f, 0.004f));
            part.Box("HeritageBlue", new Vector3(0.03f, 1.52f, 0.108f), new Vector3(0.19f, 0.33f, 0.004f));
            part.Box("HeritagePink", new Vector3(0.23f, 1.52f, 0.108f), new Vector3(0.13f, 0.33f, 0.004f));
            part.Finish();
        }

        private static void CompactLeftBays(Transform areas)
        {
            if (!dimensions.compactLeftSequence) return;
            foreach (string id in new[] { "01", "02" })
            {
                var bay = areas.Find("LEFT_" + id + " [12m x 6m]");
                bay.localPosition = new Vector3(LeftFacadeX(dimensions), 0, id == "01" ? LeftOfficeZ(dimensions) : LeftLoungeZ(dimensions));
                var marker = bay.GetComponentInChildren<UEHReservedSpace>();
                marker.size = new Vector3(10.6f, 4.1f, 6);
                marker.purpose = dimensions.widerLeftGaps
                    ? "Khoang trái đã nới khoảng tường trắng giữa các khu theo ảnh: 10.6m x 6m; tên nhóm 12m cũ giữ tương thích. Kích thước ước lượng."
                    : "Khoang trái thu sát theo ảnh: bề ngang dành chỗ 10.6m, chiều sâu 6m; tên nhóm 12m cũ giữ tương thích. Kích thước ước lượng.";
                var old = bay.Find("Floor_inlay_LEFT_" + id);
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                var inlay = new Part("Floor_inlay_LEFT_" + id, bay, "CompactLeft");
                foreach (float x in new[] { -5.3f, 5.3f }) inlay.Box("Border", new Vector3(x, 0.009f, 3.15f), new Vector3(0.1f, 0.014f, 6));
                inlay.Box("Border", new Vector3(0, 0.009f, 6.15f), new Vector3(10.6f, 0.014f, 0.1f));
                inlay.Finish();
            }
            if (dimensions.openRearAndLeftExtinguisher) BuildLeftWallExtinguisher(areas.parent);
        }

        private static void SetupCompactLeftViewpoints()
        {
            var views = GameObject.Find("Review viewpoints [select and align view]"); if (views == null) return;
            foreach (string name in new[] { "47_Slate_wall_and_1976", "48_Compact_left_sequence", "49_Office_lounge_close_joint", "50_Stone_column_beside_slate" })
            { var old = views.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Viewpoint(views.transform, "47_Slate_wall_and_1976", new Vector3(7.5f, 1.7f, -10), new Vector3(17.4f, 2, -9.8f));
            Viewpoint(views.transform, "48_Compact_left_sequence", new Vector3(5.8f, 1.85f, -20), new Vector3(17.4f, 2, -20));
            Viewpoint(views.transform, "49_Office_lounge_close_joint", new Vector3(10, 1.75f, -25.6f), new Vector3(17.4f, 2, -25.6f));
            Viewpoint(views.transform, "50_Stone_column_beside_slate", new Vector3(11, 1.7f, -5.8f), new Vector3(17.4f, 2, -2.8f));
        }

        private static void TrimCompactLeftExteriorWall()
        {
            string path = Root + "/Prefabs/UEH_Exterior.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var filter = contents.transform.Find("02_Podium_structure/Ivory").GetComponent<MeshFilter>();
                var source = filter.sharedMesh; var vertices = source.vertices;
                var positions = new List<Vector3>(); var normals = new List<Vector3>(); var uv = new List<Vector2>();
                var remap = new int[vertices.Length]; var sourceNormals = source.normals; var sourceUV = source.uv;
                for (int start = 0; start + 24 <= vertices.Length; start += 24)
                {
                    var bounds = new Bounds(vertices[start], Vector3.zero);
                    for (int i = 1; i < 24; i++) bounds.Encapsulate(vertices[start + i]);
                    bool remove = Mathf.Abs(bounds.center.x - dimensions.frontage / 2) < 0.02f && Mathf.Abs(bounds.size.x - 0.42f) < 0.02f && bounds.max.y <= dimensions.lobbyHeight + 0.01f;
                    for (int i = start; i < start + 24; i++)
                    {
                        remap[i] = remove ? -1 : positions.Count;
                        if (!remove) { positions.Add(vertices[i]); normals.Add(sourceNormals[i]); uv.Add(sourceUV[i]); }
                    }
                }
                var triangles = new List<int>(); var original = source.triangles;
                for (int i = 0; i < original.Length; i += 3)
                    if (remap[original[i]] >= 0) for (int j = 0; j < 3; j++) triangles.Add(remap[original[i + j]]);
                var kept = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                kept.SetVertices(positions); kept.SetNormals(normals); kept.SetUVs(0, uv); kept.SetTriangles(triangles, 0);
                var geometry = new Geometry(); geometry.Append(kept, Matrix4x4.identity);
                foreach (var fragment in OfficeExteriorWallFragments(dimensions.frontage, dimensions.exteriorDepth, dimensions.lobbyHeight)) geometry.Box(fragment.Key, fragment.Value);
                var mesh = geometry.ToMesh("CompactLeft_Exterior_podium_Ivory"); UnityEngine.Object.DestroyImmediate(kept);
                string meshPath = Root + "/Meshes/CompactLeft_Exterior_podium_Ivory.asset";
                var saved = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if (saved == null) { AssetDatabase.CreateAsset(mesh, meshPath); saved = mesh; }
                else
                {
                    saved.Clear(); saved.indexFormat = mesh.indexFormat;
                    saved.vertices = mesh.vertices; saved.normals = mesh.normals; saved.uv = mesh.uv;
                    saved.triangles = mesh.triangles; saved.tangents = mesh.tangents;
                    saved.RecalculateBounds(); saved.UploadMeshData(false); EditorUtility.SetDirty(saved);
                    UnityEngine.Object.DestroyImmediate(mesh);
                }
                filter.sharedMesh = saved;
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }
    }
}
#endif
