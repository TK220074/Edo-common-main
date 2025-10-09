using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

///<summary>
///シーンの読込をします。読込進捗の表示も。
///LoadingImageを変えてね。
///</summary>
public class SceneChanger : MonoBehaviour
{
    [SerializeField, Header("KomonBG")]private RectTransform patternRect;
    private coverPatternBG patternImg;
    
    private CanvasGroup canvasGroup;
    private AudioManager _audio;//AudioManager取得
    private AsyncOperation async;//読み込んでいるシーン
    //private sbyte PreLoadedSceneID = -1;//先読みされているシーン名・先読みしていなければ-1
    public bool alreadyLoad { get; private set; } //既に読み込みと遷移処理開始しているか
    private CanvasScaler canvasScaler;

    private DatabaseManager dataM;

    private WaitForSeconds waitForSeconds;

    /////////////////////////////////////////
    /////////////////////////////////////////
    //この二つを使ってね

    /// <summary>
    /// シーン読み込みを開始させる
    /// </summary>
    /// <param name="SceneID">移動先のシーンID</param>
    /// <param name="DoBGMFade">BGMフェードアウトするか</param>
    public void SceneChange(int SceneID, bool DoBGMFade = false)
    {
        if (!alreadyLoad)
        {
            devlog.log("【SceneChange実行】");
            alreadyLoad = true;
            canvasGroup.blocksRaycasts = true;
            StartCoroutine(SceneLoad(SceneID, DoBGMFade));
        }
    }
#if false
    /// <summary>
    /// シーンの先読みを行う・現時点では非推奨・現時点では、遷移先が確定している場合のみ使用してください
    /// </summary>
    /// <param name="LoadID">読み込むシーンID</param>
    public void ScenePreLoad(sbyte LoadID)
    {
        devlog.log("(先読み命令)シーンID :「" + LoadID + "」");
        StartLoad(LoadID);//読み込み開始
        PreLoadedSceneID = LoadID;//先読みしているシーン名を書き換え
    }
#endif

    /////////////////////////////////////////
    /////////////////////////////////////////
    //以下いじるな


    IEnumerator fadeout()
    {
        canvasGroup.DOFade(0f, dataM.get_constFloat(21));
        yield return new WaitForSeconds(dataM.get_constFloat(21) - 0.1f);
        canvasGroup.blocksRaycasts = false;
        patternRect.offsetMax = new Vector2(-canvasScaler.referenceResolution.x, 0);
    }

    /// <summary>
    /// 実際のシーンロード処理
    /// </summary>
    /// <param name="LoadID">移動先のシーンID</param>
    /// <param name="DoBGMFade">BGMフェードアウトするか</param>
    IEnumerator SceneLoad(int LoadID, bool DoBGMFade)
    {
        canvasGroup.DOFade(1.0f, dataM.get_constFloat(21));
        int nowSceneId = get_nowSceneId();

        _audio.BGM_Stop(true, 1); // 暗転とともに環境音は止める

        patternImg.setCoverPatternBG(); // 読み込みBGの柄をセット
        yield return new WaitForSeconds(dataM.get_constFloat(21));
#if false
        if (PreLoadedSceneID != -1)
        {//先読みしているか
            //読み込もうとしているシーンとは別のシーンを先読みしているか
            if (PreLoadedSceneID != LoadID)
            {
                devlog.logError("先読みしていたシーンと、移動しようとしているシーンが異なります！");
                SceneUnload();//先読みしたシーンを破棄
                StartLoad(LoadID);//読み込み開始
            }
            else
            {//先読みしているシーンへ移動する場合
                LoadID = PreLoadedSceneID;
                PreLoadedSceneID = -1;//先読み情報を初期化
            }
        }
        else
        {//先読みしていない場合
            StartLoad(LoadID);//読み込み開始
        }
#else
        StartLoad(LoadID);//読み込み開始
#endif

        Vector2 progressWindow = new Vector2(-canvasScaler.referenceResolution.x, 0);
        patternRect.offsetMax = progressWindow;

        while (!async.isDone)
        {
            // ロード進捗表現
            progressWindow.x = -(canvasScaler.referenceResolution.x - (canvasScaler.referenceResolution.x * (async.progress / 0.9f)));
            patternRect.offsetMax = progressWindow;

            if (async.progress >= 0.9f)//読み込みが完了したら
            {
                progressWindow.x = 0f;
                patternRect.offsetMax = progressWindow;

                int clipId = dataM.sceneInfo.list[LoadID].bgmId;
                // シーン間で異なるBgmIdを指定していて、かつ「無音」を指定していなければ、新たにBGMを再生する
                if ((clipId != -1) && (dataM.sceneInfo.list[LoadID].bgmId != dataM.sceneInfo.list[nowSceneId].bgmId))
                {
                    _audio.BGM_Play(false, (byte)clipId);
                }
                else if(clipId == -1)
                {
                    // 無音指定であれば消音
                    _audio.BGM_Stop(true, 0);
                    // フェードアウトが終了するまで待機
                    // はじめは音量が0になるのを待機していたが、0にならないパターンがあったため指定秒待機にしている
                    yield return waitForSeconds;
                }

                // 環境音
                clipId = dataM.sceneInfo.list[LoadID].envBgmId;
                if (clipId != -1) 
                {
                    _audio.BGM_Play(true, (byte)clipId, false, 0, 1);
                }
                
                yield return new WaitForSeconds(dataM.get_constFloat(21)); // 進捗表示の小紋を見せるために少しwait

                devlog.log("シーンID :「" + LoadID + "」 へ移動します。");
                async.allowSceneActivation = true; //シーン遷移する

                yield return new WaitForSeconds(0.01f);//完全に遷移しきってから明転

                mainCamera.instance.resetCamera();

                SAME_checkHomeBanner();

                reloadBgImg();

                StartCoroutine(fadeout());//明転
                alreadyLoad = false;//遷移できるフラグ立てる
                yield break;
            }
            yield return null;
        }
    }

