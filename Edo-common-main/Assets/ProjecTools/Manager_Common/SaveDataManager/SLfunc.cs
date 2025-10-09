using UnityEngine;
using System;
using System.IO;

/// <summary>
/// SaveDataManagerで使用する各種関数たち
/// </summary>

public class SLfunc : MonoBehaviour
{
    public const uint PLAYTIMELIMIT = 3599999999;//プレイ時間・バトル時間は35億9999万9999秒=99万9999時間59分59秒までカウント
    public const uint COUNTLIMIT = 999999999;//起動回数は9億9999万9999回までカウント
    private const int FORDATE_YMOD = 2000;//セーブ年のモジュロ演算に使う数
    private const int FORDATE_BY = 3;//セーブ日の合計にかける数
    private const int FORDIGIT_MOD = 10000;//チェックディジット用・変数を何で割った余りにするか
    private const int FORDIGIT_ADD = 7;//チェックディジット用・各変数の計算結果に加算する数値    

    
    /// <summary>
    /// セーブデータ保存先のパスを取得する
    /// </summary>
    /// <param name="fileName">セーブデータ名</param>
    /// <param name="doEncrypt">データは暗号化されるか</param>
    /// <returns>データの保存先パス</returns>
    public static string GetPath(string fileName, bool doEncrypt = true)
    {
        //https://qiita.com/w_yang/items/8458cd790607d14b1b36 に保存される
        string resultPath = Application.persistentDataPath + "/" + fileName;
    #if UNITY_EDITOR
        if(doEncrypt)
        {
            resultPath += ".bin";
        }
        else
        {
            resultPath += ".json";
        }
    #else
        resultPath += ".bin";
    #endif
        return resultPath;
    }
    
    /// <summary>
    /// 各変数の値から不正検知用のCheckDigitを算出する
    /// </summary>
    /// <param name="data">セーブしたいSaveData変数</param>
    /// <returns>不正検知用のCheckDigitの値</returns>
    public static int CalcCheckDigit(SaveData data)
    {//各変数の値から不正検知用のCheckDigitを算出する
        //各変数を任意の値で割った余りに任意の値を足したものの合計
        //tempに足すものの編集をしてね
        int temp = (int)(CalcEachNumForDigit(data.get_startupNum())
                        + CalcEachNumForDigit(data.get_playTime())
                        + CalcEachNumForDigit(data.playerInfo.get_titleId())
                        + CalcEachNumForDigit((uint)data.playerInfo.get_money())
                        + CalcEachNumForDigit((uint)data.playerInfo.get_albumSize(def_player.albumKind.komon))
                        + CalcEachNumForDigit((uint)data.playerInfo.get_playerRank(def_komon.creationPart.summary)));

        int date = (data.get_saveDate(SaveData.date.year) + data.get_saveDate(SaveData.date.month) + data.get_saveDate(SaveData.date.day)) * FORDATE_BY;//セーブ年月日を足して任意の値をかけた数
        return (temp - date);
    }


//////////////////////////////////////////////////////////////
//セーブデータにある変数のチェックディジット計算用関数
//////////////////////////////////////////////////////////////

    /// <summary>
    /// 任意の値で割った余りに、任意の値を足したものを返す
    /// </summary>
    /// <param name="num">任意の値</param>
    /// <returns>任意の値で割った余りに、任意の値を足したもの</returns>
    private static int CalcEachNumForDigit(uint num)
    {
        return (int)((num % FORDIGIT_MOD) + FORDIGIT_ADD);
    }

    /// <summary>
    /// bool配列向け・各要素を任意の値で割った余りに任意の値を足したものを返す
    /// </summary>
    /// <param name="array">計算したいbool配列</param>
    /// <returns>計算結果</returns>
    private static int CalcDigitForArray(bool[] array)
    {
        int temp = 0;
        foreach (bool b in array)
        {
            temp += CalcEachNumForDigit((uint)Convert.ToInt32(b));
        }
        return temp;
    }

    /// <summary>
    /// int配列向け・各要素を任意の値で割った余りに任意の値を足したものを返す
    /// </summary>
    /// <param name="array">計算したいint配列</param>
    /// <returns>計算結果</returns>
    private static int CalcDigitForArray(int[] array)
    {
        int temp = 0;
        foreach (int i in array)
        {
            temp += CalcEachNumForDigit((uint)i);
        }
        return temp;
    }

