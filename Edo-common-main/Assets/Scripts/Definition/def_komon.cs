using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 制作した小紋データ
/// </summary>
[Serializable]
public class def_komon
{
    [SerializeField]private string[] name = new string[3]; // 小紋と制作者の名前
    [SerializeField]private Color[] colorNum = new Color[3]; // ベース～上塗り(2)のCMYK値
    [SerializeField]private byte[] patternId = new byte[3]; // 使用した型紙（柄）のID
    [SerializeField]private Vector2[] coatPos = new Vector2[2]; //上塗りの位置
    [SerializeField]private string[] date_created = new string[2]; // 最初と最後の制作日
    [SerializeField]private komonLength length; // 生地の長さ
    [SerializeField]private komonRank[] rank = new komonRank[4]; // 総合ランクと各工程ランク
    [SerializeField]private float komonPoint; // 出来栄えポイント
    [SerializeField]private byte coatLimit = 2; // 残り上塗り回数
    [SerializeField]private bool isImportant; // 破棄や売却、交換できるか



    public enum komonLength
    {
        none = 0,
        s = 1, // 短
        m = 2, // 中
        l = 3 // 長

    }

    public enum komonRank
    {
        d = 0,
        c = 1,
        b = 2,
        a = 3,
        s = 4
    }

    /// <summary>
    /// 制作の立ち位置・ベース～上塗り2回目
    /// </summary>
    public enum creatonTiming
    {
        ground = 0, // 初回制作・小紋の名前
        first = 1, // 上塗り(1)
        last = 2 // 上塗り(2)
    }

    public enum creationPart
    {
        summary = 0, // 総合
        colorCreation = 1, // 色糊
        dyeing = 2, // 地色染め
        washing = 3 // 水洗い
    }

    public enum color_cymk
    {
        c = 0,
        m = 1,
        y = 2,
        k = 3
    }

    /// <summary>
    /// 小紋データの新規作成・~.havingKomon.Add(new def_komon(~~))のように
    /// </summary>
    /// <param name="name_komon">小紋の名前</param>
    /// <param name="name_creator">制作者の名前</param>
    /// <param name="cmyk">地色のCMYK値</param>
    /// <param name="patternId">ベース柄の型紙ID</param>
    /// <param name="createdDate">制作日</param>
    /// <param name="length">小紋の初期長さ</param>
    /// <param name="point">出来栄えポイント</param>
    /// <param name="important">「だいじなもの」＝売却&破棄不可能か</param>
    public def_komon(string name_komon, string name_creator, Color cmyk, byte patternId, DateTime createdDate, komonLength length, komonRank[] ranks, float point, bool important)
    {
        def_player info = Manager_CommonGroup.instance.saveM.saveData.playerInfo;
        // 保存数を超えて保存しようとしていないかチェック
        if(info.havingKomon.Count < info.get_albumSize(def_player.albumKind.komon))
        {
            set_name(creatonTiming.ground, name_komon);
            set_name(creatonTiming.first, name_creator);
            set_name(creatonTiming.last, name_creator);
            set_colorNum(creatonTiming.ground, cmyk);
            set_patternId(creatonTiming.ground, patternId);
            set_date_created(false, createdDate);
            set_date_created(true, createdDate);
            set_length(length);
            for(int i = 0; i < 4; i++)
            {
                set_rank((creationPart)i, ranks[i]);
            }
            set_komonPoint(point);
            set_isImportant(important);
        }
        else
        {
            devlog.logError($"小紋アルバムの最大保存数を超えて新たに小紋を新規作成しようとしていませんか？・現保存数{info.get_albumSize(def_player.albumKind.komon)}");
            info.havingKomon.RemoveAt(-1);
        }
    }
#if DEBUG
    /// <summary>
    /// (Dev)小紋データの新規作成・~.havingKomon.Add(new def_komon(~~))のように
    /// </summary>
    /// <param name="cmyk">地色のCMYK値</param>
    /// <param name="patternId">ベース柄の型紙ID</param>
    /// <param name="length">小紋の初期長さ</param>
    /// <param name="point">出来栄えポイント</param>
    public def_komon(Color cmyk, byte patternId, komonLength length, float point)
    {
        def_player info = Manager_CommonGroup.instance.saveM.saveData.playerInfo;
        if(info.havingKomon.Count < info.get_albumSize(def_player.albumKind.komon))
        {
            string devname = "にむら　こもお";
            set_name(creatonTiming.ground, "開発用の小紋");
            set_name(creatonTiming.first, devname);
            set_name(creatonTiming.last, devname);
            set_colorNum(creatonTiming.ground, cmyk);
            set_patternId(creatonTiming.ground, patternId);
            set_date_created(false, DateTime.Now);
            set_date_created(true, DateTime.Now);
            set_length(length);
            for(int i = 0; i < 4; i++)
            {
                set_rank((creationPart)i, (komonRank)UnityEngine.Random.Range(0, 5));
            }
            set_komonPoint(point);
            set_isImportant(false);
        }
        else
        {
            devlog.logError($"小紋アルバムの最大保存数を超えて新たに小紋を新規作成しようとしていませんか？・現保存数{info.get_albumSize(def_player.albumKind.komon)}");
            info.havingKomon.RemoveAt(-1);
        }
    }
#endif



