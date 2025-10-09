using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 色レシピデータ
/// </summary>
[System.Serializable]
public class def_colorRecipe
{
    [SerializeField] private string name; // レシピないし、その色の名前
    [SerializeField]private short[] usedDyeId = new short[3]; // 使用した“染料”のアイテムID・ベース～上塗り2回目まで・未使用なら-1
    [SerializeField]private float[] usedDyeAmount = new float[3]; //染料の使用量
    [SerializeField]private Color[] looksColor = new Color[2]; // 色糊の色と最終的な色(RGB)
    [SerializeField]private bool isImportant; // 破棄できないものか


    /// <summary>
    /// 色レシピデータの新規作成・~.havingColor.Add(new def_colorRecipe(~~))のように
    /// </summary>
    /// <param name="recipeName">レシピ名</param>
    /// <param name="itemDatabase">アイテムデータベース（染料を指定しているかの判定に使用）</param>
    /// <param name="dyeId">使用した染料のアイテムID・未投入なら-1</param>
    /// <param name="dyeAmount">染料の投入量(0.0f~)</param>
    /// <param name="color_dye">色糊の色(RGB)</param>
    /// <param name="color_last">最終的な色(RGB)</param>
    /// <param name="important">「だいじなもの」＝破棄不可能か</param>
    public def_colorRecipe(string recipeName, short[]dyeId, float[]dyeAmount, Color color_dye, Color color_last, bool important)
    {
        Manager_CommonGroup m = Manager_CommonGroup.instance;
        def_player info = m.saveM.saveData.playerInfo;

        // 保存数を超えて保存しようとしていないかチェック
        if(info.havingColor.Count < info.get_albumSize(def_player.albumKind.color))
        {
            set_name(recipeName);
            set_usedDyeId(dyeId, m.dataM.list_items);
            set_usedDyeAmount(dyeAmount);
            set_looksColor(false, color_dye);
            set_looksColor(true, color_last);
            set_isImportant(important);
        }
        else
        {
            devlog.logError($"小紋アルバムの最大保存数を超えて新たに色レシピを新規作成しようとしていませんか？・現保存数{info.get_albumSize(def_player.albumKind.color)}");
            info.havingColor.RemoveAt(-1);
        }
    }

    /// <summary>
    /// レシピ名を記録する
    /// </summary>
    /// <param name="str">記録するレシピ名</param>
    /// <returns>正常に記録出来たらtrue</returns>
    public bool set_name(string str)
    {
        int max = (int)Manager_CommonGroup.instance.dataM.get_constFloat(0);
        if ((str.Length <= max) && (str.Length > 0))
        {
            name = str;
            return true;
        }
        else
        {
            devlog.logError($"名前の最大文字数は {max} です。　入力文字数：{str.Length}");
            return false;
        }
    }
    public string get_name() { return name; }


    /// <summary>
    /// 色糊づくりで使用した染料を記録する
    /// </summary>
    /// <param name="dyeId">染料のアイテムID</param>
    /// <param name="database">アイテムデータベース（染料を指定しているかの判定に使用）</param>
    /// <returns>正常に記録出来たらtrue</returns>
    public bool set_usedDyeId(short[] dyeId, list_items database)
    {
        for(int i = 0; i < usedDyeId.Length; i++)
        {
            // 「未投入」を示す「-1」は無視
            if (dyeId[i] != -1)
            {
                if ((dyeId[i] >= database.list.Count) || (database.list[dyeId[i]].shopKind != shopKind.Dye))
                {
                    devlog.logError($"指定したIDの一部または全てが　染料ではない or データベースに存在しないようです・ID：{dyeId[0]}, {dyeId[1]}, {dyeId[2]}");
                    return false;
                }
            }
        }
        for(int i = 0; i < usedDyeId.Length; i++)
        {
            usedDyeId[i] = dyeId[i];
        }
        return true;
    }
    public short[] get_usedDyeId() {return usedDyeId;}
    /// <summary>
    /// 配合できる染料の数を取得する
    /// </summary>
    /// <returns>記録できるIDの数（＝配合できる染料の数）</returns>
    public byte get_usedDyeId_len() { return (byte)usedDyeId.Length; }

    /// <summary>
    /// 投入した染料の量を記録する
    /// </summary>
    /// <param name="value">投入量(g)のfloat[3]配列</param>
    /// <returns>記録に成功したらtrue</returns>
    public bool set_usedDyeAmount(float[] value)
    {
        // 記録できる投入回数分だけ検証する
        for (int i = 0; i < usedDyeAmount.Length; i++)
        {
            // -1（= 未投入）でない限り記録する
            if (usedDyeId[i] != -1)
            {
                // 投入量は正の数である必要がある
                if (value[i] < 0)
                {
                    devlog.logError($"投入量(value)は0.0f以上にしてください・ID：{value[0]}, {value[1]}, {value[2]}");
                    return false;
                }

                // 未投入でなく、投入量が正であれば記録
                usedDyeAmount = value;

            }
            else
            {
                // 未投入が出てきたら、それ以降は記録する必要がないので終了
                return true;
            }
        }
        return true;

    }
    public float[] get_usedDyeAmount() {return usedDyeAmount;}

    /// <summary>
    /// 色糊の色もしくは蒸し後の最終的な色を記録する(RGB)
    /// </summary>
    /// <param name="isLastColor">最終的な色ならtrue</param>
    /// <param name="setCol">記録する色データ(RGB)</param>
    public void set_looksColor(bool isLastColor, Color setCol) {looksColor[System.Convert.ToInt32(isLastColor)] = setCol;}
    /// <summary>
    /// 色糊の色もしくは蒸し後の最終的な色を取得する(RGB)
    /// </summary>
    /// <param name="isLastColor">最終的な色ならtrue</param>
    /// <returns>色糊の色もしくは蒸し後の最終的な色(RGB)</returns>
    public Color get_looksColor(bool isLastColor) {return looksColor[System.Convert.ToInt32(isLastColor)];}

    public void set_isImportant(bool important) {isImportant = important;}
    public bool get_isImportant() {return isImportant;}
}
