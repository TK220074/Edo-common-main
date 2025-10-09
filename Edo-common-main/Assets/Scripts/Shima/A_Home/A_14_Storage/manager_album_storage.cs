using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class manager_album_storage : albumCommon
{
    public static manager_album_storage instance;

    
    [SerializeField] private Button[] tabButtons;
    [SerializeField, Header("DetailBG")] private Image iconImg;

    private Vector2[] tabButtonSize;

    override protected void Start()
    {
        base.Start();
        detailGroup.SetActive(false);
        setupList(albumListInfo.albumKind);
    }

    public void reloadDetail(ushort itemId)
    {
        selectingId = itemId;
        ItemsEntity data = commonM.dataM.list_items.list[itemId];

        iconImg.sprite = commonM.dataM.imgList_items.list[itemId];

        itemNameText.text = data.name;
        explainTexts[0].text = $"×{commonM.saveM.saveData.playerInfo.get_havingItem(itemId)}"; // 所持数
        explainTexts[1].text = data.explain;
        
        bool[] temp = checkSellable(data);
        buttonsReaction[0] = temp[0];
        buttonsReaction[1] = temp[1];
        setSellingPriceText(temp[0], temp[1], checkSellingPrice(data), explainTexts[2]);

        openDetailTop(detailGroup, true);
    }

    /// <summary>
    /// 表示するアイテム種別を変更する
    /// </summary>
    /// <param name="pageKind">変更先種別・pattern/komon(/other)以外</param>
    public void changeStrageGenre(int pageKind)
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);

        // 倉庫で表示すべき種別か 0 2 3 4 5 7
        if((pageKind == 0) ||((pageKind >= 2) && (pageKind <= 5)) || (pageKind == 7))
        {
            if (selectingStorage != (shopKind)pageKind)
            {
                devlog.logWarning("//audioM.SE_Play(); // 可能ならここでページ変更SE 紙めくり？ササッ");

                selectingStorage = (shopKind)pageKind;
                openDetailTop(detailGroup, false);
                setupList(albumListInfo.albumKind);
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
