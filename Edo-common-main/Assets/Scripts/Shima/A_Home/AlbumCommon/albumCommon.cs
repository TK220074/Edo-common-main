using TMPro;
using UnityEngine;
using System.Collections;


/// <summary>
/// 各種アルバムの共通部分
/// </summary>
public class albumCommon : MonoBehaviour
{
    /// <summary>
    /// アイテムを選択する用途としてアルバムを使用するか
    /// </summary>
    [SerializeField]protected bool modeIsSelecting;
    /// <summary>
    /// 売却/破棄ボタン[0]、アイテム選択ボタン[1]・選択モードの時の非表示用
    /// </summary>
    [SerializeField] protected GameObject[] detailButtons;
    /// <summary>
    /// アイテム選択モードで開いているときに表示するGameObjectたち
    /// </summary>
    [SerializeField] protected GameObject[] selectingButtons;

    /// <summary>
    /// 各アルバムで共通して使われる情報群
    /// </summary>
    protected album_list albumListInfo;

    /// <summary>
    /// // 売却[0]/破棄[1]の可否
    /// </summary>
    protected bool[] buttonsReaction = new bool[2];
    /// <summary>
    /// 詳細表示中のアイテムのID
    /// </summary>
    public ushort selectingId { get; protected set; }
    /// <summary>
    /// 売却額
    /// </summary>
    protected uint sellingPrice;

    /// <summary>
    /// サムネイル画像の拡大率・柄がかかわるサムネイルは拡大しないと見にくい
    /// </summary>
    public Vector3 vec_imgScale_komonPattern { get; } = new Vector3(3.5f, 3.5f, 3.5f);

    /// <summary>
    /// 詳細表示画面Obj・表示非表示用
    /// </summary>
    [SerializeField] protected GameObject detailGroup;

    [SerializeField] protected TextMeshProUGUI itemNameText;

    /// <summary>
    /// 各種Text
    /// </summary>
    [SerializeField] protected TextMeshProUGUI[] explainTexts;

    /// <summary>
    /// （倉庫のみ）選択中のアイテム種別
    /// </summary>
    [SerializeField] protected shopKind selectingStorage = shopKind.Cloth;

    protected Manager_CommonGroup commonM;
    protected def_player player;

    virtual protected void OnEnable()
    {
        commonM = Manager_CommonGroup.instance;
        player = commonM.saveM.saveData.playerInfo;
        albumListInfo = album_list.instance;
        itemNameText.text = commonM.dataM.get_constString(3);
    }

    virtual protected void Start()
    {
        // 選択モード時は「これにする」を表示、アルバムモード時は「売る」などを表示
        bool[] activeMode = new bool[2] {true, false}; // 「売る」などの表示[0], 「これにする」の表示[1]
        if(modeIsSelecting)
        {
            activeMode[0] = false;
            activeMode[1] = true;
        }

        foreach (GameObject obj in detailButtons)
        {
            obj.SetActive(activeMode[0]);
        }
        foreach (GameObject obj in selectingButtons)
        {
            obj.SetActive(activeMode[1]);
        }
    }

