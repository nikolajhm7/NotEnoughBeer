using UnityEngine;

public class MouseEnemy : MonoBehaviour, IInteractable, IDamageable
{
	[Header("Refs")]
	public GridManager grid;

	Vector2Int currentCell;

	public int Priority => 5; // hvis flere interactables, er musen rimelig vigtig

	void Awake()
	{
		if (!grid) grid = FindObjectOfType<GridManager>();
	}

	void OnEnable()
	{
		UpdateCell(force: true);
	}

	void Update()
	{
		// hvis den bevæger sig (fx RandomWalker), hold registreringen ajour
		UpdateCell(force: false);
	}

	void UpdateCell(bool force)
	{
		if (!grid) return;

		var c = grid.WorldToGrid(transform.position);
		if (force || c != currentCell)
		{
			if (!force) grid.UnregisterInteractable(this);
			currentCell = c;
			grid.RegisterInteractableCells(new[] { currentCell }, this);
		}
	}

	// --- IInteractable: bruges kun hvis du også vil kunne trykke [F], men skader ikke at have med ---
	public bool CanInteract(PlayerInteractor interactor) => true;
	public void Interact(PlayerInteractor interactor) => Die();
	public string GetInteractionDescription(PlayerInteractor interactor) => "Slå musen (insta-kill)";

	// --- IDamageable: kaldt af Bobs slag ---
	public void TakeDamage(int amount)
	{
		// Ignorer amount – musen dør af første hit
		Die();
	}

	void Die()
	{
		if (grid) grid.UnregisterInteractable(this);
		Destroy(gameObject);
	}

	void OnDisable()
	{
		if (grid) grid.UnregisterInteractable(this);
	}
}
