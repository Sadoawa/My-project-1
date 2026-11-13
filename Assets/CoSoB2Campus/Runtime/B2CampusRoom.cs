using UnityEngine;
namespace UEH.B2Campus
{
    public sealed class B2CampusRoom : MonoBehaviour
    {
        public string roomId;
        public string displayName;
        public string floorLabel;
        public string use;
        public int level;
        public int wing;
        public int seats;
        public bool estimated = true;
        public Vector3 size;
        public Transform upperWalls;
        public Transform planLabel;
    }
}
