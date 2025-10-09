using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 質問ウィンドウを扱う
/// </summary>
public class window_question : commonWindow
{
#if false
    ※以下のような感じで使ってください※

    IEnumerator waitWindow_title()
    {
        window_question que = Instantiate(questionWindowObj, canvasRect).GetComponent<window_question>();
        que.setQuestion(dataM.get_constString(8));
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
            sceneM.SceneChange(0); // タイトルへ
        }
    }

#endif

    [SerializeField] private TextMeshProUGUI text_question;
    [SerializeField] private TextMeshProUGUI[] text_buttons;

    /// <summary>
    /// 質問画面のセットアップ・メッセージテキストを代入
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    /// <param name="buttonText_yes">「はい」に該当するボタンの表示内容</param>
    /// <param name="buttonText_no">「いいえ」に該当するボタンの表示内容</param>
    public void setQuestion(string message, string buttonText_yes = "はい", string buttonText_no = "やっぱいいや")
    {
        endSetup = endInput = false; // 質問がループするときの、再度セットアップに備えて初期化

        base.Start();

        text_question.SetTextAndExpandRuby(message);
        text_buttons[0].SetTextAndExpandRuby(buttonText_yes);
        text_buttons[1].SetTextAndExpandRuby(buttonText_no);
        endSetup = true;
    }

    /// <summary>
    /// はい/いいえボタン押したときの処理
    /// </summary>
    /// <param name="isYes">「はい」に該当するボタンか</param>
    public void setAnswer(bool isYes)
    {
        if (endSetup)
        {
            if (isYes)
            {
                audioM.SE_Play(AudioManager.WhichSE.Done);
            }
            else
            {
                audioM.SE_Play(AudioManager.WhichSE.Cancel);
            }

            commonM.tempValue[0] = (short)System.Convert.ToInt32(isYes); // 回答を一時格納場所に保存
            endInput = true; // 回答終了フラグ
            letWindowDestroy = true; // 破棄を許可
            StartCoroutine(waitForDestroy());
        }
    }
}
