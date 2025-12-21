using UnityEngine;
using System.Collections.Generic;

public class ConveyorBeltScript : MonoBehaviour
{
    [Header("Conveyor Settings")]
    [SerializeField] private float scrollSpeed = 4f;
    [SerializeField] private float spawnRate = 1f;
    
    [Header("Tiling Settings")]
    [SerializeField] private float tileWidth = 4f;
    
    private Camera mainCamera;
    private List<GameObject> conveyorTiles = new List<GameObject>();
    private float spawnX;
    private float timeSinceLastSpawn;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        float screenHeight = mainCamera.orthographicSize * 2f;
        float screenWidth = screenHeight * mainCamera.aspect;
        spawnX = -screenWidth / 2f - tileWidth / 2f;
        
    }

    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
        
        if (timeSinceLastSpawn >= spawnRate)
        {
            timeSinceLastSpawn = 0f;
        }
        
        for (int i = conveyorTiles.Count - 1; i >= 0; i--)
        {
            GameObject tile = conveyorTiles[i];
            if (tile != null)
            {
                tile.transform.position += Vector3.right * scrollSpeed * Time.deltaTime;
                
                float screenHeight = mainCamera.orthographicSize * 2f;
                float screenWidth = screenHeight * mainCamera.aspect;
                float rightEdge = screenWidth / 2f;
                
                if (tile.transform.position.x - tileWidth / 2f > rightEdge)
                {
                    conveyorTiles.RemoveAt(i);
                    Destroy(tile);
                }
            }
        }
    }
}
