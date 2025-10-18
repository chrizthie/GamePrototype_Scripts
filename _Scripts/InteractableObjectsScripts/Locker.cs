using UnityEngine;
using System.Collections;

public class Locker : Interactable
{
    [SerializeField]
    private GameObject lockerObject;
    private bool lockerOpen;

    [SerializeField]
    private float interactCooldownSeconds = 0.75f; // Set this in the Inspector

    private bool canInteract = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    // This method is called when the player interacts with the drawer
    protected override void Interact()
    {
        if (!canInteract)
            return;

        canInteract = false;

        Debug.Log("You interacted with the locker");

        lockerOpen = !lockerOpen;
        lockerObject.GetComponent<Animator>().SetBool("isOpen", lockerOpen);

        StartCoroutine(ResetInteractCooldown());
    }

    private IEnumerator ResetInteractCooldown()
    {
        yield return new WaitForSeconds(interactCooldownSeconds);
        canInteract = true;
    }
}
