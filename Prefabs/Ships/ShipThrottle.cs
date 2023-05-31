using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ShipThrottle : MonoBehaviour
{
    [SerializeField] SteamVR_Action_Boolean grabAction;
    [SerializeField] MeshRenderer highlight;
    [SerializeField] SteamVR_Action_Boolean lockThrottleAction;
    bool lockThrottle = false;
    SteamVR_Behaviour_Pose pose;
    Hand hand;
    bool canGrab = true;

    [SerializeField] Transform throttle;
    [SerializeField] float[] angleRange;
    [SerializeField] float[] valueRange;

    [SerializeField] Transform lockButton;
    float lockStartPos;
    [SerializeField] float lockEndPos;

    public float value { get; private set; }

    void Start()
    {
        lockStartPos = lockButton.localPosition.x;
    }

    void Update()
    {
        if (hand != null)
        {
            throttle.rotation = hand.objectAttachmentPoint.rotation;
            throttle.localEulerAngles = new Vector3(throttle.localEulerAngles.x, 0, 0);
            Vector3 fixedEulers = throttle.localEulerAngles.FixEulers();
            throttle.localEulerAngles = CustomExtensions.Clamp(fixedEulers, angleRange[0], angleRange[1]);
            Vector3 fixedClampedEulers = throttle.localEulerAngles.FixEulers();

            value = CustomExtensions.normalize(fixedClampedEulers.x, angleRange[0], angleRange[1]);

            if (VRControl.playerSettings.shipControls.grabToggle)
            {
                if (grabAction.GetStateDown(pose.inputSource))
                {
                    StartCoroutine(ResetGrab());
                }
            }
            else
            {
                if (!grabAction.GetState(pose.inputSource))
                {
                    StartCoroutine(ResetGrab());
                }
            }

            if (lockThrottleAction.GetStateDown(pose.inputSource))
            {
                if (lockThrottle)
                {
                    lockThrottle = false;
                    lockButton.localPosition = new Vector3(lockStartPos, lockButton.localPosition.y, lockButton.localPosition.z);
                }
                else
                {
                    lockThrottle = true;
                    lockButton.localPosition = new Vector3(lockEndPos, lockButton.localPosition.y, lockButton.localPosition.z);
                }
            }
        }
        else
        {
            if (!lockThrottle)
            {
                throttle.localEulerAngles = Vector3.RotateTowards(throttle.localEulerAngles.FixEulers(), new Vector3(angleRange[0], 0, 0), 2, 1);
                value = valueRange[0];
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (canGrab && hand == null && other.transform.HasTag("Hand"))
        {
            SteamVR_Behaviour_Pose poseInTrigger = other.GetComponent<SteamVR_Behaviour_Pose>();
            highlight.enabled = true;
            if (grabAction.GetStateDown(poseInTrigger.inputSource))
            {
                canGrab = false;
                pose = poseInTrigger;
                hand = poseInTrigger.GetComponent<Hand>();
                highlight.enabled = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.HasTag("Hand"))
        {
            highlight.enabled = false;
        }
    }

    IEnumerator ResetGrab()
    {
        hand = null;
        yield return new WaitUntil(() => !grabAction.GetState(pose.inputSource));
        pose = null;
        canGrab = true;
    }
}
