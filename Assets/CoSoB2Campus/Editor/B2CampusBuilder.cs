#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UEH.B2Campus.Editor
{
    public static partial class B2CampusBuilder
    {
        public const string Root="Assets/CoSoB2Campus";
        public const string ScenePath=Root+"/Scenes/CoSoB2_Campus.unity";
        const float H=4.2f;
        static Transform campus;
        static Font font;
        static Material white,cream,stone,tile,wood,blue,ink,glass,orange,green,red,screen,labelMaterial;
        static Mesh cube;
        static int meshNumber;
        static readonly List<B2CampusRoom> rooms=new List<B2CampusRoom>();

        [MenuItem("UEH/CoSoB2/Open four-wing campus")]
        public static void Open()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode){Debug.Log("Exit Play mode to switch scenes.");return;}
            if(!File.Exists(ScenePath)){Build();return;}
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        }
        public static void Build()
        {
            if(File.Exists(ScenePath) || File.Exists(Root+"/Prefabs/CoSoB2.prefab"))
            {if(!Application.isBatchMode)Open();else Debug.Log("Existing campus retained.");return;}
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Exit Play mode first.");
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            foreach(string p in new[]{"Scenes","Prefabs","Materials","Meshes"})Directory.CreateDirectory(Root+"/"+p);
            AssetDatabase.Refresh();rooms.Clear();meshNumber=0;
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            white=Mat("Ivory",new Color(.86f,.87f,.81f));cream=Mat("Old_yellow_plaster",new Color(.82f,.73f,.48f));
            stone=Mat("Grey_stone",new Color(.32f,.36f,.37f));tile=Mat("Light_floor_tiles",new Color(.65f,.67f,.64f));
            wood=Mat("Warm_wood",new Color(.43f,.21f,.09f));blue=Mat("Study_privacy_blue",new Color(.22f,.58f,.69f));
            ink=Mat("Graphite_metal",new Color(.07f,.10f,.12f));orange=Mat("Bridge_orange",new Color(.93f,.32f,.065f));
            green=Mat("Courtyard_green",new Color(.16f,.32f,.11f));red=Mat("Service_red",new Color(.65f,.10f,.10f));
            screen=Mat("Display_blue",new Color(.12f,.42f,.64f));glass=Mat("Bridge_glass",new Color(.36f,.67f,.72f,.24f));
            ReferenceMaterials();
            glass.SetFloat("_Mode",2);glass.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);glass.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);glass.SetInt("_ZWrite",0);glass.EnableKeyword("_ALPHABLEND_ON");glass.renderQueue=3000;
            font=AssetDatabase.LoadAssetAtPath<Font>(Root+"/Fonts/LiberationSans.ttf");
            labelMaterial=new Material(Shader.Find("UEH/B2Campus/DepthText"));labelMaterial.mainTexture=font.material.mainTexture;
            AssetDatabase.CreateAsset(labelMaterial,Root+"/Materials/DepthText.mat");
            var unit=GameObject.CreatePrimitive(PrimitiveType.Cube);cube=unit.GetComponent<MeshFilter>().sharedMesh;UnityEngine.Object.DestroyImmediate(unit);
            campus=new GameObject("CoSoB2").transform;
            var left=Group("01_DayTrai_Tret_va_Tang1_6",campus);
            var right=Group("02_DayPhai_Tret_va_Tang1_6",campus);
            var rear=Group("03_DayGiua_Tret_va_Tang1_6",campus);
            var old=Group("04_KhoiNoiVangCu_Tret_Tang2_Tang3",campus);
            for(int level=0;level<=6;level++)
            {
                Side(left,0,level);Side(right,1,level);Rear(rear,level);
            }
            for(int level=0;level<3;level++)Old(old,level);
            Roof(left,0,new Vector3(-23,7*H,5),new Vector3(14.2f,.23f,46.5f),7);
            Roof(right,1,new Vector3(23,7*H,5),new Vector3(14.2f,.23f,46.5f),7);
            Roof(rear,2,new Vector3(0,7*H,32),new Vector3(60.5f,.23f,12.5f),7);
            Roof(old,3,new Vector3(0,3*H,-25.5f),new Vector3(60.5f,.23f,15.5f),3);
            Bridge(old);Site();EntranceStair(right);
            // User requested no lettering anywhere in the B2 model, including hidden plan labels.
            foreach(var room in rooms)room.planLabel=null;
            foreach(var text in campus.GetComponentsInChildren<TextMesh>(true))
                UnityEngine.Object.DestroyImmediate(text.gameObject);
            var atlas=campus.gameObject.AddComponent<B2TextAtlas>();atlas.font=font;atlas.textMaterial=labelMaterial;
            foreach(var t in campus.GetComponentsInChildren<TextMesh>(true))font.RequestCharactersInTexture(t.text,t.fontSize,t.fontStyle);
            labelMaterial.mainTexture=font.material.mainTexture;
            AssetDatabase.SaveAssets();
            PrefabUtility.SaveAsPrefabAsset(campus.gameObject,Root+"/Prefabs/CoSoB2.prefab");
            UnityEngine.Object.DestroyImmediate(campus.gameObject);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/CoSoB2.prefab"));
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.57f,.61f,.67f);
            var sun=new GameObject("Daylight").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.05f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(48,-28,0);
            var fill=new GameObject("Courtyard fill light").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=.35f;fill.transform.rotation=Quaternion.Euler(55,145,0);
            QualitySettings.shadowDistance=180;QualitySettings.antiAliasing=4;
            var camera=new GameObject("Campus inspection camera").AddComponent<Camera>();camera.tag="MainCamera";camera.nearClipPlane=.08f;camera.farClipPlane=400;camera.fieldOfView=55;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.58f,.70f,.78f);
            var view=camera.gameObject.AddComponent<B2CampusViewer>();view.campus=instance.transform;view.font=font;view.floors=instance.GetComponentsInChildren<B2CampusFloor>(true);view.rooms=instance.GetComponentsInChildren<B2CampusRoom>(true);view.Overview();
            EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
            WriteSchedule(view.rooms);
            Directory.CreateDirectory("Docs/CoSoB2Campus/Previews");
            Capture(camera,"00_Overview");view.FloorPlan(0);Capture(camera,"01_Ground_plan");view.FloorPlan(1);Capture(camera,"02_First_level");view.FloorPlan(5);Capture(camera,"03_Computer_floor");view.Bridge();Capture(camera,"04_Glass_bridge");
            foreach(var r in view.rooms)if(r.roomId=="OLD-1-L"){view.FocusRoom(r);Capture(camera,"05_Old_study_room");break;}
            foreach(var r in view.rooms)if(r.roomId=="R-5-1"){view.FocusRoom(r);Capture(camera,"06_Computer_room");break;}
            view.Courtyard();Capture(camera,"07_Courtyard");
            foreach(var r in view.rooms)if(r.roomId=="R-0-3"){view.FocusRoom(r);camera.transform.position=r.transform.TransformPoint(new Vector3(0,2,-r.size.z/2-2.5f));camera.transform.LookAt(r.transform.TransformPoint(new Vector3(0,1.9f,0)));camera.fieldOfView=85;Capture(camera,"08_MakerSpace_reference");break;}
            foreach(var r in view.rooms)if(r.roomId=="R-0-2"){view.FocusRoom(r);camera.transform.position=r.transform.TransformPoint(new Vector3(-3,2,-r.size.z/2+.6f));camera.transform.LookAt(r.transform.TransformPoint(new Vector3(0,1.1f,1)));camera.fieldOfView=85;Capture(camera,"09_Canteen_reference");break;}
            camera.fieldOfView=55;
            foreach(var r in view.rooms)if(r.roomId=="L-2-1"){view.FocusRoom(r);Capture(camera,"10_Classroom_reference");break;}
            view.Overview();
            camera.transform.position=new Vector3(-3,4,-5);camera.transform.LookAt(new Vector3(12,2.6f,5));Capture(camera,"11_Right_entrance_stair");
            camera.transform.position=new Vector3(4,3,14);camera.transform.LookAt(new Vector3(8,.3f,11));Capture(camera,"12_Turtle_pond");
            view.JunctionCore();Capture(camera,"13_Junction_stairs_lifts");
            view.FloorPlan(0);camera.orthographicSize=10;camera.transform.position=new Vector3(23,40,32);camera.transform.rotation=Quaternion.Euler(90,0,0);Capture(camera,"14_Junction_floorplan");
            view.LeftStair();Capture(camera,"15_Left_stair_approved");
            view.FloorPlan(0);camera.orthographicSize=8;camera.transform.position=new Vector3(-22,40,33);camera.transform.rotation=Quaternion.Euler(90,0,0);Capture(camera,"16_Left_stair_exit_plan");
            view.FloorPlan(1);camera.orthographicSize=20;camera.transform.position=new Vector3(0,60,24);camera.transform.rotation=Quaternion.Euler(90,0,0);Capture(camera,"17_Rear_end_classrooms");
            view.Overview();
            EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
            Debug.Log("B2_CAMPUS_BUILD_COMPLETE rooms="+view.rooms.Length+" scene="+ScenePath);
        }
        static Transform Floor(Transform parent,int wing,int level)
        {var g=Group("Level_"+level,parent);var f=g.gameObject.AddComponent<B2CampusFloor>();f.wing=wing;f.level=level;g.localPosition=Vector3.up*(level*H);return g;}
        static void Roof(Transform parent,int wing,Vector3 pos,Vector3 size,int level)
        {var g=Group("Roof_EST",parent);var f=g.gameObject.AddComponent<B2CampusFloor>();f.wing=wing;f.level=level;f.roof=true;Box("Roof",g,pos,size,white);}
        static void Side(Transform parent,int wing,int level)
        {
            float sign=wing==0?-1:1;float x=sign*24;
            var floor=Floor(parent,wing,level);
            Box("Floor_slab",floor,new Vector3(sign*23,-.12f,5),new Vector3(14,.24f,46),tile);
            Box("Corridor",floor,new Vector3(sign*17.5f,.015f,5),new Vector3(3,.035f,46),stone);
            if(wing==1 && level==1)
            {
                Rail(floor,new Vector3(16,0,-8.75f),18.5f,true,true);
                Rail(floor,new Vector3(16,0,18.75f),18.5f,true,true);
            }
            else Rail(floor,new Vector3(sign*16,0,5),46,true,level>0);
            for(int i=0;i<=10;i++){if(wing==1 && level<=1 && i==5)continue;Box("Corridor_column",floor,new Vector3(sign*16.1f,2.1f,-18+i*4.6f),new Vector3(.35f,4.2f,.35f),white);}
            if(level==0)
            {
                if(wing==0)
                {
                    Room(floor,"L-0-1","Văn phòng phẩm & sách 1",wing,level,new Vector3(x,0,-8),20,10,-90,"shop",0);
                    Room(floor,"L-0-2","Văn phòng phẩm & sách 2",wing,level,new Vector3(x,0,12),20,10,-90,"shop",0);
                }
                else
                {
                    Room(floor,"R-0-1","Phòng y tế",wing,level,new Vector3(x,0,-15),6,10,90,"medical",0);
                    Room(floor,"R-0-2","Căn tin lớn",wing,level,new Vector3(x,0,-4),16,10,90,"canteen",64);
                    Room(floor,"R-0-3","UEH Maker Space",wing,level,new Vector3(x,0,13),18,10,90,"maker",24);
                }
            }
            else
            {
                int n=level==1?3:5;float width=40f/n;
                for(int i=0;i<n;i++)
                {
                    // Fill the empty end bay up to the cross corridor at z=26.
                    // Keep all preceding classrooms and the stair/lift approach unchanged.
                    float extension=i==n-1?4f:0f;
                    Room(floor,(wing==0?"L":"R")+"-"+level+"-"+(i+1),(level==5?"Phòng máy ":"Phòng học ")+(i+1),wing,level,new Vector3(x,0,-18+width*(i+.5f)+extension/2),width+extension,10,sign*90,level==5?"computer":"classroom",level==1?80:40);
                }
            }
            if(wing==0)
            {
                if(level<6)
                {
                    var stairs=Group("Left_stair_approved_beside_parking",floor);
                    stairs.localPosition=new Vector3(-19.7f,0,36.5f);
                    stairs.localRotation=Quaternion.Euler(0,90,0);
                    RefinedStair(stairs,2.6f,7);
                }
            }
            else
            {
                var core=Group("Core_2_lifts_shared_stair_lobby",floor);core.localPosition=new Vector3(27.075f,0,36.12f);core.localRotation=Quaternion.identity;
                Box("Right_core_outer_wall",floor,new Vector3(30,2.1f,33),new Vector3(.2f,4.2f,10),white);
                Box("Right_core_back_wall",floor,new Vector3(23,2.1f,38),new Vector3(14,4.2f,.2f),white);
                if(level<6)
                {
                    var stairs=Group("Right_rear_junction_stair",floor);stairs.localPosition=new Vector3(18.7f,0,35.6f);stairs.localRotation=Quaternion.identity;
                    RefinedStair(stairs,5.2f,4.4f);
                }
                for(int i=0;i<2;i++)
                {
                    float dx=i==0?-1.4f:1.4f;
                    Box("Lift_shaft_back",core,new Vector3(dx,2,1.7f),new Vector3(2.7f,4,.16f),white);
                    foreach(float sx in new[]{-1.35f,1.35f})Box("Lift_shaft_side",core,new Vector3(dx+sx,2,0),new Vector3(.15f,4,3.5f),white);
                    Box("Static_lift_doors",core,new Vector3(dx,1.25f,-1.8f),new Vector3(2.2f,2.5f,.08f),stone);
                    Box("Door_seam",core,new Vector3(dx,1.25f,-1.85f),new Vector3(.025f,2.5f,.025f),ink);
                    Label("THANG "+(i+1)+" / "+level,core,new Vector3(dx,2.85f,-1.88f),.20f,ink);
                }
            }
        }
        static void Rear(Transform parent,int level)
        {
            var floor=Floor(parent,2,level);
            if(level==0)Box("Floor_slab",floor,new Vector3(0,-.12f,32),new Vector3(60,.24f,12),tile);
            else
            {
                Box("Rear_slab_middle",floor,new Vector3(0,-.12f,32),new Vector3(32.4f,.24f,12),tile);
                Box("Left_stair_outer_landing",floor,new Vector3(-26.6f,-.12f,32),new Vector3(6.8f,.24f,12),tile);
                Box("Left_stair_exit_side_slab",floor,new Vector3(-19.7f,-.12f,30.6f),new Vector3(7,.24f,9.2f),tile);
                Box("Left_stair_back_strip",floor,new Vector3(-19.7f,-.12f,37.9f),new Vector3(7,.24f,.2f),tile);
                Box("Rear_slab_right_of_stair_void",floor,new Vector3(25.7f,-.12f,32),new Vector3(8.6f,.24f,12),tile);
                Box("Rear_slab_front_of_stair_void",floor,new Vector3(18.8f,-.12f,29.7f),new Vector3(5.2f,.24f,7.4f),tile);
                Box("Rear_slab_beyond_stair_void",floor,new Vector3(18.8f,-.12f,37.9f),new Vector3(5.2f,.24f,.2f),tile);
            }
            Box("Cross_corridor",floor,new Vector3(-5.5f,.015f,27),new Vector3(49,.035f,2),stone);
            Box("Cross_corridor_lift_side",floor,new Vector3(27.1f,.015f,27),new Vector3(5.8f,.035f,2),stone);
            Box("Cross_corridor_restored_at_old_core",floor,new Vector3(21.6f,.015f,27),new Vector3(5.2f,.035f,2),stone);
            Rail(floor,new Vector3(-7,0,26),46,false,level>0);
            if(level==0)
            {
                var room=Metadata(floor,"M-0-1","Bãi giữ xe",2,0,new Vector3(0,0,33),32,10,0,"parking",0);
                for(int i=0;i<=6;i++)Box("Parking_column",room.transform,new Vector3(-15+i*5,2.1f,0),new Vector3(.45f,4.2f,.45f),white);
                var boundary=Group("Parking_boundary_walls",room.transform);room.upperWalls=boundary;
                Box("Parking_side_boundary",boundary,new Vector3(15.9f,2.1f,0),new Vector3(.2f,4.2f,10),white);
                foreach(float z in new[]{-3.5f,3.5f})Box("Parking_exit_wall_jamb",boundary,new Vector3(-15.9f,2.1f,z),new Vector3(.2f,4.2f,3),white);
                Box("Parking_exit_header",boundary,new Vector3(-15.9f,3.65f,0),new Vector3(.2f,1.1f,4),white);
                Box("Parking_exit_lane_no_stalls",floor,new Vector3(-23,.02f,33),new Vector3(14,.04f,4),stone);
                foreach(float z in new[]{30.9f,35.1f})Box("Exit_gate_post",floor,new Vector3(-29.7f,1.25f,z),new Vector3(.22f,2.5f,.22f),ink);
                var arm=Box("Exit_barrier_raised",floor,new Vector3(-29.7f,2.1f,35),new Vector3(.08f,2.3f,.09f),white);
                arm.transform.localRotation=Quaternion.Euler(-15,0,0);
                Box("Parking_floor_middle_wing_only",room.transform,new Vector3(0,.015f,0),new Vector3(31.8f,.03f,10),stone);
                var batch=new Parts();
                for(int row=0;row<2;row++)for(int i=0;i<10;i++)
                {
                    float x=-12.6f+i*2.8f,z=row==0?-2.6f:2.5f;
                    batch.Add(new Vector3(x,.45f,z),new Vector3(.55f,.6f,1.5f),i%3==0?red:ink);
                    batch.Add(new Vector3(x,.82f,z+.1f),new Vector3(.48f,.12f,.8f),stone);
                    batch.Add(new Vector3(x,1.05f,z+.6f),new Vector3(.8f,.07f,.08f),ink);
                    batch.Add(new Vector3(x,.1f,z-.6f),new Vector3(.2f,.4f,.4f),ink);
                    batch.Add(new Vector3(x,.1f,z+.6f),new Vector3(.2f,.4f,.4f),ink);
                }
                batch.Save(room.transform,"Parking_scooters_schematic");
                Label("BÃI GIỮ XE",floor,new Vector3(0,3.35f,25.9f),.65f,ink);
            }
            else if(level==1)Room(floor,"M-1-1","Phòng tự học toàn dãy giữa",2,level,new Vector3(0,0,33),32,10,0,"study",160);
            else for(int i=0;i<2;i++)Room(floor,"M-"+level+"-"+(i+1),(level==5?"Phòng máy lớn ":"Phòng học lớn ")+(i+1),2,level,new Vector3(i==0?-8:8,0,33),16,10,0,level==5?"computer":"classroom",80);
        }
        static void Old(Transform parent,int level)
        {
            var floor=Floor(parent,3,level);
            if(level==0)Box("Old_block_floor",floor,new Vector3(0,-.12f,-25.5f),new Vector3(60,.24f,15),tile);
            else
            {
                foreach(float x in new[]{-17f,17f})Box("Old_floor_beside_stair_void",floor,new Vector3(x,-.12f,-25.5f),new Vector3(26,.24f,15),tile);
                Box("Old_front_landing",floor,new Vector3(0,-.12f,-30.75f),new Vector3(8,.24f,4.5f),tile);
                Box("Old_rear_landing",floor,new Vector3(0,-.12f,-18.75f),new Vector3(8,.24f,1.5f),tile);
            }
            if(level==0)
            {
                Room(floor,"OLD-0-L","Tự học lớn bên trái",3,level,new Vector3(-17,0,-24),24,12,0,"study",96);
                Room(floor,"OLD-0-R","Tự học lớn bên phải",3,level,new Vector3(17,0,-24),24,12,0,"study",96);
            }
            else if(level==1)
            {
                Room(floor,"OLD-1-L","Phòng tự học — theo ảnh",3,level,new Vector3(-17,0,-24),24,12,0,"study",96);
                Room(floor,"OLD-1-R","Kho — vị trí tạm bên phải",3,level,new Vector3(17,0,-24),24,12,0,"storage",0);
            }
            else
            {
                Room(floor,"OLD-2-L","Chưa xác định công năng",3,level,new Vector3(-17,0,-24),24,12,0,"unknown",0);
                Room(floor,"OLD-2-R","Chưa xác định công năng",3,level,new Vector3(17,0,-24),24,12,0,"unknown",0);
            }
            if(level<2)Stair(floor,new Vector3(0,0,-24),8,9);
            for(int bay=0;bay<12;bay++)
            {
                float x=-27.5f+bay*5;
                if(level==0)Arch(floor,x,-33);
                else if(!(level==2 && (bay==5 || bay==6)))
                {
                    Box("Old_balcony_parapet",floor,new Vector3(x,.5f,-33),new Vector3(4.5f,1,.25f),cream);
                    Box("Parapet_coping",floor,new Vector3(x,1.04f,-33),new Vector3(4.7f,.1f,.38f),white);
                    Box("Inset_panel",floor,new Vector3(x,.5f,-33.14f),new Vector3(3.4f,.62f,.035f),white);
                }
            }
            for(int i=0;i<=12;i++)
            {
                float x=-30+i*5;if(level==2 && i==6)continue;
                Box("Old_square_pier",floor,new Vector3(x,2.1f,-33),new Vector3(.5f,4.2f,.65f),cream);
                Box("Pier_base",floor,new Vector3(x,.2f,-33.04f),new Vector3(.7f,.4f,.8f),white);
            }
            Box("Cornice",floor,new Vector3(0,4.04f,-33),new Vector3(60.5f,.2f,.95f),white);
        }
        static B2CampusRoom Metadata(Transform parent,string id,string name,int wing,int level,Vector3 pos,float w,float d,float rot,string use,int seats)
        {
            var g=Group(id+"_"+name,parent);g.localPosition=pos;g.localRotation=Quaternion.Euler(0,rot,0);
            var r=g.gameObject.AddComponent<B2CampusRoom>();r.roomId=id;r.displayName=name;r.wing=wing;r.level=level;r.floorLabel=level==0?"Tầng trệt":("Tầng "+(wing==3?level+1:level));r.use=use;r.seats=seats;r.size=new Vector3(w,H,d);rooms.Add(r);return r;
        }
        static void Room(Transform parent,string id,string name,int wing,int level,Vector3 pos,float w,float d,float rot,string use,int seats)
        {
            var r=Metadata(parent,id,name,wing,level,pos,w,d,rot,use,seats);var p=r.transform;var high=Group("UpperWalls_hide_for_floorplan",p);r.upperWalls=high;
            Material wall=wing==3?cream:white;
            Box("Floor",p,new Vector3(0,.01f,0),new Vector3(w-.12f,.06f,d-.12f),tile);
            foreach(float sx in new[]{-w/2,w/2})
            {Box("Low_partition",p,new Vector3(sx,.5f,0),new Vector3(.12f,1,d),wall);Box("Upper_partition",high,new Vector3(sx,2.6f,0),new Vector3(.12f,3.2f,d),wall);}
            // Corridor-facing wall has a real central 1.8 m open doorway.
            float half=(w-1.8f)/2;
            foreach(float sign in new[]{-1f,1f})
            {
                float x=sign*(.9f+half/2);
                WindowWall(p,high,x,-d/2,half,wall);
            }
            Box("Door_header",high,new Vector3(0,3.55f,-d/2),new Vector3(1.8f,1.3f,.16f),wall);
            Label(name,high,new Vector3(0,3.35f,-d/2-.11f),Mathf.Min(.26f,3.8f/Mathf.Max(1,name.Length)),ink);
            int windows=Mathf.Max(1,Mathf.RoundToInt(w/3.2f));
            WindowWall(p,high,0,d/2,w,wall);
            Box("Room_ceiling",high,new Vector3(0,4.1f,0),new Vector3(w-.12f,.12f,d-.12f),white);
            for(int i=0;i<Mathf.Max(2,windows);i++)Box("Ceiling_light",high,new Vector3(-w/2+1+(i+.5f)*(w-2)/Mathf.Max(2,windows),3.98f,0),new Vector3(.65f,.05f,.65f),white);
            var floorText=Label(name+"\n"+(seats>0?seats+" chỗ":""),p,new Vector3(0,.07f,-d*.32f),.15f,ink);floorText.rotation=Quaternion.Euler(90,0,0);r.planLabel=floorText;floorText.gameObject.SetActive(false);
            var b=new Parts();
            if(use=="classroom")ReferenceClassroom(b,high,w,d);
            else if(use=="computer")ReferenceComputerLab(b,p,high,w,d);
            else if(use=="study")Study(b,w,d,seats,true);
            else if(use=="canteen")ReferenceCanteen(b,high,w,d);
            else if(use=="shop" || use=="storage")Shop(b,w,d,use=="shop");
            else if(use=="medical")Medical(b,w,d);
            else if(use=="maker")ReferenceMaker(b,high,w,d);
            b.Save(p,"Furniture_"+use);
        }
        static void Classroom(Parts b,float w,float d,int seats,bool pc)
        {
            int columns=seats==80?10:5,rowsN=Mathf.CeilToInt(seats/(float)columns);float dx=(w-2.4f)/columns,dz=(d-3)/rowsN;
            for(int i=0;i<seats;i++)
            {
                var pos=new Vector3(-w/2+1.2f+dx*(i%columns+.5f),0,d/2-2.3f-dz*(i/columns+.5f));
                Desk(b,pos,pc);Chair(b,pos+new Vector3(0,0,-.65f),0);
            }
            b.Add(new Vector3(0,2.2f,d/2-.14f),new Vector3(Mathf.Min(w-2,5),1.5f,.08f),green);
            b.Add(new Vector3(-2,.8f,d/2-1.2f),new Vector3(1.4f,.12f,.65f),wood);
            b.Add(new Vector3(-2,.4f,d/2-1.2f),new Vector3(.6f,.8f,.4f),stone);
        }
        static void Desk(Parts b,Vector3 p,bool pc)
        {
            b.Add(p+Vector3.up*.75f,new Vector3(.95f,.07f,.58f),wood);
            foreach(float x in new[]{-.4f,.4f})b.Add(p+new Vector3(x,.36f,0),new Vector3(.05f,.72f,.5f),ink);
            if(pc){b.Add(p+new Vector3(0,1.12f,.16f),new Vector3(.58f,.38f,.06f),ink);b.Add(p+new Vector3(0,1.12f,.122f),new Vector3(.51f,.31f,.014f),screen);b.Add(p+new Vector3(0,.85f,.16f),new Vector3(.06f,.2f,.06f),ink);b.Add(p+new Vector3(0,.8f,-.14f),new Vector3(.44f,.025f,.15f),ink);}
        }
        static void Chair(Parts b,Vector3 p,float angle)
        {
            Quaternion q=Quaternion.Euler(0,angle,0);
            b.Add(p+Vector3.up*.44f,new Vector3(.4f,.07f,.4f),wood,q);
            b.Add(p+q*new Vector3(0,.78f,-.18f),new Vector3(.4f,.63f,.055f),wood,q);
            foreach(float x in new[]{-.16f,.16f})foreach(float z in new[]{-.16f,.16f})b.Add(p+q*new Vector3(x,.21f,z),new Vector3(.045f,.42f,.045f),wood,q);
        }
        static void Study(Parts b,float w,float d,int seats,bool privacy)
        {
            int tables=seats/4,cols=Mathf.Min(tables,Mathf.Max(1,Mathf.FloorToInt((w-2)/3.1f))),rowsN=Mathf.CeilToInt(tables/(float)cols);
            for(int i=0;i<tables;i++)
            {
                var p=new Vector3(-w/2+1+(i%cols+.5f)*(w-2)/cols,0,-d/2+1.8f+(i/cols+.5f)*(d-3)/rowsN);
                b.Add(p+Vector3.up*.75f,new Vector3(1.85f,.09f,1.2f),wood);
                foreach(float x in new[]{-.8f,.8f})b.Add(p+new Vector3(x,.35f,0),new Vector3(.09f,.7f,1),wood);
                if(privacy)b.Add(p+new Vector3(0,1.03f,0),new Vector3(1.85f,.5f,.045f),blue);
                foreach(float x in new[]{-.48f,.48f}){Chair(b,p+new Vector3(x,0,-.84f),0);Chair(b,p+new Vector3(x,0,.84f),180);}
            }
            if(!privacy){b.Add(new Vector3(0,.5f,d/2-.65f),new Vector3(w-2,1,.9f),white);b.Add(new Vector3(0,1.04f,d/2-.65f),new Vector3(w-2,.08f,1),stone);}
        }
        static void Shop(Parts b,float w,float d,bool shop)
        {
            for(int row=0;row<3;row++)for(int col=0;col<Mathf.FloorToInt(w/3);col++)
            {
                var p=new Vector3(-w/2+1.5f+col*3,0,-d/2+2+row*(d-3)/3);
                b.Add(p+new Vector3(-.9f,1,0),new Vector3(.08f,2,.5f),wood);b.Add(p+new Vector3(.9f,1,0),new Vector3(.08f,2,.5f),wood);
                for(int s=0;s<4;s++){b.Add(p+new Vector3(0,.25f+s*.5f,0),new Vector3(1.9f,.06f,.55f),wood);for(int j=0;j<7;j++)b.Add(p+new Vector3(-.75f+j*.24f,.46f+s*.5f,0),new Vector3(.15f,.33f,.38f),j%3==0?blue:j%3==1?cream:red);}
            }
            if(shop)b.Add(new Vector3(w/2-2,.5f,-d/2+1.2f),new Vector3(2.4f,1,.65f),wood);
        }
        static void Medical(Parts b,float w,float d)
        {
            for(int i=0;i<2;i++){var p=new Vector3(i==0?-1.3f:1.3f,0,1.3f);b.Add(p+Vector3.up*.55f,new Vector3(1,.18f,2.1f),white);b.Add(p+Vector3.up*.3f,new Vector3(.8f,.5f,1.8f),stone);b.Add(p+new Vector3(0,.7f,.7f),new Vector3(.7f,.16f,.4f),blue);}
            Desk(b,new Vector3(-1,0,-2.5f),true);Chair(b,new Vector3(-1,0,-3.1f),0);b.Add(new Vector3(1.4f,1,d/2-.45f),new Vector3(1.1f,2,.6f),white);
            b.Add(new Vector3(1.4f,1.4f,d/2-.78f),new Vector3(.5f,.13f,.02f),red);b.Add(new Vector3(1.4f,1.4f,d/2-.79f),new Vector3(.13f,.5f,.02f),red);
        }
        static void Maker(Parts b,float w,float d)
        {
            Study(b,w,d,24,false);
            for(int i=0;i<4;i++){float x=-w/2+2+i*(w-4)/4;b.Add(new Vector3(x,1.35f,d/2-.7f),new Vector3(.8f,.7f,.7f),ink);b.Add(new Vector3(x,1.35f,d/2-1.06f),new Vector3(.63f,.52f,.025f),orange);}
        }
        static void Stair(Transform parent,Vector3 pos,float width,float depth)
        {
            var g=Group("Two_flight_stair_EST",parent);g.localPosition=pos;
            float step=(depth-1.2f)/12;
            for(int i=0;i<12;i++)
            {
                float h=(i+1)*H/24;
                Box("Flight_A_step",g,new Vector3(-width*.25f,h/2,-depth/2+(i+.5f)*step),new Vector3(width*.46f,h,step+.015f),stone);
                float y=H/2+h;
                Box("Flight_B_step",g,new Vector3(width*.25f,H/2+h/2,depth/2-1.2f-(i+.5f)*step),new Vector3(width*.46f,h,step+.015f),stone);
                foreach(float sign in new[]{-1f,1f})Box("Rail_post",g,new Vector3(sign*width*.49f,(sign<0?h:y)+.5f,sign<0?-depth/2+(i+.5f)*step:depth/2-1.2f-(i+.5f)*step),new Vector3(.04f,1,.04f),ink);
            }
            Box("Half_landing",g,new Vector3(0,H/2-.08f,depth/2-.6f),new Vector3(width,.16f,1.2f),stone);
            Beam("Handrail_A",g,new Vector3(-width*.49f,1,-depth/2),new Vector3(-width*.49f,H/2+1,depth/2-1.2f),.055f,ink);
            Beam("Handrail_B",g,new Vector3(width*.49f,H+1,-depth/2),new Vector3(width*.49f,H/2+1,depth/2-1.2f),.055f,ink);
        }
        static void Rail(Transform p,Vector3 center,float length,bool alongZ,bool elevated)
        {
            if(!elevated)return;
            Box("Corridor_parapet",p,center+Vector3.up*.43f,alongZ?new Vector3(.18f,.86f,length):new Vector3(length,.86f,.18f),white);
            Box("Rail_top",p,center+Vector3.up*1.12f,alongZ?new Vector3(.06f,.06f,length):new Vector3(length,.06f,.06f),ink);
            for(float s=-length/2;s<=length/2;s+=1.3f)Box("Rail_pickett",p,center+new Vector3(alongZ?0:s,.99f,alongZ?s:0),new Vector3(.035f,.26f,.035f),ink);
        }
        static void Bridge(Transform parent)
        {
            var g=Floor(parent,3,2);g.name="Glass_bridge_to_B1_REF_image2_EST_13m";
            Box("Bridge_deck",g,new Vector3(0,0,-39.5f),new Vector3(3.6f,.24f,13),stone);
            Box("Glass_roof",g,new Vector3(0,3.4f,-39.5f),new Vector3(3.8f,.10f,13),glass);
            foreach(float x in new[]{-1.8f,1.8f})
            {
                Box("Side_glass",g,new Vector3(x,1.7f,-39.5f),new Vector3(.035f,3.3f,13),glass);
                foreach(float y in new[]{.15f,1.7f,3.3f})Box("Long_steel_beam",g,new Vector3(x,y,-39.5f),new Vector3(.14f,y==1.7f?.07f:.2f,13),ink);
                for(int i=0;i<4;i++)
                {
                    float z=-46+i*3.25f;
                    Beam("Orange_diagonal_up",g,new Vector3(x,.28f,z),new Vector3(x,3.12f,z+3.25f),.13f,orange);
                    Beam("Orange_diagonal_down",g,new Vector3(x,3.12f,z),new Vector3(x,.28f,z+3.25f),.13f,orange);
                }
                for(int i=0;i<5;i++)Box("Bridge_frame",g,new Vector3(x,1.7f,-46+i*3.25f),new Vector3(.12f,3.4f,.12f),ink);
                for(int i=0;i<5;i++)Box("Upper_louvre",g,new Vector3(x,2.92f+i*.07f,-39.5f),new Vector3(.12f,.035f,13),ink);
            }
            Box("B1_landing_marker_only",g,new Vector3(0,-.12f,-47.5f),new Vector3(7,.24f,3),white);
            Label("HƯỚNG B1\nVị trí nối ước lượng",g,new Vector3(0,2,-48.9f),.35f,ink);
        }
        static void Site()
        {
            var g=Group("05_SanTrong_va_CanhQuan_EST",campus);
            Box("Site",g,new Vector3(0,-.32f,-3),new Vector3(72,.25f,94),stone);
            Box("Court_paving",g,new Vector3(0,-.13f,4),new Vector3(31,.12f,40),tile);
            for(int x=-14;x<=14;x+=2)Box("Tile_joint",g,new Vector3(x,-.065f,4),new Vector3(.018f,.008f,40),stone);
            for(int z=-16;z<=24;z+=2)Box("Tile_joint",g,new Vector3(0,-.065f,z),new Vector3(31,.008f,.018f),stone);
            for(int row=0;row<3;row++)foreach(float x in new[]{-8f,8f})
            {
                if(row>=1 && x>0)continue; // Clear the stair approach and pond surround.
                var p=new Vector3(x,0,-7+row*11);Box("Raised_planter",g,p+Vector3.up*.3f,new Vector3(5,.6f,5),stone);
                Box("Planting",g,p+Vector3.up*.61f,new Vector3(4.7f,.08f,4.7f),green);
                var trunk=GameObject.CreatePrimitive(PrimitiveType.Cylinder);trunk.name="Tree_trunk";trunk.transform.SetParent(g,false);trunk.transform.localPosition=p+Vector3.up*2.3f;trunk.transform.localScale=new Vector3(.18f,2,.18f);trunk.GetComponent<Renderer>().sharedMaterial=wood;
                for(int i=0;i<6;i++){var leaf=GameObject.CreatePrimitive(PrimitiveType.Sphere);leaf.name="Tree_canopy_schematic";leaf.transform.SetParent(g,false);float a=i*Mathf.PI/3;leaf.transform.localPosition=p+new Vector3(Mathf.Cos(a)*1.1f,4.3f+(i%2)*.4f,Mathf.Sin(a)*1.1f);leaf.transform.localScale=new Vector3(2.1f,1.3f,2.1f);leaf.GetComponent<Renderer>().sharedMaterial=green;UnityEngine.Object.DestroyImmediate(leaf.GetComponent<Collider>());}
                Box("Bench",g,p+new Vector3(0,.45f,3.2f),new Vector3(3,.12f,.5f),orange);
                foreach(float bx in new[]{-1.1f,1.1f})Box("Bench_leg",g,p+new Vector3(bx,.2f,3.2f),new Vector3(.14f,.4f,.5f),ink);
            }
        }
        static void Arch(Transform p,float x,float z)
        {
            var vertices=new List<Vector3>();var indices=new List<int>();const int n=28;float radius=2.25f,spring=2.1f;
            for(int i=0;i<=n;i++){float dx=-radius+2*radius*i/n;float y=spring+Mathf.Sqrt(Mathf.Max(0,radius*radius-dx*dx))*.8f;vertices.Add(new Vector3(x+dx,y,z-.2f));vertices.Add(new Vector3(x+dx,4.2f,z-.2f));vertices.Add(new Vector3(x+dx,y,z+.2f));vertices.Add(new Vector3(x+dx,4.2f,z+.2f));}
            for(int i=0;i<n;i++){int a=i*4,b=a+4;indices.AddRange(new[]{a,a+1,b,b,a+1,b+1,a+2,b+2,a+3,b+2,b+3,a+3,a,b,a+2,b,b+2,a+2});}
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();SaveMesh("Old_arch",p,mesh,cream);
            for(int i=0;i<n;i++){float a=Mathf.PI*i/n,b=Mathf.PI*(i+1)/n;Beam("Arch_moulding",p,new Vector3(x+radius*Mathf.Cos(a),spring+radius*.8f*Mathf.Sin(a),z-.27f),new Vector3(x+radius*Mathf.Cos(b),spring+radius*.8f*Mathf.Sin(b),z-.27f),.12f,white);}
            Box("Arch_keystone",p,new Vector3(x,3.85f,z-.32f),new Vector3(.26f,.43f,.26f),white);
        }
        static void Pane(Transform p,Vector3 center,float w,float h)
        {
            Box("Glass",p,center,new Vector3(w,h,.035f),glass);
            foreach(float x in new[]{-w/2,0,w/2})Box("Window_mullion",p,center+Vector3.right*x,new Vector3(.06f,h,.09f),white);
            foreach(float y in new[]{-h/2,h/2})Box("Window_frame",p,center+Vector3.up*y,new Vector3(w,.06f,.09f),white);
        }
        static Transform Group(string n,Transform parent){var t=new GameObject(n).transform;t.SetParent(parent,false);return t;}
        static Material Mat(string n,Color c){var m=new Material(Shader.Find("Standard"));m.name=n;m.color=c;m.SetFloat("_Glossiness",.18f);AssetDatabase.CreateAsset(m,Root+"/Materials/"+n+".mat");return m;}
        static GameObject Box(string n,Transform p,Vector3 pos,Vector3 scale,Material m)
        {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;return g;}
        static void Beam(string n,Transform p,Vector3 a,Vector3 b,float thickness,Material m)
        {var g=Box(n,p,(a+b)/2,new Vector3(thickness,Vector3.Distance(a,b),thickness),m);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
        static Transform Label(string text,Transform p,Vector3 pos,float size,Material color)
        {var g=Group("Label_"+text.Replace('\n',' '),p);g.localPosition=pos;var t=g.gameObject.AddComponent<TextMesh>();t.text=text;t.font=font;t.fontSize=48;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=color.color;g.GetComponent<MeshRenderer>().sharedMaterial=labelMaterial;return g;}
        static void SaveMesh(string name,Transform p,Mesh mesh,Material m)
        {mesh.name=name;AssetDatabase.CreateAsset(mesh,Root+"/Meshes/M_"+(meshNumber++)+".asset");var g=Group(name,p);g.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;g.gameObject.AddComponent<MeshRenderer>().sharedMaterial=m;}
        sealed class Parts
        {
            readonly Dictionary<Material,List<CombineInstance>> batches=new Dictionary<Material,List<CombineInstance>>();
            public void Add(Vector3 p,Vector3 size,Material m){Add(p,size,m,Quaternion.identity);}
            public void Add(Vector3 p,Vector3 size,Material m,Quaternion rotation)
            {if(!batches.TryGetValue(m,out var list)){list=new List<CombineInstance>();batches.Add(m,list);}list.Add(new CombineInstance{mesh=cube,transform=Matrix4x4.TRS(p,rotation,size)});}
            public void Save(Transform p,string label)
            {foreach(var entry in batches){var mesh=new Mesh{indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(entry.Value.ToArray(),true,true);mesh.RecalculateBounds();SaveMesh(label+"_"+entry.Key.name,p,mesh,entry.Key);}}
        }
        static void WriteSchedule(B2CampusRoom[] all)
        {
            Directory.CreateDirectory("Docs/CoSoB2Campus");var lines=new List<string>{"id,wing,level,floor_label,name,use,seats,width_est,depth_est"};
            foreach(var r in all)lines.Add(string.Join(",",r.roomId,r.wing.ToString(),r.level.ToString(),r.floorLabel,r.displayName,r.use,r.seats.ToString(),r.size.x.ToString(System.Globalization.CultureInfo.InvariantCulture),r.size.z.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            File.WriteAllLines("Docs/CoSoB2Campus/RoomSchedule.csv",lines,System.Text.Encoding.UTF8);
        }
        static void Capture(Camera camera,string name)
        {
            var rt=new RenderTexture(1800,1200,24);camera.targetTexture=rt;camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(1800,1200,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1800,1200),0,0);t.Apply();File.WriteAllBytes("Docs/CoSoB2Campus/Previews/"+name+".png",t.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
#endif
