using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AmbientZone : MonoBehaviour
{
    [Header("Ambient Profile")]
    [SerializeField] private AmbientProfile ambientProfile;

    public AmbientProfile AmbientProfile => ambientProfile;

    private void Reset()
    {
        Collider zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (ambientProfile == null)
        {
            AmbientAudioController controller =
                FindFirstObjectByType<AmbientAudioController>();

            if (controller != null)
            {
                controller.ClearAmbient();
            }

            return;
        }

        AmbientAudioController audioController =
            FindFirstObjectByType<AmbientAudioController>();

        if (audioController != null)
        {
            audioController.EnterZone(this, ambientProfile);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        AmbientAudioController audioController =
            FindFirstObjectByType<AmbientAudioController>();

        if (audioController != null)
        {
            audioController.ExitZone(this);
        }
    }

    private void OnDrawGizmos()
    {
        Collider zoneCollider = GetComponent<Collider>();

        if (zoneCollider == null)
            return;

        Gizmos.color = new Color(
            0.2f,
            0.7f,
            1f,
            0.25f
        );

        Gizmos.DrawCube(
            zoneCollider.bounds.center,
            zoneCollider.bounds.size
        );

        Gizmos.color = new Color(
            0.2f,
            0.7f,
            1f,
            0.8f
        );

        Gizmos.DrawWireCube(
            zoneCollider.bounds.center,
            zoneCollider.bounds.size
        );
    }
}