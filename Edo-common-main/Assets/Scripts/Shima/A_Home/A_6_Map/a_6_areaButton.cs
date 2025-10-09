using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class a_6_areaButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject[] iconObjs;

    private ushort areaId;
    private manager_areaSelect mapManager;

    void Start()
    {
        mapManager = manager_areaSelect.instance;
    }

    /// <summary>
    /// ボタンに表示するエリア名の書き換えと、ボタンにあるアイコンの表示/非表示
    /// </summary>
    /// <param name="areaName">そのエリアの名前</param>
    /// <param name="wasClear">クリア済のエリアか</param>
    /// <param name="hasWorkspace">そのエリアに工房があるか</param>
    /// <param name="hasEvent">そのエリアクリア時にイベント発生するか</param>
    public void setupButton(ushort id, string areaName, bool wasClear, bool hasWorkspace, bool hasEvent)
    {
        areaId = id;
        nameText.text = areaName;

        // クリアアイコンの表示/非表示
        if (wasClear != iconObjs[0].activeInHierarchy)
        {
            iconObjs[0].SetActive(wasClear);
        }

        // 工房アイコンの表示/非表示
        if (hasWorkspace != iconObjs[1].activeInHierarchy)
        {
            iconObjs[1].SetActive(hasWorkspace);
        }

        // イベント発生アイコンの表示/非表示
        // クリア前にのみ表示
        if (hasEvent && !wasClear)
        {
            if (iconObjs[2].activeInHierarchy == false)
            {
                iconObjs[2].SetActive(true);
            }
        }
        else
        {
            if (iconObjs[2].activeInHierarchy == true)
            {
                iconObjs[2].SetActive(false);
            }
        }
    }

    public void pressed()
    {
        mapManager.pressedAreaButton(areaId);
    }
}
