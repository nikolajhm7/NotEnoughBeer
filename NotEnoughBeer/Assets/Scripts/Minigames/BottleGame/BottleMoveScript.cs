using UnityEngine;

public class BottleMoveScript : MonoBehaviour
{
    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float moveSpeed = 4f;
    
    private float timeSinceLastSpawn = 0f;

    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
        
        if (timeSinceLastSpawn >= spawnInterval)
        {
            SpawnBottle();
            timeSinceLastSpawn = 0f;
        }
    }

    void SpawnBottle()
    {
        if (bottlePrefab != null)
        {
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
            GameObject bottle = Instantiate(bottlePrefab, spawnPosition, Quaternion.identity);
            
            // Add movement to the spawned bottle
            Rigidbody2D rb = bottle.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = bottle.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
            }
            rb.linearVelocity = Vector3.right * moveSpeed;
        }
    }
}
