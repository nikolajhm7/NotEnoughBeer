using UnityEngine;
using System.Collections.Generic;

public class ConveyorBeltScript : MonoBehaviour
{
    [Header("Conveyor Settings")]
    [SerializeField] private float scrollSpeed = 4f;
    [SerializeField] private float spawnRate = 1f; // Time between spawns in seconds
    
    [Header("Tiling Settings")]
    [SerializeField] private float tileWidth = 4f; // Width of 4 tiles
    
    private Camera mainCamera;
    private List<GameObject> conveyorTiles = new List<GameObject>();
    private float spawnX;
    private float timeSinceLastSpawn;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Calculate spawn position (left edge of screen, off-screen)
        float screenHeight = mainCamera.orthographicSize * 2f;
        float screenWidth = screenHeight * mainCamera.aspect;
        spawnX = -screenWidth / 2f - tileWidth / 2f;
        
        // Spawn first tile immediately
    }

    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
        
        // Spawn new tile based on spawn rate
        if (timeSinceLastSpawn >= spawnRate)
        {
            timeSinceLastSpawn = 0f;
        }
        
        // Move all tiles to the right and remove ones that exit the screen
        for (int i = conveyorTiles.Count - 1; i >= 0; i--)
        {
            GameObject tile = conveyorTiles[i];
            if (tile != null)
            {
                tile.transform.position += Vector3.right * scrollSpeed * Time.deltaTime;
                
                // Calculate right edge of screen
                float screenHeight = mainCamera.orthographicSize * 2f;
                float screenWidth = screenHeight * mainCamera.aspect;
                float rightEdge = screenWidth / 2f;
                
                // Remove tile if it's completely off the right side
                if (tile.transform.position.x - tileWidth / 2f > rightEdge)
                {
                    conveyorTiles.RemoveAt(i);
                    Destroy(tile);
                }
            }
        }
    }
}
