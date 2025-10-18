using UnityEngine;

public class SmoothIKFollow : MonoBehaviour
{
    public Transform target;     // The object you want to follow (e.g., flashlight grip)
    public float positionSmoothSpeed = 10f;
    public float rotationSmoothSpeed = 10f;

    void LateUpdate()
    {
        if (target == null)
            return;

        // Smoothly move position
        transform.position = Vector3.Lerp(
            transform.position,
            target.position,
            Time.deltaTime * positionSmoothSpeed
        );

        // Smoothly rotate
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            target.rotation,
            Time.deltaTime * rotationSmoothSpeed
        );
    }
}