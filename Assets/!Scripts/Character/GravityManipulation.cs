using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// GravityManipulation
// Top-level component that coordinates the player gravity manipulation feature.
// It implements a simple state machine (NONE, VIEW, MANIP) and exposes helper
// methods used by the state implementations. This component also caches
// references to PlayerController, Camera and SpringArmComponent.
public partial class GravityManipulation : MonoBehaviour
{
    [field: SerializeField]
    public Transform ViewSrc
    {
        get; private set;
    }
    [SerializeField] float idealPointDistance;

    [SerializeField] NoneStateData noneStateData;
    [SerializeField] ViewStateData viewStateData;
    [SerializeField] ManipStateData manipStateData;

    IState currentState;
    readonly Dictionary<Type, IState> stateCache = new Dictionary<Type, IState>();
    public PlayerController playerController
    {
        get; private set;   
    }

    public SpringArmComponent springArm
    {
        get; private set;
    }

    public Camera mainCamera
    {
        get; private set;   
    }

    bool enableUpdationCheck = true;
    // Target position and rotation used by MANIP state
    public Vector3 targetPosition;
    public Quaternion targetRotation;

    void OnEnable()
    {
    }

    void Start()
    {
        Init();
        InitStateReg();
        SetState(StateType.NONE);
    }

    void Init()
    { 
        playerController ??= this.GetComponent<PlayerController>();
        springArm ??= this.GetComponentInChildren<SpringArmComponent>();
        mainCamera = Camera.main;
    }

    void Update()
    {   
        if (currentState == null || !enableUpdationCheck) return;

        currentState.OnUpdate();
        var next = currentState.OnTransitionCheck();
        if (next != currentState.Type)
        {
            SetState(next);
        }
    }

    private void InitStateReg()
    {
        stateCache[typeof(NoneState)] = new NoneState(this , noneStateData);
        stateCache[typeof(ViewState)] = new ViewState(this , viewStateData);
        stateCache[typeof(ManipState)] = new ManipState(this , manipStateData);
    }

    private IState GetState(StateType type)
    {
        var stateType = type switch
        {
            StateType.NONE => typeof(NoneState),
            StateType.VIEW => typeof(ViewState),
            StateType.MANIP => typeof(ManipState),
            _ => typeof(NoneState),
        };

        stateCache.TryGetValue(stateType, out var cached);
        return cached;
    }

    private void SetState(StateType next)
    {
        if (currentState != null)
        {
            enableUpdationCheck = false;
            currentState.OnExit(next, OnExitCompletion);
        }
        else 
        {
            OnExitCompletion();
        }

        void OnExitCompletion()
        {
            currentState = GetState(next);
            currentState.OnEnter(OnEnterCompletion);
        }

        void OnEnterCompletion()
        {
            enableUpdationCheck = true;
        }
    }

    // Compute an ideal casting point for the view selection logic.
    public Vector3 GetIdealCastPoint()
    {
        RaycastHit lastGroundHit = playerController.GetLastGroundCheckHit();
        BoxCollider groundCollider = (BoxCollider)lastGroundHit.collider;
        Vector3 idealPoint = groundCollider.transform.position;
        idealPoint += lastGroundHit.normal * ((groundCollider.transform.localScale.y * groundCollider.size.y) / 1.95f);
        idealPoint += lastGroundHit.normal * idealPointDistance;
        return idealPoint;
    }

    void OnDisable()
    {
    }
}

