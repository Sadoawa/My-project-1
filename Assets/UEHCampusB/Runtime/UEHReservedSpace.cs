using UnityEngine;

namespace UEH.CampusB
{
    /// <summary>Editable planning volume; visible only as a selected gizmo, never as geometry.</summary>
    public sealed class UEHReservedSpace : MonoBehaviour
    {
        public Vector3 size = new Vector3(3.5f, 4.1f, 2.9f);
        public bool furnished;
        [TextArea] public string purpose = "Khoảng trống dự kiến cho thang máy; chưa dựng thang máy. Kích thước ước lượng.";

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.1f, 0.8f, 1, 0.8f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
    }
}
