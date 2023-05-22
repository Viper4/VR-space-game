using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class AmmoPickup : MonoBehaviour
{
    public Ammo ammoManager;
    public Ammo.Type ammoType;
    public int maxAmmo = 30;
    public int ammoCount = 30;
    [SerializeField] bool alternateBulletPositions = true;

    Rigidbody RB;
    Collider ammoCollider;

    [SerializeField] Transform bulletParent;
    [SerializeField] MeshRenderer bulletHighlight;
    [SerializeField] Animation addBulletAnimation;

    int bulletsInTrigger;

    void Start()
    {
        RB = GetComponent<Rigidbody>();
        ammoCollider = GetComponent<Collider>();
        UpdateBullets(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ammoManager == null && ammoCount < maxAmmo && other.transform.HasTag("BulletPickup") && other.TryGetComponent<BulletPickup>(out var bulletPickup) && bulletPickup.ammoType == ammoType)
        {
            bulletPickup.magazine = this;
            bulletsInTrigger++;
            bulletHighlight.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ammoManager == null && ammoCount < maxAmmo && other.transform.HasTag("BulletPickup") && other.TryGetComponent<BulletPickup>(out var bulletPickup) && bulletPickup.ammoType == ammoType)
        {
            bulletPickup.magazine = null;
            RemoveBulletsInTrigger();
        }
    }

    public void OnAttachToHand()
    {
        if (ammoManager != null)
        {
            transform.SetParent(null);
            ammoCollider.isTrigger = false;

            ammoManager.DetachAmmo();
            ammoManager = null;
        }
        RB.isKinematic = true;
    }

    public void OnDetachFromHand()
    {
        if (ammoManager != null)
        {
            ammoManager.AttachAmmo();
            RB.isKinematic = true;
            ammoCollider.isTrigger = true;
        }
        else
        {
            RB.isKinematic = false;
        }
    }

    public void UpdateBullets(bool changedBulletCount)
    {
        for (int i = 0; i < bulletParent.childCount; i++)
        {
            Transform bullet = bulletParent.GetChild(i);
            if (i > ammoCount - 1)
            {
                bullet.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                bullet.GetComponent<MeshRenderer>().enabled = true;
            }
            if (alternateBulletPositions && changedBulletCount)
            {
                bullet.localPosition = new Vector3(-bullet.localPosition.x, bullet.localPosition.y, bullet.localPosition.z);
            }
        }
    }

    public void AddBullet(GameObject bullet, int amount)
    {
        if (ammoCount < maxAmmo)
        {
            ammoCount += amount;
            UpdateBullets(true);
            Destroy(bullet);
            addBulletAnimation.Play();

            RemoveBulletsInTrigger();
            if(alternateBulletPositions)
                bulletHighlight.transform.localPosition = new Vector3(-bulletHighlight.transform.localPosition.x, bulletHighlight.transform.localPosition.y, bulletHighlight.transform.localPosition.z);
        }
    }

    void RemoveBulletsInTrigger()
    {
        bulletsInTrigger--;
        if (bulletsInTrigger <= 0)
        {
            bulletHighlight.enabled = false;
            bulletsInTrigger = 0;
        }
    }
}
