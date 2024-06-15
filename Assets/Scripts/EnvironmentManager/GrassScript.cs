using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class GrassScript : MonoBehaviour
{
	public static GrassScript instance;
    public LayerMask groundLayer;  // Layer mask to check if the point is on the ground
    public NavMeshSurface navSurface;

    public GameObject forestMountainPrefab;
    public float forestMountainScale = 0.75f;
    public Vector3 forestMountainPos = new Vector3(20.0f, -0.5f, -20.0f);

    public GameObject lakePrefab;
    public float lakeScale = 1.0f;
    public Vector3 lakePos = new Vector3(-19.0f, -0.75f, -15.5f);

    public GameObject meadowPrefab;
    public float meadowScale = 1.0f;
    public Vector3 meadowPos = new Vector3(-20.0f, -1.5f, 15.0f);

    public GameObject rockPrefab;
    public int numberOfRocks = 15;
    public float rockScale = 0.5f;

    public float minX = -40.5f;
    public float maxX = 40.5f;
    public float minY = 0.0f;
    public float maxY = 3.0f;
    public float minZ = -40.5f;
    public float maxZ = 40.5f;

    void Start()
    {
        SpawnTerrain();
        navSurface.BuildNavMesh();
    }

    void Awake()
    {
        instance = this;
    }

    // TODO: function to generate random coordinates appropriate for each terrain object
    // lake on water, meadow on grass, rock on grass, etc.
    void SpawnTerrain()
    {
        SpawnObjectAtPos(forestMountainPrefab, forestMountainScale, forestMountainPos);
        SpawnObjectAtPos(lakePrefab, lakeScale, lakePos);
        SpawnObjectAtPos(meadowPrefab, meadowScale, meadowPos);
        for (int i = 0; i < numberOfRocks; i++) {
            SpawnObject(rockPrefab, rockScale);
        }
    }

    public void SpawnObjectAtPos(GameObject prefabObject, float scale, Vector3 spawnPosition)
    {
        while (true)
        {
            GameObject obj = Instantiate(prefabObject, spawnPosition, Quaternion.identity);
            obj.transform.localScale = new Vector3(scale, scale, scale);
            break;
        }
    }

    public void SpawnObject(GameObject prefabObject, float scale)
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
                // GameObject obj;
                GameObject obj = Instantiate(prefabObject, spawnPosition, Quaternion.identity);
                if (scale != 1.0) {
                    obj.transform.localScale = new Vector3(scale, scale, scale);
                }
                break;
            }
        }
    }

    Vector3 GetRandomPosition()
    {
        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);
        return new Vector3(x, 0, z);
    }

    Vector3 GetRandomElevatedPosition()
    {
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        float z = Random.Range(minZ, maxZ);
        return new Vector3(x, y, z);
    }
}
