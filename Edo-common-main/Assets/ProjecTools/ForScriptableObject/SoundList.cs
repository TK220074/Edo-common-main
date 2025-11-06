using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class SoundData
{
    [Tooltip("この音の識別名（例：BGM_Title, SE_Click など）")]
    public string name;

    [Tooltip("実際に再生するオーディオクリップ")]
    public AudioClip clip;
}

[CreateAssetMenu(menuName = "Audio/SoundList", fileName = "SoundList")]
public class SoundList : ScriptableObject
{
    [Tooltip("登録されている音データの一覧")]
    public List<SoundData> list;


    /// <summary>
    /// 指定した名前の SoundData を返す
    /// </summary>
    /// <param name="name">検索したい音の名前</param>
    /// <returns>対応する SoundData。見つからなければ null。</returns>
    public SoundData GetItemByName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("[SoundList] 空または無効な名前が指定されました。");
            return null;
        }

        foreach (var s in list)
        {
            if (s != null && s.name == name)
                return s;
        }

        Debug.LogWarning($"[SoundList] '{name}' は登録されていません。");
        return null;
    }

    /// <summary>
    /// 指定した名前の AudioClip を直接取得する便利メソッド
    /// </summary>
    public AudioClip GetClip(string name)
    {
        var data = GetItemByName(name);
        return data != null ? data.clip : null;
    }
}
