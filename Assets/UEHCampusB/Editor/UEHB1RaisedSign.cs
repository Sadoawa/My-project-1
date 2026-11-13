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
        [Serializable] private sealed class RaisedSignMesh { public float[] vertices; public int[] triangles; }
        [Serializable] private sealed class RaisedSignData
        {
            public string text;
            public float width, capHeight, outlineWidth, bodyDepth;
            public float[] supportX;
            public RaisedSignMesh blue, white;
        }

        private static void BuildB1RaisedUniversitySign(Transform parent, float centerX)
        {
            var data = JsonUtility.FromJson<RaisedSignData>(File.ReadAllText(Root + "/Fonts/B1RaisedSignGeometry.json"));
            var blue = Mat("B1SignBlue", "00ADEE", 0.38f);
            // Preserve the bright cyan acrylic face under the shaded canopy; white returns retain lighting.
            blue.shader = Shader.Find("Unlit/Color");
            blue.color = new Color(0, 173f / 255, 238f / 255);
            Mat("B1SignWhite", "FAF9F5", 0.32f);
            float baseY = B1FrameTop + B1CanopyThickness + 0.08f;
            float baseline = baseY + 0.40f;
            float rearZ = B1FrameFrontZ + B1CanopyProjection - 0.24f;
            var part = new Part("University_name", parent, "B1RaisedSign");
            var transform = Matrix4x4.TRS(new Vector3(centerX + data.width / 2, baseline, rearZ), Quaternion.Euler(0,180,0), Vector3.one);
            AppendRaisedSignMesh(part, "B1SignWhite", data.white, transform);
            AppendRaisedSignMesh(part, "B1SignBlue", data.blue, transform);
            foreach (float u in data.supportX)
            {
                float x = centerX + data.width / 2 - u;
                // Slim rear fixing, short foot and anchor plate, with no continuous backing panel.
                part.Box("B1SignWhite", new Vector3(x, baseY + 0.35f, rearZ - 0.035f), new Vector3(0.045f,0.70f,0.045f));
                part.Box("B1SignWhite", new Vector3(x, baseY + 0.025f, rearZ - 0.13f), new Vector3(0.045f,0.05f,0.25f));
                part.Box("Metal", new Vector3(x, baseY + 0.008f, rearZ - 0.13f), new Vector3(0.15f,0.016f,0.28f));
            }
            part.Finish();
        }

        private static void AppendRaisedSignMesh(Part part, string material, RaisedSignMesh data, Matrix4x4 transform)
        {
            var mesh = new Mesh { indexFormat = IndexFormat.UInt32 };
            var vertices = new Vector3[data.vertices.Length / 3];
            var uv = new Vector2[vertices.Length];
            for (int i=0;i<vertices.Length;i++)
            {
                vertices[i] = new Vector3(data.vertices[3*i],data.vertices[3*i+1],data.vertices[3*i+2]);
                uv[i] = new Vector2(vertices[i].x, vertices[i].y);
            }
            mesh.vertices=vertices; mesh.uv=uv; mesh.triangles=data.triangles;
            mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
            part.AppendMesh(material,mesh,transform);
            UnityEngine.Object.DestroyImmediate(mesh);
        }

        [MenuItem("UEH/Campus B/36. Build freestanding blue and white university letters")]
        public static void RefineB1RaisedSignAndCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions=AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root+"/Settings/EstimatedDimensions.asset");
            if (dimensions==null || !dimensions.wrappedB1Frame) throw new InvalidOperationException("Build the B1 frame first.");
            materials.Clear();
            foreach(string guid in AssetDatabase.FindAssets("t:Material",new[]{Root+"/Materials"}))
            { var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[m.name]=m; }
            string path=Root+"/Prefabs/UEH_Exterior.prefab";
            var contents=PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach(string name in new[]{"University_name","05_University_identity"})
                { var old=contents.transform.Find(name); if(old!=null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                BuildB1CanopyName(contents.transform,B1FrameRightX,B1FrameLeftX);
                PrefabUtility.SaveAsPrefabAsset(contents,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            SetupB1RaisedSignViewpoints(GameObject.Find("Review viewpoints [select and align view]").transform);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            CaptureB1RaisedSignPreviews();
            var data=JsonUtility.FromJson<RaisedSignData>(File.ReadAllText(Root+"/Fonts/B1RaisedSignGeometry.json"));
            Directory.CreateDirectory(Path.GetFullPath("Docs/UEH-CampusB"));
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/b1-raised-sign-layout.json"),JsonUtility.ToJson(new RaisedSignLayout
            { text=data.text,width=data.width,capHeight=data.capHeight,outlineWidth=data.outlineWidth,bodyDepth=data.bodyDepth,
              baselineY=B1FrameTop+B1CanopyThickness+0.48f,rearZ=B1FrameFrontZ+B1CanopyProjection-0.24f,
              centerX=(B1FrameRightX+B1FrameLeftX)/2,supportCount=data.supportX.Length },true));
            Debug.Log("UEH_B1_RAISED_SIGN_COMPLETE Old small text removed; freestanding solid blue letters with white outlines and individual support brackets installed above the canopy.");
        }

        [Serializable] private sealed class RaisedSignLayout
        {
            public string text;
            public string status="Freestanding lettering based on supplied Virtual Tour image; colours, typography and dimensions are visual estimates.";
            public string blueHex="00ADEE", whiteHex="FAF9F5";
            public float width,capHeight,outlineWidth,bodyDepth,baselineY,rearZ,centerX;
            public int supportCount;
            public bool uprightSolidLetters=true,individuallySupported=true,continuousBackingPanel=false;
        }

        private static void SetupB1RaisedSignViewpoints(Transform views)
        {
            foreach(Transform child in new List<Transform>(GetDirectChildren(views)))
            { int n; if(child.name.Length>3 && int.TryParse(child.name.Substring(0,3),out n) && n>=117 && n<=120) UnityEngine.Object.DestroyImmediate(child.gameObject); }
            Viewpoint(views,"117_B1_raised_sign_front",new Vector3(1.1f,13,36),new Vector3(1.1f,12,0));
            Viewpoint(views,"118_B1_raised_sign_white_outline_closeup",new Vector3(11,12.6f,12),new Vector3(5.5f,12.1f,4.2f));
            Viewpoint(views,"119_B1_raised_sign_from_street",new Vector3(22,1.7f,32),new Vector3(2,13,1));
            Viewpoint(views,"120_B1_raised_sign_rear_supports",new Vector3(9,12.5f,2.2f),new Vector3(9,11.9f,4.3f));
        }

        private static void CaptureB1RaisedSignPreviews()
        {
            string output=Path.GetFullPath("Docs/UEH-CampusB/Previews"); Directory.CreateDirectory(output);
            foreach(Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                int n; if(point.name.Length<4 || !int.TryParse(point.name.Substring(0,3),out n) || n<117 || n>120) continue;
                var go=new GameObject("Temporary raised lettering camera");var camera=go.AddComponent<Camera>();camera.enabled=false;
                camera.transform.SetPositionAndRotation(point.position,point.rotation);
                camera.fieldOfView=n==117?58:n==118?62:n==120?85:68;camera.nearClipPlane=.05f;camera.farClipPlane=220;
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
