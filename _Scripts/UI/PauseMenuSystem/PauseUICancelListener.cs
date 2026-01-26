using UnityEngine;
using UnityEngine.InputSystem;

public class PauseUICancelListener : MonoBehaviour
{
    [SerializeField] private PauseMenuManager pauseMenuManager;
    [SerializeField] private InputActionReference cancelAction;

    private void OnEnable()
    {
        if (cancelAction == null) return;

        cancelAction.action.Enable();
        cancelAction.action.performed += OnCancel;
    }

    private void OnDisable()
    {
        if (cancelAction == null) return;

        cancelAction.action.performed -= OnCancel;
        cancelAction.action.Disable();
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        pauseMenuManager.Cancel();
    }
}
