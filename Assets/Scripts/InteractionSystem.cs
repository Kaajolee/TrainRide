using System;
using PlayerInputActionsNamespace;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Raycasts from the camera each frame, tracking whatever IInteractable the player is looking at.
/// Fires events on hover/unhover and when the interact action is pressed.
///
/// Input: expects a "Player/Interact" action on the existing PlayerInputActions asset.
///        If you haven't added one, the script falls back to a configurable key (default: E).
///
/// Usage:
///   interaction.OnHover    += target => uiController.ShowHint(target?.Label);
///   interaction.OnInteract += target => Debug.Log($"Clicked {target.Label}");
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    [Tooltip("Raycast origin. Defaults to Camera.main if null.")]
    public Camera sourceCamera;

    [Tooltip("Maximum interaction distance (meters).")]
    public float maxDistance = 3.5f;

    [Tooltip("Layers considered for interaction. Set to only the 'Interactable' layer to keep it cheap.")]
    public LayerMask interactableLayers = ~0;

    [Tooltip("Fallback key if no Interact input action is bound.")]
    public KeyCode fallbackInteractKey = KeyCode.E;

    public event Action<IInteractable> OnHover;
    public event Action<IInteractable> OnInteract;

    private IInteractable current;
    private PlayerInputActions inputActions;
    private InputAction interactAction;

    void Awake()
    {
        if (sourceCamera == null) sourceCamera = Camera.main;
        inputActions = new PlayerInputActions();

        // The starter PlayerInputActions may or may not include an "Interact" action.
        // FindAction(..., throwIfNotFound: false) is a safe probe.
        interactAction = inputActions.asset.FindAction("Player/Interact", throwIfNotFound: false);
    }

    void OnEnable()
    {
        inputActions.Enable();
        if (interactAction != null) interactAction.performed += HandleInteractPerformed;
    }

    void OnDisable()
    {
        if (interactAction != null) interactAction.performed -= HandleInteractPerformed;
        inputActions.Disable();
    }

    void Update()
    {
        UpdateHover();

        // Fallback input path — only used when no InputAction is available.
        if (interactAction == null && Input.GetKeyDown(fallbackInteractKey))
            TryInteract();
    }

    void UpdateHover()
    {
        if (sourceCamera == null) return;

        Ray ray = new Ray(sourceCamera.transform.position, sourceCamera.transform.forward);
        IInteractable hit = null;

        if (Physics.Raycast(ray, out RaycastHit info, maxDistance, interactableLayers, QueryTriggerInteraction.Collide))
            hit = info.collider.GetComponentInParent<IInteractable>();

        if (!ReferenceEquals(hit, current))
        {
            current = hit;
            OnHover?.Invoke(current);
        }
    }

    void HandleInteractPerformed(InputAction.CallbackContext _) => TryInteract();

    void TryInteract()
    {
        if (current == null) return;
        current.Interact();
        OnInteract?.Invoke(current);
    }
}
