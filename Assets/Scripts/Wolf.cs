using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;

public class Wolf : MonoBehaviour
{
    public NavMeshAgent agent;
    public float destinationThreshold = 0.15f; // Distance to determine if the destination is reached
    public float decisionInterval = 2.0f; // Interval for making movement decisions
    public float waitTimeBeforeMove = 1.0f; // Wait time before moving to the new location
    public float moveDistance = 1.0f; // Distance to move each time
    public LayerMask unwalkableLayer; // Layer mask for unwalkable areas

    private bool overrideDestination = false;
    private Vector3 newDestination;
    private Vector3 lastDirection;
    private bool isNearWater = false;

    private bool isNearDuck = false;
    private bool foundWater = false;

    [Header("State")]
    private float hunger;
    private float thirst;

    private float reproductiveUrge;
    float timeToDeathByHunger = 200f;
    float timeToDeathByThirst = 200f;

    float maxUrgeValue = 200f;

    private Animator animator;
    private bool isMakingDecision = false;

    private float timeToDrink = 2f;

    [Header("Reproduction")]

    public bool isMale;
    public bool isReadyToMate = false;

    private bool isMatingCooldown = false;

    public GameObject wolfPrefab;

    [Header("Inheritable")]
    public float constantSpeed = 3.5f;

    public float hungerRate = 1;

    public float thirstRate = 1;

    public float reproductiveUrgeRate = 1;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.speed = constantSpeed;
        lastDirection = UnityEngine.Random.insideUnitSphere.normalized;
        isMale = UnityEngine.Random.Range(0, 2) == 0;
    }

    void Update()
    {
        // Check if the agent is close to the destination
        if (agent.remainingDistance <= destinationThreshold)
        {
            if (!isMakingDecision)
            {
                agent.isStopped = true;
                isMakingDecision = true;
                StartCoroutine(MakeDecisionAfterDelay());
            }
        }

        // Check if the agent is near water or Duck
        isNearWater = HelperFunctions.CheckIsNearObject(transform, "Water", 2f);
        isNearDuck = HelperFunctions.CheckIsNearObject(transform, "Duck", 1.0f);

        hunger += Time.deltaTime * hungerRate / timeToDeathByHunger;
        thirst += Time.deltaTime * thirstRate / timeToDeathByThirst;
        reproductiveUrge += Time.deltaTime * reproductiveUrgeRate / maxUrgeValue;

        if (hunger >= 1 || thirst >= 1)
        {
            // delete
            delete();
        }

        if (!isMatingCooldown)
        {
            isReadyToMate = true;
        }
        else
        {
            isReadyToMate = false;
        }
    }

    IEnumerator MakeDecisionAfterDelay()
    {
        yield return new WaitForSeconds(waitTimeBeforeMove);

        if(reproductiveUrge > hunger && reproductiveUrge > thirst)
        {
            if(isReadyToMate)
            {
                if(isMale)
                {
                    if(HelperFunctions.CheckIsNearObject(transform, "Wolf", 1f, true, false))
                    {
                        Mate();
                    }
                    else
                    {
                        HelperFunctions.LookingForObject(transform, ref agent, "Wolf", moveDistance, destinationThreshold, ref lastDirection, true, false);
                    }
                }
                else
                {
                    if(HelperFunctions.CheckIsNearObject(transform, "Wolf", 1f, true, true))
                    {
                        Birth();
                    }
                    else
                    {
                        HelperFunctions.LookingForObject(transform, ref agent, "Wolf", moveDistance, destinationThreshold, ref lastDirection, true, true);
                    }
                }
            }
            else
            {
                HelperFunctions.Exploring(transform, ref agent, ref lastDirection, moveDistance);
            }
        }
        else if(hunger > thirst && hunger > .3f)
        {
            if (!isNearDuck)
            {
                HelperFunctions.LookingForObject(transform, ref agent, "Duck", moveDistance, destinationThreshold, ref lastDirection);
            }
            else
            {
                EatDuck();
            }
        }
        else if(thirst >= hunger && thirst > .3f)
        {
            if(!isNearWater)
            {
                HelperFunctions.LookingForObject(transform, ref agent, "Water", moveDistance, destinationThreshold, ref lastDirection);
            }
            else
            {
                yield return new WaitForSeconds(timeToDrink);
                DrinkWater();
            }
        }
        else
        {
            HelperFunctions.Exploring(transform, ref agent, ref lastDirection, moveDistance);
        }

        agent.isStopped = false;
        isMakingDecision = false; // Reset the flag to allow for new decisions after the current action is finished
    }

    void DrinkWater()
    {
        thirst = 0f;
        agent.isStopped = false;    
    }

    void EatDuck()
    {
        hunger = 0f;
        Collider collider = HelperFunctions.GetClosestObject(transform, "Duck", 1.0f);
        if (collider != null)
        {
            GameObject DuckObject = collider.gameObject;
            Destroy(DuckObject);
        }
        agent.isStopped = false;  
    }

    void Mate()
    {
        Debug.Log("Mated");
        isReadyToMate = false;
        isMatingCooldown = true;
        reproductiveUrge = 0;
        StartCoroutine(MatingCooldown());
    }

    void Birth()
    {
        Collider collider = HelperFunctions.GetClosestObject(transform, "Wolf", 1.0f, true, true);
        Wolf father = collider.GetComponent<Wolf>();

        GameObject babyDuck = Instantiate(wolfPrefab, transform.position, Quaternion.identity);
        babyDuck.GetComponent<Wolf>().InheritGenes(father, this);
        Mate();
    }

    public void InheritGenes(Wolf Father, Wolf Mother)
    {
        float meanSpeed = (Father.constantSpeed + Mother.constantSpeed) / 2;

        float stdDev = 1f;
        float sampledSpeed = HelperFunctions.SampleNormalDistribution(meanSpeed, stdDev);

        constantSpeed = sampledSpeed;

        float RateOfHungerThirst = DetermineThirstAndHungerRate(constantSpeed);

        thirstRate = RateOfHungerThirst;
        hungerRate = RateOfHungerThirst;
    }

    public static float DetermineThirstAndHungerRate(float speed)
    {
        // default speed is 3.5 if larger than 3.5 get thirstier/hungrier faster
        return speed / 10.0f;
    }
    
    IEnumerator MatingCooldown()
    {
        yield return new WaitForSeconds(30f);
        isMatingCooldown = false;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position to visualize the check radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2.0f);
    }

    private void delete()
    {
        Destroy(gameObject);
    }
}
