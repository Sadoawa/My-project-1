using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Điều khiển NPC patrol đơn giản:
/// đi qua lại theo trục, dừng lại ngẫu nhiên.
/// </summary>
public class NPCController : MonoBehaviour
{
    [Header("Patrol")]
    public float walkSpeed = 1.4f;
    public float patrolRange = 7f;
    public float pauseChance = 0.002f;   // xác suất dừng mỗi frame
    public float pauseDuration = 2f;

    [Header("Shirt Color (nếu dùng material)")]
    public Color shirtColor = new Color(0.05f, 0.48f, 0.54f);

    // Trục di chuyển
    public enum PatrolAxis { X, Z }
    public PatrolAxis axis = PatrolAxis.X;

    private Vector3  _startPos;
    private float    _dir      = 1f;
    private float    _pauseTimer = 0f;
    private bool     _paused   = false;
    private Animator _anim;

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        _startPos = transform.position;
        _anim     = GetComponentInChildren<Animator>();
        _dir      = Random.value > 0.5f ? 1f : -1f;
    }

    void Update()
    {
        if (_paused)
        {
            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer <= 0) _paused = false;
            SetWalking(false);
            return;
        }

        // Ngẫu nhiên dừng
        if (Random.value < pauseChance)
        {
            _paused     = true;
            _pauseTimer = pauseDuration + Random.value * 2f;
            return;
        }

        Vector3 move = axis == PatrolAxis.X
            ? new Vector3(_dir * walkSpeed * Time.deltaTime, 0, 0)
            : new Vector3(0, 0, _dir * walkSpeed * Time.deltaTime);

        transform.position += move;

        // Đổi chiều
        float offset = axis == PatrolAxis.X
            ? transform.position.x - _startPos.x
            : transform.position.z - _startPos.z;

        if (Mathf.Abs(offset) > patrolRange)
        {
            _dir = -_dir;
            Vector3 fwd = axis == PatrolAxis.X ? new Vector3(_dir, 0, 0) : new Vector3(0, 0, _dir);
            if (fwd != Vector3.zero) transform.forward = fwd;
        }

        SetWalking(true);
    }

    void SetWalking(bool walking)
    {
        if (_anim != null) _anim.SetBool("IsWalking", walking);
    }
}

