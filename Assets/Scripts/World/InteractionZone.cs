using UnityEngine;
using System;

/// <summary>
/// Collider trigger vô hình — đánh dấu điểm tương tác trong thế giới.
/// Khi nhân vật bấm F gần đây, Activate() được gọi.
/// </summary>
public class InteractionZone : MonoBehaviour
{
    [Header("Thông tin")]
    public string interactionId;    // "laptop" | "bed" | "manager" | "bike" ...
    public string label;            // hiển thị trên HUD
    public string subLabel;         // mô tả phụ
    public float  radius = 2f;

    [Header("Gizmo")]
    public Color gizmoColor = new Color(0, 1, 1, 0.3f);

    // Sự kiện: ai đó có thể đăng ký để xử lý
    public static event Action<InteractionZone> OnActivated;

    public void Activate()
    {
        Debug.Log($"[Interact] {interactionId} — {label}");
        OnActivated?.Invoke(this);
    }

    // Hiện sphere xanh trong Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, radius);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.1f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

