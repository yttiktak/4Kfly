using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enumerate : MonoBehaviour {
	Mesh themesh;
	// Use this for initialization wtf??
	[ExecuteInEditMode]
	void Start () {
		themesh = gameObject.GetComponent<SkinnedMeshRenderer> ().sharedMesh;
		int mlen = themesh.vertexCount;
		Vector2[] theuvs = themesh.uv;
		Debug.Log ("mesh uv count " + theuvs.Length);
		Debug.Log ("mesh count " + mlen);
		if (theuvs.Length != mlen) {
			theuvs = new Vector2[mlen];
		}
		// int stride = (MeshTopology.Quads == themesh.GetTopology (0)) ? 4 : 3; 
		for (int i = 0; i < mlen; i++) { //please do not be array[3][n]
			theuvs[i].x = (1.0f * i) / (1.0f*mlen);
			theuvs[i].y = (1.0f * i) / (1.0f*mlen);
			if (i % 100 == 0) {
				Debug.Log ("uv now " + theuvs [i].x);
			}
		}
		Debug.Log ("big assign loop done");
		// I suspect the verticis are not welded. Import says otherwise.
		themesh.uv = theuvs;
		themesh.UploadMeshData (false);
		Debug.Log ("and uploaded ");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
