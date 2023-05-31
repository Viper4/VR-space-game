using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Switch : MonoBehaviour
{
    public SteamVR_Action_Boolean toggleAction;
    MeshRenderer highlight;
    AudioSource audioSource;
    [SerializeField] Vector3[] eulerStates;
    [SerializeField] MeshRenderer indicator;
    [SerializeField] Color[] indicatorColors;
    [SerializeField] Color[] indicatorEmissionColors;
    public int currentState;
    [SerializeField] int index;

    public event SwitchEventHandler OnSwitchToggle;

    private void Start()
    {
        highlight = transform.GetChild(0).GetComponent<MeshRenderer>();
        audioSource = GetComponent<AudioSource>();
        transform.eulerAngles = eulerStates[currentState];
        if (indicator != null)
        {
            indicator.material = Instantiate(indicator.sharedMaterial);
            indicator.material.SetColor("_MainColor", indicatorColors[currentState]);
            indicator.material.SetColor("_EmissionColor", indicatorEmissionColors[currentState]);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.HasTag("Hand"))
        {
            SteamVR_Behaviour_Pose poseInTrigger = other.GetComponent<SteamVR_Behaviour_Pose>();
            Hover(true);
            if (toggleAction.GetStateDown(poseInTrigger.inputSource))
            {
                Toggle();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.HasTag("Hand"))
        {
            Hover(false);
        }
    }

    public void Hover(bool value)
    {
        highlight.enabled = value;
    }

    public void Toggle()
    {
        currentState++;
        if (currentState > eulerStates.Length - 1)
            currentState = 0;
        transform.localEulerAngles = eulerStates[currentState];
        if(indicator != null)
        {
            indicator.material.SetColor("_MainColor", indicatorColors[currentState]);
            indicator.material.SetColor("_EmissionColor", indicatorColors[currentState]);
        }

        OnToggle(currentState);
        audioSource.Play();
        highlight.enabled = false;
    }

    public virtual void OnToggle(int newState)
    {
        OnSwitchToggle?.Invoke(index, newState);
    }

    public delegate void SwitchEventHandler(int index, int state);
}
