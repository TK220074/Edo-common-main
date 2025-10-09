using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// お出かけのショップ内における商品リストに並ぶボタンについて
/// </summary>
public class itemButton_inShop : MonoBehaviour
{
    [SerializeField] private Image img_iconBG;
    [SerializeField] private Image img_icon;
    [SerializeField] private RectTransform rect_img_main;
    [SerializeField] private GameObject obj_soldout;

    private int buttonId;
    private bool isSoldout;

    /// <summary>
    /// ボタンの見た目やボタンに対応するアイテム（のID）を設定する
    /// </summary>
    /// <param name="itemId">セットするアイテムの番号</param>
    /// <param name="soldout">そのアイテムを売り切れ表示にするか</param>
    public void buttonSetup(int itemId, bool soldout)
    {
        buttonId = itemId;
        manager_shopItemList m = manager_shopItemList.instance;
        switch (m.nowShopKind)
        {
            case shopKind.Pattern:
                img_icon.sprite = Manager_CommonGroup.instance.dataM.imgList_pattern.list[itemId];
                rect_img_main.localScale = manager_shopItemList.instance.vec_imgScale_komonPattern;
                break;

            default:
                img_icon.sprite = Manager_CommonGroup.instance.dataM.imgList_items.list[itemId];
                img_iconBG.color = Color.white;
                break;
        }
        isSoldout = soldout;
        obj_soldout.SetActive(isSoldout);
    }

    /// <summary>
    /// アイテムボタンが押された時の処理・詳細画面を書き換える
    /// </summary>
    public void buttonPressed()
    {
        Manager_CommonGroup.instance.audioM.SE_Play(AudioManager.WhichSE.Done);
        manager_shopItemList.instance.setDetail(buttonId, isSoldout);
    }
}
