using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 色糊づくりのアレコレ
/// </summary>
public class manager_B_colorPaste : Singleton<manager_B_colorPaste>
{
    [SerializeField] private OneforAll_manager manager_dyeing;
    [SerializeField] private Image img_paste;

    [SerializeField, Header("Database")] private list_constFloat_komonCreation list_ConstFloat_KomonCreation;
    [SerializeField] private list_dyeInfo list_DyeInfo;

    private float nowAddAmount; // 瞬間投入量
    private Color addCmykCol = new Color();
    /// <summary>
    /// 各染料の累計投入量
    /// </summary>
    public float[] inputAmount { get; private set; } = new float[3];
    /// <summary>
    /// 完成した色糊のCMYK
    /// </summary>
    public Color resultCmykCol { get; private set; } = new Color();

    private bool buttonDownNow; // ボタン押下しているか
    private float accel; // 投入の加速度
    private float speed; // 投入スピード
    private float pressTime; // 投入ボタン押下時間
    private short nowDyeId; // 選択された染料の、染料内での番号（アイテムID->染料内のID）
#if UNITY_EDITOR
    [SerializeField] private byte dealDyeNum; // 何個目の染料か
    public short[] selectedDyeId = new short[3]{ -1, -1, -1}; // 選択された染料のアイテム番号
#else
    private byte dealDyeNum; // 何個目の染料か
    public ushort selectedDyeId{get; private set;} // 選択中の染料のアイテム番号
#endif
    private float colNumPerGram; // 1グラムあたりのCMYK加算値

    private Manager_CommonGroup commonM;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        commonM = Manager_CommonGroup.instance;
        accel = list_ConstFloat_KomonCreation.list[1].num / list_ConstFloat_KomonCreation.list[3].num;
        colNumPerGram = list_ConstFloat_KomonCreation.list[4].num;
#if false
        // 何かしらの形で、選択された染料のアイテム番号を取得したい
        selectedDyeId[0] = commonM.tempValue[0];
        convertItemIdToDyeId(selectedDyeId[dealDyeNum]);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (buttonDownNow)
        {
            pressTime += Time.deltaTime;

            // 投入スピードの算出
            // 最大速度を超えていないか
            if(speed < list_ConstFloat_KomonCreation.list[1].num)
            {
                speed = accel * pressTime;
            }
            else
            {
                speed = list_ConstFloat_KomonCreation.list[1].num;
            }

            // 投入
            float num = 0f; // その瞬間におけるCMYK加算値の合計
            // 最大量の確認
            if (inputAmount[dealDyeNum] < list_ConstFloat_KomonCreation.list[57].num)
            {
                speed *= Time.deltaTime;
                inputAmount[dealDyeNum] += speed; // その染料の累計投入量
                nowAddAmount = speed; // 瞬間投入量

                // 各染料内でCMYKの占める割合に応じた、CMYK加算値
                num = nowAddAmount * colNumPerGram;
                addCmykCol.r = num * list_DyeInfo.list[nowDyeId].c;
                addCmykCol.g = num * list_DyeInfo.list[nowDyeId].m;
                addCmykCol.b = num * list_DyeInfo.list[nowDyeId].y;
                addCmykCol.a = num * list_DyeInfo.list[nowDyeId].k;

                resultCmykCol += addCmykCol;
                devlog.log($"NowCMYKCol: {resultCmykCol}");
            }
            else
            {
                inputAmount[dealDyeNum] = list_ConstFloat_KomonCreation.list[57].num;
                nowAddAmount = 0f;
                devlog.log("end input");
                button_inputPaste_up();
            }

            img_paste.color = commonM.get_cmykToRgb(resultCmykCol);
        }
        else
        {
            pressTime = 0f;
            nowAddAmount = 0f;
        }
    }

    public void button_inputPaste_down()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        StartCoroutine(commonM.audioM.BGM_Play(commonM.audioM.list_SE.list[26], false, 0, 2));
        buttonDownNow = true;
    }
    public void button_inputPaste_up()
    {
        commonM.audioM.BGM_Stop(false, 2);
        buttonDownNow = false;
    }

    public void button_end()
    {
        commonM.audioM.SE_Play(AudioManager.WhichSE.Done);
        manager_dyeing.color_k = resultCmykCol;
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// アイテム番号に対応する染料の、染料内でのIDに変換する・list_dyeInfoの参照用・染料が選択されたタイミングで実行
    /// </summary>
    /// <param name="itemId">染料のアイテム番号</param>
    private short convertItemIdToDyeId(ushort itemId)
    {
        for(short i = 0; i < list_DyeInfo.list.Count; i++)
        {
            if (list_DyeInfo.list[i].itemId == itemId)
            {
                nowDyeId = i;
                return i;
            }
        }
        return -1;
    }
}
