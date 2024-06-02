using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrackPlants : MonoBehaviour
{
    public static TrackPlants instance;
    public GameObject plantPrefab; // The prefab to spawn
    public LayerMask groundLayer;  // Layer mask to check if the point is on the ground
    public int numberOfPlants = 5;
    public float minX = -40.5f;
    public float maxX = 40.5f;
    public float minZ = -40.5f;
    public float maxZ = 40.5f;

    void Start()
    {
        SpawnPlants();
    }

    void Awake()
    {
        instance = this;
    }

    void SpawnPlants()
    {
        for (int i = 0; i < numberOfPlants; i++)
        {
            SpawnPlant();
        }
    }

    public void SpawnPlant()
    {
        while (true)
        {
            Vector3 spawnPosition = GetRandomPosition();

            // Raycast downwards from above the point to check if it's on the ground
            Ray ray = new Ray(spawnPosition + Vector3.up * 10, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                // Adjust the spawn position to the hit point on the ground
                spawnPosition = hit.point;
                Instantiate(plantPrefab, spawnPosition, Quaternion.identity);
                break;
            }
        }
    }

    private bool AttemptPlantSpawn()
    {
        Vector3 spawnPosition = GetRandomPosition();

        // Raycast downwards from above the point to check if it's on the ground
        Ray ray = new Ray(spawnPosition + Vector3.up * 10, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            // Adjust the spawn position to the hit point on the ground
            spawnPosition = hit.point;
            Instantiate(plantPrefab, spawnPosition, Quaternion.identity);
            return true;
        }
        else
        {
            // If no valid ground point found, try again (or handle the failure as needed)
            return false;
        }
    }

    Vector3 GetRandomPosition()
    {
        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);
        return new Vector3(x, 0, z);
    }
}
