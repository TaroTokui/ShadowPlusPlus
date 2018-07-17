using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureMixer : MonoBehaviour {

    [SerializeField] private Material textureMixerMaterial;
    [SerializeField] private RenderTexture FrontTexture;
    [SerializeField] private RenderTexture RearTexture;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        //textureMixerMaterial.SetTexture("FrontTexture", FrontTexture);
        //textureMixerMaterial.SetTexture("RearTexture", RearTexture);
        Graphics.Blit(src, dest, textureMixerMaterial);
        //Graphics.Blit(src, dest);
    }
}
