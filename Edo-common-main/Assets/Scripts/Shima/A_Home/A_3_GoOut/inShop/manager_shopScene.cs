using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class manager_shopScene : MonoBehaviour
{
    [SerializeField] private manager_shopItemList shopItemListM;
    [SerializeField] private shopKind[] shopLoopList;

    [SerializeField, Header("UI Component")] private TextMeshProUGUI[] text_shopName;
    [SerializeField] private Image shopIcon;
    [SerializeField] private Image[] buttonIconImgs; // ボタンに表示する店アイコン・右側[0]/左側[1]

    private img_BG bgInstance; // 背景画像スクリプトのキャッシュ

    [SerializeField, Header("Database")] private SpriteList imgList_shopIcon;
    [SerializeField] private StringList list_shopName;

    private Manager_CommonGroup commonM;

    // Start is called before the first frame update
    void Start()
    {
        commonM = Manager_CommonGroup.instance;
        bgInstance = img_BG.instance;

        shopItemListM.changeShop((shopKind)commonM.tempValue[0]); // 最初に入った店をセット

        byte now = (byte)Array.IndexOf(shopLoopList, shopItemListM.nowShopKind);
        set_buttonIcon(get_LRSideShopIndex(now));

        commonM.audioM.SE_Play(commonM.tempValue[0] + 7);
        commonM.audioM.BGM_Play(false, commonM.tempValue[0] + 5);
        StartCoroutine(bgInitialize(now));
    }
    IEnumerator bgInitialize(byte firstShop)
    {
        yield return null;
        set_shopName(firstShop);
    }

    /// <summary>
    /// 店種変更ボタンを押したときの処理
    /// </summary>
    /// <param name="isRight"></param>
    public void button_shopChange(bool isRight)
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);

        byte shopIndex = (byte)Array.IndexOf(shopLoopList, shopItemListM.nowShopKind); // 表示している店が、ループのどこに位置するか

#if true
        byte[] sideNum = get_LRSideShopIndex(shopIndex);
        // 押したのは右側のボタンか
        if (isRight)
        {
            shopIndex = sideNum[1];
        }
        else
        {
            shopIndex = sideNum[0];
        }

        sideNum = get_LRSideShopIndex(shopIndex); // 移動後の左右を取得

        shopItemListM.changeShop(shopLoopList[shopIndex]); // 店移動

        commonM.audioM.SE_Play(12);
        commonM.audioM.BGM_Play(false, shopIndex + 5);

        set_buttonIcon(sideNum);
        set_shopName(shopIndex);
#endif
    }

    /// <summary>
    /// 現在の店の位置に基づいて、左右の店の番号を取得する
    /// </summary>
    /// <param name="nowIndex">どの店視点か</param>
    /// <returns>左右の店の番号・左側[0]/右側[1]</returns>
    private byte[] get_LRSideShopIndex(byte nowIndex)
    {
        byte[] result = new byte[2];
       
        // L
        // 現在の店が、ループの頭にあるか
        if (nowIndex == 0)
        {
            result[0] = (byte)(shopLoopList.Length - 1);
        }
        else
        {
            result[0] = (byte)(nowIndex - 1);
        }

        // R
        // ループの最後尾にあるか
        if (nowIndex == (shopLoopList.Length - 1))
        {
            result[1] = 0;
        }
        else
        {
            result[1] = (byte)(nowIndex + 1);
        }
        return result;
    }

    /// <summary>
    /// 左右のボタンアイコンを設定
    /// </summary>
    /// <param name="LRShopNum">左右の店の番号</param>
    private void set_buttonIcon(byte[] LRShopNum)
    {
        buttonIconImgs[0].sprite = imgList_shopIcon.list[LRShopNum[0]];
        buttonIconImgs[1].sprite = imgList_shopIcon.list[LRShopNum[1]];
    }

    /// <summary>
    /// ショップ名表示を更新する
    /// </summary>
    /// <param name="nowShopNum">表示する店舗の番号</param>
    private void set_shopName(byte nowShopNum)
    {
        shopIcon.sprite = imgList_shopIcon.list[nowShopNum];
        bgInstance.changeBG((short)(nowShopNum + 3)); // 背景切替

        byte num = (byte)(nowShopNum * 2);
        text_shopName[0].SetTextAndExpandRuby(list_shopName.list[num]); // 店種
        text_shopName[1].SetTextAndExpandRuby(list_shopName.list[num + 1]); // 店名
    }
}
