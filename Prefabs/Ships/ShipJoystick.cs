using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ShipJoystick : MonoBehaviour
{
    [SerializeField] SteamVR_Action_Boolean grabAction;
    [SerializeField] MeshRenderer highlight;
    SteamVR_Behaviour_Pose pose;
    Hand hand;
    bool canGrab = true;

    [SerializeField] SteamVR_Action_Single squeezeAction;
    [SerializeField] SteamVR_Action_Boolean triggerAction;
    [SerializeField] SteamVR_Action_Vector2 touchpadPositionAction;
    [SerializeField] SteamVR_Action_Boolean touchpadClickAction;

    [SerializeField] Transform joystick;
    [SerializeField] float maxAngle = 35;
    [SerializeField] Transform trigger;
    bool triggerDown;
    [SerializeField] float triggerDistance = 0.015f;
    float triggerStartPos;
    [SerializeField] float triggerAngle = 8;
    [SerializeField] Transform releaseButton;
    [SerializeField] Animation releaseAnimation;
    [SerializeField] Transform toggleSwitch;
    bool toggle;
    Vector3 toggleStartEulers;
    [SerializeField] Vector3 toggleEndEulers;

    public Vector3 direction { get; private set; }

    public event JoystickEventHandler JoystickTriggerDown;
    public event JoystickEventHandler JoystickTriggerUp;
    public event JoystickEventHandler JoystickRelease;
    public event JoystickEventHandler JoystickToggle;

    void Start()
    {
        triggerStartPos = trigger.localPosition.z;
        toggleStartEulers = toggleSwitch.localEulerAngles;
    }

    void Update()
    {
        if(hand != null)
        {
            joystick.rotation = hand.objectAttachmentPoint.rotation;
            Vector3 fixedEulers = joystick.localEulerAngles.FixEulers();
            joystick.localEulerAngles = CustomExtensions.Clamp(fixedEulers, -maxAngle, maxAngle);
            Vector3 fixedClampedEulers = joystick.localEulerAngles.FixEulers();
            direction = new Vector3(fixedClampedEulers.x / maxAngle, fixedClampedEulers.y / maxAngle, fixedClampedEulers.z / maxAngle);

            float triggerAxis = squeezeAction.GetAxis(pose.inputSource);
            trigger.localPosition = new Vector3(trigger.localPosition.x, trigger.localPosition.y, triggerStartPos - triggerDistance * triggerAxis);
            trigger.localEulerAngles = new Vector3(triggerAngle * triggerAxis, trigger.localEulerAngles.y, trigger.localEulerAngles.z);
            if (triggerAction.GetStateDown(pose.inputSource))
            {
                OnJoystickTriggerDown();
            }
            else if (triggerAction.GetStateUp(pose.inputSource))
            {
                OnJoystickTriggerUp();
            }

            Vector2 touchpadPosition = touchpadPositionAction.GetAxis(pose.inputSource);
            if (touchpadClickAction.GetStateDown(pose.inputSource))
            {
                if(touchpadPosition.x < 0) // Left side
                {
                    if (touchpadPosition.y < 0) // Bottom
                    {
                        OnJoystickRelease();
                    }
                    else if (touchpadPosition.y > 0) // Top
                    {
                        OnJoystickToggle();
                    }
                }
                else if(touchpadPosition.x > 0) // Right side
                {
                    if (touchpadPosition.y < 0)
                    {
                        
                    }
                    else if (touchpadPosition.y > 0)
                    {
                        
                    }
                }
            }

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
        }
        else
        {
            joystick.rotation = Quaternion.RotateTowards(joystick.rotation, transform.rotation, 5);
            if (triggerDown)
            {
                OnJoystickTriggerUp();
            }
            trigger.localPosition = new Vector3(trigger.localPosition.x, trigger.localPosition.y, triggerStartPos);
            trigger.localEulerAngles = new Vector3(0, trigger.localEulerAngles.y, trigger.localEulerAngles.z);
            direction = Vector3.zero;
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

    public virtual void OnJoystickTriggerDown()
    {
        triggerDown = true;
        JoystickTriggerDown?.Invoke();
    }

    public virtual void OnJoystickTriggerUp()
    {
        triggerDown = false;
        JoystickTriggerUp?.Invoke();
    }

    public virtual void OnJoystickRelease()
    {
        releaseAnimation.Play();
        JoystickRelease?.Invoke();
    }

    public virtual void OnJoystickToggle()
    {
        if (toggle)
        {
            toggle = false;
            toggleSwitch.localEulerAngles = toggleStartEulers;
        }
        else
        {
            toggle = true;
            toggleSwitch.localEulerAngles = toggleEndEulers;
        }
        JoystickToggle?.Invoke();
    }

    public delegate void JoystickEventHandler();
}
