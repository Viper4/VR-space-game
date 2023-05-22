using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPickup : MonoBehaviour
{
    public Ammo.Type ammoType;
    public AmmoPickup magazine;

    [SerializeField] int rounds = 1;

    public void OnDetachFromHand()
    {
        if(magazine != null)
        {
            magazine.AddBullet(gameObject, rounds);
        }
    }
}
