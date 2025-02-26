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

    [Header("Combat")]
    public float attackRange = 2.0f;

    [Header("Animations")]
    public Animator animator;
    public float slashAnimationDuration = 1.08f;

    private GameObject playerObj;
    private bool isAttacking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.speed = patrolSpeed;
    }

    void Update()
    {
        if (isAttacking) return;

        playerObj = GameObject.FindGameObjectWithTag("Character");

        if (playerObj != null && PlayerInSight())
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerObj.transform.position);
            if (distanceToPlayer <= attackRange)
            {
                Attack();
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        agent.speed = patrolSpeed;
        animator.SetBool("isWalking", true);
        animator.SetBool("isRunning", false);

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(centrePoint.position, range, out point))
            {
                agent.SetDestination(point);
            }
        }
    }

    void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", true);
        agent.SetDestination(playerObj.transform.position);
    }

    void Attack()
    {
        isAttacking = true;
        agent.isStopped = true;
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetTrigger("SlashTrigger");
        Invoke("ResetAttack", slashAnimationDuration);
    }

    void ResetAttack()
    {
        agent.isStopped = false;
        isAttacking = false;
    }

    bool PlayerInSight()
    {
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
