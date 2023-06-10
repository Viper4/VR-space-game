using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class Gun : MonoBehaviour
{
    SteamVR_Behaviour_Pose handPose = null;
    [HideInInspector] public Hand hand = null;

    Interactable interactable;
    [SerializeField] SteamVR_Action_Boolean fireAction = null;
    [SerializeField] SteamVR_Action_Single squeezeAction = null;

    [SerializeField] Transform trigger;
    [SerializeField] GameObject shootParticles;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] AudioSource emptyAudio;
    [SerializeField] float bulletSpeed = 60;

    [SerializeField] Animation slideAnimation;
    [SerializeField] GameObject casing;
    [SerializeField] Transform casingSpawn;
    [SerializeField] float casingSpeed = 5;

    [SerializeField] GameObject[] gunColliderObjects;
    [SerializeField] GunGrabPoint grabPoint;

    [SerializeField] Switch fireModeSwitch;

    [SerializeField] Ammo ammoManager;
    [SerializeField] int tracerInterval = 3;
    [SerializeField] float fireRate = 0.077f;
    float holdTime = 0;
    [SerializeField] float burstDelay = 1;
    [SerializeField] int burstAmount = 3;
    int burstCount;

    private void Start()
    {
        interactable = GetComponent<Interactable>();
    }

    void Update()
    {
        if (handPose != null)
        {
            float squeezeAxis = squeezeAction.GetAxis(handPose.inputSource);
            trigger.localEulerAngles = new Vector3(squeezeAxis * 40, 0, 0);
            if (ammoManager.attachedToGun && ammoManager.currentAmmo > 0)
            {
                switch (fireModeSwitch.currentState)
                {
                    case 1: // Semi
                        if (fireAction.GetStateDown(handPose.inputSource))
                        {
                            Fire();
                        }
                        break;
                    case 2: // Auto
                        if (fireAction.GetState(handPose.inputSource))
                        {
                            holdTime += Time.deltaTime;
                            if (holdTime >= fireRate)
                            {
                                Fire();
                                holdTime = 0;
                            }
                        }
                        break;
                    case 3: // Burst
                        if (fireAction.GetState(handPose.inputSource))
                        {
                            holdTime += Time.deltaTime;
                            if (burstCount < burstAmount)
                            {
                                if (holdTime >= fireRate)
                                {
                                    Fire();
                                    holdTime = 0;
                                    burstCount++;
                                }
                            }
                            else
                            {
                                if (holdTime >= burstDelay)
                                {
                                    holdTime = 0;
                                    burstCount = 0;
                                }
                            }
                        }
                        break;
                    default:
                        if (fireAction.GetStateDown(handPose.inputSource))
                        {
                            emptyAudio.Play();
                        }
                        break;
                }
            }
            else
            {
                if (fireAction.GetStateDown(handPose.inputSource))
                {
                    emptyAudio.Play();
                }
            }
        }
    }

    public void Attach()
    {
        handPose = GetComponentInParent<SteamVR_Behaviour_Pose>();
        hand = handPose.GetComponent<Hand>();
        foreach (GameObject GO in gunColliderObjects)
        {
            GO.layer = 3;
        }

        if (grabPoint)
        {
            grabPoint.Attach(hand);
        }
        interactable.canAttachToHand = false;
    }

    public void Detach()
    {
        handPose = null;
        hand = null;
        foreach (GameObject GO in gunColliderObjects)
        {
            GO.layer = 0;
        }

        if (grabPoint != null)
        {
            grabPoint.Detach();
        }
        interactable.canAttachToHand = true;
    }

    public void Fire()
    {
        if(ammoManager.attachedToGun && ammoManager.currentAmmo > 0)
        {
            slideAnimation.Play();
            Rigidbody bulletRB = Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation).GetComponent<Rigidbody>();
            bulletRB.velocity = bulletSpawn.forward * bulletSpeed;
            if (ammoManager.currentAmmo % tracerInterval == 0)
            {
                bulletRB.GetComponent<TrailRenderer>().enabled = true;
            }
            Instantiate(shootParticles, bulletSpawn.position, bulletSpawn.rotation);
            ammoManager.RemoveBullet();

            Rigidbody casingRB = Instantiate(casing, casingSpawn.position, casingSpawn.rotation).GetComponent<Rigidbody>();
            casingRB.velocity = casingSpawn.right * casingSpeed;
        }
    }

    public int AmmoCount()
    {
        return ammoManager.currentAmmo;
    }

    public int MaxAmmo()
    {
        return ammoManager.maxAmmo;
    }
}
