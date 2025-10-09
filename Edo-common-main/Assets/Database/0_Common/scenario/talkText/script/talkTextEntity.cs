/// <summary>
/// セリフEntity
/// </summary>

[System.Serializable]
public class talkTextEntity
{
    public talkCommand command;
    public ushort charaId; // 発話者のキャラクタID
    public string value; // 発話内容

    /// <summary>
    /// シナリオExcelのcommand列に入力する内容・普通のセリフであれば入力しなくてOK（勝手にtalkになる）
    /// </summary>
    public enum talkCommand
    {
        talk = 0, //普通のセリフ・value = セリフ
        bgm, // BGM切替・value = BGMのID
        bgm_env, // 環境音切替・value = BGMのID
        se, // SE再生・value = SEのID
        wait, // 指定時間待機・value = 待機時間(s)
        fadeout, // 暗転・value = 暗転時間(s)
        fadein, // 明転・value = 明転時間(s)
        sceneChange, // 指定シーンへ遷移・value = 遷移先シーンのID
        changeBg, // 背景画像の切替
        changeChara, // キャラ画像交換だけ・value = 変更先のキャラID
        getTitle, // 称号獲得
        camPos, // カメラ位置移動・value = カンマ区切りで座標指定
        camRot, // カメラ角度
        camShake // カメラ揺らし・value = カメラ揺らす時間(s)
    }
}
