using System;
using System.Threading.Tasks;
using UnityEngine;

// NoneState: default state when player is in normal control mode.
// Watches for input to transition into the VIEW state. When exiting this
// state it retargets the camera smoothly towards the view source transform.
public partial class GravityManipulation
{
    private class NoneState : IState
    {
        private GravityManipulation owner;
        private NoneStateData data;
        public NoneState(GravityManipulation owner , NoneStateData data) { this.owner = owner; this.data = data; }
        public StateType Type => StateType.NONE;
        public async void OnEnter(Action onCompletion = null)
        {
            onCompletion?.Invoke();
        }
        public void OnUpdate()
        {
            // No per-frame behavior in the default state
        }
        public StateType OnTransitionCheck()
        {
            Debug.Log("Checking for Transition");
            if (Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.RightArrow) ||
                Input.GetKeyDown(KeyCode.LeftArrow))
            {
                return StateType.VIEW;
            }
            return Type;
        }
        public async void OnExit(StateType next, Action onCompletion = null)
        {
            Debug.Log("Entered OnExit");
            owner.springArm.enabled = false;
            owner.playerController.isPossesed = false;
            await ReTargetCamera();
            Debug.Log("Completed Retarget");
            onCompletion?.Invoke();
        }

        // Smoothly move the main camera to the view source pose
        async Task ReTargetCamera()
        {
            Debug.Log("Retargeting Cam");
            Vector3 viewPosition = owner.ViewSrc.position;
            Quaternion viewRotation = owner.ViewSrc.rotation;

            Camera cam = owner.mainCamera;
            
            Vector3 currentPosition = cam.transform.position;
            Quaternion currentRotation = cam.transform.rotation;

            float t = 0;

            Debug.Log("Started Ret Cam");
            while (t < data.RetargetDuration)
            { 
                t += Time.deltaTime;
                cam.transform.position = Vector3.Lerp(currentPosition, viewPosition, t / data.RetargetDuration);
                cam.transform.rotation = Quaternion.Slerp(currentRotation, viewRotation, t / data.RetargetDuration);
                await Task.Yield();
            }

            cam.transform.position = viewPosition;
            cam.transform.rotation = viewRotation;
        }

    }
}

[Serializable]
public struct NoneStateData
{
    // Duration used when retargeting camera during state exit
    public float RetargetDuration;
}
