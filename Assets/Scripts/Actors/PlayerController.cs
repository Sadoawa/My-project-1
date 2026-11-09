using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Điều khiển nhân vật theo phong cách góc nhìn thứ nhất (First-Person POV)
/// chuẩn nguyên tác "Qua Tháng Này Đã":
///   - WASD / Mũi tên: Đi tới/lui/trái/phải
///   - Shift: Chạy nhanh
///   - Chuột: Xoay góc nhìn (Yaw / Pitch)
///   - F: Lên/xuống xe máy Vespa hoặc tương tác
///   - E: Tương tác với đối tượng gần nhất
///   - Tab / P: Mở điện thoại
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Tốc độ di chuyển")]
    public float walkSpeed   = 4.5f;
    public float runSpeed    = 7.5f;
    public float gravity     = -20f;
    public float mouseSensitivity = 2.0f;

    [Header("Xe máy Vespa")]
    public float bikeMaxSpeed  = 13f;
    public float bikeAccel     = 5f;
    public float bikeBrake     = 8f;
    public float bikeTurnSpeed = 80f;

    [Header("Camera")]
    public Transform playerCamera;
    public float eyeHeight = 1.65f;
    public bool isFirstPerson = true;

    // Trạng thái
    private CharacterController _cc;
    private Vector3  _velocity;
    private float    _yaw = 180f;
    private float    _pitch = 0f;
    private float    _bob = 0f;
    private bool     _onBike = false;
    private float    _bikeSpeed = 0f;
    private float    _bikeHeading = 180f;

    public bool  IsOnBike  => _onBike;
    public float BikeSpeed => _bikeSpeed;

    void Awake()
    {
        Instance = this;
        _cc = GetComponent<CharacterController>();
    }

    void Start()
    {
        var state = GameManager.Instance?.State;
        if (state != null)
        {
            transform.position = state.spawnPosition;
            _yaw = state.spawnYaw * Mathf.Rad2Deg;
        }

        // Tự tìm camera chính nếu chưa gán
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        // Ẩn visual model của người chơi để không chắn tầm nhìn ở góc nhìn thứ nhất
        var visual = transform.Find("VisualModel");
        if (visual != null) visual.gameObject.SetActive(false);
        var capsule = transform.Find("VisualCapsule");
        if (capsule != null) capsule.gameObject.SetActive(false);

        // Khóa con trỏ chuột vào giữa màn hình khi chơi
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Phím Esc để mở/khóa chuột
        if (GetKeyDown(Key.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // Nhấn chuột để khóa lại con trỏ nếu đang tự do
        if (GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        HandleMouseLook();

        if (_onBike)
            HandleBikeMovement();
        else
            HandleWalkMovement();

        UpdateCameraPosition();
        HandleInteractionKeys();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GÓC NHÌN CHUỘT
    // ─────────────────────────────────────────────────────────────────────────
    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 mouseDelta = GetMouseDelta();
        float mouseX = mouseDelta.x * mouseSensitivity * 0.15f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.15f;

        if (_onBike)
        {
            _yaw += mouseX * 0.5f;
        }
        else
        {
            _yaw += mouseX;
        }

        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, -65f, 65f);

        transform.rotation = Quaternion.Euler(0, _yaw, 0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ĐI BỘ (WASD)
    // ─────────────────────────────────────────────────────────────────────────
    void HandleWalkMovement()
    {
        float fwd = 0f;
        float right = 0f;

        if (GetKey(Key.W) || GetKey(Key.UpArrow)) fwd += 1f;
        if (GetKey(Key.S) || GetKey(Key.DownArrow)) fwd -= 1f;
        if (GetKey(Key.D) || GetKey(Key.RightArrow)) right += 1f;
        if (GetKey(Key.A) || GetKey(Key.LeftArrow)) right -= 1f;

        bool sprint = GetKey(Key.LeftShift);
        float currentSpeed = sprint ? runSpeed : walkSpeed;

        Vector3 inputDir = (transform.forward * fwd + transform.right * right).normalized;
        Vector3 move = inputDir * currentSpeed;

        // Trọng lực
        if (_cc.isGrounded && _velocity.y < 0) _velocity.y = -2f;
        _velocity.y += gravity * Time.deltaTime;
        move.y = _velocity.y;

        _cc.Move(move * Time.deltaTime);

        // Hiệu ứng nhấp nhô đầu (head bobbing) như bản gốc
        if (inputDir.sqrMagnitude > 0.01f && _cc.isGrounded)
        {
            _bob += Time.deltaTime * (sprint ? 12f : 8f);
        }
        else
        {
            _bob = Mathf.MoveTowards(_bob, 0, Time.deltaTime * 6f);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LÁI XE MÁY
    // ─────────────────────────────────────────────────────────────────────────
    void HandleBikeMovement()
    {
        float gas = 0f;
        if (GetKey(Key.W) || GetKey(Key.UpArrow)) gas += 1f;
        if (GetKey(Key.S) || GetKey(Key.DownArrow)) gas -= 1f;

        float steer = 0f;
        if (GetKey(Key.D) || GetKey(Key.RightArrow)) steer += 1f;
        if (GetKey(Key.A) || GetKey(Key.LeftArrow)) steer -= 1f;

        // Ga / phanh
        if (gas > 0.1f)
            _bikeSpeed = Mathf.MoveTowards(_bikeSpeed, bikeMaxSpeed, bikeAccel * Time.deltaTime);
        else if (gas < -0.1f)
            _bikeSpeed = Mathf.MoveTowards(_bikeSpeed, 0, bikeBrake * Time.deltaTime);
        else
            _bikeSpeed = Mathf.MoveTowards(_bikeSpeed, 0, bikeAccel * 0.4f * Time.deltaTime);

        // Rẽ
        if (Mathf.Abs(_bikeSpeed) > 0.2f)
        {
            _bikeHeading += steer * bikeTurnSpeed * Time.deltaTime;
            _yaw = _bikeHeading;
            transform.rotation = Quaternion.Euler(0, _bikeHeading, 0);
        }

        Vector3 move = transform.forward * _bikeSpeed;
        move.y = gravity * Time.deltaTime;
        _cc.Move(move * Time.deltaTime);

        // Va chạm xe
        if (_cc.collisionFlags != CollisionFlags.None && Mathf.Abs(_bikeSpeed) > 3f)
        {
            var (damage, engLoss, _) = StoryData.CrashCost(_bikeSpeed);
            var state = GameManager.Instance?.State;
            if (state != null)
            {
                state.bikeCondition = Mathf.Max(0, state.bikeCondition - damage);
                state.ApplyDelta(new StatDelta { energy = -engLoss, health = -Mathf.RoundToInt(engLoss * 0.5f) }, "Va chạm xe máy");
                _bikeSpeed = 0;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // VỊ TRÍ CAMERA
    // ─────────────────────────────────────────────────────────────────────────
    void UpdateCameraPosition()
    {
        if (playerCamera == null) return;

        float bobOffset = Mathf.Sin(_bob) * 0.025f;
        Vector3 eyePos = transform.position + Vector3.up * (eyeHeight + bobOffset);

        if (isFirstPerson)
        {
            playerCamera.position = eyePos;
            playerCamera.rotation = Quaternion.Euler(_pitch, _yaw, 0);
        }
        else
        {
            // Fallback góc nhìn người thứ ba nếu muốn bật
            Vector3 camTarget = eyePos - transform.forward * 3.5f + Vector3.up * 0.8f;
            playerCamera.position = camTarget;
            playerCamera.LookAt(eyePos);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TƯƠNG TÁC PHÍM F, E, TAB
    // ─────────────────────────────────────────────────────────────────────────
    void HandleInteractionKeys()
    {
        // F: Lên / xuống xe
        if (GetKeyDown(Key.F))
        {
            if (_onBike)
            {
                GetOffBike();
            }
            else
            {
                // Tìm xe Vespa gần nhất trong bán kính 3m
                var zones = FindObjectsByType<InteractionZone>(FindObjectsInactive.Exclude);
                InteractionZone bikeZone = null;
                float minDist = 3.5f;
                foreach (var z in zones)
                {
                    if (z.interactionId == "bike")
                    {
                        float d = Vector3.Distance(transform.position, z.transform.position);
                        if (d < minDist) { minDist = d; bikeZone = z; }
                    }
                }

                if (bikeZone != null)
                {
                    GetOnBike(bikeZone.transform.position);
                }
                else
                {
                    // Nếu không ở gần xe, thử kích hoạt tương tác thường
                    TriggerNearestInteraction();
                }
            }
        }

        // E: Tương tác với đối tượng gần nhất
        if (GetKeyDown(Key.E) && !_onBike)
        {
            TriggerNearestInteraction();
        }

        // V: Đổi giữa góc nhìn thứ nhất và thứ ba
        if (GetKeyDown(Key.V))
        {
            isFirstPerson = !isFirstPerson;
            var visual = transform.Find("VisualModel");
            if (visual != null) visual.gameObject.SetActive(!isFirstPerson);
        }
    }

    void TriggerNearestInteraction()
    {
        var zones = FindObjectsByType<InteractionZone>(FindObjectsInactive.Exclude);
        InteractionZone nearest = null;
        float minDist = 3.0f;
        foreach (var z in zones)
        {
            float d = Vector3.Distance(transform.position, z.transform.position);
            if (d < minDist) { minDist = d; nearest = z; }
        }
        nearest?.Activate();
    }

    public void GetOnBike(Vector3 bikePos)
    {
        _onBike = true;
        _bikeSpeed = 0f;
        _bikeHeading = transform.eulerAngles.y;
        transform.position = new Vector3(bikePos.x, transform.position.y, bikePos.z);
        eyeHeight = 1.45f;
    }

    public void GetOffBike()
    {
        _onBike = false;
        _bikeSpeed = 0f;
        eyeHeight = 1.65f;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BỘ ĐỌC INPUT HYBRID (Hỗ trợ cả New Input System và Legacy)
    // ─────────────────────────────────────────────────────────────────────────
    bool GetKey(Key key)
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            switch (key)
            {
                case Key.W: return kb.wKey.isPressed;
                case Key.S: return kb.sKey.isPressed;
                case Key.A: return kb.aKey.isPressed;
                case Key.D: return kb.dKey.isPressed;
                case Key.UpArrow: return kb.upArrowKey.isPressed;
                case Key.DownArrow: return kb.downArrowKey.isPressed;
                case Key.LeftArrow: return kb.leftArrowKey.isPressed;
                case Key.RightArrow: return kb.rightArrowKey.isPressed;
                case Key.LeftShift: return kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
            }
        }
#endif
        try
        {
            switch (key)
            {
                case Key.W: return Input.GetKey(KeyCode.W);
                case Key.S: return Input.GetKey(KeyCode.S);
                case Key.A: return Input.GetKey(KeyCode.A);
                case Key.D: return Input.GetKey(KeyCode.D);
                case Key.UpArrow: return Input.GetKey(KeyCode.UpArrow);
                case Key.DownArrow: return Input.GetKey(KeyCode.DownArrow);
                case Key.LeftArrow: return Input.GetKey(KeyCode.LeftArrow);
                case Key.RightArrow: return Input.GetKey(KeyCode.RightArrow);
                case Key.LeftShift: return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            }
        }
        catch {}
        return false;
    }

    bool GetKeyDown(Key key)
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            switch (key)
            {
                case Key.F: return kb.fKey.wasPressedThisFrame;
                case Key.E: return kb.eKey.wasPressedThisFrame;
                case Key.V: return kb.vKey.wasPressedThisFrame;
                case Key.Tab: return kb.tabKey.wasPressedThisFrame;
                case Key.P: return kb.pKey.wasPressedThisFrame;
                case Key.Escape: return kb.escapeKey.wasPressedThisFrame;
            }
        }
#endif
        try
        {
            switch (key)
            {
                case Key.F: return Input.GetKeyDown(KeyCode.F);
                case Key.E: return Input.GetKeyDown(KeyCode.E);
                case Key.V: return Input.GetKeyDown(KeyCode.V);
                case Key.Tab: return Input.GetKeyDown(KeyCode.Tab);
                case Key.P: return Input.GetKeyDown(KeyCode.P);
                case Key.Escape: return Input.GetKeyDown(KeyCode.Escape);
            }
        }
        catch {}
        return false;
    }

    Vector2 GetMouseDelta()
    {
#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse != null)
            return mouse.delta.ReadValue();
#endif
        try
        {
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }
        catch {}
        return Vector2.zero;
    }

    bool GetMouseButtonDown(int button)
    {
#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse != null)
        {
            if (button == 0) return mouse.leftButton.wasPressedThisFrame;
            if (button == 1) return mouse.rightButton.wasPressedThisFrame;
        }
#endif
        try
        {
            return Input.GetMouseButtonDown(button);
        }
        catch {}
        return false;
    }

    public enum Key { W, A, S, D, UpArrow, DownArrow, LeftArrow, RightArrow, LeftShift, F, E, V, Tab, P, Escape }
}
