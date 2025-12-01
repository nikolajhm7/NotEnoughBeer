using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
	[Header("Refs")]
	public GridManager Grid;
	public PlayerMovement Movement;

	[Header("Settings")]
	public float attackCooldown = 0.2f; 
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

		
		var hits = Grid.GetInteractablesAt(targetCell).ToList();
		if (hits.Count == 0) return; 

		
		foreach (var hit in hits)
		{
			if (hit is IDamageable d)
			{
				d.TakeDamage(1); 
			}
		}
	}
}
