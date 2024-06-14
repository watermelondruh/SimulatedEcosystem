using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GrassScript : MonoBehaviour
{
	public static GrassScript instance;
    public GameObject grassBlockPrefab; // The prefab to spawn
    public LayerMask groundLayer;  // Layer mask to check if the point is on the ground
    public int numberOfGrassBlocks = 25;
    public float minX = -40.5f;
    public float maxX = 40.5f;
    public float minY = -1.0f;
    public float maxY = 5.0f;
    public float minZ = -40.5f;
    public float maxZ = 40.5f;

    void Start()
    {
        SpawnGrassBlocks();
    }

    void Awake()
    {
        instance = this;
    }

    void SpawnGrassBlocks()
    {
        for (int i = 0; i < numberOfGrassBlocks; i++)
        {
            SpawnGrassBlock();
        }
    }

    public void SpawnGrassBlock()
    {
        while (true)
        {
            Vector3 spawnPosition = GetRandomElevatedPosition();
            Instantiate(grassBlockPrefab, spawnPosition, Quaternion.identity);
        }
    }

    Vector3 GetRandomElevatedPosition()
    {
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        float z = Random.Range(minZ, maxZ);
        return new Vector3(x, 0, z);
    }  
}
