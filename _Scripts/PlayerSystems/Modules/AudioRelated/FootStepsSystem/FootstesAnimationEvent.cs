using UnityEngine;

public class FootstepAnimationEvents : MonoBehaviour
{
    [SerializeField] private FootstepsHandler footstepsHandler;

    public void LeftFootstep()
    {
        footstepsHandler.PlayFootStepAudio();
    }

    public void RightFootstep()
    {
        footstepsHandler.PlayFootStepAudio();
    }
}