using UnityEngine;

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
        ComputePoses();
    }

    
    void LateUpdate()
    {
        ApplyPoses();
    }

    void ComputePoses()
    {
        Vector3 velocity = controller.GetVelocity();
        bool isGrounded = controller.IsGrounded;
        ComputeRotation(ref velocity);
        UpdateAnimation(ref velocity , ref isGrounded);
    }

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

    void UpdateAnimation(ref Vector3 velocity, ref bool isGrounded)
    {
        animator.SetFloat(blendParam, velocity.sqrMagnitude);
        animator.SetBool(groundedParam, isGrounded);
    }

    void ApplyPoses()
    { 
        this.transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed); ;
    }
}
