using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RandomMovement : MonoBehaviour
{
    // 1. Definimos un enum con los estados posibles
    private enum EnemyState
    {
        Patrol,
        Chase,
        Attack
    }

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

    // 2. Variables internas
    private EnemyState currentState = EnemyState.Patrol;
    private GameObject playerObj;
    private bool isAttacking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.speed = patrolSpeed;  // Velocidad inicial para patrullar
    }

    void Update()
    {
        // Actualizamos la referencia al jugador (por si apareciera/desapareciera dinámicamente)
        playerObj = GameObject.FindGameObjectWithTag("Player");

        // Máquina de estados principal
        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolUpdate();
                break;
            case EnemyState.Chase:
                ChaseUpdate();
                break;
            case EnemyState.Attack:
                AttackUpdate();
                break;
        }
    }

    // --------------------------------------------------
    // PATRULLA
    // --------------------------------------------------
    private void PatrolUpdate()
    {
        // Configurar animaciones y velocidad para patrullar
        agent.speed = patrolSpeed;
        animator.SetBool("isWalking", true);
        animator.SetBool("isRunning", false);

        // Si el enemigo ve al jugador, pasamos a "Chase"
        if (PlayerInSight())
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // Si ya llegó al destino, busca un nuevo punto al azar
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(centrePoint.position, range, out point))
            {
                agent.SetDestination(point);
            }
        }
    }

    // --------------------------------------------------
    // PERSECUCIÓN
    // --------------------------------------------------
    private void ChaseUpdate()
    {
        // Si el enemigo no ve al jugador, regresamos a "Patrol"
        if (!PlayerInSight())
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerObj.transform.position);

        // Si el jugador está a rango de ataque, pasamos a "Attack"
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        // Ajustar animación y velocidad para perseguir
        agent.speed = chaseSpeed;
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", true);

        // Actualizamos la ruta hacia el jugador
        agent.SetDestination(playerObj.transform.position);
    }

    // --------------------------------------------------
    // ATAQUE
    // --------------------------------------------------
    private void AttackUpdate()
    {
        // Mientras esté atacando (reproduciendo la animación), no hacer nada más
        if (isAttacking) return;

        // Verificamos si el jugador sigue estando a la vista
        float distanceToPlayer = (playerObj != null)
            ? Vector3.Distance(transform.position, playerObj.transform.position)
            : Mathf.Infinity;

        // Si no está en la línea de visión o se alejó, cambiamos de estado
        if (!PlayerInSight())
        {
            ChangeState(EnemyState.Patrol);
            return;
        }
        else if (distanceToPlayer > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // Realizar ataque
        isAttacking = true;
        agent.isStopped = true;
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);

        // Dispara la animación de ataque
        animator.SetTrigger("SlashTrigger");

        // Después de la duración de la animación, ejecutamos ResetAttack()
        Invoke(nameof(ResetAttack), slashAnimationDuration);
    }

    // Se llama tras completarse la animación de ataque
    private void ResetAttack()
    {
        agent.isStopped = false;
        isAttacking = false;

        // Revisamos si seguir atacando, perseguir o patrullar
        float distanceToPlayer = (playerObj != null)
            ? Vector3.Distance(transform.position, playerObj.transform.position)
            : Mathf.Infinity;

        if (!PlayerInSight())
        {
            ChangeState(EnemyState.Patrol);
        }
        else if (distanceToPlayer > attackRange)
        {
            ChangeState(EnemyState.Chase);
        }
        else
        {
            // El jugador sigue en rango de ataque → volver a Attack
            ChangeState(EnemyState.Attack);
        }
    }

    // --------------------------------------------------
    // DETECCIÓN DEL JUGADOR
    // --------------------------------------------------
    private bool PlayerInSight()
    {
        if (playerObj == null) return false;

        Vector3 directionToPlayer = playerObj.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Si supera el radio de detección, no lo ve
        if (distanceToPlayer > detectionRadius) return false;

        // Si está fuera del ángulo de visión, no lo ve
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > viewAngle * 0.5f) return false;

        // Por último, revisamos si hay línea de visión usando un raycast
        return HasLineOfSight(directionToPlayer, distanceToPlayer);
    }

    private bool HasLineOfSight(Vector3 directionToPlayer, float distanceToPlayer)
    {
        // Lanzamos el rayo desde una posición un poco más alta (ej. la “cabeza” del enemigo)
        Vector3 origin = transform.position + Vector3.up * 1.5f;

        // Normalizamos la dirección para el raycast
        Vector3 direction = directionToPlayer.normalized;

        // Hacemos el raycast. Si golpea algo que no sea el Player, no tenemos visión directa
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, distanceToPlayer))
        {
            // Comprobar si el collider que golpeamos no es el Player
            if (!hit.collider.CompareTag("Player"))
                return false;
        }
        // Si no golpea nada o golpea al jugador, hay línea de visión
        return true;
    }

    // --------------------------------------------------
    // PUNTO ALEATORIO PARA PATRULLAR
    // --------------------------------------------------
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

    // --------------------------------------------------
    // CAMBIO DE ESTADO
    // --------------------------------------------------
    private void ChangeState(EnemyState newState)
    {
        currentState = newState;
    }
}
