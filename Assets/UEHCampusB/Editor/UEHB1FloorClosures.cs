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
        private const float B1TerraceWalkwayWidth = 2.6f;

        private static void BuildB1FloorClosures(Transform exterior)
        {
            var old = exterior.Find("19_Floor_enclosures_and_junctions");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var p = new Part("19_Floor_enclosures_and_junctions", exterior, "B1FloorClosure");
            float r = B1FrameRightX + B1UpperTerraceInset + B1TerraceWalkwayWidth;
            float l = B1FrameLeftX - B1UpperTerraceInset - B1TerraceWalkwayWidth;
            float front = B1FrameFrontZ - B1UpperTerraceInset - 4f;
            float rear = B1FrameRearZ + B1UpperTerraceInset + B1TerraceWalkwayWidth;
            for (int floor = 0; floor < 2; floor++)
            {
                float y = B1FrameTop + floor * 3.6f;
                // The existing slab is solid. Close the room perimeter, leaving an exterior balcony corridor.
                foreach (float x in new[] {r,l})
                {
                    float outward = x == r ? -1 : 1;
                    AddB1InfillWall(p, new Vector3(x,y,(front+rear)/2), front-rear, true, outward);
                }
                AddB1InfillWall(p, new Vector3((r+l)/2,y,rear), l-r, false, -1);
                // Lap the existing front infill against the slab underside instead of leaving a sky slit.
                p.Box("Ivory",new Vector3((r+l)/2,y+3.54f,front),new Vector3(l-r+0.22f,0.24f,0.26f));
                // A shallow ceiling liner overlaps perimeter heads and closes corner seams.
                p.Box("Ivory",new Vector3((r+l)/2,y+3.57f,(front+rear)/2),new Vector3(l-r+0.22f,0.10f,front-rear+0.22f));
            }
            // Tie the upper tower to the terrace roof, including the former 90 mm gap below the shell.
            float roofTop = B1FrameTop + 7.2f + 0.36f;
            float towerBase = B1FrameTop + 7.65f;
            float half = (dimensions.frontage - 3.5f)/2;
            p.Box("Ivory",new Vector3(0,(roofTop+towerBase+0.12f)/2,(B1TowerFrontZ+B1TowerRearZ)/2),
                new Vector3(half*2+0.08f,towerBase+0.12f-roofTop,B1TowerDepth+0.08f));
            // Close the side returns where recessed front glazing formerly ended short of the side shell.
            foreach (float x in new[] {-half,half})
            {
                float outer = x < 0 ? -1 : 1;
                p.Box("BlueGlass",new Vector3(x-outer*0.06f,(towerBase+dimensions.towerHeight)/2,B1TowerFrontZ-0.76f),
                    new Vector3(0.12f,dimensions.towerHeight-towerBase,1.52f));
                p.Box("Ivory",new Vector3(x-outer*0.03f,(towerBase+dimensions.towerHeight)/2,B1TowerFrontZ-0.02f),
                    new Vector3(0.18f,dimensions.towerHeight-towerBase,0.18f));
            }
            p.Finish();
        }

        private static void AddB1InfillWall(Part p, Vector3 origin, float length, bool alongZ, float outward)
        {
            // Actual wall fragments around recessed glazing; no opaque panel hiding a gap behind a window.
            Action<string,float,float,float,float,float> box = (material,u,y,w,h,offset) =>
            {
                var pos = origin + (alongZ ? new Vector3(offset,y,u) : new Vector3(u,y,offset));
                p.Box(material,pos,alongZ ? new Vector3(0.22f,h,w) : new Vector3(w,h,0.22f));
            };
            box("Ivory",0,0.72f,length+0.22f,0.76f,0);
            box("Ivory",0,3.30f,length+0.22f,0.64f,0);
            int count = Mathf.Max(1,Mathf.RoundToInt(length/4f));
            float step = length/count;
            for(int i=0;i<count;i++)
            {
                float u = -length/2+(i+0.5f)*step;
                var center=origin+(alongZ?new Vector3(0,2.04f,u):new Vector3(u,2.04f,0));
                p.Box("BlueGlass",center,alongZ?new Vector3(0.09f,1.96f,step-0.20f):new Vector3(step-0.20f,1.96f,0.09f));
                // Window frames face the balcony, and overlap the surrounding wall by a few centimetres.
                foreach(float yy in new[]{1.08f,2.04f,3.0f}) box("White",u,yy,step-0.18f,0.07f,outward*0.10f);
                box("White",u,2.04f,0.07f,1.96f,outward*0.10f);
            }
            for(int i=0;i<=count;i++) box("Ivory",-length/2+i*step,2.04f,0.24f,1.98f,0);
        }

        [MenuItem("UEH/Campus B/38. Close missing floor infills and slab junctions")]
        public static void RefineB1FloorGapsAndCapture()
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
            try { BuildB1FloorClosures(contents.transform);PrefabUtility.SaveAsPrefabAsset(contents,path); }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            SetupB1FloorClosureViewpoints(GameObject.Find("Review viewpoints [select and align view]").transform);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            CaptureB1FloorClosurePreviews();
            Directory.CreateDirectory(Path.GetFullPath("Docs/UEH-CampusB"));
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/b1-floor-closure-layout.json"),JsonUtility.ToJson(new B1FloorClosureLayout
            {
                roomRightX=B1FrameRightX+B1UpperTerraceInset+B1TerraceWalkwayWidth,
                roomLeftX=B1FrameLeftX-B1UpperTerraceInset-B1TerraceWalkwayWidth,
                roomFrontZ=B1FrameFrontZ-B1UpperTerraceInset-4f,
                roomRearZ=B1FrameRearZ+B1UpperTerraceInset+B1TerraceWalkwayWidth,
                floorLevels=new[]{B1FrameTop,B1FrameTop+3.6f},
                roofTop=B1FrameTop+7.56f,towerBase=B1FrameTop+7.65f
            },true));
            Debug.Log("UEH_B1_FLOOR_CLOSURE_COMPLETE Two enclosed room perimeters with side/rear windows; continuous ceiling heads; roof-to-tower plinth and tower facade returns closed.");
        }

        [Serializable] private sealed class B1FloorClosureLayout
        {
            public string status="Missing room walls and junction gaps closed; dimensions are estimates from the current reference model.";
            public float roomRightX,roomLeftX,roomFrontZ,roomRearZ,roofTop,towerBase;
            public float walkwayWidth=B1TerraceWalkwayWidth;
            public float[] floorLevels;
        }

        private static void SetupB1FloorClosureViewpoints(Transform views)
        {
            foreach(Transform child in new List<Transform>(GetDirectChildren(views)))
            { int n;if(child.name.Length>3 && int.TryParse(child.name.Substring(0,3),out n) && n>=125 && n<=128) UnityEngine.Object.DestroyImmediate(child.gameObject); }
            Viewpoint(views,"125_B1_closed_floor_side",new Vector3(44,17,-23),new Vector3(17,14.4f,-23));
            Viewpoint(views,"126_B1_closed_floor_rear",new Vector3(18,17,-66),new Vector3(1,14,-42));
            Viewpoint(views,"127_B1_enclosed_room_beside_balcony",new Vector3(21.55f,12.5f,-34),new Vector3(21.55f,12.5f,-4));
            Viewpoint(views,"128_B1_closed_tower_base_junction",new Vector3(25,21,-1),new Vector3(14,18.5f,-1.4f));
        }

        private static void CaptureB1FloorClosurePreviews()
        {
            string output=Path.GetFullPath("Docs/UEH-CampusB/Previews");Directory.CreateDirectory(output);
            foreach(Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                int n;if(point.name.Length<4 || !int.TryParse(point.name.Substring(0,3),out n) || n<125 || n>128) continue;
                var go=new GameObject("Temporary floor closure review camera");var camera=go.AddComponent<Camera>();camera.enabled=false;
                camera.transform.SetPositionAndRotation(point.position,point.rotation);camera.fieldOfView=n==127?73:65;
                camera.nearClipPlane=.05f;camera.farClipPlane=220;
                var target=new RenderTexture(1800,1100,24,RenderTextureFormat.ARGB32){antiAliasing=4};
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
