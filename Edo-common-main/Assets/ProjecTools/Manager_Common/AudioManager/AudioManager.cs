using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

///<summary>
///オーディオの管理と再生を行う
///</summary>
public class AudioManager : MonoBehaviour
{
    private AudioSource[] _source_BGM;//BGMのAudioSource
    [SerializeField] private AudioSource _source_SE;//SEのAudioSource

    [SerializeField] private SoundList _list_BGM;
    public SoundList list_BGM => _list_BGM;

    [SerializeField] private SoundList _list_BGM_Env;
    public SoundList list_BGM_Env => _list_BGM_Env;

    [SerializeField] private SoundList _list_SE;
    public SoundList list_SE => _list_SE;

    [SerializeField] private SoundList list_SE_UI;//UI用SEのデータベース（配列）
    [SerializeField] private AudioMixer _mixer;//ミキサー

    [SerializeField] private byte _bgmSourceNum = 3; // BGMのAudioSourceの数＝チャネル数
    public byte bgmSourceNum => _bgmSourceNum;

    private SaveDataManager _saveDataManager;//セーブデータを管理するスクリプト
    private DatabaseManager dataM;

    public bool[] BGM_IsPlaying { get; private set; }//BGM再生中か

    private float time = 0;//フェードアウト用
    private const float ENVVOL = 0.4f; // 環境音のボリューム基準(0~1f)

    public enum WhichAudio
    {
        Master = 0,
        BGM = 1,
        SE = 2
    }

    public enum WhichSE
    {
        Done = 0,
        Cancel = 1,
    }

    /// <summary>
    /// AudioManagerの初期化
    /// </summary>
    /// <returns>終了したらtrue</returns>
    public bool Initialize()
    {
        Manager_CommonGroup commonM = Manager_CommonGroup.instance;
        _saveDataManager = commonM.saveM;//設定内容一覧をセーブデータから取得
        dataM = commonM.dataM;
        CreateAudioSource(bgmSourceNum);
        while (true)
        {
            if (_saveDataManager.Loaded)
            {//セーブデータの読込待ち
                //音量の初期化
                //devlog.log("Audio初期化：セーブデータにあるBGMとSEの音量を反映させます。");
                ChangeVol(WhichAudio.BGM, GetAudioNum(WhichAudio.BGM));
                ChangeVol(WhichAudio.SE, GetAudioNum(WhichAudio.SE));
                break;
            }
        }
        return true;
    }

    /// <summary>
    /// 任意のAudioClipを再生する・SE用
    /// </summary>
    /// <param name="clip">再生させるSE Clip</param>
    public void SE_Play(AudioClip clip)
    {
        //devlog.log("SE：「" + clip.name + "」 を再生します。");
        ChangePitch(WhichAudio.SE, 1.0f);
        ChangeVol_Temp(WhichAudio.SE, 1.0f);
        _source_SE.PlayOneShot(clip);
    }
    /// <summary>
    /// SEのSoundListから任意のAudioClipを再生する・UIのSE用
    /// </summary>
    /// <param name="whichSE">再生させるSE種</param>
    public void SE_Play(WhichSE whichSE)
    {
        //devlog.log("UI用のSE：「" + list_SE_UI.SoundList[(int)whichSE].name + "」 を再生します。");
        ChangePitch(WhichAudio.SE, 1.0f);
        ChangeVol_Temp(WhichAudio.SE, 1.0f);
        _source_SE.PlayOneShot(list_SE_UI.list[(int)whichSE]);
    }
    /// <summary>
    /// 指定したIDのSEを再生させる
    /// </summary>
    /// <param name="seId">再生させるSEの番号</param>
    public void SE_Play(int seId)
    {
        if(seId < _list_SE.list.Count)
        {
            SE_Play(_list_SE.list[seId]);
        }
        else
        {
            devlog.logError($"リストの範囲外を指定しています！：{seId}");
        }
    }