    /// <summary>
    /// 小紋もしくは制作者の名前を記録する
    /// </summary>
    /// <param name="nameKind">ground=小紋・first=初回制作者・last=最新制作者</param>
    /// <param name="setName">記録する名前</param>
    /// <returns>正常に記録出来たらtrue</returns>
    public bool set_name(creatonTiming nameKind, string setName)
    {
        byte constNum = (byte)Manager_CommonGroup.instance.dataM.get_constFloat(0); // 最大文字数を取得
        if ((setName.Length <= constNum) && (setName.Length > 0))
        {
            name[(int)nameKind] = setName;
            return true;
        }
        else
        {
            devlog.logError($"名前の最大文字数は {constNum} です。　入力文字数：{setName.Length}");
            return false;
        }
    }
    /// <summary>
    /// 小紋もしくは制作者の名前を取得する
    /// </summary>
    /// <param name="nameKind">ground=小紋・first=初回制作者・last=最新制作者</param>
    public string get_name(creatonTiming nameKind) {return name[(int)nameKind];}

    /// <summary>
    /// ベース～上塗り2回目それぞれの地色を格納する
    /// </summary>
    /// <param name="colorKind">ベース～上塗り2回目のどれか</param>
    /// <param name="setCol">格納するCMYK値配列</param>
    public void set_colorNum(creatonTiming colorKind, Color setCol) {colorNum[(int)colorKind] = setCol;}
    /// <summary>
    /// ベース～上塗り2回目それぞれの地色を CMYKで 取得する
    /// </summary>
    /// <param name="colorKind">ベース～上塗り2回目のどれか</param>
    /// <returns>指定した制作における地色の CMYK値 配列</returns>
    public Color get_colorNum(creatonTiming colorKind) {return colorNum[(int)colorKind];}
    /// <summary>
    /// ベース～上塗り2回目それぞれの地色を RGBで 取得する
    /// </summary>
    /// <param name="colorKind">ベース～上塗り2回目のどれか</param>
    /// <returns>RGBのColor(0~1f)</returns>
    public Color get_colorNum_toRgb(creatonTiming colorKind)
    {
        Color cmyk = get_colorNum(colorKind);
        return Manager_CommonGroup.instance.get_cmykToRgb(cmyk);
    }

    /// <summary>
    /// 指定した制作において使用した型紙を記録する
    /// </summary>
    /// <param name="when">ベース～上塗り2回目のどれか</param>
    /// <param name="id">使用した型紙の番号</param>
    public void set_patternId(creatonTiming when, byte id) {patternId[(int)when] = id;}
    /// <summary>
    /// 指定した制作において使用した型紙のIDを取得する
    /// </summary>
    /// <param name="when">ベース～上塗り2回目のどれか</param>
    /// <returns>指定した制作において使用した型紙のID</returns>
    public byte get_patternId(creatonTiming when) {return patternId[(int)when];}

    public void set_coatPos(creatonTiming when, Vector2 pos) {coatPos[(int)when - 1] = pos;}
    public Vector2 get_coatPos(creatonTiming when) {return coatPos[(int)when - 1];}

    public void set_date_created(bool isLast, DateTime date) {date_created[Convert.ToInt32(isLast)] = date.ToString();}
    public DateTime get_date_created(bool isLast) {return DateTime.Parse(date_created[Convert.ToInt32(isLast)]);}

    /// <summary>
    /// 小紋の長さを代入する
    /// </summary>
    /// <param name="setLength">使用後の小紋の長さ</param>
    /// <param name="zeroLengthDelete">小紋の長さがなくなった(=none)時に、その小紋を自動的に破棄するか</param>
    public void set_length(komonLength setLength, bool zeroLengthDelete = true)
    {
        length = setLength;
        
        // 小紋を使い切った、かつ使い切ったときの自動破棄がtrueの場合、その小紋を破棄する
        if((setLength == komonLength.none) && zeroLengthDelete)
        {
            def_player p = Manager_CommonGroup.instance.saveM.saveData.playerInfo;
            int index = p.havingKomon.IndexOf(this);
            p.set_havingKomon_delete((uint)index);
        }
    }
    public komonLength get_length() {return length;}

    public void set_rank(creationPart part, komonRank setRank) {rank[(int)part] = setRank;}
    public komonRank get_rank(creationPart part) {return rank[(int)part];}

    public void set_komonPoint(float point) {komonPoint = point;}
    public float get_komonPoint() {return komonPoint;}

    /// <summary>
    /// 上塗り可能回数を「消費(-1)」する・上塗り実行時にどうぞ
    /// </summary>
    public void set_coatLimit()
    {
        if(coatLimit > 0)
        {
            coatLimit--;
        }
        else
        {
            devlog.logWarning("上塗り回数は　もう0よ！");
            coatLimit = 0;
        }
    }
    public byte get_coatLimit() {return coatLimit;}
    public void set_isImportant(bool important) {isImportant = important;}
    public bool get_isImportant() {return isImportant;}

    /// <summary>
    /// 「長さ」の表現を取得する
    /// </summary>
    /// <param name="length">取得したい長さ種類</param>
    /// <returns>長さの表現・def_komon.komonLength.sなら 短</returns>
    public string getLengthString(def_komon.komonLength length)
    {
        string result = "";
        switch (length)
        {
            case def_komon.komonLength.s:
                result = "短";
                break;

            case def_komon.komonLength.m:
                result = "中";
                break;

            case def_komon.komonLength.l:
                result = "長";
                break;
        }
        return result;
    }
}
