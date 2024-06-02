using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public static class HelperFunctions
{
    private static System.Random rand = new System.Random();
    public static bool CheckIsNearObject(Transform transform, string mask, float checkRadius, bool isMating = false, bool lookingForMale = false)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius, LayerMask.GetMask(mask));
        if(isMating)
        {
            colliders = FilterCollidersByGender(colliders, lookingForMale, mask);
        }
        return colliders.Length > 0;
    }

    public static Collider GetClosestObject(Transform transform, string mask, float checkRadius, bool isMating = false, bool lookingForMale = false)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius, LayerMask.GetMask(mask));
        if(isMating)
        {
            colliders = FilterCollidersByGender(colliders, lookingForMale, mask);
        }
        if (colliders.Length > 0)
        {
            Collider closestCollider = null;
            float minDistance = Mathf.Infinity;

            foreach (Collider collider in colliders)
            {
                float distance = Vector3.Distance(collider.transform.position, transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCollider = collider;
                }
            }
            return closestCollider;
        }
        return null;
    }

    public static void Exploring(Transform transform, ref NavMeshAgent agent, ref Vector3 lastDirection, float moveDistance)
    {
        Vector3 biasedDirection = lastDirection + UnityEngine.Random.insideUnitSphere * 0.8f; // slight randomness
        biasedDirection = biasedDirection.normalized * moveDistance;
        Vector3 targetPosition = transform.position + biasedDirection;

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, moveDistance, NavMesh.AllAreas))
        {
            Vector3 finalPosition = hit.position;
            agent.SetDestination(finalPosition);
            lastDirection = (finalPosition - transform.position).normalized;
            agent.isStopped = false;
        }
    }

    public static void LookingForObject(Transform transform, ref NavMeshAgent agent, string mask, float moveDistance, float destinationThreshold, ref Vector3 lastDirection, bool isMating = false, bool lookingForMale = false)
    {
        float checkRadius = 5.0f; // Radius for the overlap check
        Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius, LayerMask.GetMask(mask));
        if(isMating)
        {
            colliders = FilterCollidersByGender(colliders, lookingForMale, mask);
        }
        if (colliders.Length > 0)
        {
            Collider closestCollider = null;
            float minDistance = Mathf.Infinity;

            foreach (Collider collider in colliders)
            {
                float distance = Vector3.Distance(collider.transform.position, transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCollider = collider;
                }
            }

            if (closestCollider != null)
            {
                Vector3 destination;
                destination = closestCollider.transform.position;
                agent.SetDestination(destination);
                agent.isStopped = false;
            }
        }
        else
        {
            Exploring(transform, ref agent, ref lastDirection, moveDistance);
        }
    }

    public static Vector3 FindSafeRunAwayPosition(Transform transform, Collider dangerObject, float safeDistance)
    {
        Vector3 runDirection = (transform.position - dangerObject.transform.position).normalized;
        Vector3 runTo = transform.position + runDirection * safeDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(runTo, out hit, safeDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            // If the target position is invalid, try other directions
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f; // 45 degrees increments
                Vector3 direction = Quaternion.Euler(0, angle, 0) * runDirection;
                Vector3 alternateRunTo = transform.position + direction * safeDistance;
                if (NavMesh.SamplePosition(alternateRunTo, out hit, safeDistance, NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }
        }
        // If no valid position is found, return the current position
        return transform.position;
    }

    private static Collider[] FilterCollidersByGender(Collider[] colliders, bool lookingForMale, string mask)
    {
        List<Collider> filteredColliders = new List<Collider>();

        foreach (Collider collider in colliders)
        {
            if(mask == "Duck")
            {
                Duck duck = collider.GetComponent<Duck>();
                if (duck != null && duck.isMale == lookingForMale && duck.isReadyToMate == true)
                {
                    filteredColliders.Add(collider);
                }
            }
            else
            {
                Wolf wolf = collider.GetComponent<Wolf>();
                if (wolf != null && wolf.isMale == lookingForMale && wolf.isReadyToMate == true)
                {
                    filteredColliders.Add(collider);
                }
            }
            
        }

        return filteredColliders.ToArray();
    }

    public static float SampleNormalDistribution(float mean, float stdDev)
    {
        // Use the Box-Muller transform to generate a normal distribution sample
        double u1 = 1.0 - rand.NextDouble(); // uniform(0,1] random doubles
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); // random normal(0,1)
        double randNormal = mean + stdDev * randStdNormal; // random normal(mean,stdDev)
        return (float)randNormal;
    }

    
    




    
}
