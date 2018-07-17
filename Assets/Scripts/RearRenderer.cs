using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RearRenderer : MonoBehaviour {

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest);
    }
}
