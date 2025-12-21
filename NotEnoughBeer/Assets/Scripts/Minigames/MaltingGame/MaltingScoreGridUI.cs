using UnityEngine;
using UnityEngine.InputSystem;

public class MaltingScoreGridUI : MonoBehaviour
{
	[Header("Area (UI)")]
	public RectTransform grainArea; // dit GrainArea (Image)

	[Header("Grid")]
	public int cols = 25;
	public int rows = 15;

	[Header("Cleaning")]
	[Range(0.01f, 0.5f)] public float brushRadius01 = 0.10f; // i area-UV (0..1)
	public float cleanRatePerSecond = 2f; // hvor hurtigt man "renser"
	public bool requireHoldMouse0 = true;

	public float CleanPercent { get; private set; } // 0..1

	float[,] progress;

	void OnEnable()
	{
		EnsureInit();
	}

	void EnsureInit()
	{
		if (progress == null || progress.GetLength(0) != cols || progress.GetLength(1) != rows)
		{
			progress = new float[cols, rows];
			Recalculate();
		}
	}


	void Awake()
	{
		progress = new float[cols, rows];
		Recalculate();
	}

	void Update()
	{
		EnsureInit();

		if (grainArea == null) return;
		if (Mouse.current == null) return;

		if (requireHoldMouse0 && !Mouse.current.leftButton.isPressed) return;

		Vector2 screen = Mouse.current.position.ReadValue();

		if (!TryGetAreaUV(screen, out Vector2 uv)) return;

		Paint(uv);
		Recalculate();
	}

	bool TryGetAreaUV(Vector2 screenPos, out Vector2 uv)
	{
		uv = default;

		// screen -> local i rect
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(grainArea, screenPos, null, out Vector2 local))
			return false;

		Rect r = grainArea.rect;

		// inde i området?
		if (local.x < r.xMin || local.x > r.xMax || local.y < r.yMin || local.y > r.yMax)
			return false;

		float u = Mathf.InverseLerp(r.xMin, r.xMax, local.x);
		float v = Mathf.InverseLerp(r.yMin, r.yMax, local.y);

		uv = new Vector2(u, v);
		return true;
	}

	void Paint(Vector2 uv)
	{
		if (progress == null) return;
		float r = brushRadius01;
		float r2 = r * r;
		float delta = cleanRatePerSecond * Time.deltaTime;

		int minX = Mathf.Clamp(Mathf.FloorToInt((uv.x - r) * cols), 0, cols - 1);
		int maxX = Mathf.Clamp(Mathf.FloorToInt((uv.x + r) * cols), 0, cols - 1);
		int minY = Mathf.Clamp(Mathf.FloorToInt((uv.y - r) * rows), 0, rows - 1);
		int maxY = Mathf.Clamp(Mathf.FloorToInt((uv.y + r) * rows), 0, rows - 1);

		for (int x = minX; x <= maxX; x++)
		{
			for (int y = minY; y <= maxY; y++)
			{
				Vector2 c = new Vector2((x + 0.5f) / cols, (y + 0.5f) / rows);
				if ((c - uv).sqrMagnitude <= r2)
				{
					progress[x, y] = Mathf.Clamp01(progress[x, y] + delta);
				}
			}
		}
	}

	void Recalculate()
	{
		float sum = 0f;
		for (int x = 0; x < cols; x++)
			for (int y = 0; y < rows; y++)
				sum += progress[x, y];

		CleanPercent = sum / (cols * rows);
	}
}
