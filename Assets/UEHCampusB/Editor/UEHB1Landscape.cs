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
        private static Mesh landscapeLeaf;
        private static float LR(System.Random random,float a,float b) => a+(b-a)*(float)random.NextDouble();

        private static void EnsureB1LandscapeMaterials()
        {
            MakeB1LandscapeTexture("B1_YardGranite",0);
            MakeB1LandscapeTexture("B1_SidewalkPattern",1);
            MakeB1LandscapeTexture("B1_PlanterGranite",2);
            var yard=Mat("B1YardGranite","FFFFFF",.12f,0,"B1_YardGranite");yard.mainTextureScale=Vector2.one/0.8f;
            var walk=Mat("B1SidewalkTile","FFFFFF",.08f,0,"B1_SidewalkPattern");walk.mainTextureScale=Vector2.one/0.4f;
            var red=Mat("B1SidewalkRed","BD8C83",.08f,0,"B1_SidewalkPattern");red.mainTextureScale=Vector2.one/0.4f;
            var stone=Mat("B1PlanterGranite","FFFFFF",.14f,0,"B1_PlanterGranite");stone.mainTextureScale=Vector2.one*1.8f;
            foreach(var m in new[]{yard,walk,red,stone})
            {
                string texture=m==yard?"B1_YardGranite":m==stone?"B1_PlanterGranite":"B1_SidewalkPattern";
                m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/"+texture+"_Normal.png"));
                m.SetFloat("_BumpScale",m==stone?.18f:.35f);m.EnableKeyword("_NORMALMAP");EditorUtility.SetDirty(m);
            }
            Mat("B1Soil","393329",.02f);Mat("B1PotDark","535856",.12f);Mat("B1Curb","B6B7AC",.10f);
            Mat("B1Bark","675044",.10f);Mat("B1BarkLight","85705B",.10f);Mat("B1TreeStake","A58D6D",.14f);
            Mat("B1LeafDark","285334",.12f);Mat("B1LeafMid","467641",.14f);Mat("B1LeafLight","789345",.16f);
            Mat("B1GroundCover","476C31",.10f);Mat("B1SnakeGreen","3E6941",.16f);Mat("B1SnakeYellow","B6B45C",.14f);
            Mat("B1ShrubFlower","C9667D",.15f);Mat("B1Drain","484B49",.10f,.25f);
        }

        private static void MakeB1LandscapeTexture(string name,int kind)
        {
            string path=Root+"/Textures/"+name+".png";
            if(File.Exists(path)) return;
            int size=kind==2?512:1024;
            var texture=new Texture2D(size,size,TextureFormat.RGB24,false);
            var height=new Texture2D(size,size,TextureFormat.RGB24,false);
            var colors=new Color[size*size];var heights=new Color[size*size];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            {
                float u=x/(float)size,v=y/(float)size;
                uint hash=(uint)(x*73856093^y*19349663);hash^=hash>>13;hash*=1274126177;
                float grain=(hash&1023)/1023f;
                float n=Mathf.PerlinNoise(u*8+17,v*8+9);
                bool groove=false;
                Color color;
                float h;
                if(kind==0)
                {
                    groove=x<7 || y<7;
                    color=new Color(.61f,.59f,.58f)*(0.94f+n*.10f+(grain-.5f)*.10f);
                    if(groove)color*=.66f;
                    h=groove?.14f:.65f+grain*.08f;
                }
                else if(kind==1)
                {
                    float a=Mathf.Abs(Vector2.Distance(new Vector2(u,v),new Vector2(.26f,.26f))-.225f);
                    float b=Mathf.Abs(Vector2.Distance(new Vector2(u,v),new Vector2(.74f,.74f))-.225f);
                    float c=Mathf.Abs(Vector2.Distance(new Vector2(u,v),new Vector2(.26f,.26f))-.325f);
                    float d=Mathf.Abs(Vector2.Distance(new Vector2(u,v),new Vector2(.74f,.74f))-.325f);
                    bool edge=x<10 || y<10;
                    groove=Mathf.Min(a,b,c,d)<.0055f || edge;
                    color=new Color(.54f,.59f,.57f)*(.91f+n*.15f+(grain-.5f)*.15f);
                    if(groove)color*=edge?.63f:.73f;
                    h=groove?.16f:.63f+grain*.15f;
                }
                else
                {
                    float chip=grain>.88f?.22f:grain<.14f?-.24f:(grain-.5f)*.18f;
                    color=new Color(.52f+chip,.55f+chip,.55f+chip)*(0.94f+n*.1f);
                    h=.5f+grain*.3f;
                }
                colors[y*size+x]=color;heights[y*size+x]=new Color(h,h,h);
            }
            texture.SetPixels(colors);texture.Apply();height.SetPixels(heights);height.Apply();
            File.WriteAllBytes(path,texture.EncodeToPNG());string normal=Root+"/Textures/"+name+"_Normal.png";File.WriteAllBytes(normal,height.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(height);
            AssetDatabase.ImportAsset(path);AssetDatabase.ImportAsset(normal);
            foreach(string file in new[]{path,normal})
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(file);importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=16;importer.maxTextureSize=1024;
                if(file==normal){importer.textureType=TextureImporterType.NormalMap;importer.convertToNormalmap=true;importer.heightmapScale=.12f;}
                importer.SaveAndReimport();
            }
        }

        private static void BuildB1ForecourtPaving(Transform parent)
        {
            EnsureB1LandscapeMaterials();
            var p=new Part("01_Forecourt_and_pavement",parent,"B1Landscape");
            p.Box("B1YardGranite",new Vector3(0,-.13f,4),new Vector3(62,.30f,9),true);
            p.Box("B1SidewalkTile",new Vector3(0,-.13f,12.5f),new Vector3(62,.30f,8),true);
            // A narrow dark sill marks the change in paving at the campus boundary.
            p.Box("B1PotDark",new Vector3(0,.022f,8.45f),new Vector3(62,.035f,.16f));
            foreach(int side in new[]{-1,1})
                p.Box("B1SidewalkRed",new Vector3(side*4.7f,.023f,15.65f),new Vector3(1.5f,.025f,1.5f));
            p.Box("Asphalt",new Vector3(0,-.23f,23),new Vector3(75,.16f,13),true);
            p.Box("B1Curb",new Vector3(0,-.02f,16.55f),new Vector3(62,.27f,.22f));
            for(int i=-6;i<=6;i++)p.Box("White",new Vector3(i*5,-.139f,23),new Vector3(2.7f,.012f,.13f));
            for(int i=0;i<7;i++)p.Box("White",new Vector3(0,-.139f,17.8f+i*.95f),new Vector3(4.6f,.012f,.45f));
            foreach(int side in new[]{-1,1})
                for(float x=6;x<30;x+=6)
                {
                    p.Box("B1Drain",new Vector3(side*x,.032f,8.45f),new Vector3(.65f,.018f,.22f));
                    for(int i=0;i<9;i++)p.Box("Metal",new Vector3(side*x-.28f+i*.07f,.045f,8.45f),new Vector3(.018f,.012f,.22f));
                }
            p.Finish();
        }

        private static void BuildB1CampusLandscape(Transform parent)
        {
            EnsureB1LandscapeMaterials();
            var pots=new Part("08_Retracted_gate_and_planters",parent,"B1Landscape");
            var plants=new Part("09_Reference_trees_and_street_planting",parent,"B1Landscape");
            var random=new System.Random(27940);
            foreach(int side in new[]{-1,1})
            {
                foreach(float distance in new[]{7.5f,14f,21f})
                {
                    Vector3 center=new Vector3(side*distance,0,8.15f);
                    LandscapePlanter(pots,center,1.55f,1.55f,1.05f,"B1PlanterGranite");
                    LandscapeTree(plants,center+Vector3.up*1.04f,6.1f+LR(random,0,1),random,true);
                }
                foreach(float distance in new[]{10.75f,17.5f})
                {
                    Vector3 center=new Vector3(side*distance,0,8.15f);
                    float shrubWidth=distance<12?4.75f:5.15f;
                    LandscapePlanter(pots,center,shrubWidth,1.25f,.9f,"B1PotDark");
                    LandscapeHedge(plants,center+Vector3.up*.91f,shrubWidth-.10f,1.12f,.60f,random,true);
                }
                // Keep the central nine-metre entry clear, with compact retracted gate stacks beside it.
                float gx=side*4.75f;
                pots.Box("B1PlanterGranite",new Vector3(gx,.58f,8.2f),new Vector3(.52f,1.16f,.75f),true);
                pots.Box("B1Curb",new Vector3(gx,1.18f,8.2f),new Vector3(.60f,.06f,.83f));
                for(int i=0;i<13;i++)
                {
                    float x=gx+side*(.36f+i*.085f);
                    pots.Box("Silver",new Vector3(x,1.02f,8.2f),new Vector3(.035f,1.94f,.54f));
                    pots.Box("Metal",new Vector3(x,.09f,8.2f),new Vector3(.11f,.16f,.57f));
                }
                pots.Box("Silver",new Vector3(gx+side*1.5f,.96f,8.2f),new Vector3(.5f,1.92f,.66f),true);
                pots.Box("Dark",new Vector3(gx+side*1.5f,1.39f,8.55f),new Vector3(.37f,.52f,.025f));
                // Low planted street islands separated from the higher stone pots at the campus gate.
                float cx=side*17.4f;float length=23.2f;
                Vector3 bed=new Vector3(cx,0,15.12f);
                foreach(float z in new[]{14.08f,16.16f}) pots.Box("B1Curb",new Vector3(cx,.12f,z),new Vector3(length+.25f,.25f,.16f));
                foreach(float x in new[]{cx-length/2,cx+length/2}) pots.Box("B1Curb",new Vector3(x,.12f,15.12f),new Vector3(.16f,.25f,2.1f));
                pots.Box("B1Soil",bed+Vector3.up*.10f,new Vector3(length,.21f,1.98f));
                LandscapeHedge(plants,bed+Vector3.up*.20f,length,1.85f,.23f,random,false);
                foreach(float distance in new[]{9f,25.4f})
                    LandscapeTree(plants,new Vector3(side*distance,.24f,15.15f),7.9f+LR(random,0,1),random,false);
                foreach(float distance in new[]{13f,20f}) LandscapePalm(plants,new Vector3(side*distance,.32f,15.15f),random);
                for(float distance=6.8f;distance<28.5f;distance+=.50f)
                    LandscapeSnakePlant(plants,new Vector3(side*distance,.35f,15.25f+LR(random,-.25f,.25f)),random);
            }
            pots.Finish();plants.Finish();
        }

        private static void LandscapePlanter(Part p,Vector3 c,float width,float depth,float height,string material)
        {
            foreach(int side in new[]{-1,1})
            {
                p.Box(material,c+new Vector3(side*(width/2-.075f),height/2,0),new Vector3(.15f,height,depth),true);
                p.Box(material,c+new Vector3(0,height/2,side*(depth/2-.075f)),new Vector3(width-.30f,height,.15f),true);
            }
            p.Box("B1Soil",c+Vector3.up*(height-.08f),new Vector3(width-.24f,.08f,depth-.24f));
            foreach(int side in new[]{-1,1})
            {
                p.Box(material,c+new Vector3(side*(width/2-.055f),height+.025f,0),new Vector3(.18f,.075f,depth+.06f));
                p.Box(material,c+new Vector3(0,height+.025f,side*(depth/2-.055f)),new Vector3(width+.06f,.075f,.18f));
            }
        }

        private static Mesh LandscapeLeafMesh()
        {
            if(landscapeLeaf!=null)return landscapeLeaf;
            var vertices=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();
            Vector3[] rim={new Vector3(0,.5f,0),new Vector3(-.36f,.18f,0),new Vector3(-.27f,-.25f,0),new Vector3(0,-.5f,0),new Vector3(.27f,-.25f,0),new Vector3(.36f,.18f,0)};
            vertices.Add(new Vector3(0,0,.055f));vertices.AddRange(rim);
            uv.Add(new Vector2(.5f,.5f));foreach(var v in rim)uv.Add(new Vector2(v.x+.5f,v.y+.5f));
            vertices.AddRange(vertices.ToArray());uv.AddRange(uv.ToArray());
            for(int i=0;i<6;i++)
            {
                int next=(i+1)%6;triangles.AddRange(new[]{0,i+1,next+1,7,next+8,i+8});
            }
            landscapeLeaf=new Mesh{name="Folded_elliptical_leaf"};landscapeLeaf.SetVertices(vertices);landscapeLeaf.SetUVs(0,uv);landscapeLeaf.SetTriangles(triangles,0);landscapeLeaf.RecalculateNormals();landscapeLeaf.RecalculateTangents();return landscapeLeaf;
        }

        private static void LandscapeLeaf(Part p,string mat,Vector3 center,Vector3 direction,float width,float length,float roll=0)
        {
            var q=Quaternion.FromToRotation(Vector3.up,direction.normalized)*Quaternion.AngleAxis(roll,Vector3.up);
            p.AppendMesh(mat,LandscapeLeafMesh(),Matrix4x4.TRS(center,q,new Vector3(width,length,Mathf.Min(width,length))));
        }

        private static void LandscapeStem(Part p,string mat,Vector3 a,Vector3 b,float radius,float endRadius)
        {
            const int sides=9;var vertices=new Vector3[sides*2];var uv=new Vector2[sides*2];var tri=new List<int>();
            var q=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);
            for(int i=0;i<sides;i++)
            {
                float angle=i*Mathf.PI*2/sides;var radial=q*new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                vertices[i]=a+radial*radius;vertices[i+sides]=b+radial*endRadius;uv[i]=new Vector2(i/(float)sides,0);uv[i+sides]=new Vector2(i/(float)sides,Vector3.Distance(a,b));
                int j=(i+1)%sides;tri.AddRange(new[]{i,i+sides,j+sides,i,j+sides,j});
            }
            var mesh=new Mesh();mesh.vertices=vertices;mesh.uv=uv;mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateTangents();p.AppendMesh(mat,mesh,Matrix4x4.identity);UnityEngine.Object.DestroyImmediate(mesh);
        }

        private static void LandscapeTree(Part p,Vector3 root,float height,System.Random random,bool staked)
        {
            Vector3 trunk=root+new Vector3(.12f,height*.64f,.08f);
            LandscapeStem(p,"B1Bark",root,root+new Vector3(.08f,height*.33f,-.03f),.15f,.11f);
            LandscapeStem(p,"B1BarkLight",root+new Vector3(.08f,height*.33f,-.03f),trunk,.11f,.055f);
            for(int branch=0;branch<11;branch++)
            {
                float angle=branch*2.399f;float radius=height*LR(random,.13f,.25f);
                Vector3 start=root+new Vector3(.08f,height*LR(random,.39f,.62f),0);
                Vector3 end=root+new Vector3(Mathf.Cos(angle)*radius,height*LR(random,.66f,.90f),Mathf.Sin(angle)*radius);
                LandscapeStem(p,"B1Bark",start,end,.055f,.018f);
                for(int twig=0;twig<2;twig++)
                {
                    Vector3 tip=end+new Vector3(LR(random,-.55f,.55f),LR(random,.1f,.65f),LR(random,-.55f,.55f));
                    LandscapeStem(p,"B1BarkLight",end,tip,.018f,.006f);
                    for(int leaf=0;leaf<66;leaf++)
                    {
                        Vector3 offset=new Vector3(LR(random,-.85f,.85f),LR(random,-.5f,.55f),LR(random,-.85f,.85f));
                        if(offset.sqrMagnitude>1)offset*=.75f;
                        Vector3 dir=new Vector3(LR(random,-1,1),LR(random,-.8f,1),LR(random,-1,1));
                        LandscapeLeaf(p,leaf%7==0?"B1LeafLight":leaf%3==0?"B1LeafDark":"B1LeafMid",tip+offset,dir,LR(random,.13f,.23f),LR(random,.22f,.36f),LR(random,0,360));
                    }
                }
            }
            if(staked)
            {
                for(int i=0;i<3;i++)
                {
                    float a=i*Mathf.PI*2/3;
                    LandscapeStem(p,"B1TreeStake",root+new Vector3(Mathf.Cos(a)*.62f,.02f,Mathf.Sin(a)*.62f),root+new Vector3(0,2.1f,0),.028f,.028f);
                }
                p.Box("Metal",root+Vector3.up*2.1f,new Vector3(.28f,.10f,.26f));
            }
        }

        private static void LandscapeHedge(Part p,Vector3 center,float width,float depth,float height,System.Random random,bool flowers)
        {
            if(!flowers)p.Box("B1GroundCover",center+Vector3.up*height*.15f,new Vector3(width,height*.30f,depth));
            int count=Mathf.RoundToInt(width*depth*(flowers?210:125));
            for(int i=0;i<count;i++)
            {
                Vector3 pos=center+new Vector3(LR(random,-width/2,width/2),LR(random,height*.15f,height),LR(random,-depth/2,depth/2));
                Vector3 dir=new Vector3(LR(random,-1,1),LR(random,.1f,1),LR(random,-1,1));
                LandscapeLeaf(p,i%5==0?"B1LeafLight":i%3==0?"B1LeafDark":"B1LeafMid",pos,dir,flowers?.13f:.08f,flowers?.20f:.12f,LR(random,0,360));
                if(flowers && i%53==0)
                    for(int petal=0;petal<5;petal++)
                        LandscapeLeaf(p,"B1ShrubFlower",pos+new Vector3(Mathf.Cos(petal*1.256f)*.045f,.05f,Mathf.Sin(petal*1.256f)*.045f),Vector3.up,.07f,.09f,petal*72);
            }
        }

        private static void LandscapeSnakePlant(Part p,Vector3 root,System.Random random)
        {
            for(int i=0;i<7;i++)
            {
                float a=i*2.399f;float h=LR(random,.5f,.95f);
                Vector3 dir=new Vector3(Mathf.Cos(a)*.2f,1,Mathf.Sin(a)*.2f).normalized;
                Vector3 center=root+dir*h*.5f;
                LandscapeLeaf(p,"B1SnakeYellow",center,dir,.13f,h,i*51);
                Vector3 normal=(Quaternion.FromToRotation(Vector3.up,dir)*Quaternion.AngleAxis(i*51,Vector3.up))*Vector3.forward;
                foreach(int side in new[]{-1,1})LandscapeLeaf(p,"B1SnakeGreen",center+normal*.002f*side,dir,.116f,h*.98f,i*51);
            }
        }

        private static void LandscapePalm(Part p,Vector3 root,System.Random random)
        {
            for(int cane=0;cane<3;cane++)
            {
                Vector3 top=root+new Vector3(LR(random,-.25f,.25f),LR(random,1.3f,1.9f),LR(random,-.25f,.25f));
                LandscapeStem(p,"B1BarkLight",root,top,.025f,.018f);
                for(int frond=0;frond<9;frond++)
                {
                    float a=frond*Mathf.PI*2/9+cane;Vector3 radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                    Vector3 previous=top;
                    for(int segment=1;segment<=12;segment++)
                    {
                        float t=segment/12f;Vector3 at=top+radial*(t*1.15f)+Vector3.up*(Mathf.Sin(t*Mathf.PI)*.32f-t*.40f);
                        LandscapeStem(p,"B1LeafMid",previous,at,.009f,.005f);previous=at;
                        Vector3 perpendicular=new Vector3(-radial.z,0,radial.x);
                        foreach(int side in new[]{-1,1})
                        {
                            Vector3 dir=(perpendicular*side*.7f+radial*.35f+Vector3.down*.25f).normalized;
                            float length=.40f*(1-t*.6f);
                            LandscapeLeaf(p,"B1LeafLight",at+dir*length*.45f,dir,.055f,length,20);
                        }
                    }
                }
            }
        }

        [MenuItem("UEH/Campus B/40. Refine forecourt paving and reference planting")]
        public static void RefineB1LandscapeAndCapture()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Exit Play Mode first.");
            if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            dimensions=AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root+"/Settings/EstimatedDimensions.asset");
            materials.Clear();foreach(string guid in AssetDatabase.FindAssets("t:Material",new[]{Root+"/Materials"}))
            {var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));materials[m.name]=m;}
            EnsureB1LandscapeMaterials();
            string path=Root+"/Prefabs/UEH_Exterior.prefab";var contents=PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach(Transform child in new List<Transform>(GetDirectChildren(contents.transform)))
                    if(child.name=="01_Forecourt_and_pavement"||child.name=="08_Retracted_gate_and_planters"||child.name.StartsWith("09_Tree_")||child.name=="09_Reference_trees_and_street_planting")UnityEngine.Object.DestroyImmediate(child.gameObject);
                BuildB1ForecourtPaving(contents.transform);BuildB1CampusLandscape(contents.transform);
                var paving=contents.transform.Find("18_B1_structural_frame/04_Side_and_rear_paving");
                if(paving!=null)foreach(var r in paving.GetComponentsInChildren<MeshRenderer>())if(r.sharedMaterial!=null&&r.sharedMaterial.name=="Pavement")r.sharedMaterial=materials["B1YardGranite"];
                PrefabUtility.SaveAsPrefabAsset(contents,path);
            }
            finally{PrefabUtility.UnloadPrefabContents(contents);}
            AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            SetupB1LandscapeViewpoints(GameObject.Find("Review viewpoints [select and align view]").transform);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());CaptureB1LandscapePreviews();
            Directory.CreateDirectory(Path.GetFullPath("Docs/UEH-CampusB"));
            File.WriteAllText(Path.GetFullPath("Docs/UEH-CampusB/b1-landscape-layout.json"),JsonUtility.ToJson(new B1LandscapeLayout(),true));
            Debug.Log("UEH_B1_LANDSCAPE_COMPLETE Distinct 0.8 m granite yard and 0.4 m patterned sidewalk; speckled stone planters, branched leafy trees, shrubs, snake plants and feather palms built from supplied reference images.");
        }

        [Serializable]private sealed class B1LandscapeLayout
        {
            public string status="Forecourt and street planting reconstructed from supplied Virtual Tour images; dimensions and plant identities are visual estimates.";
            public float yardTileMetres=.8f,sidewalkTileMetres=.4f,pavingBoundaryZ=8.5f,streetCurbZ=16.55f,gateClearWidth=9f;
            public int stoneTreePlanters=6,darkShrubPlanters=4,streetPlantingBeds=2,forecourtTrees=6,streetTrees=4,palmClumps=4;
            public string[] vegetation={"Broadleaf branching trees with individual folded leaves","Low leafy shrub borders","Yellow-edged upright snake plants","Pinnate palm fronds"};
        }

        private static void SetupB1LandscapeViewpoints(Transform views)
        {
            foreach(Transform child in new List<Transform>(GetDirectChildren(views)))
            {int n;if(child.name.Length>3&&int.TryParse(child.name.Substring(0,3),out n)&&n>=133&&n<=137)UnityEngine.Object.DestroyImmediate(child.gameObject);}
            Viewpoint(views,"133_B1_reference_forecourt_overview",new Vector3(29,9,27),new Vector3(2,2.6f,6));
            Viewpoint(views,"134_B1_two_distinct_paving_types",new Vector3(1.8f,2.8f,12),new Vector3(1.8f,.1f,7.7f));
            Viewpoint(views,"135_B1_sidewalk_planted_street_strip",new Vector3(-3.8f,1.7f,12.1f),new Vector3(-21,2,13.1f));
            Viewpoint(views,"136_B1_granite_tree_planters_and_shrubs",new Vector3(3.6f,1.8f,13),new Vector3(14,2.9f,8));
            Viewpoint(views,"137_B1_snake_plants_and_feather_palms",new Vector3(-6.5f,1.5f,12.7f),new Vector3(-12.8f,1.2f,15.15f));
        }

        private static void CaptureB1LandscapePreviews()
        {
            string output=Path.GetFullPath("Docs/UEH-CampusB/Previews");Directory.CreateDirectory(output);
            foreach(Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                int n;if(point.name.Length<4||!int.TryParse(point.name.Substring(0,3),out n)||n<133||n>137)continue;
                var go=new GameObject("Temporary landscape review camera");var camera=go.AddComponent<Camera>();camera.enabled=false;
                camera.transform.SetPositionAndRotation(point.position,point.rotation);camera.fieldOfView=n==134?66:72;camera.nearClipPlane=.05f;camera.farClipPlane=220;
                var target=new RenderTexture(1800,1100,24,RenderTextureFormat.ARGB32){antiAliasing=4};var prior=RenderTexture.active;
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;var texture=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
                texture.ReadPixels(new Rect(0,0,target.width,target.height),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(output,point.name+".png"),texture.EncodeToPNG());
                camera.targetTexture=null;RenderTexture.active=prior;target.Release();UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
#endif
