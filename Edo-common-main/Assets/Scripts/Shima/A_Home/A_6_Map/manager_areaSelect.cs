using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class manager_areaSelect : Singleton<manager_areaSelect>
{
    [SerializeField, Header("Database")] private list_battle_stage battleStageList;
    [SerializeField] private komonDataLIst enemyKomonList;

    [SerializeField, Header("Map")] private RectTransform pinRect;

    // エリアボタン一覧周辺
    [SerializeField, Header("AreaList")] private RectTransform stageListRect;
    [SerializeField] private GameObject areaSelectButtonObj;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private TextMeshProUGUI text_button_modeChange; // モード切替ボタンの表記
    [SerializeField] private Button buttonObj_modeChange; // バトル←→ていさつのモード切替ボタン・ていさつ中は押せなくする目的
    [SerializeField] private TextMeshProUGUI[] clearAreaNumText; // クリア割合の表記Text
    [SerializeField] private RectTransform rect_canvas;

    // 選択エリア情報表示
    [SerializeField, Header("AreaDetail")] private GameObject areaDetailObj;
    [SerializeField] private TextMeshProUGUI text_nowMode;
    [SerializeField] private TextMeshProUGUI areaDetailAreaNameText;
    [SerializeField] private TextMeshProUGUI areaDetailAreaExplainText;
    [SerializeField] private TextMeshProUGUI areaDetailEnemyNameText;
    [SerializeField] private TextMeshProUGUI selectingKomonLengthText;
    [SerializeField] private Image[] enemyKomonImgs; // 敵の小紋を表示するImage
    [SerializeField] private GameObject[] areaDetailIconObjs; // クリア状況などを示すアイコン
    [SerializeField] private Button button_battle_letsGo; // バトル開始ボタン
    [SerializeField] private Button button_teisatsu_letsGo;
    [SerializeField] private GameObject[] obj_detailByMode; // バトル[0]・ていさつ[1]用のエリア情報表示Obj

    // 選択エリア情報表示 - ていさつモード特有
    [SerializeField, Header("AreaDetail_Teisatsu")] private GameObject[] obj_itemThumbnails;
    [SerializeField] private TextMeshProUGUI text_timeComment;
    private Image[] img_itemThumbnails = new Image[4];
    [SerializeField] private GameObject obj_teisatsuStart; // ていさつ開始時の演出用Obj

    // ていさつ結果系
    [SerializeField, Header("TeisatsuResult")] private GameObject obj_teisatsuEnd; // ていさつ終了時の演出用Obj
    [SerializeField] private TextMeshProUGUI text_teisatsuEnd_area; // ていさつ先のエリア名
    [SerializeField] private TextMeshProUGUI text_teisatsuEnd_top; // おかえりなさい！
    [SerializeField] private RectTransform rect_teisatsuEnd_itemsParent; // アイテムサムネイルたちの親ObjのRectTransform
    [SerializeField] private TextMeshProUGUI text_teisatsuEnd_partnerComment; // ていさつ終了時の、相手の情報表示text

    // バトル - 小紋選択
    [SerializeField, Header("SelectKomon")] private GameObject komonAlbumObj;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Image[] selectedKomonImgs;

    Manager_CommonGroup commonM;
    private def_player player;

    /// <summary>
    /// 選択中のエリアの番号
    /// </summary>
    public ushort id_selectingArea { get; private set; }
    /// <summary>
    /// 選択中の小紋の番号
    /// </summary>
    public byte id_selectingKomon { get; private set; }
    public def_komon.komonLength selectingKomonLength { get; private set; }


    private bool modeIsTeisatsu; // ていさつエリア選択モードか




    // Start is called before the first frame update
    void Start()
    {
        commonM = Manager_CommonGroup.instance;
        player = commonM.saveM.saveData.playerInfo;

        objActiveCheck(areaDetailObj, false); // エリア詳細を隠す
        obj_detailByMode[0].SetActive(true); // バトル情報は出しておく
        obj_detailByMode[1].SetActive(false); // ていさつ情報はあらかじめ隠す
        button_battle_letsGo.interactable = false; // バトル出陣ボタンは押せなくしておく
        obj_teisatsuStart.SetActive(false);
        obj_teisatsuEnd.SetActive(false);

        // ていさつ でのアイテム候補のサムネイル画像コンポーネント取得
        for (int i = 0; i < commonM.dataM.get_constFloat(18); i++)
        {
            img_itemThumbnails[i] = obj_itemThumbnails[i].GetComponent<RectTransform>().GetChild(0).GetChild(1).GetComponent<Image>();
        }
        teisatsuEndCheck();
        text_nowMode.SetTextAndExpandRuby(commonM.dataM.get_constString(27 + Convert.ToInt32(modeIsTeisatsu)));
        setupList();
    }


    /// <summary>
    /// GameObjectの表示/非表示
    /// </summary>
    /// <param name="obj">非表示にしたいもの</param>
    /// <param name="doOpen">閉じたければfalse</param>
    private void objActiveCheck(GameObject obj, bool doOpen)
    {
        if (obj.activeInHierarchy != doOpen)
        {
            obj.SetActive(doOpen);
        }
    }

    /// <summary>
    /// 敵が使用する小紋のサムネイルを設定する
    /// </summary>
    /// <param name="areaId">選択したエリアの番号</param>
    private void setEnemyKomonImg(ushort areaId)
    {
        // 指定したエリアがクリアしているか確認
        if (commonM.saveM.saveData.playerInfo.get_battle_areaClear(areaId))
        {
            // クリア済の場合
            // 敵小紋を表示
            ushort komonId = battleStageList.list[areaId].enemyKomonId;
            enemyKomonImgs[0].color = enemyKomonList.list[komonId].get_colorNum(def_komon.creatonTiming.ground); // 地色
            enemyKomonImgs[1].sprite = commonM.dataM.imgList_pattern.list[enemyKomonList.list[komonId].get_patternId(def_komon.creatonTiming.ground)]; // 柄
        }
        else
        {
            // 未クリアの場合
            // 敵小紋は表示しない
            enemyKomonImgs[0].color = Color.white; // 地色
            enemyKomonImgs[1].sprite = null; // 柄
        }
    }

    /// <summary>
    /// エリアリストの初期化
    /// </summary>
    private void setupList()
    {
        // リストUIにもう何かあるか＝初期のリロードではないか
        if (stageListRect.childCount > 0)
        {
            // リストをいったんリセット
            foreach (RectTransform child in stageListRect)
            {
                // 全ての子ObjをDestroy
                Destroy(child.gameObject);
            }
        }

        // バトルステージ数だけボタン生成
        // エリア0はチュートリアルなので1から
        for (ushort i = 1; i < battleStageList.list.Count; i++)
        {
            BattleStageEntity stage = battleStageList.list[i];
            // ボタン生成と情報書き換え

            // バトル前後にシナリオ設定あるか
            bool hasEvent = false;
            if ((stage.scenarioId_beforeStart != -1) || (stage.scenarioId_afterClear != -1))
            {
                hasEvent = true;
            }

            Instantiate(areaSelectButtonObj, stageListRect).GetComponent<a_6_areaButton>().setupButton(i, stage.name, player.get_battle_areaClear(i), stage.hasWorkspace, hasEvent);
        }

        // エリア数とクリア数とクリア率
        ushort clearNum = player.get_battle_areaClear_clearNum();
        clearAreaNumText[0].text = clearNum.ToString("00");
        clearAreaNumText[1].text = $"/{battleStageList.list.Count.ToString("00")}";
        float rate = (float)clearNum / (float)battleStageList.list.Count; // クリア率
        rate *= 100f;
        clearAreaNumText[2].text = $"{rate.ToString("0.0")}%";

        text_button_modeChange.SetTextAndExpandRuby(commonM.dataM.get_constString(27 + Convert.ToInt32(!modeIsTeisatsu)) + commonM.dataM.get_constString(29)); // モード切替ボタン書き換え（「ていさつにする」）

        scrollbar.value = 1.0f;
    }

    /// <summary>
    /// エリアボタンが押された時の処理
    /// </summary>
    /// <param name="pressedAreaId">押されたエリアの番号</param>
    public void pressedAreaButton(ushort pressedAreaId)
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        id_selectingArea = pressedAreaId;
        reloadDetail(pressedAreaId);

        objActiveCheck(areaDetailObj, true);
    }

    /// <summary>
    /// エリア詳細を書き換える
    /// </summary>
    /// <param name="areaId">書き換えるエリアの番号</param>
    private void reloadDetail(ushort areaId)
    {
        BattleStageEntity stage = battleStageList.list[areaId];

        areaDetailAreaNameText.SetTextAndExpandRuby($"【{stage.name}】");
        areaDetailAreaExplainText.SetTextAndExpandRuby(stage.explain);
        id_selectingArea = areaId;

        ///////////////////////
        // バトル情報について
        ///////////////////////
        areaDetailEnemyNameText.SetTextAndExpandRuby(stage.enemyName);

        // クリアアイコンの表示/非表示
        bool wasClear = player.get_battle_areaClear(areaId);
        areaDetailIconObjs[0].SetActive(wasClear);

        // 工房アイコンの表示/非表示
        areaDetailIconObjs[1].SetActive(stage.hasWorkspace);

        // イベント発生アイコンの表示/非表示
        // クリア前にのみ表示
        if (!wasClear)
        {
            // バトル前後にシナリオ設定あるか
            if((stage.scenarioId_beforeStart != -1) || (stage.scenarioId_afterClear != -1))
            {
                areaDetailIconObjs[2].SetActive(true);
            }
            else
            {
                areaDetailIconObjs[2].SetActive(false);
            }
        }
        else
        {
            areaDetailIconObjs[2].SetActive(false);
        }

        setEnemyKomonImg(areaId);



        ///////////////////////
        // ていさつ情報について
        ///////////////////////

        // アイテム候補サムネイルたちを一旦非表示
        for (int i = 0; i < obj_itemThumbnails.Length; i++)
        {
            obj_itemThumbnails[i].SetActive(false);
        }

        // 獲得アイテム候補を取得
        string[] items = stage.getItemId_reconnaissance.Split(',');
        // 候補が存在するか
        if (items.Length > 0)
        {
            bool letActiveThumbnail = false; // アイテム候補表示してよいか＝アイテム候補あるか
            for (int i = 0; i < items.Length; i++)
            {
                int id = int.Parse(items[i]);
                // アイテムIDが-1などの負の数だったら、獲得アイテム無しとして扱う
                if (id < 0)
                {
                    for (int j = 0; j < obj_itemThumbnails.Length; j++)
                    {
                        obj_itemThumbnails[i].SetActive(false); // サムネイルを全て隠す
                    }
                    break;
                }
                letActiveThumbnail = true;
            }

            if (letActiveThumbnail)
            {
                button_teisatsu_letsGo.interactable = true;

                // 獲得アイテム候補の数が表示上限以下だったら、「...」は表示しない
                if (items.Length <= commonM.dataM.get_constFloat(18))
                {
                    objActiveCheck(obj_itemThumbnails[(int)commonM.dataM.get_constFloat(18)], false); // 最後の「...」を非表示
                }
                else
                {
                    objActiveCheck(obj_itemThumbnails[(int)commonM.dataM.get_constFloat(18)], true); // 最後の「...」を表示
                }

                // サムネイル設定
                for (int i = 0; i < items.Length; i++)
                {
                    int id = int.Parse(items[i]);

                    // 獲得候補のアイテムアイコン生成とImage取得、アイコン設定
                    objActiveCheck(obj_itemThumbnails[i], true);
                    img_itemThumbnails[i].sprite = commonM.dataM.imgList_items.list[id];

                    // 表示するアイテム候補は最大3つまで。もう3つ表示したか？
                    if (i >= commonM.dataM.get_constFloat(18) - 1)
                    {
                        break;
                    }
                }
            }
            else
            {
                button_teisatsu_letsGo.interactable = false;
            }
        }
        else
        {
            button_teisatsu_letsGo.interactable = false;
        }

        // 所要時間について
        // 獲得済エリアからの距離によって所要時間が変わる
        int time = 0;
        // ここで、選択したエリアの、獲得済エリアからの距離を判定したい
        if (true)
        {
            // エリアA：獲得済エリア
            time = (int)commonM.dataM.get_constFloat(15);

            // エリアB：獲得済に隣り合うエリア
            //time = (int)commonM.dataM.get_constFloat(16);

            // エリアC：エリアBに隣り合うエリア
            //time = (int)commonM.dataM.get_constFloat(17);
        }

        commonM.saveM.saveData.playerInfo.time_reconnaissanceRequired = (byte)time; // ていさつ所要時間をあらかじめ入れてしまう
        text_timeComment.text = time.ToString() + commonM.dataM.get_constString(26);
    }

    /// <summary>
    /// 小紋アルバムを開く
    /// </summary>
    public void openKomonAlbum()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        Instantiate(komonAlbumObj, canvasRect);
    }
    /// <summary>
    /// 小紋アルバムにて、使用する小紋の選択ボタン（これにする）を押した時の処理
    /// </summary>
    public void selectKomon()
    {
        def_komon data = player.havingKomon[commonM.tempValue[0]];
        id_selectingKomon = (byte)commonM.tempValue[0]; // 選択した小紋の番号を記録

        selectedKomonImgs[0].color = data.get_colorNum_toRgb(def_komon.creatonTiming.ground); // 地色

        selectedKomonImgs[1].sprite = commonM.dataM.imgList_pattern.list[data.get_patternId(def_komon.creatonTiming.ground)]; // ベース柄

        selectingKomonLength = (def_komon.komonLength)commonM.tempValue[1];
        selectingKomonLengthText.text = $"【{data.getLengthString(selectingKomonLength)}】"; // 選択中の長さ

        button_battle_letsGo.interactable = true;
        Debug.Log(data);
        Debug.Log(data.get_colorNum_toRgb(def_komon.creatonTiming.ground));
    }

    /// <summary>
    /// 出陣ボタン押された時の処理
    /// </summary>
    public void pressButtonGo()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);

        if (!modeIsTeisatsu)
        {
            // バトル開始
            commonM.tempValue[0] = id_selectingKomon;
            commonM.tempValue[1] = (short)selectingKomonLength;
            commonM.tempValue[2] = (short)id_selectingArea;

            devlog.logWarning("ここで戦闘準備画面へ");
            // 以下のどちらか？で　バトルシーン？戦闘準備シーン？へ遷移
            commonM.sceneChanger.SceneChange(10, true);
            // commonM.sceneChanger.SceneChange(battleStageList.list[id_selectingArea].battleSceneId);
        }
        else
        {
            // ていさつ開始
            commonM.saveM.saveData.playerInfo.doingReconnaissance = true; // ていさつ中フラグたてる
            commonM.saveM.saveData.playerInfo.time_reconnaissanceStart = DateTime.Now.ToString(); // ていさつ開始時刻に現日時を入れる
            commonM.saveM.saveData.playerInfo.id_reconnaissanceArea = (byte)id_selectingArea; // ていさつ先IDを記録

            devlog.logWarning("ここでていさつ開始演出");
            obj_teisatsuStart.SetActive(true);
            commonM.saveM.Save(commonM.saveM.saveData);
            StartCoroutine(teisatsuStart());
        }
    }
    IEnumerator teisatsuStart()
    {
        commonM.audioM.BGM_Play(false, 11);
        yield return new WaitForSeconds(4.5f);
        thisSceneReload();
    }
    public void thisSceneReload()
    {
        commonM.sceneChanger.SceneChange(commonM.sceneChanger.get_nowSceneId()); // シーン初期化
    }

    /// <summary>
    /// 初期化時の、ていさつ終了判定・終了時のリザルト処理
    /// </summary>
    private void teisatsuEndCheck()
    {
        // ていさつ状況に応じたUI表示
        bool isProgress = commonM.saveM.saveData.playerInfo.doingReconnaissance; // ていさつしているか
        buttonObj_modeChange.interactable = !isProgress; // モード切替の押下可・不可
        if (isProgress)
        {
            float leftT = commonM.saveM.saveData.playerInfo.get_teisatsuLeftTime(); // ていさつ残り時間
            if (leftT <= 0f)
            {
                // ていさつ終了
                commonM.audioM.BGM_Play(false, 13);
                commonM.saveM.saveData.playerInfo.doingReconnaissance = false; // ていさつ中フラグおろす

                // ていさつ結果表示のtext
                text_teisatsuEnd_area.SetTextAndExpandRuby($"{battleStageList.list[commonM.saveM.saveData.playerInfo.id_reconnaissanceArea].name}{commonM.dataM.get_constString(39)}"); //～から帰ってきた！
                text_teisatsuEnd_top.SetTextAndExpandRuby(commonM.dataM.get_constString(40)); // おかえり！

                obj_teisatsuEnd.SetActive(true); // ていさつ終了演出表示

                string[] items = battleStageList.list[commonM.saveM.saveData.playerInfo.id_reconnaissanceArea].getItemId_reconnaissance.Split(','); // 獲得アイテム候補
                int num_get = UnityEngine.Random.Range(1, (int)commonM.dataM.get_constFloat(19)); // アイテム入手数抽選
                ushort[] id_getItems = new ushort[num_get]; // 獲得アイテムを格納する配列
                // 獲得アイテムの抽選
                Image img_thumbnail; // 操作するアイテムサムネイル
                GameObject obj_teisatsuEnd_itemThumbnail = rect_teisatsuEnd_itemsParent.GetChild(0).gameObject; // コピーするアイテムサムネイルObj
                for (int i = 0; i < num_get; i++)
                {
                    int num_element = UnityEngine.Random.Range(0, items.Length); // 要素番号
                    id_getItems[i] = ushort.Parse(items[num_element]);
                    commonM.saveM.saveData.playerInfo.set_havingItem(id_getItems[i], 1); // アイテム追加処理

                    if (i == 0)
                    {
                        img_thumbnail = rect_teisatsuEnd_itemsParent.GetChild(0).GetChild(0).GetChild(1).GetComponent<Image>();
                    }
                    else
                    {
                        // アイテムサムネイルのコピー
                        img_thumbnail = Instantiate(obj_teisatsuEnd_itemThumbnail, rect_teisatsuEnd_itemsParent).GetComponent<RectTransform>().GetChild(0).GetChild(1).GetComponent<Image>();
                    }
                    img_thumbnail.sprite = commonM.dataM.imgList_items.list[id_getItems[i]]; // アイテムアイコンを反映させる
                }
                commonM.audioM.SE_Play(56); // アイテム獲得SE
            }
        }

        // 相手の情報コメントtextについて・ここでは「RGB」基準で情報を伝える
        // 敵の小紋の地色(RGB)
        def_komon enemyKomon = enemyKomonList.list[commonM.saveM.saveData.playerInfo.id_reconnaissanceArea];
        Color col = enemyKomon.get_colorNum_toRgb(def_komon.creatonTiming.ground);
        float colSum = col.r + col.g + col.b;
        float[] colRate = new float[3] { (col.r / colSum), (col.g / colSum), (col.b / colSum) };
        float temp = 0f; // 色比率比較用
        byte topRateCol = 0; // 比率の高い色・R=0,G=1,B=2
        for (byte i = 0; i < colRate.Length; i++)
        {
            if (temp < colRate[i])
            {
                temp = colRate[i];
                topRateCol = i;
            }
        }

        string comment = commonM.dataM.get_constString(35); // 相手の小紋は
        int rand = UnityEngine.Random.Range(0, 5); // コメント抽選用
        comment += $" {commonM.dataM.get_constString(36).Split(',')[rand]}"; // たしか

        // 間違ったことを言うかの抽選
        int missRate = (int)commonM.dataM.get_constFloat(20); // 間違った情報伝える確率
        if (UnityEngine.Random.Range(0, 100) < missRate)
        {
            // ミス
            rand = UnityEngine.Random.Range(0, 3);
            switch (topRateCol)
            {
                case 0:
                    // 赤系・それ系以外のコメントをランダムで表示
                    comment += $" {getHintColorString((byte)(rand + 1))}";
                    break;

                case 3:
                    // モノクロ系
                    // 抽選結果は0~2なので、モノクロ系の場合は結果をそのまま使用
                    comment += $" {getHintColorString((byte)rand)}";
                    break;

                default:
                    // 緑か青系
                    // 抽選結果の値が本当の色の値と異なっていれば、そのまま使用
                    if (rand != topRateCol)
                    {
                        comment += $" {getHintColorString((byte)rand)}";
                    }
                    else
                    {
                        comment += $" {getHintColorString(3)}"; // モノクロ系
                    }
                    break;
            }
        }
        else
        {
            // 正しいコメント
            comment += $" {getHintColorString(topRateCol)}"; // 色
        }
        rand = UnityEngine.Random.Range(0, 5);
        comment += $" {commonM.dataM.get_constString(37).Split(',')[rand]}"; // っぽい
        comment += commonM.dataM.get_constString(38); // サメ！（語尾）
        text_teisatsuEnd_partnerComment.SetTextAndExpandRuby(comment);
    }
    /// <summary>
    /// 指定した色に類似する色のStringをランダムで取得する（ex.赤(0)→赤 or オレンジ or ピンク or むらさき）
    /// </summary>
    /// <param name="colorGenre">RGBのどれ系の色か・赤[0],緑[1],青[2],白黒グレー[3]</param>
    /// <returns>指定した色に類似する色のStringをランダムで返す</returns>
    private string getHintColorString(byte colorGenre)
    {
        string comment = "";
        string[] strs = commonM.dataM.get_constString(41).Split(',');
        int rate = UnityEngine.Random.Range(0, 100);
        rate /= 25;
        // 正しめな情報
        switch (colorGenre)
        {
            case 0:
                // 赤強め
                //strs = commonM.dataM.get_constString(41).Split(',');
                break;

            case 1:
                // 緑強め
                strs = commonM.dataM.get_constString(42).Split(',');
                break;

            case 2:
                // 青強め
                strs = commonM.dataM.get_constString(43).Split(',');
                break;

            case 3:
                //黒・グレー・白
                strs = commonM.dataM.get_constString(44).Split(',');
                break;
        }
        comment = strs[rate];
        return comment;
    }

    /// <summary>
    /// モード切替ボタン押された時の処理・実行するたびに「ていさつ←→バトル」を切り替える
    /// </summary>
    public void button_modeChange()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        modeIsTeisatsu = !modeIsTeisatsu; // モード切替（反転）

        // 表示物切替
        obj_detailByMode[Convert.ToInt32(modeIsTeisatsu)].SetActive(true);
        obj_detailByMode[Convert.ToInt32(!modeIsTeisatsu)].SetActive(false);

        text_button_modeChange.SetTextAndExpandRuby(commonM.dataM.get_constString(27 + Convert.ToInt32(!modeIsTeisatsu)) + commonM.dataM.get_constString(29)); // モード切替ボタン書き換え
        text_nowMode.SetTextAndExpandRuby(commonM.dataM.get_constString(27 + Convert.ToInt32(modeIsTeisatsu)));
    }
 
}


