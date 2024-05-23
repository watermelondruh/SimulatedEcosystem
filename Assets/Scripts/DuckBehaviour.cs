using UnityEngine;
using UnityEngine.AI;

public class DuckBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;
    public float walkRadius;
    public float constantSpeed = 3.5f;
    public float destinationThreshold = 1.0f; // Distance to determine if the destination is reached

    private bool overrideDestination = false;
    private Vector3 newDestination;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = constantSpeed; // Set the constant speed
        SetRandomDestination(); // Set the initial destination
    }

    void Update()
    {
        MaintainConstantSpeed();

        // Check if the agent is close to the destination
        if (!agent.pathPending && agent.remainingDistance <= destinationThreshold)
        {
            SetRandomDestination();
        }
    }

    void SetRandomDestination()
    {
        if (overrideDestination)
        {
            agent.SetDestination(newDestination);
            overrideDestination = false;
        }
        else
        {
            Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
            randomDirection += transform.position;
        
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1))
            {
                Vector3 finalPosition = hit.position;
                agent.SetDestination(finalPosition);
            }
        }
    }

    void MaintainConstantSpeed()
    {
        if (agent.velocity.magnitude > 0)
        {
            agent.velocity = agent.velocity.normalized * constantSpeed;
        }
    }

    public void OverrideDestination(Vector3 destination)
    {
        newDestination = destination;
        overrideDestination = true;
    }
}
