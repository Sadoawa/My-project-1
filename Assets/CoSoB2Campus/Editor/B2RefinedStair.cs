#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
namespace UEH.B2Campus.Editor
{
    public static partial class B2CampusBuilder
    {
        static void RefinedStair(Transform parent,float width,float depth)
        {
            var root=Group("Refined_two_flight_stair",parent);
            float run=depth-1.2f,flightWidth=width*.46f;
            RefinedFlight(root,"Ascending_toward_middle",new Vector3(-width*.25f,0,-depth/2),0,flightWidth,run);
            RefinedFlight(root,"Return_to_next_floor",new Vector3(width*.25f,H/2,depth/2-1.2f),180,flightWidth,run);
            Box("Thin_half_landing",root,new Vector3(0,H/2-.09f,depth/2-.6f),new Vector3(width,.18f,1.2f),stone);
            // Landing rails close the outer edges without blocking either flight.
            foreach(float x in new[]{-width*.48f,width*.48f})
            {
                Beam("Landing_handrail",root,new Vector3(x,H/2+1,depth/2-1.2f),new Vector3(x,H/2+1,depth/2-.03f),.04f,ink);
                foreach(float z in new[]{depth/2-.6f,depth/2-.03f})
                    Box("Landing_baluster",root,new Vector3(x,H/2+.5f,z),new Vector3(.025f,1,.025f),ink);
            }
            Beam("Landing_end_handrail",root,new Vector3(-width*.48f,H/2+1,depth/2-.03f),new Vector3(width*.48f,H/2+1,depth/2-.03f),.04f,ink);
            for(int i=1;i<8;i++)Box("Landing_end_baluster",root,new Vector3(Mathf.Lerp(-width*.48f,width*.48f,i/8f),H/2+.5f,depth/2-.03f),new Vector3(.025f,1,.025f),ink);
        }
        static void RefinedFlight(Transform parent,string name,Vector3 origin,float yaw,float width,float run)
        {
            var flight=Group(name,parent);flight.localPosition=origin;flight.localRotation=Quaternion.Euler(0,yaw,0);
            const int steps=12;const float thickness=.18f;
            float rise=H/24,tread=run/steps;
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<steps;i++)
            {
                float z0=i*tread,z1=(i+1)*tread,top=(i+1)*rise;
                float bottom0=i*rise-thickness,bottom1=top-thickness;
                int start=vertices.Count;
                vertices.AddRange(new[]{
                    new Vector3(-width/2,bottom0,z0),new Vector3(width/2,bottom0,z0),
                    new Vector3(width/2,top,z0),new Vector3(-width/2,top,z0),
                    new Vector3(-width/2,bottom1,z1),new Vector3(width/2,bottom1,z1),
                    new Vector3(width/2,top,z1),new Vector3(-width/2,top,z1)});
                int[] faces={0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,3,7,6,3,6,2,0,4,7,0,7,3,1,2,6,1,6,5};
                foreach(int index in faces)triangles.Add(start+index);
                Box("Stone_tread_nosing",flight,new Vector3(0,top+.006f,z0+.015f),new Vector3(width,.012f,.03f),tile);
            }
            // Duplicate triangle vertices for crisp stone edges instead of smoothed block shading.
            var flatVertices=new Vector3[triangles.Count];var flatIndices=new int[triangles.Count];
            for(int i=0;i<triangles.Count;i++){flatVertices[i]=vertices[triangles[i]];flatIndices[i]=i;}
            var mesh=new Mesh();mesh.vertices=flatVertices;mesh.triangles=flatIndices;mesh.RecalculateNormals();mesh.RecalculateBounds();
            SaveMesh("Thin_sloped_stone_flight",flight,mesh,stone);
            flight.Find("Thin_sloped_stone_flight").gameObject.AddComponent<MeshCollider>().sharedMesh=mesh;
            foreach(float side in new[]{-1f,1f})
            {
                float x=side*(width/2-.04f);
                for(int i=0;i<=steps;i++)
                {
                    float z=i*tread,y=i*rise;
                    Box("Slender_baluster",flight,new Vector3(x,y+.5f,z),new Vector3(.025f,1,.025f),ink);
                }
                Beam("Continuous_handrail",flight,new Vector3(x,1,0),new Vector3(x,H/2+1,run),.04f,ink);
                Beam("Lower_rail",flight,new Vector3(x,.15f,0),new Vector3(x,H/2+.15f,run),.025f,ink);
            }
        }
    }
}
#endif
