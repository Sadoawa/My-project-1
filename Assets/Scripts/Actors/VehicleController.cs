using UnityEngine;

/// <summary>
/// Xe NPC chạy tự động trên đường theo trục X hoặc Z.
/// </summary>
public class VehicleController : MonoBehaviour
{
    [Header("Chuyển động")]
    public float speed = 4.5f;         // m/s, dương = theo trục, âm = ngược
    public float startX = -90f;
    public float endX   = 90f;
    public float startZ = -90f;
    public float endZ   = 90f;

    public enum VehicleAxis { X, Z }
    public VehicleAxis axis = VehicleAxis.X;

    // Spin bánh xe (optional)
    public Transform[] wheels;
    public float wheelRadius = 0.35f;

    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        // Di chuyển
        Vector3 move = axis == VehicleAxis.X
            ? new Vector3(speed * Time.deltaTime, 0, 0)
            : new Vector3(0, 0, speed * Time.deltaTime);

        transform.position += move;

        // Wrap khi ra ngoài biên
        if (axis == VehicleAxis.X)
        {
            if (speed > 0 && transform.position.x > endX)
                transform.position = new Vector3(startX, transform.position.y, transform.position.z);
            else if (speed < 0 && transform.position.x < startX)
                transform.position = new Vector3(endX, transform.position.y, transform.position.z);
        }
        else
        {
            if (speed > 0 && transform.position.z > endZ)
                transform.position = new Vector3(transform.position.x, transform.position.y, startZ);
            else if (speed < 0 && transform.position.z < startZ)
                transform.position = new Vector3(transform.position.x, transform.position.y, endZ);
        }

        // Quay bánh xe
        if (wheels != null)
        {
            float rpm = (Mathf.Abs(speed) / (2 * Mathf.PI * wheelRadius)) * 360f;
            foreach (var w in wheels)
                w.Rotate(speed > 0 ? Vector3.right : Vector3.left, rpm * Time.deltaTime);
        }
    }
}

