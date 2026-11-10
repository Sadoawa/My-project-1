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
        public const float OfficeRecessDepth = 2.8f;

        [MenuItem("UEH/Campus B/10. Recess office behind lobby side wall")]
        public static void RecessOfficeIntoWall()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before recessing the office.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.leftOfficeAndLounge)
                throw new InvalidOperationException("Furnish the left office before applying this correction.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                materials[material.name] = material;
            }
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var office = contents.transform.Find("09_Four_expansion_areas/LEFT_01 [12m x 6m]/Office_room");
                if (office == null) throw new InvalidOperationException("Missing left office.");
                AlignOfficeBehindWall(office);
                RebuildOfficeWallOpening(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            TrimOfficeExteriorGroundWall();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupLeftAreaViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_OFFICE_RECESSED Office shifted 2.8m behind the side-wall line; facade at X=17.4m. Main wall and overlapping ground-floor cladding opened; level pocket floor added, doorway walkable.");
        }

        private static void AlignOfficeBehindWall(Transform office)
        {
            Vector3 position = office.localPosition;
            position.z = -OfficeRecessDepth;
            office.localPosition = position;
            var volume = office.parent.Find("Reserved_area_LEFT_01");
            if (volume != null)
            {
                volume.localPosition = new Vector3(0, 2.1f, -0.05f);
                volume.GetComponent<UEHReservedSpace>().purpose = "Office lùi sau tuyến tường sảnh; mặt tiền cùng tuyến vách, phòng ở phía trong, ghế chờ sát vách.";
            }
            // Main lobby floor ends at world X=18. The extra slab abuts it, avoiding coplanar overlaps.
            if (office.Find("Office_recess_floor") == null)
            {
                var floor = new Part("Office_recess_floor", office, "OfficeRecess");
                floor.Box("Floor", new Vector3(0, -0.14f, 0.975f), new Vector3(12, 0.28f, 2.45f), true);
                floor.Finish();
            }
        }

        private static void RebuildOfficeWallOpening(Transform lobbyRoot)
        {
            var shell = lobbyRoot.Find("01_Floor_and_walls");
            if (shell != null) UnityEngine.Object.DestroyImmediate(shell.gameObject);
            BuildExpandedShell(lobbyRoot);
        }

        private static List<KeyValuePair<Vector3, Vector3>> OfficeExteriorWallFragments(float w, float d, float h)
        {
            var fragments = new List<KeyValuePair<Vector3, Vector3>>();
            float start = Mathf.Min(d, -LeftOfficeZ(dimensions) - 5.3f), end = Mathf.Min(d, -LeftOfficeZ(dimensions) + 5.3f);
            if (start > 0) fragments.Add(new KeyValuePair<Vector3, Vector3>(new Vector3(w / 2, h - 1.9f, -start / 2), new Vector3(0.42f, 3.7f, start)));
            if (d > end) fragments.Add(new KeyValuePair<Vector3, Vector3>(new Vector3(w / 2, h - 1.9f, -(d + end) / 2), new Vector3(0.42f, 3.7f, d - end)));
            float header = h - 0.05f - 4.2f;
            if (end > start && header > 0)
                fragments.Add(new KeyValuePair<Vector3, Vector3>(new Vector3(w / 2, 4.2f + header / 2, -(start + end) / 2), new Vector3(0.42f, header, end - start)));
            return fragments;
        }

        private static void TrimOfficeExteriorGroundWall()
        {
            if (dimensions.compactLeftSequence) { TrimCompactLeftExteriorWall(); return; }
            string path = Root + "/Prefabs/UEH_Exterior.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var part = contents.transform.Find("02_Podium_structure/Ivory");
                if (part == null) throw new InvalidOperationException("Missing podium side cladding.");
                var filter = part.GetComponent<MeshFilter>();
                var source = filter.sharedMesh;
                var sourceVertices = source.vertices;
                int remove = -1;
                Vector3 target = new Vector3(dimensions.frontage / 2, dimensions.lobbyHeight - 1.9f, -dimensions.exteriorDepth / 2);
                for (int start = 0; start + 24 <= sourceVertices.Length; start += 24)
                {
                    Vector3 center = Vector3.zero;
                    for (int i = 0; i < 24; i++) center += sourceVertices[start + i] / 24;
                    if (Vector3.Distance(center, target) < 0.02f) { remove = start; break; }
                }
                if (remove < 0) return; // Already opened by this correction or by a full rebuild.
                var positions = new List<Vector3>(); var normals = new List<Vector3>();
                var uv = new List<Vector2>(); var indices = new List<int>();
                var sourceNormals = source.normals; var sourceUV = source.uv; var sourceIndices = source.triangles;
                for (int i = 0; i < sourceVertices.Length; i++)
                {
                    if (i >= remove && i < remove + 24) continue;
                    positions.Add(sourceVertices[i]); normals.Add(sourceNormals[i]); uv.Add(sourceUV[i]);
                }
                for (int i = 0; i < sourceIndices.Length; i += 3)
                {
                    if (sourceIndices[i] >= remove && sourceIndices[i] < remove + 24) continue;
                    for (int j = 0; j < 3; j++) indices.Add(sourceIndices[i + j] >= remove + 24 ? sourceIndices[i + j] - 24 : sourceIndices[i + j]);
                }
                var kept = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                kept.SetVertices(positions); kept.SetNormals(normals); kept.SetUVs(0, uv); kept.SetTriangles(indices, 0);
                var geometry = new Geometry();
                geometry.Append(kept, Matrix4x4.identity);
                foreach (var fragment in OfficeExteriorWallFragments(dimensions.frontage, dimensions.exteriorDepth, dimensions.lobbyHeight))
                    geometry.Box(fragment.Key, fragment.Value);
                var mesh = geometry.ToMesh("OfficeRecess_Exterior_podium_Ivory");
                UnityEngine.Object.DestroyImmediate(kept);
                string meshPath = Root + "/Meshes/OfficeRecess_Exterior_podium_Ivory.asset";
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
