using UnityEngine;

public class StorageContainerRegister : MonoBehaviour
{
    StorageContainer _c;

    void Awake() => _c = GetComponent<StorageContainer>();

    void OnEnable()
    {
        if (StorageRegistry.Instance && _c)
            StorageRegistry.Instance.Register(_c);
    }

    void OnDisable()
    {
        if (StorageRegistry.Instance && _c)
            StorageRegistry.Instance.Unregister(_c);
    }
}
