using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;   // ligesom i PlayerInteractor

public class RatAttackManager : MonoBehaviour
{
	[Header("Minigame")]
	public string minigameSceneName = "Milo Scene";

	[Header("Input")]
	// + på numpad – kan ændres i Inspector
	public Key triggerKey = Key.NumpadPlus;

	[Header("Random rotteangreb (kan slås fra)")]
	public bool enableRandomAttacks = false;
	public float minDelay = 30f;
	public float maxDelay = 90f;

	private float nextAttackTime;

	void Start()
	{
		if (enableRandomAttacks)
		{
			ScheduleNextAttack();
		}
	}

	void Update()
	{
		// 1) Manuel start med + (samme stil som PlayerInteractor)
		var kb = Keyboard.current;
		if (kb != null && kb[triggerKey].wasPressedThisFrame)
		{
			StartMinigame();
		}

		// 2) Random start (hvis du slår det til)
		if (enableRandomAttacks && Time.time >= nextAttackTime)
		{
			StartMinigame();
			ScheduleNextAttack();
		}
	}

	private void StartMinigame()
	{
		// Sørg for at tiden kører normalt når vi skifter scene
		Time.timeScale = 1f;

		Debug.Log("Starter minigame: " + minigameSceneName);
		SceneManager.LoadScene(minigameSceneName);
	}

	private void ScheduleNextAttack()
	{
		float delay = Random.Range(minDelay, maxDelay);
		nextAttackTime = Time.time + delay;
	}
}
