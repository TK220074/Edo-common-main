using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class manager_album_komon : albumCommon
{
    public static manager_album_komon instance;

    [SerializeField, Header("DetailBG_Top")]private rankStamp rankImg;
    [SerializeField]private komonLengthBars komonLengthBarsUI;
    [SerializeField]private Image[] patternImg; // 詳細情報における、小紋のサムネイル・下地[0]/柄[1]
    [SerializeField] private GameObject lengthSelectorObj;

    [SerializeField, Header("DetailBG_rank")]private GameObject rankPageGroup;
    [SerializeField]private rankStamp[] rankStamps;

    [SerializeField, Header("LengthSelector")] private Button[] lengthButtons;

    private bool openedPageisRank = false; // 詳細画面がTopとrankPageのどちらを表示しているか

    public def_komon.komonLength selectingKomonLength { get; private set; } // （選択モードの時）選択した長さ

    override protected void OnEnable()
    {
        base.OnEnable();
        openDetailTop(detailGroup, false);
        openDetailTop(rankPageGroup, false);

        // 選択モード用のボタンを念のため非表示にする
        lengthSelectorObj.SetActive(false);
    }

    override protected void Start()
    {
        base.Start();
        setupList(albumListInfo.albumKind);
    }

    /// <summary>
    ///  小紋詳細表示の内容を更新する
    /// </summary>
    /// <param name="id">表示させる小紋のID</param>
    public void reloadDetail(ushort id)
    {
        selectingId = id; // 詳細表示中の型紙のID
        def_komon data = commonM.saveM.saveData.playerInfo.havingKomon[id];

        // サムネイル
        for(int i = 0; i < 4; i += 2)
        {
            patternImg[i].color = data.get_colorNum_toRgb(def_komon.creatonTiming.ground); // 地色
            patternImg[i + 1].sprite = commonM.dataM.imgList_pattern.list[(int)data.get_patternId(def_komon.creatonTiming.ground)]; // ベース柄
        }

        // 名前系
        itemNameText.text = data.get_name(def_komon.creatonTiming.ground); // 小紋
        explainTexts[0].text = data.get_name(def_komon.creatonTiming.first); // 初回制作者名
        explainTexts[1].text = data.get_name(def_komon.creatonTiming.last); // 最終制作者名

        // 日付系
        System.DateTime date = data.get_date_created(false);
        explainTexts[2].text = $"{date.Year} / {date.Month.ToString("00")} / {date.Day.ToString("00")}";// 初回制作日
        date = data.get_date_created(true);
        explainTexts[3].text = $"{date.Year} / {date.Month.ToString("00")} / {date.Day.ToString("00")}";// 最終制作日

        // ランク表示の設定
        rankImg.setRangImg(data.get_rank(def_komon.creationPart.summary));
        for(int i = 0; i < 4; i++)
        {
            rankStamps[i].setRangImg(data.get_rank((def_komon.creationPart)i));
        }
        
        // 売却額表示
        bool[] temp = checkSellable(data);
        buttonsReaction[0] = temp[0];
        buttonsReaction[1] = temp[1];
        setSellingPriceText(temp[0], temp[1], checkSellingPrice(data), explainTexts[4]);

        // 小紋の残り長さ表示更新
        komonLengthBarsUI.setKomonLengthBars(data.get_length());

        // 残り上塗り回数
        explainTexts[5].text = $"あと {data.get_coatLimit()} 回";

        // 画面表示切替
        openedPageisRank = false;
        openDetailTop(detailGroup, true);
        openDetailTop(rankPageGroup, false);
        if(modeIsSelecting)
        {
            // 選択モードの時は、長さ選択ウィンドウを念のため非表示
            lengthSelectorObj.SetActive(false);
            reloadLengthButton(id); // 長さ選択ボタンの押下可不可を更新
        }
    }

    /// <summary>
    /// 実行するたびに、詳細表示の内容を切り替える
    /// </summary>
    public void changePage()
    {
        openedPageisRank = !openedPageisRank;
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        detailGroup.SetActive(!openedPageisRank);
        rankPageGroup.SetActive(openedPageisRank);
    }

    public void button_komonLooking()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        devlog.logWarning("ここで小紋全体を見る画面にしてください！");
    }

    /// <summary>
    /// 小紋の長さ選択画面を開く
    /// </summary>
    public void openLengthSelector()
    {
        openDetailTop(lengthSelectorObj, true);
    }
    /// <summary>
    /// 長さボタン押下・押された小紋の長さを記録する
    /// </summary>
    /// <param name="lengthValue">小紋の長さ</param>
    public void button_komonLength(int lengthValue)
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        if (lengthValue < 4)
        {
            selectingKomonLength = (def_komon.komonLength)lengthValue;
            commonM.tempValue[0] = (short)selectingId;
            commonM.tempValue[1] = (short)lengthValue;

            // エリアセレクト画面であったら、選択小紋を反映させる
            if (manager_areaSelect.instance != null)
            {
                manager_areaSelect.instance.selectKomon();
            }

            Destroy(transform.parent.parent.gameObject);
        }
    }

    /// <summary>
    /// 小紋の長さに応じて、長さ選択ボタンの押下可/不可を設定する
    /// </summary>
    /// <param name="komonId">小紋のID</param>
    private void reloadLengthButton(uint komonId)
    {
        def_komon.komonLength length = player.havingKomon[(int)komonId].get_length();

        byte processed = 0; // enabledをtrueにした回数
        for (int i = 0; i < 3; i++)
        {
            if (processed < (int)length)
            {
                lengthButtons[i].interactable = true;
                processed++;
            }
            else
            {
                lengthButtons[i].interactable = false;
            }
        }
    }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
}