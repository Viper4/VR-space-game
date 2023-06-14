using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;
using TMPro;
using SpaceStuff;

public class VRPointer : MonoBehaviour
{
    SteamVR_Behaviour_Pose pose;
    Hand hand;
    Transform attachmentPoint;

    // Interactables
    [SerializeField] SteamVR_Action_Boolean grabAction = null;
    [SerializeField] SteamVR_Action_Boolean keepAttachmentAction = null;

    // UI
    [SerializeField] SteamVR_Action_Boolean interactUIAction = null;
    Collider previousUIContact;
    PointerEventData pointerEventData;

    Switch previousSwitch;

    [SerializeField] LayerMask interactableLayers;
    [SerializeField] Color pointerColor = Color.yellow;
    [SerializeField] Color grabColor = Color.green;
    [SerializeField] GameObject linePrefab;
    LineRenderer line;
    [SerializeField] GameObject dotPrefab;
    Transform dot;

    [SerializeField] float rayDistance = 20;

    // Start is called before the first frame update
    void Start()
    {
        pose = GetComponent<SteamVR_Behaviour_Pose>();
        hand = GetComponent<Hand>();
        attachmentPoint = hand.objectAttachmentPoint;
        line = Instantiate(linePrefab, attachmentPoint).GetComponent<LineRenderer>();
        line.gameObject.SetActive(false);
        dot = Instantiate(dotPrefab, attachmentPoint).transform;
        dot.gameObject.SetActive(false);

        pointerEventData = new PointerEventData(EventSystem.current)
        {
            button = PointerEventData.InputButton.Left,
        };
    }

    // Update is called once per frame
    void LateUpdate()
    {
        HidePointer();
        if (hand.currentAttachedObject == null)
        {
            if (grabAction.GetState(pose.inputSource) || interactUIAction.GetState(pose.inputSource))
            {
                line.material.color = grabColor;
                dot.GetComponent<MeshRenderer>().material.color = grabColor;
            }
            else
            {
                line.material.color = pointerColor;
                dot.GetComponent<MeshRenderer>().material.color = pointerColor;
            }

            if (Physics.Raycast(attachmentPoint.position, attachmentPoint.forward, out RaycastHit hit, rayDistance, interactableLayers, QueryTriggerInteraction.Collide))
            {
                Debug.DrawLine(attachmentPoint.position, hit.point, Color.red, 0.1f);
                if(hit.collider.gameObject.layer == 5) // UI
                {
                    if (hit.collider.transform.parent.TryGetComponent<Selectable>(out var selectable))
                    {
                        ShowPointer(new Vector3[] { Vector3.zero, attachmentPoint.InverseTransformPoint(hit.point) });

                        if (previousUIContact != hit.collider)
                        {
                            if (previousUIContact)
                            {
                                previousUIContact.transform.parent.GetComponent<Selectable>().OnPointerExit(null);
                            }

                            selectable.OnPointerEnter(pointerEventData);

                            previousUIContact = hit.collider;
                        }
                        if (interactUIAction.GetStateDown(pose.inputSource))
                        {
                            selectable.OnPointerDown(pointerEventData);
                        }
                        if (interactUIAction.GetStateUp(pose.inputSource))
                        {
                            selectable.OnPointerUp(pointerEventData);
                            switch (selectable)
                            {
                                case Button button:
                                    button.OnPointerClick(pointerEventData);
                                    break;
                                case Slider slider:

                                    break;
                                case Toggle toggle:
                                    toggle.OnPointerClick(pointerEventData);
                                    break;
                                case TMP_Dropdown dropdown:
                                    dropdown.OnPointerClick(pointerEventData);
                                    break;
                                case TMP_InputField inputField:
                                    inputField.OnPointerClick(pointerEventData);
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    PointerExit();

                    if (hit.collider.transform.HasTag("PointerInteractable"))
                    {
                        ShowPointer(new Vector3[] { Vector3.zero, attachmentPoint.InverseTransformPoint(hit.point) });

                        TestPointerInteractable(hit.collider.transform);
                    }
                    else if (hit.transform.HasTag("PointerInteractable"))
                    {
                        ShowPointer(new Vector3[] { Vector3.zero, attachmentPoint.InverseTransformPoint(hit.point) });

                        TestPointerInteractable(hit.transform);
                    }
                }
            }
        }
        else
        {
            PointerExit();
            if (hand.currentAttachedObject && keepAttachmentAction.GetStateDown(pose.inputSource))
            {
                hand.keepAttachment = !hand.keepAttachment;
            }
        }
    }

    void TestPointerInteractable(Transform hit)
    {
        if (hit.TryGetComponent<Interactable>(out var hitInteractable) && hitInteractable.canAttachToHand)
        {
            if (grabAction.GetStateDown(pose.inputSource))
            {
                if (hitInteractable.TryGetComponent<Throwable>(out var throwable))
                {
                    hand.AttachObject(hitInteractable.gameObject, hand.GetBestGrabbingType(), throwable.attachmentFlags, throwable.attachmentOffset);
                }
                else
                {
                    hand.AttachObject(hitInteractable.gameObject, hand.GetBestGrabbingType());
                }
            }
        }
        else if (hit.TryGetComponent<Switch>(out var hitSwitch))
        {
            hitSwitch.Hover(true);
            if (hitSwitch.toggleAction.GetStateDown(pose.inputSource))
            {
                hitSwitch.Toggle();
            }
            previousSwitch = hitSwitch;
        }
    }

    void PointerExit()
    {
        if (previousUIContact != null)
        {
            previousUIContact.transform.parent.GetComponent<Selectable>().OnPointerExit(pointerEventData);
            previousUIContact = null;
        }

        if (previousSwitch != null)
        {
            previousSwitch.Hover(false);
            previousSwitch = null;
        }
    }

    void ShowPointer(Vector3[] positions)
    {
        line.SetPositions(positions);
        line.gameObject.SetActive(true);
        dot.localPosition = positions[^1];
        dot.gameObject.SetActive(true);
    }

    void HidePointer()
    {
        line.gameObject.SetActive(false);
        dot.gameObject.SetActive(false);
    }
}
