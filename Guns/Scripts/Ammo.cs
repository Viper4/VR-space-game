using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;

public class Ammo : MonoBehaviour
{
    public enum Type
    {
        FiveFiveSix,
        SevenSixTwo,
    }
    [SerializeField] Type ammoType;

    public int maxAmmo
    {
        get
        {
            if(magazine != null)
            {
                return magazine.maxAmmo;
            }
            return 0;
        }
        set
        {
            if(magazine != null)
            {
                magazine.maxAmmo = value;
            }
        }
    }

    public int currentAmmo 
    {
        get
        {
            if(magazine != null)
            {
                return magazine.ammoCount;
            }
            return 0;
        }
    }


    [SerializeField] MeshRenderer highlight;
    [SerializeField] AmmoPickup magazine;

    public bool attachedToGun;
    List<AmmoPickup> magsInTrigger = new List<AmmoPickup>();

    void Start()
    {
        if(magazine != null)
        {
            attachedToGun = true;
            magazine.ammoManager = this;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!attachedToGun && other.transform.HasTag("Ammo"))
        {
            AmmoPickup otherMagazine = other.GetComponent<AmmoPickup>();

            if(otherMagazine.ammoType == ammoType)
            {
                highlight.enabled = true;
                if (magazine != otherMagazine && !magsInTrigger.Contains(otherMagazine))
                {
                    magsInTrigger.Add(otherMagazine);
                    magazine = otherMagazine;
                    magazine.ammoManager = this;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!attachedToGun && other.transform.HasTag("Ammo"))
        {
            AmmoPickup otherMagazine = other.GetComponent<AmmoPickup>();

            if(otherMagazine.ammoType == ammoType)
            {
                magsInTrigger.Remove(otherMagazine);

                if (magsInTrigger.Count > 0)
                {
                    magazine = magsInTrigger[^1];
                    magazine.ammoManager = this;
                }
                else
                {
                    highlight.enabled = false;
                    magazine.ammoManager = null;
                    magazine = null;
                }
            }
        }
    }

    public void AttachAmmo()
    {
        attachedToGun = true;
        highlight.enabled = false;

        magazine.transform.SetParent(transform);
        magazine.transform.position = transform.position;
        magazine.transform.rotation = transform.rotation;
    }

    public void DetachAmmo()
    {
        attachedToGun = false;
        magazine = null;
    }

    public void RemoveBullet()
    {
        magazine.ammoCount--;
        magazine.UpdateBullets(true);
    }
}
