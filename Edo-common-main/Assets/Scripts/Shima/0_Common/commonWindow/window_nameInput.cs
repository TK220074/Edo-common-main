using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 名前入力についてのスクリプト。
/// </summary>
public class window_nameInput : commonWindow
{
#if false
       ※以下のような感じで使ってください※
        
    IEnumerator waitingNameInput(bool isPlayer)
    {
        def_player.playerNameKind nameKind = def_player.playerNameKind.player;
        if (!isPlayer)
        {
            // 工房名
            nameKind = def_player.playerNameKind.workspace;
        }
        window_nameInput input = Instantiate(editObj_playerName, windowRect).GetComponent<window_nameInput>(); // ウィンドウ生成
        input.setupField(nameKind, true); // ウィンドウの初期化

        // 入力が終了するまで待機
        string value = "";
        while (true)
        {
            // 入力終了判定
            if (input.endInput)
            {
                value = input.inputvalue;
                input.letWindowDestroy = true; // ウィンドウ破棄を許可
                break;
            }
            else if(input == null)
            {
                // ウィンドウを閉じた＝変更を反映させない場合は何もしない
                break;
            }

            yield return null;
        }

        // 入力内容の検証
        // 何かしら変更が入ったと考えられる場合は、プロフィール画面を更新
        if (value != "")
        {
            if (isPlayer && (value != name_player.text))
            {
                setupProfile();
            }
            else if (!isPlayer && (value != name_workspace.text))
            {
                setupProfile();
            }
        }
    }

#endif

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI topTitleText;

    [SerializeField] private GameObject button_close;

    [SerializeField, Header("Button_SelectThis")] private TextMeshProUGUI selectThisButtonText;
    [SerializeField] private Button button_selectThis;

    /// <summary>
    /// 入力された内容
    /// </summary>
    public string inputvalue { get; private set; }

    private Manager_CommonGroup m;
    private def_player playerInfo;

    /// <summary>
    /// どの名前について設定するか
    /// </summary>
    private def_player.playerNameKind setNameKind;

    /// <summary>
    /// 名前入力ウィンドウを初期化する
    /// </summary>
    /// <param name="kind">このウィンドウで入力させる名前種</param>
    /// <param name="enableClosing">「閉じる」ボタンを表示させるならtrue</param>
    /// <param name="buttonText">「これにする！」ボタンに該当するボタンのText内容</param>
    /// <param name="message">（kindでotherを選択した場合のみ）表示する内容</param>
    public void setupField(def_player.playerNameKind kind, bool enableClosing, string buttonText = "これにする！", string message = "")
    {
        endSetup = endInput = false; // 入力内容が異なったときの、再度セットアップに備えて初期化

        base.Start();
        m = Manager_CommonGroup.instance;
        playerInfo = m.saveM.saveData.playerInfo;

        setNameKind = kind;

        selectThisButtonText.SetTextAndExpandRuby(buttonText);

        button_close.SetActive(enableClosing);

        // 最大入力数を設定
        if (kind != def_player.playerNameKind.other)
        {
            inputField.characterLimit = (int)m.dataM.get_constFloat(0); // 名前の最大文字数
        }
        else
        {
            inputField.characterLimit = 0; // パスワードなど、名前以外の入力数は無制限？
        }

        // 上部に表示する文章を設定
        if (kind == def_player.playerNameKind.other)
        {
            // other = 例外的な処理の場合、引数messageを表示する
            topTitleText.SetTextAndExpandRuby(message);
        }
        else
        {
            topTitleText.SetTextAndExpandRuby(m.dataM.get_constString((int)kind + 4));
        }
        inputString();
        endSetup = true;
    }

    /// <summary>
    /// 「これにする」が押された時の処理
    /// </summary>
    public void pressed_SelectThis()
    {
        if(endSetup)
        {
            audioM.SE_Play(AudioManager.WhichSE.Done);

            switch (setNameKind)
            {
                case def_player.playerNameKind.komon:
                    // 小紋の名づけ
                    break;

                case def_player.playerNameKind.colorRecipe:
                    // 色レシピの名づけ
                    break;

                case def_player.playerNameKind.other:
                    // 例外的な処理（パスワード入力など）
                    break;


                default:
                    // player or workspace
                    // 名前記録を検証
                    if (!playerInfo.set_name(setNameKind, inputField.text))
                    {
                        // 名前の記録失敗（文字数上限など）
                        string mess = $"文字数が1<r=い>以</r>上{m.dataM.get_constFloat(0)}<r=い>以</r>下でないか\n<r=つか>使</r>えない文字が入っています";
                        topTitleText.SetTextAndExpandRuby(mess);
                        return;
                    }
                    break;

            }
            endInput = true;
            StartCoroutine(waitForDestroy());
        }
    }

    /// <summary>
    /// 入力内容に変更が入ったら呼び出される・入力内容を格納する
    /// </summary>
    public void inputString()
    {
        inputvalue = inputField.text;
        if(inputvalue.Length > 0)
        {
            button_selectThis.interactable = true;
        }
        else
        {
            button_selectThis.interactable = false;
        }
    }
    /// <summary>
    /// 入力フィールドがタップ/選択された時に呼び出される
    /// </summary>
    public void pressed_field()
    {
        audioM.SE_Play(AudioManager.WhichSE.Done);
    }
}
