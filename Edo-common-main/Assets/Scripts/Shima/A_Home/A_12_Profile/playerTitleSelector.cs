using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class playerTitleSelector : Singleton<playerTitleSelector>
{
    [SerializeField] private GameObject buttonObj;
    [SerializeField] private RectTransform buttonParentrect;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private coverPatternBG coverPattern;

    [SerializeField, Header("TitleList")] private TextMeshProUGUI[] texts_getNum;

    [SerializeField, Header("TitleDetail")] private TextMeshProUGUI text_title;
    [SerializeField] private TextMeshProUGUI text_explain;
    [SerializeField] private TextMeshProUGUI text_hint;
    [SerializeField] private Button button_selectThis;

    public byte selectingId { get; private set; } // 選択中の称号ID

    private AudioManager audioM;
    private DatabaseManager dataM;
    private SaveDataManager saveM;
    
    // Start is called before the first frame update
    void Start()
    {
        Manager_CommonGroup m = Manager_CommonGroup.instance;
        audioM = m.audioM;
        dataM = m.dataM;
        saveM = m.saveM;

        coverPattern.setCoverPatternBG();
        listSetup();
        reloadDetail(saveM.saveData.playerInfo.get_titleId());
    }

    private void listSetup()
    {
        int getNum = 0; // 称号入手数
        int allTitleNum = dataM.list_playerTitle.list.Count; // 称号の数
        for (int i = 0; i < allTitleNum; i++)
        {
            // 生成と名前書き換え
            TextMeshProUGUI buttonTex = Instantiate(buttonObj, buttonParentrect).transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            if (saveM.saveData.playerInfo.get_havingTitle((byte)i))
            {
                buttonTex.SetTextAndExpandRuby(dataM.list_playerTitle.list[i].title);
                getNum++;
            }
            else
            {
                // 未入手であれば「？」表示
                setQuestionMarkString(buttonTex, 10);
            }
        }

        // 称号入手率
        texts_getNum[0].text = getNum.ToString("00");
        texts_getNum[1].text = $"/ {allTitleNum.ToString()}";
        float rate = ((float)getNum / (float)allTitleNum) * 100f;
        texts_getNum[2].text = $"{rate.ToString("0.0")}%";

        scrollbar.value = 1.0f;
    }

    /// <summary>
    /// リスト内のボタンが押された時の処理
    /// </summary>
    /// <param name="pressedId">押されたボタンに該当する称号のID</param>
    public void pressedButton(byte pressedId)
    {
        audioM.SE_Play(AudioManager.WhichSE.Done);
        selectingId = pressedId;
        reloadDetail(pressedId);
    }
    /// <summary>
    /// 「これにする」が押された時の処理
    /// </summary>
    public void pressedSelectThis()
    {
        audioM.SE_Play(AudioManager.WhichSE.Done);
        saveM.saveData.playerInfo.set_titleId(selectingId); // 選択された称号を記録

        // Profile画面があれば、書き換え
        if(profileContent.instance != null)
        {
            profileContent.instance.setupProfile();
        }
        Destroy(this.gameObject);
    }

    /// <summary>
    /// 称号の詳細表示を書き換える
    /// </summary>
    /// <param name="titleId">書き換え内容の称号ID</param>
    private void reloadDetail(byte titleId)
    {
        PlayerTitleEntity info = dataM.list_playerTitle.list[titleId];

        // 称号持っているか
        if (saveM.saveData.playerInfo.get_havingTitle(titleId))
        {
            text_title.SetTextAndExpandRuby(info.title); // 称号名
            text_explain.SetTextAndExpandRuby(info.explain);
            button_selectThis.interactable = true;
        }
        else
        {
            // 未入手であれば「？」表示
            setQuestionMarkString(text_title, 10);
            setQuestionMarkString(text_explain, 70);
            button_selectThis.interactable = false;
        }

        text_hint.SetTextAndExpandRuby(info.hint); // ヒント内容
    }

    /// <summary>
    /// 指定したTextに、指定文字数分「？」を書き込む
    /// </summary>
    /// <param name="text">「？」にしたいText</param>
    /// <param name="insertNum">「？」の文字数</param>
    private void setQuestionMarkString(TextMeshProUGUI text, int insertNum)
    {
        text.text = "";
        for (int i = 0; i < insertNum; i++)
        {
            text.text += "？";
        }
    }
}
