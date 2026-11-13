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
        public static float RearColumnZ(UEHDimensions s) => -s.lobbyDepth + 6;

        [MenuItem("UEH/Campus B/23. Add rear columns souvenir cabinet and three trees")]
        public static void AddRearColumnsAndSouvenir()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.refinedLobbyLayout) throw new InvalidOperationException("Build the current lobby first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[mat.name] = mat; }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.rearColumnsAndSouvenir = true;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var old = contents.transform.Find("02_Stone_columns");
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                BuildExpandedColumns(contents.transform);
                BuildRearSouvenirAndPlants(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupSouvenirViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_SOUVENIR_UPDATED Added two rear 2m stone columns at X=+/-4.5, Z=" + RearColumnZ(dimensions)
                + "; glazed UEH SOUVENIR cabinet on inner face of rear right column facing the central aisle; exactly three new white-pot trees. Estimated dimensions from supplied photos.");
        }

        private static void BuildRearSouvenirAndPlants(Transform lobby)
        {
            if (!dimensions.rearColumnsAndSouvenir) return;
            var old = lobby.Find("14_Rear_souvenir_and_three_trees");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            Mat("SouvenirFrame", "E0E4E2", 0.65f, 0.55f);
            Mat("SouvenirTeal", "006E79", 0.45f, 0.15f);
            Mat("SouvenirRed", "B32737", 0.24f);
            Mat("SouvenirLeafDeep", "294D30", 0.12f);
            Mat("SouvenirLeafMid", "3D6339", 0.12f);
            Mat("SouvenirLeafLight", "587745", 0.12f);
            Mat("SouvenirSoil", "332D23", 0.05f);
            var group = RightGroup(lobby, "14_Rear_souvenir_and_three_trees", Vector3.zero);
            float x = dimensions.frontage * 0.125f, rear = RearColumnZ(dimensions);
            RightGroup(group, "Rear_column_LEFT_center", new Vector3(x, 0, rear));
            RightGroup(group, "Rear_column_RIGHT_center", new Vector3(-x, 0, rear));
            // Cabinet back touches the inner face of the right column; +local Z points at aisle.
            var cabinet = RightGroup(group, "UEH_SOUVENIR_display_cabinet",
                new Vector3(-x + dimensions.lobbyColumnWidth / 2 + 0.29f, 0, rear));
            cabinet.localRotation = Quaternion.Euler(0, 90, 0);
            BuildSouvenirCabinet(cabinet);
            BuildReferencePottedTree(group, "Tree_01_MAIN_LEFT", new Vector3(x, 0, ExpandedColumnZ(dimensions) + dimensions.lobbyColumnWidth / 2 + 0.8f), 0);
            BuildReferencePottedTree(group, "Tree_02_REAR_LEFT", new Vector3(x, 0, rear + dimensions.lobbyColumnWidth / 2 + 0.8f), 1);
            BuildReferencePottedTree(group, "Tree_03_REAR_RIGHT", new Vector3(-x, 0, rear + dimensions.lobbyColumnWidth / 2 + 0.8f), 2);
        }

        private static void BuildSouvenirCabinet(Transform root)
        {
            const float width = 1.96f, height = 4.1f, depth = 0.58f;
            var frame = new Part("Cabinet_frame_base_and_shelves", root, "Souvenir");
            frame.Box("Dark", new Vector3(0, 0.12f, 0), new Vector3(width + 0.06f, 0.24f, depth + 0.04f), true);
            foreach (float x in new[] { -width / 2, width / 2 })
                foreach (float z in new[] { -depth / 2, depth / 2 })
                    frame.Box("SouvenirFrame", new Vector3(x, (height + 0.24f) / 2, z), new Vector3(0.045f, height - 0.24f, 0.045f));
            foreach (float y in new[] { 0.27f, 2.46f, height })
            {
                frame.Box("SouvenirFrame", new Vector3(0, y, depth / 2), new Vector3(width, 0.045f, 0.045f));
                frame.Box("SouvenirFrame", new Vector3(0, y, -depth / 2), new Vector3(width, 0.045f, 0.045f));
                foreach (float x in new[] { -width / 2, width / 2 })
                    frame.Box("SouvenirFrame", new Vector3(x, y, 0), new Vector3(0.045f, 0.045f, depth));
            }
            frame.Box("SouvenirFrame", new Vector3(0, 2.18f, depth / 2), new Vector3(0.026f, 3.81f, 0.035f));
            frame.Box("SouvenirRed", new Vector3(0, 1.37f, -0.277f), new Vector3(1.86f, 1.58f, 0.018f));
            foreach (float y in new[] { 0.34f, 0.98f, 1.62f, 2.26f, 2.90f, 3.54f })
            {
                frame.Box("GlassOfficeGlass", new Vector3(0, y, 0), new Vector3(1.9f, 0.015f, 0.53f));
                foreach (float x in new[] { -0.91f, 0.91f })
                    frame.Box("Silver", new Vector3(x, y - 0.025f, -0.08f), new Vector3(0.04f, 0.035f, 0.38f));
            }
            frame.Finish();
            foreach (int side in new[] { -1, 1 })
            {
                var glass = new Part("Cabinet_side_glass_" + side, root, "Souvenir");
                glass.Box("GlassOfficeGlass", new Vector3(side * 0.98f, 2.18f, 0), new Vector3(0.012f, 3.8f, 0.54f), true);
                glass.Finish();
                foreach (int row in new[] { 0, 1 })
                {
                    var door = new Part("Front_glass_door_" + side + "_" + row, root, "Souvenir");
                    float low = row == 0 ? 0.29f : 2.49f, high = row == 0 ? 2.43f : 4.07f;
                    door.Box("GlassOfficeGlass", new Vector3(side * 0.49f, (low + high) / 2, 0.294f), new Vector3(0.94f, high - low, 0.012f), true);
                    door.Box("Silver", new Vector3(side * 0.065f, low + (high - low) * 0.55f, 0.324f), new Vector3(0.018f, 0.19f, 0.025f));
                    foreach (float y in new[] { low + 0.18f, high - 0.18f })
                        door.Box("Silver", new Vector3(side * 0.93f, y, 0.308f), new Vector3(0.028f, 0.09f, 0.024f));
                    door.Finish();
                }
            }
            Text(root, "UEH_SOUVENIR_base_sign", "UEH SOUVENIR", new Vector3(0, 0.125f, 0.318f), 1.88f, 0.17f, "White");
            var goods = new Part("Souvenir_caps_bottles_mugs_and_gifts", root, "Souvenir");
            foreach (float shelf in new[] { 2.90f, 3.54f })
                foreach (float x in new[] { -0.57f, 0, 0.57f })
                {
                    // Teal caps and projecting brims, as in the upper part of the reference.
                    goods.Sphere("SouvenirTeal", new Vector3(x, shelf + 0.10f, -0.015f), new Vector3(0.32f, 0.19f, 0.28f));
                    goods.Box("SouvenirTeal", new Vector3(x, shelf + 0.025f, 0.15f), new Vector3(0.27f, 0.025f, 0.16f));
                    Text(root, "Cap_UEH_" + shelf + "_" + x, "UEH", new Vector3(x, shelf + 0.10f, 0.129f), 0.11f, 0.042f, "White");
                }
            foreach (float shelf in new[] { 1.62f, 2.26f })
                for (int i = 0; i < 7; i++)
                {
                    float x = -0.78f + i * 0.26f;
                    string mat = i % 3 == 0 ? "Dark" : "SouvenirTeal";
                    goods.Cylinder(mat, new Vector3(x, shelf + 0.20f, -0.025f), new Vector3(0.105f, 0.19f, 0.105f));
                    goods.Cylinder("Silver", new Vector3(x, shelf + 0.396f, -0.025f), new Vector3(0.075f, 0.016f, 0.075f));
                    if (i == 1 || i == 5)
                        Text(root, "Bottle_UEH_" + shelf + "_" + i, "UEH", new Vector3(x, shelf + 0.21f, 0.031f), 0.073f, 0.035f, "White");
                }
            for (int i = 0; i < 5; i++)
            {
                float x = -0.76f + i * 0.38f;
                goods.Box(i % 2 == 0 ? "SouvenirRed" : "SouvenirTeal", new Vector3(x, 0.48f, 0.015f), new Vector3(0.25f, 0.26f, 0.2f));
                goods.Box("White", new Vector3(x, 0.48f, 0.121f), new Vector3(0.018f, 0.25f, 0.006f));
                goods.Box("White", new Vector3(x, 0.51f, 0.121f), new Vector3(0.25f, 0.018f, 0.006f));
                goods.Cylinder(i % 2 == 0 ? "White" : "Dark", new Vector3(x, 1.09f, 0.025f), new Vector3(0.16f, 0.1f, 0.16f));
                // Small open rectangular mug handles.
                goods.Box("Silver", new Vector3(x + 0.112f, 1.09f, 0.025f), new Vector3(0.02f, 0.12f, 0.025f));
                foreach (float y in new[] { 1.03f, 1.15f }) goods.Box("Silver", new Vector3(x + 0.085f, y, 0.025f), new Vector3(0.06f, 0.02f, 0.025f));
            }
            goods.Box("Gold", new Vector3(0, 1.26f, -0.17f), new Vector3(0.08f, 0.18f, 0.035f));
            goods.Sphere("Gold", new Vector3(0, 1.41f, -0.17f), new Vector3(0.22f, 0.22f, 0.035f));
            goods.Finish();
        }

        private static void BuildReferencePottedTree(Transform parent, string name, Vector3 position, int seed)
        {
            var root = RightGroup(parent, name, position);
            var p = new Part("White_planter_and_branching_tree", root, "Souvenir_" + name);
            var pot = TaperedWhitePotMesh();
            p.AppendMesh("White", pot, Matrix4x4.identity); UnityEngine.Object.DestroyImmediate(pot);
            p.Cylinder("White", new Vector3(0, 0.025f, 0), new Vector3(0.66f, 0.025f, 0.66f));
            p.Cylinder("SouvenirSoil", new Vector3(0, 0.756f, 0), new Vector3(0.59f, 0.015f, 0.59f));
            var collider = root.gameObject.AddComponent<BoxCollider>(); collider.center = new Vector3(0, 0.4f, 0); collider.size = new Vector3(0.65f, 0.8f, 0.65f);
            p.Cylinder("Wood", new Vector3(0, 1.19f, 0), new Vector3(0.06f, 0.43f, 0.06f));
            for (int i = 0; i < 9; i++)
            {
                float a = i * 2.39996f + seed * 0.7f;
                var end = new Vector3(Mathf.Cos(a) * 0.49f, 1.80f + (i % 3) * 0.14f, Mathf.Sin(a) * 0.49f);
                AppendTreeBranch(p, new Vector3(0, 1.17f + (i % 3) * 0.09f, 0), end, 0.026f);
            }
            for (int i = 0; i < 48; i++)
            {
                float a = i * 2.39996f + seed * 0.7f;
                float r = 0.23f + (i % 5) * 0.105f;
                float y = 1.72f + (i % 4) * 0.16f - r * 0.07f;
                string mat = i % 7 == 0 ? "SouvenirLeafLight" : i % 3 == 0 ? "SouvenirLeafDeep" : "SouvenirLeafMid";
                p.Sphere(mat, new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r), new Vector3(0.43f, 0.35f, 0.4f));
            }
            p.Finish();
        }

        private static void AppendTreeBranch(Part part, Vector3 start, Vector3 end, float diameter)
        {
            if (!primitives.TryGetValue(PrimitiveType.Cylinder, out var mesh))
            {
                var temporary = GameObject.CreatePrimitive(PrimitiveType.Cylinder); mesh = temporary.GetComponent<MeshFilter>().sharedMesh;
                primitives[PrimitiveType.Cylinder] = mesh; UnityEngine.Object.DestroyImmediate(temporary);
            }
            var delta = end - start;
            part.AppendMesh("Wood", mesh, Matrix4x4.TRS((start + end) / 2, Quaternion.FromToRotation(Vector3.up, delta.normalized), new Vector3(diameter, delta.magnitude / 2, diameter)));
        }

        private static Mesh TaperedWhitePotMesh()
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            Action<Vector3, Vector3, Vector3, Vector3> quad = (a, b, c, d) =>
            {
                int n = vertices.Count; vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
                triangles.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3 });
            };
            const int segments = 40;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments, b = (i + 1) * Mathf.PI * 2 / segments;
                var la = new Vector3(Mathf.Cos(a) * 0.235f, 0.05f, Mathf.Sin(a) * 0.235f);
                var lb = new Vector3(Mathf.Cos(b) * 0.235f, 0.05f, Mathf.Sin(b) * 0.235f);
                var ha = new Vector3(Mathf.Cos(a) * 0.33f, 0.8f, Mathf.Sin(a) * 0.33f);
                var hb = new Vector3(Mathf.Cos(b) * 0.33f, 0.8f, Mathf.Sin(b) * 0.33f);
                var ia = new Vector3(Mathf.Cos(a) * 0.297f, 0.8f, Mathf.Sin(a) * 0.297f);
                var ib = new Vector3(Mathf.Cos(b) * 0.297f, 0.8f, Mathf.Sin(b) * 0.297f);
                quad(la, ha, hb, lb); quad(ha, ia, ib, hb);
                quad(ia, ia - Vector3.up * 0.07f, ib - Vector3.up * 0.07f, ib);
            }
            var mesh = new Mesh { name = "Tapered_white_planter" }; mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
            mesh.uv = new Vector2[vertices.Count]; mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        private static void SetupSouvenirViewpoints()
        {
            var views = GameObject.Find("Review viewpoints [select and align view]"); if (views == null) return;
            foreach (string name in new[] { "63_Rear_pair_souvenir_and_trees", "64_Souvenir_cabinet_at_column", "65_UEH_souvenir_display_front", "66_Three_new_white_pot_trees" })
            { var old = views.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            float z = RearColumnZ(dimensions);
            Viewpoint(views.transform, "63_Rear_pair_souvenir_and_trees", new Vector3(0, 1.75f, z + 13), new Vector3(0, 2, z - 1));
            Viewpoint(views.transform, "64_Souvenir_cabinet_at_column", new Vector3(0.4f, 1.8f, z + 2.5f), new Vector3(-3.2f, 2, z));
            Viewpoint(views.transform, "65_UEH_souvenir_display_front", new Vector3(0.8f, 2.05f, z), new Vector3(-3.2f, 2.05f, z));
            Viewpoint(views.transform, "66_Three_new_white_pot_trees", new Vector3(0, 1.85f, -15), new Vector3(0, 1.9f, z));
        }

        public static void AddSouvenirAndCapture()
        {
            AddRearColumnsAndSouvenir(); CaptureReflections();
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            foreach (Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                if (!point.name.StartsWith("63_") && !point.name.StartsWith("64_") && !point.name.StartsWith("65_") && !point.name.StartsWith("66_")) continue;
                var cameraGO = new GameObject("Temporary souvenir review camera"); var camera = cameraGO.AddComponent<Camera>(); camera.enabled = false;
                camera.transform.SetPositionAndRotation(point.position, point.rotation);
                camera.fieldOfView = point.name.StartsWith("65_") ? 60 : point.name.StartsWith("64_") ? 62 : 82;
                camera.nearClipPlane = 0.08f; camera.farClipPlane = 240;
                var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                var prior = RenderTexture.active; camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                var texture = new Texture2D(1600, 1000, TextureFormat.RGB24, false); texture.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = prior; rt.Release();
                UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(cameraGO);
            }
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/souvenir-layout.json"), JsonUtility.ToJson(new SouvenirLayout
            {
                rearLeftColumn = new Vector3(4.5f, 0, RearColumnZ(dimensions)), rearRightColumn = new Vector3(-4.5f, 0, RearColumnZ(dimensions)),
                cabinetCenter = new Vector3(-3.21f, 0, RearColumnZ(dimensions)), cabinetFacing = Vector3.right,
                newTreeCenters = new[] { new Vector3(4.5f, 0, -22.2f), new Vector3(4.5f, 0, RearColumnZ(dimensions) + 1.8f), new Vector3(-4.5f, 0, RearColumnZ(dimensions) + 1.8f) }
            }, true));
            Debug.Log("UEH_SOUVENIR_ASSETS_AND_PREVIEWS_SAVED");
        }

        [Serializable] private sealed class SouvenirLayout
        {
            public string status = "Native Unity generation and rendered previews; no Play Mode tests for this revision. Estimated dimensions from photos.";
            public int totalColumns = 8, addedColumns = 2, addedTrees = 3;
            public Vector3 rearLeftColumn, rearRightColumn, cabinetCenter, cabinetFacing;
            public Vector3[] newTreeCenters;
        }
    }
}
#endif
