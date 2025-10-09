using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// お店のいろいろ
/// </summary>
public class manager_shopItemList : Singleton<manager_shopItemList>
{
    [SerializeField] private GameObject obj_itemButton; // リストに並べるボタン
    [SerializeField] private RectTransform itemButtonRect;
    [SerializeField] private RectTransform rect_questionWindowParent;

    /// <summary>
    /// 閲覧中の店種
    /// </summary>
    public shopKind nowShopKind { get; private set; }

    /// <summary>
    /// サムネイル画像の拡大率・柄がかかわるサムネイルは拡大しないと見にくい
    /// </summary>
    public Vector3 vec_imgScale_komonPattern { get; } = new Vector3(3.5f, 3.5f, 3.5f);

    [SerializeField, Header("ItemDetail")] private GameObject obj_detailContent;
    [SerializeField] private TextMeshProUGUI text_itemName;
    [SerializeField] private TextMeshProUGUI text_itemExplain;
    [SerializeField] private TextMeshProUGUI text_price;
    [SerializeField] private TextMeshProUGUI text_haveNum;
    [SerializeField] private Button button_selectThis;

    [SerializeField, Header("Detail Thumbnail")] private Image img_itemIcon;
    [SerializeField] private RectTransform rect_itemIcon;
    [SerializeField] private Image img_itemIconBG;

    private int selectingId; // 選択中のアイテム番号
    private int nowPrice; // 選択中アイテムの値段

    private Manager_CommonGroup commonM;
    private def_player player;

    private list_items list_item;
    private SpriteList list_itemIcon;

    private bool endStartProcess;
    
    // Start is called before the first frame update
    void Start()
    {
        commonM = Manager_CommonGroup.instance;
        list_item = commonM.dataM.list_items;
        list_itemIcon = commonM.dataM.imgList_items;
        player = commonM.saveM.saveData.playerInfo;
        obj_detailContent.SetActive(false);
        endStartProcess = true;
    }

    /// <summary>
    /// 表示する店種を変更する
    /// </summary>
    /// <param name="shopKind">変更先の店種</param>
    public void changeShop(shopKind shopKind)
    {
        if (!endStartProcess) { Start(); } // Startが実行される前に店変更が実行されてしまった場合に備える
        nowShopKind = shopKind;
        obj_detailContent.SetActive(false);
        text_itemName.SetTextAndExpandRuby(commonM.dataM.get_constString(3));
        listSetup(shopKind);
    }

    /// <summary>
    /// 店頭にアイテムを並べる
    /// </summary>
    /// <param name="shopKind">閲覧する店舗の種類</param>
    private void listSetup(shopKind shopKind)
    {
        // 店頭販売の対象でないものが入力されたらはじく
        switch (shopKind)
        {
            case shopKind.Important:
                return;
            case shopKind.Komon:
                return;
            case shopKind.BattleItem:
                return;
            case shopKind.Other:
                return;

            default:
                break;
        }
        
        // リストに何かある＝初期のリロードではないか
        if (itemButtonRect.childCount > 0)
        {
            // リストをリセット
            foreach (RectTransform child in itemButtonRect)
            {
                // 全ての子ObjをDestroy
                Destroy(child.gameObject);
            }
        }

        // 要素数を取得
        int listNum = 0;
        switch (nowShopKind)
        {
            case shopKind.Pattern:
                listNum = commonM.dataM.list_pattern.list.Count;
                break;

            default:
                listNum = list_item.list.Count;
                break;
        }

        // アイテムデータとして登録されている分だけ検証
        for (int i = 0; i < listNum; i++)
        {
            ItemsEntity item; // 検証するアイテムデータ

            switch (nowShopKind)
            {
                case shopKind.Pattern:
                    item = commonM.dataM.list_pattern.list[i];
                    break;

                default:
                    item = list_item.list[i];
                    break;
            }
            
            // セット対象の店舗で扱うアイテムか確認
            // 0円のものは店頭に並べない
            if ((item.shopKind == shopKind) && (item.price != 0))
            {
                // 条件のエリアクリア判定
                // 条件のステージIDを取得
                int[] ids = Array.ConvertAll(item.clearStageId.Split(','), s => int.TryParse(s, out var x) ? x : -1);

                bool canSale = false; // 店頭に並べることができるか
                // 無条件解放 = -1 を指定しているか
                if (Array.IndexOf(ids, -1) >= 0)
                {
                    canSale = true; // 販売可能フラグ
                }
                else
                {
                    // クリア条件の数だけ検証
                    bool[] clear = new bool[ids.Length];
                    for(int j = 0; j < ids.Length; j++)
                    {
                        // そのエリアをクリアしていれば販売OK
                        if ((ids[j] == -1) || player.get_battle_areaClear((ushort)j))
                        {
                            clear[j] = true;
                        }
                    }
                    // 未クリアエリアが無ければ販売可能
                    if(Array.IndexOf(clear, false) == -1)
                    {
                        canSale = true;
                    }
                }

                // ここまでの条件検証では「販売可能」と判断できるアイテムか？
                if (canSale)
                {
                    // 職人ランクが、商品登場のランク基準以上か
                    if (player.get_playerRank(def_komon.creationPart.summary) >= item.needLv)
                    {
                        // ここで商品表示
                        Instantiate(obj_itemButton, itemButtonRect).GetComponent<itemButton_inShop>().buttonSetup(i, checkSoldout(i));
                    }
                }
            }
        }
    }

