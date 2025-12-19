using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;


public class GameOverUI : MonoBehaviour
{
	public RatDamageApplier ratDamage;

	[Header("Refs")]
	public GameObject panel;             
	public TMP_Text finalScoreText;
	public TMP_Text highScoreText;

	[Header("Settings")]
	public string continueSceneName = "Main";     

	private void Start()
	{
		if (panel != null)
			panel.SetActive(false);
	}

	public void ShowGameOver()
	{
		if (ScoreSystem.Instance != null)
		{
			// Opdater highscore
			ScoreSystem.Instance.TrySetHighScore();

			int finalScore = ScoreSystem.Instance.TotalScore;
			int highScore = ScoreSystem.Instance.HighScore;

			if (finalScoreText != null)
				finalScoreText.text = $"Din score: {finalScore}";

			if (highScoreText != null)
				highScoreText.text = $"Highscore: {highScore}";
		}

		if (panel != null)
			panel.SetActive(true);

		Time.timeScale = 0f;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public void OnContinueButton()
	{
		Debug.Log("[GameOverUI] Continue pressed");

		Time.timeScale = 1f;

		int score = ScoreSystem.Instance ? ScoreSystem.Instance.TotalScore : 0;
		Debug.Log($"[GameOverUI] Score = {score}");

		if (ratDamage == null)
		{
			Debug.LogError("[GameOverUI] ratDamage IS NULL");
		}
		else
		{
			Debug.Log("[GameOverUI] ratDamage found, applying loss");
			ratDamage.ApplyLossToSaveFile(score);
		}

		ScoreSystem.Instance?.ResetScore();

		SceneManager.LoadScene(continueSceneName);
	}

}
