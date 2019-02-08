using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindAttachTexture : MonoBehaviour
{
    public Camera textureFromThiscamera;
    void Start() {
        if (!textureFromThiscamera) {return;}

        RenderTexture newTex = textureFromThiscamera.targetTexture;
        gameObject.GetComponent<Renderer>().material.SetTexture("_MainTexA",newTex);
    }

}
