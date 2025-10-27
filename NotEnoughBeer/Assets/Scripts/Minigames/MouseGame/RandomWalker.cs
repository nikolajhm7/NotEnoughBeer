using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWalker : MonoBehaviour
{
	GridManager grid;
	Vector2Int currentCell;
	Vector2Int targetCell;

	float speed = 3f;
	float pause = 0.1f;
	bool isMoving;

	// (Valgfrit) undgå at to walkers står i samme celle:
	bool useOccupancy = false;

	public void Init(GridManager g, Vector2Int startCell, float moveSpeed = 3f, float stepPause = 0.1f, bool useOcc = false)
	{
		grid = g;
		currentCell = startCell;
		speed = moveSpeed;
		pause = stepPause;
		useOccupancy = useOcc;

		if (useOccupancy) grid.SetOccupied(new[] { currentCell }, true);

		PickNextTarget();
		StopAllCoroutines();
		StartCoroutine(MoveRoutine());
	}

	void OnDestroy()
	{
		if (grid != null && useOccupancy)
			grid.SetOccupied(new[] { currentCell }, false);
	}

	IEnumerator MoveRoutine()
	{
		while (true)
		{
			if (!isMoving)
			{
				// Hvis vi allerede er ved targetCell, så vælg en ny
				if (currentCell == targetCell)
				{
					yield return new WaitForSeconds(pause);
					PickNextTarget();
				}

				// Bevæg mod targetCell i world-space
				yield return StartCoroutine(MoveToCell(targetCell));
				currentCell = targetCell;

				// (Valgfrit occupancy)
				if (useOccupancy) grid.SetOccupied(new[] { currentCell }, true);
			}
			yield return null;
		}
	}

	IEnumerator MoveToCell(Vector2Int cell)
	{
		isMoving = true;

		Vector3 dest = grid.GridToWorld(cell);
		float floorTop = grid.GetFloorTopY(cell);
		float halfHeight = grid.GetObjectExtentsY(gameObject);
		dest = new Vector3(dest.x, floorTop + halfHeight, dest.z);

		while ((transform.position - dest).sqrMagnitude > 0.001f)
		{
			transform.position = Vector3.MoveTowards(transform.position, dest, speed * Time.deltaTime);
			// (Valgfrit) kig i bevægelsesretning
			Vector3 dir = (dest - transform.position);
			dir.y = 0f;
			if (dir.sqrMagnitude > 0.0001f)
				transform.forward = Vector3.Lerp(transform.forward, dir.normalized, 10f * Time.deltaTime);

			yield return null;
		}

		transform.position = dest;
		isMoving = false;
	}

	void PickNextTarget()
	{
		// Vælg én af 4 naboer tilfældigt (N,S,Ø,V). Du kan også give vægte eller “levy flights”.
		var candidates = new List<Vector2Int>
		{
			currentCell + Vector2Int.up,
			currentCell + Vector2Int.down,
			currentCell + Vector2Int.left,
			currentCell + Vector2Int.right
		};

		// Filtrér kun til celler, der er inde i gridden og walkable
		candidates.RemoveAll(c => !grid.IsWalkable(c));

		// Hvis occupancy bruges, undgå celler, der er optaget
		if (useOccupancy)
			candidates.RemoveAll(c => grid.IsOccupied(c));

		if (candidates.Count == 0)
		{
			// Stå stille et tick og prøv igen næste frame
			targetCell = currentCell;
			return;
		}

		targetCell = candidates[Random.Range(0, candidates.Count)];

		// (Valgfrit) “reservér” næste celle for at mindske sammenstød
		if (useOccupancy)
		{
			grid.SetOccupied(new[] { currentCell }, false); // vi forlader den
			grid.SetOccupied(new[] { targetCell }, true);   // vi er på vej dertil
		}
	}
}