    /// <summary>
    /// 売却、破棄可否の判定（型紙・倉庫）
    /// </summary>
    /// <param name="data">参照したいアイテムデータ</param>
    /// <returns>売却[0]・破棄[1]それぞれ可能ならtrue</returns>
    protected bool[] checkSellable(ItemsEntity data)
    {
        bool[] result = new bool[2];

        // だいじなものでないか
        if(!data.important || (data.shopKind != shopKind.Important))
        {
            // 売却について
            if(data.price != 0)
            {
                // 売れるもの
                int sel = (int)Mathf.Floor(data.price / 2);
                if(sel > 0)
                {
                    // 売却も破棄もできるフラグ
                    result[0] = true;
                    result[1] = true;
                }
            }
            else
            {
                // 売却はできないが破棄はできる
                result[0] = false;
                result[1] = true;
            }
        }
        else
        {
            // 売却も破棄もできないフラグ
            result[0] = false;
            result[1] = false;
        }
        // だいじなもの・売却できない
        return result;
    }
    /// <summary>
    /// 売却、破棄可否の判定（小紋）
    /// </summary>
    /// <param name="data">参照したいアイテムデータ</param>
    /// <returns>売却[0]・破棄[1]それぞれ可能ならtrue</returns>
    protected bool[] checkSellable(def_komon data)
    {
        bool[] result = new bool[2];

        // だいじなものでないか
        if(!data.get_isImportant())
        {
            // 売却も破棄もできるフラグ
            result[0] = true;
            result[1] = true;
        }
        else
        {
            // 売却も破棄もできないフラグ
            result[0] = false;
            result[1] = false;
        }
        // だいじなもの・売却できない
        return result;
    }
    /// <summary>
    /// 売却、破棄可否の判定（色レシピ）
    /// </summary>
    /// <param name="data">参照したい色レシピデータ</param>
    /// <returns>売却[0]・破棄[1]それぞれ可能ならtrue</returns>
    protected bool[] checkSellable(def_colorRecipe data)
    {
        bool[] result = new bool[2];

        // だいじなものでないか
        if (!data.get_isImportant())
        {
            // 売却も破棄もできるフラグ
            result[0] = true;
            result[1] = true;
        }
        else
        {
            // 売却も破棄もできないフラグ
            result[0] = false;
            result[1] = false;
        }
        // だいじなもの・売却できない
        return result;
    }

    /// <summary>
    /// サムネイルリストを生成する
    /// </summary>
    /// <param name="itemKind">表示するアイテム種</param>
    protected void setupList(def_player.albumKind albumKind)
    {
        if (albumListInfo.listRect.childCount > 0)
        {
            // リストに何かある＝初期のリロードではない→リストをいったんリセット
            foreach (RectTransform child in albumListInfo.listRect)
            {
                // 全ての子ObjをDestroy
                Destroy(child.gameObject);
            }
        }

        short wasSetNum = 0; // 所持していたアイテム種の数
        int len = 0; // 検索する配列の要素数
        short max = player.get_albumSize(albumKind); // アルバムの最大保存数
        
        switch(albumKind)
        {
            case def_player.albumKind.pattern:
                // 型紙アルバム
                len = commonM.dataM.list_pattern.list.Count;
                for(int i = 0; i < len; i++)
                {
                    // そのアイテムを所持していたらボタン生成
                    if(player.get_havingPattern_haveNum((byte)i) >= 1)
                    {
                        // 生成ついでにIDも付与しておく
                        Instantiate(albumListInfo.itemButtonObj, albumListInfo.listRect).GetComponent<itemButton>().initialize((ushort)i);
                        wasSetNum++;
                    }
                }
                break;

            case def_player.albumKind.komon:
                // 小紋アルバム
                len = player.get_havingKomon_len();
                for(int i = 0; i < len; i++)
                {
                    // ボタン生成
                    // 生成ついでにIDも付与しておく
                    Instantiate(albumListInfo.itemButtonObj, albumListInfo.listRect).GetComponent<itemButton>().initialize((ushort)i);
                    wasSetNum++;
                }
                break;

            case def_player.albumKind.color:
                // 色レシピ
                len = player.get_havingColor_len();
                for (int i = 0; i < len; i++)
                {
                    // ボタン生成
                    // 生成ついでにIDも付与しておく
                    Instantiate(albumListInfo.itemButtonObj, albumListInfo.listRect).GetComponent<itemButton>().initialize((ushort)i);
                    wasSetNum++;
                }
                break;

            default:
                // 倉庫
                len = commonM.dataM.list_items.list.Count;
                for(int i = 0; i < len; i++)
                {
                    // i番目のアイテムは、選択している種別に該当しているか
                    if (commonM.dataM.list_items.list[i].shopKind == selectingStorage)
                    {
                        if(player.get_havingItem((ushort)i) >= 1)
                        {
                            // アイテムを1つ以上持っていたら、そのボタン生成
                            Instantiate(albumListInfo.itemButtonObj, albumListInfo.listRect).GetComponent<itemButton>().initialize((ushort)i);
                            wasSetNum++;
                        }
                    }
                }
                break;
        }

        // 空いているマスがあることを示す点線生成
        short left = (short)(max - wasSetNum);
        if(left > 0)
        {
            for(int i = 0; i < (int)left; i++)
            {
                Instantiate(albumListInfo.itemNoneObj, albumListInfo.listRect);
            }
        }
        // アルバムの保存数
        albumListInfo.albumSizeNumText[0].text = wasSetNum.ToString("00"); // 現保存数
        albumListInfo.albumSizeNumText[1].text = $"/ {max.ToString("00")}"; // 最大保存数
        float rate = (float)wasSetNum / (float)max * 100f;
        albumListInfo.albumSizeNumText[2].text = $"{rate.ToString("0.0")}%"; // 保存率
    }


