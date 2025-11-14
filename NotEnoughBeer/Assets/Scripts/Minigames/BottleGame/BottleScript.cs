using UnityEngine;

public class BottleScript : MonoBehaviour
{
    void Update()
    {
        // Get the right edge of the screen in world coordinates
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));
        
        // If bottle has passed the right edge, destroy it
        if (transform.position.x > rightEdge.x)
        {
            Destroy(gameObject);
        }
    }
}
