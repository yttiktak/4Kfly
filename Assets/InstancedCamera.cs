using UnityEngine;

public class InstancedCamera : MonoBehaviour {

	static MaterialPropertyBlock camerasPropertyBlock;

	static int cameraPosID = Shader.PropertyToID("_CameraPos");

	void Awake () {
		OnValidate();
	}

	void OnValidate () {
		if (camerasPropertyBlock == null) {
            camerasPropertyBlock = new MaterialPropertyBlock();
		}
        camerasPropertyBlock.SetVector(cameraPosID, Camera.current.transform.position);
		// ?? now what?? .SetPropertyBlock(camerasPropertyBlock);
	}
}