    /// <summary>
    /// ループするSEの再生を止める
    /// </summary>
    /// <param name="DoFadeout">停止時フェードアウトするか</param>
    public void SE_Stop(bool DoFadeout = false)
    {
        if (DoFadeout)
        {//フェードアウトを行うか
            StartCoroutine(AudioFadeout(false));
        }
        else
        {
            //devlog.log("SEを停止します。");
            _source_SE.Stop();
        }
    }

    /// <summary>
    /// BGM再生の本体・StartCoroutine()で呼び出してね
    /// </summary>
    /// <param name="clip">再生させるBGM Clip</param>
    /// <param name="notLoop">ループさせない＝PlayeOneShotでの再生か</param>
    /// <param name="StartTime">どの時点から再生開始するか(秒)</param>
    /// <param name="channel">どのチャンネルで再生させるか</param>
    public IEnumerator BGM_Play(AudioClip clip, bool notLoop = false, float StartTime = 0, byte channel = 0)
    {
        // 再生中か
        if (BGM_IsPlaying[channel])
        {
            BGM_Stop(true, channel);

            // フェードアウト終了まで待機
            while (true)
            {
                if (get_audioVol(WhichAudio.BGM, channel) == 0)
                {
                    break;
                }
                yield return null;
            }
        }
        if (notLoop)
        {
            _source_BGM[channel].PlayOneShot(clip);
        }
        else
        {
            _source_BGM[channel].clip = clip;
            _source_BGM[channel].time = StartTime;
            ChangePitch(WhichAudio.BGM, 1.0f, channel);

            // 通常BGM再生時は、音量の一時的変更をリセットする
            if (channel == 0)
            {
                ChangeVol_Temp(WhichAudio.BGM, 1.0f, channel);
            }

            _source_BGM[channel].Play();
            BGM_IsPlaying[channel] = true;
        }
        //devlog.log("BGM：「" + clip.name + "」 を再生します。");
    }
    /// <summary>
    /// 指定したIDのBGMを再生させる
    /// </summary>
    /// <param name="isEnv">環境音か</param>
    /// <param name="bgmId">再生させるBGMのID</param>
    /// <param name="notLoop">ループさせない＝PlayeOneShotでの再生か</param>
    /// <param name="StartTime">どの時点から再生開始するか(秒)</param>
    /// <param name="channel">どのチャンネルで再生させるか</param>
    public void BGM_Play(bool isEnv, int bgmId, bool notLoop = false, float StartTime = 0, byte channel = 0)
    {
        if(!isEnv)
        {
            // 普通のBGM
            if(bgmId < list_BGM.list.Count)
            {
                StartCoroutine(BGM_Play(list_BGM.list[bgmId], notLoop, StartTime, channel));
            }
            else
            {
                devlog.logError($"リストの範囲外を指定しています！：{bgmId}");
            }
        }
        else
        {
            // 環境音
            if (bgmId < list_BGM_Env.list.Count)
            {
                ChangeVol_Temp(WhichAudio.BGM, ENVVOL, 1); // 環境音のBGM音量は小さめ
                StartCoroutine(BGM_Play(list_BGM_Env.list[bgmId], notLoop, 0, 1));
            }
            else
            {
                devlog.logError($"リストの範囲外を指定しています！：{bgmId}");
            }
        }
    }

    /// <summary>
    /// BGMの再生を止める
    /// </summary>
    /// <param name="DoFadeout">停止時フェードアウトさせるか</param>
    /// /// <param name="channel">どのチャンネルのBGMをストップさせるか</param>
    public void BGM_Stop(bool DoFadeout = false, byte channel = 0)
    {
        // 再生状態か
        if (BGM_IsPlaying[channel])
        {
            // フェードアウトを行うか
            if (DoFadeout)
            {
                StartCoroutine(AudioFadeout(true, channel));
            }
            else
            {
                //devlog.log($"Channel{channel} BGMを停止します。");
                _source_BGM[channel].Stop();
                _source_BGM[channel].clip = null;
            }

            BGM_IsPlaying[channel] = false;
        }
    }

