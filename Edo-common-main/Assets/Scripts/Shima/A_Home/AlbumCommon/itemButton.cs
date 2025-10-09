using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 各種アルバムのリストに表示するボタン
/// </summary>
public class itemButton : MonoBehaviour
{
    private ushort itemId;
    private def_player.albumKind album;
    [SerializeField]private Image[] img;
    [SerializeField] private RectTransform rect_img_main;
    [SerializeField]private TMPro.TextMeshProUGUI numText;
    
    [SerializeField,Header("rankObj")]private rankStamp rankImg;
    [SerializeField] private GameObject obj_rank;


    /// <summary>
    /// リストに表示するボタンの見た目などを設定する
    /// </summary>
    /// <param name="id">参照するデータのID</param>
    public void initialize(ushort id)
    {
        itemId = id;
        album = album_list.instance.albumKind;
        Manager_CommonGroup m = Manager_CommonGroup.instance;
        def_player playerData = m.saveM.saveData.playerInfo;
        DatabaseManager dataM = m.dataM;

        // サムネ画像決定
        switch (album)
        {
            case def_player.albumKind.pattern:
                // 型紙アルバム
                img[1].sprite = dataM.imgList_pattern.list[id];
                numText.text = $"×{playerData.get_havingPattern_haveNum((byte)id).ToString("00")}"; // 所持数
                rect_img_main.localScale = manager_album_pattern.instance.vec_imgScale_komonPattern; // サムネイル画像の拡大
                break;

            case def_player.albumKind.komon:
                // 小紋アルバム
                def_komon komon = Manager_CommonGroup.instance.saveM.saveData.playerInfo.havingKomon[id]; // 参照する小紋データ

                // ランク表示
                obj_rank.SetActive(true);
                rankImg.setRangImg(komon.get_rank(def_komon.creationPart.summary));

                //ボタンのサムネ
                img[0].color =  komon.get_colorNum_toRgb(def_komon.creatonTiming.ground); // 地色
                img[1].sprite = dataM.imgList_pattern.list[(int)komon.get_patternId(def_komon.creatonTiming.ground)]; // ベース柄

                rect_img_main.localScale = manager_album_komon.instance.vec_imgScale_komonPattern; // サムネイル画像の拡大
                numText.text = ""; // 型紙アルバムのボタンと共通なので、所持数を示すものを消しておく
                break;

            case def_player.albumKind.color:
                // 色レシピ
                img[1].color = playerData.get_havingColor((byte)id).get_looksColor(true); // サムネの色は最終的な色
                numText.text = ""; // 型紙アルバムのボタンと共通なので、所持数を示すものを消しておく
                break;

            default:
                // 倉庫
                img[0].color = Color.white;
                img[1].sprite = dataM.imgList_items.list[id];
                numText.text = $"×{playerData.get_havingItem(id).ToString("00")}"; // 所持数
                break;
        }
    }

    public void press()
    {
        Manager_CommonGroup.instance.audioM.SE_Play(AudioManager.WhichSE.Done);
        switch(album)
        {
            case def_player.albumKind.pattern:
                // 型紙アルバム
                manager_album_pattern.instance.reloadDetail(itemId); // 詳細表示の更新
                break;

            case def_player.albumKind.komon:
                // 小紋アルバム
                manager_album_komon.instance.reloadDetail(itemId);
                break;

            case def_player.albumKind.color:
                // 色レシピ
                manager_album_colorRecipe.instance.reloadDetail(itemId);
                break;

            default:
                // 倉庫
                manager_album_storage.instance.reloadDetail(itemId);
                break;
        }
    }
}
