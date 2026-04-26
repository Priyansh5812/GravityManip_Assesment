using UnityEngine;

public class SpringArmComponent : MonoBehaviour
{
    Camera mainCamera;
    [Header("Position")]
    [SerializeField] float m_raycastDistance = 7.0f;
    [SerializeField] Vector3 originOffset = Vector3.up;
    [SerializeField] Vector3 direction = Vector3.up - Vector3.forward;
    [SerializeField] LayerMask layersToIgnore;
    [Header("Rotation")]
    [SerializeField] Vector3 EulerOffset;
    [Header("Input")]
    [SerializeField] float XSens;
    [SerializeField] float YSens;
    [SerializeField] float globalSensMultiplier = 5f;
    [SerializeField, Range(0f , 90f)] float minMaxXAngle;
    float xInput, yInput;
    Vector3 origin;
    Vector3 rayDirection;
    Vector3 position;
    Quaternion rotation;
    float currentXRotation = 0f;
    RaycastHit[] hitsInfo = new RaycastHit[1];
    private void Start()
    {
        Initialize();
    }

    void Initialize()
    { 
        mainCamera ??= Camera.main;
        Cursor.lockState = CursorLockMode.Locked;   
    }

    void AddInput()
    {
        xInput = Input.GetAxis("Mouse X");
        yInput = Input.GetAxis("Mouse Y");
        Vector3 rotation = this.transform.localRotation.eulerAngles;
        float xStep = -yInput * XSens * globalSensMultiplier * Time.deltaTime;
        float yStep = xInput * YSens * globalSensMultiplier * Time.deltaTime;
        currentXRotation += xStep;
        currentXRotation = Mathf.Clamp(currentXRotation, -minMaxXAngle, minMaxXAngle);

        float currentYRotation = transform.localEulerAngles.y + yStep;

        transform.localRotation = Quaternion.Euler(currentXRotation, currentYRotation, 0f);
    }


    void ComputeRayParams()
    {
        origin = this.transform.position;
        origin += (this.transform.forward * originOffset.z) + (this.transform.up * originOffset.y) + (this.transform.right * originOffset.x);
        rayDirection = this.transform.rotation * direction;
    }

    void ComputePosition()
    {
        float camDistance = m_raycastDistance;
        Vector3 normDir = rayDirection.normalized;
        LayerMask mask = 0;
        mask = mask | layersToIgnore;
        mask = ~mask;
        if (Physics.SphereCastNonAlloc(origin, 0.25f , normDir, hitsInfo , m_raycastDistance, mask) > 0)
        {
            RaycastHit hitInfo = hitsInfo[0];
            camDistance = hitInfo.distance;
        }
        position = origin + (normDir * camDistance);
    }

    void ComputeRotation()
    {   
        rotation = Quaternion.LookRotation(-rayDirection, this.transform.up);
        rotation *= Quaternion.Euler(EulerOffset);
    }

    void ApplyPose()
    {   
        mainCamera.transform.position = position;
        mainCamera.transform.rotation = rotation;
    }

    public (Vector3, Quaternion) GetComputedPose()
    { 
        ComputePosition();
        ComputeRotation();
        return (position, rotation);
    }

    private void Update()
    {
        AddInput();
        ComputeRayParams();
        ComputeRotation();
        ComputePosition();
        ApplyPose();
        Debug.DrawLine(origin, position);
    }







}
