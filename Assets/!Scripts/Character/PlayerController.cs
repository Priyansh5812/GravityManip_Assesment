using UnityEngine;

// PlayerController
// Responsible for low-level character physics, movement, jumping and ground checks.
// The controller computes a desired linear velocity and writes it to the Rigidbody
// each FixedUpdate. It also responds to global game start/end events to enable
// or disable player input.
[RequireComponent(typeof(Rigidbody) , typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    CapsuleCollider capsuleCollision;
    [SerializeField, Range(0f, 2f)] float radius; 
    [SerializeField, Range(0f, 2f)] float height;

    [Header("Base Movement")]
    [SerializeField] Transform OrientationReference;
    [SerializeField] float maxSpeed;
    [SerializeField] float accelarationConstant;
    [SerializeField, Range(0f , 0.9f)] float deaccelarationConstant;
    float currentSpeed;
    float lastSpeed;
    Vector3 currentVelocity = Vector3.zero;
    Vector3 motionVector = Vector3.zero; // Apply it in localSpace
    [SerializeField] float airModifier;

    [Header("Jump")]
    bool queueJump = false;
    [SerializeField] float maxJumpSpeed;
    [SerializeField] float JumpSpeedDecrementFactor;
    float remainingJumpSpeed;
    
    [Header("Ground Probing")]
    [SerializeField] LayerMask groundMask;
    [field: SerializeField]
    public bool IsGrounded
    {
        get; private set;
    }
    [SerializeField] float groundCheckOffset = 0.1f;
    [SerializeField] float groundCheckRangeOffset = 0.01f;
    [SerializeField] float groundCheckCapsuleRadiusModifier = 1.0f;
    RaycastHit lastGroundCheckHit;
    RaycastHit[] groundCheckInfo = new RaycastHit[1];
    private Collider groundCollider;

    [Header("Gravity")]
    [SerializeField] float maxGravity;
    [SerializeField] float gravityAccumulation;
    float currentGravity;

    // Whether the player currently receives input (controlled externally)
    public bool isPossesed = true;

    private void OnEnable()
    {
        EventManager.OnGameStarted.AddListener(SetGameStartState);
        EventManager.OnGameEnded.AddListener(SetGameEndState);
    }

    private void Start()
    {
        ValidateCapsuleBounds();
    }

    void ValidateCapsuleBounds()
    { 
        rb ??= this.GetComponent<Rigidbody>();
        capsuleCollision ??= this.GetComponent<CapsuleCollider>();
        capsuleCollision.height = height;
        capsuleCollision.radius = radius;
    }

    void GetCapsuleBounds(out Vector3 bottomHemisphereCenter , out Vector3 topHemisphereCenter, out Vector3 worldCenter)
    {   
        worldCenter = this.transform.position + this.transform.rotation * capsuleCollision.center;
        bottomHemisphereCenter = worldCenter + this.transform.rotation * (Vector3.down * ((height / 2) - radius));
        topHemisphereCenter = worldCenter + this.transform.rotation * (Vector3.up * ((height/2)-radius));
    }

    void Update()
    {   
        ReceiveInput();
    }
    
    private void FixedUpdate()
    {
        ComputeGroundCheck();
        ComputeVelocity();
    }

    void ReceiveInput()
    {
        motionVector.x = isPossesed ? Input.GetAxisRaw("Horizontal") : 0;
        motionVector.z = isPossesed ? Input.GetAxisRaw("Vertical") : 0;
        queueJump = isPossesed ? (IsGrounded && Input.GetKeyDown(KeyCode.Space)) : false;
        SetJumpState();
        motionVector.Normalize();
    }

    void SetJumpState()
    {
        if (!queueJump)
            return;

        // Lift the character slightly so the physics will treat it as airborne
        this.transform.position += this.transform.up * (groundCheckOffset + groundCheckRangeOffset + 0.05f);
        remainingJumpSpeed = maxJumpSpeed;
        queueJump = false;
    }

    // Perform a capsule probe to determine grounded state and relevant hit info
    void ComputeGroundCheck()
    {
        Vector3 bottom, top;
        GetCapsuleBounds(out bottom, out top, out _);
        bottom += this.transform.rotation * Vector3.up * groundCheckOffset;
        top += this.transform.rotation * Vector3.up * groundCheckOffset;
        if (Physics.CapsuleCastNonAlloc(bottom,
            top,
            radius + groundCheckCapsuleRadiusModifier,
            this.transform.rotation * Vector3.down,
            groundCheckInfo,
            groundCheckOffset + groundCheckRangeOffset,
            groundMask) > 0)
        {
            IsGrounded = true;
            remainingJumpSpeed = 0.0f;
        }
        else 
        {
            IsGrounded = false;
        }

        lastGroundCheckHit = groundCheckInfo[0];
    }

    // Compose current velocity from gravity, jump and locomotion and write to rigidbody
    void ComputeVelocity()
    {
        currentVelocity = Vector3.zero;
        ApplyGravity(ref currentVelocity);
        ApplyJumpForces(ref currentVelocity);
        ApplyLocomotion(ref currentVelocity);
        rb.linearVelocity = currentVelocity;
    }

    void ApplyGravity(ref Vector3 currentVelocity)
    {
        if (IsGrounded)
        {
            currentGravity = 0.0f;
        }
        else 
        {
            currentGravity += gravityAccumulation * Time.fixedDeltaTime;
            currentGravity = Mathf.Clamp(currentGravity, 0.0f, maxGravity);
        }

        Vector3 gravityDirection = this.transform.rotation * Vector3.down;

        currentVelocity += gravityDirection * currentGravity * Time.fixedDeltaTime;

        if (remainingJumpSpeed > 0)
        {
            currentVelocity += -gravityDirection * remainingJumpSpeed * Time.fixedDeltaTime;
            remainingJumpSpeed -= Time.fixedDeltaTime;
            remainingJumpSpeed = Mathf.Clamp(remainingJumpSpeed, 0, maxJumpSpeed);
        }
    }

    void ApplyJumpForces(ref Vector3 currentVelocity)
    {
        if (remainingJumpSpeed <= 0)
            return;

        currentVelocity += this.transform.up * remainingJumpSpeed * Time.fixedDeltaTime;
        remainingJumpSpeed -= JumpSpeedDecrementFactor * Time.fixedDeltaTime;
    }

    void CalculateBaseMovementSpeed()
    {
        if (motionVector.sqrMagnitude > 0)
            currentSpeed = lastSpeed + accelarationConstant * Time.fixedDeltaTime;
        else
            currentSpeed *= deaccelarationConstant * Time.fixedDeltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0.0f, maxSpeed);
        lastSpeed = currentSpeed;
    }

    void ApplyLocomotion(ref Vector3 currentVelocity)
    {
        CalculateBaseMovementSpeed();
        Vector3 velocity = motionVector * currentSpeed ;
        velocity = OrientationReference.rotation * velocity;
        if(IsGrounded)
            velocity = Vector3.ProjectOnPlane(velocity, lastGroundCheckHit.normal); // no slope projection
        currentVelocity += velocity; 
    }

    void SetGameStartState()
    {   
        isPossesed = true;
    }
    void SetGameEndState()
    {
        isPossesed = false;
    }

    public RaycastHit GetLastGroundCheckHit() => this.lastGroundCheckHit;
    public Vector3 GetVelocity() => currentVelocity;

    private void OnDrawGizmos()
    {
        Vector3 bottom, top, worldCenter;
        GetCapsuleBounds(out bottom, out top, out worldCenter);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(worldCenter, 0.15f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bottom, 0.15f);
        Gizmos.DrawWireSphere(top, 0.15f);
        Gizmos.color = Color.blueViolet;
        Gizmos.DrawRay(this.transform.position, currentVelocity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CollectibleIdentifier>(out var comp))
        {
            comp.SetAsCollected();
            return;
        }

        if (other.TryGetComponent<OutOfBounds>(out var comp_1))
        {
            EventManager.OnGameEnded.Invoke();
            return;
        }
    }

    private void OnValidate()
    {
        ValidateCapsuleBounds();
    }

    private void OnDisable()
    {
        EventManager.OnGameStarted.RemoveListener(SetGameStartState);
        EventManager.OnGameEnded.RemoveListener(SetGameEndState);
    }
}
