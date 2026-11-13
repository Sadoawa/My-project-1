using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UEH.B2Campus
{
    [RequireComponent(typeof(Camera))]
    public sealed class B2CampusViewer : MonoBehaviour
    {
        public Transform campus;
        public Font font;
        public B2CampusFloor[] floors;
        public B2CampusRoom[] rooms;
        public int floor = -1;
        public int wing = -1;
        public bool cutaway;
        public string currentView = "Toàn cảnh 4 dãy";
        Vector2 scroll;
        readonly string[] wingNames = { "Dãy trái", "Dãy phải", "Dãy giữa", "Khối nối cũ" };

        public void RefreshVisibility()
        {
            foreach(var f in floors)
                if(f != null) f.gameObject.SetActive(floor<0 || (!f.roof && f.level==floor));
            foreach(var r in rooms)
                if(r != null)
                {
                    if(r.upperWalls != null) r.upperWalls.gameObject.SetActive(!cutaway);
                    if(r.planLabel != null) r.planLabel.gameObject.SetActive(cutaway);
                }
        }
        public void Overview()
        {
            floor=-1;wing=-1;cutaway=false;RefreshVisibility();
            GetComponent<Camera>().orthographic=false;
            Pose(new Vector3(76,55,-83),new Vector3(0,10,0));
            currentView="Toàn cảnh: 3 dãy mới + khối nối vàng cũ";
        }
        public void FloorPlan(int index)
        {
            floor=index;cutaway=true;RefreshVisibility();
            var camera=GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=44;
            Pose(new Vector3(0,index*4.2f+90,2),new Vector3(0,index*4.2f,2.01f));
            currentView=index==0?"Mặt bằng tầng trệt":("Dãy mới: tầng "+index+(index<3?" | Khối cũ: tầng "+(index+1):""));
        }
        public void FocusRoom(B2CampusRoom r)
        {
            floor=r.level;wing=r.wing;cutaway=false;RefreshVisibility();
            GetComponent<Camera>().orthographic=false;
            Vector3 eye=r.transform.TransformPoint(new Vector3(0,1.8f,-r.size.z/2+1.2f));
            Pose(eye,r.transform.TransformPoint(new Vector3(0,1.3f,r.size.z*.3f)));
            currentView=r.floorLabel+" / "+r.displayName+(r.seats>0?" / "+r.seats+" chỗ":"");
        }
        public void Bridge()
        {
            floor=-1;cutaway=false;RefreshVisibility();
            GetComponent<Camera>().orthographic=false;
            Pose(new Vector3(13,13,-44),new Vector3(0,10,-39));currentView="Cầu kính khối cũ tầng 3 → B1 (vị trí ước lượng)";
        }
        public void Courtyard()
        {
            floor=-1;cutaway=false;RefreshVisibility();
            GetComponent<Camera>().orthographic=false;
            Pose(new Vector3(6,3,-9),new Vector3(-2,12,27));currentView="Sân trong nhìn về dãy giữa";
        }
        public void EntranceStair()
        {
            floor=-1;wing=-1;cutaway=false;RefreshVisibility();
            GetComponent<Camera>().orthographic=false;
            Pose(new Vector3(-3,4,-5),new Vector3(12,2.6f,5));
            currentView="Cầu thang ngoài trời và hồ rùa";
        }
        public void JunctionCore()
        {
            floor=0;wing=1;cutaway=false;RefreshVisibility();
            GetComponent<Camera>().orthographic=false;
            Pose(new Vector3(23.5f,8,26.5f),new Vector3(24.5f,1.1f,33.5f));
            currentView="Cầu thang và hai thang máy — góc nối dãy giữa / phải";
        }
        public void LeftStair()
        {
            floor=0;wing=0;cutaway=false;RefreshVisibility();
            GetComponent<Camera>().orthographic=false;
            Pose(new Vector3(-28,6,29),new Vector3(-19.7f,1.8f,36.5f));
            currentView="Cầu thang trái sát bãi xe — theo bản vẽ đã chốt";
        }
        void Pose(Vector3 p,Vector3 target){transform.position=p;transform.LookAt(target);}
        void Update()
        {
            Vector3 move=Vector3.zero;Vector2 delta=Vector2.zero;bool look=false,fast=false;
#if ENABLE_INPUT_SYSTEM
            var k=Keyboard.current;var m=Mouse.current;
            if(m!=null){look=m.rightButton.isPressed;delta=m.delta.ReadValue()*.12f;}
            if(k!=null && look){move=new Vector3((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.eKey.isPressed?1:0)-(k.qKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));fast=k.leftShiftKey.isPressed;}
#elif ENABLE_LEGACY_INPUT_MANAGER
            look=Input.GetMouseButton(1);
            if(look){delta=new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"))*2;move=new Vector3((Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0),(Input.GetKey(KeyCode.E)?1:0)-(Input.GetKey(KeyCode.Q)?1:0),(Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.S)?1:0));fast=Input.GetKey(KeyCode.LeftShift);}
#endif
            if(look){var e=transform.eulerAngles;float pitch=Mathf.DeltaAngle(0,e.x);transform.rotation=Quaternion.Euler(Mathf.Clamp(pitch-delta.y,-85,85),e.y+delta.x,0);transform.position+=transform.TransformDirection(move)*Time.deltaTime*(fast?18:6);}
        }
        void OnGUI()
        {
            if(font!=null)GUI.skin.font=font;
            GUI.skin.label.wordWrap=true;
            GUILayout.BeginArea(new Rect(12,12,325,Mathf.Min(Screen.height-24,850)),GUI.skin.box);
            scroll=GUILayout.BeginScrollView(scroll);
            GUILayout.Label("B2 — BỐ CỤC 4 DÃY NHÀ");
            GUILayout.Label(currentView);
            GUILayout.Label("Kích thước & vị trí phòng: ước lượng");
            if(GUILayout.Button("Toàn cảnh"))Overview();
            if(GUILayout.Button("Cầu thang ngoài trời / hồ rùa"))EntranceStair();
            if(GUILayout.Button("Cầu thang + thang máy góc nối"))JunctionCore();
            if(GUILayout.Button("Cầu thang trái sát bãi xe"))LeftStair();
            GUILayout.BeginHorizontal();if(GUILayout.Button("Sân trong"))Courtyard();if(GUILayout.Button("Cầu kính"))Bridge();GUILayout.EndHorizontal();
            GUILayout.Label("Xem mặt bằng / tách tầng:");
            if(GetComponent<Camera>().orthographic)
            {GUILayout.BeginHorizontal();if(GUILayout.Button("Phóng to"))GetComponent<Camera>().orthographicSize=Mathf.Max(8,GetComponent<Camera>().orthographicSize-5);if(GUILayout.Button("Thu nhỏ"))GetComponent<Camera>().orthographicSize=Mathf.Min(90,GetComponent<Camera>().orthographicSize+5);GUILayout.EndHorizontal();}
            for(int i=0;i<7;i++) if(GUILayout.Button(i==0?"Tầng trệt":("Tầng "+i+(i==5?" — phòng máy":""))))FloorPlan(i);
            bool next=GUILayout.Toggle(cutaway,"Ẩn phần tường cao để nhìn nội thất");if(next!=cutaway){cutaway=next;RefreshVisibility();}
            GUILayout.Label("Chọn dãy để vào phòng:");
            wing=GUILayout.SelectionGrid(wing,wingNames,2);
            if(wing>=0 && floor>=0)
            {
                foreach(var r in rooms)if(r.wing==wing && r.level==floor)
                    if(GUILayout.Button(r.displayName+(r.seats>0?" ("+r.seats+" chỗ)":"")))FocusRoom(r);
            }
            GUILayout.Label("Giữ chuột phải + WASD: bay; Q/E: xuống/lên; Shift: nhanh. Camera xem tự do, không mô phỏng va chạm.");
            GUILayout.Label("Khối cũ: trệt, tầng 2, tầng 3 = cao độ 0, 1, 2. Công năng tầng 3 chưa xác định.");
            GUILayout.EndScrollView();GUILayout.EndArea();
        }
    }
}
