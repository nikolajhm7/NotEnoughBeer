using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
	[Header("Refs")]
	public GameObject panel;             
	public TMP_Text finalScoreText;
	public TMP_Text highScoreText;

	[Header("Settings")]
	public string continueSceneName /*= "Nikolaj Scene"*/;     

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

	// --- Dette er den ENESTE knap ---
	public void OnContinueButton()
	{
		Time.timeScale = 1f;

		// Reset score – så minigamet starter fra 0 næste gang
		ScoreSystem.Instance?.ResetScore();

		// Skift scene
		SceneManager.LoadScene(continueSceneName);
		Debug.Log(continueSceneName);
	}
}
