using TMPro;
using UnityEngine;

public class MaltingScoreHUD : MonoBehaviour
{
	public MaltingScoreGridUI score;
	public TMP_Text text;

	void Update()
	{
		if (score == null || text == null) return;
		text.text = Mathf.RoundToInt(score.CleanPercent * 100f) + "%";
	}
}
