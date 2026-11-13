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
        private const float B1TowerEndSetback = 4f;
        private static float B1TowerFrontZ => B1FrameFrontZ - B1UpperTerraceInset - B1TowerEndSetback;
        private static float B1TowerRearZ => B1FrameRearZ + B1UpperTerraceInset + B1TowerEndSetback;
        private static float B1TowerDepth => B1TowerFrontZ - B1TowerRearZ;

        [MenuItem("UEH/Campus B/37. Balance upper tower length with lower floors")]
        public static void RefineB1TowerProportionsAndCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.wrappedB1Frame) throw new InvalidOperationException("Build the B1 frame first.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] {Root + "/Materials"}))
            { var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[m.name] = m; }
            string path = Root + "/Prefabs/UEH_Exterior.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach(string name in new[] {"06_Tower_shell_reference", "07_Tower_horizontal_bands_and_vertical_window_bays"})
                { var old=contents.transform.Find(name); if(old!=null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                BuildB1ReferenceTower(contents.transform);
                BuildB1FloorClosures(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            SetupB1TowerProportionViewpoints(GameObject.Find("Review viewpoints [select and align view]").transform);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            CaptureB1TowerProportionPreviews();
            float width=dimensions.frontage-3.5f;
            Directory.CreateDirectory(Path.GetFullPath("Docs/UEH-CampusB"));
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/b1-tower-proportions-layout.json"),JsonUtility.ToJson(new B1TowerProportionLayout
            {
                width=width,depth=B1TowerDepth,frontZ=B1TowerFrontZ,rearZ=B1TowerRearZ,centerX=B1TowerOffsetX,
                envelopeFloorArea=width*B1TowerDepth,previousDepth=dimensions.exteriorDepth-5,
                previousEnvelopeFloorArea=width*(dimensions.exteriorDepth-5),
                lowerTerraceDepth=B1FrameFrontZ-B1FrameRearZ-2*B1UpperTerraceInset,
                lowerTerraceWidth=B1FrameLeftX-B1FrameRightX-2*B1UpperTerraceInset,
                baseY=B1FrameTop+7.65f,roofY=dimensions.towerHeight,upperFloorCount=dimensions.upperFloors
            },true));
            Debug.Log("UEH_B1_TOWER_PROPORTIONS_COMPLETE Upper tower deepened from 19 m to " + B1TowerDepth + " m with 4 m end setbacks to the open terraces; shell, roof, floor bands and glazing follow the new footprint.");
        }

        [Serializable] private sealed class B1TowerProportionLayout
        {
            public string status="Upper tower footprint balanced against lower terraces; image-based estimated dimensions, not surveyed floor areas.";
            public float width,depth,frontZ,rearZ,centerX,envelopeFloorArea,previousDepth,previousEnvelopeFloorArea;
            public float lowerTerraceDepth,lowerTerraceWidth,baseY,roofY;
            public float endSetback=B1TowerEndSetback;
            public int upperFloorCount;
        }

        private static void SetupB1TowerProportionViewpoints(Transform views)
        {
            foreach(Transform child in new List<Transform>(GetDirectChildren(views)))
            { int n; if(child.name.Length>3 && int.TryParse(child.name.Substring(0,3),out n) && n>=121 && n<=124) UnityEngine.Object.DestroyImmediate(child.gameObject); }
            float middle=(B1TowerFrontZ+B1TowerRearZ)/2;
            Viewpoint(views,"121_B1_balanced_tower_full_side",new Vector3(75,28,middle),new Vector3(0,27,middle));
            Viewpoint(views,"122_B1_balanced_tower_front_corner",new Vector3(60,36,52),new Vector3(0,27,-20));
            Viewpoint(views,"123_B1_balanced_tower_rear_corner",new Vector3(53,30,-85),new Vector3(0,27,-30));
            Viewpoint(views,"124_B1_balanced_tower_footprint_above",new Vector3(5,110,-22),new Vector3(0,0,middle));
        }

        private static void CaptureB1TowerProportionPreviews()
        {
            string output=Path.GetFullPath("Docs/UEH-CampusB/Previews");Directory.CreateDirectory(output);
            foreach(Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                int n; if(point.name.Length<4 || !int.TryParse(point.name.Substring(0,3),out n) || n<121 || n>124) continue;
                var go=new GameObject("Temporary tower proportion review camera");var camera=go.AddComponent<Camera>();camera.enabled=false;
                camera.transform.SetPositionAndRotation(point.position,point.rotation);
                camera.fieldOfView=n==124?60:62;camera.nearClipPlane=.08f;camera.farClipPlane=250;
                if(n==124) { camera.orthographic=true; camera.orthographicSize=39; }
                var target=new RenderTexture(1800,1100,24,RenderTextureFormat.ARGB32){antiAliasing=4};
                var prior=RenderTexture.active;camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                var texture=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
                texture.ReadPixels(new Rect(0,0,target.width,target.height),0,0);texture.Apply();
                File.WriteAllBytes(Path.Combine(output,point.name+".png"),texture.EncodeToPNG());
                camera.targetTexture=null;RenderTexture.active=prior;target.Release();
                UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
#endif
