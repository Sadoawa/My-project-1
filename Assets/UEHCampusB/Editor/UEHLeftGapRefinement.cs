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
        [MenuItem("UEH/Campus B/24. Widen white wall gaps along left gallery")]
        public static void WidenLeftGalleryGaps()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.compactLeftSequence || !dimensions.leftOfficeAndLounge)
                throw new InvalidOperationException("Build the current left gallery, office and lounge first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            { var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[mat.name] = mat; }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.widerLeftGaps = true;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var zone = contents.transform.Find("06_1976_LEFT_from_gate");
                zone.localPosition = new Vector3(LeftFacadeX(dimensions), 0, LeftHeritageZ(dimensions));
                var gallery = zone.Find("06_Heritage_1976_and_gallery");
                // Keep the slate and its backing at the entrance. The three exhibit panels
                // move together, exposing a wider strip of the existing white wall.
                BuildHeritageContrast(gallery);
                gallery.Find("01_Plain_wall_left").localPosition = new Vector3(5.65f + LeftHeritageSpacingOffset(dimensions), 0, 0);
                CompactLeftBays(contents.transform.Find("09_Four_expansion_areas"));
                RebuildOfficeWallOpening(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupWiderLeftGapViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_LEFT_GAPS_WIDENED Slate anchored near entrance; gallery Z=-9.4, office Z=-22, green/timber lounge Z=-34. White gaps about 0.8m, 1.72m, 1.59m; facade stays X=" + LeftFacadeX(dimensions) + ". Estimated layout from supplied photo.");
        }

        private static void SetupWiderLeftGapViewpoints()
        {
            var views = GameObject.Find("Review viewpoints [select and align view]"); if (views == null) return;
            foreach (string name in new[] { "67_Wider_gap_slate_to_Since1976", "68_Wider_gap_heritage_to_office", "69_Wider_gap_office_to_greenery", "70_Left_gallery_wider_spacing_overview" })
            { var old = views.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            float x = LeftFacadeX(dimensions);
            Viewpoint(views.transform, "67_Wider_gap_slate_to_Since1976", new Vector3(5.5f, 1.7f, -7), new Vector3(x, 2, -7.8f));
            Viewpoint(views.transform, "68_Wider_gap_heritage_to_office", new Vector3(7.3f, 1.7f, -15.9f), new Vector3(x, 2, -15.9f));
            Viewpoint(views.transform, "69_Wider_gap_office_to_greenery", new Vector3(7, 1.7f, -28.1f), new Vector3(x, 2, -28.1f));
            Viewpoint(views.transform, "70_Left_gallery_wider_spacing_overview", new Vector3(5.7f, 1.85f, -21.2f), new Vector3(x, 2, -21.2f));
        }

        public static void WidenLeftGapsAndCapture()
        {
            WidenLeftGalleryGaps(); CaptureReflections();
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            foreach (Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                if (!point.name.StartsWith("67_") && !point.name.StartsWith("68_") && !point.name.StartsWith("69_") && !point.name.StartsWith("70_")) continue;
                var cameraGO = new GameObject("Temporary left-spacing review camera"); var camera = cameraGO.AddComponent<Camera>(); camera.enabled = false;
                camera.transform.SetPositionAndRotation(point.position, point.rotation);
                camera.fieldOfView = point.name.StartsWith("70_") ? 112 : point.name.StartsWith("67_") ? 80 : 72;
                camera.nearClipPlane = 0.08f; camera.farClipPlane = 240;
                var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                var prior = RenderTexture.active; camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                var texture = new Texture2D(1600, 1000, TextureFormat.RGB24, false); texture.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = prior; rt.Release();
                UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(cameraGO);
            }
            var gallery = GameObject.Find("06_1976_LEFT_from_gate").transform.Find("06_Heritage_1976_and_gallery");
            var slate = LeftGapRenderBounds(gallery.Find("00_Contrasting_slate_panel"));
            var display = LeftGapRenderBounds(gallery.Find("Timber_frame"));
            var office = LeftGapRenderBounds(GameObject.Find("Office_room").transform.Find("Office_blue_perimeter"));
            var greenery = LeftGapRenderBounds(GameObject.Find("Green_timber_lounge").transform.Find("01_Greenery_LEFT"));
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/left-spacing-layout.json"), JsonUtility.ToJson(new LeftGapLayout
            {
                heritageZ = LeftHeritageZ(dimensions), officeZ = LeftOfficeZ(dimensions), loungeZ = LeftLoungeZ(dimensions),
                slateToHeritage = slate.min.z - display.max.z, heritageToOffice = display.min.z - office.max.z,
                officeToGreenery = office.min.z - greenery.max.z, facadeX = LeftFacadeX(dimensions)
            }, true));
            Debug.Log("UEH_LEFT_SPACING_ASSETS_AND_PREVIEWS_SAVED");
        }

        private static Bounds LeftGapRenderBounds(Transform root)
        {
            var renderers = root.GetComponentsInChildren<MeshRenderer>();
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        [Serializable] private sealed class LeftGapLayout
        {
            public string status = "Native Unity generation and four rendered previews; facade gaps recorded from renderer bounds; no Play Mode tests for this revision.";
            public float heritageZ, officeZ, loungeZ, slateToHeritage, heritageToOffice, officeToGreenery, facadeX;
        }
    }
}
#endif