    /// <summary>
    /// 選択したアイテムの詳細情報を表示する
    /// </summary>
    /// <param name="itemId">表示するアイテムの番号</param>
    /// <param name="isSoldout">そのアイテムは売り切れているか</param>
    public void setDetail(int itemId, bool isSoldout)
    {
        selectingId = itemId;
        ItemsEntity item; // 参照しているアイテムの情報
        byte haveNum = 0; // そのアイテムの所持数

        switch (nowShopKind)
        {
            case shopKind.Pattern:
                item = commonM.dataM.list_pattern.list[itemId];
                haveNum = (byte)player.get_havingPattern_haveNum((byte)itemId);
                img_itemIcon.sprite = commonM.dataM.imgList_pattern.list[itemId];
                rect_itemIcon.localScale = vec_imgScale_komonPattern;
                img_itemIconBG.color = new Color(UnityEngine.Random.Range(0f, commonM.dataM.get_constFloat(10)), UnityEngine.Random.Range(0f, commonM.dataM.get_constFloat(10)), UnityEngine.Random.Range(0f, commonM.dataM.get_constFloat(10)));
                
                string[] strs = item.explain.Split((','));
                text_itemExplain.SetTextAndExpandRuby($"{strs[0]}\n\n{strs[1]}");

                break;

            default:
                item = list_item.list[itemId];
                haveNum = player.get_havingItem((ushort)itemId);
                img_itemIcon.sprite = list_itemIcon.list[itemId]; // アイテムアイコン
                rect_itemIcon.localScale = vec_imgScale_komonPattern / 3.5f;
                img_itemIconBG.color = Color.white; // 真っ白
                text_itemExplain.SetTextAndExpandRuby(item.explain);
                break;
        }

        text_itemName.SetTextAndExpandRuby(item.name); // アイテム名
        nowPrice = (int)item.price;
        text_price.text = Manager_CommonGroup.instance.get_priceString(nowPrice); // 値段表示
        text_haveNum.text = $"{haveNum}コ もってる"; // 所持数

        button_selectThis.interactable = !isSoldout; // 購入ボタンの表示/非表示

        if(obj_detailContent.activeInHierarchy == false)
        {
            obj_detailContent.SetActive(true);
        }
    }

    /// <summary>
    /// 購入ボタン押したときの処理
    /// </summary>
    public void pressed_selectThis()
    {
        // 所持金足りるか？
        bool purchase = false;
        if (player.get_money() >= nowPrice)
        {
            // 足りる
            commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
            purchase = true;
        }
        else
        {
            // 足らない
            commonM.audioM.SE_Play(AudioManager.WhichSE.Cancel);
        }
        StartCoroutine(waitWindow_purchase(purchase));
    }
    IEnumerator waitWindow_purchase(bool canPurchase)
    {
        bool okhaveNum = false; // 所持数大丈夫か
        window_question que = Instantiate(commonM.dataM.window_question, rect_questionWindowParent).GetComponent<window_question>();

        // 所持数大丈夫か
        int haveNum = 0;
        switch (nowShopKind)
        {
            case shopKind.Pattern:
                haveNum = player.get_havingPattern_haveNum((byte)selectingId);
                break;

            default:
                haveNum = player.get_havingItem((ushort)selectingId);
                break;
        }

        if(haveNum < commonM.dataM.get_constFloat(3))
        {
            // 所持数大丈夫
            okhaveNum = true;

            if (canPurchase)
            {
                string price = commonM.get_priceString(nowPrice);
                que.setQuestion($"{price} {commonM.dataM.get_constString(17)}", "買う！");
            }
            else
            {
                que.setQuestion(commonM.dataM.get_constString(18), "またきます！", "オッケー");
            }
        }
        else
        {
            // 所持数いっぱい
            que.setQuestion(commonM.dataM.get_constString(19), "またきます！", "オッケー");
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
        
        // お金足りる？・所持数大丈夫？・購入を選択した？
        if(canPurchase && okhaveNum && System.Convert.ToBoolean(commonM.tempValue[0]))
        {
            // 購入処理
            switch (nowShopKind)
            {
                case shopKind.Pattern:
                    player.set_havingPattern_new((byte)selectingId, 1); // 小紋の（耐久値の）追加処理
                    break;

                default:
                    player.set_havingItem((ushort)selectingId, 1); // 1つ追加
                    break;
            }
            player.set_money(-nowPrice);

            commonM.audioM.SE_Play(2); // チャリーン

            listSetup(nowShopKind);

            setDetail(selectingId, checkSoldout(selectingId));

            home_banner.instance.reloadBannerInfo();
        }
    }

    /// <summary>
    /// そのアイテムが売り切れかどうか確認する
    /// </summary>
    /// <param name="itemId">確認したいアイテムの番号</param>
    /// <returns>売り切れならtrue</returns>
    private bool checkSoldout(int itemId)
    {
        bool isSoldout = false; // 売り切れていないか＝購入可能なアイテムか
        if (list_item.list[itemId].buyOnce)
        {
            // ここで、以前購入しているかを確認
            // やりかた1：buyOnceフラグがあるものは大体大事なもの→破棄できないから、購入していれば必ず1つはあると踏んだ
            if (player.get_havingItem((ushort)itemId) > 0)
            {
                // 購入していたら売り切れ表示フラグ
                isSoldout = true;
            }
        }
        return isSoldout;
    }
}