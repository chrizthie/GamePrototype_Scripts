using UnityEngine;

public class BlockAheadDetection : MonoBehaviour
{
    [Header("Detection Parameters")]
    [SerializeField] public bool blocked;
    [SerializeField] private float rayDistance = 0.4f;      // how far forward to check
    [SerializeField] private LayerMask hitMask;           // which layers the ray can hit

    private void Update()
    {
        Vector3 bodyOffset = new Vector3(0, 0.5f, 0); // adjust Y for chest height
        Vector3 origin = transform.position + bodyOffset;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance, hitMask))
        {
            blocked = true;
            Debug.Log("Blocked by: " + hit.collider.name);
        }
        else
        {
            blocked = false;
        }

        Debug.DrawRay(origin, direction * rayDistance, Color.green);
    }
}
