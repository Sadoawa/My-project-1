using UnityEngine;
namespace UEH.B2Campus
{
    [ExecuteAlways]
    public sealed class B2TextAtlas : MonoBehaviour
    {
        public Font font;
        public Material textMaterial;
        void OnEnable()
        {
            Font.textureRebuilt+=Sync;
            if(font!=null)
            {
                foreach(var t in GetComponentsInChildren<TextMesh>(true)) font.RequestCharactersInTexture(t.text,t.fontSize,t.fontStyle);
                Sync(font);
            }
        }
        void OnDisable(){Font.textureRebuilt-=Sync;}
        void Sync(Font changed){if(changed==font && textMaterial!=null && font!=null)textMaterial.mainTexture=font.material.mainTexture;}
    }
}
