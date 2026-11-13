#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UEH.B2.Editor
{
    public static class CoSoB2Builder
    {
        const string Root = "Assets/CoSoB2";
        public const string ScenePath = Root + "/Scenes/CoSoB2_Preview.unity";
        static Material cream, trim, stone, red, glass, metal, green, bark, soil, wood;
        static int meshId;
        static Transform model;
        static readonly System.Random rng = new System.Random(28);

        [MenuItem("UEH/CoSoB2/Open preview scene")]
        public static void OpenPreview()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.Log("CoSoB2: Exit Play mode before opening the preview scene.");
                return;
            }
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("CoSoB2: Preview scene is missing. Use Build separate preview if no model has been created yet.");
                return;
            }
            // Batch runs must not change their loaded scenes just because assets already exist.
            if (Application.isBatchMode)
            {
                Debug.Log("CoSoB2: Existing preview retained at " + ScenePath);
                return;
            }
            var current = SceneManager.GetActiveScene();
            if (current.path != ScenePath || SceneManager.sceneCount != 1)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("CoSoB2: Preview ready. Press Play to use the camera view buttons.");
        }

        [MenuItem("UEH/CoSoB2/Build separate preview")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
            if (File.Exists(ScenePath))
            {
                OpenPreview();
                return;
            }
            if (File.Exists(Root + "/Prefabs/CoSoB2.prefab"))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/CoSoB2.prefab");
                Debug.LogWarning("CoSoB2: Existing prefab selected. The preview scene is missing; restore it or place this prefab in a new scene. No assets were overwritten.");
                return;
            }
            foreach (string folder in new[] { "Scenes", "Prefabs", "Materials", "Meshes", "Textures" }) Directory.CreateDirectory(Root + "/" + folder);
            AssetDatabase.Refresh();
            var previous = SceneManager.GetActiveScene();
            if (!Application.isBatchMode && string.IsNullOrEmpty(previous.path))
                throw new InvalidOperationException("Save your untitled scene before building the separate B2 preview.");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, Application.isBatchMode ? NewSceneMode.Single : NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            meshId = 0;
            cream = Mat("Warm_cream_plaster", new Color(.83f,.79f,.62f));
            trim = Mat("Ivory_moulding", new Color(.94f,.92f,.81f));
            stone = Mat("Grey_granite", new Color(.39f,.43f,.43f));
            red = Mat("Red_terrazzo", new Color(.45f,.25f,.22f));
            glass = Mat("Smoky_glass", new Color(.16f,.25f,.25f), .62f);
            metal = Mat("Dark_railings", new Color(.09f,.12f,.11f), .4f);
            green = Mat("Foliage", new Color(.19f,.34f,.08f));
            bark = Mat("Bark", new Color(.29f,.23f,.15f));
            soil = Mat("Planting_soil", new Color(.15f,.17f,.10f));
            wood = Mat("Orange_benches", new Color(.67f,.30f,.08f));
            Grain(stone, false); Grain(red, false); Grain(cream, false);
            model = new GameObject("CoSoB2").transform;
            var shell = Group("01_B2_arcade_block_EST_29.4x9m_3_visible_levels", model);
            // Front faces -Z. Ground plus two upper levels inferred from the opening shot.
            for (int floor = 0; floor <= 3; floor++)
                Box("Floor_slab_" + floor, shell, new Vector3(0, floor*3.9f-.12f, 4.5f), new Vector3(30,.24f,9.6f), floor==0?red:trim);
            for (int level = 0; level < 3; level++)
            {
                float y = level*3.9f;
                for (int bay = 0; bay <= 7; bay++)
                {
                    float x = -14.7f + bay*4.2f;
                    Box("Square_pier",shell,new Vector3(x,y+1.95f,0),new Vector3(.58f,3.9f,.65f),cream);
                    Box("Pier_base",shell,new Vector3(x,y+.2f,-.04f),new Vector3(.76f,.4f,.8f),trim);
                    Box("Pier_capital",shell,new Vector3(x,y+2.25f,-.04f),new Vector3(.74f,.16f,.8f),trim);
                }
                Box("Rear_wall",shell,new Vector3(0,y+1.95f,7),new Vector3(29.4f,3.9f,.25f),cream);
                for (int bay = 0; bay < 7; bay++)
                {
                    float x = -12.6f+bay*4.2f;
                    if (level == 0)
                    {
                        Arch(shell,x,0,0,1.81f,2.16f,3.9f);
                        if (bay != 3) Window(shell,x,1.8f,6.83f,3.3f,2.9f);
                    }
                    else
                    {
                        Box("Balcony_solid_parapet",shell,new Vector3(x,y+.48f,-.10f),new Vector3(3.65f,.96f,.25f),cream);
                        Box("Parapet_top",shell,new Vector3(x,y+1,-.13f),new Vector3(3.82f,.10f,.36f),trim);
                        Box("Recessed_panel",shell,new Vector3(x,y+.5f,-.242f),new Vector3(2.9f,.55f,.035f),trim);
                        Window(shell,x,y+2.1f,6.83f,2.5f,2.6f);
                        if (bay%2==0 || level==1) Shrub(shell,new Vector3(x,y+1.03f,.55f),1.2f);
                    }
                }
                for (int end = -1; end <= 1; end+=2)
                    Box("Simplified_unseen_end",shell,new Vector3(end*14.7f,y+1.95f,4.5f),new Vector3(.3f,3.9f,9),cream);
                Box("Facade_string_course",shell,new Vector3(0,y+3.77f,-.10f),new Vector3(30,.18f,.88f),trim);
            }
            // A walk-through opening through the ground-floor rear partition.
            var rear = shell.Find("Rear_wall");
            if(rear!=null) UnityEngine.Object.DestroyImmediate(rear.gameObject);
            Box("Passage_left_wall",shell,new Vector3(-8.4f,1.95f,7),new Vector3(12.6f,3.9f,.25f),cream);
            Box("Passage_right_wall",shell,new Vector3(8.4f,1.95f,7),new Vector3(12.6f,3.9f,.25f),cream);
            Arch(shell,0,7,0,1.81f,2.16f,3.9f);
            var court = Group("02_Courtyard_facing_windows_REF_87s_EST_layout",model);
            for(int bay=0;bay<7;bay++)
            {
                float x=-12.6f+bay*4.2f;
                if(bay!=3)
                {
                    Box("Courtyard_sill_wall",court,new Vector3(x,.45f,9),new Vector3(3.65f,.9f,.3f),cream);
                    Window(court,x,2.3f,9,3.5f,2.8f);
                }
                Arch(court,x,9,0,1.81f,2.16f,3.9f);
                for(int level=1;level<3;level++)
                {
                    float y=3.9f*level;
                    Box("Upper_infill",court,new Vector3(x,y+1.95f,9),new Vector3(4.2f,3.9f,.26f),cream);
                    Window(court,x,y+2,9.18f,2.65f,2.2f);
                    Box("Window_hood",court,new Vector3(x,y+3.2f,9.25f),new Vector3(2.95f,.14f,.32f),trim);
                }
            }
            for(int i=0;i<=7;i++) Box("Courtyard_pier",court,new Vector3(-14.7f+i*4.2f,1.95f,9),new Vector3(.58f,3.9f,.65f),cream);
            var ground = Group("03_Paving_and_garden_EST_spatial_arrangement",model);
            Box("Site_paving",ground,new Vector3(0,-.25f,5),new Vector3(40,.22f,42),stone);
            for(int x=-19;x<20;x+=2) Box("Paving_joint",ground,new Vector3(x,-.132f,5),new Vector3(.018f,.006f,42),metal);
            for(int z=-15;z<26;z+=2) Box("Paving_joint",ground,new Vector3(0,-.131f,z),new Vector3(40,.006f,.018f),metal);
            for(int i=0;i<4;i++)
            {
                float x=-10.5f+i*7;
                Planter(ground,new Vector3(x,0,16.5f),3.8f,3.8f);
                Tree(ground,new Vector3(x,.45f,16.5f));
                Bench(ground,new Vector3(x,0,20));
            }
            for(int i=0;i<7;i++)
            {
                float x=-12.6f+i*4.2f;
                if(i!=3) { Planter(ground,new Vector3(x,0,-.95f),1.05f,.8f); Shrub(ground,new Vector3(x,.5f,-.95f),.85f); }
            }
            var details=Group("04_Corridor_details_REF_4_8_14s_EST_positions",model);
            for(int i=0;i<7;i++)
            {
                float x=-12.6f+i*4.2f;
                Box("Ceiling_beam",details,new Vector3(x,3.55f,4.5f),new Vector3(.2f,.4f,9),trim);
                Box("Fluorescent_fitting",details,new Vector3(x,3.5f,3.8f),new Vector3(.25f,.06f,1.1f),trim);
                if(i!=3) { Bench(details,new Vector3(x,0,4)); Window(details,x,1.9f,2,3.4f,2.9f); }
            }
            Box("Fire_cabinet",details,new Vector3(2.65f,1.5f,1.5f),new Vector3(.6f,.9f,.25f),red);
            Box("Vending_machine",details,new Vector3(3.3f,1.05f,1.9f),new Vector3(1.15f,2.1f,.8f),metal);
            Box("Vending_display",details,new Vector3(3.3f,1.6f,1.48f),new Vector3(.92f,.55f,.03f),wood);
            PrefabUtility.SaveAsPrefabAsset(model.gameObject,Root+"/Prefabs/CoSoB2.prefab");
            UnityEngine.Object.DestroyImmediate(model.gameObject);
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/CoSoB2.prefab"),scene);
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.65f,.70f,.76f);
            var sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.5f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(48,-35,0);
            var cam=new GameObject("Preview Camera").AddComponent<Camera>(); cam.tag="MainCamera";cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.65f,.76f,.82f);cam.nearClipPlane=.08f;cam.farClipPlane=300;cam.fieldOfView=48;
            var controls=cam.gameObject.AddComponent<UEH.B2.CoSoB2Preview>();controls.View(0);
            EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Docs/CoSoB2/Previews");
            for(int i=0;i<4;i++) { controls.View(i); Capture(cam,"Docs/CoSoB2/Previews/view_"+i+".png"); }
            controls.View(0);EditorSceneManager.SaveScene(scene,ScenePath);
            if(previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            Debug.Log("COSOB2_BUILD_COMPLETE "+ScenePath);
        }
        static Transform Group(string n,Transform p) { var g=new GameObject(n).transform;g.SetParent(p,false);return g; }
        // Batch-only finishing pass for the isolated generated preview.
        public static void PolishPreview()
        {
            if(!Application.isBatchMode) throw new InvalidOperationException("Use this finishing pass only in the isolated build project.");
            trim=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Ivory_moulding.mat");
            glass=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Smoky_glass.mat");
            string path=Root+"/Prefabs/CoSoB2.prefab";
            var prefab=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var court=prefab.transform.Find("02_Courtyard_facing_windows_REF_87s_EST_layout");
                var remove=new List<GameObject>();
                foreach(Transform child in court) if(child.name.StartsWith("Window_EST_") && child.Find("Glass").localPosition.y<3) remove.Add(child.gameObject);
                foreach(var g in remove) UnityEngine.Object.DestroyImmediate(g);
                for(int i=0;i<7;i++) if(i!=3) Window(court,-12.6f+i*4.2f,2.3f,9,3.5f,2.8f);
                PrefabUtility.SaveAsPrefabAsset(prefab,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var cam=Camera.main;var controls=cam.GetComponent<UEH.B2.CoSoB2Preview>();
            for(int i=0;i<5;i++){controls.View(i);Capture(cam,"Docs/CoSoB2/Previews/view_"+i+".png");}
            controls.View(0);EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),ScenePath);AssetDatabase.SaveAssets();
            Debug.Log("COSOB2_POLISH_COMPLETE");
        }
        static Material Mat(string n,Color c,float smooth=.12f) { var m=new Material(Shader.Find("Standard"));m.name=n;m.color=c;m.SetFloat("_Glossiness",smooth);AssetDatabase.CreateAsset(m,Root+"/Materials/"+n+".mat");return m; }
        static void Grain(Material m,bool unused)
        {
            var t=new Texture2D(128,128,TextureFormat.RGB24,false);t.name=m.name+"_grain";
            var pixels=new Color[128*128]; for(int i=0;i<pixels.Length;i++){float v=.78f+(float)rng.NextDouble()*.22f;pixels[i]=new Color(v,v,v);}
            t.SetPixels(pixels);t.Apply();AssetDatabase.CreateAsset(t,Root+"/Textures/"+t.name+".asset");m.mainTexture=t;m.mainTextureScale=new Vector2(4,4);
        }
        static GameObject Box(string n,Transform p,Vector3 pos,Vector3 size,Material mat) {return Shape(n,p,pos,size,mat,PrimitiveType.Cube);}
        static GameObject Shape(string n,Transform p,Vector3 pos,Vector3 size,Material mat,PrimitiveType type)
        {var g=GameObject.CreatePrimitive(type);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;return g;}
        static void Window(Transform p,float x,float y,float z,float w,float h)
        {
            var g=Group("Window_EST_"+x+"_"+y,p);
            Box("Glass",g,new Vector3(x,y,z),new Vector3(w,h,.045f),glass);
            foreach(float dx in new[]{-w/2,0,w/2})Box("Vertical_frame",g,new Vector3(x+dx,y,z),new Vector3(.045f,h+.08f,.11f),trim);
            foreach(float dy in new[]{-h/2,0,h/2})Box("Horizontal_frame",g,new Vector3(x,y+dy,z),new Vector3(w+.08f,.045f,.11f),trim);
        }
        static void Arch(Transform p,float x,float z,float y,float r,float spring,float top)
        {
            var g=Group("Arch_REF_8s",p);
            // Extruded spandrel leaves a real open arch, not a painted arch on a wall.
            var v=new List<Vector3>();var triangles=new List<int>(); int segments=32;
            for(int i=0;i<=segments;i++)
            { float dx=-r+2*r*i/segments;float arc=spring+Mathf.Sqrt(Mathf.Max(0,r*r-dx*dx))*.82f;
              v.Add(new Vector3(x+dx,y+arc,z-.22f));v.Add(new Vector3(x+dx,y+top,z-.22f));v.Add(new Vector3(x+dx,y+arc,z+.22f));v.Add(new Vector3(x+dx,y+top,z+.22f)); }
            for(int i=0;i<segments;i++){int a=i*4,b=a+4;foreach(int k in new[]{a,a+1,b,b,a+1,b+1,a+2,b+2,a+3,b+2,b+3,a+3,a,b,a+2,b,b+2,a+2})triangles.Add(k);}
            var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Root+"/Meshes/Arch_"+(meshId++)+".asset");
            var fill=new GameObject("Spandrel");fill.transform.SetParent(g,false);fill.AddComponent<MeshFilter>().sharedMesh=mesh;fill.AddComponent<MeshRenderer>().sharedMaterial=cream;fill.AddComponent<MeshCollider>().sharedMesh=mesh;
            for(int i=0;i<segments;i++)
            { float a=Mathf.PI*i/segments,b=Mathf.PI*(i+1)/segments;var start=new Vector3(x+r*Mathf.Cos(a),y+spring+r*.82f*Mathf.Sin(a),z-.27f);var end=new Vector3(x+r*Mathf.Cos(b),y+spring+r*.82f*Mathf.Sin(b),z-.27f);var piece=Box("Arch_moulding",g,(start+end)/2,new Vector3(.105f,Vector3.Distance(start,end)+.025f,.13f),trim);piece.transform.up=(end-start).normalized; }
            Box("Keystone",g,new Vector3(x,y+spring+r*.82f-.05f,z-.35f),new Vector3(.25f,.42f,.23f),trim);
        }
        static void Planter(Transform p,Vector3 pos,float w,float d)
        { var g=Group("Stone_planter",p);Box("Body",g,pos+Vector3.up*.22f,new Vector3(w,.44f,d),stone);Box("Soil",g,pos+Vector3.up*.45f,new Vector3(w-.16f,.03f,d-.16f),soil);for(int i=0;i<9;i++)Shrub(g,pos+new Vector3(((i%3)-1)*w*.28f,.48f,((i/3)-1)*d*.28f),.5f); }
        static void Shrub(Transform p,Vector3 pos,float size)
        { for(int i=0;i<4;i++){var g=Shape("Leaf_cluster",p,pos+new Vector3((float)(rng.NextDouble()-.5)*size,size*.45f,(float)(rng.NextDouble()-.5)*size),new Vector3(size*.7f,size*(.7f+(float)rng.NextDouble()*.5f),size*.65f),green,PrimitiveType.Sphere);UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());} }
        static void Tree(Transform p,Vector3 pos)
        {var g=Group("Courtyard_tree_EST",p);Shape("Trunk",g,pos+Vector3.up*1.9f,new Vector3(.16f,1.9f,.16f),bark,PrimitiveType.Cylinder);for(int i=0;i<7;i++){float a=i*Mathf.PI*2/7;Shrub(g,pos+new Vector3(Mathf.Cos(a)*1.15f,3.6f+(i%2)*.5f,Mathf.Sin(a)*1.15f),1.6f);} }
        static void Bench(Transform p,Vector3 pos)
        {Box("Bench_seat",p,pos+Vector3.up*.48f,new Vector3(2.4f,.12f,.48f),wood);for(int i=-1;i<=1;i+=2)Box("Bench_leg",p,pos+new Vector3(i*.9f,.23f,0),new Vector3(.12f,.46f,.4f),metal);}
        static void Capture(Camera cam,string path)
        {var rt=new RenderTexture(1600,1000,24);cam.targetTexture=rt;cam.Render();var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(1600,1000,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1600,1000),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);}
    }
}
#endif
