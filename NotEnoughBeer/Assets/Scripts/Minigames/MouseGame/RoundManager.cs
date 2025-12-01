using UnityEngine;

public class RoundManager : MonoBehaviour
{
	public static RoundManager Instance { get; private set; }

	[Header("Refs")]
	public GameOverUI gameOverUI;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	public void EndRound()
	{
		Debug.Log("RoundManager.EndRound() called");

		if (gameOverUI == null)
		{
			gameOverUI = FindObjectOfType<GameOverUI>();
			Debug.Log($"FindObjectOfType<GameOverUI>() => {gameOverUI}");
		}

		if (gameOverUI != null)
		{
			gameOverUI.ShowGameOver();
		}
		else
		{
			Debug.LogWarning("RoundManager: GameOverUI reference mangler, og kunne ikke findes i scenen!");
		}
	}

}
