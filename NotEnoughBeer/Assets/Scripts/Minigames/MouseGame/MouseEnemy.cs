using UnityEngine;

public class MouseEnemy : MonoBehaviour, IInteractable, IDamageable
{
	[Header("Refs")]
	public GridManager grid;

	Vector2Int currentCell;

	public int Priority => 5;
	private Vector3 lastPos;

	[Header("Score")]
	public float startScore = 20f;          
	public float scoreDecayPerSecond = 0.5f;  
	private float timeAlive = 0f;

	public static int totalDead = 0;

	void Awake()
	{
		if (!grid) grid = FindObjectOfType<GridManager>();
	}

	void Start()
	{
		lastPos = transform.position;
	}

	void OnEnable()
	{
		UpdateCell(force: true);
	}

	void Update()
	{
		timeAlive += Time.deltaTime;
		Vector3 delta = transform.position - lastPos;
		delta.y = 0f;


		if (delta.sqrMagnitude > 0.0001f)
		{
			Quaternion targetRot = Quaternion.LookRotation(delta, Vector3.up);

			
			targetRot *= Quaternion.Euler(0f, -90f, 0f);

			transform.rotation = targetRot;
		}

		lastPos = transform.position;
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

	
	public bool CanInteract(PlayerInteractor interactor) => true;
	public void Interact(PlayerInteractor interactor) => Die();
	public string GetInteractionDescription(PlayerInteractor interactor) => "Hit";

	
	public void TakeDamage(int amount)
	{
		
		Die();
	}

	void Die()
	{
		
		if (ScoreSystem.Instance != null)
		{
			float rawScore = startScore - timeAlive * scoreDecayPerSecond;
			int points = Mathf.Max(0, Mathf.RoundToInt(rawScore));
			ScoreSystem.Instance.AddScore(points);
		}

		
		if (grid) grid.UnregisterInteractable(this);


		totalDead++;
		Debug.Log($"Mouse died. totalDead={totalDead}, totalSpawned={ObjectSpawner.totalSpawned}, finishedSpawning={ObjectSpawner.finishedSpawning}");

		if (ObjectSpawner.finishedSpawning &&
			totalDead >= ObjectSpawner.totalSpawned)
		{
			Debug.Log("Alle mus døde -> EndRound()");
			if (RoundManager.Instance != null)
				RoundManager.Instance.EndRound();
			else
				Debug.LogWarning("RoundManager.Instance er NULL");
		}

		Destroy(gameObject);
	}

	void OnDisable()
	{
		if (grid) grid.UnregisterInteractable(this);
	}
}
