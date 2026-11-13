#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UEH.CampusB.Editor
{
    public static partial class UEHCampusBuilder
    {
        [MenuItem("UEH/Campus B/26. Open rear lobby and add left wall fire extinguisher")]
        public static void OpenRearAndAddLeftExtinguisher()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.extendedGlassOfficeAndClosedRear || !dimensions.leftOfficeAndLounge)
                throw new InvalidOperationException("Build the extended office and left office/lounge first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[mat.name] = mat; }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.openRearAndLeftExtinguisher = true;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                // Removes the wall and its collider, without restoring the deleted courtyard.
                BuildRearLobbyClosure(contents.transform, dimensions.lobbyDepth, dimensions.lobbyHeight);
                BuildLeftWallExtinguisher(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupOpenRearFireViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_REAR_OPEN_AND_EXTINGUISHER_ADDED Rear closure and collider removed. One hanging ABC extinguisher centered on the white wall between the left office and green/timber lounge.");
        }

        private static float LeftExtinguisherZ(Transform root)
        {
            var office = root.Find("09_Four_expansion_areas/LEFT_01 [12m x 6m]/Office_room/Office_blue_perimeter");
            var green = root.Find("09_Four_expansion_areas/LEFT_02 [12m x 6m]/Green_timber_lounge/01_Greenery_LEFT");
            if (office == null || green == null) throw new InvalidOperationException("Missing left office or greenery facade.");
            float worldZ = (LeftGapRenderBounds(office).min.z + LeftGapRenderBounds(green).max.z) / 2;
            return root.InverseTransformPoint(new Vector3(0, 0, worldZ)).z;
        }

        private static void BuildLeftWallExtinguisher(Transform root)
        {
            var old = root.Find("16_Left_wall_fire_extinguisher");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            Mat("FireEquipmentRed", "BA2028", 0.4f, 0.12f);
            var fixture = RightGroup(root, "16_Left_wall_fire_extinguisher", new Vector3(LeftFacadeX(dimensions) - 0.025f, 0, LeftExtinguisherZ(root)));
            fixture.localRotation = Quaternion.Euler(0, -90, 0);
            var body = new Part("Wall_bracket_red_tank_valve_hose_and_label", fixture, "FireEquipment");
            // Local +Z faces the middle aisle. The cylinder hangs clear of the floor.
            body.Box("Silver", new Vector3(0, 1.19f, 0.035f), new Vector3(0.13f, 0.58f, 0.035f));
            foreach (float y in new[] { 0.97f, 1.38f })
                body.Box("OfficeCharcoal", new Vector3(0, y, 0.085f), new Vector3(0.25f, 0.035f, 0.13f));
            body.Cylinder("FireEquipmentRed", new Vector3(0, 1.18f, 0.20f), new Vector3(0.24f, 0.245f, 0.24f), true);
            body.Sphere("FireEquipmentRed", new Vector3(0, 0.945f, 0.20f), new Vector3(0.24f, 0.12f, 0.24f));
            body.Sphere("FireEquipmentRed", new Vector3(0, 1.425f, 0.20f), new Vector3(0.24f, 0.12f, 0.24f));
            body.Cylinder("Silver", new Vector3(0, 1.51f, 0.20f), new Vector3(0.075f, 0.025f, 0.075f));
            body.Box("Silver", new Vector3(0, 1.55f, 0.20f), new Vector3(0.13f, 0.035f, 0.09f));
            body.Box("OfficeCharcoal", new Vector3(-0.025f, 1.585f, 0.20f), new Vector3(0.17f, 0.025f, 0.06f));
            body.Box("FireEquipmentRed", new Vector3(-0.025f, 1.625f, 0.20f), new Vector3(0.17f, 0.025f, 0.06f));
            body.Box("Silver", new Vector3(0.055f, 1.60f, 0.20f), new Vector3(0.018f, 0.06f, 0.025f));
            body.Sphere("Silver", new Vector3(0.04f, 1.56f, 0.265f), new Vector3(0.075f, 0.075f, 0.025f));
            body.Sphere("White", new Vector3(0.04f, 1.56f, 0.281f), new Vector3(0.059f, 0.059f, 0.007f));
            body.Box("Teal", new Vector3(0.045f, 1.569f, 0.286f), new Vector3(0.004f, 0.025f, 0.003f));
            // Flexible black discharge hose, with a short nozzle and rounded upper bend.
            body.Sphere("OfficeCharcoal", new Vector3(0.13f, 1.54f, 0.18f), new Vector3(0.13f, 0.04f, 0.04f));
            body.Sphere("OfficeCharcoal", new Vector3(0.19f, 1.51f, 0.18f), new Vector3(0.04f, 0.08f, 0.04f));
            body.Cylinder("OfficeCharcoal", new Vector3(0.195f, 1.30f, 0.18f), new Vector3(0.025f, 0.19f, 0.025f));
            body.Cylinder("OfficeCharcoal", new Vector3(0.195f, 1.08f, 0.18f), new Vector3(0.045f, 0.055f, 0.045f));
            body.Box("White", new Vector3(0, 1.20f, 0.321f), new Vector3(0.15f, 0.23f, 0.008f));
            body.Box("FireEquipmentRed", new Vector3(0, 1.90f, 0.025f), new Vector3(0.55f, 0.30f, 0.025f));
            body.Finish();
            Text(fixture, "Tank_ABC_label", "ABC", new Vector3(0, 1.24f, 0.329f), 0.12f, 0.045f, "FireEquipmentRed");
            Text(fixture, "Wall_identification_sign", "BÌNH\nCHỮA CHÁY", new Vector3(0, 1.90f, 0.042f), 0.48f, 0.075f, "White");
        }

        private static void SetupOpenRearFireViewpoints()
        {
            var views = GameObject.Find("Review viewpoints [select and align view]"); if (views == null) return;
            foreach (string name in new[] { "75_Rear_lobby_wall_removed", "76_Extinguisher_between_office_and_greenery", "77_Wall_mounted_extinguisher_detail" })
            { var old = views.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Viewpoint(views.transform, "75_Rear_lobby_wall_removed", new Vector3(0, 2, -36), new Vector3(0, 2, -48));
            Viewpoint(views.transform, "76_Extinguisher_between_office_and_greenery", new Vector3(6.5f, 1.8f, -28.1f), new Vector3(LeftFacadeX(dimensions), 1.9f, -28.1f));
            Viewpoint(views.transform, "77_Wall_mounted_extinguisher_detail", new Vector3(LeftFacadeX(dimensions) - 1.7f, 1.50f, -28.1f), new Vector3(LeftFacadeX(dimensions), 1.47f, -28.1f));
        }

        public static void OpenRearFireAndCapture()
        {
            OpenRearAndAddLeftExtinguisher(); CaptureReflections();
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            foreach (Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                if (!point.name.StartsWith("75_") && !point.name.StartsWith("76_") && !point.name.StartsWith("77_")) continue;
                var cameraGO = new GameObject("Temporary open-rear fire-equipment review camera"); var camera = cameraGO.AddComponent<Camera>(); camera.enabled = false;
                camera.transform.SetPositionAndRotation(point.position, point.rotation);
                camera.fieldOfView = point.name.StartsWith("77_") ? 45 : 80;
                camera.nearClipPlane = 0.08f; camera.farClipPlane = 240;
                var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                var prior = RenderTexture.active; camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                var texture = new Texture2D(1600, 1000, TextureFormat.RGB24, false); texture.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = prior; rt.Release();
                UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(cameraGO);
            }
            var fixture = GameObject.Find("16_Left_wall_fire_extinguisher").transform;
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/open-rear-fire-layout.json"), JsonUtility.ToJson(new OpenRearFireLayout
            { extinguisherPosition = fixture.position, facing = fixture.forward, rearBoundaryZ = -dimensions.lobbyDepth }, true));
            Debug.Log("UEH_OPEN_REAR_FIRE_ASSETS_AND_PREVIEWS_SAVED");
        }

        [Serializable] private sealed class OpenRearFireLayout
        {
            public string status = "Native Unity generation and three rendered previews; no Play Mode or automated tests for this revision.";
            public bool rearClosureRemoved = true;
            public bool academicCourtyardStillRemoved = true;
            public int wallMountedExtinguishersAdded = 1;
            public Vector3 extinguisherPosition, facing;
            public float rearBoundaryZ;
        }
    }
}
#endif
