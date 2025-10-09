using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 共通背景画像
/// </summary>
public class img_BG : Singleton<img_BG>
{
    [SerializeField] private Image bg; // 背景画像コンポーネント

    private List<Sprite> imgList; // BGリストのキャッシュ
    
    /// <summary>
    /// 背景画像を入れ替える
    /// </summary>
    /// <param name="imgId">入れ替え先の画像の番号</param>
    /// <returns>正常に切り替えられたらtrue</returns>
    public bool changeBG(short imgId)
    {
        // 画像リストをキャッシュ
        if(imgList == null)
        {
            imgList = Manager_CommonGroup.instance.dataM.imgList_bg.list;
        }
        
        if(imgId < 0)
        {
            // BG無しを指定しているか
            bg.CrossFadeAlpha(0f, 0f, true);
            return true;
        }
        else
        {
            bg.CrossFadeAlpha(1f, 0f, true);
        }
        
        // 範囲外を指定していないか
        if(imgId < imgList.Count)
        {
            bg.sprite = imgList[imgId];
            return true;
        }
        else
        {
            devlog.logError($"BGリストの範囲外を参照しています！：{imgId}");
            return false;
        }
    }
}
