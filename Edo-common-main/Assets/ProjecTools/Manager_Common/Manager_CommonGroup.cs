using UnityEngine;

/// <summary>
/// シーン共通Managerの親Obj
/// </summary>
public class Manager_CommonGroup : MonoBehaviour
{
    public static Manager_CommonGroup instance;

    [SerializeField, Header("Debug")] private GameObject debugLogObj; // FPSとかのモニタ表示するObj

    [SerializeField, Header("GameStarter - SceneCommonObj")] private GameObject cameraObj;
    [SerializeField] private GameObject homeBannerObj;
    public GameObject homeBannerObj_public => homeBannerObj;

    // Managerの登録
    // AudioManager
    [SerializeField, Header("Managers")] private AudioManager _audioM;
    public AudioManager audioM => _audioM;

    // SaveDataManager
    [SerializeField] private SaveDataManager _saveM;
    public SaveDataManager saveM => _saveM;

    // AchievementManager
    [SerializeField] private AchievementManager _achieveM;
    public AchievementManager achieveM => _achieveM;

    // SceneChanger
    [SerializeField] private SceneChanger _sceneChanger;
    public SceneChanger sceneChanger => _sceneChanger;

    // DatabaseManager
    [SerializeField] private DatabaseManager _dataM;
    public DatabaseManager dataM => _dataM;

    [SerializeField] private GameObject obj_eventSystem;

    /// <summary>
    /// シーン間のデータ受け渡し用、変数一時格納
    /// </summary>
    public short[] tempValue { get; set; } = new short[3];

    /// <summary>
    /// 再生するシナリオ番号・会話シーン遷移前にここへ格納しておく
    /// </summary>
    public ushort id_startScenario { get; set; }


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            gameStarter();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// ゲーム開始時の処理
    /// </summary>
    private void gameStarter()
    {
        _saveM.Initialize(); // SaveDataManagerの初期化・セーブデータの読み込みなど
        _audioM.Initialize();
        _sceneChanger.Initialize();
        obj_eventSystem.SetActive(true);

#if DEBUG
        debugLogObj.SetActive(true);//FPSとかのモニタ表示をアクティブ化する
#else
        Destroy(debugLogObj);
#endif

        // カメラ配置
        if(Camera.main == null)
        {
            Instantiate(cameraObj);
        }

        // ホームパートだったらトップバナー配置
        if ((_dataM.sceneInfo.list[_sceneChanger.get_nowSceneId()].part == sceneInfoEntity.gamePart.A) && (home_banner.instance == null))
        {
            Instantiate(homeBannerObj);
        }

         // BGM再生
        if(_dataM.sceneInfo.list[_sceneChanger.get_nowSceneId()].bgmId >= 0)
        {
            _audioM.BGM_Play(false, (byte)_dataM.sceneInfo.list[_sceneChanger.get_nowSceneId()].bgmId);
        }
        if(_dataM.sceneInfo.list[_sceneChanger.get_nowSceneId()].envBgmId >= 0)
        {
            _audioM.BGM_Play(true, (byte)_dataM.sceneInfo.list[_sceneChanger.get_nowSceneId()].envBgmId);
        }

        Input.multiTouchEnabled = false; // マルチタッチ無効化

        // 画面向きの固定化
        // https://uuma-memo.xyz/2024/02/29/714/
        Screen.autorotateToLandscapeLeft = true;　// 左向きを有効にする
        Screen.autorotateToLandscapeRight = true;　// 右向きを有効にする
        Screen.orientation = ScreenOrientation.AutoRotation;　// 画面の向きを自動回転に設定する
    }

    /// <summary>
    /// 値段表示のstringをつくる
    /// </summary>
    /// <param name="price">値段</param>
    /// <param name="zeroIsHyphen">price = 0 の場合、ハイフン表示にするならtrue</param>
    /// <returns>値段表示</returns>
    public string get_priceString(int price, bool zeroIsHyphen = false)
    {
        string result = $"{dataM.get_constString(1)} "; // 通貨単位
        if (zeroIsHyphen)
        {
            result += "-";
        }
        else
        {
            result += price.ToString("N0"); // カンマ付きの数値表現
        }
        return result;
    }

    /// <summary>
    /// 入力したCMYK値をRGB値に変換する
    /// </summary>
    /// <param name="cmyk">RGB値に変換したいCMYK値</param>
    /// <returns></returns>
    public Color get_cmykToRgb(Color cmyk)
    {
        Color result = new Color();
        float r = (1 - Mathf.Min(1, cmyk.r * (1 - cmyk.a) + cmyk.a));
        float g = (1 - Mathf.Min(1, cmyk.g * (1 - cmyk.a) + cmyk.a));
        float b = (1 - Mathf.Min(1, cmyk.b * (1 - cmyk.a) + cmyk.a));
        result = new Color(r, b, g);
        return result;
    }
}