    /// <summary>
    /// BGMを一時停止させる
    /// </summary>
    /// /// <param name="channel">一時停止するBGMのチャンネル</param>
    public void BGM_Pause(byte channel = 0)
    {
        if (BGM_IsPlaying[channel])
        {//再生状態か
            //devlog.log("BGMを一時停止します。");
            _source_BGM[channel].Pause();//一時停止して
            BGM_IsPlaying[channel] = false;//停止状態にする
        }
    }

    /// <summary>
    /// BGMの一時停止を解除する
    /// </summary>
    /// <param name="channel">停止解除するBGMのチャンネル</param>
    public void BGM_UnPause(byte channel = 0)
    {
        if (!BGM_IsPlaying[channel])
        {//停止状態か
            //devlog.log("BGMの一時停止を解除します。");
            _source_BGM[channel].UnPause();
            BGM_IsPlaying[channel] = true;
        }
    }

    /// <summary>
    /// 「オーディオミキサで」音量の変更を行う
    /// </summary>
    /// <param name="audio">どのオーディオの音量を変えるか</param>
    /// <param name="volume">変更後の音量・0~10で指定</param>
    public void ChangeVol(WhichAudio audio, byte volume)
    {
        //設定値は10より大きい値にはならない
        if (volume > 10)
        {
            volume = 10;
        }

        //https://kingmo.jp/kumonos/unity-audiomixer-control-volume/#index_id4 より
        //音量設定は10段階で行うから、÷10して「0 ~ 1」に収める
        float fNum = (float)volume / 10.0f;

        //Mathf.Log10(value) * 20fは相対量をdBに変換する式
        //Mathf.Clampで「-80~0」の間に収まるようにしている
        float vol = Mathf.Clamp(Mathf.Log10(fNum) * 20.0f, -80.0f, 0f);

        //AudioMixerでの値を変更する
        switch (audio)
        {
            case WhichAudio.Master:
                _mixer.SetFloat("Vol_Master", vol);
                break;

            case WhichAudio.BGM:
                _mixer.SetFloat("Vol_BGM", vol);
                break;

            case WhichAudio.SE:
                _mixer.SetFloat("Vol_SE", vol);
                break;
        }
        _saveDataManager.saveData.Volume[(int)audio - 1] = volume;
    }

    /// <summary>
    /// 「AudioSource」で音量の変更を行う
    /// 一時的に変更する用・例えばポーズ画面開く時とか
    /// </summary>
    /// <param name="audio">BGM/SE　どちらの音量を変えるか</param>
    /// <param name="volume">変更後の音量・0~1で指定</param>
    /// <param name="bgmChannel">どのチャンネルのBGMの音量を変えるか</param>
    public void ChangeVol_Temp(WhichAudio audio, float volume, byte bgmChannel = 0)
    {
        if (volume > 1)
        {//volumeが1より大きい（最大値を超える）場合は最大値(1)に設定
            volume = 1;
        }
        else if (volume < 0)
        {
            volume = 0;
        }

        switch (audio)
        {
            case WhichAudio.BGM:
                _source_BGM[bgmChannel].volume = volume;
                break;

            case WhichAudio.SE:
                _source_SE.volume = volume;
                break;
        }
    }
    /// <summary>
    /// 指定したAudioSourceのvolumeを取得する
    /// </summary>
    /// <param name="audio"></param>
    /// <param name="bgmChannel"></param>
    /// <returns></returns>
    public float get_audioVol(WhichAudio audio, byte bgmChannel = 0)
    {
        switch (audio)
        {
            case WhichAudio.BGM:
                return _source_BGM[bgmChannel].volume;

            case WhichAudio.SE:
                return _source_SE.volume;
            
        }
        return -1f;
    }

