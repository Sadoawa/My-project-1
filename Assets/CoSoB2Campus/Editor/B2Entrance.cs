#if UNITY_EDITOR
using UnityEngine;
namespace UEH.B2Campus.Editor
{
    public static partial class B2CampusBuilder
    {
        static void WindowWall(Transform low,Transform high,float x,float z,float width,Material wall)
        {
            int bays=Mathf.Max(1,Mathf.RoundToInt(width/3.8f));
            float pitch=width/bays;
            float opening=Mathf.Min(2.35f,pitch*.65f);
            Box("Solid_window_sill",low,new Vector3(x,.5f,z),new Vector3(width,1,.18f),wall);
            Box("Solid_window_header",high,new Vector3(x,3.65f,z),new Vector3(width,1.1f,.18f),wall);
            for(int i=0;i<bays;i++)
            {
                float center=x-width/2+(i+.5f)*pitch;
                Pane(high,new Vector3(center,2.05f,z),opening,2.1f);
                float pier=(pitch-opening)/2;
                foreach(float side in new[]{-1f,1f})
                    Box("Solid_window_pier",high,new Vector3(center+side*(opening/2+pier/2),2.05f,z),new Vector3(pier,2.1f,.18f),wall);
            }
        }
        static GameObject RoundPart(string name,Transform p,Vector3 pos,Vector3 scale,Material material,PrimitiveType kind=PrimitiveType.Sphere)
        {
            var g=GameObject.CreatePrimitive(kind);g.name=name;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=material;return g;
        }
        static void EntranceStair(Transform parent)
        {
            var g=Floor(parent,1,0);g.name="Right_central_exterior_stair_EST";
            // x=16 joins the courtyard corridor. Run rises straight toward the right wing.
            const int count=24;const float start=6,run=8.4f,width=8;
            for(int i=0;i<count;i++)
            {
                float top=(i+1)*H/count,x=start+(i+.5f)*run/count;
                Box("Exterior_stair_tread",g,new Vector3(x,top-.07f,5),new Vector3(run/count+.02f,.14f,width),stone);
                Box("Exterior_stair_riser",g,new Vector3(x+run/count/2,top-H/count/2-.07f,5),new Vector3(.07f,H/count,width),cream);
            }
            Box("First_floor_entry_landing",g,new Vector3(15.2f,H-.12f,5),new Vector3(1.6f,.24f,width),stone);
            foreach(float z in new[]{1f,9f})
            {
                Beam("Stair_stringer",g,new Vector3(start,.02f,z),new Vector3(start+run,H-.17f,z),.18f,cream);
                Beam("Exterior_handrail",g,new Vector3(start,1.05f,z),new Vector3(start+run,H+1.05f,z),.07f,ink);
                for(int i=0;i<=count;i++)
                {
                    float t=i/(float)count;
                    Box("Exterior_rail_baluster",g,new Vector3(start+t*run,t*H+.52f,z),new Vector3(.045f,1.04f,.045f),ink);
                }
                Box("Landing_rail",g,new Vector3(15.2f,H+1.05f,z),new Vector3(1.6f,.07f,.07f),ink);
            }
            var clad=Mat("Entrance_sandstone",new Color(.58f,.48f,.29f));
            foreach(float x in new[]{7.4f,14.1f})foreach(float z in new[]{.7f,9.3f})
                Box("Canopy_stone_column",g,new Vector3(x,3.85f,z),new Vector3(.5f,7.7f,.5f),clad);
            Box("Entrance_canopy",g,new Vector3(11,7.8f,5),new Vector3(11,.24f,10),wood);
            Box("Canopy_orange_fascia",g,new Vector3(5.45f,7.8f,5),new Vector3(.12f,.4f,10),orange);
            for(float z=.2f;z<10;z+=.18f)Box("Canopy_soffit_slat",g,new Vector3(11,7.65f,z),new Vector3(11,.06f,.055f),ink);
            foreach(float x in new[]{8f,12f})foreach(float z in new[]{2f,8f})Box("Canopy_recessed_light",g,new Vector3(x,7.6f,z),new Vector3(.22f,.025f,.22f),porcelain);
            // One continuous oval beneath the flight: its long axis crosses the stair width.
            var pond=Group("Turtle_pond_EST",g);pond.localPosition=new Vector3(10,0,5);
            var water=Mat("Pond_olive_water",new Color(.19f,.23f,.095f));water.SetFloat("_Glossiness",.82f);
            RoundPart("Pond_water",pond,new Vector3(0,.06f,0),new Vector3(4.5f,.025f,12),water,PrimitiveType.Cylinder);
            RoundPart("Pond_basin",pond,new Vector3(0,-.1f,0),new Vector3(4.9f,.12f,12.4f),ink,PrimitiveType.Cylinder);
            for(int i=0;i<96;i++)
            {
                float a=i*2*Mathf.PI/96;
                var tangent=new Vector3(-2.45f*Mathf.Sin(a),0,6.2f*Mathf.Cos(a));
                var rim=Box("Rounded_stone_pond_rim",pond,new Vector3(Mathf.Cos(a)*2.45f,.19f,Mathf.Sin(a)*6.2f),new Vector3(.55f,.28f,tangent.magnitude*2*Mathf.PI/96+.04f),stone);
                rim.transform.localRotation=Quaternion.LookRotation(tangent);
            }
            RoundPart("Turtle_basking_rock",pond,new Vector3(0,.19f,4.9f),new Vector3(1.05f,.4f,.75f),stone);
            var turtle=Group("Turtle_static",pond);turtle.localPosition=new Vector3(0,.45f,4.9f);
            RoundPart("Shell",turtle,Vector3.zero,new Vector3(.5f,.22f,.65f),green);
            RoundPart("Head",turtle,new Vector3(0,-.02f,.39f),new Vector3(.15f,.12f,.19f),cream);
            foreach(float x in new[]{-.22f,.22f})foreach(float z in new[]{-.19f,.19f})
                RoundPart("Flipper",turtle,new Vector3(x,-.06f,z),new Vector3(.18f,.07f,.14f),green);
            for(int i=0;i<14;i++)
            {
                float a=(55+(i%7)*12+(i>=7?180:0))*Mathf.Deg2Rad;
                var pos=new Vector3(Mathf.Cos(a)*3.05f,.3f,Mathf.Sin(a)*6.8f);
                RoundPart("Pond_border_foliage",pond,pos,new Vector3(.7f,.55f,.65f),green);
            }
        }
    }
}
#endif
