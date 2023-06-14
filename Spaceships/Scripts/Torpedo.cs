using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torpedo : MonoBehaviour
{
    public Transform target;
    public float detonationDistance = 5;
    [SerializeField] float propulsionForce = 1000;
    [SerializeField] float translationForce = 50;
    [SerializeField] float rotationForce = 100;
    Rigidbody _rigidbody;
    PIDController translationPID;
    PIDController rotationPID;
    [SerializeField] float p, i, d;
    bool active;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        translationPID = new PIDController(p, i, d);
        rotationPID = new PIDController(p, i ,d);
    }

    void Update()
    {
        if (target)
        {
            Vector3 targetDirection = target.position - transform.position;
            Vector3 error = targetDirection - transform.forward;
            float torqueX = Mathf.Clamp(rotationPID.GetOutput(error.x, Time.deltaTime), -rotationForce, rotationForce);
            float torqueY = Mathf.Clamp(rotationPID.GetOutput(error.y, Time.deltaTime), -rotationForce, rotationForce);
            float torqueZ = Mathf.Clamp(rotationPID.GetOutput(error.z, Time.deltaTime), -rotationForce, rotationForce);
            _rigidbody.AddRelativeTorque(new Vector3(torqueX, torqueY, torqueZ) * rotationForce, ForceMode.Acceleration);
            if(error.sqrMagnitude < 4)
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
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Here " + collision.transform.name);
        Detonate();
    }
}
