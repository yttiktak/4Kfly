using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burble : MonoBehaviour {
	public Vector4 colorMagnitudes;
	Material theMaterial;

	// Use this for initialization
	void Start () {
		SkinnedMeshRenderer smr = gameObject.GetComponent<SkinnedMeshRenderer> ();
		Renderer por = gameObject.GetComponent<Renderer> ();
		if (smr != null) {
			theMaterial = smr.material;
		} else {
			theMaterial = por.material;
		}
	}
	
	// Update is called once per frame
	void Update () {
		Vector4 burbleV = new Vector4 (Random.Range (0.03f, 1.0f), Random.Range (0f, 1.0f), Random.Range (0.08f, 1.0f),-0.01f);
		burbleV.x *= colorMagnitudes.x;
		burbleV.y *= colorMagnitudes.y;
		burbleV.z *= colorMagnitudes.z;
		theMaterial.SetVector ("_hydrophobic_color",  burbleV);
	}
}
