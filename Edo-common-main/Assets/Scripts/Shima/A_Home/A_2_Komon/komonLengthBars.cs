using UnityEngine;
using UnityEngine.UI;

public class komonLengthBars : MonoBehaviour
{
    [SerializeField]private GameObject[] lengthTexts;
    [SerializeField]private Image[] barImg;
    [SerializeField]private ColorList lengthColorList;
    [SerializeField]private Sprite barSprite_none;

    /// <summary>
    /// 小紋の残り長さUIを更新する
    /// </summary>
    /// <param name="length">更新後の長さ</param>
    public void setKomonLengthBars(def_komon.komonLength length)
    {
        byte processed = 0; // バー画像をnullにした回数
        for(int i = 0; i < barImg.Length; i++)
        {
            barImg[i].color = lengthColorList.list[(int)length];
            
            // lengthの数だけバー画像をnullにする
            if(processed < (int)length)
            {
                barImg[i].sprite = null;
                processed++;
            }
            else
            {
                // 点線表示
                barImg[i].sprite = barSprite_none;
            }

            // 該当する長さの文字表示
            if((i == ((int)length - 1)) && length != def_komon.komonLength.none)
            {
                lengthTexts[i].SetActive(true);
            }
            else
            {
                lengthTexts[i].SetActive(false);
            }
        }
    }
}
