using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アルバムの共通リスト部分のオブジェクトたち
/// </summary>
public class album_list : Singleton<album_list>
{
    /// <summary>
    /// 何のアルバムか
    /// </summary>
    public def_player.albumKind albumKind;

    /// <summary>
    /// リストに表示するボタンObj
    /// </summary>
    public GameObject itemButtonObj;
    /// <summary>
    /// ボタン数が最大数に満たない場合の埋め合わせ画像Obj
    /// </summary>
    public GameObject itemNoneObj;

    /// <summary>
    /// ボタン群の親Rect
    /// </summary>
    public RectTransform listRect;

    /// <summary>
    /// アルバム保存数を表示するText・保存数[0]/最大数[1]
    /// </summary>
    public TMPro.TextMeshProUGUI[] albumSizeNumText;

    public SpriteList imgList_rank;
    public ColorList rankColorList;
}
