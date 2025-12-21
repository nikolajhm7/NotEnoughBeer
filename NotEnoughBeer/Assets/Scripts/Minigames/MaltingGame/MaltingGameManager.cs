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

		
		isScoring = Mouse.current != null && Mouse.current.leftButton.isPressed;

		if (!scoreIsJustFinalCoverage)
		{
			
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
		
		float quality = grid != null ? grid.CleanPercent : 0f;

		
		if (!scoreIsJustFinalCoverage)
		{
			float maxPossible = pointsPerSecond * roundTime;
			quality = maxPossible > 0f ? Mathf.Clamp01(currentScore / maxPossible) : 0f;
		}

		
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
