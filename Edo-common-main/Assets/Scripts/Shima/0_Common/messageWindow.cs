using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class messageWindow : Singleton<messageWindow>
{
    [SerializeField, Header("Scenario Data")] private list_scenario_character list_character;
    [SerializeField] private listList_scenario list_list_scenario;
    [SerializeField] private SpriteList imgList_character;

    [SerializeField, Header("UI Parts")] private TextMeshProUGUI text_message;
    [SerializeField] private TextMeshProUGUI text_charaName;
    [SerializeField] private Image img_character;
    [SerializeField] private CanvasGroup canvasGroup_messageWindow;

    [SerializeField] private RectTransform rect_messageWindow; // (Dev)メッセージウィンドウのRect・疑似的なカメラ揺らし用

    [SerializeField, Header("BGForFadeOut")] private CanvasGroup canvasGroup_bg_black; // フェードアウト用のBG
    [SerializeField] private coverPatternBG bg_pattern;

    private img_BG bgInstance; // 背景画像スクリプトのインスタンス

    private bool scenarioReady; // 使用するシナリオデータを指定し、準備完了しているか
    private ushort nowTextId; // 現在読み込むセリフデータの番号
    private int speakNum; // 使用するシナリオ内のセリフデータの数
    private List<talkTextEntity> nowScenario; // 今回使用するシナリオデータ

    private Manager_CommonGroup commonM; // 共通マネージャ
    private Transform camTrans; // メインカメラのTransform

    private Coroutine showCoroutine; // 実行中のコルーチン
    private int messageLength = -1; // 表示するセリフの文字数
    private bool talkingNow; // 発話中（セリフ文字送り途中）か

    void Start()
    {
        canvasGroup_bg_black.DOFade(0f, 0f); // フェードアウト用のBGを隠す
        img_character.CrossFadeAlpha(0f, 0f, true);
    }

    /// <summary>
    /// 表示するシナリオリストを指定し、シナリオ表示を開始する。
    /// </summary>
    /// <param name="scenarioId">はじめたいシナリオのID</param>
    public bool setScenario(int scenarioId)
    {
        if((scenarioId < list_list_scenario.list.Count) && (scenarioId >= 0))
        {
            commonM = Manager_CommonGroup.instance;
            camTrans = Camera.main.transform;
            bgInstance = img_BG.instance;

            nowScenario = list_list_scenario.list[scenarioId].list; // 今回使うシナリオデータを格納
            speakNum = nowScenario.Count; // セリフ数を取得
            scenarioReady = true;
            showCoroutine = StartCoroutine(setText(0)); // そのシナリオの一番最初のセリフを表示
            canvasGroup_bg_black.interactable = true;
            return true;
        }
        else
        {
            devlog.logError($"範囲外のシナリオIDを指定しています！：{scenarioId}");
            return false;
        }
    }

    /// <summary>
    /// メッセージウィンドウに文字を表示させる
    /// </summary>
    /// <param name="textId">表示させたいセリフの番号</param>
    private IEnumerator setText(ushort textId)
    {
        if (scenarioReady)
        {
            text_charaName.text = "";
            text_message.SetTextAndExpandRuby("");

            talkTextEntity info = nowScenario[textId]; // そのセリフの内容（キャラと発言）を格納
            switch (info.command)
            {
                case talkTextEntity.talkCommand.talk:

                    canvasGroup_messageWindow.DOFade(1f, commonM.dataM.get_constFloat(13)); // セリフ表示時は必ずウィンドウ表示する

                    // キャラ画像切替
                    changeCharaImg(info.charaId);

                    // キャラ名表示
                    if (info.charaId == 0)
                    {
                        // 0番指定だったらプレイヤ名にする
                        text_charaName.text = commonM.saveM.saveData.playerInfo.get_name(def_player.playerNameKind.player);
                    }
                    else
                    {
                        // 0以降は設定されたキャラ名を表示
                        text_charaName.text = list_character.list[info.charaId].name;
                    }

                    // セリフ文字表示
                    // https://nekojara.city/unity-textmesh-pro-typewriter-effect
                    var delay = new WaitForSeconds(commonM.dataM.get_constFloat(12)); // GC Allocを最小化するためキャッシュしておく

                    // ルビふりに対応したtextのセット
                    text_message.SetTextAndExpandRuby(info.value);

                    // タグを除いた文字数を取得する
                    // https://garnetcode.jp/blog/2024/02/textmeshpro_richtexttag/#%E3%82%BF%E3%82%B0%E3%82%92%E9%99%A4%E3%81%84%E3%81%9F%E6%96%87%E5%AD%97%E5%88%97%E3%82%92%E5%8F%96%E5%BE%97%E3%81%99%E3%82%8B
                    text_message.ForceMeshUpdate();
                    messageLength = text_message.GetParsedText().Length; // タグを除いた文字列の長さを取得

                    talkingNow = true; // 文字送り中フラグ

                    // １文字ずつ表示する演出
                    for (int i = 0; i < messageLength; i++)
                    {
                        // 発話中か（ボタン押下によるセリフ全文表示がなされていないか）
                        if (talkingNow)
                        {
                            text_message.maxVisibleCharacters = i; // 徐々に表示文字数を増やしていく

                            // 発話SEの再生数を間引く
                            if ((i % 3) == 0)
                            {
                                commonM.audioM.SE_Play(66 + (int)list_character.list[info.charaId].charaOld); // ボイス再生
                            }

                            yield return delay; // 一定時間待機
                        }
                        else
                        {
                            // 発話終了していたら以降の文字送りをスキップ
                            yield break;
                        }
                    }
                    endShowingMessage();
                    break;

                case talkTextEntity.talkCommand.bgm:
                    // BGM再生・切替
                    commonM.audioM.BGM_Play(false, int.Parse(info.value));
                    button_nextScenario();
                    break;

                case talkTextEntity.talkCommand.bgm_env:
                    // 環境音再生・切替
                    commonM.audioM.BGM_Play(false, int.Parse(info.value));
                    button_nextScenario();
                    break;

                case talkTextEntity.talkCommand.se:
                    // SE再生
                    commonM.audioM.SE_Play(int.Parse(info.value));
                    button_nextScenario();
                    break;

                case talkTextEntity.talkCommand.wait:
                    // ウェイト・待機・沈黙
                    canvasGroup_bg_black.interactable = false;
                    canvasGroup_messageWindow.DOFade(0f, commonM.dataM.get_constFloat(13)); // ウェイト時はウィンドウ非表示
                    yield return new WaitForSeconds(float.Parse(info.value));
                    button_nextScenario();
                    break;

                case talkTextEntity.talkCommand.fadeout:
                    // 背景黒くする・暗転
                    canvasGroup_bg_black.interactable = false;
                    bg_pattern.setCoverPatternBG();
                    canvasGroup_bg_black.DOFade(1f, float.Parse(info.value));
                    button_nextScenario();
                    break;

                case talkTextEntity.talkCommand.fadein:
                    // 背景戻す・明転
                    canvasGroup_bg_black.interactable = false;
                    canvasGroup_bg_black.DOFade(0f, float.Parse(info.value));
                    button_nextScenario();
                    break;

                case talkTextEntity.talkCommand.changeBg:
                    // 背景画像切替
                    bgInstance.changeBG(short.Parse(info.value));
                    button_nextScenario();
                    break;

                case talkTextEntity.talkCommand.sceneChange:
                    // シーン遷移
                    canvasGroup_bg_black.interactable = false;
                    talkingNow = false; // 発話終了フラグ
                    commonM.sceneChanger.SceneChange(byte.Parse(info.value));
                    break;

                case talkTextEntity.talkCommand.changeChara:
                    // キャラ画像切替のみ
                    changeCharaImg(info.charaId);
                    button_nextScenario();
                    break;

                case talkTextEntity.talkCommand.getTitle:
                    // 称号獲得
                    commonM.achieveM.OpenAchievement(int.Parse(info.value));
                    button_nextScenario();
                    break;
#if false
                case talkTextEntity.talkCommand.camPos:
                    {
                        // カメラ移動
                        string[] str = info.value.Split(',');
                        camTrans.DOComplete();
                        camTrans.DOMove(new Vector3(float.Parse(str[0]), float.Parse(str[1]), float.Parse(str[2])), commonM.dataM.get_constFloat(14));
                        break;
                    }

                case talkTextEntity.talkCommand.camRot:
                    {
                        // カメラ回転
                        // 2.5D世界なら DOLockat() を使う？ でもどうやってExcelでキャラ指定して、そのキャラのTransformを取得する？
                        string[] str = info.value.Split(',');
                        camTrans.DOComplete();
                        camTrans.DORotate(new Vector3(float.Parse(str[0]), float.Parse(str[1]), float.Parse(str[2])), commonM.dataM.get_constFloat(14));
                        break;
                    }
#endif
                case talkTextEntity.talkCommand.camShake:
                    // カメラゆらし
                    // https://ゲーム制作.com/%E3%80%90unity%E3%80%91%E3%82%AB%E3%83%A1%E3%83%A9%E3%82%92%E3%82%B7%E3%82%A7%E3%82%A4%E3%82%AF%EF%BC%88%E6%8F%BA%E3%82%89%E3%81%99%EF%BC%89%E3%81%95%E3%81%9B%E3%82%8B%E6%96%B9%E6%B3%95%EF%BC%81/
                    // https://zenn.dev/ohbashunsuke/books/20200924-dotween-complete/viewer/dotween-20
                    float t = float.Parse(info.value); // 揺らす時間
#if false
                    // 通常のカメラ揺らし（3D空間の揺れ）
                    camTrans.DOComplete();
                    camTrans.DOShakePosition(t, 40f, (int)((t * 30f) + 15f));
#else
                    // (Dev)メッセージウィンドウ揺らし（2D空間の揺れ・カメラの疑似揺れ）
                    rect_messageWindow.DOComplete();
                    rect_messageWindow.DOShakePosition(t, 50f, (int)((t * 30f) + 15f));
#endif
                    button_nextScenario();
                    break;

            }
        }
    }

    /// <summary>
    /// 次のセリフデータを参照する
    /// </summary>
    public void button_nextScenario()
    {
        // シナリオデータの指定完了しているか
        if (scenarioReady)
        {
            canvasGroup_bg_black.interactable = false; // 念のためセリフ送りを制限

            // 発話中のシナリオ進行命令か
            if (talkingNow)
            {
                endShowingMessage(); // 全てのセリフを表示する
            }
            else
            {
                // セリフ全文表示後の場合

                nowTextId++; // 次のセリフを指定

                // 会話終了しないかチェック
                if (nowTextId < speakNum)
                {
                    // 会話がまだ続く

                    // 前回の演出処理が走っていたら、停止
                    if (showCoroutine != null)
                    {
                        StopCoroutine(showCoroutine);
                    }

                    // 次のセリフ表示を開始
                    showCoroutine = StartCoroutine(setText(nowTextId));
                    canvasGroup_bg_black.interactable = true;
                }
                else
                {
                    // 会話の終端だった
#if false
                // (Dev)工房に戻す
                commonM.sceneChanger.SceneChange(2);
#endif
                }
            }
        }
    }

    /// <summary>
    /// セリフ全文を表示させる
    /// </summary>
    private void endShowingMessage()
    {
        text_message.maxVisibleCharacters = messageLength; // 全文表示
        talkingNow = false; // 発話終了フラグ
        showCoroutine = null; // 文字送りコルーチン終了
        canvasGroup_bg_black.interactable = true; // セリフ送り制限解除
    }

    /// <summary>
    /// キャラ画像を切り替える
    /// </summary>
    /// <param name="charaId">切替先のキャラID</param>
    private void changeCharaImg(int charaId)
    {
        // キャラ画像切替
        if (img_character != null)
        {
            Sprite sprite = imgList_character.list[charaId];
            // キャラ画像が存在しなければ、透明/非表示にする
            if (sprite != null)
            {
                img_character.sprite = imgList_character.list[charaId];
                img_character.CrossFadeAlpha(1f, 0f, true);
            }
            else
            {
                img_character.CrossFadeAlpha(0f, 0f, true);
            }
        }
    }
}
