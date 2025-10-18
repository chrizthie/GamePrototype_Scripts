using UnityEngine;
using UnityEngine.Animations.Rigging;

public class FlashlightIKHandler : MonoBehaviour
{
    public Animator animator;
    public Rig leftArmRig;
    public string flashlightIdleStateName = "Flashlight Idle";
    public int flashlightLayerIndex = 1;
    public float blendSpeed = 5f; // how fast to blend

    void Update()
    {
        if (animator == null || leftArmRig == null)
            return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(flashlightLayerIndex);

        bool inIdle = stateInfo.IsName(flashlightIdleStateName);

        // Smooth blend between 0 and 1
        float targetWeight = inIdle ? 1f : 0f;
        leftArmRig.weight = Mathf.MoveTowards(leftArmRig.weight, targetWeight, Time.deltaTime * blendSpeed);
    }
}