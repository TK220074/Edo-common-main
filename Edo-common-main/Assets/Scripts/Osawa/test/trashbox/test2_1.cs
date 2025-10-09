using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test2_1 : MonoBehaviour
{
    Texture2D drawTexture;
    Color32[] buffer;

    // Start is called before the first frame update
    void Start()
    {
        Texture2D mainTexture = (Texture2D)GetComponent<Renderer>().material.mainTexture;
        Color32[] pixels = mainTexture.GetPixels32();

        buffer = new Color32[pixels.Length];
        pixels.CopyTo(buffer, 0);

        drawTexture = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);
        drawTexture.filterMode = FilterMode.Point;
    }

    public void Draw(Vector2 p, Color32[] brushbuffer, Vector2 brushSize)
    {
        p.x = p.x * drawTexture.width;
        p.y = p.y * drawTexture.height;

        for (int x = 0; x < drawTexture.width; x++)
        {
            for (int y = 0; y < drawTexture.height; y++)
            {
                if ((p.x - (brushSize.x / 2)) < x && x < (p.x + (brushSize.x / 2)) && (p.y - (brushSize.y / 2) < y) && y < (p.y + (brushSize.y / 2)))
                {
                    if (brushbuffer[(int)(x - (p.x - (brushSize.x / 2)) + (int)brushSize.x * (int)(y - (p.y - (brushSize.y / 2))))].a != 0)
                    {
                        buffer.SetValue(brushbuffer[(int)(x - (p.x - (brushSize.x / 2)) + (int)brushSize.x * (int)(y - (p.y - (brushSize.y / 2))))], x + drawTexture.width * y);
                    }
                }
            }
        }

        drawTexture.SetPixels32(buffer);
        drawTexture.Apply();
        GetComponent<Renderer>().material.mainTexture = drawTexture;
    }
}