    /// <summary>
    /// アイテム 売却時の値段を算出・売価の1/2
    /// </summary>
    /// <param name="data">売却額を調べたいアイテム</param>
    /// <returns>売却額</returns>
    protected uint checkSellingPrice(ItemsEntity data)
    {
        return (uint)Mathf.Floor(data.price / 2.0f);
    }
    /// <summary>
    /// 小紋 売却時の値段を算出・長さ×職人ランク×定数×出来栄えポイント
    /// </summary>
    /// <param name="data">売却額を調べたい小紋データ</param>
    /// <returns>売却額</returns>
    protected uint checkSellingPrice(def_komon data)
    {
        uint rankNum = (uint)player.get_playerRank(def_komon.creationPart.summary);
        uint result = (uint)Mathf.Floor((float)data.get_length() * rankNum * commonM.dataM.get_constFloat(11) * data.get_komonPoint());
        return result;
    }
    /// <summary>
    /// 色レシピ の売却額を算出する・職人ランクと色糊作りレベルで上下
    /// </summary>
    /// <returns>レシピの売却額</returns>
    protected uint checkSellingPrice()
    {
        uint rank_summary = player.get_playerRank(def_komon.creationPart.summary);
        uint rank_colCre = player.get_playerRank(def_komon.creationPart.colorCreation);
        uint result = (uint)Mathf.Floor((rank_summary + rank_colCre) * commonM.dataM.get_constFloat(11));
        return result;
    }


    /// <summary>
    /// 売却額表示を反映させる
    /// </summary>
    /// <param name="canSelling">そのアイテムは売却可能か</param>
    /// <param name="isNotImportant">そのアイテムは「だいじなもの ではない」か</param>
    /// <param name="price">売却額</param>
    /// <param name="priceText">売却額を表示するText</param>
    protected void setSellingPriceText(bool canSelling, bool isNotImportant, uint price, TextMeshProUGUI priceText)
    {
        sellingPrice = price;

        if (canSelling && isNotImportant)
        {
            // 売却額
            priceText.text = commonM.get_priceString((int)sellingPrice);
        }
        else
        {
            priceText.text = commonM.get_priceString(0, true);
            
        }
    }

    /// <summary>
    /// アルバム詳細画面を書き換えた後の共通処理・詳細画面が隠れていたら表示する
    /// </summary>
    /// <param name="openedObj"></param>
    /// <param name="doOpen">詳細画面を表示するタイミングならtrue</param>
    protected void openDetailTop(GameObject openedObj, bool doOpen)
    {
        if(openedObj.activeInHierarchy != doOpen)
        {
            openedObj.SetActive(doOpen);
        }
    }

