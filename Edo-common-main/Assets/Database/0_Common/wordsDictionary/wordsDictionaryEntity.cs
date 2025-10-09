using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class wordsDictionaryEntity
{
    /// <summary>
    /// ふりがなを振る単語
    /// </summary>
    public string words;
    /// <summary>
    /// 読み・振り方に指定がある場合は、この文字列にルビのタグを含ませる
    /// </summary>
    public string ruby;
}
