using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RandomMovement : MonoBehaviour
{
    [Header("Patrol")]
    public NavMeshAgent agent;
    public float range = 10f;
    public Transform centrePoint;
    public float patrolSpeed = 3.0f;

    [Header("Chase")]
    public float detectionRadius = 15f;
    public float viewAngle = 60f;
    public float chaseSpeed = 3.6f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
    }

    void Update()
    {
        if (PlayerInSight())
        {
            agent.speed = chaseSpeed;
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                agent.SetDestination(playerObj.transform.position);
            }
        }
        else
        {
            agent.speed = patrolSpeed;

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                Vector3 point;
                if (RandomPoint(centrePoint.position, range, out point))
                {
                    Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
                    agent.SetDestination(point);
                }
            }
        }
    }

    // Determina si el objeto con Tag "Player" está dentro del campo de visión
    // y a una distancia menor o igual a detectionRadius.
    bool PlayerInSight()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return false;

        Vector3 directionToPlayer = playerObj.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > detectionRadius) return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle < viewAngle * 0.5f)
        {
            return true;
        }

        return false;
    }

    // Genera un punto aleatorio dentro de un rango para la patrulla y
    // devuelve ese punto en "result" si es válido para el NavMesh.
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
}
