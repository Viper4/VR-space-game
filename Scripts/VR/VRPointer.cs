using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;
using TMPro;

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
        line = Instantiate(linePrefab).GetComponent<LineRenderer>();
        line.gameObject.SetActive(false);
        dot = Instantiate(dotPrefab).transform;
        dot.gameObject.SetActive(false);

        pointerEventData = new PointerEventData(EventSystem.current)
        {
            button = PointerEventData.InputButton.Left,
        };
    }

    // Update is called once per frame
    void Update()
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

            if (Physics.Raycast(attachmentPoint.position, attachmentPoint.forward, out RaycastHit hit, rayDistance, interactableLayers, QueryTriggerInteraction.Ignore))
            {
                if(hit.collider.gameObject.layer == 5)
                {
                    ShowPointer(new Vector3[] { attachmentPoint.position, hit.point });
                    Selectable selectable = hit.collider.transform.parent.GetComponent<Selectable>();
                    
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
                else
                {
                    PointerExit();
                    if (hit.transform.HasTag("PointerInteractable"))
                    {
                        if (hit.transform.GetComponent<Interactable>().canAttachToHand)
                        {
                            ShowPointer(new Vector3[] { attachmentPoint.position, hit.point });

                            if (grabAction.GetStateDown(pose.inputSource))
                            {
                                if(hit.transform.TryGetComponent<Throwable>(out var throwable))
                                {
                                    hand.AttachObject(hit.transform.gameObject, hand.GetBestGrabbingType(), throwable.attachmentFlags, throwable.attachmentOffset);
                                }
                                else
                                {
                                    hand.AttachObject(hit.transform.gameObject, hand.GetBestGrabbingType());
                                }
                            }
                        }
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

    void PointerExit()
    {
        if (previousUIContact)
        {
            previousUIContact.transform.parent.GetComponent<Selectable>().OnPointerExit(pointerEventData);
            previousUIContact = null;
        }
    }

    void ShowPointer(Vector3[] positions)
    {
        line.SetPositions(positions);
        line.gameObject.SetActive(true);
        dot.position = positions[^1];
        dot.gameObject.SetActive(true);
    }

    void HidePointer()
    {
        line.gameObject.SetActive(false);
        dot.gameObject.SetActive(false);
    }
}
