using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PointerGrab : MonoBehaviour
{
    [SerializeField] SteamVR_Behaviour_Pose leftPose;
    [SerializeField] Transform leftAttachPoint;
    Hand leftHand;
    [SerializeField] SteamVR_Behaviour_Pose rightPose;
    [SerializeField] Transform rightAttachPoint;
    Hand rightHand;
    [SerializeField] SteamVR_Action_Boolean grabAction = null;

    [SerializeField] float rayDistance = 10;

    [SerializeField] GameObject linePrefab;
    LineRenderer[] lines = new LineRenderer[2];

    // Start is called before the first frame update
    void Start()
    {
        leftHand = leftPose.GetComponent<Hand>();
        rightHand = rightPose.GetComponent<Hand>();

        lines[0] = Instantiate(linePrefab).GetComponent<LineRenderer>();
        lines[1] = Instantiate(linePrefab).GetComponent<LineRenderer>();
        lines[0].gameObject.SetActive(false);
        lines[1].gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (rightHand.currentAttachedObject == null && Physics.Raycast(rightHand.transform.position, rightAttachPoint.forward, out RaycastHit rightHit, rayDistance) && rightHit.transform.CompareTag("Interactable"))
        {
            ShowLine(0, new Vector3[] { rightHand.transform.position, rightHit.point });
            if (grabAction.GetStateDown(rightPose.inputSource))
            {
                rightHand.AttachObject(rightHit.transform.gameObject, rightHand.GetBestGrabbingType());
            }
        }
        else
        {
            HideLine(0);
        }

        if(leftHand.currentAttachedObject == null && Physics.Raycast(leftHand.transform.position, leftAttachPoint.forward, out RaycastHit leftHit, rayDistance) && leftHit.transform.CompareTag("Interactable"))
        {
            ShowLine(1, new Vector3[] { leftHand.transform.position, leftHit.point });
            if (grabAction.GetStateDown(leftPose.inputSource))
            {
                leftHand.AttachObject(leftHit.transform.gameObject, leftHand.GetBestGrabbingType());
            }
        }
        else
        {
            HideLine(1);
        }
    }

    void ShowLine(int lineIndex, Vector3[] positions)
    {
        LineRenderer line = lines[lineIndex];
        if (line != null)
        {
            line.SetPositions(positions);
            line.gameObject.SetActive(true);
        }
    }

    void HideLine(int lineIndex)
    {
        LineRenderer line = lines[lineIndex];
        if (line != null)
        {
            line.gameObject.SetActive(false);
        }
    }
}
