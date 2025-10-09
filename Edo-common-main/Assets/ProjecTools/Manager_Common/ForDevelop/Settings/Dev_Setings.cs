#if DEBUG
using UnityEngine;
using TMPro;

public class Dev_Setings : MonoBehaviour
{
    private SaveDataManager saveM;
    private AudioManager audioM;

    [SerializeField, Header("for Audio")] private TextMeshProUGUI volumeText;
    [SerializeField, Header("for Resolution")] private TextMeshProUGUI resolText;
    [SerializeField] private TextMeshProUGUI screenModeText;
    [SerializeField, Header("for FPS")] private TextMeshProUGUI fpsText;
    [SerializeField] private TextMeshProUGUI vBlankText;
    [SerializeField, Header("for Save")] private list_items itemList;

    private Vector2 screenRes;// 解像度格納
    private bool everyVBlank;// 垂直同期設定状況 0,1 = しない,する

    // Start is called before the first frame update
    void Start()
    {
        Manager_CommonGroup commonM = Manager_CommonGroup.instance;
        saveM = commonM.saveM;
        audioM = commonM.audioM;
        screenRes = GetResMode();
        everyVBlank = System.Convert.ToBoolean(QualitySettings.vSyncCount);
        WriteVolumeText();
        WriteScreenMode();
        WriteFPSInfo();
    }

