using System.Collections.Generic;
using UnityEngine;
using static talkTextEntity;

/// <summary>
/// キャラクタ名のリスト
/// </summary>
[ExcelAsset]
public class list_scenario_character : ScriptableObject
{
    public List<characterEntity> list;
}

[System.Serializable]
public class characterEntity
{
    public string name;
    public voiceOld charaOld; // 発話者の性別と年齢層
    public enum voiceOld
    {
        male_child = 0,
        male_adult,
        male_old,
        female_child,
        female_adult,
        female_old
    }
}
