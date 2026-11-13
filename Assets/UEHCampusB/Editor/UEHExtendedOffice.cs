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
        [MenuItem("UEH/Campus B/25. Extend glass office past rear column and remove academic connection")]
        public static void ExtendGlassOfficeAndCloseRear()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.compactRightSequence || !dimensions.rightGlassOffice || !dimensions.refinedLobbyLayout)
                throw new InvalidOperationException("Build the current recessed glass office and refined lobby first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[mat.name] = mat; }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.extendedGlassOfficeAndClosedRear = true;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                // The elevator end stays at Z=-31.3. Extend only the rear end to Z=-47.3.
                PopulateRightGlassOffice(contents.transform.Find("09_Four_expansion_areas"));
                RebuildOfficeWallOpening(contents.transform);
                RemoveAcademicConnection(contents.transform);
                BuildRearLobbyClosure(contents.transform, dimensions.lobbyDepth, dimensions.lobbyHeight);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupGlassOfficeViewpoints();
            SetupExtendedOfficeViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_OFFICE_EXTENDED Office 16x4.6m from Z=-31.3 to -47.3, past column Z=-42. Front X=" + RightFacadeX(dimensions) + "; six workstations; academic passage, court, trees and sign removed; rear closed at Z=-48.");
        }

        private static void RemoveAcademicConnection(Transform root)
        {
            foreach (string name in new[] { "07_Inner_passage_inferred", "Courtyard_context_left", "Courtyard_context_right", "Inner_direction" })
            { var old = root.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
        }

        private static void BuildRearLobbyClosure(Transform root, float depth, float height)
        {
            var old = root.Find("15_Rear_lobby_closure");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            if (dimensions.openRearAndLeftExtinguisher) return;
            var wall = new Part("15_Rear_lobby_closure", root, "RearClosure");
            // A plain wall terminates the lobby at its existing floor edge.
            // No inferred courtyard, garden beds or onward passage remains behind it.
            wall.Box("Ivory", new Vector3(0, height / 2, -depth - 0.15f), new Vector3(dimensions.frontage, height, 0.3f), true);
            wall.Box("Border", new Vector3(0, 0.065f, -depth + 0.0125f), new Vector3(dimensions.frontage, 0.13f, 0.025f));
            wall.Finish();
        }

        private static void SetupExtendedOfficeViewpoints()
        {
            var views = GameObject.Find("Review viewpoints [select and align view]"); if (views == null) return;
            foreach (string name in new[] { "71_Extended_glass_office_front", "72_Office_past_souvenir_column", "73_Extended_office_interior", "74_Rear_lobby_without_academic_connection" })
            { var old = views.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Viewpoint(views.transform, "71_Extended_glass_office_front", new Vector3(-6.5f, 2, -39.3f), new Vector3(-13.4f, 1.9f, -39.3f));
            Viewpoint(views.transform, "72_Office_past_souvenir_column", new Vector3(0, 2.1f, -46), new Vector3(-13.4f, 1.9f, -39));
            Viewpoint(views.transform, "73_Extended_office_interior", new Vector3(-16.9f, 1.7f, -45.8f), new Vector3(-15.1f, 1.8f, -34.3f));
            Viewpoint(views.transform, "74_Rear_lobby_without_academic_connection", new Vector3(0, 2, -36), new Vector3(0, 2, -48));
        }

        public static void ExtendOfficeAndCapture()
        {
            ExtendGlassOfficeAndCloseRear(); CaptureReflections();
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            foreach (Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                if (!point.name.StartsWith("71_") && !point.name.StartsWith("72_") && !point.name.StartsWith("73_") && !point.name.StartsWith("74_")) continue;
                var cameraGO = new GameObject("Temporary extended-office review camera"); var camera = cameraGO.AddComponent<Camera>(); camera.enabled = false;
                camera.transform.SetPositionAndRotation(point.position, point.rotation);
                camera.fieldOfView = point.name.StartsWith("73_") ? 78 : 80;
                camera.nearClipPlane = 0.08f; camera.farClipPlane = 240;
                var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                var prior = RenderTexture.active; camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                var texture = new Texture2D(1600, 1000, TextureFormat.RGB24, false); texture.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = prior; rt.Release();
                UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(cameraGO);
            }
            var room = GameObject.Find("Glass_office_recessed").transform;
            var front = LeftGapRenderBounds(room.Find("01_FRONT_glass_wall"));
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/office-extension-layout.json"), JsonUtility.ToJson(new ExtendedOfficeLayout
            {
                centerZ = GlassOfficeZ, width = GlassOfficeWidth, depth = GlassOfficeDepth,
                nearEndZ = GlassOfficeZ + GlassOfficeWidth / 2, farEndZ = GlassOfficeZ - GlassOfficeWidth / 2,
                frontX = RightFacadeX(dimensions), frontBoundsMin = front.min, frontBoundsMax = front.max,
                columnZ = -42, rearWallZ = -dimensions.lobbyDepth, entranceWorld = room.TransformPoint(new Vector3(GlassOfficeDoorX, 0, 0))
            }, true));
            Debug.Log("UEH_OFFICE_EXTENSION_ASSETS_AND_PREVIEWS_SAVED");
        }

        [Serializable] private sealed class ExtendedOfficeLayout
        {
            public string status = "Native Unity generation and four rendered previews; no Play Mode or automated tests for this revision.";
            public float centerZ, width, depth, nearEndZ, farEndZ, frontX, columnZ, rearWallZ;
            public int workstationCount = 6;
            public bool academicConnectionRemoved = true;
            public Vector3 frontBoundsMin, frontBoundsMax, entranceWorld;
        }
    }
}
#endif
