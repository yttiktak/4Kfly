using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;


public class MakeTex2dArray : MonoBehaviour
{

    static public int size = 128;
    static public int slices = 12;

    [MenuItem("GameObject/Create TextureArray")]
    static void CreateTextureArray()
    {
        RenderTexture renderTexture = new RenderTexture(size,size,24,RenderTextureFormat.ARGB32);
        renderTexture.dimension = TextureDimension.Tex2DArray;
        renderTexture.volumeDepth = slices;
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        AssetDatabase.CreateAsset(renderTexture, "Assets/Tex2dArray.asset");

    }

}
