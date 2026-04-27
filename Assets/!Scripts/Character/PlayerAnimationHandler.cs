using UnityEngine;

// PlayerAnimationHandler
// Computes and applies animation parameters and subtle model rotation based on
// the player's movement velocity and grounded state.
public class PlayerAnimationHandler : MonoBehaviour
{
    [SerializeField] SpringArmComponent springArm;
    [SerializeField, Range(1f, 50f)] float rotationSpeed;
    PlayerController controller;
    Animator animator;
    Quaternion targetRotation;
    string blendParam = "velocityHandle";
    string groundedParam = "isGrounded";

    void Start()
    {
        controller ??= GetComponentInParent<PlayerController>();
        animator ??= GetComponent<Animator>();
    }

    void Update()
    {
        // Compute procedural pose/parameters each frame
        ComputePoses();
    }

    void LateUpdate()
    {
        // Apply rotation smoothly after pose computation
        ApplyPoses();
    }

    void ComputePoses()
    {
        Vector3 velocity = controller.GetVelocity();
        bool isGrounded = controller.IsGrounded;
        ComputeRotation(ref velocity);
        UpdateAnimation(ref velocity , ref isGrounded);
    }

    // Update target rotation based on movement direction projected on the local up-axis.
    void ComputeRotation(ref Vector3 velocity)
    {
        if (velocity.magnitude > 0.1f)
        {
            Vector3 localUp = transform.parent.up;
            Vector3 projectedForward = Vector3.ProjectOnPlane(velocity, localUp);

            if (projectedForward != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(projectedForward, localUp);
            }
        }
    }

    // Update parameters used by the Animator
    void UpdateAnimation(ref Vector3 velocity, ref bool isGrounded)
    {
        animator.SetFloat(blendParam, velocity.sqrMagnitude);
        animator.SetBool(groundedParam, isGrounded);
    }

    // Smoothly interpolate the transform rotation towards the target.
    void ApplyPoses()
    { 
        this.transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
