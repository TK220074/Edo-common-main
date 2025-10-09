using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class manager_workspace : MonoBehaviour
{
    private Manager_CommonGroup commonM;
    [SerializeField,Header("window_cannotCreate")] private GameObject obj_window_notCreate; // 制作条件整っていないことを表示するウィンドウ
    [SerializeField] private TextMeshProUGUI text_windowMessage;

    // Start is called before the first frame update
    void Start()
    {
        commonM = Manager_CommonGroup.instance;
        obj_window_notCreate.SetActive(false);
    }

    public void buttonPressed(bool isKomonCreation)
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);

        byte nextId = 0;
        if (isKomonCreation)
        {
            // 小紋づくりシーン

#if true
            // 制作開始条件を検証
            def_player player = commonM.saveM.saveData.playerInfo;
            bool[] check = new bool[5];

            // 生地・染料があるか
            List<ItemsEntity> list = commonM.dataM.list_items.list;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].shopKind == shopKind.Cloth)
                {
                    if(commonM.saveM.saveData.playerInfo.get_havingItem((ushort)i) > 0)
                    {
                        // 生地が1種類でもあればOK
                        check[0] = true;
                        //break;ここでbreakすると染料のチェックまでいかないので
                    }
                }
                else if(list[i].shopKind == shopKind.Dye)
                {
                    if (commonM.saveM.saveData.playerInfo.get_havingItem((ushort)i) > 0)
                    {
                        // 染料が1種類でもあればOK
                        check[1] = true;
                        break;
                    }
                }
            }

            // 型紙1種以上持ってるか？
            for (int i = 0; i < commonM.dataM.list_pattern.list.Count; i++)
            {
                if (player.get_havingPattern_haveNum((byte)i) > 0)
                {
                    // 型紙1種類以上あればOK
                    check[2] = true;
                    //break;
                }
            }

            // 小紋アルバムに空きがあるか
            if (player.get_havingKomon_len() < player.get_albumSize(def_player.albumKind.komon))
            {
                check[3] = true;
            }

            // 色レシピに空きがあるか
            if (player.get_havingColor_len() < player.get_albumSize(def_player.albumKind.color))
            {
                check[4] = true;
            }

            // 新品の生地が無い場合
            if (!check[0])
            {
                // 上塗りできる小紋を探す
                for (int i = 0; i < player.get_havingKomon_len(); i++)
                {
                    if (player.havingKomon[i].get_coatLimit() > 0)
                    {
                        // 上塗りできる小紋があれば、生地はあるものとしてチェック
                        check[0] = true;
                        break;
                    }
                }
            }

            // ここまでの条件判定を検証
            text_windowMessage.text = "";
            bool end = false; // 終了フラグ
            for (byte i = 0; i < check.Length; i++)
            {
                // 整ってない条件はあるか？
                if (!check[i])
                {
                    // 条件に対応するメッセージを挿入
                    text_windowMessage.text += $"{commonM.dataM.get_constString(i + 20)}　{commonM.dataM.get_constString(25)}\n\n";
                    end = true;
                }
            }
            if (end)
            {
                // 開始条件整ってないウィンドウ出して終了
                obj_window_notCreate.SetActive(true);
                devlog.log("小紋づくり できません！");
                return;
            }

            // 全てtrue = 条件整ってるなら、シーン遷移
#endif
            nextId = (byte)commonM.dataM.get_nextSceneId(commonM.sceneChanger.get_nowSceneId(), 0);
        }
        else
        {
            // アイテムづくりシーン
            nextId = (byte)commonM.dataM.get_nextSceneId(commonM.sceneChanger.get_nowSceneId(), 1);
        }

        commonM.sceneChanger.SceneChange(2);
    }

    /// <summary>
    /// 条件整ってないウィンドウでの了承ボタン押下時の処理
    /// </summary>
    public void button_window_ok()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        obj_window_notCreate.SetActive(false);
    }
}