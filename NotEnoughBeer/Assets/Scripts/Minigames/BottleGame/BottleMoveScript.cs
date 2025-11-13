using UnityEngine;

public class BottleMoveScript : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + (Vector3.right * moveSpeed) * Time.deltaTime;
    }
}
