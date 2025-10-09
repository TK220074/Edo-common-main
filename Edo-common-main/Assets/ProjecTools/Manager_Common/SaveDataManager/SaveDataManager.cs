using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

//参考
//https://kurokumasoft.com/2022/01/03/unity-savesystem-using-json/
//https://qiita.com/InfiniteGame/items/01da9d83853fecb95132
//https://kan-kikuchi.hatenablog.com/entry/JsonUtility

///<summary>
/// ユーザーデータのセーブとロードを行う
///</summary>
public class SaveDataManager : MonoBehaviour
{
    [SerializeField, Header("Data Info")] private string _fileName = "Data";//セーブデータの名前・まあ変更することはないだろう
    public string fileName => _fileName;
    [SerializeField]private StringList ps; // 暗号化時のアプリパスワード(https://www.create-forever.games/unity-aes-encrypt/)

    private string _filePath;//セーブデータ保存先のパス
    public SaveData saveData { get; set; }//セーブデータ内の各データを格納・ここから各データを取得する

    public bool Loaded { get; private set; }//セーブデータをロードし終えたか

    private byte[] _list_FPS { get; } = { 30, 60, 90, 120 };

    //UI関連
    [SerializeField, Header("for UI")] private GameObject savingUIObj;//「セーブ中」を示すUIオブジェクト
    [SerializeField] private RectTransform uiCanvasRect;//UIオブジェクトを表示させるCanvas

#if UNITY_EDITOR
    [SerializeField, Header("for Dev"), Tooltip("ゲーム起動時に初期化するか")]private bool Dev_Initialize;//起動時に初期化するか
    [SerializeField, Tooltip("セーブデータのjsonを読みやすく整形するか")]private bool Dev_jsonEasyRead = true; // セーブデータのjsonを読みやすく整形するか
    [SerializeField, Tooltip("セーブデータを暗号化するか")]private bool Dev_dataEncrypt = false; // セーブデータを暗号化するか
#endif

    ///////////////////////////////////////////////
    ///////////////////////////////////////////////
    ///////////////////////////////////////////////

    /// <summary>
    /// ユーザデータの「セーブ処理」を行う
    /// </summary>
    /// <param name="Save">セーブしたいSaveData変数</param>
    /// <param name="path">保存先パス・指定しなければSaveDataManagerで初期に取得したパスになる</param>
    /// <returns>セーブ終了したらtrue</returns>
    public bool Save(SaveData Save, string path = "")
    {
        devlog.log("ユーザデータの「セーブ処理」を開始します。");
        Save.str_appVer = Application.version.ToString();

        if(path == "")
        {
            path = _filePath;
        }

        //「セーブ中」を示すUIが設定されていれば表示させる。
        GameObject instedSavingUI = null;
        if (savingUIObj != null)
        {
            instedSavingUI = SLfunc.Instantiate(savingUIObj, uiCanvasRect);
        }

        SLfunc.CalcSavedDate(Save);//セーブ日を格納
        Save.set_check(SLfunc.CalcCheckDigit(Save));//チェックディジットを計算して格納

        // セーブデータの各変数をjson化する
        // 「第2引数をtrueにすると読みやすく整形される」らしい
    #if UNITY_EDITOR
        string SavedData_Json = JsonUtility.ToJson(Save, Dev_jsonEasyRead);
        if(Dev_dataEncrypt)
        {   
            File.WriteAllBytes(path, SLfunc.enc(SavedData_Json, ps.list[3]));
        }
        else
        {
            // jsonで保存する（平文）
            StreamWriter Writer = new StreamWriter(path);
            Writer.Write(SavedData_Json);
            Writer.Flush();
            Writer.Close();
        }
    #else
        // 問答無用に暗号化
        string SavedData_Json = JsonUtility.ToJson(Save);
        File.WriteAllBytes(_filePath, SLfunc.enc(SavedData_Json, ps.list[3]));
    #endif

        //「セーブ中」を示すUIが表示されていたら削除する。
        if (savingUIObj != null)
        {
            Destroy(instedSavingUI);
        }

        devlog.log("セーブしました。内容は以下の通りです。\n" + SavedData_Json);
        return true;
    }

