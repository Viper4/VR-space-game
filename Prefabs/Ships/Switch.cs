using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Switch : MonoBehaviour
{
    [SerializeField] SteamVR_Action_Boolean toggleAction;
    [SerializeField] MeshRenderer highlight;
    [SerializeField] Vector3[] eulerStates;
    public int currentState { get; private set; } = 0;
    [SerializeField] int index;

    public event SwitchEventHandler SwitchToggle;

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.HasTag("Hand"))
        {
            SteamVR_Behaviour_Pose poseInTrigger = other.GetComponent<SteamVR_Behaviour_Pose>();
            highlight.enabled = true;
            if (toggleAction.GetStateDown(poseInTrigger.inputSource))
            {
                currentState++;
                if (currentState > eulerStates.Length - 1)
                    currentState = 0;
                transform.localEulerAngles = eulerStates[currentState];
                
                OnToggle(currentState);
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

    public virtual void OnToggle(int newState)
    {
        SwitchToggle?.Invoke(index, newState);
    }

    public delegate void SwitchEventHandler(int index, int state);
}
