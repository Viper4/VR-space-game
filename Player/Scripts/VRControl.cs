using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VRControl : MonoBehaviour
{
    // General use
    private bool paused;
    public bool Paused
    {
        get
        {
            return paused;
        }
        set
        {
            paused = value;
            if(movementEnabled)
                teleporting.SetActive(!paused);
            if (snapTurnEnabled)
                snapTurn.SetActive(!paused);
        }
    }
    [SerializeField] SteamVR_Behaviour_Pose leftPose;
    [SerializeField] SteamVR_Behaviour_Pose rightPose;

    // Trackpad Movement
    bool movementEnabled = false;
    public bool enableMovement
    {
        get
        {
            return movementEnabled;
        }
        set
        {
            movementEnabled = value;

            teleporting.SetActive(value);
        }
    }

    bool snapTurnEnabled = false;
    public bool enableSnapTurn
    {
        get
        {
            return snapTurnEnabled;
        }
        set
        {
            snapTurnEnabled = value;

            snapTurn.SetActive(value);
        }
    }
    [SerializeField] GameObject teleporting;
    [SerializeField] GameObject snapTurn;
    [SerializeField] SteamVR_Action_Vector2 touchpadPositionAction = null;
    [SerializeField] SteamVR_Action_Boolean touchpadClickAction = null;
    [SerializeField] Transform head;
    [SerializeField] Rigidbody playerRigidbody;
    [SerializeField] float moveSpeed = 5;

    public static PlayerSettings playerSettings;

    void Start()
    {
        playerSettings = new PlayerSettings()
        {
            grabToggle = false,
        };
    }

    void Update()
    {
        if (paused)
        {
            
        }
        else if(movementEnabled)
        {
            float distanceFromFloor = Vector3.Dot(head.localPosition, Vector3.up);
            bool isGrounded = Physics.Raycast(head.position, -transform.up, distanceFromFloor + 0.1f);

            if (isGrounded)
            {
                Vector2 inputDir = touchpadClickAction.state ? Vector2.zero : touchpadPositionAction.axis * moveSpeed;
                playerRigidbody.AddForce(new Vector3(head.forward.x, 0, head.forward.z) * inputDir.y + new Vector3(head.right.x, 0, head.right.z) * inputDir.x, ForceMode.VelocityChange);
            }
        }
    }
}
