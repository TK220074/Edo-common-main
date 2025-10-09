using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Manager_A_Home : MonoBehaviour
{
    [SerializeField]private RectTransform canvasRect;

    [SerializeField] private GameObject obj_settings;

    [SerializeField, Header("Text_MenuButton")] private TextMeshProUGUI[] text_menuButton;

    [SerializeField, Header("for Partner")] private list_trivia list_Trivia;
    [SerializeField] private GameObject obj_partner; // 相棒
    [SerializeField] private GameObject obj_bubbleBox; // フキダシ
    [SerializeField] private TextMeshProUGUI text_partnerComment; // トリビア表示するText
    [SerializeField] private float timeForCloseComment = 7.4f; // コメント表示から消えるまでの時間
    private float countForCloseComment; // コメント非表示のためのカウンタ

    [SerializeField, Header("obj_album")] private GameObject obj_window_selectAlbum;
    [SerializeField]private GameObject obj_album_pattern;
    [SerializeField] private GameObject obj_album_komon;
    [SerializeField] private GameObject obj_album_colorRecipe;
    [SerializeField] private GameObject obj_album_storage;

    [SerializeField, Header("komon Creation")] private GameObject obj_window_notCreate; // 制作条件整っていないことを表示するウィンドウ
    [SerializeField] private TextMeshProUGUI text_windowMessage;

    private Manager_CommonGroup commonM;
    
    // Start is called before the first frame update
    void Start()
    {
        commonM = Manager_CommonGroup.instance;

        obj_bubbleBox.SetActive(false); // トリビアフキダシ非表示
        bool doingTeisatsu = commonM.saveM.saveData.playerInfo.doingReconnaissance; // ていさつ中フラグ取得
        obj_partner.SetActive(!doingTeisatsu); // ていさつ中だったら相棒表示しない
        // ていさつ中だったらコメント初期更新しない
        if (!doingTeisatsu)
        {
            changePartnerComment();
        }

        // チュートリアル(2)終了しているか
        // （全チュートリアル終了後、ホーム画面に戻ってくるという前提の処理）
        if (!commonM.saveM.saveData.playerInfo.isEndTutorial[2])
        {
            // 終了していない
            commonM.saveM.saveData.playerInfo.isEndTutorial[2] = true; // Tutorial2終了フラグ
        }
    }

    void Update()
    {
        // コメント表示されていたら、非表示にするカウントする
        if (obj_bubbleBox.activeInHierarchy)
        {
            if (countForCloseComment < timeForCloseComment)
            {
                countForCloseComment += Time.deltaTime;
            }
            else
            {
#if false
                obj_bubbleBox.SetActive(false);
#else
                changePartnerComment();
#endif
                countForCloseComment = 0f;
            }
        }
    }

    public void button_sceneChange(int sceneID)
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        commonM.sceneChanger.SceneChange((byte)sceneID);
    }
    
    public void button_openAlbumsWindow()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        obj_window_selectAlbum.SetActive(true);
    }
    public void button_openAlbum_pattern()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        Instantiate(obj_album_pattern, canvasRect);
    }
    public void button_openAlbum_komon()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        Instantiate(obj_album_komon, canvasRect);
    }
    public void button_openAlbum_colorRecipe()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        Instantiate(obj_album_colorRecipe, canvasRect);
    }
    public void button_openAlbum_storage()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        Instantiate(obj_album_storage, canvasRect);
    }

    public void button_settings()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        Instantiate(obj_settings, canvasRect);
    }

    /// <summary>
    /// ランダムで一言表示
    /// </summary>
    public void button_partner()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        changePartnerComment();
    }
    private void changePartnerComment()
    {
        devlog.logWarning("ここでしゃべる音？");
        //obj_bubbleBox.SetActive(false); // いったん消えて、再度ふわっと表示させる
        countForCloseComment = 0f;
        text_partnerComment.SetTextAndExpandRuby(list_Trivia.list[Random.Range(0, list_Trivia.list.Count)].comment);
        obj_bubbleBox.SetActive(true);
    }


    /// <summary>
    /// 小紋づくりできるかの判定する
    /// </summary>
    public void button_komonCreation()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);

        // 制作開始条件を検証
        def_player player = commonM.saveM.saveData.playerInfo;
        bool[] check = new bool[5];

        // 生地・染料があるか
        List<ItemsEntity> list = commonM.dataM.list_items.list;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].shopKind == shopKind.Cloth)
            {
                if (commonM.saveM.saveData.playerInfo.get_havingItem((ushort)i) > 0)
                {
                    // 生地が1種類でもあればOK
                    check[0] = true;
                    //break;  染料の確認をしないためアウト
                }
            }
            else if (list[i].shopKind == shopKind.Dye)
            {
                if (commonM.saveM.saveData.playerInfo.get_havingItem((ushort)i) > 0)
                {
                    // 染料が1種類でもあればOK
                    check[1] = true;
                    break;
                }
            }
        }

        // 型紙1種以上持ってるか？
        for (int i = 0; i < commonM.dataM.list_pattern.list.Count; i++)
        {
            if (player.get_havingPattern_haveNum((byte)i) > 0)
            {
                // 型紙1種類以上あればOK
                check[2] = true;
                break;
            }
        }

        // 小紋アルバムに空きがあるか
        if (player.get_havingKomon_len() < player.get_albumSize(def_player.albumKind.komon))
        {
            check[3] = true;
        }

        // 色レシピに空きがあるか
        if (player.get_havingColor_len() < player.get_albumSize(def_player.albumKind.color))
        {
            check[4] = true;
        }

        // 新品の生地が無い場合
        if (!check[0])
        {
            // 上塗りできる小紋を探す
            for (int i = 0; i < player.get_havingKomon_len(); i++)
            {
                if (player.havingKomon[i].get_coatLimit() > 0)
                {
                    // 上塗りできる小紋があれば、生地はあるものとしてチェック
                    check[0] = true;
                    break;
                }
            }
        }

        // ここまでの条件判定を検証
        string message = "";
        bool end = false; // 終了フラグ
        for (byte i = 0; i < check.Length; i++)
        {
            // 整ってない条件はあるか？
            if (!check[i])
            {
                // 条件に対応するメッセージを挿入
                message += $"{commonM.dataM.get_constString(i + 20)}　{commonM.dataM.get_constString(25)}\n\n";
                end = true;
            }
        }
        if (end)
        {
            // 開始条件整ってないウィンドウ出して終了
            text_windowMessage.SetTextAndExpandRuby(message);
            obj_window_notCreate.SetActive(true);
            devlog.log("小紋づくり できません！");
            return;
        }

        // 全てtrue = 条件整ってるなら、シーン遷移
        byte nextId = (byte)commonM.dataM.get_nextSceneId(commonM.sceneChanger.get_nowSceneId(), 0);
        commonM.sceneChanger.SceneChange(nextId);
    }
    /// <summary>
    /// 小紋づくり開始判定における、条件整ってないウィンドウでの了承ボタン押下時の処理
    /// </summary>
    public void button_komonCreationwindow_ok()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        obj_window_notCreate.SetActive(false);
    }
}
