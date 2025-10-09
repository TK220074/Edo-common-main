using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class manager_album_pattern : albumCommon
{
    public static manager_album_pattern instance;

    [SerializeField, Header("DetailBG")] private StringList duraComment; // 耐久値のコメントリスト
    [SerializeField]private Image[] patternImg;

    override protected void Start()
    {
        base.Start();
        detailGroup.SetActive(false);
        setupList(albumListInfo.albumKind);
    }

    public void reloadDetail(ushort patternId)
    {
        selectingId = patternId;
        ItemsEntity data = commonM.dataM.list_pattern.list[patternId];

        patternImg[0].color = new Color(Random.Range(0f, commonM.dataM.get_constFloat(10)), Random.Range(0f, commonM.dataM.get_constFloat(10)), Random.Range(0f, commonM.dataM.get_constFloat(10)));
        patternImg[1].sprite = commonM.dataM.imgList_pattern.list[patternId];

        itemNameText.SetTextAndExpandRuby(commonM.dataM.list_pattern.list[patternId].name);
        explainTexts[0].text = $"耐久：{getDuraComment(patternId)}";

        string[] exp = data.explain.Split(',');
        explainTexts[1].SetTextAndExpandRuby(exp[0]); // バトル効果
        explainTexts[2].SetTextAndExpandRuby(exp[1]); // 柄由来
        
        bool[] temp = checkSellable(data);
        buttonsReaction[0] = temp[0];
        buttonsReaction[1] = temp[1];
        setSellingPriceText(temp[0], temp[1], checkSellingPrice(data), explainTexts[3]);

        explainTexts[4].text = $"×{commonM.saveM.saveData.playerInfo.get_havingPattern_haveNum((byte)patternId).ToString("00")}"; // 所持数

        openDetailTop(detailGroup, true);
    }

    /// <summary>
    /// 耐久値のコメントを取得
    /// </summary>
    /// <param name="patternId">参照する型紙の番号</param>
    /// <returns>耐久値のコメント</returns>
    private string getDuraComment(ushort patternId)
    {
        // 選択した型紙の中で「（疑似的に）一番古い＝耐久が一番低いもの」の耐久値を算出
        float num = (float)commonM.saveM.saveData.playerInfo.get_havingPattern_duraNum((byte)patternId) % commonM.dataM.list_pattern.list[patternId].durability;
        float rate = num / (float)commonM.dataM.list_pattern.list[patternId].durability; // 残率

        byte index = 0; // 100~70%・バッチリ
        if((rate <= 0.7f) && (rate > 0.4f))
        {
            // 70~40%・まだまだ
            index = 2;
        }
        else if((rate <= 0.4f) && (rate >= 0f))
        {
            // 40~0%・ヤバい
            index = 1;
        }
        return duraComment.list[index];
    }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
}