    /// <summary>
    /// int配列向け・各要素を任意の値で割った余りに任意の値を足したものを返す
    /// </summary>
    /// <param name="array">計算したいbyte配列</param>
    /// <returns>計算結果</returns>
    private static int CalcDigitForArray(byte[] array)
    {
        int temp = 0;
        foreach (byte b in array)
        {
            temp += CalcEachNumForDigit((uint)b);
        }
        return temp;
    }


//////////////////////////////////////////////////////////////
//セーブデータにある配列のチェックディジット計算用関数　ここまで
//////////////////////////////////////////////////////////////
    
    
    /// <summary>
    /// 初期状態のユーザデータを作成する
    /// </summary>
    /// <returns>初期状態のユーザデータ</returns>
    public static SaveData InitializeData()
    {
        SaveData NewSaveData = new SaveData();
        NewSaveData.playerInfo = new def_player();
        devlog.logWarning("初期状態のユーザデータを作成しました。");
        return NewSaveData;
    }
    
    
    /// <summary>
    /// 起動回数を加算する
    /// </summary>
    /// <param name="data">加算対象のセーブデータ</param>
    public static void CountStartup(SaveData data)
    {
        if (data.get_startupNum() < COUNTLIMIT)
        {//上限に達していなければ
            data.set_startupNum();//起動回数を+1
        }
    }

    /// <summary>
    /// セーブした年月日を格納する
    /// </summary>
    /// <param name="data"></param>
    /// <returns>一応、計算結果に任意の数倍したものを返す</returns>
    public static byte CalcSavedDate(SaveData data)
    {
        DateTime now = DateTime.Now;//日付を取得
        byte[] date = new byte[3];

        date[(int)SaveData.date.year] = (byte)(now.Year % FORDATE_YMOD);//年を任意の数でモジュロ演算
        //月と日はそのまま格納
        date[(int)SaveData.date.month] = (byte)now.Month;
        date[(int)SaveData.date.day] = (byte)now.Day;
        data.set_saveDate(date);
        return (byte)((date[(int)SaveData.date.year] + date[(int)SaveData.date.month] + date[(int)SaveData.date.day]) * FORDATE_BY);//一応、和の任意の数倍したものを返す
    }

    
    
    /// <summary>
    /// ユーザデータに不正がないかを確認する
    /// </summary>
    /// <param name="data">確認対象のSaveData変数</param>
    /// <returns>不正なし:true　不正あり:false</returns>
    public static bool CheckCheckDigit(SaveData data)
    {
        int result = CalcCheckDigit(data);//計算結果を格納
        if (data.get_check() == result)
        {//チェックディジットが一致=不正ナシ
            return true;
        }
        else
        {//チェックディジットが不一致=不正アリ
            devlog.logWarning($"ユーザデータの不正を検知しました。\nセーブ内容：{data.get_check()}　計算結果：{result}");
            return false;
        }
    }

    
    /// <summary>
    /// セーブデータを暗号化する(json->bin)
    /// https://www.create-forever.games/unity-aes-encrypt/
    /// </summary>
    /// <param name="json">暗号化するjson</param>
    /// <param name="pass">アプリパスワード</param>
    /// <returns></returns>
    public static byte[] enc(string json, string pass)
    {
        // Json -> Binary
        byte[] bin = GZipCompressor.Compress(json);
        // Encrypt
        return AesCbc.Encrypt(bin, pass); 
    }
    
    
    /// <summary>
    /// セーブデータを復号化する(bin->json)
    /// https://www.create-forever.games/unity-aes-encrypt/
    /// </summary>
    /// <param name="path">データの保存先</param>
    /// <param name="pass">アプリパスワード</param>
    /// <returns>復号化したユーザデータのjson(string)</returns>
    public static string dec(string path, string pass)
    {
        // read encrypt file
        byte[] encbin = File.ReadAllBytes(path);
        // 1. Decrypt
        byte[] decbin = AesCbc.Decrypt(encbin, pass);
        // 2. Binary -> Json
        return GZipCompressor.Decompress(decbin);
    }
}