    public void setNewKomon()
    {
        saveM.saveData.playerInfo.havingKomon.Add(new def_komon(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)), (byte)Random.Range(0, 11), (def_komon.komonLength)Random.Range(1, 4), Random.Range(10f, 30f)));
        devlog.log("新規小紋作成ボタンを押下しました");
    }
    public void setNewColorRecipe()
    {
        saveM.saveData.playerInfo.havingColor.Add(new def_colorRecipe("開発用の適当レシピ", new short[3]{(short)Random.Range(43, 59), (short)Random.Range(43, 59), (short)Random.Range(43, 59)}, new float[3]{Random.Range(0f, 50f), Random.Range(0f, 50f), Random.Range(0f, 50f)}, new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)), new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)), false));
        devlog.log("新規色レシピ作成ボタンを押下しました");
    }

    /// <summary>
    /// ユーザデータの即時セーブ/ロード
    /// </summary>
    /// <param name="saveOrLoad">セーブ：true　ロード：false</param>
    public void UserDataSaveOrLoad(bool saveOrLoad)
    {
        if (saveOrLoad)
        {
            //その時点でのデータをセーブ
            devlog.logWarning("(Dev)ユーザデータの強制セーブを実行します。");
            saveM.Save(saveM.saveData);
        }
        else
        {
            //最後に保存したデータをロード
            devlog.logWarning("(Dev)ユーザデータの強制ロードを実行します。");
            saveM.Load(saveM.fileName, true);
            reset();
        }
    }

    /// <summary>
    /// ユーザデータの即時削除
    /// </summary>
    public void UserDataDelete()
    {
        devlog.logWarning("(Dev)ユーザデータの削除を実行します。");
        saveM.dataDelete();
        reset();
    }

    /// <summary>
    /// BGMの音量を変える
    /// </summary>
    /// <param name="plusOrMinus">大きくするならtrue</param>
    public void ChangeBGMVolume(bool plusOrMinus)
    {
        if(plusOrMinus)
        {
            if(audioM.GetAudioNum(AudioManager.WhichAudio.BGM) != 10)
            {
                audioM.ChangeVol(AudioManager.WhichAudio.BGM, (byte)(audioM.GetAudioNum(AudioManager.WhichAudio.BGM) + 1));
            }
        }
        else
        {
            if (audioM.GetAudioNum(AudioManager.WhichAudio.BGM) != 0)
            {
                audioM.ChangeVol(AudioManager.WhichAudio.BGM, (byte)(audioM.GetAudioNum(AudioManager.WhichAudio.BGM) - 1));
            }
        }
        WriteVolumeText();
    }

    /// <summary>
    /// SEの音量を変える
    /// </summary>
    /// <param name="plusOrMinus">大きくするならtrue</param>
    public void ChangeSEVolume(bool plusOrMinus)
    {
        if (plusOrMinus)
        {
            if (audioM.GetAudioNum(AudioManager.WhichAudio.SE) != 10)
            {
                audioM.ChangeVol(AudioManager.WhichAudio.SE, (byte)(audioM.GetAudioNum(AudioManager.WhichAudio.SE) + 1));
            }
        }
        else
        {
            if (audioM.GetAudioNum(AudioManager.WhichAudio.SE) != 0)
            {
                audioM.ChangeVol(AudioManager.WhichAudio.SE, (byte)(audioM.GetAudioNum(AudioManager.WhichAudio.SE) - 1));
            }
        }
        WriteVolumeText();
    }

    /// <summary>
    /// Volume表示を書き換える
    /// </summary>
    private void WriteVolumeText()
    {
        volumeText.text = $"\n{saveM.saveData.Volume[(int)AudioManager.WhichAudio.BGM - 1].ToString("00")}\n{saveM.saveData.Volume[(int)AudioManager.WhichAudio.SE - 1].ToString("00")}";
    }

    /// <summary>
    /// 実行のたびに解像度を変更する
    /// </summary>
    public void ChangeResolution()
    {
        switch (saveM.saveData.Resolution)
        {
            case 2:
                // HDにする
                saveM.saveData.Resolution = 0;
                break;

            default:
                saveM.saveData.Resolution++;
                break;

            
        }
        screenRes = GetResMode();
        Screen.SetResolution((int)screenRes.x, (int)screenRes.y, saveM.saveData.Screen);
        WriteScreenMode();
    }
    private Vector2 GetResMode()
    {
        Vector2 value = Vector2.zero;
        switch (saveM.saveData.Resolution)
        {
            case 0:
                value = new Vector2(1280, 720);
                break;

            case 1:
                value = new Vector2(1920, 1080);
                break;

            case 2:
                value = new Vector2(3840, 2160);
                break;
        }
        return value;
    }

    /// <summary>
    /// 実行のたびにフルスクリーン/ウィンドウを切り替える
    /// </summary>
    public void ChangeScreenMode()
    {
        saveM.saveData.Screen = !saveM.saveData.Screen;
        Screen.SetResolution((int)screenRes.x, (int)screenRes.y, saveM.saveData.Screen);
        WriteScreenMode();
    }

    /// <summary>
    /// ウィンドウ状態の表示を書き換える
    /// </summary>
    private void WriteScreenMode()
    {
        if (saveM.saveData.Screen)
        {
            screenModeText.text = "FullScreen";
        }
        else
        {
            screenModeText.text = "Window";
        }
        resolText.text = $"{(int)screenRes.x}×{(int)screenRes.y}";
    }

    public void ChangeVBlankMode()
    {
        everyVBlank = !everyVBlank;
        QualitySettings.vSyncCount = System.Convert.ToInt32(everyVBlank);
        WriteFPSInfo();
    }

    /// <summary>
    /// FPS値を変更する()
    /// </summary>
    public void ChangeFPS()
    {
        switch (saveM.saveData.FPS)
        {
            case 3:
                saveM.saveData.FPS = 0;
                break;

            default:
                saveM.saveData.FPS++;
                break;
        }
        byte value = (byte)((saveM.saveData.FPS + 1) * 30);
        Application.targetFrameRate = value;
        WriteFPSInfo();
    }

    private void WriteFPSInfo()
    {
        byte value = (byte)((saveM.saveData.FPS + 1) * 30);
        fpsText.text = value.ToString();
        if (everyVBlank)
        {
            vBlankText.text = "Every V Blank";
        }
        else
        {
            vBlankText.text = "Don't Sync";
        }
    }

    /// <summary>
    /// ゲームを終了させる
    /// </summary>
    public void EndGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
        Application.Quit();//ゲームプレイ終了
#endif
    }

    /// <summary>
    /// セーブデータの強制セーブ/ロードや消去（初期化）が行われた際のリセット処理
    /// </summary>
    private void reset()
    {
        audioM.ChangeVol(AudioManager.WhichAudio.BGM, saveM.saveData.Volume[(int)AudioManager.WhichAudio.BGM - 1]);
        audioM.ChangeVol(AudioManager.WhichAudio.SE, saveM.saveData.Volume[(int)AudioManager.WhichAudio.SE - 1]);
        screenRes = GetResMode();
        Screen.SetResolution((int)screenRes.x, (int)screenRes.y, saveM.saveData.Screen);
        QualitySettings.vSyncCount = 0;
        byte value = (byte)((saveM.saveData.FPS + 1) * 30);
        Application.targetFrameRate = value;
        WriteVolumeText();
        WriteScreenMode();
        WriteFPSInfo();
    }
}
#endif