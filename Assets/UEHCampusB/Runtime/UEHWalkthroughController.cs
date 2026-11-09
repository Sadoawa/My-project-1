using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UEH.CampusB
{
    /// <summary>Independent preview controls. Does not load saves or create game managers.</summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class UEHWalkthroughController : MonoBehaviour
    {
        public Camera viewCamera;
        public float walkSpeed = 4.8f;
        public float runSpeed = 7.8f;
        public float eyeHeight = 1.65f;
        public Vector3 entranceSpawn = new Vector3(0, 0.08f, 13);
        public Vector3 lobbySpawn = new Vector3(0, 0.08f, -10);
        public bool externalControl;
        public bool showHelp = true;
        private CharacterController controller;
        private float yaw = 180, pitch, verticalSpeed;

        public CharacterController Body => controller != null ? controller : GetComponent<CharacterController>();

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            yaw = transform.eulerAngles.y;
        }

        private void Start()
        {
            if (!Application.isBatchMode) SetCursor(true);
        }

        private void Update()
        {
            if (externalControl) return;
            Vector2 movement = Vector2.zero, mouse = Vector2.zero;
            bool sprint = false, escape = false, click = false, reset = false, lobby = false, help = false;
#if ENABLE_INPUT_SYSTEM
            var keys = Keyboard.current;
            if (keys != null)
            {
                movement.x = (keys.dKey.isPressed || keys.rightArrowKey.isPressed ? 1 : 0) - (keys.aKey.isPressed || keys.leftArrowKey.isPressed ? 1 : 0);
                movement.y = (keys.wKey.isPressed || keys.upArrowKey.isPressed ? 1 : 0) - (keys.sKey.isPressed || keys.downArrowKey.isPressed ? 1 : 0);
                sprint = keys.leftShiftKey.isPressed || keys.rightShiftKey.isPressed;
                escape = keys.escapeKey.wasPressedThisFrame;
                reset = keys.digit1Key.wasPressedThisFrame;
                lobby = keys.digit2Key.wasPressedThisFrame;
                help = keys.hKey.wasPressedThisFrame;
            }
            var pointer = Mouse.current;
            if (pointer != null) { mouse = pointer.delta.ReadValue() * 0.12f; click = pointer.leftButton.wasPressedThisFrame; }
#else
            movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 2;
            sprint = Input.GetKey(KeyCode.LeftShift);
            escape = Input.GetKeyDown(KeyCode.Escape);
            click = Input.GetMouseButtonDown(0);
            reset = Input.GetKeyDown(KeyCode.Alpha1);
            lobby = Input.GetKeyDown(KeyCode.Alpha2);
            help = Input.GetKeyDown(KeyCode.H);
#endif
            if (escape) SetCursor(Cursor.lockState != CursorLockMode.Locked);
            if (click && Cursor.lockState != CursorLockMode.Locked) SetCursor(true);
            if (help) showHelp = !showHelp;
            if (reset) Teleport(entranceSpawn, 180);
            if (lobby) Teleport(lobbySpawn, 180);
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                yaw += mouse.x;
                pitch = Mathf.Clamp(pitch - mouse.y, -75, 80);
                transform.rotation = Quaternion.Euler(0, yaw, 0);
            }
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
            verticalSpeed = Mathf.Max(verticalSpeed - 22 * Time.deltaTime, -40);
            Vector3 direction = transform.TransformDirection(new Vector3(movement.x, 0, movement.y).normalized);
            controller.Move((direction * (sprint ? runSpeed : walkSpeed) + Vector3.up * verticalSpeed) * Time.deltaTime);
            if (transform.position.y < -5) Teleport(entranceSpawn, 180);
            if (viewCamera != null)
            {
                viewCamera.transform.position = transform.position + Vector3.up * eyeHeight;
                viewCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            }
        }

        public void Teleport(Vector3 position, float heading)
        {
            Body.enabled = false;
            transform.position = position;
            yaw = heading;
            pitch = 0;
            verticalSpeed = 0;
            transform.rotation = Quaternion.Euler(0, heading, 0);
            Body.enabled = true;
            Physics.SyncTransforms();
        }

        private static void SetCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void OnDisable() { if (!Application.isBatchMode) SetCursor(false); }

        private void OnGUI()
        {
            if (!showHelp || externalControl || Application.isBatchMode) return;
            GUI.color = new Color(0.04f, 0.17f, 0.19f, 0.92f);
            GUI.Box(new Rect(20, 20, 455, 99), GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(34, 28, 425, 24), "UEH  /  CƠ SỞ B  ·  TÒA TRUNG TÂM & SẢNH B1");
            GUI.Label(new Rect(34, 52, 425, 24), "Bản tái dựng với kích thước ước lượng theo ảnh tour.");
            GUI.Label(new Rect(34, 76, 425, 30), "WASD: đi  ·  Shift: chạy  ·  Chuột: nhìn  ·  Esc: con trỏ");
            GUI.Label(new Rect(26, Screen.height - 36, 650, 25), "1: ngoài cổng   /   2: giữa sảnh   /   H: ẩn hướng dẫn");
        }
    }
}