    public void Initialize()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        patternImg = patternRect.GetComponent<coverPatternBG>();
        canvasScaler = transform.parent.gameObject.GetComponent<CanvasScaler>();
        patternRect.offsetMax = new Vector2(-canvasScaler.referenceResolution.x, 0);

        Manager_CommonGroup m = Manager_CommonGroup.instance;
        _audio = m.audioM;
        dataM = m.dataM;

        waitForSeconds = new WaitForSeconds(dataM.get_constFloat(21));

        reloadBgImg();
    }

    /// <summary>
    /// sceneInfoで指定されたBGに差し替える
    /// </summary>
    private void reloadBgImg()
    {
        if(img_BG.instance != null)
        {
            img_BG.instance.changeBG((short)dataM.sceneInfo.list[get_nowSceneId()].bgId);
        }
    }

    /// <summary>
    /// 非同期読み込みを開始する
    /// </summary>
    /// <param name="ID">読み込むシーンID</param>
    private void StartLoad(int ID)
    {
        devlog.log("シーンID :「" + ID + "」 の読み込みを開始します。");
        async = SceneManager.LoadSceneAsync(ID);
        async.allowSceneActivation = false;
    }

    /// <summary>
    /// アウトゲームパートなら存在するであろうバナーが存在する場合、シーン遷移に合わせてバナー情報を更新する。
    /// </summary>
    /// <param name="forceDestroy">強制的にDestroyするか</param>
    private void SAME_checkHomeBanner(bool forceDestroy = false)
    {
        home_banner banner = home_banner.instance;
        if(banner != null)
        {
            // 強制削除するか
            if(forceDestroy)
            {
                banner.destroyHomeBanner();
                return;
            }

            byte id = (byte)get_nowSceneId();
            if (dataM.sceneInfo.list[id].part != sceneInfoEntity.gamePart.A)
            {
                // 遷移先がタイトルやクレジットのような「よく訪れそう&&バナーが確実に要らない」シーンであればここで消す
                banner.destroyHomeBanner();
            }
            else
            {
                banner.reloadBannerInfo();
            }
        }
        else
        {
            // ホームパートだったらトップバナー配置
            if ((dataM.sceneInfo.list[get_nowSceneId()].part == sceneInfoEntity.gamePart.A) && (home_banner.instance == null))
            {
                Instantiate(Manager_CommonGroup.instance.homeBannerObj_public);
            }
        }
    }

    /// <summary>
    /// 今いるシーンの（ビルド）番号を取得する
    /// </summary>
    /// <returns>今いるシーンの（ビルド）番号</returns>
    public int get_nowSceneId() { return SceneManager.GetActiveScene().buildIndex; }

#if false
    /// <summary>
    /// 先読みしたシーンを破棄する
    /// </summary>
    /// <returns>破棄した:true　破棄しない/できない:false</returns>
    private bool SceneUnload()
    {
        if (PreLoadedSceneID != -1)
        {
            devlog.log("先読みしていたシーンID :「" + PreLoadedSceneID + "」 を破棄します。（まだ破棄できないよ）");
            SceneManager.UnloadSceneAsync(PreLoadedSceneID);//先読みしたシーンを破棄
            PreLoadedSceneID = -1;//先読み情報を初期化
            return true;
        }
        else
        {
            devlog.log("先読みしているシーンが存在しません！");
            return false;
        }
    }
#endif
}