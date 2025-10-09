using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class profileContent : Singleton<profileContent>
{
    private SaveDataManager saveM;
    private def_player player;
    private DatabaseManager dataM;
    private AudioManager audioM;

    [SerializeField, Header("Name")] private TextMeshProUGUI name_player;
    [SerializeField] private TextMeshProUGUI name_workspace;
    [SerializeField] private TextMeshProUGUI name_title;

    [SerializeField, Header("Rank")] private TextMeshProUGUI num_summary;
    [SerializeField] private TextMeshProUGUI num_creation;
    [SerializeField] private TextMeshProUGUI levelDist_summary;
    [SerializeField] private TextMeshProUGUI levelDist_creation;

    [SerializeField, Header("Favorite")] private Image favImg_color;
    [SerializeField] private Image favImg_pattern;
    [SerializeField] private TextMeshProUGUI favTex;

    [SerializeField, Header("EditingWindow")] private RectTransform windowRect;
    [SerializeField] private GameObject editObj_playerTitle;
    [SerializeField] private GameObject editObj_favoriteColor;
    [SerializeField] private GameObject editObj_favoritePattern;

    // Start is called before the first frame update
    void Start()
    {
        Manager_CommonGroup m = Manager_CommonGroup.instance;
        saveM = m.saveM;
        player = saveM.saveData.playerInfo;
        dataM = m.dataM;
        audioM = m.audioM;

        img_BG.instance.changeBG(8); // BG切替

        setupProfile();
    }

    /// <summary>
    /// プロフィール内容を更新する
    /// </summary>
    public void setupProfile()
    {
        name_title.SetTextAndExpandRuby(dataM.list_playerTitle.list[player.get_titleId()].title); // 称号
        name_player.text = player.get_name(def_player.playerNameKind.player); // プレイヤ名
        name_workspace.SetTextAndExpandRuby($"{player.get_name(def_player.playerNameKind.workspace)}　<r=しょぞく>所属</r>"); // 工房名

        // 職人ランク
        uint rank = player.get_playerRank(def_komon.creationPart.summary);
        num_summary.text = rank.ToString();
        levelDist_summary.text = $"あと {dataM.get_pointToNextLevel(def_komon.creationPart.summary, (int)rank).ToString("0.0")} P"; // 次のランクまでの数値表示
        // 工程レベル
        num_creation.text = $"{player.get_playerRank(def_komon.creationPart.colorCreation).ToString()}\n{player.get_playerRank(def_komon.creationPart.dyeing).ToString()}\n{player.get_playerRank(def_komon.creationPart.washing).ToString()}";
        // 次のレベルまでの数値表示
        levelDist_creation.text = "";
        for (int i = 1; i < 4; i++)
        {
            rank = player.get_playerRank((def_komon.creationPart)i);
            levelDist_creation.text += $"あと {dataM.get_pointToNextLevel((def_komon.creationPart)i, (int)rank).ToString("0.0")} P\n";
        }

        // お気にのこと
        byte patternId = player.get_favorite_pattern(); // お気にの柄（型紙）の番号
        // 見た目反映
        favImg_color.color = player.get_favorite_color();
        favImg_pattern.sprite = dataM.imgList_pattern.list[patternId];
        favTex.SetTextAndExpandRuby($"{dataM.list_pattern.list[patternId].name}\n+\n{player.get_favorite_color_name()}"); // 組み合わせText
    }

    public void pressedEdit_playerTitle()
    {
        audioM.SE_Play(AudioManager.WhichSE.Done);
        Instantiate(editObj_playerTitle, windowRect);
    }
    /// <summary>
    /// プレイヤ/工房名の編集
    /// </summary>
    /// <param name="isPlayer">プレイヤ名の編集ならtrue。工房名ならfalse</param>
    public void pressedEdit_name(bool isPlayer)
    {
        audioM.SE_Play(AudioManager.WhichSE.Done);

        StartCoroutine(waitingNameInput(isPlayer));
    }
    public void pressedEdit_favoriteColor()
    {
        audioM.SE_Play(AudioManager.WhichSE.Done);
        Instantiate(editObj_favoriteColor, windowRect);
    }
    public void pressedEdit_favoritePattern()
    {
        audioM.SE_Play(AudioManager.WhichSE.Done);
        Instantiate(editObj_favoritePattern, windowRect);
    }

    /// <summary>
    /// 名前入力の待機コルーチン
    /// </summary>
    /// <param name="isPlayer">プレイヤ名の編集ならtrue。工房名ならfalse</param>
    /// <returns></returns>
    IEnumerator waitingNameInput(bool isPlayer)
    {
        def_player.playerNameKind nameKind = def_player.playerNameKind.player;
        if (!isPlayer)
        {
            // 工房名
            nameKind = def_player.playerNameKind.workspace;
        }
        window_nameInput input = Instantiate(dataM.window_nameInput, windowRect).GetComponent<window_nameInput>(); // ウィンドウ生成
        input.setupField(nameKind, true); // ウィンドウの初期化

        // 入力が終了するまで待機
        string value = "";
        while (true)
        {
            // 入力終了判定
            if (input.endInput)
            {
                value = input.inputvalue;
                input.letWindowDestroy = true; // ウィンドウ破棄を許可
                break;
            }
            else if(input == null)
            {
                // ウィンドウを閉じた＝変更を反映させない場合は何もしない
                break;
            }

            yield return null;
        }

        // 入力内容の検証
        // 何かしら変更が入ったと考えられる場合は、プロフィール画面を更新
        if (value != "")
        {
            if (isPlayer && (value != name_player.text))
            {
                setupProfile();
            }
            else if (!isPlayer && (value != name_workspace.text))
            {
                setupProfile();
            }
        }
    }
}
