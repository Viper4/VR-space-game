using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicEnemy : MonoBehaviour
{
    NavMeshAgent agent;
    [SerializeField] Gun gun;
    [SerializeField] Transform target;
    [SerializeField] float rotateSpeed = 10;
    [SerializeField] float fireSpeed = 1;
    bool firing = false;
    Vector3 targetDirection;
    bool targetVisible;

    float angularSpeed;

    [SerializeField] float fieldOfView = 45;
    enum State
    {
        Idle,
        Patrol,
        Wander
    }
    [SerializeField] State state = State.Idle;
    bool checkState = true;

    [SerializeField] LayerMask coverLayers;
    Transform cover;

    // Idle settings

    // Patrol settings
    [SerializeField] Transform[] patrolPoints;
    int previousPatrolPoint = 0;

    // Wander settings
    [SerializeField] float wanderDistance;
    [SerializeField] Vector2 restTime;

    Coroutine stateRoutine;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        angularSpeed = agent.angularSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        targetDirection = target.position - gun.transform.position;
        float angleToTarget = Vector3.Angle(transform.forward, targetDirection);
        targetVisible = Physics.Raycast(transform.position, targetDirection, out RaycastHit hit, Mathf.Infinity);
        if (angleToTarget <= fieldOfView && targetVisible && hit.transform.CompareTag("Player"))
        {
            if (stateRoutine != null)
            {
                agent.isStopped = true;
                StopCoroutine(stateRoutine);
                checkState = true;
            }
            float ammo = gun.AmmoCount();
            if (ammo > 0)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(new Vector3(targetDirection.x, 0, targetDirection.z)), rotateSpeed);
                if (!firing && angleToTarget < 5)
                {
                    gun.transform.rotation = Quaternion.RotateTowards(gun.transform.rotation, Quaternion.LookRotation(targetDirection, transform.up), rotateSpeed);
                    StartCoroutine(Fire());
                }
            }
            if (ammo / gun.MaxAmmo() < 0.5f)
            {
                if (cover == null)
                {
                    StartCoroutine(FindCover());
                }
            }
        }
        else if (checkState)
        {
            gun.transform.rotation = Quaternion.RotateTowards(gun.transform.rotation, Quaternion.identity, rotateSpeed);
            stateRoutine = StartCoroutine(StateAction());
        }
    }

    bool AtDestination()
    {
        return agent.remainingDistance <= agent.stoppingDistance;
    }

    IEnumerator StateAction()
    {
        checkState = false;
        switch (state)
        {
            case State.Idle:
                break;
            case State.Patrol:
                if(patrolPoints.Length != 0)
                {
                    previousPatrolPoint++;
                    Transform patrolPoint = patrolPoints[previousPatrolPoint];
                    if (previousPatrolPoint >= patrolPoints.Length - 1)
                        previousPatrolPoint = 0;

                    agent.SetDestination(patrolPoint.position);
                    while (!AtDestination())
                    {
                        yield return new WaitForSecondsRealtime(0.25f);
                    }
                    yield return new WaitForSeconds(1);
                }
                break;
            case State.Wander:
                NavMesh.SamplePosition((Random.insideUnitSphere * wanderDistance) + transform.position, out NavMeshHit hit, wanderDistance, 1);
                agent.SetDestination(hit.position);
                while (!AtDestination())
                {
                    yield return new WaitForSecondsRealtime(0.25f);
                }
                break;
        }
        yield return new WaitForSeconds(Random.Range(restTime.x, restTime.y));
        checkState = true;
    }

    IEnumerator Fire()
    {
        firing = true;
        gun.Fire();
        yield return new WaitForSeconds(fireSpeed);
        firing = false;
    }

    IEnumerator FindCover()
    {
        checkState = false;
        Collider[] coverPoints = Physics.OverlapSphere(transform.position, 25, coverLayers);
        if (coverPoints.Length > 0)
        {
            cover = coverPoints[0].transform;
            float closestDistance = (coverPoints[0].transform.position - transform.position).sqrMagnitude;
            for (int i = 1; i < coverPoints.Length; i++)
            {
                float distance = (coverPoints[i].transform.position - transform.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    cover = coverPoints[i].transform;
                }
            }
        }
        agent.SetDestination(cover.position);
        agent.angularSpeed = 0;
        while (!AtDestination())
        {
            if (!targetVisible)
            {
                agent.angularSpeed = angularSpeed;
            }
            yield return new WaitForSecondsRealtime(0.25f);
        }
        agent.angularSpeed = angularSpeed;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, cover.rotation, rotateSpeed);
        yield return new WaitForSeconds(3);
        checkState = true;
    }
}

