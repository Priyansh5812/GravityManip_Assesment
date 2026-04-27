using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// ViewState: allows the player to preview valid translation targets by
// raycasting around an ideal point. The player can select one of the
// gathered points to set a target for manipulation.
public partial class GravityManipulation
{
    private class ViewState : IState
    {
        private GravityManipulation owner;
        private ViewStateData data;
        RaycastHit[] hits;
        Dictionary<KeyCode, (Vector3 , Vector3 , Vector3)> gatheredPoints; // Value : Position , LookDirection , GroundNormal

        public StateType Type => StateType.VIEW;
        public ViewState(GravityManipulation owner, ViewStateData data) 
        {
            this.owner = owner;
            this.data = data;
            hits = new RaycastHit[1];
            gatheredPoints = new Dictionary<KeyCode, (Vector3 , Vector3 , Vector3)>();
        }

        public void OnEnter(Action onCompletion = null)
        {
            CalculateTranslationPositions();
            PrepareStartup();
            onCompletion?.Invoke();
        }

        void PrepareStartup()
        {
            foreach (var i in gatheredPoints)
            {
                PollForTranslation(i.Key , true);
                return;
            }
        }

        public void OnUpdate()
        {
            // Poll for directional inputs to move the preview hologram
            PollForTranslation(KeyCode.UpArrow);
            PollForTranslation(KeyCode.DownArrow);
            PollForTranslation(KeyCode.RightArrow);
            PollForTranslation(KeyCode.LeftArrow);
        }

        public StateType OnTransitionCheck()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                return StateType.NONE;
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                return StateType.MANIP;
            }

            return Type;
        }
        public async void OnExit(StateType next, Action onCompletion = null)
        {
            Debug.Log("Exiting VIEW state");
            data.playerHolo.gameObject.SetActive(false);
            await RepositionCamToSpringArm();
            owner.springArm.enabled = true;
            owner.playerController.isPossesed = true;
            onCompletion?.Invoke();
        }

        // Smoothly reposition camera back to the spring arm pose
        async Task RepositionCamToSpringArm()
        {
            var pose = owner.springArm.GetComputedPose();
            Vector3 targetPosition = pose.Item1;
            Quaternion targetRotation = pose.Item2;

            Camera cam = owner.mainCamera;

            Vector3 currentPosition = cam.transform.position;
            Quaternion currentRotation = cam.transform.rotation;

            float t = 0;

            while (t < data.RetargetDuration)
            {
                t += Time.deltaTime;
                cam.transform.position = Vector3.Lerp(currentPosition, targetPosition, t / data.RetargetDuration);
                cam.transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, t / data.RetargetDuration);
                await Task.Yield();
            }

            cam.transform.position = targetPosition;
            cam.transform.rotation = targetRotation;
        }

        void CalculateTranslationPositions()
        {
            Vector3 idealRaycastSrc = owner.GetIdealCastPoint();
            Transform orientationRef = owner.ViewSrc;
            gatheredPoints?.Clear();

            GatherPoint(idealRaycastSrc, orientationRef.transform.up , KeyCode.UpArrow);
            GatherPoint(idealRaycastSrc, -orientationRef.transform.up , KeyCode.DownArrow);
            GatherPoint(idealRaycastSrc, orientationRef.transform.right , KeyCode.RightArrow);
            GatherPoint(idealRaycastSrc, -orientationRef.transform.right , KeyCode.LeftArrow);
        }

        void GatherPoint(Vector3 idealRaycastSrc, Vector3 direction , KeyCode keycode)
        {   
            if (Physics.RaycastNonAlloc(idealRaycastSrc, direction, hits, Mathf.Infinity, data.groundMask) > 0)
            {
                if (Vector3.Dot(owner.transform.up, hits[0].normal) > 0.9f)
                    return;

                gatheredPoints.Add(keycode, (hits[0].point , Vector3.ProjectOnPlane(owner.ViewSrc.position - hits[0].point , hits[0].normal).normalized , hits[0].normal));
            }
        }

        void PollForTranslation(KeyCode key , bool forceTranslation = false)
        {   
            if (!gatheredPoints.ContainsKey(key))
                return;

            if (Input.GetKeyDown(key) || forceTranslation)
            {
                data.playerHolo.gameObject.SetActive(true);
                data.playerHolo.transform.position = owner.targetPosition = gatheredPoints[key].Item1;
                data.playerHolo.transform.rotation = owner.targetRotation = Quaternion.LookRotation(gatheredPoints[key].Item2, gatheredPoints[key].Item3);
            }
        }

    }
}
[Serializable]
public struct ViewStateData
{
    // Hologram used to preview the target position/rotation
    public Transform playerHolo;
    // Mask used when raycasting ground candidate points
    public LayerMask groundMask;
    // Duration for camera retargeting when exiting the view state
    public float RetargetDuration;
}

