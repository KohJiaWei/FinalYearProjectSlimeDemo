using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SlimeSpawner : MonoBehaviour
{
    public GameObject slimePrefab; // Assign your slime prefab in the Inspector
    public int numberOfSlimes = 10; // Number of slimes to spawn
    public Vector2 mapSize = new Vector2(50, 50); // Size of the map (width, height)

    void Start()
    {
        SpawnSlimes();
    }

    void SpawnSlimes()
    {
        for (int i = 0; i < numberOfSlimes; i++)
        {
            // Generate a random position within the map boundaries
            Vector2 randomPosition = new Vector2(
                Random.Range(-mapSize.x / 2, mapSize.x / 2),
                Random.Range(-mapSize.y / 2, mapSize.y / 2)
            );

            // Instantiate the slime at the random position
            Instantiate(slimePrefab, randomPosition, Quaternion.identity);
        }
    }
}
