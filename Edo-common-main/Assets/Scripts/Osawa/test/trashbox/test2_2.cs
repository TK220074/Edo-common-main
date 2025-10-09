using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test2_2 : MonoBehaviour
{
    test2_1 object_Logic = null;
    public Texture2D brushTexture;
    public Color brushColor = Color.white;
    public float magnification = 1;
    public Color32[] brushbuffer;

    // Start is called before the first frame update
    void Start()
    {
        if (magnification != 1)
        {
            Vector2 newSize = new Vector2(brushTexture.width * magnification, brushTexture.height * magnification);
            brushTexture = Resize(brushTexture, newSize);
        }

        brushbuffer = brushTexture.GetPixels32();

        for (int i = 0; i < brushbuffer.Length; i++)
        {
            byte gray = (byte)(brushbuffer[i].r * 0.2126 + brushbuffer[i].g * 0.7152 + brushbuffer[i].b * 0.0722);

            brushbuffer[i] = new Color32((byte)(gray * brushColor.r),
                                            (byte)(gray * brushColor.g),
                                            (byte)(gray * brushColor.b),
                                            brushbuffer[i].a);
        }
    }

    Texture2D Resize(Texture2D texture2D, Vector2 newSize)
    {
        RenderTexture rt = new RenderTexture((int)newSize.x, (int)newSize.y, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D((int)newSize.x, (int)newSize.y);
        result.ReadPixels(new Rect(0, 0, (int)newSize.x, (int)newSize.y), 0, 0);
        result.Apply();
        return result;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                object_Logic = hit.collider.GetComponent<test2_1>();

                if (object_Logic != null)
                {
                    object_Logic.Draw(hit.textureCoord, brushbuffer, new Vector2(brushTexture.width, brushTexture.height));
                }
            }
        }
    }
}
