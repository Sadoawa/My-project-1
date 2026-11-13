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
        private const float B1TowerViewerLeftMargin = 1f;
        // Positive world X is viewer-left when facing the entrance from the street (+Z).
        private static float B1TowerOffsetX => B1FrameLeftX - B1UpperTerraceInset - B1TowerViewerLeftMargin - (dimensions.frontage - 3.5f) / 2;

        private static void ApplyB1TowerAlignment(Transform exterior)
        {
            foreach(string name in new[]{"06_Tower_shell_reference","07_Tower_horizontal_bands_and_vertical_window_bays"})
            {
                var tower=exterior.Find(name);
                if(tower==null) throw new InvalidOperationException("Missing tower group: "+name);
                tower.localPosition=new Vector3(B1TowerOffsetX,0,0);
            }
        }

        [MenuItem("UEH/Campus B/39. Align upper tower toward viewer left")]
        public static void RefineB1TowerAlignmentAndCapture()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions=AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root+"/Settings/EstimatedDimensions.asset");
            if(dimensions==null || !dimensions.wrappedB1Frame) throw new InvalidOperationException("Build the B1 frame first.");
            materials.Clear();
            foreach(string guid in AssetDatabase.FindAssets("t:Material",new[]{Root+"/Materials"}))
            { var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));materials[m.name]=m; }
            string path=Root+"/Prefabs/UEH_Exterior.prefab";
            var contents=PrefabUtility.LoadPrefabContents(path);
            try
            {
                ApplyB1TowerAlignment(contents.transform);
                BuildB1FloorClosures(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            SetupB1TowerAlignmentViewpoints(GameObject.Find("Review viewpoints [select and align view]").transform);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            CaptureB1TowerAlignmentPreviews();
            float half=(dimensions.frontage-3.5f)/2;
            Directory.CreateDirectory(Path.GetFullPath("Docs/UEH-CampusB"));
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/b1-tower-alignment-layout.json"),JsonUtility.ToJson(new B1TowerAlignmentLayout
            {
                centerX=B1TowerOffsetX,viewerLeftEdgeX=B1TowerOffsetX+half,viewerRightEdgeX=B1TowerOffsetX-half,
                lowerViewerLeftEdgeX=B1FrameLeftX-B1UpperTerraceInset,lowerViewerRightEdgeX=B1FrameRightX+B1UpperTerraceInset,
                viewerRightMargin=B1TowerOffsetX-half-(B1FrameRightX+B1UpperTerraceInset),
                width=half*2,depth=B1TowerDepth,frontZ=B1TowerFrontZ,rearZ=B1TowerRearZ
            },true));
            Debug.Log("UEH_B1_TOWER_ALIGNMENT_COMPLETE Upper tower translated "+B1TowerOffsetX+" m toward viewer-left, with 1 m left margin to the lower terrace; plinth and facade returns follow the shifted tower.");
        }

        [Serializable] private sealed class B1TowerAlignmentLayout
        {
            public string status="Upper tower positioned toward viewer-left based on supplied Virtual Tour image; offset is a visual estimate.";
            public string axisConvention="Positive X is viewer-left from the entrance street (+Z).";
            public float centerX,viewerLeftEdgeX,viewerRightEdgeX,lowerViewerLeftEdgeX,lowerViewerRightEdgeX;
            public float viewerLeftMargin=B1TowerViewerLeftMargin,viewerRightMargin,width,depth,frontZ,rearZ;
            public bool towerGeometryRescaled=false;
        }

        private static void SetupB1TowerAlignmentViewpoints(Transform views)
        {
            foreach(Transform child in new List<Transform>(GetDirectChildren(views)))
            { int n;if(child.name.Length>3 && int.TryParse(child.name.Substring(0,3),out n) && n>=129 && n<=132) UnityEngine.Object.DestroyImmediate(child.gameObject); }
            Viewpoint(views,"129_B1_left_aligned_tower_front",new Vector3(1.1f,27,80),new Vector3(1.1f,27,0));
            Viewpoint(views,"130_B1_left_aligned_tower_virtual_tour_angle",new Vector3(18,1.7f,26),new Vector3(2,22,0));
            Viewpoint(views,"131_B1_left_aligned_tower_front_corner",new Vector3(56,33,45),new Vector3(3,27,-17));
            Viewpoint(views,"132_B1_shifted_tower_base_and_returns",new Vector3(29,21,2),new Vector3(B1TowerOffsetX+14,18.8f,-1.4f));
        }

        private static void CaptureB1TowerAlignmentPreviews()
        {
            string output=Path.GetFullPath("Docs/UEH-CampusB/Previews");Directory.CreateDirectory(output);
            foreach(Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                int n;if(point.name.Length<4 || !int.TryParse(point.name.Substring(0,3),out n) || n<129 || n>132) continue;
                var go=new GameObject("Temporary tower alignment review camera");var camera=go.AddComponent<Camera>();camera.enabled=false;
                camera.transform.SetPositionAndRotation(point.position,point.rotation);camera.fieldOfView=n==130?80:62;
                camera.nearClipPlane=.05f;camera.farClipPlane=250;
                if(n==129) {camera.orthographic=true;camera.orthographicSize=31;}
                var target=new RenderTexture(1800,1200,24,RenderTextureFormat.ARGB32){antiAliasing=4};
                var prior=RenderTexture.active;camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                var texture=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
                texture.ReadPixels(new Rect(0,0,target.width,target.height),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(output,point.name+".png"),texture.EncodeToPNG());
                camera.targetTexture=null;RenderTexture.active=prior;target.Release();
                UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
#endif
