using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RatDamageApplier : MonoBehaviour
{
	[Header("Balance")]
	[Range(0f, 1f)] public float maxLossPercent = 0.55f;

	[Tooltip("X=score, Y=loss01 (0..1). 189=>0 tab. 140=>max tab.")]
	public AnimationCurve lossFromScore = new AnimationCurve(
		new Keyframe(140, 0.7f),
		new Keyframe(160, 0.6f),
		new Keyframe(170, 0.35f),
		new Keyframe(180, 0.15f),
		new Keyframe(188, 0.05f),
		new Keyframe(189, 0.0f)
	);

	[Header("What gets eaten")]
	public bool affectPocket = true;
	public bool affectContainers = true;

	[Tooltip("Hvis true: mindst 1 fjernes pr item-type der findes, når loss>0.")]
	public bool minimumOnePerTypeIfAnyLoss = false;

	// ItemIds vi vil reducere (tilpas hvis du ikke bruger rarities)
	static readonly ItemId[] Targets =
	{
		ItemId.Barley,
		ItemId.Yeast,
		ItemId.Bottles,
		ItemId.Beer_Common,
		ItemId.Beer_Uncommon,
		ItemId.Beer_Rare,
		ItemId.Beer_Mythical,
		ItemId.Beer_Legendary
	};

	/// <summary>
	/// Læser save-filen for current slot, fjerner items, og skriver tilbage.
	/// Milo-scenen behøver ikke have PocketInventory/SaveManager i scenen.
	/// </summary>
	public (int removedTotal, float lossPercent) ApplyLossToSaveFile(int score)
	{
		string path = SaveManager.GetSlotPath(SaveManager.CurrentSlot);
		if (!File.Exists(path))
		{
			Debug.LogWarning($"[Rats] No save file at {path}");
			return (0, 0f);
		}

		float lossPercent = Mathf.Clamp01(lossFromScore.Evaluate(score));


		var json = File.ReadAllText(path);
		var data = JsonUtility.FromJson<SaveData>(json);

		if (data == null)
		{
			Debug.LogWarning("[Rats] SaveData parse failed.");
			return (0, lossPercent);
		}

		int removedTotal = 0;

		foreach (var id in Targets)
		{
			int have = CountInSave(data, id);
			if (have <= 0) continue;

			int toRemove = Mathf.Clamp(Mathf.RoundToInt(have * lossPercent), 0, have);

			if (minimumOnePerTypeIfAnyLoss && lossPercent > 0f && toRemove == 0)
				toRemove = 1;

			if (toRemove <= 0) continue;

			removedTotal += RemoveFromSave(data, id, toRemove);
		}

		// skriv tilbage
		data.SavedAtIsoUtc = System.DateTime.UtcNow.ToString("o");
		var outJson = JsonUtility.ToJson(data, true);
		File.WriteAllText(path, outJson);

		Debug.Log($"[Rats] Score={score} Loss={lossPercent:P0} Removed={removedTotal} -> {path}");
		return (removedTotal, lossPercent);
	}

	int CountInSave(SaveData data, ItemId id)
	{
		int total = 0;
		int idInt = (int)id;

		if (affectPocket && data.PocketItems != null)
			total += CountStacks(data.PocketItems, id);

		if (affectContainers && data.Containers != null)
		{
			foreach (var c in data.Containers)
				if (c != null && c.Items != null)
					total += CountStacks(c.Items, id);
		}

		return total;
	}

	int RemoveFromSave(SaveData data, ItemId id, int amount)
	{
		int left = amount;

		// pocket først
		if (affectPocket && data.PocketItems != null && left > 0)
			left -= RemoveFromStacks(data.PocketItems, id, left);

		// så containers
		if (affectContainers && data.Containers != null && left > 0)
		{
			foreach (var c in data.Containers)
			{
				if (c == null || c.Items == null || left <= 0) continue;
				left -= RemoveFromStacks(c.Items, id, left);
			}
		}

		return amount - left;
	}

	int CountStacks(List<ItemStack> stacks, ItemId id)
	{
		int total = 0;
		for (int i = 0; i < stacks.Count; i++)
			if (stacks[i].Id == id) total += stacks[i].Amount;   // matcher din JSON
		return total;
	}

	int RemoveFromStacks(List<ItemStack> stacks, ItemId id, int amount)
	{
		int left = amount;

		// loop baglæns så RemoveAt er sikkert
		for (int i = stacks.Count - 1; i >= 0 && left > 0; i--)
		{
			if (stacks[i].Id != id) continue;

			int take = Mathf.Min(stacks[i].Amount, left);
			stacks[i].Amount -= take;
			left -= take;

			if (stacks[i].Amount <= 0)
				stacks.RemoveAt(i);
		}

		return amount - left;
	}
}
