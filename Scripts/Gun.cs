using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Gun : MonoBehaviour
{
    SteamVR_Behaviour_Pose handPose = null;
    Transform attachmentPoint = null;

    [SerializeField] SteamVR_Action_Boolean squeezeAction = null;
    [SerializeField] SteamVR_Action_Boolean gripAction = null;
    [SerializeField] Vector3 attachmentEulers = new Vector3(45, 0, 0);
    Vector3 previousAttachmentEulers;

    [SerializeField] Transform bulletSpawn;
    [SerializeField] GameObject bullet;
    [SerializeField] float bulletSpeed = 10;

    void Start()
    {
        
    }

    void Update()
    {
        if (handPose != null && squeezeAction.GetStateDown(handPose.inputSource))
        {
            Fire();
        }
    }

    public void Attach()
    {
        handPose = GetComponentInParent<SteamVR_Behaviour_Pose>();
        attachmentPoint = handPose.GetComponent<Hand>().objectAttachmentPoint;
        previousAttachmentEulers = attachmentPoint.eulerAngles;
        attachmentPoint.eulerAngles = attachmentEulers;
    }

    public void Detach()
    {
        attachmentPoint.eulerAngles = previousAttachmentEulers;
        handPose = null;
        attachmentPoint = null;
    }

    void Fire()
    {
        Rigidbody bulletRB = Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation).GetComponent<Rigidbody>();
        bulletRB.velocity = bulletSpawn.forward * bulletSpeed;
    }
}
