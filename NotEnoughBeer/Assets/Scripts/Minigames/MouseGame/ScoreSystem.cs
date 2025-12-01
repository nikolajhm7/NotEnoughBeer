using UnityEngine;
using TMPro;   

public class ScoreSystem : MonoBehaviour
{
	public static ScoreSystem Instance { get; private set; }

	public int TotalScore { get; private set; } = 0;
	public int HighScore { get; private set; } = 0;

	[Header("UI")]
	[SerializeField] private TMP_Text scoreText; 

	const string HighScoreKey = "HighScore";

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

	
		HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
		UpdateUI();
	}

	public void AddScore(int amount)
	{
		if (amount <= 0) return;

		TotalScore += amount;
		UpdateUI();
	}

	public void ResetScore()
	{
		TotalScore = 0;
		UpdateUI();
	}

	public void TrySetHighScore()
	{
		if (TotalScore > HighScore)
		{
			HighScore = TotalScore;
			PlayerPrefs.SetInt(HighScoreKey, HighScore);
			PlayerPrefs.Save();
		}
	}

	private void UpdateUI()
	{
		if (scoreText != null)
		{
			scoreText.text = $"Score: {TotalScore}";
		}
	}
}
