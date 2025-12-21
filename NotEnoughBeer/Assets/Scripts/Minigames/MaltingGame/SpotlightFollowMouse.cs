using UnityEngine;
using UnityEngine.InputSystem;

public class SpotlightFollowMouse : MonoBehaviour
{
	public Material spotlightMaterial;
	[Range(0.05f, 0.5f)] public float radius = 0.18f;
	[Range(0.01f, 0.5f)] public float softness = 0.12f;

	void Update()
	{
		if (spotlightMaterial == null) return;

		Vector2 mouse = Mouse.current.position.ReadValue();

		Vector2 uv = new Vector2(
			mouse.x / Screen.width,
			mouse.y / Screen.height
		);

		spotlightMaterial.SetVector("_Center", new Vector4(uv.x, uv.y, 0, 0));
		spotlightMaterial.SetFloat("_Radius", radius);
		spotlightMaterial.SetFloat("_Softness", softness);
	}
}
