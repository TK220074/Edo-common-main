using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class Battlemanager : Singleton<Battlemanager>
{
    public def_player player { get; private set; }
    private DatabaseManager dataM;
    private Manager_CommonGroup commonM;

    [SerializeField] private Vector3 battleCam_pos; // バトル中のカメラ位置
    [SerializeField] private Vector3 battleCam_rot; // バトル中のカメラ角度

    [SerializeField, Header("UI")] public TextMeshProUGUI name_p;
    [SerializeField] public TextMeshProUGUI name_e;
    [SerializeField] public TextMeshProUGUI edoko_c;
    [SerializeField] public Slider timer;
    [SerializeField] private GameObject obj_edokkoPlaceButton;
    [SerializeField] private GameObject obj_yellButton;

    [SerializeField, Header("Object")] public GameObject Edokko;
    [SerializeField] private GameObject _obj_battleEffect; // 戦闘中のボカスカエフェクト
    public GameObject obj_battleEffect => _obj_battleEffect;

    [SerializeField, Header("UI_topInfo_komonPreview")] private Image[] img_komonPreview_BG; // 小紋地色
    [SerializeField]private Image[] img_komonPreview_main; // 小紋柄

    [SerializeField, Header("UI_EdokkoPlacing")] private TextMeshProUGUI edoko_p_f;
    [SerializeField] private TextMeshProUGUI edoko_p_g;
    [SerializeField] private TextMeshProUGUI edoko_p_b;
    [SerializeField] private TextMeshProUGUI[] text_edokkoPlacing_button; // エドッコ配置時のボタンに、タイプ名表記

    [SerializeField] private Transform enemy_rangeA;
    [SerializeField] private Transform enemy_rangeB;

    [SerializeField, Header("Database")] list_battle_stage battleStageList;
    [SerializeField] list_constFloat_battle _list_battleValiable;
    public list_constFloat_battle list_battleValiable => _list_battleValiable;
    [SerializeField] komonDataLIst komonDataL;
    [SerializeField] list_battle_player_randomTable list_Battle_Player_RandomTable;
    [SerializeField] list_constFloat_komonCreation list_ConstFloat_KomonCreation;

    private float time_limit; // 制限時間
    float now = 0f; // 経過時間

    /// <summary>
    /// 敵のエドッコ数
    /// </summary>
    public int enemyedoko { get; set; }
    /// <summary>
    /// プレイヤのエドッコ数
    /// </summary>
    public int playeredoko { get; set; }
    private int playeredoko_limit; // プレイヤのエドッコMax数
    public int battle_result { get; set; }
    public int support { get; set; }
    public bool AreYouRedy { get; private set; } = false;
    private List<Vector3> savedPositions = new List<Vector3>();
    public float ignoreRadius = 1f;
    bool battle_f;
    bool battle_s;
    bool battle_r;
    public int edo_ss { get; set; } = 0;

    float r;
    float g;
    float b;

    private List<Vector3> edokos = new List<Vector3>();
    private float radius = 1.5f;
    private float minDistance = 0.5f;

    private byte id_playerKomon; // 前画面で選択された小紋の番号
    private def_komon.komonLength playerKomonLength; // 前画面で選択された小紋の長さ
    private short id_battleArea; // 前画面で選択されたバトルエリアの番号
    private BattleStageEntity stageData; // 前画面で選択されたバトルエリアの情報
    private def_komon enemyKomon; // バトルエリアに対応する敵小紋の情報

    /// <summary>
    /// 応援ゲージの値
    /// </summary>
    public float num_yell { get; private set; }
    private float time_fromDoYell; // おうえん無操作判定をするためのカウンタ
    /// <summary>
    /// バトル終了したか・終了処理しているか
    /// </summary>
    public bool isEnd { get; private set; }


    [SerializeField] private GameObject textbox;


    // Start is called before the first frame update
    void Start()
    {
        obj_yellButton.SetActive(false);
        obj_edokkoPlaceButton.SetActive(true);

        time_limit = _list_battleValiable.list[3].num; // 制限時間

        commonM = Manager_CommonGroup.instance;
        player = commonM.saveM.saveData.playerInfo;
        dataM = commonM.dataM;

        // エドッコ配置時のボタン表記（タイプ名）
        for(int i = 0; i < 3; i++)
        {
            text_edokkoPlacing_button[i].SetTextAndExpandRuby(dataM.get_constString(45 + i) + dataM.get_constString(48));
        }

#if true
        // バトルチュートリアル終了しているか
        if (!player.isEndTutorial[1])
        {
            // 終了していない
            id_playerKomon = 0; // 選択小紋
            playerKomonLength = def_komon.komonLength.s; // 選択小紋の長さ
            id_battleArea = 0; // 選択バトルエリア
        }
        else
        {
            Destroy(textbox);
            id_playerKomon = (byte)commonM.tempValue[0]; // 選択小紋
            playerKomonLength = (def_komon.komonLength)commonM.tempValue[1]; // 選択小紋の長さ
            id_battleArea = commonM.tempValue[2]; // 選択バトルエリア
        }
#endif

        enemyKomon = komonDataL.list[battleStageList.list[id_battleArea].enemyKomonId];
        stageData = battleStageList.list[id_battleArea];
        num_yell = list_battleValiable.list[6].num; // 応援ゲージの初期値

        devlog.log("選択小紋" + id_playerKomon);
        devlog.log("選択小紋の長さ" + playerKomonLength);
        devlog.log("エリアID" + id_battleArea);

        // UIで表示する小紋プレビューの設定
        def_komon playerKomon = player.havingKomon[id_playerKomon];
        img_komonPreview_BG[0].color = playerKomon.get_colorNum_toRgb(def_komon.creatonTiming.ground);
        img_komonPreview_BG[1].color = enemyKomon.get_colorNum_toRgb(def_komon.creatonTiming.ground);
        img_komonPreview_main[0].sprite = dataM.imgList_pattern.list[playerKomon.get_patternId(def_komon.creatonTiming.ground)];
        img_komonPreview_main[1].sprite = dataM.imgList_pattern.list[enemyKomon.get_patternId(def_komon.creatonTiming.ground)];

#if DEBUG
        if (playerKomonLength == 0)
        {
            playerKomonLength = def_komon.komonLength.s;
        }
#endif
        // プレイヤのエドッコMAX数
        playeredoko_limit = (int)_list_battleValiable.list[(int)playerKomonLength - 1].num;
#if true
        float rand = 0f;
        for (int i = 0; i < list_Battle_Player_RandomTable.list.Count; i++)
        {
            if (player.get_playerRank(def_komon.creationPart.summary) <= list_Battle_Player_RandomTable.list[i].levelBorder)
            {
                rand = Random.Range(list_Battle_Player_RandomTable.list[i].min, list_Battle_Player_RandomTable.list[i].max);
                break;
            }
        }
        // もしランダムテーブルに該当しなかったら
        if (rand == 0f)
        {
            // 最高レベルとして扱う
            int n = list_Battle_Player_RandomTable.list.Count - 1;
            rand = Random.Range(list_Battle_Player_RandomTable.list[n].min, list_Battle_Player_RandomTable.list[n].max);
        }

        float qualityRate = playerKomon.get_komonPoint() / (list_ConstFloat_KomonCreation.list[36].num * list_ConstFloat_KomonCreation.list[56].num); // 出来栄えポイント獲得率
        float result = (float)playeredoko_limit * qualityRate * rand; // とりあえずのプレイヤエドッコ数

        // RGB各軍の人数＝（軍全体の人数）×（（そのRGB値）/（各RGB値の合計））
#endif

        enemyedoko = (int)Mathf.Floor(_list_battleValiable.list[(int)enemyKomon.get_length() - 1].num * (rand + list_battleValiable.list[24].num)); // 敵のエドッコ数

        name_e.SetTextAndExpandRuby(stageData.enemyName);
        name_p.text = player.get_name(def_player.playerNameKind.player);

        //Battle_redy();
        Battle_enemy();
        redybattle();
        
    }

    void redybattle()
    {

        id_playerKomon = (byte)commonM.tempValue[0];
        if (id_playerKomon < 0 && id_playerKomon > 11)
        {
            id_playerKomon = 0;
            id_playerKomon = (byte)Random.Range(0, 9);
        }
        else
        {
            //def_komon playerKomon = player.havingKomon[id_playerKomon];
        }
        playerKomonLength = (def_komon.komonLength)commonM.tempValue[1]; // 選択小紋の長さ
        float rand = 0f;
        for (int i = 0; i < list_Battle_Player_RandomTable.list.Count; i++)
        {
            if (player.get_playerRank(def_komon.creationPart.summary) <= list_Battle_Player_RandomTable.list[i].levelBorder)
            {
                rand = Random.Range(list_Battle_Player_RandomTable.list[i].min, list_Battle_Player_RandomTable.list[i].max);
                break;
            }
        }
        // もしランダムテーブルに該当しなかったら
        if (rand == 0f)
        {
            // 最高レベルとして扱う
            int n = list_Battle_Player_RandomTable.list.Count - 1;
            rand = Random.Range(list_Battle_Player_RandomTable.list[n].min, list_Battle_Player_RandomTable.list[n].max);
        }

        float qualityRate = 25f / (list_ConstFloat_KomonCreation.list[36].num * list_ConstFloat_KomonCreation.list[56].num); // 出来栄えポイント獲得率
        float result = (float)playeredoko_limit * qualityRate * rand; // とりあえずのプレイヤエドッコ数

        def_komon data = player.havingKomon[id_playerKomon];
        Color color_c = data.get_colorNum_toRgb(def_komon.creatonTiming.ground);

        float max = (color_c.r + color_c.g + color_c.b);
        r = Mathf.Floor((color_c.r / max) * playeredoko_limit);
        g = Mathf.Floor((color_c.g / max) * playeredoko_limit);
        b = Mathf.Floor((color_c.b / max) * playeredoko_limit);
        playeredoko_limit = (int)Mathf.Floor(r + g + b);

        if (playeredoko_limit >= 33)
        {
            playeredoko_limit = 33;
            int random1 = Random.Range(0, 34);
            int random2 = Random.Range(0, 34 - random1);
            int random3 = 33 - random1 - random2;

            r = random1;
            g = random2;
            b = random3;
        }
        if (playeredoko_limit<0)
        {

            int demo= Random.Range(30, 39);

            playeredoko_limit = demo;
            int random1 = Random.Range(0, demo);
            int random2 = Random.Range(0, demo - random1);
            int random3 = demo - random1 - random2 - 1;

            r = random1;
            g = random2;
            b = random3;
        }
        if (playeredoko_limit<30)
        {

            int demo= 30;

            playeredoko_limit = demo;
            int random1 = Random.Range(0, demo);
            int random2 = Random.Range(0, demo - random1);
            int random3 = demo - random1 - random2 - 1;

            r = random1;
            g = random2;
            b = random3;
        }

        edoko_p_f.text = $"×{r}";
        edoko_p_g.text = $"×{g}";
        edoko_p_b.text = $"×{b}";
        Debug.Log("選択小紋ID"+commonM.tempValue[0]);
        Debug.Log("選択小紋ID2" + id_playerKomon);
        devlog.log($"プレイヤエドッコ数：{playeredoko_limit}");
        devlog.log("void Start() 終了");
    }
    /*public void summon_satus(byte x)
    {
        edo_ss = x;
    }*/
    void Battle_redy(Vector3 summon)
    {
        if (playeredoko < playeredoko_limit)
        {
            battle_f = false;
            int spawned = 0;

            if (r > 0f && edo_ss == 1)
            {
                if (r > 11)
                {
                    while (spawned < 11)
                    {
                        if (r != 0)
                        {
                            if (battle_f == false)
                            {
                                GameObject instance = Instantiate(Edokko, summon, Quaternion.Euler(-90, 0, 0));
                                instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
                                Edoko edoko = instance.GetComponent<Edoko>();
                                edoko.Edokotype = Edoko.edokotype.flame;
                                playeredoko++;
                                // 生成済みリストに追加
                                edokos.Add(summon);
                                battle_f = true;
                                spawned++;
                                r--;
                                continue;
                            }
                            Vector3 randomPosition = GetRandomPositionInCircle(summon);
                            if (Mathf.Abs(randomPosition.x) <= 5 && randomPosition.y >= 10 && randomPosition.y <= 15)
                            {

                                // 生成済みオブジェクトとの距離を確認
                                if (IsPositionValid(randomPosition))
                                {
                                    // オブジェクトを生成
                                    GameObject instance = Instantiate(Edokko, randomPosition, Quaternion.Euler(-90, 0, 0));
                                    instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
                                    Edoko edoko = instance.GetComponent<Edoko>();
                                    edoko.Edokotype = Edoko.edokotype.flame;
                                    playeredoko++;
                                    // 生成済みリストに追加
                                    edokos.Add(randomPosition);
                                    r--;
                                    spawned++;
                                }
                            }
                        }
                    }
                }
                else
                {


                    while (spawned < r)
                    {
                        if (battle_f == false)
                        {
                            GameObject instance = Instantiate(Edokko, summon, Quaternion.Euler(-90, 0, 0));
                            instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
                            Edoko edoko = instance.GetComponent<Edoko>();
                            edoko.Edokotype = Edoko.edokotype.flame;
                            playeredoko++;
                            // 生成済みリストに追加
                            edokos.Add(summon);
                            battle_f = true;
                            spawned++;
                            continue;
                        }
                        Vector3 randomPosition = GetRandomPositionInCircle(summon);
                        if (Mathf.Abs(randomPosition.x) <= 5 && randomPosition.y >= 10 && randomPosition.y <= 15)
                        {

                            // 生成済みオブジェクトとの距離を確認
                            if (IsPositionValid(randomPosition))
                            {
                                // オブジェクトを生成
                                GameObject instance = Instantiate(Edokko, randomPosition, Quaternion.Euler(-90, 0, 0));
                                instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
                                Edoko edoko = instance.GetComponent<Edoko>();
                                edoko.Edokotype = Edoko.edokotype.flame;
                                playeredoko++;
                                // 生成済みリストに追加
                                edokos.Add(randomPosition);

                                spawned++;
                            }
                        }
                    }
                    r = 0;
                }
            }
            else if (g > 0f && edo_ss == 2)
            {
                if (g > 11)
                {
                    while (spawned < 11)
                    {
                        if (g != 0)
                        {
                            if (battle_f == false)
                            {
                                GameObject instance = Instantiate(Edokko, summon, Quaternion.Euler(-90, 0, 0));
                                instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
                                Edoko edoko = instance.GetComponent<Edoko>();
                                edoko.Edokotype = Edoko.edokotype.grass;
                                playeredoko++;
                                // 生成済みリストに追加
                                edokos.Add(summon);
                                battle_f = true;
                                spawned++;
                                g--;
                                continue;
                            }
                            Vector3 randomPosition = GetRandomPositionInCircle(summon);
                            if (Mathf.Abs(randomPosition.x) <= 5 && randomPosition.y >= 10 && randomPosition.y <= 15)
                            {

                                // 生成済みオブジェクトとの距離を確認
                                if (IsPositionValid(randomPosition))
                                {
                                    // オブジェクトを生成
                                    GameObject instance = Instantiate(Edokko, randomPosition, Quaternion.Euler(-90, 0, 0));
                                    instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
                                    Edoko edoko = instance.GetComponent<Edoko>();
                                    edoko.Edokotype = Edoko.edokotype.grass;
                                    playeredoko++;
                                    // 生成済みリストに追加
                                    edokos.Add(randomPosition);
                                    g--;
                                    spawned++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    while (spawned < g)
                    {
                        if (battle_f == false)
                        {
                            GameObject instance = Instantiate(Edokko, summon, Quaternion.Euler(-90, 0, 0));
                            instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
                            Edoko edoko = instance.GetComponent<Edoko>();
                            edoko.Edokotype = Edoko.edokotype.grass;
                            playeredoko++;
                            // 生成済みリストに追加
                            edokos.Add(summon);
                            battle_f = true;
                            spawned++;
                            continue;
                        }
                        Vector3 randomPosition = GetRandomPositionInCircle(summon);
                        if (Mathf.Abs(randomPosition.x) <= 5 && randomPosition.y >= 10 && randomPosition.y <= 15)
                        {

                            // 生成済みオブジェクトとの距離を確認
                            if (IsPositionValid(randomPosition))
                            {
                                // オブジェクトを生成
                                GameObject instance = Instantiate(Edokko, randomPosition, Quaternion.Euler(-90, 0, 0));
                                instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
                                Edoko edoko = instance.GetComponent<Edoko>();
                                edoko.Edokotype = Edoko.edokotype.grass;
                                playeredoko++;
                                // 生成済みリストに追加
                                edokos.Add(randomPosition);

                                spawned++;
                            }
                        }
                    }
                    g = 0;
                }
            }
            else if (b > 0f && edo_ss == 3)
            {
                    if (b > 11)
                    {
                        while (spawned < 11)
                        {
                            if (b != 0)
                            {
                                if (battle_f == false)
                                {
                                    GameObject instance = Instantiate(Edokko, summon, Quaternion.Euler(-90, 0, 0));
                                    instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.blue);
                                    Edoko edoko = instance.GetComponent<Edoko>();
                                    edoko.Edokotype = Edoko.edokotype.water;
                                    playeredoko++;
                                    // 生成済みリストに追加
                                    edokos.Add(summon);
                                    battle_f = true;
                                    spawned++;
                                    b--;
                                    continue;
                                }
                                Vector3 randomPosition = GetRandomPositionInCircle(summon);
                                if (Mathf.Abs(randomPosition.x) <= 5 && randomPosition.y >= 10 && randomPosition.y <= 15)
                                {

                                    // 生成済みオブジェクトとの距離を確認
                                    if (IsPositionValid(randomPosition))
                                    {
                                        // オブジェクトを生成
                                        GameObject instance = Instantiate(Edokko, randomPosition, Quaternion.Euler(-90, 0, 0));
                                        instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.blue);
                                        Edoko edoko = instance.GetComponent<Edoko>();
                                        edoko.Edokotype = Edoko.edokotype.water;
                                        playeredoko++;
                                        // 生成済みリストに追加
                                        edokos.Add(randomPosition);
                                        b--;
                                        spawned++;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        while (spawned < b)
                        {
                            if (battle_f == false)
                            {
                                GameObject instance = Instantiate(Edokko, summon, Quaternion.Euler(-90, 0, 0));
                                instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.blue);
                                Edoko edoko = instance.GetComponent<Edoko>();
                                edoko.Edokotype = Edoko.edokotype.water;
                                playeredoko++;
                                // 生成済みリストに追加
                                edokos.Add(summon);
                                battle_f = true;
                                spawned++;
                                continue;
                            }
                            Vector3 randomPosition = GetRandomPositionInCircle(summon);
                            if (Mathf.Abs(randomPosition.x) <= 5 && randomPosition.y >= 10 && randomPosition.y <= 15)
                            {

                                // 生成済みオブジェクトとの距離を確認
                                if (IsPositionValid(randomPosition))
                                {
                                    // オブジェクトを生成
                                    GameObject instance = Instantiate(Edokko, randomPosition, Quaternion.Euler(-90, 0, 0));
                                    instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.blue);
                                    Edoko edoko = instance.GetComponent<Edoko>();
                                    edoko.Edokotype = Edoko.edokotype.water;
                                    playeredoko++;
                                    // 生成済みリストに追加
                                    edokos.Add(randomPosition);

                                    spawned++;
                                }
                            }
                        }
                        b = 0;
                    }
            }
        }
        if (r <= 0 && g <= 0 && b <= 0)
        {
            playeredoko_limit = playeredoko;
        }
        edoko_p_f.text = $"×{r}";
        edoko_p_g.text = $"×{g}";
        edoko_p_b.text = $"×{b}";
    }

    void Battle_enemy()
    {
        List<Vector3> savedPositions_e = new List<Vector3>();
        Color color_c = enemyKomon.get_colorNum(def_komon.creatonTiming.ground); // 敵の小紋色はRGB基準で設定されているので_toRgbで取得しないようにしている
        float max = (color_c.r + color_c.g + color_c.b);
        r = Mathf.Floor((color_c.r / max) * enemyedoko);
        g = Mathf.Floor((color_c.g / max) * enemyedoko);
        b = Mathf.Floor((color_c.b / max) * enemyedoko);
        r = r / 2;
        g = g / 2;
        b = b / 2;
        enemyedoko = (int)(r + g + b);
        devlog.log($"敵のエドッコ数：{enemyedoko}");
        for (int enemyz = 0; enemyz < enemyedoko; enemyz++)
        {
            float x = Random.Range(enemy_rangeA.position.x, enemy_rangeB.position.x);
            float y = Random.Range(enemy_rangeA.position.y, enemy_rangeB.position.y);

            if (r > 0f)
            {
                GameObject instance = Instantiate(Edokko, new Vector3(x, y, 0), Quaternion.Euler(-90, 0, 0));
                instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
                Edoko edoko = instance.GetComponent<Edoko>();
                edoko.enemyor = true;
                edoko.Edokotype = Edoko.edokotype.flame;
                r--;
            }
            else if (g > 0f)
            {
                GameObject instance = Instantiate(Edokko, new Vector3(x, y, 0), Quaternion.Euler(-90, 0, 0));
                g--;
                instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
                Edoko edoko = instance.GetComponent<Edoko>();
                edoko.enemyor = true;
                edoko.Edokotype = Edoko.edokotype.grass;
            }
            else if (b > 0f)
            {
                GameObject instance = Instantiate(Edokko, new Vector3(x, y, 0), Quaternion.Euler(-90, 0, 0));
                instance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.blue);
                Edoko edoko = instance.GetComponent<Edoko>();
                edoko.enemyor = true;
                edoko.Edokotype = Edoko.edokotype.water;
                b--;

            }
            //def_komon data = player.havingKomon[m.tempValue[0]];
            //devlog.log(data);
        }
    }
    void edokosummon()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                devlog.log(hit.point);
                if (Mathf.Abs(hit.point.x) <= 5 && hit.point.y >= 10 && hit.point.y <= 15)
                {
                    if (Mathf.Abs(hit.point.z) < 0.1f)
                    {
                        if (hit.collider.name == "Edokko")
                        {

                        }
                        else
                        {
                            savedPositions.Add(hit.point);
                            Battle_redy(hit.point);
                            commonM.audioM.SE_Play(65); // 配置時の音
                        }
                    }

                }
            }
        }
    }
        

    private bool IsPositionInRange(Vector3 position)
    {
        foreach (Vector3 savedPosition in savedPositions)
        {
            // 距離を計算
            if (Vector3.Distance(position, savedPosition) <= ignoreRadius)
            {
                return true; // 一定範囲内にある場合は無視
            }
        }
        return false; // 範囲外の場合は処理を続行
    }

    Vector3 GetRandomPositionInCircle(Vector3 summon_cen)
    {
        // 円内のランダムな位置を取得（極座標を利用）
        float angle = Random.Range(0, Mathf.PI * 2); // ランダムな角度
        float distance = Random.Range(0, radius);   // ランダムな半径（均等な分布）
        float x = summon_cen.x + Mathf.Cos(angle) * distance;
        float y = summon_cen.y + Mathf.Sin(angle) * distance;

        return new Vector3(x, y, 0);
    }


    bool IsPositionValid(Vector3 position)
    {
        // すでに生成されたオブジェクトの位置と比較
        foreach (Vector3 spawnedPosition in edokos)
        {
            if (Vector3.Distance(position, spawnedPosition) < minDistance)
            {
                return false; // 最小距離未満なら無効
            }
        }
        return true; // 有効な位置
    }

    /// <summary>
    /// おうえんボタン押下時の、おうえん値加算
    /// </summary>
    public void button_yell()
    {
        commonM.audioM.SE_Play(44);
        float addNum = Random.Range(list_battleValiable.list[7].num, list_battleValiable.list[8].num); // おうえん値加算
        if (num_yell + addNum < 100f)
        {
            num_yell += addNum;
        }
        else
        {
            num_yell = 100f;
        }
        time_fromDoYell = list_battleValiable.list[25].num;
    }

    // Update is called once per frame
    void Update()
    {
        edoko_c.text = $"{playeredoko.ToString("000")} VS {enemyedoko.ToString("000")}";

        /*if (r==null)
        {
            redybattle();
            Debug.Log(r);
        }*/
        if (Input.GetMouseButton(0))
        {
            Destroy(textbox);
        }
        if (!AreYouRedy)
        {
            edokosummon();
            if (playeredoko == playeredoko_limit)
            {
                // バトル開始
                AreYouRedy = true;
                obj_yellButton.SetActive(true); // 応援ボタン表示
                obj_edokkoPlaceButton.SetActive(false);
                commonM.audioM.SE_Play(62); // エドッコ軍勢SE
                commonM.audioM.SE_Play(47); // バトル開始SE
                commonM.audioM.BGM_Play(false, 31);

                // カメラ操作
                mainCamera cam = mainCamera.instance;
                cam.moveCamera(battleCam_pos);
                cam.rotateCamera(battleCam_rot);
            }
        }
        else
        {
            now += Time.deltaTime;
            float t = now / time_limit;
            timer.value = Mathf.Lerp(100f, 0f, t);

            // おうえん値減少
            time_fromDoYell -= Time.deltaTime;
            if(time_fromDoYell < 0f)
            {
                time_fromDoYell = 0f;
                num_yell -= list_battleValiable.list[12].num * Time.deltaTime;
                if (num_yell < 0f)
                {
                    num_yell = 0f;
                }
            }

            if (enemyedoko == 0)
            {
                // 勝ち
                endProcess_win();
            }
            else if (playeredoko == 0)
            {
                // 負け
                endProcess_lose();
            }
            else if (timer.value <= 0f)
            {
                // 時間切れ・残りエドッコ数で勝負
                if (playeredoko > enemyedoko)
                {
                    // 勝ち
                    endProcess_win();
                }
                else if (playeredoko == enemyedoko)
                {
                    // 引き分け（負け）
                    devlog.log("引き分け");
                    endProcess_lose();
                }
                else
                {
                    // 負け
                    endProcess_lose();
                }
            }
        }
    }



    /// <summary>
    /// バトル終了時の処理（勝ち）
    /// </summary>
    void endProcess_win()
    {
        if (!isEnd)
        {
            isEnd = true;

            devlog.log("勝ち");

            obj_yellButton.SetActive(false);

            commonM.audioM.SE_Play(53); // バトル終了SE
            commonM.audioM.SE_Play(55); // 勝ちSE
            commonM.audioM.BGM_Play(false, 32);// リザルトBGM

            int nextSceneId = 4; // 遷移先シーン番号・現時点ではマップ
            string[] getItems; // 獲得アイテム
            int getMoney = 0; // 獲得賞金

            // バトルチュートリアル終了しているか
            if (!player.isEndTutorial[1])
            {
                // 終了していない
                player.isEndTutorial[1] = true; // バトルTutorial終了フラグ
            }

            // バトルパートの通常終了処理

            // 初回クリアか？
            if (!player.get_battle_areaClear((ushort)id_battleArea))
            {
                player.set_battle_areaClear((ushort)id_battleArea, true); // クリアフラグ

                getMoney = stageData.getMoney_first; // お金
                getItems = stageData.getItemId_first.Split(','); // 獲得アイテム

                // クリア後のシナリオあるか？
                if (battleStageList.list[id_battleArea].scenarioId_afterClear != -1)
                {
                    commonM.id_startScenario = (ushort)stageData.scenarioId_afterClear; // 再生シナリオ指定
                    nextSceneId = commonM.dataM.get_nextSceneId(0, 1); // 遷移先にシナリオシーンを指定・タイトルシーンのnextSceneIdを参照している
                }
            }
            else
            {
                getMoney = stageData.getMoney_after; // お金ゲット
                getItems = stageData.getItemId_after.Split(','); // 獲得アイテム
            }

            // お金獲得
            if (getMoney > 0)
            {
                player.set_money(getMoney); // お金ゲット
                commonM.audioM.SE_Play(57); // お金獲得SE
            }
            getItemsCheck(getItems); // アイテム獲得
            commonM.sceneChanger.SceneChange(nextSceneId); // 指定シーンへ移動
        }
    }

    /// <summary>
    /// バトル終了時の処理（負け）
    /// </summary>
    void endProcess_lose()
    {
        if (!isEnd)
        {
            isEnd = true;

            devlog.log("負け");

            obj_yellButton.SetActive(false);

            commonM.audioM.SE_Play(53); // バトル終了SE
            commonM.audioM.SE_Play(54); // 負けSE
            commonM.audioM.BGM_Play(false, 32); // リザルトBGM
            int nextSceneId = 4;

            // バトルチュートリアル終了しているか
            if (!player.isEndTutorial[1])
            {
                // チュートリアルで負けた場合
                // （とりあえずタイトル画面へ）
                nextSceneId = 0;
            }
            else
            {

            }

            commonM.sceneChanger.SceneChange(nextSceneId); // マップシーンへ
        }
    }

    /// <summary>
    /// アイテム獲得処理
    /// </summary>
    /// <param name="itemsId">獲得するアイテム番号のstring配列</param>
    void getItemsCheck(string[] itemsId)
    {
        // 獲得アイテム無しの-1が指定されていないかチェック
        if (!itemsId.Contains("-1"))
        {
            for (int i = 0; i < itemsId.Length; i++)
            {
                devlog.log($"GetItem!：{itemsId[i]}");
                player.set_havingItem(ushort.Parse(itemsId[i]), 1);
            }
            commonM.audioM.SE_Play(56); // アイテム獲得SE
        }
    }
}