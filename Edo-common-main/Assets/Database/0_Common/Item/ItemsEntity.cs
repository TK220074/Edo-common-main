using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテムEntity
/// </summary>

[System.Serializable]
public class ItemsEntity
{
    //public ushort id;
    public string name;
    public shopKind shopKind;
    public string explain;
    public uint price;
    public ushort durability; // 耐久値（型紙のみ？）
    public string clearStageId; // 購入可能になるクリアステージ条件・カンマで入力・無ければ-1
    public byte needLv; // 職人ランクのボーダー
    public bool buyOnce; // 購入は1回限りか
    public bool important;
}

/// <summary>
/// アイテム（販売店）の種類
/// </summary>
public enum shopKind
{
    Cloth = 0, // 生地
    Pattern = 1, // 型紙
    Dye = 2, // 染料
    Expansion = 3, // 職人道具
    Furniture = 4,
    Important = 5, // だいじなもの
    Komon = 6,
    BattleItem = 7,
    Other = 8
}
