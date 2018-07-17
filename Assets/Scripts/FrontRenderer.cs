using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontRenderer : MonoBehaviour {

    //[SerializeField] private Material textureMixerMaterial;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        //textureMixerMaterial.SetTexture("hoge", )
        Graphics.Blit(src, dest);
    }
}
