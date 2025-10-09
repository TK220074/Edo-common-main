using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Excel上で設定したゲーム内の定数(?)リスト・要素番号と内容についてはlist_constString.xlsxを参照
/// </summary>
[ExcelAsset]
public class list_constString : ScriptableObject
{
    public List<stringEntity> list;
}