    /// <summary>
    /// 売却ボタン押下時の処理・破棄→所持金加算
    /// </summary>
    public void button_selling()
    {
        // 売却可能か
        if (buttonsReaction[0])
        {
            commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        }
        else
        {
            commonM.audioM.SE_Play(AudioManager.WhichSE.Cancel);
        }
        StartCoroutine(waitWindow_selling(buttonsReaction[0]));
    }
    /// <summary>
    /// 売却確認ウィンドウ
    /// </summary>
    /// <param name="canDo">売却可能か</param>
    /// <returns></returns>
    IEnumerator waitWindow_selling(bool canDo)
    {
        window_question que = Instantiate(commonM.dataM.window_question, transform.parent).GetComponent<window_question>();
        if (canDo)
        {
            string[] strs = commonM.dataM.get_constString(12).Split(',');
            que.setQuestion($"{strs[0]} {commonM.get_priceString((int)sellingPrice)} {strs[1]}\n{commonM.dataM.get_constString(14)}");
        }
        else
        {
            que.setQuestion(commonM.dataM.get_constString(15), "わかった", "オッケー");
            yield break;
        }

        while (true)
        {
            // 答えが入力されたか
            if (que.endInput)
            {
                break;
            }
            yield return null;
        }

        // 選択内容の検証
        if (System.Convert.ToBoolean(commonM.tempValue[0]))
        {
            commonM.audioM.SE_Play(2); // チャリーン
            itemDelete(selectingId);
            player.set_money((int)sellingPrice);
            home_banner.instance.reloadBannerInfo(); // バナーを更新（所持金）
        }
    }

    /// <summary>
    /// 破棄ボタン押下時の処理
    /// </summary>
    public void button_delete()
    {
        // 破棄可能か
        if (buttonsReaction[1])
        {
            commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        }
        else
        {
            commonM.audioM.SE_Play(AudioManager.WhichSE.Cancel);
        }
        StartCoroutine(waitWindow_delete(buttonsReaction[1]));
    }
    /// <summary>
    /// 破棄確認ウィンドウ
    /// </summary>
    /// <param name="canDo">破棄可能か</param>
    /// <returns></returns>
    IEnumerator waitWindow_delete(bool canDo)
    {
        window_question que = Instantiate(commonM.dataM.window_question, transform.parent).GetComponent<window_question>();
        if (canDo)
        {
            que.setQuestion($"{commonM.dataM.get_constString(13)}\n{commonM.dataM.get_constString(14)}");
        }
        else
        {
            que.setQuestion(commonM.dataM.get_constString(16), "わかった", "オッケー");
            yield break;
        }
        
        while (true)
        {
            // 答えが入力されたか
            if (que.endInput)
            {
                break;
            }
            yield return null;
        }

        // 選択内容の検証
        if (System.Convert.ToBoolean(commonM.tempValue[0]))
        {
            commonM.audioM.SE_Play(3); // ゴソッ
            itemDelete(selectingId);
        }
    }

    /// <summary>
    /// 選択したアイテムを破棄する
    /// </summary>
    private void itemDelete(uint id)
    {
        switch (albumListInfo.albumKind)
        {
            case def_player.albumKind.pattern:
                // 破棄 - 型紙
                player.set_havingPattern_delete((byte)id);
                if (player.get_havingPattern_haveNum((byte)id) <= 0)
                {
                    // 破棄後、型紙が残っていなければ、詳細画面を閉じる
                    openDetailTop(detailGroup, false);
                    itemNameText.text = commonM.dataM.get_constString(3);
                }
                else
                {
                    // 破棄後も型紙が残っていたら、詳細画面を更新する
                    manager_album_pattern.instance.reloadDetail((ushort)id);
                }
                break;

            case def_player.albumKind.komon:
                // 破棄 - 小紋
                player.set_havingKomon_delete(id);
                openDetailTop(detailGroup, false);
                itemNameText.text = commonM.dataM.get_constString(3);
                break;

            case def_player.albumKind.color:
                // 破棄 - 色レシピ
                player.set_havingColor_delete(id);
                openDetailTop(detailGroup, false);
                itemNameText.text = commonM.dataM.get_constString(3);
                break;

            case def_player.albumKind.storage:
                // 破棄 - 倉庫（アイテム）

                player.set_havingItem((ushort)id, -1);
                if (player.get_havingItem((ushort)id) <= 0)
                {
                    // 破棄後、そのアイテムが残っていなければ、詳細画面を閉じる
                    openDetailTop(detailGroup, false);
                    itemNameText.text = commonM.dataM.get_constString(3);
                }
                else
                {
                    // 破棄後もそのアイテムが残っていたら、詳細画面を更新する
                    manager_album_storage.instance.reloadDetail((ushort)id);
                }
                break;
        }
        setupList(albumListInfo.albumKind); // リストのリロード
    }

