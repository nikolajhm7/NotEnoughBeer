using System.Collections.Generic;
using UnityEngine;

public class StorageRegistry : MonoBehaviour
{
    public static StorageRegistry Instance;

    readonly List<StorageContainer> _containers = new();

    public IReadOnlyList<StorageContainer> Containers => _containers;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(StorageContainer c)
    {
        if (c && !_containers.Contains(c)) _containers.Add(c);
    }

    public void Unregister(StorageContainer c)
    {
        _containers.Remove(c);
    }
}
