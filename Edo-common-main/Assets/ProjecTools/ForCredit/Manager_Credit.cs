using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Unity.VectorGraphics.SVGParser;

/// <summary>
/// エンドロールを制御します。
/// </summary>
public class Manager_Credit : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _copyText; // 最後に表示されるコピーライト
    //[SerializeField] private string copyString; // コピーライト
    [SerializeField] private GameObject _endrollTextObj; //エンドロールオブジェクト
    [SerializeField] private float _speed = 0.01f; // 通常スクロールスピード
    [SerializeField] private float _speedAdd = 0.2f; // ボタン押下時のスピード
    [SerializeField] private float _copySec = 3.0f; // コピーライト表示時間（秒）
    [SerializeField] private bool _textPosIsBottom = true; // クレジットテキストの初期位置について・画面下端に来るか
    [SerializeField] private StaffRollWriter SFW; // スタッフロールを記述するスクリプト
    [SerializeField] private float endPosAdjust = 30f; // 終了Posの微調整用(画面上端から上にいくつ動くか)

    private float _startPosY = 0;
    private float _endPosY = 0;
    private RectTransform _textRect;
    private SceneChanger _sc;
    private AudioManager _audio;
    private bool _getOperate = true;
    private bool _loadedNextScene = false;
    private bool _openedCopy = false;
    private float count = 0;
    private bool[] _doScroll = new bool[2]; // 0,1 = Up,Down

    private bool isTapping; // タップ中か

    // Start is called before the first frame update
    void Start()
    {
        _copyText.CrossFadeAlpha(0, 0, false); // コピーライトの文字を見えなくする

        Manager_CommonGroup commonM = Manager_CommonGroup.instance;
        _sc = commonM.sceneChanger;
        _audio = commonM.audioM;

        _textRect = _endrollTextObj.GetComponent<RectTransform>();
        TextMeshProUGUI text = _endrollTextObj.GetComponent<TextMeshProUGUI>();

        //_copyText.text = copyString; // コピーライト入力
        _copyText.text = $"©{commonM.dataM.get_constString(0)} {Application.companyName}";
        SFW.WriteStaffRoll(text); // エンドロールを入力

        StartCoroutine(Calculate()); // 開始・終了Posを計算する。

        _audio.BGM_Stop();
        _audio.SE_Play(_audio.list_BGM.list[(byte)commonM.dataM.sceneInfo.list[(int)_sc.get_nowSceneId()].bgmId].clip);
        //_sc.ScenePreLoad(_nextSceneID); // エンドロール後のシーンを先読み
    }

    // Update is called once per frame
    void Update()
    {
        if (_getOperate)
        {
            if(isTapping)
            {
                //決定ボタンで早送り
                count += _speedAdd * Time.deltaTime;
                _audio.ChangePitch(AudioManager.WhichAudio.SE, 2.5f);
            }
            else
            {
                //何も押されていなければ自動スクロール
                _audio.ChangePitch(AudioManager.WhichAudio.SE, 1.0f);
                count += _speed * Time.deltaTime;
            }
            DownText();//スクロール実行
        }
    }

    public void onTapping()
    {
        isTapping = true;
    }
    public void endTapping()
    {
        isTapping = false;
    }

    /// <summary>
    /// (初期処理で求めたStartとEndはおかしな数値になっていたため)
    /// 呼び出し1フレーム後に開始、終了Posを計算する。
    /// </summary>
    IEnumerator Calculate()
    {
        yield return null;
        CalcPos(_textPosIsBottom);
        //_textRect.anchoredPosition = new Vector2(0, _startPosY); //テキストオブジェクトの上端を画面中央に来るようにする
        _textRect.localPosition = new Vector2(_textRect.localPosition.x, _startPosY); //テキストオブジェクトの上端を画面中央に来るようにする
    }

    /// <summary>
    /// 開始、終了判定後、テキストをスクロールする。
    /// </summary>
    private void DownText()
    {
        if (count <= 0)
        {
            count = 0;
        }
        else if (count >= 1.005f)
        {
            _getOperate = false;//入力を受け付けなくする
            _audio.ChangePitch(AudioManager.WhichAudio.SE, 1.0f);//ピッチを元に戻す
            count = 1.0f;
            if (!_openedCopy)
            {//既にコピーライト表示していないか
                _openedCopy = true;
                _getOperate = false;
                StartCoroutine(OpenCopy());//コピーライト表示
            }
        }
        else
        {
            _textRect.localPosition = new Vector2(_textRect.localPosition.x, Mathf.Lerp(_startPosY, _endPosY, count)); //Leapでスクロール
        }
    }

    /// <summary>
    /// コピーライトを表示し、指定秒待機後シーン読込開始する
    /// </summary>
    IEnumerator OpenCopy()
    {
        _copyText.CrossFadeAlpha(1, 0.5f, false);
        yield return new WaitForSeconds(_copySec);
        LoadNextScene();
    }

    /// <summary>
    /// キャンセルボタンが押された時の挙動
    /// </summary>
    private void Cancel()
    {
        if (_getOperate)
        {
            _getOperate = false;
            LoadNextScene();
        }
    }

    /// <summary>
    /// 開始、終了Posを計算する。
    /// </summary>
    /// <param name="PosIsBottom">テキスト上端を、画面下端に移動させるか</param>
    private void CalcPos(bool PosIsBottom)
    {
        float rectY = _textRect.sizeDelta.y; //テキストオブジェクトの縦サイズ
        if (PosIsBottom)
        {
            _startPosY = -(rectY / 2.0f) * 1.06f;//上端が画面下端に来る
        }
        else
        {
            _startPosY = -(rectY / 2.0f);//上端が真ん中に来る
        }

        // 原点は中心→画面上端は表示領域の縦サイズの半分だけ上に移動した場所
        // FullHD基準なので1080にしているが、アスペクト比が変わると上手くいかなくなる。
        // より良い方法は？？？
        float EndLine = (1080f / 2.0f) + endPosAdjust;
        // スクロール終了ポイントの設定（画面上端から上にendPosAdjust分動いたところ）
        _endPosY = (rectY / 2.0f) + EndLine;
    }

    /// <summary>
    /// 次のシーンをロードし、移動する
    /// </summary>
    private void LoadNextScene()
    {
        if (!_loadedNextScene)
        {//シーンロード開始してないか
            _getOperate = false;
            _loadedNextScene = true;
            _audio.SE_Stop(true);
            byte nextId = (byte)Manager_CommonGroup.instance.dataM.get_nextSceneId(_sc.get_nowSceneId());
            _sc.SceneChange((byte)nextId);
        }
    }
}
