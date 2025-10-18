using UnityEngine;
using System.Collections;

public class Drawer : Interactable
{
    [SerializeField]
    private GameObject drawerObject;
    private bool drawerOpen;

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
        if (!canInteract) return;

        canInteract = false;

        Debug.Log("You interacted with the drawer");

        drawerOpen = !drawerOpen;
        drawerObject.GetComponent<Animator>().SetBool("isOpen", drawerOpen);

        StartCoroutine(ResetInteractCooldown());
    }

    private IEnumerator ResetInteractCooldown()
    {
        yield return new WaitForSeconds(interactCooldownSeconds);
        canInteract = true;
    }
}
