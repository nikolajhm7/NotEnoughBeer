using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MaltingGameScorer : MonoBehaviour
{
	[Header("References")]
	public MaltingScoreGridUI grid;              

	[Header("UI Elements")]
	public TextMeshProUGUI scoreText;
	public TextMeshProUGUI timerText;

	[Header("Round Settings")]
	public float roundTime = 20f;

	[Header("Scoring")]
	[Tooltip("Hvor meget vi belønner 'coverage' over tid. 1 = slutscore = coverage ved slut.")]
	public bool scoreIsJustFinalCoverage = true;

	[Tooltip("Hvis false, samler vi points per sekund baseret på current coverage.")]
	public float pointsPerSecond = 1f;

	[Header("Current State")]
	public float currentScore = 0f; 
	public bool isScoring = false;

	float timer;

	void Start()
	{
		if (grid == null)
			grid = FindAnyObjectByType<MaltingScoreGridUI>();

		timer = roundTime;
	}

	void Update()
	{
		timer -= Time.deltaTime;
		if (timer < 0f) timer = 0f;

		// "isScoring" i malting giver mening som: holder man spray nede (Mouse0)?
		// (grid.Update() kører kun når der sprayes, men vi kan vise status her)
		isScoring = Mouse.current != null && Mouse.current.leftButton.isPressed;

		if (!scoreIsJustFinalCoverage)
		{
			// Points tick baseret på hvor rent der er lige nu
			// (så man bliver belønnet for at få coverage op tidligt)
			float qualityNow = grid != null ? grid.CleanPercent : 0f;
			currentScore += (qualityNow * pointsPerSecond) * Time.deltaTime;
		}

		UpdateUI();

		if (timer <= 0f)
		{
			Finish();
		}
	}

	void Finish()
	{
		// Slut-kvalitet:
		float quality = grid != null ? grid.CleanPercent : 0f;

		// Hvis du kører points-model, kan du normalisere:
		// max points = pointsPerSecond * roundTime (hvis quality=1 hele tiden)
		if (!scoreIsJustFinalCoverage)
		{
			float maxPossible = pointsPerSecond * roundTime;
			quality = maxPossible > 0f ? Mathf.Clamp01(currentScore / maxPossible) : 0f;
		}

		// Send tilbage til main game (0..1)
		if (MinigameBridge.Instance != null)
			MinigameBridge.Instance.FinishMinigame(quality);
		else
			Debug.LogWarning("[MaltingGameScorer] MinigameBridge.Instance is null!");
	}

	public float GetQuality01()
	{
		if (grid == null) return 0f;
		return Mathf.Clamp01(grid.CleanPercent);
	}

	public float GetScore()
	{
		return currentScore;
	}

	public int GetScoreInt()
	{
		return Mathf.FloorToInt(currentScore);
	}

	void UpdateUI()
	{
		if (timerText != null)
			timerText.text = $"{timer:0.0}s";

		if (scoreText != null && grid != null)
		{
			int percent = Mathf.RoundToInt(grid.CleanPercent * 100f);
			scoreText.text = $"Clean: {percent}%";
		}
	}
}
