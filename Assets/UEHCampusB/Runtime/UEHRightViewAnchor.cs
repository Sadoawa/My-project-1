using UnityEngine;
namespace UEH.CampusB
{
    /// <summary>Original review marker position, so right-wing alignment is repeatable.</summary>
    public sealed class UEHRightViewAnchor : MonoBehaviour
    {
        [HideInInspector] public Vector3 originalPosition;
    }
}
