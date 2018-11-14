using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ReplaceShader : MonoBehaviour {

    public float ZOpt = 10;
    public float ZMag = 100;
    Camera dc;
    Shader replacementShader = null;
    /**
		_Zopt("Zopt",Float) = 0.0
		_Zd("Zd",Float) = 0.5
    **/

    // Use this for initialization
    void Start () {
        dc = GetComponent<Camera>();
        Debug.Log(dc);
        replacementShader = Shader.Find("Custom/ReplacementShader");
        if (replacementShader == null)
        {
            Debug.Log("Rplcmt not fond");
        }
        dc.SetReplacementShader(replacementShader, "");
    }


    private void OnValidate()
    {
        Shader.SetGlobalFloat("_ZOpt", ZOpt);
        Shader.SetGlobalFloat("_ZMag", ZMag);
    }
    // Update is called once per frame
    void Update () {
     
    }
}
