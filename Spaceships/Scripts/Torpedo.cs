using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torpedo : MonoBehaviour
{
    Transform target;
    public float detonationDistance = 5;
    [SerializeField] float propulsionForce = 1000;
    [SerializeField] float translationForce = 50;
    [SerializeField] float rotationForce = 100;
    Rigidbody _rigidbody;
    PIDController rotationPID;
    PIDController translationPID;
    [SerializeField] float p, i, d;
    bool active;

    void Update()
    {
        if (active)
        {
            if (target != null)
            {
                Vector3 targetDirection = target.position - transform.position;
                Vector3 rotationError = targetDirection - transform.forward;
                float torqueX = Mathf.Clamp(rotationPID.GetOutput(rotationError.x, Time.deltaTime), -rotationForce, rotationForce);
                float torqueY = Mathf.Clamp(rotationPID.GetOutput(rotationError.y, Time.deltaTime), -rotationForce, rotationForce);
                float torqueZ = Mathf.Clamp(rotationPID.GetOutput(rotationError.z, Time.deltaTime), -rotationForce, rotationForce);
                _rigidbody.AddTorque(new Vector3(torqueX, torqueY, torqueZ) * rotationForce, ForceMode.Acceleration);

                Vector3 velocityError = targetDirection - _rigidbody.velocity.normalized;
                float forceX = Mathf.Clamp(translationPID.GetOutput(velocityError.x, Time.deltaTime), -translationForce, translationForce);
                float forceY = Mathf.Clamp(translationPID.GetOutput(velocityError.x, Time.deltaTime), -translationForce, translationForce);
                float forceZ = Mathf.Clamp(translationPID.GetOutput(velocityError.x, Time.deltaTime), -translationForce, translationForce);
                _rigidbody.AddForce(new Vector3(forceX, forceY, forceZ));

                if (rotationError.sqrMagnitude < 2)
                    _rigidbody.AddForce(transform.forward * propulsionForce, ForceMode.Acceleration);

                if ((target.position - transform.position).sqrMagnitude < detonationDistance * detonationDistance)
                    Detonate();
            }
            else
            {
                _rigidbody.AddForce(transform.forward * propulsionForce, ForceMode.Acceleration);
            }
        }
    }

    public void Activate(Rigidbody rigidbody, Transform target, float delay)
    {
        _rigidbody = rigidbody;
        this.target = target;
        rotationPID = new PIDController(p, i, d);
        translationPID = new PIDController(p, i, d);
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
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Detonate();
    }
}
