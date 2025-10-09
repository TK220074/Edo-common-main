using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class settingsScript : MonoBehaviour
{
    [SerializeField]private RectTransform[] volumeBarsParent;
    [SerializeField]private Color[] barColor;
    [SerializeField]private Sprite[] sprites;
    [SerializeField]private TextMeshProUGUI[] volNumText;
    [SerializeField]private GameObject licenseObj;
    [SerializeField]private RectTransform canvasRect;

    private Image[,] volumeBarsImgs;

    private Manager_CommonGroup commonM;

    // Start is called before the first frame update
    void Start()
    {
        commonM = Manager_CommonGroup.instance;
        
        volumeBarsImgs = new Image[volumeBarsParent.Length, volumeBarsParent[(int)AudioManager.WhichAudio.BGM - 1].childCount];
        for(int h = 0; h < volumeBarsParent.Length; h++)
        {
            for(int i = 0; i < volumeBarsParent[h].childCount; i++)
            {
                volumeBarsImgs[h, i] = volumeBarsParent[h].GetChild(i).GetComponent<Image>();
            }
        }
        setVolumeBarImg(AudioManager.WhichAudio.BGM, commonM.audioM.GetAudioNum(AudioManager.WhichAudio.BGM));
        setVolumeBarImg(AudioManager.WhichAudio.SE, commonM.audioM.GetAudioNum(AudioManager.WhichAudio.SE));
    }

    /// <summary>
    /// 音量バーの表示切替をする
    /// </summary>
    /// <param name="audioType">BGMかSEか</param>
    /// <param name="num">音量(0~10)</param>
    /// <returns>終了したらtrue</returns>
    private bool setVolumeBarImg(AudioManager.WhichAudio audioType, byte num)
    {
        int h = (int)audioType - 1;

        // バーをいったん黒く、点線にする
        for(int i = 1; i < volumeBarsParent[h].childCount; i++)
        {
            volumeBarsImgs[h, i].color = barColor[0];
            volumeBarsImgs[h, i].sprite = sprites[0];
        }
        
        // 音量0
        if(num == 0)
        {
            // バツマークを色付け、実線にする
            volumeBarsImgs[h, 0].color = barColor[1];
            volumeBarsImgs[h, 0].sprite = sprites[1];

            // バーを黒く、点線にする
            for(int i = 1; i < volumeBarsParent[h].childCount; i++)
            {
                volumeBarsImgs[h, i].color = barColor[0];
                volumeBarsImgs[h, i].sprite = sprites[0];
            }
            volNumText[(byte)audioType - 1].text = num.ToString("00");
            return true;
        }
        else if(num > 10)
        {
            // 最大音量は10
            num = 10;
        }

        // バツマークを黒く、点線にする
        volumeBarsImgs[h, 0].color = barColor[0];
        volumeBarsImgs[h, 0].sprite = sprites[2];
        for(int i = 1; i < (num + 1); i++)
        {
            // バーを音量の数だけ色付け&実線にする
            volumeBarsImgs[h, i].color = barColor[2];
            volumeBarsImgs[h, i].sprite = null;
        }
        volNumText[(byte)audioType - 1].text = num.ToString("00");
        return true;
    }

    public void setVolumeNum_BGM(bool isPlus)
    {
        byte vol = commonM.audioM.GetAudioNum(AudioManager.WhichAudio.BGM);
        if(isPlus)
        {
            if(vol < 10)vol++;
        }
        else
        {
            if(vol > 0)vol--;
        }
        commonM.audioM.ChangeVol(AudioManager.WhichAudio.BGM, vol);
        setVolumeBarImg(AudioManager.WhichAudio.BGM, vol);
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
    }

    public void setVolumeNum_SE(bool isPlus)
    {
        byte vol = commonM.audioM.GetAudioNum(AudioManager.WhichAudio.SE);
        if(isPlus)
        {
            if(vol < 255)vol++;
        }
        else
        {
            if(vol > 0)vol--;
        }
        commonM.audioM.ChangeVol(AudioManager.WhichAudio.SE, vol);
        setVolumeBarImg(AudioManager.WhichAudio.SE, vol);
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
    }

    /// <summary>
    /// タイトル画面へ遷移
    /// </summary>
    public void goTitle()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        StartCoroutine(waitWindow_title());
    }
    IEnumerator waitWindow_title()
    {
        window_question que = Instantiate(commonM.dataM.window_question, canvasRect).GetComponent<window_question>();
        que.setQuestion(commonM.dataM.get_constString(8)); // 質問書き換え
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
            // yes だったら
            commonM.sceneChanger.SceneChange(0); // タイトルへ
        }
    }

    /// <summary>
    /// セーブデータの初期化
    /// </summary>
    public void dataDelete()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        StartCoroutine(waitWindow_delete());
    }
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

    /// <summary>
    /// クレジットロールへ遷移
    /// </summary>
    public void goCredit()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        commonM.sceneChanger.SceneChange(1);
    }

    /// <summary>
    /// ライセンス表示の開
    /// </summary>
    public void doLicense()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        Instantiate(licenseObj, canvasRect);
    }
    
}
