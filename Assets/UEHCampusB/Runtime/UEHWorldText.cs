using UnityEngine;

namespace UEH.CampusB
{
    /// <summary>Connects a dynamic font atlas to a world-space, depth-tested text material.</summary>
    [ExecuteAlways, RequireComponent(typeof(TextMesh))]
    public sealed class UEHWorldText : MonoBehaviour
    {
        public Material worldMaterial;
        private void OnEnable() { Font.textureRebuilt += OnAtlasRebuilt; Refresh(); }
        private void OnDisable() { Font.textureRebuilt -= OnAtlasRebuilt; }
        private void OnAtlasRebuilt(Font font)
        {
            var text = GetComponent<TextMesh>();
            if (text.font == font) Refresh();
        }
        public void Refresh()
        {
            var text = GetComponent<TextMesh>();
            if (worldMaterial == null || text.font == null) return;
            worldMaterial.mainTexture = text.font.material.mainTexture;
            GetComponent<MeshRenderer>().sharedMaterial = worldMaterial;
        }
    }
}
