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
        [Serializable] private sealed class GlyphFile { public SolidGlyph[] glyphs; }
        [Serializable] private sealed class SolidGlyph
        {
            public string letter;
            public float advance;
            public float[] vertices;
            public int[] triangles;
        }
        private static readonly Dictionary<char, Mesh> welcomeGlyphMeshes = new Dictionary<char, Mesh>();
        private static readonly Dictionary<char, float> welcomeGlyphAdvances = new Dictionary<char, float>();

        [MenuItem("UEH/Campus B/7. Refine WELCOME wall and reserve lift space")]
        public static void RefineReceptionWall()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before editing reception.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                materials[material.name] = material;
            }
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null) throw new FileNotFoundException("Missing estimated dimensions.");
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var zone = contents.transform.Find("05_WELCOME_RIGHT_from_gate");
                if (zone == null) throw new InvalidOperationException("Missing right welcome zone.");
                foreach (string name in new[] { "05_Reception_and_welcome_wall", "Welcome", "To", "UEH_reception", "Reception_label", "Reception_potted_plant" })
                {
                    var old = zone.Find(name);
                    if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                }
                RemoveReceptionColumn(contents.transform);
                var group = new GameObject("05_Reception_and_welcome_wall").transform;
                group.SetParent(zone, false);
                PopulateReception(group);
                BuildReservedFireWall(contents.transform);
                if (dimensions.facultyLiftEntrance) BuildFacultyLiftEntrance(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var views = GameObject.Find("Review viewpoints [select and align view]");
            if (views != null) SetupReceptionViewpoints(views.transform);
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_RECEPTION_UPDATED Removed the inner reception-side column; left greenery, solid brass WELCOME TO UEH, raised background alphabet and 3.5m clear space on the viewer's right.");
        }

        private static void SetupReceptionViewpoints(Transform root)
        {
            foreach (string name in new[] { "09_Welcome_front", "10_Welcome_raised_letters" })
            {
                var old = root.Find(name);
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            }
            Viewpoint(root, "09_Welcome_front", new Vector3(-7.2f, 1.8f, -8.3f), new Vector3(-17.4f, 2, -8.3f));
            Viewpoint(root, "10_Welcome_raised_letters", new Vector3(-14.4f, 2.9f, -7.7f), new Vector3(-17.4f, 2.65f, -9.3f));
        }

        private static void LoadWelcomeGlyphs()
        {
            welcomeGlyphMeshes.Clear(); welcomeGlyphAdvances.Clear();
            var file = JsonUtility.FromJson<GlyphFile>(File.ReadAllText(Root + "/Fonts/UppercaseSolidGlyphs.json"));
            if (file == null || file.glyphs == null || file.glyphs.Length != 26)
                throw new InvalidDataException("Expected 26 solid uppercase glyphs.");
            foreach (var glyph in file.glyphs)
            {
                char letter = glyph.letter[0];
                string path = Root + "/Meshes/Welcome_Glyph_" + letter + ".asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh == null)
                {
                    mesh = new Mesh { name = "Welcome_solid_glyph_" + letter, indexFormat = IndexFormat.UInt32 };
                    var vertices = new Vector3[glyph.vertices.Length / 3];
                    var uv = new Vector2[vertices.Length];
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(glyph.vertices[i * 3], glyph.vertices[i * 3 + 1], glyph.vertices[i * 3 + 2]);
                        uv[i] = new Vector2(vertices[i].x, vertices[i].y);
                    }
                    mesh.vertices = vertices; mesh.uv = uv; mesh.triangles = glyph.triangles;
                    mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
                    AssetDatabase.CreateAsset(mesh, path);
                }
                welcomeGlyphMeshes[letter] = mesh;
                welcomeGlyphAdvances[letter] = glyph.advance;
            }
        }

        private static void PopulateReception(Transform parent)
        {
            LoadWelcomeGlyphs();
            Mat("WelcomeBrass", "CCA352", 0.42f, 0.55f);
            Mat("WelcomeRelief", "E3DDCF", 0.18f);
            Mat("WelcomeDesk", "C7C7BE", 0.4f);
            Mat("WelcomeLeafDark", "234E2D", 0.1f);
            const float panelU = -1.8f;
            var panel = HeritageSection(parent, "02_WELCOME_letter_wall", panelU);
            var wall = new Part("Welcome_wall_and_timber_base", panel, "Welcome");
            wall.Box("White", new Vector3(0, 2.35f, -0.11f), new Vector3(6.8f, 2.9f, 0.22f));
            wall.Box("Wood", new Vector3(0, 0.45f, -0.11f), new Vector3(6.8f, 0.9f, 0.22f));
            foreach (int side in new[] { -1, 1 })
                wall.Box("Dark", new Vector3(side * 3.43f, 1.9f, -0.05f), new Vector3(0.07f, 3.8f, 0.2f));
            wall.Finish();

            var green = HeritageSection(parent, "01_Greenery_LEFT_of_WELCOME", -5.95f);
            var leaves = new Part("Welcome_green_wall", green, "Welcome");
            leaves.Box("WelcomeLeafDark", new Vector3(0, 1.9f, -0.08f), new Vector3(1.4f, 3.8f, 0.16f));
            for (int row = 0; row < 16; row++)
                for (int col = 0; col < 6; col++)
                {
                    int i = row * 6 + col;
                    float x = -0.55f + col * 0.22f + Mathf.Sin(i * 2.3f) * 0.02f;
                    float y = 0.18f + row * 0.23f;
                    leaves.Leaf(i % 5 == 0 ? "LeafLight" : i % 3 == 0 ? "WelcomeLeafDark" : "Leaf",
                        new Vector3(x, y, 0.015f + (i % 3) * 0.012f), new Vector2(0.3f, 0.38f), i * 137.5f);
                }
            leaves.Finish();

            SolidWelcomeWord(panel, "Welcome", "WELCOME", new Vector3(0, 3.1f, 0.006f), 3.5f, 0.47f, 0.06f);
            SolidWelcomeWord(panel, "To", "TO", new Vector3(0, 2.57f, 0.006f), 1.15f, 0.4f, 0.06f);
            SolidWelcomeWord(panel, "UEH_reception", "UEH", new Vector3(0, 2.04f, 0.006f), 1.75f, 0.46f, 0.06f);
            string[] rows = { "OAMNKPDHKTGCNCOSV", "EIDCLJPHLCXCQNSD", "OCFTDNCAVZPTJEOR", "AECPKAMSCZQEVJTI", "MDRMQHOLLXNZTMND", "NDTJSANMIPMHTTNV" };
            for (int row = 0; row < rows.Length; row++)
            {
                var letters = new Part("Alphabet_row_" + (row + 1).ToString("00"), panel, "Welcome");
                float y = 3.6f - row * 0.5f;
                for (int col = 0; col < rows[row].Length; col++)
                {
                    float u = -3.04f + col * 6.08f / (rows[row].Length - 1);
                    // Exclude any background cell whose height overlaps a brass word.
                    float exclusion = 0;
                    if (Mathf.Abs(y - 3.1f) < 0.47f / 2 + 0.20f) exclusion = 1.9f;
                    if (Mathf.Abs(y - 2.57f) < 0.4f / 2 + 0.20f) exclusion = Mathf.Max(exclusion, 0.75f);
                    if (Mathf.Abs(y - 2.04f) < 0.46f / 2 + 0.20f) exclusion = Mathf.Max(exclusion, 1.0f);
                    if (exclusion > 0 && Mathf.Abs(u) < exclusion) continue;
                    char c = rows[row][col];
                    var mesh = welcomeGlyphMeshes[c];
                    float width = mesh.bounds.size.x * 0.32f;
                    Vector3 position = new Vector3(-u + width / 2 + mesh.bounds.min.x * 0.32f, y - 0.16f, 0.002f);
                    letters.AppendMesh("WelcomeRelief", mesh, Matrix4x4.TRS(position, Quaternion.Euler(0, 180, 0), new Vector3(0.32f, 0.32f, 0.012f)));
                }
                letters.Finish();
            }

            var desk = HeritageSection(parent, "04_Reception_desk", panelU);
            var counter = new Part("Reception_counter", desk, "Welcome");
            counter.Box("WelcomeDesk", new Vector3(0, 0.52f, 2.5f), new Vector3(4.5f, 1.04f, 0.95f), true);
            counter.Box("Ivory", new Vector3(0, 1.07f, 2.5f), new Vector3(4.58f, 0.06f, 1.02f));
            counter.Box("White", new Vector3(0, 0.98f, 3.025f), new Vector3(1.8f, 0.85f, 0.12f));
            counter.Box("Dark", new Vector3(0, 1.14f, 2.3f), new Vector3(0.62f, 0.1f, 0.28f));
            counter.Finish();
            foreach (float u in new[] { -1.55f, 1.55f })
            {
                var mount = HeritageSection(desk, "Counter_plant_" + u, u);
                mount.localPosition += new Vector3(0, 1.1f, 2.4f);
                mount.localScale = Vector3.one * 0.38f;
                Plant(mount, u < 0 ? "Counter_plant_left" : "Counter_plant_right", Vector3.zero, "Welcome");
            }

            var track = new Part("Welcome_track_lighting", panel, "Welcome");
            track.Box("Dark", new Vector3(0, 4.05f, 0.5f), new Vector3(6.7f, 0.055f, 0.07f));
            for (int i = 0; i < 8; i++)
            {
                float x = -2.95f + i * 0.84f;
                track.Cylinder("Dark", new Vector3(x, 3.92f, 0.5f), new Vector3(0.11f, 0.11f, 0.11f));
                track.Cylinder("WarmLight", new Vector3(x, 3.805f, 0.5f), new Vector3(0.09f, 0.006f, 0.09f));
            }
            track.Finish();

            var lift = HeritageSection(parent, dimensions != null && dimensions.rightLiftsAndServices ? "03_Clear_entrance_pocket" : "03_Future_lift_space_RIGHT", 3.5f);
            lift.localPosition += new Vector3(0, 2.1f, 1.25f);
            var reserved = lift.gameObject.AddComponent<UEHReservedSpace>();
            if (dimensions != null && dimensions.rightLiftsAndServices)
                reserved.purpose = "Khoảng trống gần cửa vào được giữ thoáng; cụm thang máy và hành lang nằm ở hai khoang mở rộng phía trong.";
        }

        private static void SolidWelcomeWord(Transform parent, string name, string word, Vector3 center, float width, float capHeight, float depth)
        {
            const float tracking = 0.18f;
            float advance = 0;
            foreach (char c in word) advance += welcomeGlyphAdvances[c] + tracking;
            advance -= tracking;
            float scale = Mathf.Min(capHeight, width / advance);
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = center - Vector3.up * scale / 2;
            root.localRotation = Quaternion.Euler(0, 180, 0);
            float pen = -advance * scale / 2;
            foreach (char c in word)
            {
                var letter = new GameObject("Solid_" + c).transform;
                letter.SetParent(root, false);
                letter.localPosition = new Vector3(pen, 0, 0);
                letter.localScale = new Vector3(scale, scale, depth);
                letter.gameObject.AddComponent<MeshFilter>().sharedMesh = welcomeGlyphMeshes[c];
                var renderer = letter.gameObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = materials["WelcomeBrass"];
                renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
                GameObjectUtility.SetStaticEditorFlags(letter.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic);
                pen += (welcomeGlyphAdvances[c] + tracking) * scale;
            }
        }

        private static void RemoveReceptionColumn(Transform root)
        {
            var group = root.Find("02_Stone_columns");
            if (group == null) throw new InvalidOperationException("Missing stone columns.");
            Vector3 target = new Vector3(-dimensions.frontage * 0.375f, dimensions.lobbyHeight / 2, -dimensions.lobbyDepth / 2);
            foreach (var filter in group.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = filter.sharedMesh;
                var oldVertices = mesh.vertices;
                int remove = -1;
                for (int start = 0; start < oldVertices.Length; start += 24)
                {
                    Vector3 center = Vector3.zero;
                    for (int i = 0; i < 24; i++) center += oldVertices[start + i] / 24;
                    if (Mathf.Abs(center.x - target.x) < 0.02f && Mathf.Abs(center.z - target.z) < 0.02f)
                    {
                        if (remove >= 0) throw new InvalidOperationException("Ambiguous reception column module.");
                        remove = start;
                    }
                }
                if (remove < 0) continue; // Applying the command twice is safe.
                var vertices = new List<Vector3>(); var uv = new List<Vector2>();
                var normals = new List<Vector3>(); var tangents = new List<Vector4>(); var triangles = new List<int>();
                var oldUV = mesh.uv; var oldNormals = mesh.normals; var oldTangents = mesh.tangents;
                for (int i = 0; i < oldVertices.Length; i++)
                {
                    if (i >= remove && i < remove + 24) continue;
                    vertices.Add(oldVertices[i]); uv.Add(oldUV[i]); normals.Add(oldNormals[i]); tangents.Add(oldTangents[i]);
                }
                var oldTriangles = mesh.triangles;
                for (int i = 0; i < oldTriangles.Length; i += 3)
                {
                    if (oldTriangles[i] >= remove && oldTriangles[i] < remove + 24) continue;
                    for (int j = 0; j < 3; j++) triangles.Add(oldTriangles[i + j] >= remove + 24 ? oldTriangles[i + j] - 24 : oldTriangles[i + j]);
                }
                mesh.Clear(); mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetNormals(normals);
                mesh.SetTangents(tangents); mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds(); mesh.UploadMeshData(false);
                EditorUtility.SetDirty(mesh);
            }
            foreach (var collider in group.GetComponentsInChildren<BoxCollider>())
                if (Vector3.Distance(collider.transform.localPosition, target) < 0.02f)
                    UnityEngine.Object.DestroyImmediate(collider.gameObject);
        }
    }
}
#endif
