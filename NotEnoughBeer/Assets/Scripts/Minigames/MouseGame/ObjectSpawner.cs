using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
	[Header("Refs")]
	public GridManager grid;
	public GameObject prefab;

	[Header("Spawn")]
	public int count = 5;
	public bool avoidPerimeter = true;   // hold afstand til ydre kant/vægge
	public int perimeterMargin = 1;      // 1 celle fra kanten

	[Header("Walker Settings (optional)")]
	public float moveSpeed = 3f;
	public float stepPause = 0.1f;

	void Start()
	{
		if (!grid) grid = FindObjectOfType<GridManager>();
		if (!grid || !prefab) { Debug.LogWarning("Missing GridManager or prefab"); return; }

		_ = SpawnMany();
	}

	async Task SpawnMany()
	{
		var candidates = new List<Vector2Int>();

		for (int y = 0; y < grid.height; y++)
			for (int x = 0; x < grid.width; x++)
			{
				var c = new Vector2Int(x, y);
				if (!grid.IsInside(c)) continue;

				if (avoidPerimeter)
				{
					if (x < perimeterMargin || y < perimeterMargin ||
						x >= grid.width - perimeterMargin || y >= grid.height - perimeterMargin)
						continue;
				}

				if (grid.IsWalkable(c))
					candidates.Add(c);
			}

		// Shuffle for tilfældighed
		for (int i = 0; i < candidates.Count; i++)
		{
			int j = Random.Range(i, candidates.Count);
			(candidates[i], candidates[j]) = (candidates[j], candidates[i]);
		}

		int spawned = 0;
		foreach (var cell in candidates)
		{
			if (spawned >= count) break;
			SpawnOneAt(cell);
			spawned++;

			await Task.Delay(3000);
		}
	}

	void SpawnOneAt(Vector2Int cell)
	{
		Vector3 world = grid.GridToWorld(cell);

		// Sæt Y korrekt på toppen af gulv + halv højde af objekt
		float floorTop = grid.GetFloorTopY(cell);
		var go = Instantiate(prefab, world, Quaternion.identity);
		float halfHeight = grid.GetObjectExtentsY(go);
		go.transform.position = new Vector3(world.x, floorTop + halfHeight, world.z);

		// (Valgfrit) Markér startcellen som optaget, hvis du vil undgå overlap.
		// grid.SetOccupied(new[]{ cell }, true);

		// Giv den en RandomWalker, hvis ikke den har
		var walker = go.GetComponent<RandomWalker>();
		if (!walker) walker = go.AddComponent<RandomWalker>();
		walker.Init(grid, cell, moveSpeed, stepPause);
		var enemy = go.GetComponent<MouseEnemy>();
		if (!enemy) enemy = go.AddComponent<MouseEnemy>();
		enemy.grid = grid;
	}
}
