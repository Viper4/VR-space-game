using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torpedo : MonoBehaviour
{
    public Transform target;
    public float detonationDistance = 5;
    [SerializeField] float propulsionForce = 1000;
    [SerializeField] float rotationForce = 100;
    Rigidbody _rigidbody;
    PIDController xRotationPID;
    PIDController yRotationPID;
    PIDController zRotationPID;
    [SerializeField] float p, i, d;
    bool active;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        xRotationPID = new PIDController(p, i ,d);
        yRotationPID = new PIDController(p, i ,d);
        zRotationPID = new PIDController(p, i ,d);
    }

    void Update()
    {
        if (target)
        {
            Vector3 targetDirection = target.position - transform.position;
            Vector3 error = targetDirection - transform.forward;
            float torqueX = xRotationPID.GetOutput(error.x, Time.deltaTime);
            float torqueY = yRotationPID.GetOutput(error.y, Time.deltaTime);
            float torqueZ = zRotationPID.GetOutput(error.z, Time.deltaTime);
            _rigidbody.AddRelativeTorque(new Vector3(torqueX, torqueY, torqueZ) * rotationForce, ForceMode.Acceleration);
            if(error.sqrMagnitude < 0.5f)
            {
                _rigidbody.AddRelativeForce(Vector3.forward * propulsionForce, ForceMode.Acceleration);
            }
            if ((target.position - transform.position).sqrMagnitude < detonationDistance * detonationDistance)
            {
                Detonate();
            }
        }
        else
        {
            if(active)
                _rigidbody.AddRelativeForce(Vector3.forward * propulsionForce, ForceMode.Acceleration);
        }
    }

    public void Activate(float delay)
    {
        StartCoroutine(ActivateRoutine(delay));
    }

    IEnumerator ActivateRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        active = true;
        GetComponent<Collider>().enabled = true;
    }

    private void Detonate()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Here " + collision.transform.name);
        Detonate();
    }
}
