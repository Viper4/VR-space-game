using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    VRControl control;
    [SerializeField] Camera UICamera;
    [SerializeField] LayerMask pausedCullingMask;
    [SerializeField] LayerMask normalCullingMask;
    [SerializeField] Transform head;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] SteamVR_Action_Boolean pauseAction = null;
    Animator animator;
    bool settings = false;

    [SerializeField] Transform settingsParent;

    void Awake()
    {
        control = GetComponent<VRControl>();
        animator = pauseMenu.GetComponent<Animator>();
    }

    void Update()
    {
        if (pauseAction.GetStateDown(SteamVR_Input_Sources.Any))
        {
            if (pauseMenu.activeSelf)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        UICamera.cullingMask = normalCullingMask;
        control.Paused = false;
        pauseMenu.SetActive(false);
    }

    public void Pause()
    {
        UICamera.cullingMask = pausedCullingMask;
        control.Paused = true;
        pauseMenu.transform.position = head.position + head.forward;
        pauseMenu.transform.rotation = head.rotation;
        pauseMenu.SetActive(true);
        animator.SetTrigger("PauseIn");
        settings = false;
    }

    public void Settings()
    {
        animator.SetTrigger("SettingsIn");
        settings = true;
        foreach(Transform child in settingsParent)
        {
            switch (child.name)
            {
                case "Grab Toggle":
                    child.GetComponent<Toggle>().SetIsOnWithoutNotify(VRControl.playerSettings.shipControls.grabToggle);
                    break;
            }
        }
    }

    public void Back()
    {
        if (settings)
        {
            animator.SetTrigger("SettingsOut");
        }
        settings = false;
    }

    public void Exit()
    {
        Application.Quit();
    }


    public void SetGrabToggle(Toggle toggle)
    {
        VRControl.playerSettings.shipControls.grabToggle = toggle.isOn;
    }
}