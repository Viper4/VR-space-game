using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;

public abstract class Turret : MonoBehaviour
{
    public bool active = true;
    public bool manual = false;
    public LayerMask ignoreLayers;
    [SerializeField] Transform platform;
    [SerializeField] Transform rotatingObject;
    [SerializeField] Vector3 minAngles;
    [SerializeField] Vector3 maxAngles;
    [SerializeField] float rotateSpeed = 10;

    [SerializeField] float shootAngle = 1;
    [SerializeField] float fireRate = 0.15f;
    float fireTime = 0;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject shootParticles;
    public Transform firePoint;
    [SerializeField] float bulletSpeed = 20;
    [HideInInspector] public Transform target;

    [HideInInspector] public Vector3 targetDirection;
    [HideInInspector] public bool fire = false;

    protected void Start()
    {

    }

    protected void Update()
    {
        if (active)
        {
            if (!manual)
            {
                if (target)
                {
                    targetDirection = target.position - rotatingObject.position;

                    if (fire)
                    {
                        fireTime += Time.deltaTime;
                        if (fireTime >= fireRate)
                        {
                            float angleToTarget = Vector3.Angle(rotatingObject.forward, targetDirection);
                            if (angleToTarget < shootAngle && (!Physics.Raycast(firePoint.position, targetDirection, out RaycastHit hit, Vector3.Distance(firePoint.position, target.position) - 0.1f, ~ignoreLayers) || hit.transform == target))
                            {
                                Fire();
                            }
                        }
                    }
                }
            }
            else
            {
                if (fire)
                {
                    fireTime += Time.deltaTime;
                    if (fireTime >= fireRate)
                    {
                        Fire();
                    }
                }
            }
            if(targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection, platform.up);
                platform.rotation = rotatingObject.rotation = Quaternion.RotateTowards(rotatingObject.rotation, targetRotation, rotateSpeed);
                platform.localEulerAngles = new Vector3(0, platform.localEulerAngles.y, 0);
                rotatingObject.localEulerAngles = CustomMethods.Clamp(rotatingObject.localEulerAngles.FixEulers(), minAngles, maxAngles);
                Debug.DrawLine(firePoint.position, firePoint.position + targetDirection, Color.blue, 0.1f);
            }
        }
    }

    void Fire()
    {
        fireTime = 0;
        Rigidbody bulletRigidbody = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation).GetComponent<Rigidbody>();
        bulletRigidbody.velocity = firePoint.forward * bulletSpeed;
        bulletRigidbody.GetComponent<TrailRenderer>().enabled = true;
        Instantiate(shootParticles, firePoint.position, firePoint.rotation);
    }
}
