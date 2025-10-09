using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 実績解放をしたりする
/// </summary>
public class AchievementManager : MonoBehaviour
{
    private SaveDataManager save;
    private AudioManager audioM;

    [SerializeField, Header("for GetUI")] private GameObject getBarObj;//実績解放時に表示させるObj
    [SerializeField] private RectTransform canvas;//実績解放UIを表示させるCanvas
    [SerializeField, Range(0f, 10f)] private float UiDisplayTime = 3.0f;//実績解放UIを表示させてから消すまでの時間
    [SerializeField] private AudioClip getSeClip;//実績解放時のSE

    [SerializeField, Header("for DataBase")] private list_player_title _database;
    public list_player_title database => _database;
    [SerializeField] private SpriteList _iconList;
    public SpriteList iconList => _iconList;

    // Start is called before the first frame update
    void Start()
    {
        Manager_CommonGroup commonM = Manager_CommonGroup.instance;
        save = commonM.saveM;
        audioM = commonM.audioM;
    }

    /// <summary>
    /// 実績を解放する
    /// </summary>
    /// <param name="id">解放したい実績のID</param>
    /// <returns>実績を解放できたらtrue</returns>
    public bool OpenAchievement(int id)
    {
        if (id < _database.list.Count)
        {
            if (!save.saveData.playerInfo.get_havingTitle((byte)id))
            {
                //解放状況の格納
                save.saveData.playerInfo.set_havingTitle((byte)id, true);
                save.Save(save.saveData);

                //UI表示
                audioM.SE_Play(getSeClip);
                GameObject instObj = Instantiate(getBarObj, canvas);
                instObj.GetComponent<GetAchievementUI>().SetAchievementInfo(_iconList.list[id], _database.list[id].title, UiDisplayTime);
                devlog.log($"称号解放！ ID:{id}");
                return true;
            }
            else
            {
                devlog.logWarning($"ID {id} の実績は既に解放済です！");
            }
        }
        else
        {
            devlog.logError($"指定したID {id} は実績数の範囲外です！");
        }
        return false;
    }
}
