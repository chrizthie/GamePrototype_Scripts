using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interact Parameters")]
    [SerializeField] public bool interactableDetected;
    [SerializeField] private bool readableDetected;
    [SerializeField] public Readable currentReadable;
    public Readable CurrentReadable { get; private set; }
    [SerializeField] private float distance = 1.2f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private PlayerUI playerUI;
    private Collider currentReadableCollider;

    [Header("Crosshairs")]
    [SerializeField] private CanvasGroup circleCrosshairCanvasGroup;
    [SerializeField] private CanvasGroup handCrosshairCanvasGroup;
    [SerializeField] private CanvasGroup readCrosshairCanvasGroup;

    [Header("Crosshair Fade")]
    [SerializeField] private float crosshairFadeSpeed = 10f;

    [Header("Required Components")]
    [SerializeField] Camera mainCamera;
    [SerializeField] InputManager inputManager;

    void Start()
    {
        playerUI = GetComponent<PlayerUI>();
    }

    void Update()
    {
        playerUI.UpdateText(string.Empty);

        interactableDetected = false;
        readableDetected = false;
        currentReadable = null;
        currentReadableCollider = null;

        //create a ray at the center of the camera, shooting outwards
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * distance);

        //variable to score our collision information
        RaycastHit hitInfo;

        // check if our ray collides with anything on our interactable layer
        if (Physics.Raycast(ray, out hitInfo, distance, interactableMask))
        {
            // Check Readable first
            Readable readable = hitInfo.collider.GetComponent<Readable>();

            if (readable != null)
            {
                playerUI.UpdateText(readable.promptMessage);

                interactableDetected = true;
                readableDetected = true;
                currentReadable = readable;
                currentReadableCollider = hitInfo.collider;
            }
            else
            {
                // Normal Interactable
                Interactable interactable = hitInfo.collider.GetComponentInParent<Interactable>();

                if (interactable != null)
                {
                    playerUI.UpdateText(interactable.promptMessage);

                    if (inputManager.playerInputs.Gameplay.Interact.triggered)
                    {
                        interactable.BaseInteract();
                    }

                    interactableDetected = true;
                }
            }
        }

        UpdateCrosshairs();
    }

    private void UpdateCrosshairs()
    {
        bool isZooming = InputManager.instance.CurrentInput.zoom;

        float circleTargetAlpha = 0f;
        float handTargetAlpha = 0f;
        float readTargetAlpha = 0f;

        // READING
        if (readableDetected)
        {
            // Hide readable crosshair while holding RMB
            if (!isZooming)
            {
                readTargetAlpha = 1f;
            }
        }

        // NORMAL INTERACTABLE
        else if (interactableDetected)
        {
            handTargetAlpha = 1f;
        }

        // NOTHING DETECTED
        else
        {
            circleTargetAlpha = 1f;
        }

        // Smoothly fade each crosshair
        circleCrosshairCanvasGroup.alpha = Mathf.Lerp(
            circleCrosshairCanvasGroup.alpha,
            circleTargetAlpha,
            crosshairFadeSpeed * Time.deltaTime
        );

        handCrosshairCanvasGroup.alpha = Mathf.Lerp(
            handCrosshairCanvasGroup.alpha,
            handTargetAlpha,
            crosshairFadeSpeed * Time.deltaTime
        );

        readCrosshairCanvasGroup.alpha = Mathf.Lerp(
            readCrosshairCanvasGroup.alpha,
            readTargetAlpha,
            crosshairFadeSpeed * Time.deltaTime
        );
    }

    public bool TryGetReadableCenter(Readable readable, out Vector3 center)
    {
        center = Vector3.zero;

        if (readable == null)
            return false;

        Collider readableCollider =
            readable.GetComponent<Collider>();

        if (readableCollider == null)
        {
            readableCollider =
                readable.GetComponentInChildren<Collider>();
        }

        if (readableCollider == null)
            return false;

        center = readableCollider.bounds.center;

        return true;
    }
}
