using UnityEngine;
using System.Collections;

public class Skin : MonoBehaviour {
	[SerializeField]
	private MeshRenderer skinRenderer;
	[SerializeField]
	private MeshRenderer fleshRenderer;

	public Color Color {
		get { return skinRenderer.material.color; }
		set { skinRenderer.material.color = value; }
	}

	public float Alpha {
		get { return skinRenderer.material.color.a; }
		set {
			var c1 = skinRenderer.material.color;
			var c2 = fleshRenderer.material.color;
			c1.a = value;
			c2.a = value;
			skinRenderer.material.color = c1;
			fleshRenderer.material.color = c2;
		}
	}
}
