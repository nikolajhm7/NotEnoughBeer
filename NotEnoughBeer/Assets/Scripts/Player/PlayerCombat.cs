using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
	[Header("Refs")]
	public GridManager Grid;
	public PlayerMovement Movement;

	[Header("Settings")]
	public float attackCooldown = 0.2f; // lidt debounce
	private float lastAttackTime = -999f;

	void Start()
	{
		if (!Grid) Grid = FindObjectOfType<GridManager>();
		if (!Movement) Movement = GetComponent<PlayerMovement>();
	}

	void Update()
	{
		var mouse = Mouse.current;
		if (mouse == null) return;

		if (mouse.leftButton.wasPressedThisFrame)
		{
			TryAttack();
		}
	}

	void TryAttack()
	{
		if (Time.time - lastAttackTime < attackCooldown) return;
		lastAttackTime = Time.time;

		// Cellen foran Bob
		Vector2Int targetCell = Movement.CurrentCell + Movement.CurrentFacingDir;

		// Find alle registrerede interactables i cellen
		var hits = Grid.GetInteractablesAt(targetCell).ToList();
		if (hits.Count == 0) return; // intet i cellen – "sving i luften"

		// Dræb alt i cellen der kan tage skade (musen dør instant)
		foreach (var h in hits)
		{
			if (h is IDamageable d)
			{
				d.TakeDamage(1); // dør straks uanset værdi
			}
		}
	}
}
