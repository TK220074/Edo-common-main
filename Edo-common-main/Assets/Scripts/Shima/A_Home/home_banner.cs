using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class home_banner : Singleton<home_banner>
{
    [SerializeField]private byte homeTop_sceneId = 2; // A_Homeのシーン番号

    [SerializeField, Header("UI Parts")]private TextMeshProUGUI text_topicPath;
    [SerializeField]private TextMeshProUGUI text_money;
    [SerializeField]private GameObject[] buttons;
    [SerializeField] private Canvas canvas;
#if false
    [SerializeField, Header("ShortCutMenu")] private GameObject obj_shortCutMenu;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField]private GameObject obj_album_pattern;
    [SerializeField] private GameObject obj_album_komon;
    [SerializeField] private GameObject obj_album_colorRecipe;
    [SerializeField] private GameObject obj_album_storage;
    [SerializeField] private GameObject obj_settings;
#endif

    private Manager_CommonGroup commonM;

    /// <summary>
    /// 実行時に開いているシーンの番号をもとに、バナーの情報を更新する
    /// </summary>
    public void reloadBannerInfo()
    {
        int sceneId = commonM.sceneChanger.get_nowSceneId();
        text_money.text = commonM.get_priceString(commonM.saveM.saveData.playerInfo.get_money()); // ここにセーブデータから所持金持ってくる
        text_topicPath.text = getTopicPath((byte)sceneId);
#if true
        // ホームシーンだったらバナー（特に所持金情報）を最前面にしたい
        // アイテム売却の参考になる可能性あるため
        if(commonM.sceneChanger.get_nowSceneId() == homeTop_sceneId)
        {
            canvas.sortingOrder = 1;
        }
        else
        {
            // BGより前面だが、質問ウィンドウよりは後ろにしたい意図
            canvas.sortingOrder = -1;
        }
#endif

        bool active = true;
        if(sceneId == homeTop_sceneId) active = false;
        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetActive(active);
        }
    }

    /// <summary>
    /// パンくずリストをつくる
    /// </summary>
    /// <param name="sceneId">欲するパンくずのシーンID</param>
    /// <returns>パンくずリスト文字列/returns>
    private string getTopicPath(byte sceneId)
    {
        string nowPath = commonM.dataM.sceneInfo.list[sceneId].name;

        // 「もどる」押したときの戻り先シーン番号をもとに、sceneIdのシーンの階層を回帰的に求める
        List<byte> idList = new List<byte>(); // 「指定されたシーンまでに経由するシーンの番号」を記録するリスト
        idList.Insert(0, sceneId); // 指定されたシーン
        byte i = sceneId;
        while(true)
        {
            if(i == homeTop_sceneId)
            {
                string result = "";
                // シーンの階層リストをもとに、階層の先頭（＝工房）からパンくずリストを作成
                for(int j = 0; j < idList.Count; j++)
                {
                    result += $"{commonM.dataM.sceneInfo.list[idList[j]].name}";
                    if(j != (idList.Count - 1)) result += " > "; // パンくずリストの末尾でなければ加筆
                }
                return result;
            }
            else
            {
                // 戻り先が最終的なシーン（＝工房）でなければ、その経由シーンIDを記録
                i = commonM.dataM.sceneInfo.list[i].returnId; // 「シーン番号 i の戻り先」のシーン番号
                idList.Insert(0, i);
            }
        }
    }

    /// <summary>
    /// バナーを削除する。タイトルやクレジット、小紋づくりやバトルに入る時などに。
    /// </summary>
    public void destroyHomeBanner()
    {
        Destroy(transform.parent.gameObject);
    }

    public void button_return()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Cancel);
        commonM.sceneChanger.SceneChange(commonM.dataM.sceneInfo.list[(byte)commonM.sceneChanger.get_nowSceneId()].returnId); // sceneInfoで指定したreturnIdのシーンへ移動
    }
    public void button_shortcut()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
#if false
        obj_shortCutMenu.SetActive(true);
#else
        commonM.sceneChanger.SceneChange(2); // ホームに戻す
#endif
    }

#if false
    /// <summary>
    /// ショートカットメニュー内で、シーン遷移を伴うボタンが押された時の処理
    /// </summary>
    /// <param name="sceneId">遷移先のシーン番号</param>
    public void button_shortCut(int sceneId)
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        commonM.sceneChanger.SceneChange((byte)sceneId);
        obj_shortCutMenu.SetActive(false);
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
#endif

    void Start()
    {
        if(isOnlyOne)
        {
            DontDestroyOnLoad(transform.parent.gameObject);

            commonM = Manager_CommonGroup.instance;
#if false
            obj_shortCutMenu.SetActive(false);
#endif

            reloadBannerInfo();
        }
    }
}
