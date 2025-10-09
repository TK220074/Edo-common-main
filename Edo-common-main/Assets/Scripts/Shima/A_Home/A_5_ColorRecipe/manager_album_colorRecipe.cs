using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class manager_album_colorRecipe : albumCommon
{
    public static manager_album_colorRecipe instance;

    [SerializeField, Header("DetailBG")] private Image[] colorImg_Tops; // 色糊と最終的な色

    [SerializeField, Header("DyeItemsInfo")] private Image[] iconImg_dyes; // 投入した染料種
    [SerializeField] private TextMeshProUGUI[] dyeAmountTexts;
    [SerializeField] private Sprite itemNoneImg;

    override protected void Start()
    {
        base.Start();
        detailGroup.SetActive(false);
        setupList(albumListInfo.albumKind);
    }

    public void reloadDetail(ushort recipeId)
    {
        selectingId = recipeId;
        def_colorRecipe data = commonM.saveM.saveData.playerInfo.get_havingColor((byte)recipeId);
        Debug.Log(recipeId);

        // レシピ名
        itemNameText.text = data.get_name();

        // 色糊と最終的な色
        colorImg_Tops[0].color = data.get_looksColor(false);
        colorImg_Tops[1].color = data.get_looksColor(true);

        // 染料の配合
        short[] dyeId = data.get_usedDyeId();
        float[] dyeAmount = data.get_usedDyeAmount();
        // 配合できる染料の数だけ確認する
        for (int i = 0; i < data.get_usedDyeId_len(); i++)
        {
            // 未使用でなければ内容を記載
            if (dyeId[i] != -1)
            {
                // 染料使用
                iconImg_dyes[i].sprite = commonM.dataM.imgList_items.list[dyeId[i]]; // 染料のアイコン
                dyeAmountTexts[i].text = $"【{commonM.dataM.list_items.list[dyeId[i]].name}】\n{dyeAmount[i].ToString("0.0")}"; // 染料名と配合量
            }
            else
            {
                // 染料未使用
                iconImg_dyes[i].sprite = itemNoneImg;
                dyeAmountTexts[i].text = "\n-";
            }
            dyeAmountTexts[i].text += " g";
        }

        // 売却額
        bool[] temp = checkSellable(data);
        buttonsReaction[0] = temp[0];
        buttonsReaction[1] = temp[1];
        setSellingPriceText(temp[0], temp[1], checkSellingPrice(), explainTexts[0]);

        openDetailTop(detailGroup, true);
    }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
}
