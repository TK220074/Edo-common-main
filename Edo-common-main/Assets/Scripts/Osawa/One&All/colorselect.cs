using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colorselect : MonoBehaviour
{
    public Texture2D sourceTexture; // 元のテクスチャ
    public Color targetColor = Color.white; // 置き換えない対象の色
    public Color replacementColor = Color.red; // 置き換える色
    public float colorTolerance = 0.1f; // 色の許容範囲

    public testsome Testsome;
    public gamisome Gamisome;

    private Texture2D modifiedTexture;

    void Start()
    {
        if (sourceTexture == null)
        {
            Debug.LogError("Source texture is not assigned.");
            return;
        }

        //ReplaceColors();
        if (Testsome==null)
        {
        }
        else
        {
            Testsome.baseTexture = modifiedTexture;
        }
        if (Gamisome == null)
        {

        }
        else
        {
            Gamisome.baseTexture = modifiedTexture;
        }
    }

    public void ReplaceColors()
    {
        // 元のテクスチャを新しいテクスチャとしてコピー
        modifiedTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
        modifiedTexture.SetPixels(sourceTexture.GetPixels());

        Color[] pixels = modifiedTexture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            if (IsColorMatch(pixels[i], targetColor, colorTolerance))
            {
                pixels[i] = replacementColor;
            }
        }

        modifiedTexture.SetPixels(pixels);
        modifiedTexture.Apply();

        // 置き換えたテクスチャを適用 (この例では、RendererのMaterialに適用)
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.mainTexture = modifiedTexture;
        }
    }

    bool IsColorMatch(Color color1, Color color2, float tolerance)
    {
        // 色差の絶対値を比較 (RGBそれぞれの差の合計が許容範囲以内であるかを判定)
        return Mathf.Abs(color1.r - color2.r) <= tolerance &&
               Mathf.Abs(color1.g - color2.g) <= tolerance &&
               Mathf.Abs(color1.b - color2.b) <= tolerance;
    }
}