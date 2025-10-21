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
    [SerializeField] private float distance = 1.2f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private PlayerUI playerUI;

    [Header("Required Components")]
    [SerializeField] private GameObject circleCrosshair;
    [SerializeField] private GameObject handCrosshair;
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

        //create a ray at the center of the camera, shooting outwards
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * distance);

        //variable to score our collision information
        RaycastHit hitInfo;

        // check if our ray collides with anything on our interactable layer
        if (Physics.Raycast(ray, out hitInfo, distance, interactableMask))
        {
            if (hitInfo.collider.GetComponent<Interactable>() != null)
            {
                Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
                playerUI.UpdateText(interactable.promptMessage);
                if (inputManager.playerInputs.Player.Interact.triggered)
                {
                    interactable.BaseInteract();
                }
                interactableDetected = true;
            }
        }

        // change crosshair based on whether an interactable is detected
        if (interactableDetected)
        {
            circleCrosshair.SetActive(false);
            handCrosshair.SetActive(true);
        }
        else
        {
            circleCrosshair.SetActive(true);
            handCrosshair.SetActive(false);
        }
    }
}
