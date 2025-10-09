using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ていさつ進捗を示すUIの操作
/// </summary>
public class teisatsuProgressInfo : MonoBehaviour
{
    [SerializeField] private list_battle_stage list_battleStage;
    [SerializeField] private TextMeshProUGUI text_areaName; // ていさつ先表示Text
    [SerializeField] private TextMeshProUGUI text_leftTime; // 残り時間表示Text
    [SerializeField] private float time_timeCheckSec = 5.0f; // 進捗表示の更新頻度
    [SerializeField] private Scrollbar scrollbar_partner;

    private float time_count;
    private Manager_CommonGroup commonM;
    
    // Start is called before the first frame update
    void Start()
    {
        commonM = Manager_CommonGroup.instance;

        // ていさつ中か
        if (commonM.saveM.saveData.playerInfo.doingReconnaissance)
        {
            string areaName = list_battleStage.list[commonM.saveM.saveData.playerInfo.id_reconnaissanceArea].name; // エリア名
            text_areaName.text = areaName + commonM.dataM.get_constString(34);
            reloadProgressInfo();
            this.gameObject.SetActive(true); // UI表示
        }
        else
        {
            this.gameObject.SetActive(false); // UI非表示
        }
    }

    // Update is called once per frame
    void Update()
    {
        // ていさつ中か
        if(commonM.saveM.saveData.playerInfo.doingReconnaissance)
        {
            time_count += Time.deltaTime;
            // 規定時間を超えたら情報表示を更新
            if (time_count >= time_timeCheckSec)
            {
                time_count = 0f;
                reloadProgressInfo();
            }
        }
    }

    /// <summary>
    /// ていさつ進捗表示を更新する
    /// </summary>
    private void reloadProgressInfo()
    {
        if(commonM == null)
        {
            commonM = Manager_CommonGroup.instance;
        }

        float leftT = commonM.saveM.saveData.playerInfo.get_teisatsuLeftTime(); // ていさつ残り時間

        // 残り時間表記
        // 残り時間が1分未満か
        if(leftT < 1f)
        {
            if(leftT <= 0f)
            {
                // 0分＝終了したか
                text_leftTime.text = commonM.dataM.get_constString(33);
            }
            else
            {
                // 0分表示になるので別表記にする
                text_leftTime.text = commonM.dataM.get_constString(32);
            }
        }
        else
        {
            text_leftTime.text = commonM.dataM.get_constString(30) + leftT.ToString("0") + commonM.dataM.get_constString(31); // 残り時間表示を更新
        }
        float rate = leftT / (float)commonM.saveM.saveData.playerInfo.time_reconnaissanceRequired; // 進捗率
        scrollbar_partner.value = rate; // 進捗率演出更新
    }
}
