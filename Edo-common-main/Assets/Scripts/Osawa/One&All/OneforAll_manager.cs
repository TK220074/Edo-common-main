using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using static def_komon;

using Random = UnityEngine.Random;

public class OneforAll_manager : MonoBehaviour
{
    //public static OneforAll_manager instance;

    [SerializeField] private creationPart creationPartNum = creationPart.dyeing; // 何個目の制作工程か＝再生するイマジネーション動画の番号
    [SerializeField, Header("Database")] private list_constFloat_komonCreation list_komonCreationValiable;
    [SerializeField] private VideoList videoList;

    private string nowScene;
    private string nexScene;
    [SerializeField, Header("CreationParts")] private GameObject _base;
    [SerializeField] private GameObject komon;
    [SerializeField] private GameObject katagami;
    [SerializeField] private testsome Testsome;
    [SerializeField] private colorselect Colorselect_k;
    [SerializeField] private gamisome Gamisome;
    [SerializeField] private PatenList paten_C;

    private string name_b = "button_selectThis";

    public bool start_kc;
    public Color color_k;
    public short idCheck = 2;

    [SerializeField, Header("UI Object")] private RectTransform canvasRect;
    [SerializeField] private GameObject obj_imagination;
    private imagination imaginationScript;
    [SerializeField] private GameObject obj_album_pattern;
    [SerializeField] private GameObject obj_album_color;

    [SerializeField] private GameObject textbox;

    private def_player player;
    private Manager_CommonGroup commonM;

    private bool letPress_endButton; // 終了ボタン二度おしを防ぐ用フラグ

    void Start()
    {
        
        start_kc = true;
        //CheckInstance();
        commonM = Manager_CommonGroup.instance;
        player = commonM.saveM.saveData.playerInfo;
        //player.set_money(10000);
        List<ItemsEntity> list = commonM.dataM.list_items.list;
        //player.set_havingItem((ushort)43, 1);
        //player.set_havingItem((ushort)31, 1);
        //player.set_havingPattern_new((byte)0, (ushort)commonM.dataM.list_pattern.list[0].durability);
        //Instantiate(obj_album_color, canvasRect);
        obj_album_pattern = Instantiate(obj_album_pattern, canvasRect);
        //obj_album_pattern.SetActive(false);
        //commonM.tempValue[0] = idCheck;
        //Createstanby();
        _base.SetActive(false);
        komon.SetActive(false);
        katagami.SetActive(false);

        obj_imagination.SetActive(false);
        imaginationScript = obj_imagination.GetComponent<imagination>();

        letPress_endButton = true;

        if (commonM.saveM.saveData.playerInfo.isEndTutorial[0])
        {
            Destroy(textbox);
        }
        else
        {
            //commonM.tempValue[0] = 0;
        }
    }

    void Update()
    {
        // Createstanby();
        //Createcheck();
        if (Input.GetMouseButton(0))
        {
            Destroy(textbox);
        }
    }

    /*
    void CheckInstance()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        if (nowScene == null)
        {
            nowScene = SceneManager.GetActiveScene().name;
        }

    }*/
    public void komon_patan()
    {
        Debug.Log("ugoitaze");
        //Instantiate(obj_album_pattern, canvasRect);
        //byte colorid = ((byte)commonM.tempValue[0]);色情報が未実装？
#if false
        // 色レシピは小紋完成以後に使用します・ここで使う色情報は、前工程（色糊づくり）のものになります。
        byte colorid = (byte)UnityEngine.Random.Range(1, 5);
        def_colorRecipe data = commonM.saveM.saveData.playerInfo.get_havingColor(colorid);
        color_k = data.get_looksColor(true);
#else
        // 色糊づくりができるまでの暫定的処置
        //color_k = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f,1f), UnityEngine.Random.Range(0f,1f), 1f);
        devlog.log(color_k);
#endif
        komon_createstart();

    }
    public void komon_createstart()
    {
        if (start_kc==true)
        {
            komon_onoff(start_kc);
            Createcheck();

            Destroy(textbox);
        }
        else
        {
            start_kc = true;
        }


    }

    public void komon_onoff(bool redy)
    {
        //_base.SetActive(redy);
        //komon.SetActive(redy);
        katagami.SetActive(redy);
    }

    public void Createcheck()
    {
        idCheck = commonM.tempValue[0];
        devlog.log(idCheck + "が選択");

        var item = paten_C.GetItemById(idCheck);
        if (item == null)
        {
            Debug.LogError($"[Createcheck] paten_C に id={idCheck} のパターンが存在しません。");
            return;
        }

        if (item.lists == null)
        {
            Debug.LogError($"[Createcheck] id={idCheck} の Paten_L.lists が未設定です。");
            return;
        }

        Gamisome.baseTexture = item.lists;
        Gamisome.sourceTexture = item.lists;
        Colorselect_k.sourceTexture = item.lists;
        Gamisome.color_change = color_k;
        Gamisome.Start_N();
        Colorselect_k.ReplaceColors();
    }

    public void Createstanby()
    {
        Button button = FindButtonByName(obj_album_pattern.transform, name_b);

        if (button != null)
        {
            devlog.log("ボタンが見つかりました: " + button.name);
            // ボタンにクリックイベントを追加
            button.onClick.AddListener(Createcheck);
            devlog.log(name_b + " ボタンがクリックされました！");
        }
        else
        {
            devlog.log("指定された名前のボタンが見つかりません: " + name_b);
        }

        //Destroy(transform.parent.parent.gameObject);
    }

