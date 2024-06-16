using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;


public class Duck : MonoBehaviour
{
    public NavMeshAgent agent;
    public float destinationThreshold = 0.15f; // Distance to determine if the destination is reached
    public float waitTimeBeforeMove = 1.0f; // Wait time before moving to the new location
    public float moveDistance = 1.0f; // Distance to move each time
    private Vector3 lastDirection;
    private bool isNearWater = false;

    private bool isNearPlant = false;

    private static System.Random rand = new System.Random();


    [Header("State")]
    private float hunger;
    private float thirst;

    private float reproductiveUrge;
    private float timeToDeathByHunger = 200f;
    private float timeToDeathByThirst = 200f;

    private float maxUrgeValue = 200f;

    private Animator animator;
    private bool isMakingDecision = false;

    private float timeToDrink = 2f;

    [Header("Reproduction")]

    public bool isMale;
    public bool isReadyToMate = false;

    private bool isMatingCooldown = false;

    public GameObject duckPrefab;

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

        // Check if the agent is near water or plant
        isNearWater = HelperFunctions.CheckIsNearObject(transform, "Water", 2f);
        isNearPlant = HelperFunctions.CheckIsNearObject(transform, "Plant", 1.0f);

        reproductiveUrge += Time.deltaTime * reproductiveUrgeRate / maxUrgeValue;
        hunger += Time.deltaTime * hungerRate / timeToDeathByHunger;
        thirst += Time.deltaTime * thirstRate / timeToDeathByThirst;

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
        
        // TODO: run away from both?
        if (HelperFunctions.CheckIsNearObject(transform, "Tiger", 5f))
        {
            RunAwayFromTigers();
        }
        else if (HelperFunctions.CheckIsNearObject(transform, "Wolf", 6f))
        {
            RunAwayFromWolves();
        }
        else if(reproductiveUrge >= hunger && reproductiveUrge >= thirst)
        {
            if(isReadyToMate)
            {
                if(isMale)
                {
                    if(HelperFunctions.CheckIsNearObject(transform, "Duck", 1f, true, false))
                    {
                        Mate();
                    }
                    else
                    {
                        HelperFunctions.LookingForObject(transform, ref agent, "Duck", moveDistance, destinationThreshold, ref lastDirection, true, false);
                    }
                }
                else
                {
                    if(HelperFunctions.CheckIsNearObject(transform, "Duck", 1f, true, true))
                    {
                        Birth();
                    }
                    else
                    {
                        HelperFunctions.LookingForObject(transform, ref agent, "Duck", moveDistance, destinationThreshold, ref lastDirection, true, true);
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
            if (!isNearPlant)
            {
                HelperFunctions.LookingForObject(transform, ref agent, "Plant", moveDistance, destinationThreshold, ref lastDirection);
            }
            else
            {
                yield return new WaitForSeconds(timeToDrink);
                EatPlant();
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

        isMakingDecision = false; // Reset the flag to allow for new decisions after the current action is finished
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
        Collider collider = HelperFunctions.GetClosestObject(transform, "Duck", 1.0f, true, true);
        Duck father = collider.GetComponent<Duck>();

        GameObject babyDuck = Instantiate(duckPrefab, transform.position, Quaternion.identity);
        babyDuck.GetComponent<Duck>().InheritGenes(father, this);
        babyDuck.transform.parent = father.transform.parent;
        Mate();
    }

    IEnumerator MatingCooldown()
    {
        yield return new WaitForSeconds(30f);
        isMatingCooldown = false;
    }

    void DrinkWater()
    {
        thirst = 0f;
        agent.isStopped = false;  
    }

    void EatPlant()
    {
        hunger = 0f;
        Collider collider = HelperFunctions.GetClosestObject(transform, "Plant", 1.0f);
        if (collider != null)
        {
            GameObject plantObject = collider.gameObject;
            Destroy(plantObject);
            TrackPlants.instance.SpawnPlant();
        }
        agent.isStopped = false;  
    }


    void RunAwayFromWolves()
    {
        float detectionRadius = 10.0f; // Radius within which the duck will detect wolves
        Collider closestWolf = HelperFunctions.GetClosestObject(transform, "Wolf", detectionRadius);

        if (closestWolf != null)
        {
            Vector3 safePosition = HelperFunctions.FindSafeRunAwayPosition(transform, closestWolf, 15.0f);
            agent.SetDestination(safePosition);
            agent.isStopped = false;
        }
    }


    void RunAwayFromTigers()
    {
        float detectionRadius = 5.0f; // Radius within which the duck will detect tigers
        Collider closestTiger = HelperFunctions.GetClosestObject(transform, "Tiger", detectionRadius);

        if (closestTiger != null)
        {
            Vector3 safePosition = HelperFunctions.FindSafeRunAwayPosition(transform, closestTiger, 10.0f);
            agent.SetDestination(safePosition);
            agent.isStopped = false;
        }
    }



    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position to visualize the check radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2.0f);
    }

    public void InheritGenes(Duck Father, Duck Mother)
    {
        float meanSpeed = (Father.constantSpeed + Mother.constantSpeed) / 2;

        float stdDev = 1f;
        float sampledSpeed = SampleNormalDistribution(meanSpeed, stdDev);

        constantSpeed = sampledSpeed;

        float RateOfHungerThirst = DetermineThirstAndHungerRate(constantSpeed);

        thirstRate = RateOfHungerThirst;
        hungerRate = RateOfHungerThirst;
    }

    private float SampleNormalDistribution(float mean, float stdDev)
    {
        // Use the Box-Muller transform to generate a normal distribution sample
        double u1 = 1.0 - rand.NextDouble(); // uniform(0,1] random doubles
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); // random normal(0,1)
        double randNormal = mean + stdDev * randStdNormal; // random normal(mean,stdDev)
        return (float)randNormal;
    }

    public float DetermineThirstAndHungerRate(float speed)
    {
        // default speed is 3.5 if larger than 3.5 get thirstier/hungrier faster
        return speed / 3.5f;
    }

    private void delete()
    {
        Destroy(gameObject);
    }
}
