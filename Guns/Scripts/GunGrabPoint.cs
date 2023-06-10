using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using SpaceStuff;

[RequireComponent(typeof(Collider))]
public class GunGrabPoint : MonoBehaviour
{
    [SerializeField] Transform gun;
    Vector3 previousGunRotation;
    [SerializeField] MeshRenderer highlight;

    [SerializeField] SteamVR_Action_Boolean grabAction;
    bool canGrab = true;
    bool grabbing;
    [SerializeField] Vector3 leftPosition;
    [SerializeField] Vector3 leftRotation;
    [SerializeField] Vector3 rightPosition;
    [SerializeField] Vector3 rightRotation;

    Hand mainHand;

    SteamVR_Behaviour_Pose secondPose;
    Hand secondHand;
    GameObject secondHandClone;

    void Update()
    {
        if (grabbing)
        {
            highlight.enabled = false;
            mainHand.objectAttachmentPoint.rotation = Quaternion.LookRotation(secondHand.transform.position - gun.position, mainHand.transform.parent.up);
            secondHandClone.transform.position = transform.position;
            secondHandClone.transform.rotation = transform.rotation;
            if(secondPose == null || grabAction.GetStateDown(secondPose.inputSource))
            {
                StopGrab();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!grabbing && other.transform.HasTag("Hand"))
        {
            if (canGrab && secondHand != null)
            {
                if (grabAction.GetStateDown(secondPose.inputSource))
                {
                    StartGrab();
                }
            }
            if (mainHand != null)
            {
                highlight.enabled = true;
                if(secondHand == null)
                {
                    secondPose = other.GetComponent<SteamVR_Behaviour_Pose>();
                    secondHand = other.GetComponent<Hand>();
                }
                secondHand.hoveringGrabPoint = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (mainHand != null && secondHand != null && other.transform.HasTag("Hand"))
        {
            highlight.enabled = false;
            secondHand.hoveringGrabPoint = false;
            if (grabbing)
            {
                StopGrab();
            }
        }
    }

    public void Attach(Hand hand)
    {
        mainHand = hand;
        if(mainHand.handType == SteamVR_Input_Sources.RightHand)
        {
            transform.localPosition = leftPosition;
            transform.localEulerAngles = leftRotation;
        }
        else
        {
            transform.localPosition = rightPosition;
            transform.localEulerAngles = rightRotation;
        }
    }

    public void Detach()
    {
        StopGrab();
        mainHand = null;
    }

    void StartGrab()
    {
        grabbing = true;
        previousGunRotation = mainHand.objectAttachmentPoint.localEulerAngles;

        secondHand.Hide();
        secondHandClone = Instantiate(secondHand.mainRenderModel.gameObject);
    }

    void StopGrab()
    {
        canGrab = false;
        if (grabbing)
        {
            grabbing = false;
            mainHand.objectAttachmentPoint.localEulerAngles = previousGunRotation;
            secondHand.Show();
        }

        highlight.enabled = false;
        secondHand = null;
        Destroy(secondHandClone);
        StartCoroutine(ResetGrab());
    }

    IEnumerator ResetGrab()
    {
        yield return new WaitUntil(() => !grabAction.GetState(secondPose.inputSource));
        secondPose = null;
        canGrab = true;
    }
}
