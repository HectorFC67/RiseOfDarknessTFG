using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class RandomMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public float range;
    public Transform centrePoint;
    public float rotationSpeed = 2f;

    private Coroutine rotateCoroutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.angularSpeed = 0f;
    }

    void Update()
    {
        if (agent.remainingDistance <= agent.stoppingDistance && rotateCoroutine == null)
        {
            Vector3 randomDestination;
            if (RandomPoint(centrePoint.position, range, out randomDestination))
            {
                rotateCoroutine = StartCoroutine(RotateAndMove(randomDestination));
            }
        }
    }

    IEnumerator RotateAndMove(Vector3 destination)
    {
        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion rawLookRotation = Quaternion.LookRotation(direction, Vector3.up);
            Vector3 euler = rawLookRotation.eulerAngles;

            euler.x = -90f;
            euler.z += 90f;

            Quaternion finalRotation = Quaternion.Euler(euler);

            Quaternion startRotation = transform.rotation;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime * rotationSpeed;
                transform.rotation = Quaternion.Slerp(startRotation, finalRotation, t);
                yield return null;
            }

            transform.rotation = finalRotation;
        }

        agent.SetDestination(destination);

        rotateCoroutine = null;
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