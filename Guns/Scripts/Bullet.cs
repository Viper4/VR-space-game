using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float destroyDelay = 5;
    [SerializeField] float damageAmount = 10;
    [SerializeField] float armorPiercing = 0.25f;

    void Start()
    {
        Destroy(gameObject, destroyDelay);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.TryGetComponent<StatSystem>(out var statSystem))
        {
            statSystem.Damage(damageAmount, armorPiercing);
        }
        Destroy(gameObject, 0.25f);
    }
}
