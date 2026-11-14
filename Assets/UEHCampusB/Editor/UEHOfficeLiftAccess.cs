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
        public const float GlassOfficeDoubleDoorWidth = 1.8f;
        private const string MainLiftAccessName = "18_Main_lift_access_partition_and_readers";

        // Office-local +X is viewer-left; +Z faces the public hall.
        // Keep all opaque office construction inside the room's X bounds.
        private static void RefineOfficeFrontAndBoundary(Transform room)
        {
            foreach (string name in new[] { "01_FRONT_glass_wall", "03_Open_entrance_1_25m", "03_Double_entrance_LEFT", "Building_context_beyond_right_glass" })
            {
                var old = room.Find(name);
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            }
            float half = GlassOfficeWidth / 2, doorHalf = GlassOfficeDoubleDoorWidth / 2;
            float start = GlassOfficeDoorX - doorHalf, end = GlassOfficeDoorX + doorHalf;
            var front = RightGroup(room, "01_FRONT_glass_wall", Vector3.zero);
            var posts = new SortedSet<float>();
            BuildOfficeFixedRange(front, "Main", -half, start, posts);
            BuildOfficeFixedRange(front, "Left_return", end, half, posts);
            var frame = new Part("Front_frame_double_door_LEFT", front, "OfficeLiftRefine");
            foreach (float x in posts)
                frame.Box("Metal", new Vector3(x, 1.95f, 0), new Vector3(.045f, 3.9f, .09f));
            foreach (float y in new[] { .06f, 3.87f })
                frame.Box("Metal", new Vector3(0, y, 0), new Vector3(GlassOfficeWidth + .05f, .07f, .09f));
            frame.Box("Metal", new Vector3(GlassOfficeDoorX, 2.68f, 0), new Vector3(GlassOfficeDoubleDoorWidth, .055f, .09f));
            frame.Box("GlassOfficeGlass", new Vector3(GlassOfficeDoorX, 3.275f, 0), new Vector3(GlassOfficeDoubleDoorWidth - .05f, 1.12f, .025f), true);
            frame.Box("Teal", new Vector3(.8f, 3.43f, .05f), new Vector3(1.9f, .36f, .04f));
            frame.Finish();
            Text(front, "Glass_office_sign", "OFFICE", new Vector3(.8f, 3.43f, .08f), 1.45f, .24f, "White");

            var entrance = RightGroup(room, "03_Double_entrance_LEFT", new Vector3(GlassOfficeDoorX, 0, 0));
            for (int side = -1; side <= 1; side += 2)
            {
                string id = side > 0 ? "Left" : "Right";
                var hinge = RightGroup(entrance, id + "_leaf_open_INWARD", new Vector3(side * doorHalf, 0, -.065f));
                hinge.localRotation = Quaternion.Euler(0, -side * 75, 0);
                float mid = -side * doorHalf / 2;
                var leaf = new Part(id + "_double_door_leaf", hinge, "OfficeLiftRefine");
                leaf.Box("GlassOfficeGlass", new Vector3(mid, 1.32f, 0), new Vector3(doorHalf - .065f, 2.57f, .025f), true);
                foreach (float x in new[] { -side * .0225f, -side * (doorHalf - .0225f) })
                    leaf.Box("Metal", new Vector3(x, 1.32f, 0), new Vector3(.04f, 2.64f, .065f));
                foreach (float y in new[] { .03f, 2.63f })
                    leaf.Box("Metal", new Vector3(mid, y, 0), new Vector3(doorHalf, .04f, .065f));
                foreach (float z in new[] { -.055f, .055f })
                    leaf.Box("Silver", new Vector3(-side * (doorHalf - .13f), 1.25f, z), new Vector3(.03f, .4f, .035f));
                leaf.Box("White", new Vector3(mid, 1.12f, .016f), new Vector3(doorHalf - .1f, .045f, .003f));
                leaf.Finish();
            }
            Text(entrance, "Entrance_label", "LỐI VÀO", new Vector3(0, 2.95f, .04f), 1.1f, .15f, "Teal");
            var planter = room.Find("Indoor_planter");
            if (planter != null) planter.localPosition = new Vector3(-half + .85f, 0, -.85f);
        }

        private static void BuildOfficeFixedRange(Transform front, string name, float start, float end, SortedSet<float> posts)
        {
            int count = Mathf.Max(1, Mathf.CeilToInt((end - start) / 2.6f));
            float step = (end - start) / count;
            for (int i = 0; i <= count; i++) posts.Add(start + i * step);
            for (int i = 0; i < count; i++)
            {
                var pane = new Part(name + "_fixed_pane_" + (i + 1), front, "OfficeLiftRefine");
                float x = start + (i + .5f) * step;
                pane.Box("GlassOfficeGlass", new Vector3(x, 1.95f, 0), new Vector3(step - .045f, 3.78f, .025f), true);
                pane.Box("White", new Vector3(x, 1.12f, .016f), new Vector3(step - .1f, .045f, .003f));
                pane.Finish();
            }
        }

        private static Vector3 MainLiftPartitionWall => new Vector3(RightFacadeX(dimensions) + .08f, 0, LiftBankZ - 5.75f);
        private static Vector3 MainLiftPartitionEnd => new Vector3(-4.5f, 0, LiftBankZ - 5.75f);

        private static void BuildMainLiftAccess(Transform lobby)
        {
            var old = lobby.Find(MainLiftAccessName);
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var root = RightGroup(lobby, MainLiftAccessName, Vector3.zero);
            Vector3 wall = MainLiftPartitionWall, end = MainLiftPartitionEnd;
            float length = end.x - wall.x;
            int count = Mathf.CeilToInt(length / 1.25f);
            float step = length / count;
            var rail = new Part("Office_to_lift_glass_partition", root, "OfficeLiftRefine");
            for (int i = 0; i <= count; i++)
            {
                float x = wall.x + step * i;
                rail.Box("Silver", new Vector3(x, .57f, wall.z), new Vector3(.045f, 1.1f, .045f));
                rail.Box("Metal", new Vector3(x, .045f, wall.z), new Vector3(.095f, .035f, .095f));
            }
            for (int i = 0; i < count; i++)
                rail.Box("RailingGlass", new Vector3(wall.x + step * (i + .5f), .6f, wall.z), new Vector3(step - .04f, .94f, .018f), true);
            rail.Box("Silver", new Vector3((wall.x + end.x) / 2, 1.12f, wall.z), new Vector3(length + .04f, .04f, .055f));
            rail.Finish();
            RightGroup(root, "Connection_at_office_lift_stone_pier", wall + Vector3.up * .6f);

            // Same size, stainless finish, top reader and front indicator as faculty lift access.
            var readers = RightGroup(root, "Three_access_control_pedestals", new Vector3(end.x, 0, LiftBankZ - 2.85f));
            readers.localRotation = Quaternion.Euler(0, 90, 0);
            for (int i = 0; i < 3; i++)
            {
                var pedestal = RightGroup(readers, "Access_pedestal_" + (i + 1), new Vector3(-1.6f + i * 1.6f, 0, 0));
                var g = new Part("Main_lift_access_" + (i + 1), pedestal, "OfficeLiftRefine");
                g.Box("LiftStainless", new Vector3(0, .5f, 0), new Vector3(.28f, 1, .72f), true);
                g.Box("Dark", new Vector3(0, 1.015f, .12f), new Vector3(.2f, .035f, .24f));
                g.Box("Teal", new Vector3(0, 1.037f, .12f), new Vector3(.09f, .008f, .14f));
                g.Box("CoolLight", new Vector3(0, .7f, .367f), new Vector3(.08f, .08f, .01f));
                g.Finish();
            }
        }

        [MenuItem("UEH/Campus B/41. Refine office double door and main lift access")]
        public static void RefineOfficeLiftAccessAndCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[m.name] = m; }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var room = contents.transform.Find("09_Four_expansion_areas/RIGHT_02 [12m x 6m]/Glass_office_recessed");
                if (room == null) throw new InvalidOperationException("Build the recessed office first.");
                RefineOfficeFrontAndBoundary(room);
                BuildMainLiftAccess(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupOfficeLiftAccessViewpoints();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            CaptureOfficeLiftAccessPreviews();
            Directory.CreateDirectory(Path.GetFullPath("Docs/UEH-CampusB"));
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/office-lift-access-layout.json"), JsonUtility.ToJson(new OfficeLiftAccessLayout(), true));
            Debug.Log("UEH_OFFICE_LIFT_ACCESS_COMPLETE Viewer-left 1.8m double office door; overlapping context wall removed; low glass partition from office/lift pier to hall; three main lift access pedestals.");
        }

        [Serializable] private sealed class OfficeLiftAccessLayout
        {
            public string status = "Reference-based static geometry; dimensions estimated from supplied screenshots.";
            public string doorSide = "Viewer-left when facing office from lobby; two inward-open leaves";
            public float doubleDoorWidth = GlassOfficeDoubleDoorWidth, doorCenterLocalX = GlassOfficeDoorX;
            public float openLeafDegrees = 75, clearPassageBetweenOpenLeafTips = GlassOfficeDoubleDoorWidth * (1 - Mathf.Cos(75 * Mathf.Deg2Rad)) - .07f;
            public bool removedContextWallBeyondOffice = true;
            public Vector3 partitionWall = MainLiftPartitionWall, partitionHallEnd = MainLiftPartitionEnd;
            public float partitionHeight = 1.14f, pedestalPitch = 1.6f;
            public int accessPedestals = 3;
        }

        private static void SetupOfficeLiftAccessViewpoints()
        {
            var views = GameObject.Find("Review viewpoints [select and align view]");
            if (views == null) return;
            foreach (Transform child in new List<Transform>(GetDirectChildren(views.transform)))
            { int n; if (child.name.Length > 3 && int.TryParse(child.name.Substring(0, 3), out n) && n >= 138 && n <= 142) UnityEngine.Object.DestroyImmediate(child.gameObject); }
            Viewpoint(views.transform, "138_Office_double_door_on_LEFT", new Vector3(-4, 2, GlassOfficeZ), new Vector3(RightFacadeX(dimensions), 2, GlassOfficeZ));
            Viewpoint(views.transform, "139_Office_left_double_door_detail", new Vector3(-9, 1.8f, GlassOfficeZ - GlassOfficeDoorX), new Vector3(-14.5f, 1.7f, GlassOfficeZ - GlassOfficeDoorX));
            Viewpoint(views.transform, "140_Office_lift_boundary_and_partition", new Vector3(-2.8f, 2.5f, -35), new Vector3(-12.8f, 1.8f, -31));
            Viewpoint(views.transform, "141_Main_lift_glass_partition_and_readers", new Vector3(-.5f, 2.4f, -29.5f), new Vector3(-10, 1.4f, -26));
            Viewpoint(views.transform, "142_Main_lift_access_reader_detail", new Vector3(-2, 1.65f, -27.85f), new Vector3(-5, 1, -27.85f));
        }

        private static void CaptureOfficeLiftAccessPreviews()
        {
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            foreach (Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                int n; if (point.name.Length < 4 || !int.TryParse(point.name.Substring(0, 3), out n) || n < 138 || n > 142) continue;
                var go = new GameObject("Temporary office and lift review camera"); var camera = go.AddComponent<Camera>(); camera.enabled = false;
                camera.transform.SetPositionAndRotation(point.position, point.rotation); camera.fieldOfView = n == 138 ? 65 : 72; camera.nearClipPlane = .05f; camera.farClipPlane = 220;
                var target = new RenderTexture(1800, 1100, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 }; var prior = RenderTexture.active;
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                var texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = prior; target.Release();
                UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
#endif
