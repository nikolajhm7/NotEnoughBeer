using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;   

public class RatAttackManager : MonoBehaviour
{
	[Header("Minigame")]
	public string minigameSceneName = "Milo Scene";

	[Header("Save")]
	[SerializeField] SaveManager saveManager;

	[Header("Input")]
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
		
		var kb = Keyboard.current;
		if (kb != null && kb[triggerKey].wasPressedThisFrame)
		{
			StartMinigame();
		}

		
		if (enableRandomAttacks && Time.time >= nextAttackTime)
		{
			StartMinigame();
			ScheduleNextAttack();
		}
	}

	private void StartMinigame()
	{
		
		Time.timeScale = 1f;

		saveManager.Save();

		Debug.Log("Starter minigame: " + minigameSceneName);
		SceneManager.LoadScene(minigameSceneName);
	}

	private void ScheduleNextAttack()
	{
		float delay = Random.Range(minDelay, maxDelay);
		nextAttackTime = Time.time + delay;
	}
}
