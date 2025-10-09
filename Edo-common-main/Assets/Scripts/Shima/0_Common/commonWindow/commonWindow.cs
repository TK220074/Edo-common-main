using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 名前入力/質問ウィンドウの共通部分
/// </summary>
public class commonWindow : MonoBehaviour
{
#if false
    // 以下は、名前入力と質問の合わせ技

    IEnumerator waitWindow_delete()
    {
        // 質問は2回行う
        window_question que = Instantiate(commonM.dataM.window_question, canvasRect).GetComponent<window_question>();
        que.setQuestion(commonM.dataM.get_constString(9));
        while (true)
        {
            // 答えが入力されたか
            if (que.endInput)
            {
                break;
            }
            yield return null;
        }
        if (System.Convert.ToBoolean(commonM.tempValue[0]))
        {
            // yesだったらもう一度
            que = Instantiate(commonM.dataM.window_question, canvasRect).GetComponent<window_question>();
            que.setQuestion(commonM.dataM.get_constString(10));
            while (true)
            {
                // 答えが入力されたか
                if (que.endInput)
                {
                    break;
                }
                yield return null;
            }
        }

        // 再度確認での選択内容の検証
        if (System.Convert.ToBoolean(commonM.tempValue[0])) 
        { 
            // yesだった場合
            // パスワード要求
            // 入力画面生成と初期化
            window_nameInput input = Instantiate(commonM.dataM.window_nameInput, canvasRect).GetComponent<window_nameInput>();
            input.setupField(def_player.playerNameKind.other, true, "これでOK", "あいことばを　入力してください");
            string value;
            while (true)
            {
                // 答えが入力されたか
                if (input.endInput)
                {
                    value = input.inputvalue; // 選択内容を取得
                    
                    // パスワード検証
                    if (value == commonM.dataM.get_constString(11))
                    {
                        // パスワードが正しい場合
                        input.letWindowDestroy = true; // ウィンドウ破棄を許可
                        commonM.saveM.dataDelete(); // 削除
                        commonM.sceneChanger.SceneChange(0); // タイトルへ
                        break;
                    }
                    else
                    {
                        commonM.audioM.SE_Play(AudioManager.WhichSE.Cancel);
                        input.setupField(def_player.playerNameKind.other, true, "これでOK", "あいことばが　ちがいます");
                    }
                }
                yield return null;
            }
        }
    }

#endif

    /// <summary>
    /// 入力終了したか
    /// </summary>
    public bool endInput { get; protected set; }
    /// <summary>
    /// ウィンドウ破棄してよいか（＝選択を取得できたか）
    /// </summary>
    public bool letWindowDestroy { get; set; }

    /// <summary>
    /// ウィンドウの初期化完了したか
    /// </summary>
    protected bool endSetup;

    protected Manager_CommonGroup commonM;
    protected AudioManager audioM;

    // Start is called before the first frame update
    virtual protected void Start()
    {
        commonM = Manager_CommonGroup.instance;
        audioM = commonM.audioM;
    }

    /// <summary>
    /// 選択入力後の破棄を待機する
    /// </summary>
    protected IEnumerator waitForDestroy()
    {
        while (true)
        {
            if (endSetup && endInput && letWindowDestroy)
            {
                Destroy(this.gameObject);
                break;
            }
            yield return null;
        }
    }
}
