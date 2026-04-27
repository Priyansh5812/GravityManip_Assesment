using System;
using UnityEngine;

// ManipState: responsible for smoothly moving the player from the view
// selection pose into the final manipulation position. During this state
// player control is disabled and the camera spring arm is enabled.
public partial class GravityManipulation
{
    private class ManipState : IState
    {
        private GravityManipulation owner;
        private ManipStateData data;
        public ManipState(GravityManipulation owner , ManipStateData data) {this.owner = owner; this.data = data; }
        public StateType Type => StateType.MANIP;
        bool canTransition = false;
        float t;
        Vector3 currentPosition, targetPosition;
        Quaternion currentRotation, targetRotation;

        // Enter the manipulation state and prepare for interpolation
        public void OnEnter(Action onCompletion = null)
        {
            Debug.Log("Entered MANIP state");
            PrepareStartup();
            onCompletion?.Invoke();
        }

        void PrepareStartup()
        { 
            canTransition = false;
            
            owner.springArm.enabled = true;
            owner.playerController.isPossesed = false;
            currentPosition = owner.transform.position;
            currentRotation = owner.transform.rotation;
            targetPosition = owner.targetPosition;
            targetRotation = owner.targetRotation;

            t = 0;   
        }

        // Lerp position/rotation over time until the manipulation transform is reached
        public void OnUpdate()
        {
            t += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(t / data.manipDuration);
            owner.transform.position = Vector3.Lerp(currentPosition, targetPosition, normalizedTime);
            owner.transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, normalizedTime);
            if (normalizedTime >= 1f)
            {
                canTransition = true;
            }
        }

        // Once interpolation completes, transition back to NONE state
        public StateType OnTransitionCheck()
        {   
            if (canTransition)
                return StateType.NONE;

            return Type;
        }

        public void OnExit(StateType next, Action onCompletion = null)
        {
            Debug.Log("Exiting MANIP state");
            owner.playerController.isPossesed = true;
            onCompletion?.Invoke();
        }
    }
}

[Serializable]

// Configuration data for the ManipState
public struct ManipStateData
{   
    // Duration in seconds for the manipulation interpolation
    public float manipDuration;
}
