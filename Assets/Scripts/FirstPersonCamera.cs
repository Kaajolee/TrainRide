using PlayerInputActionsNamespace;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Look")]
    public float mouseSensitivity = 0.15f;
    public float minPitch = -85f;
    public float maxPitch = 85f;

    [Header("Neck Lock")]
    public bool useNeckLock = false;
    [Tooltip("Reference the body/root transform. Yaw is clamped relative to it.")]
    public Transform body;
    [Tooltip("How far the neck can twist left/right from the body's forward, in degrees.")]
    public float maxNeckYaw = 70f;

    [Header("Cursor")]
    public bool lockCursorOnStart = true;
    public KeyCode unlockCursorKey = KeyCode.Escape;

    private PlayerInputActions inputActions;
    private Vector2 lookInput;

    private float pitch;
    private float yaw;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;
    }

    void OnDisable()
    {
        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLook;
        inputActions.Disable();
    }

    void Start()
    {
        Vector3 e = transform.localEulerAngles;
        pitch = NormalizeAngle(e.x);
        yaw = NormalizeAngle(e.y);

        if (lockCursorOnStart)
            SetCursorLocked(true);
    }

    void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        HandleCursor();

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        yaw += lookInput.x * mouseSensitivity;
        pitch -= lookInput.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (useNeckLock && body != null)
        {
            float bodyYaw = body.eulerAngles.y;
            float minYaw = bodyYaw - maxNeckYaw;
            float maxYaw = bodyYaw + maxNeckYaw;
            yaw = ClampAngle(yaw, minYaw, maxYaw);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    void HandleCursor()
    {
        if (Input.GetKeyDown(unlockCursorKey))
            SetCursorLocked(false);
        else if (Cursor.lockState != CursorLockMode.Locked && Input.GetMouseButtonDown(0))
            SetCursorLocked(true);
    }

    void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    static float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a > 180f) a -= 360f;
        return a;
    }

    static float ClampAngle(float angle, float min, float max)
    {
        float delta = Mathf.DeltaAngle(min + (max - min) * 0.5f, angle);
        float half = (max - min) * 0.5f;
        delta = Mathf.Clamp(delta, -half, half);
        return min + (max - min) * 0.5f + delta;
    }
}