    public void commonCreate()
    {
        nexScene = "KomonCreat";
        StartCoroutine(LoadNextSceneAsync(nowScene, nexScene));

#if false
        // シーン遷移する
        int sceneId = commonM.sceneChanger.get_nowSceneId(); // 今のシーン番号取得
        sceneId = commonM.dataM.get_nextSceneId(sceneId); // 今のシーンに対応する、次のシーン番号取得
        commonM.sceneChanger.SceneChange((byte)sceneId); // シーン遷移実行
#endif

    }

    IEnumerator LoadNextSceneAsync(string thisScene, string nextScene)
    {
        SceneManager.UnloadSceneAsync(thisScene);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
        nowScene = nextScene;
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private Button FindButtonByName(Transform parent, string name)
    {
        //devlog.log("aaaaa");
        foreach (Transform child in parent)
        {
            // 子オブジェクトの名前をチェック
            if (child.name == name)
            {
                // 名前が一致したらボタンコンポーネントを取得

                return child.GetComponent<Button>();
            }

            // 子オブジェクト内を再帰的に探索
            Button button = FindButtonByName(child, name);
            if (button != null)
            {
                return button;
            }
        }

        return null; // 見つからない場合はnullを返す
    }

    public static Vector4 RGBToCMYK(Color color)
    {
        float r = color.r;
        float g = color.g;
        float b = color.b;

        // ブラック(K)の値を計算
        float k = 1 - Mathf.Max(r, Mathf.Max(g, b));

        // ブラックが1の場合、すべての色が0になる
        if (Mathf.Approximately(k, 1.0f))
        {
            return new Vector4(0, 0, 0, 1);
        }

        // シアン(C), マゼンタ(M), イエロー(Y)の値を計算
        float c = (1 - r - k) / (1 - k);
        float m = (1 - g - k) / (1 - k);
        float y = (1 - b - k) / (1 - k);

        return new Vector4(c, m, y, k);
    }

    public void createcomplete()
    {
        if (letPress_endButton)
        {
            Gamisome.finish = true;
            letPress_endButton = false;

            commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
            player.set_havingPattern_use((ushort)idCheck); // 型紙の使用
            StartCoroutine(waitingNameInput()); // 名前入力へ進む
        }
    }
    IEnumerator waitingNameInput()
    {
        // 小紋名
        window_nameInput input = Instantiate(commonM.dataM.window_nameInput, canvasRect).GetComponent<window_nameInput>(); // ウィンドウ生成
        input.setupField(def_player.playerNameKind.komon, false); // ウィンドウの初期化

        // 入力が終了するまで待機
        string komonName = "";
        while (true)
        {
            // 入力終了判定
            if (input.endInput)
            {
                komonName = input.inputvalue;
                input.letWindowDestroy = true; // ウィンドウ破棄を許可
                break;
            }
            else if (input == null)
            {
                // ウィンドウを閉じた＝変更を反映させない場合は何もしない
                break;
            }

            yield return null;
        }

#if false
        // 色レシピ名
        input = Instantiate(commonM.dataM.window_nameInput, canvasRect).GetComponent<window_nameInput>(); // ウィンドウ生成
        input.setupField(def_player.playerNameKind.colorRecipe, false); // ウィンドウの初期化

        // 入力が終了するまで待機
        string colorName = "";
        while (true)
        {
            // 入力終了判定
            if (input.endInput)
            {
                colorName = input.inputvalue;
                input.letWindowDestroy = true; // ウィンドウ破棄を許可
                break;
            }
            else if (input == null)
            {
                // ウィンドウを閉じた＝変更を反映させない場合は何もしない
                break;
            }

            yield return null;
        }
#endif

        // 新規小紋の登録
        //Vector4 cmyk_k = RGBToCMYK(Colorselect_k.replacementColor);
        Vector4 cmyk_k = RGBToCMYK(color_k);
        devlog.log(cmyk_k);
        def_komon.komonRank[] ranks = new def_komon.komonRank[4] { def_komon.komonRank.s, def_komon.komonRank.a, def_komon.komonRank.b, def_komon.komonRank.c };
        player.havingKomon.Add(new def_komon(komonName, player.get_name(def_player.playerNameKind.player), cmyk_k, (byte)idCheck, DateTime.Now, def_komon.komonLength.s, ranks, 15f, true));

        //player.havingKomon.Add(new def_komon(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)), (byte)Random.Range(0, 11), DateTime.Now, (def_komon.komonLength)Random.Range(3, 4), Random.Range(10f, 25f)));
#if false
        // 色レシピの登録
        manager_B_colorPaste pasteM = manager_B_colorPaste.instance;
        player.havingColor.Add(new def_colorRecipe(colorName, pasteM.selectedDyeId, pasteM.inputAmount, color_k, , false));
#endif

        // チュートリアル終了しているか
        if (!commonM.saveM.saveData.playerInfo.isEndTutorial[0])
        {
            // 終了していない
            commonM.saveM.saveData.playerInfo.isEndTutorial[0] = true; // 小紋づくりTutorial終了フラグ
            commonM.id_startScenario = 1; // 次のチュートリアルシナリオ(1番)を再生するようにする
            commonM.sceneChanger.SceneChange(commonM.dataM.get_nextSceneId(0, 1)); // チュートリアルシナリオテストシーン・タイトルシーンのnextSceneIdを参照している
        }
        else
        {
            // Tutorial終了済→普通の小紋づくりパートの終了処理
            commonM.sceneChanger.SceneChange(2); // ホーム画面に戻る
        }
    }

    /// <summary>
    /// イマジネーション動画再生ボタンを押したときの処理
    /// </summary>
    public void button_imagination()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        commonM.audioM.SE_Play(24);
        imaginationScript.setVideo(videoList.list[(int)creationPartNum - 1]);
        obj_imagination.SetActive(true);
    }
}
