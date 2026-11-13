#if UNITY_EDITOR
using UnityEngine;

namespace UEH.B2Campus.Editor
{
    public static partial class B2CampusBuilder
    {
        static Material paleWood, mint, yellow, pink, vividRed, vividBlue, porcelain, labFloor;
        static void ReferenceMaterials()
        {
            paleWood=Mat("Reference_light_oak",new Color(.67f,.48f,.29f));
            mint=Mat("Reference_mint_chair",new Color(.39f,.70f,.49f));
            yellow=Mat("Reference_yellow_chair",new Color(.98f,.73f,.06f));
            pink=Mat("Reference_lilac_chair",new Color(.74f,.50f,.72f));
            vividRed=Mat("Reference_red_chair",new Color(.88f,.12f,.09f));
            vividBlue=Mat("Reference_blue_table",new Color(.02f,.48f,.72f));
            porcelain=Mat("Reference_warm_white",new Color(.93f,.91f,.85f));
            labFloor=Mat("Reference_lab_sage_tiles",new Color(.52f,.63f,.52f));
        }
        static void ReferenceComputerLab(Parts b,Transform room,Transform high,float w,float d)
        {
            room.Find("Floor").GetComponent<Renderer>().sharedMaterial=labFloor;
            // Four representative rows with a central aisle; capacity is separate metadata.
            int perSide=Mathf.Clamp(Mathf.FloorToInt((w-2)/2.4f),2,5);
            float deskWidth=Mathf.Min(1.05f,(w/2-1.2f)/perSide);
            float bankWidth=deskWidth*perSide;
            for(int row=0;row<4;row++)foreach(float side in new[]{-1f,1f})
            {
                float z=-d/2+1.7f+row*1.65f;
                float bankX=side*(.65f+bankWidth/2);
                b.Add(new Vector3(bankX,.76f,z),new Vector3(bankWidth,.07f,.78f),paleWood);
                b.Add(new Vector3(bankX,.39f,z+.36f),new Vector3(bankWidth,.76f,.065f),paleWood);
                foreach(float edge in new[]{-1f,1f})b.Add(new Vector3(bankX+edge*(bankWidth/2-.035f),.38f,z),new Vector3(.07f,.76f,.78f),paleWood);
                for(int col=0;col<perSide;col++)
                {
                    float x=bankX-bankWidth/2+(col+.5f)*deskWidth;
                    b.Add(new Vector3(x,1.14f,z+.13f),new Vector3(.68f,.43f,.06f),ink);
                    b.Add(new Vector3(x,1.14f,z+.166f),new Vector3(.62f,.37f,.012f),screen);
                    b.Add(new Vector3(x,.91f,z+.13f),new Vector3(.06f,.23f,.06f),stone);
                    b.Add(new Vector3(x,.81f,z+.13f),new Vector3(.3f,.035f,.22f),ink);
                    b.Add(new Vector3(x,.81f,z-.2f),new Vector3(.45f,.025f,.15f),ink);
                    b.Add(new Vector3(x+.34f,.81f,z-.18f),new Vector3(.08f,.035f,.12f),ink);
                    MetalChair(b,new Vector3(x,0,z+.9f),ink,180);
                    b.Add(new Vector3(x+deskWidth*.36f,.35f,z),new Vector3(.18f,.5f,.4f),ink);
                }
            }
            // Lectern and teacher desk at the front; no device numbers or text.
            Table(b,new Vector3(-w/2+1.7f,0,d/2-1.1f),1.5f,.7f,porcelain);
            b.Add(new Vector3(-w/2+1.7f,1.05f,d/2-1.1f),new Vector3(.7f,.16f,.55f),stone,Quaternion.Euler(-12,0,0));
            b.Add(new Vector3(-w/2+1.7f,.55f,d/2-1.1f),new Vector3(.65f,1.1f,.5f),porcelain);
            MetalChair(b,new Vector3(-w/2+.5f,0,d/2-1.2f),ink,90);
            foreach(var t in high.GetComponentsInChildren<Transform>())
                if(t.name=="Window_frame" || t.name=="Window_mullion")t.GetComponent<Renderer>().sharedMaterial=ink;
            for(int i=0;i<Mathf.Max(2,Mathf.RoundToInt(w/3.2f));i++)
            {
                float x=-w/2+(i+.5f)*w/Mathf.Max(2,Mathf.RoundToInt(w/3.2f));
                Box("Lab_roller_blind",high,new Vector3(x,3.45f,d/2-.12f),new Vector3(w/Mathf.Max(2,Mathf.RoundToInt(w/3.2f))-.2f,.16f,.14f),stone);
            }
            for(float x=-w/2+.6f;x<w/2;x+=.6f)Box("Lab_ceiling_joint",high,new Vector3(x,4.025f,0),new Vector3(.009f,.01f,d),stone);
            for(float z=-d/2+.6f;z<d/2;z+=.6f)Box("Lab_ceiling_joint",high,new Vector3(0,4.025f,z),new Vector3(w,.01f,.009f),stone);
            for(float x=-w/2+1.4f;x<w/2;x+=2.7f)for(float z=-3;z<4;z+=3)
                Box("Lab_square_light",high,new Vector3(x,3.99f,z),new Vector3(.62f,.04f,.62f),porcelain);
            Box("Lab_projector",high,new Vector3(0,3.35f,2.2f),new Vector3(.5f,.2f,.4f),ink);
            foreach(float x in new[]{-.22f,.22f})Box("Projector_hanger",high,new Vector3(x,3.7f,2.2f),new Vector3(.025f,.7f,.025f),stone);
            for(float x=-w/2+2;x<w/2;x+=3.5f)
            {
                var fan=Group("Lab_wall_fan",high);fan.localPosition=new Vector3(x,3.35f,d/2-.35f);
                for(int j=0;j<24;j++)
                {
                    float a=j*Mathf.PI/12,aa=(j+1)*Mathf.PI/12;
                    Beam("Fan_guard",fan,new Vector3(Mathf.Cos(a)*.25f,Mathf.Sin(a)*.25f,0),new Vector3(Mathf.Cos(aa)*.25f,Mathf.Sin(aa)*.25f,0),.012f,stone);
                }
                Box("Fan_blade",fan,Vector3.zero,new Vector3(.43f,.05f,.06f),stone);
                Box("Fan_blade",fan,Vector3.zero,new Vector3(.05f,.43f,.06f),stone);
            }
            for(float x=-w/2+.8f;x<w/2;x+=.8f)b.Add(new Vector3(x,.045f,0),new Vector3(.009f,.005f,d-.15f),white);
            for(float z=-d/2+.8f;z<d/2;z+=.8f)b.Add(new Vector3(0,.045f,z),new Vector3(w-.15f,.005f,.009f),white);
        }
        static void MetalChair(Parts b,Vector3 p,Material color,float angle=0,float height=.45f)
        {
            var q=Quaternion.Euler(0,angle,0);
            b.Add(p+Vector3.up*height,new Vector3(.46f,.07f,.44f),color,q);
            b.Add(p+q*new Vector3(0,height+.23f,-.2f),new Vector3(.46f,.42f,.055f),color,q*Quaternion.Euler(-8,0,0));
            foreach(float x in new[]{-.19f,.19f})foreach(float z in new[]{-.18f,.18f})
                b.Add(p+q*new Vector3(x,height/2,z),new Vector3(.035f,height,.035f),ink,q);
            b.Add(p+q*new Vector3(0,height*.45f,-.18f),new Vector3(.42f,.025f,.025f),ink,q);
        }
        static void Table(Parts b,Vector3 p,float w,float d,Material top,float height=.76f)
        {
            b.Add(p+Vector3.up*height,new Vector3(w,.065f,d),top);
            foreach(float x in new[]{-w/2+.12f,w/2-.12f})foreach(float z in new[]{-d/2+.1f,d/2-.1f})
                b.Add(p+new Vector3(x,height/2,z),new Vector3(.045f,height,.045f),ink);
        }
        static void Fixtures(Transform high,float w,float d,bool classroom)
        {
            // Light fixtures and fans belong to the ceiling group, so cutaway hides them.
            for(float z=-d/2+1.4f;z<d/2-1;z+=2.5f)
            {
                foreach(float x in new[]{-w*.27f,w*.27f})
                    Box("Reference_linear_light",high,new Vector3(x,3.91f,z),new Vector3(1.3f,.06f,.20f),porcelain);
                var fan=Group("Reference_ceiling_fan",high);fan.localPosition=new Vector3(0,3.56f,z);
                Box("Stem",fan,new Vector3(0,.18f,0),new Vector3(.04f,.36f,.04f),stone);
                Box("Motor",fan,Vector3.zero,new Vector3(.21f,.13f,.21f),stone);
                for(int j=0;j<4;j++)
                {
                    float a=j*90;var q=Quaternion.Euler(0,a,0);
                    var blade=Box("Blade",fan,q*new Vector3(.28f,0,0),new Vector3(.48f,.035f,.13f),stone);
                    blade.transform.localRotation=q;
                }
                if(classroom)for(int j=0;j<24;j++)
                {
                    float a=j*Mathf.PI/12,aa=(j+1)*Mathf.PI/12;
                    Beam("Fan_guard",fan,new Vector3(Mathf.Cos(a)*.57f,-.07f,Mathf.Sin(a)*.57f),new Vector3(Mathf.Cos(aa)*.57f,-.07f,Mathf.Sin(aa)*.57f),.012f,stone);
                }
            }
        }
        static void ReferenceClassroom(Parts b,Transform high,float w,float d)
        {
            // White teaching wall; the reference has dark windows along the side.
            foreach(var t in high.GetComponentsInChildren<Transform>())
                if((t.name=="Glass" || t.name=="Window_frame" || t.name=="Window_mullion") && t.localPosition.z>d/2-.1f)t.gameObject.SetActive(false);
            Box("Teaching_wall",high,new Vector3(0,2.6f,d/2-.07f),new Vector3(w,3.2f,.12f),porcelain);
            var windows=Group("Reference_side_window_insets",high);
            for(int i=0;i<3;i++)
            {
                float z=-d/2+2+i*2.5f;
                Box("Dark_window_frame",windows,new Vector3(w/2-.085f,2.15f,z),new Vector3(.04f,1.8f,1.85f),ink);
                Box("Blue_glass_inset",windows,new Vector3(w/2-.115f,2.15f,z),new Vector3(.02f,1.65f,1.7f),screen);
                Box("Window_bar",windows,new Vector3(w/2-.14f,2.15f,z),new Vector3(.03f,1.7f,.045f),ink);
                Box("Window_bar",windows,new Vector3(w/2-.14f,2.15f,z),new Vector3(.03f,.045f,1.75f),ink);
            }
            // Representative rows: room capacity remains metadata, not a required prop count.
            int cols=Mathf.Clamp(Mathf.FloorToInt((w-2)/2.7f),2,6);
            float span=Mathf.Min(w-2,cols*2.7f);
            Material[] colors={porcelain,mint,porcelain,vividRed};
            for(int row=0;row<4;row++)for(int col=0;col<cols;col++)
            {
                var p=new Vector3(-span/2+(col+.5f)*span/cols,0,-d/2+1.7f+row*1.55f);
                Table(b,p,2.15f,.57f,paleWood);
                for(int j=0;j<2;j++)MetalChair(b,p+new Vector3(j==0?-.53f:.53f,0,-.57f),colors[(row+col+j)%4]);
            }
            b.Add(new Vector3(0,2.2f,d/2-.18f),new Vector3(Mathf.Min(4.4f,w-2),1.65f,.10f),ink);
            b.Add(new Vector3(0,2.2f,d/2-.25f),new Vector3(Mathf.Min(4.25f,w-2.15f),1.5f,.025f),porcelain);
            Table(b,new Vector3(-w*.3f,0,d/2-1.1f),1.4f,.65f,porcelain);
            Box("Ceiling_projector",high,new Vector3(0,3.48f,1),new Vector3(.38f,.16f,.3f),white);
            Box("Projector_mount",high,new Vector3(0,3.74f,1),new Vector3(.06f,.5f,.06f),stone);
            foreach(float x in new[]{-w/2+.15f,w/2-.15f})
            {
                Box("Wall_air_conditioner",high,new Vector3(x,3.35f,2),new Vector3(.3f,.38f,1.1f),porcelain);
                Box("AC_vent",high,new Vector3(x+(x<0?.17f:-.17f),3.26f,2),new Vector3(.02f,.09f,.95f),stone);
            }
            foreach(var t in high.GetComponentsInChildren<Transform>())
                if(t.name=="Window_frame" || t.name=="Window_mullion")t.GetComponent<Renderer>().sharedMaterial=ink;
            Fixtures(high,w,d,true);
            for(float x=-w/2+.6f;x<w/2;x+=.6f)Box("Ceiling_grid",high,new Vector3(x,4.025f,0),new Vector3(.012f,.01f,d),stone);
            for(float z=-d/2+.6f;z<d/2;z+=.6f)Box("Ceiling_grid",high,new Vector3(0,4.025f,z),new Vector3(w,.01f,.012f),stone);
        }
        static void ReferenceCanteen(Parts b,Transform high,float w,float d)
        {
            float counterZ=1.1f;
            b.Add(new Vector3(0,.54f,counterZ),new Vector3(6.6f,1.08f,1.3f),paleWood);
            b.Add(new Vector3(0,1.12f,counterZ),new Vector3(6.9f,.12f,1.55f),porcelain);
            for(float x=-3.2f;x<=3.2f;x+=.16f)b.Add(new Vector3(x,.55f,counterZ-.67f),new Vector3(.045f,1.06f,.04f),wood);
            for(int i=0;i<5;i++)MetalChair(b,new Vector3(-2.7f+i*1.35f,0,counterZ-1.3f),i==1?vividRed:stone,0,.78f);
            Material[] colors={yellow,mint,pink,vividBlue};
            for(int i=0;i<6;i++)
            {
                float x=-w/2+2+(i%3)*(w-4)/2,z=-3.1f+(i/3)*5.9f;
                if(i==4)continue;
                var p=new Vector3(x,0,z);Table(b,p,1.15f,1.1f,i%2==0?vividRed:vividBlue);
                MetalChair(b,p+new Vector3(-.83f,0,0),colors[i%4],90);
                MetalChair(b,p+new Vector3(.83f,0,0),colors[(i+1)%4],-90);
            }
            // Glass-front drink refrigerator and visible bottles, inspired by the photo.
            float fridgeX=-w/2+1.1f;
            b.Add(new Vector3(fridgeX,1.25f,2.2f),new Vector3(1.45f,2.5f,.9f),vividBlue);
            b.Add(new Vector3(fridgeX,1.2f,1.72f),new Vector3(1.22f,2.1f,.03f),ink);
            for(int row=0;row<4;row++)for(int i=0;i<5;i++)
                b.Add(new Vector3(fridgeX-.46f+i*.23f,.4f+row*.48f,1.64f),new Vector3(.12f,.28f,.12f),colors[(i+row)%4]);
            Box("Fridge_glass",high,new Vector3(fridgeX,1.2f,1.53f),new Vector3(1.22f,2.1f,.025f),glass);
            Label("DRINKS",high,new Vector3(fridgeX,2.34f,1.68f),.035f,porcelain);
            for(int row=0;row<3;row++)
            {
                b.Add(new Vector3(0,1.1f+row*.52f,d/2-.5f),new Vector3(6,.06f,.7f),paleWood);
                for(int i=0;i<16;i++)b.Add(new Vector3(-2.8f+i*.37f,1.3f+row*.52f,d/2-.55f),new Vector3(.24f,.3f,.17f),colors[(i+row)%4]);
            }
            Fixtures(high,w,d,false);
        }
        static void ReferenceMaker(Parts b,Transform high,float w,float d)
        {
            // Wide open workshop frontage and suspended wood sign.
            foreach(var t in high.GetComponentsInChildren<Transform>())
                if((t.name=="Glass" || t.name=="Window_frame" || t.name=="Window_mullion") && t.localPosition.z<0)t.gameObject.SetActive(false);
            Box("Maker_wood_sign",high,new Vector3(0,3.1f,-d/2-.14f),new Vector3(7.3f,.95f,.13f),paleWood);
            Label("UEH | MAKER SPACE",high,new Vector3(0,3.15f,-d/2-.22f),.065f,wood);
            foreach(float x in new[]{-2.6f,2.6f})Box("Sign_chain",high,new Vector3(x,3.75f,-d/2-.14f),new Vector3(.025f,.6f,.025f),ink);
            b.Add(new Vector3(0,1.85f,d/2-.18f),new Vector3(w-1,2.5f,.12f),paleWood);
            for(int i=0;i<22;i++)
            {
                float x=-w/2+1+i*(w-2)/22;
                b.Add(new Vector3(x,2+(i%3)*.25f,d/2-.3f),new Vector3(.045f,.55f,.06f),ink,Quaternion.Euler(0,0,(i%3-1)*25));
                b.Add(new Vector3(x,2.28f+(i%3)*.25f,d/2-.31f),new Vector3(.23f,.065f,.08f),stone);
            }
            for(int i=0;i<4;i++)
            {
                var p=new Vector3((i%2==0?-1:1)*3.3f,0,-1+(i/2)*3.2f);
                Table(b,p,3.6f,1.25f,paleWood,.87f);
                b.Add(p+new Vector3(1.2f,.98f,-.58f),new Vector3(.3f,.22f,.25f),ink);
                b.Add(p+new Vector3(0,.94f,.1f),new Vector3(.85f,.07f,.45f),wood);
            }
            foreach(float x in new[]{-w/2+1.3f,w/2-1.3f})
            {
                b.Add(new Vector3(x,.6f,1.2f),new Vector3(1.25f,1.2f,1.1f),ink);
                b.Add(new Vector3(x,1.3f,1.2f),new Vector3(1.7f,.12f,1.4f),stone);
                b.Add(new Vector3(x+.4f,2,1.6f),new Vector3(.3f,1.5f,.4f),ink);
                b.Add(new Vector3(x,2.68f,1.5f),new Vector3(1.1f,.3f,.5f),ink);
                b.Add(new Vector3(x,1.9f,1.1f),new Vector3(.025f,1.1f,.04f),stone);
            }
            foreach(float x in new[]{-w*.3f,w*.3f})
            {
                b.Add(new Vector3(x,.55f,-d/2+.35f),new Vector3(3.8f,1.1f,.65f),porcelain);
                for(int i=0;i<15;i++)
                {
                    var p=new Vector3(x-1.7f+i*.24f,1.15f,-d/2+.35f);
                    b.Add(p,new Vector3(.22f,.22f,.4f),green);
                    b.Add(p+new Vector3(.04f,-.25f,-.35f),new Vector3(.12f,.5f,.08f),green,Quaternion.Euler(0,0,(i%3-1)*20));
                }
            }
            Fixtures(high,w,d,false);
        }
    }
}
#endif
