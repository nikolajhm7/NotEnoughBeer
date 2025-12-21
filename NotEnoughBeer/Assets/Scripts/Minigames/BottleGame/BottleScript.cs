using UnityEngine;

public class BottleScript : MonoBehaviour
{
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeIntensity = 0.1f;
    
    void Update()
    {
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));
        if (transform.position.x > rightEdge.x)
        {
            Destroy(gameObject);
        }
    }
    
    public void Shake()
    {
        StartCoroutine(ShakeCoroutine());
    }
    
    private System.Collections.IEnumerator ShakeCoroutine()
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float x = originalPosition.x + Random.Range(-shakeIntensity, shakeIntensity);
            float y = originalPosition.y + Random.Range(-shakeIntensity, shakeIntensity);
            
            transform.position = new Vector3(x, y, originalPosition.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = originalPosition;
    }
}
