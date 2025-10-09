using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class coverPatternBG : MonoBehaviour
{
    [SerializeField] private Image img;

    void Start()
    {
        setCoverPatternBG();
    }

    /// <summary>
    /// 背景カバー用の小紋画像を入れ替える
    /// </summary>
    /// <param name="id">入れ替え後の小紋画像の番号</param>
    public void setCoverPatternBG(int id)
    {
        DatabaseManager dataM = Manager_CommonGroup.instance.dataM;
        if (id < dataM.imgList_pattern.list.Count)
        {
            img.sprite = dataM.imgList_pattern.list[id]; // BGの柄をセット
        }
    }
    /// <summary>
    /// 背景カバー用の小紋画像をランダムなものに入れ替える
    /// </summary>
    public void setCoverPatternBG()
    {
        DatabaseManager dataM = Manager_CommonGroup.instance.dataM;
        img.sprite = Manager_CommonGroup.instance.dataM.imgList_pattern.list[Random.Range(0, dataM.imgList_pattern.list.Count)]; // BGの柄をセット
    }
}
