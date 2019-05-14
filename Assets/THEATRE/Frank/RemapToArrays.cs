using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// FAILED ATTEMPT
public class RemapToArrays : MonoBehaviour
{

    public Shader unlitTexArray;

void Awake() {

}
void OnValidate(){
    if (gameObject.name == "clone") {
        return;
    }


    GameObject clone = Instantiate(gameObject, gameObject.transform.parent);
    clone.name = "clone";
    clone.transform.Translate(-20, 10, 0); // just to see where it is.


    Mesh originalMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
    Mesh cloneMesh = clone.GetComponent<MeshFilter>().sharedMesh;
    int subMeshCount = originalMesh.subMeshCount;
    Debug.Log("sub mesh count is " + subMeshCount);
    Vector2[] uvs = originalMesh.uv;
    Vector2[] uv2 = new Vector2[uvs.Length];
    Debug.Log("uvs length is " + uvs.Length);
    Debug.Log("and vert count is " + originalMesh.vertexCount);
    int[] indicis;
    for (int i = 0; i < subMeshCount; i++) {
        Debug.Log("idx start " +originalMesh.GetIndexStart(i) + " count " + originalMesh.GetIndexCount(i));
        indicis = originalMesh.GetIndices(i);
        foreach (int j in indicis) {
            uv2[j].x = i;
            uv2[j].y = 0;
        }
    }
    Renderer renderer = gameObject.GetComponent<Renderer>();

    Texture2DArray combinedTex = new Texture2DArray(
            renderer.material.mainTexture.width,
            renderer.material.mainTexture.height,
            subMeshCount,
        TextureFormat.RGBA32,
        false
    );
    for (int mi = 0; mi < renderer.materials.Length; mi++){
        combinedTex.SetPixels(renderer.materials[mi].GetColorArray("_mainTex"),mi);
        DestroyImmediate(clone.GetComponent<Renderer>().materials[mi]);
    }
    cloneMesh.uv2 = uv2;
    clone.GetComponent<MeshFilter>().mesh.uv2 = uv2;
    clone.GetComponent<Renderer>().materials = new Material[1];
    clone.GetComponent<Renderer>().material = new Material(unlitTexArray);
    clone.GetComponent<Renderer>().material.SetTexture("_MainTexA",combinedTex);


}

}