    /// <summary>
    /// 「こうかん」ボタン押下時の処理
    /// </summary>
    public void button_trade()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        switch (albumListInfo.albumKind)
        {
            case def_player.albumKind.pattern:
                break;

            case def_player.albumKind.komon:
                break;

            case def_player.albumKind.color:

                break;

            case def_player.albumKind.storage:

                break;
        }
    }

    /// <summary>
    /// 「シェア」ボタン押下時の処理
    /// </summary>
    public void button_share()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        switch (albumListInfo.albumKind)
        {
            case def_player.albumKind.pattern:
                break;

            case def_player.albumKind.komon:
                break;

            case def_player.albumKind.color:

                break;

            case def_player.albumKind.storage:

                break;
        }
    }

    /// <summary>
    /// 選択モードにて、詳細画面のアイテム選択ボタンが押下された時の処理
    /// </summary>
    public void button_selectThis()
    {
        //Debug.Log(commonM.tempValue[0]);
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        GameObject targetObject = GameObject.Find("Common_manager");
        Debug.Log(targetObject);
        commonM.tempValue[0] = (short)selectingId;
        switch (albumListInfo.albumKind)
        {
            case def_player.albumKind.pattern:
                commonM.tempValue[0] = (short)selectingId;
                Debug.Log(commonM.tempValue[0]);
                if (targetObject != null)
                {
                    OneforAll_manager oneforAll_manager = targetObject.GetComponent<OneforAll_manager>();
                    oneforAll_manager.komon_patan();
                }
                // 現時点ではプロフィール画面専用の仕組み
                // Profile画面があれば、書き換え
                if (profileContent.instance != null)
                {
                    commonM.saveM.saveData.playerInfo.set_favorite_pattern((byte)selectingId); // 選択された柄を記録
                    profileContent.instance.setupProfile();
                }
                Destroy(transform.parent.parent.gameObject);
                break;

            case def_player.albumKind.komon:
                // 使用する長さを選択する
                manager_album_komon.instance.openLengthSelector();
                break;

            case def_player.albumKind.color:
                commonM.tempValue[1] = (short)selectingId;

                devlog.log($"selected: {selectingId}");

                if (targetObject != null)
                {
                    OneforAll_manager oneforAll_manager= targetObject.GetComponent<OneforAll_manager>();
                    oneforAll_manager.komon_patan();
                }
                // 現時点ではプロフィール画面専用の仕組み
                // Profile画面があれば（＝プロフィール画面での選択だった場合は）、書き換え
                if (profileContent.instance != null)
                {
                    // 選択された色を記録
                    def_colorRecipe info = commonM.saveM.saveData.playerInfo.get_havingColor((byte)selectingId);
                    commonM.saveM.saveData.playerInfo.set_favorite_color(info.get_looksColor(true));
                    commonM.saveM.saveData.playerInfo.set_favorite_color_name(info.get_name());

                    profileContent.instance.setupProfile();
                }
                Destroy(transform.parent.parent.gameObject);
                break;

            case def_player.albumKind.storage:
                commonM.tempValue[0] = (short)selectingId; // 選択したアイテムの番号を一時保存変数に格納

                devlog.log($"selected: {selectingId}");

                if (targetObject != null)
                {
                    OneforAll_manager oneforAll_manager = targetObject.GetComponent<OneforAll_manager>();
                    oneforAll_manager.komon_patan();
                }

                Destroy(transform.parent.parent.gameObject);
                break;
        }
    }
}
