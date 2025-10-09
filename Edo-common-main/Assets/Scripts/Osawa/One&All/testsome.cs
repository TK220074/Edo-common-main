using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class testsome : MonoBehaviour
{
    Texture2D drawTexture;
    Color[] buffer;
    Texture2D combinedTexture;
    public Texture2D baseTexture;    // 下に配置する基本のテクスチャ
    public Texture2D overlayTexture; // 上に重ねるテクスチャ（透明ピクセルを含む）


    private Texture2D modifiedTexture;
    public int b_size = 50;//ブラシサイズ
    public int t_sizex = 1024;
    public int t_sizey = 512;
    int qq = 0;



    void Start()
    {
        Texture2D mainTexture = (Texture2D)GetComponent<Renderer>().material.mainTexture;
        Color[] pixels = mainTexture.GetPixels();




        buffer = new Color[pixels.Length];
        pixels.CopyTo(buffer, 0);
        /* for (int x = 0; x < mainTexture.width; x++)
         {
             for (int y = 0; y < mainTexture.height; y++)
             {
                 if (y < mainTexture.height / 2)
                 {
                     buffer.SetValue(Color.clear, x + t_sizex * y);
                 }
             }
         }*/

        drawTexture = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);
        drawTexture.filterMode = FilterMode.Point;
    } 

    public void Draw(Vector3 p)
    {
        for (int x = 0; x < t_sizex; x++)
        {
            for (int y = 0; y <t_sizey; y++)
            {
                if ((p - new Vector3(x, y,0)).magnitude <b_size )
                {
                    buffer.SetValue(Color.clear, x + t_sizex * y);
                }
            }
        }
    }
    public void Drawfast()
    {
        for (int x = 0; x < t_sizex; x++)
        {
            for (int y = 0; y <t_sizey; y++)
            {
                
                    buffer.SetValue(Color.clear, x + t_sizex * y);
                
            }
        }
    }




    Texture2D CombineTextures(Texture2D baseTex, Texture2D overlayTex)
    {
        // 合成するテクスチャのサイズを小さい方に合わせる
        int width = Mathf.Max(baseTex.width, overlayTex.width);
        int height = Mathf.Max(baseTex.height, overlayTex.height);

        Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // 各ピクセルを取得して、透明度に応じて合成
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color baseColor = baseTex.GetPixel(x, y);
                Color overlayColor = overlayTex.GetPixel(x, y);

                // オーバーレイのアルファ値が0より大きい場合のみ上に重ねる
                Color finalColor = overlayColor.a > 0 ? overlayColor : baseColor;

                result.SetPixel(x, y, finalColor);
            }
        }

        result.Apply();  // テクスチャの変更を適用
        return result;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                Draw(hit.textureCoord * t_sizex);
            }

            drawTexture.SetPixels(buffer);
            drawTexture.Apply();
            GetComponent<Renderer>().material.mainTexture = drawTexture;

        }

        if (Input.GetMouseButton(1))
        {
            overlayTexture = (Texture2D)GetComponent<Renderer>().material.mainTexture;
             combinedTexture = CombineTextures(baseTexture, overlayTexture);
            GetComponent<Renderer>().material.mainTexture = combinedTexture;
        }
        if (Input.GetMouseButton(2))
        {
            savetex("komon");
        }
    }
    public void savetex(string name)
    {
        var storagePath = Application.dataPath + "/Scripts/Osawa/" + name + ".png";

        //テクスチャの外側を定義
        Texture2D tex = new Texture2D(combinedTexture.width, combinedTexture.height);
        //ピクセル情報を入れる処理
        tex.SetPixels(combinedTexture.GetPixels());
        //エンコード処理
        var png = tex.EncodeToPNG();
        File.WriteAllBytes(storagePath, png);
    }

}