    /// <summary>
    /// Audioのピッチを変更する
    /// </summary>
    /// <param name="audio">BGM/SE　どちらのピッチを変えるか</param>
    /// <param name="num">変更後のピッチ</param>
    /// /// <param name="bgmChannel">どのチャンネルのBGMのピッチを変えるか</param>
    public void ChangePitch(WhichAudio audio, float num, byte bgmChannel = 0)
    {
        switch (audio)
        {
            case WhichAudio.BGM:
                _source_BGM[bgmChannel].pitch = num;
                break;

            case WhichAudio.SE:
                _source_SE.pitch = num;
                break;
        }
    }

    /// <summary>
    /// セットされているAudioClipを返す・再生中の音楽の名前表示に使えたり？
    /// </summary>
    /// <param name="which">BGM/SE　どちらにセットされているClipについて調べるか</param>
    /// /// <param name="bgmChannel">どのチャンネルのBGMのClipについてか</param>
    /// <returns>指定したAudioSourceにセットされているAudioClip</returns>
    public AudioClip GetPlayingClip(WhichAudio which, byte bgmChannel = 0)
    {
        if (which == WhichAudio.BGM)
        {
            return _source_BGM[bgmChannel].clip;
        }
        else if (which == WhichAudio.SE)
        {
            return _source_SE.clip;
        }
        return null;
    }

    /// <summary>
    /// Audioのフェードアウトを行う
    /// </summary>
    /// <param name="BGMOrSE">BGM:true　SE:false</param>
    IEnumerator AudioFadeout(bool BGMOrSE, byte channel = 0)
    {
        //https://xr-hub.com/archives/18550 より
        //FadeDeltaTime/FadeInSecondsでボリュームの比率が0に近づくようにする。
        time = 0f;
        float maxVol = 1f;
        // 環境音だったときの音量減開始値
        if(channel == 1)
        {
            maxVol = ENVVOL;
        }

        while (true)
        {
            time += Time.deltaTime;
            if (BGMOrSE)
            {
                ChangeVol_Temp(WhichAudio.BGM, (maxVol - (time / dataM.get_constFloat(21))), channel);
            }
            else
            {
                ChangeVol_Temp(WhichAudio.SE, (1.0f - (time / dataM.get_constFloat(21))));
            }
            if (time >= dataM.get_constFloat(21))
            {
                time = dataM.get_constFloat(21);
                if (BGMOrSE)
                {
                    BGM_Stop(false, channel);//停止
                    //devlog.log("BGMのフェードアウトが完了しました。");
                }
                else
                {
                    SE_Stop(false);
                    //devlog.log("SEのフェードアウトが完了しました。");
                }
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// BGM用のAudioSourceの初期化(生成)
    /// </summary>
    /// <param name="num">BGMのチャネル数</param>
    public void CreateAudioSource(byte num)
    {
        BGM_IsPlaying = new bool[num];
        _source_BGM = new AudioSource[num];
        for (byte i = 0; i < num; i++)
        {
            AudioSource src = this.gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.outputAudioMixerGroup = _mixer.FindMatchingGroups("BGM")[0];
            _source_BGM[i] = src;
        }
    }

    /// <summary>
    /// 現在の音量を10段階で返す
    /// </summary>
    /// <param name="audioType">BGMかSEか</param>
    /// <returns>現在の音量（10段階）</returns>
    public byte GetAudioNum(WhichAudio audioType)
    {
        return _saveDataManager.saveData.Volume[(int)audioType - 1];
    }

    /// <summary>
    /// AudioSource自体を取得する
    /// </summary>
    /// <param name="audioType">欲しいSourceの種類</param>
    /// <param name="channel">（BGMのみ）Sourceのチャネル</param>
    /// <returns>指定したAudioSource</returns>
    public AudioSource GetAudioSource(WhichAudio audioType, byte channel = 0)
    {
        switch (audioType)
        {
            case WhichAudio.BGM:
                return _source_BGM[channel];

            case WhichAudio.SE:
                return _source_SE;

            default:
                return null;
        }
    }
}