    /// <summary>
    /// 取得したファイルパスにあるユーザデータの「ロード処理」を行う
    /// </summary>
    /// <param name="path">読み込み先パス・指定しなければSaveDataManagerで初期に取得したパスになる</param>
    /// <param name="forced">強制的にロードさせるか</param>
    /// <returns>ロードしたSaveData変数</returns>
    public SaveData Load(string path = "", bool forced = false)
    {
        // 未ロードor強制ロードの場合、ロード処理をする
        if (!Loaded || forced)
        {
            devlog.log("ユーザデータの「ロード処理」を開始します。");

            if(path == "")
            {
                path = _filePath;
            }

            if (File.Exists(path))
            {
                // セーブデータが存在する場合
                string LoadedData_Json = "";
            #if UNITY_EDITOR
                if(Dev_dataEncrypt)
                {
                    LoadedData_Json = SLfunc.dec(_filePath, ps.list[3]);
                }
                else
                {
                    StreamReader streamReader = new StreamReader(_filePath);
                    LoadedData_Json = streamReader.ReadToEnd();
                    streamReader.Close();
                }
            #else
                LoadedData_Json = SLfunc.dec(path, ps.list[3]);
            #endif

                saveData = JsonUtility.FromJson<SaveData>(LoadedData_Json);

                if (!SLfunc.CheckCheckDigit(saveData))
                {
                    // 不正検知した場合
                    devlog.log("ユーザデータを初期化します。");
                    Save(SLfunc.InitializeData(), path);//ロード用変数に初期状態のSaveDataを格納後セーブする
                    Load(path);
                }
                else
                {
                    // 最終セーブ時と起動バージョンが異なる場合、セーブデータの宣言配列数が異なることに備えてデータを移し替えておくべき？
                    if(saveData.str_appVer != Application.version.ToString())
                    {
                        devlog.logWarning("最終セーブ時と起動バージョンが異なります！");
                        // ここらへんで、セーブデータの宣言配列数が異なることに備えてデータを移し替える？
                    }

                    devlog.log("ロードしました。内容は以下の通りです。\n" + LoadedData_Json);
                }
            }
            else
            {
                // セーブデータが存在しない場合
                devlog.logWarning("ユーザデータが存在しません。新しく作成します。");
                Save(SLfunc.InitializeData(), path); // ロード用変数に初期状態のSaveDataを格納後セーブする
                Load(path);
            }
        }
        Loaded = true;
        return saveData; // 最後にロードしたデータを返す
    }
    ///////////////////////////////////////////////
    ///////////////////////////////////////////////
    ///////////////////////////////////////////////

    /// <summary>
    /// SaveDataManagerの初期化。Manager_CommonGroupで呼び出す。
    /// </summary>
    public void Initialize()
    {
#if UNITY_EDITOR
        // ユーザデータの初期化
        _filePath = SLfunc.GetPath(_fileName, Dev_dataEncrypt);
        if(Dev_Initialize)
        {
            SaveData newSave = SLfunc.InitializeData();
            newSave.playerInfo = new def_player("", "");
            Save(newSave, _filePath);
        }
#else
        _filePath = SLfunc.GetPath(_fileName);
#endif
        Load(_filePath, true);
        StartCoroutine(CountTime()); // プレイタイムカウント開始
        QualitySettings.vSyncCount = 0; // 垂直同期をOffにする
        Application.targetFrameRate = _list_FPS[saveData.FPS]; // FPS設定
        SLfunc.CountStartup(saveData);
#if true
        // 名前が存在しない＝初期起動かどうか・アイテム付与まだされてないか
        if ((saveData.playerInfo.get_name(def_player.playerNameKind.player) == "") && (saveData.playerInfo.get_havingItem(43) == 0))
        {
            // 初期アイテムプレゼント
            // 型紙
            saveData.playerInfo.set_havingPattern_new(5, 1);
            // 生地
            saveData.playerInfo.set_havingItem(31, 5);
            // 染料
            saveData.playerInfo.set_havingItem(43, 1);
            saveData.playerInfo.set_havingItem(44, 1);
            saveData.playerInfo.set_havingItem(45, 1);
        }
#endif
    }


    /// <summary>
    /// 起動時間をカウントする・ポインタ渡しで色んなカウントに対応してえ
    /// </summary>
    IEnumerator CountTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (saveData.get_playTime() != SLfunc.PLAYTIMELIMIT)
            {
                // カウント上限じゃないかどうか
                saveData.set_playTime();
            }
        }
    }

    /// <summary>
    /// セーブデータの初期化
    /// </summary>
    public void dataDelete()
    {
        Save(SLfunc.InitializeData());
        Initialize();
    }

    /// <summary>
    /// ゲーム終了時、自動的にセーブする
    /// </summary>
    private void OnApplicationQuit()
    {
        devlog.logWarning("ゲーム終了時のオートセーブを行います。");
        Save(saveData, _filePath);
    }
    /// <summary>
    /// アプリがサスペンドされた時の処理・オートセーブ/ロード
    /// </summary>
    /// <param name="pauseStatus"></param>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            devlog.log($"アプリが一時停止(バックグラウンドに行った)");
            Save(saveData, _filePath);
        }
#if false
        else
        {
            devlog.log($"アプリが再開(バックグラウンドから戻った)");
            Load("", true);
        }
#endif
    }
}