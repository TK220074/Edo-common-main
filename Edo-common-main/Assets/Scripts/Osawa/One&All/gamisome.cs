using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gamisome : MonoBehaviour
{
    Texture2D drawTexture;
    Color[] buffer;
    Texture2D combinedTexture;
    public Texture2D baseTexture;    // 下に配置する基本のテクスチャ
    public Texture2D overlayTexture; // 上に重ねるテクスチャ（透明ピクセルを含む）

    public Texture2D sourceTexture; // 元のテクスチャ
    public Color targetColor = Color.white; // 置き換えない対象の色
    public Color replacementColor = Color.red; // 置き換える色
    public Color color_change=Color.white; // 塗る色
    public float colorTolerance = 0.1f; // 色の許容範囲
    public float colorTolerance_c = 0f; // 色の許容範囲

    
    private Texture2D modifiedTexture;

    private Renderer rend;
    public int b_size = 50;//ブラシサイズ
    public int t_sizex = 1024;
    public int t_sizey = 512;
    private int profound = 0;
    private int checker_b;
    private int checker_1;
    int qq = 0;
    private bool washing = false;
    public bool finish = false;

    private AudioManager audioM;


    HashSet<int> changedIndices = new HashSet<int>();

    void Start()
    {
        audioM = Manager_CommonGroup.instance.audioM;
    }

    public void Start_N()
    {
        profound = 0;
        checker_b = 0;
        checker_1 = 0;
        if (sourceTexture == null)
        {
            Debug.LogError("Source texture is not assigned.");
            return;
        }

        ReplaceColors();

        baseTexture = modifiedTexture;

        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // テクスチャをコピーして編集可能にする
            drawTexture = Instantiate(rend.material.mainTexture) as Texture2D;
            rend.material.mainTexture = drawTexture;
            drawTexture.Apply();
        }
        // mainTexture = (Texture2D)GetComponent<Renderer>().material.mainTexture;
        //mainTexture = baseTexture;
        //Color[] pixels = mainTexture.GetPixels();



        /*
        //buffer = new Color[pixels.Length];
        //pixels.CopyTo(buffer, 0);
        for (int x = 0; x < mainTexture.width; x++)
        {
            for (int y = 0; y < mainTexture.height; y++)
            {
                //checker_b++;
                if (y < mainTexture.height / 2)
                {
                    buffer.SetValue(Color.clear, x + t_sizex * y);
                }
            }
        }
        */
        //drawTexture = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);
        drawTexture.filterMode = FilterMode.Point;
    }

    public void Draw(Vector3 p)
    {
        for (int x = 0; x < t_sizex; x++)
        {
            for (int y = 0; y < t_sizey; y++)
            {
                if ((p - new Vector3(x, y, 0)).magnitude < b_size)
                {
                    buffer.SetValue(color_change, x + t_sizex* y);
                }
            }
        }
    }
    void PaintTexture(RaycastHit hit)
    {
        if (drawTexture == null) return;

        Vector2 pixelUV = hit.textureCoord;
        pixelUV.x *= drawTexture.width;
        pixelUV.y *= drawTexture.height;

        int x = Mathf.FloorToInt(pixelUV.x);
        int y = Mathf.FloorToInt(pixelUV.y);

        int xStart = Mathf.Clamp(x - b_size, 0, drawTexture.width - 1);
        int yStart = Mathf.Clamp(y - b_size, 0, drawTexture.height - 1);
        int xEnd = Mathf.Clamp(x + b_size, 0, drawTexture.width - 1);
        int yEnd = Mathf.Clamp(y + b_size, 0, drawTexture.height - 1);

        int width = Mathf.Max(1, xEnd - xStart);
        int height = Mathf.Max(1, yEnd - yStart);

        Color[] pixels = drawTexture.GetPixels(xStart, yStart, xEnd - xStart, yEnd - yStart);

        for (int i = 0; i < (xEnd - xStart); i++)
        {
            for (int j = 0; j < (yEnd - yStart); j++)
            {
                int dx = (xStart + i) - x;
                int dy = (yStart + j) - y;
                if ((dx * dx + dy * dy) <= (b_size * b_size)) // 正しく円形に塗る条件
                {
                    pixels[j * width + i] = color_change;
                }
            }
        }
        drawTexture.SetPixels(xStart, yStart, xEnd - xStart, yEnd - yStart, pixels);
        drawTexture.Apply();
    }
        
    
    public void Drawfast()
    {
        for (int x = 0; x < t_sizex; x++)
        {
            for (int y = 0; y < t_sizey; y++)
            {

                buffer.SetValue(color_change, x + t_sizex * y);

            }
        }
    }




   /* Texture2D CombineTextures(Texture2D baseTex, Texture2D overlayTex)
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
   */
    void Update()
    {
        if (finish == false)
        {
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    //Debug.Log(hit);
                    //Debug.Log(hit.textureCoord);
                    //Draw(hit.textureCoord * t_sizex);

                    if (Input.GetMouseButtonDown(0))
                    {
                        audioM.SE_Play(28); // ヘラが当たる音
                        StartCoroutine(audioM.BGM_Play(audioM.list_SE.list[27].clip, false, 0, 2)); // 塗り延ばす音
                    }

                    PaintTexture(hit);
                }
                /*
                drawTexture.SetPixels(buffer);
                drawTexture.Apply();
                GetComponent<Renderer>().material.mainTexture = drawTexture;
                */
            }
            else if (Input.GetMouseButtonUp(0))
            {
                audioM.BGM_Stop(false, 2); // 塗り延ばす音を止める
            }
        }
    }

    void ReplaceColors()
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
        sourceTexture = modifiedTexture;
        drawTexture=modifiedTexture;

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

    public void dekimaecheck()
    {
        Texture2D mainTexture = (Texture2D)GetComponent<Renderer>().material.mainTexture;
        Color[] pixels = mainTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (!IsColorMatch(pixels[i], color_change, colorTolerance_c))
            {
                checker_1++;
            }
        }
        float resurt = checker_1 / checker_b;
        resurt = resurt * 100;
        Debug.Log(resurt);

    }
}
