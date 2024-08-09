using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SlimeSpawner : MonoBehaviour
{
    public GameObject slimePrefab; // Assign your slime prefab in the Inspector
    public int numberOfSlimes = 1; // Number of slimes to spawn
    public Vector2 mapSize = new Vector2(25, 25); // Size of the map (width, height)
    public float timer = 5;
    public float tickTime = 1;

    void Start()
    {
        SpawnSlimes();
    }
    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0) {
            SpawnSlimes();
            timer = tickTime;
            
        }
    }
    void SpawnSlimes()
    {
        for (int i = 0; i < numberOfSlimes; i++)
        {
            // Generate a random position within the map boundaries
            Vector3 randomPosition = new Vector3(
                Random.Range(-mapSize.x / 2, mapSize.x / 2),
                Random.Range(-mapSize.y / 2, mapSize.y / 2)
            );
            randomPosition += transform.position;

            // Instantiate the slime at the random position
            Instantiate(slimePrefab, randomPosition, Quaternion.identity);
        }
    }
}
