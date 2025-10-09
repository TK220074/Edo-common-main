using UnityEngine;

//参考
//https://kurokumasoft.com/2022/01/03/unity-savesystem-using-json/
//https://kazupon.org/unity-jsonutility/#JSON-2

///<summary>
///ユーザーデータの型となるクラス
///</summary>
[System.Serializable]
public class SaveData
{
    //初期状態を代入しておく

    //弄らない項目
    [SerializeField]private uint StartupNum = 0;//起動回数
    [SerializeField]private uint PlayTime = 0;//second・総起動時間 0秒

    //不正検知
    [SerializeField]private byte[] saveDate = new byte[3];//セーブ時の「年」を任意の数で割った余り
    [SerializeField]private int Check = 0;//各変数の数値に不正がないか確認する用・CalcCheckDigitで算出・(戦歴関係の値を任意の値で割った余りに任意の値を足したものの合計 - SavedDateの値)

    //オプション
    public byte[] Volume = new byte[]{10, 10};//BGM Lv.10
    public bool Screen = true;//スクリーンのモード・true=Full, false=Window
    public byte FPS = 1;//FPS指定・ゲーム内でFPS弄らない場合は不要・0,1,2,3 = 30,60,90,120
    public byte Resolution = 1;//解像度指定・0,1,2 = 1280*720,1920*1080,3840*2160

    /// <summary>
    /// プレイヤ情報（SAMEカスタム）
    /// </summary>
    public def_player playerInfo; // ゲーム開始時に初期化すること
    /// <summary>
    /// 最終起動時のバージョン
    /// </summary>
    public string str_appVer;

    public enum date
    {
        year = 0,
        month = 1,
        day = 2
    }

    public void set_startupNum() {StartupNum++;}
    public uint get_startupNum() {return StartupNum;}
    public void set_playTime() {PlayTime++;}
    public uint get_playTime() {return PlayTime;}
    public void set_check(int num) {Check = num;}
    public int get_check() {return Check;}
    public void set_saveDate(byte[] data)
    {
        byte len = (byte)saveDate.Length;
        if(data.Length == len)
        {
            saveDate = data;
        }
        else
        {
            devlog.logError($"引数の要素数は {len} である必要があります・今回の要素数：{data.Length}");
        }
    }
    public byte get_saveDate(date kind) {return saveDate[(int)kind];}
}