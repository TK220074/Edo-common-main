using System.Collections;
using UnityEngine;
using TMPro;

public class Manager_Title : MonoBehaviour
{
    [SerializeField, Header("Content Info")]private TextMeshProUGUI text_ver; // バージョン表示
    [SerializeField]private TextMeshProUGUI text_copy; // コピーライト表示

    [SerializeField, Header("WindowObj")] private RectTransform canvasRect;

    private Manager_CommonGroup commonM;
    private bool wasPressed; // 連続タップ防止用フラグ

    // Start is called before the first frame update
    void Start()
    {
        commonM = Manager_CommonGroup.instance;

        text_ver.text = $"Ver {Application.version}";
        text_copy.text = $"©{commonM.dataM.get_constString(0)} {Application.companyName}";
    }

    /// <summary>
    /// タイトル画面がタップされたときの処理
    /// </summary>
    public void TitleButtonPressed()
    {
        if (!wasPressed)
        {
            wasPressed = true;
            StartCoroutine(checkNameInput());
        }
    }

    IEnumerator checkNameInput()
    {
        byte nextId = (byte)commonM.dataM.get_nextSceneId(commonM.sceneChanger.get_nowSceneId()); // sceneInfoに記載された「次のシーンのID」を参照

        // 名前が存在しない＝初期起動の場合はプレイヤ名など入力してもらう
        if (commonM.saveM.saveData.playerInfo.get_name(def_player.playerNameKind.player) == "")
        {
            commonM.audioM.SE_Play(AudioManager.WhichSE.Done);

            nextId = (byte)commonM.dataM.get_nextSceneId(commonM.sceneChanger.get_nowSceneId(), 1); // チュートリアルシナリオテストシーン
            commonM.id_startScenario = 0; // チュートリアルシナリオを再生するように、シナリオ0番を指定しておく

            // プレイヤ名について
            window_nameInput input = Instantiate(commonM.dataM.window_nameInput, canvasRect).GetComponent<window_nameInput>();
            input.setupField(def_player.playerNameKind.player, false);
            // 入力が終了するまで待機
            while (true)
            {
                // 入力終了判定
                if (input.endInput)
                {
                    input.letWindowDestroy = true; // ウィンドウ破棄を許可
                    break;
                }
                yield return null;
            }

            // 工房名について
            input = Instantiate(commonM.dataM.window_nameInput, canvasRect).GetComponent<window_nameInput>();
            input.setupField(def_player.playerNameKind.workspace, false);
            while (true)
            {
                // 入力終了判定
                if (input.endInput)
                {
                    input.letWindowDestroy = true;
                    break;
                }
                yield return null;
            }
        }
        else if (!commonM.saveM.saveData.playerInfo.isEndTutorial[0])
        {
            // 名前入力終わってるが、小紋づくりTutorialが終わっていない
            nextId = (byte)commonM.dataM.get_nextSceneId(commonM.sceneChanger.get_nowSceneId(), 1); // チュートリアルシナリオテストシーン
            commonM.id_startScenario = 0;
        }
        else if (!commonM.saveM.saveData.playerInfo.isEndTutorial[1])
        {
            // チュートリアルシナリオを再生するように、シナリオ1番を指定しておく
            nextId = (byte)commonM.dataM.get_nextSceneId(commonM.sceneChanger.get_nowSceneId(), 1); // チュートリアルシナリオテストシーン
            commonM.id_startScenario = 1;
        }
        else if (!commonM.saveM.saveData.playerInfo.isEndTutorial[2])
        {
            // チュートリアルシナリオを再生するように、シナリオ2番を指定しておく
            nextId = (byte)commonM.dataM.get_nextSceneId(commonM.sceneChanger.get_nowSceneId(), 1); // チュートリアルシナリオテストシーン
            commonM.id_startScenario = 2;
        }
        else
        {
            nextId = (byte)commonM.dataM.get_nextSceneId(commonM.sceneChanger.get_nowSceneId()); // sceneInfoに記載された「次のシーンのID」を参照
            commonM.audioM.SE_Play(0);
        }

        commonM.sceneChanger.SceneChange(nextId); // シーン遷移
    }
}
