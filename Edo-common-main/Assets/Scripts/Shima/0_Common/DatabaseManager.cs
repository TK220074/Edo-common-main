using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  ゲーム内でよく使うデータたち
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    [SerializeField, Header("GameDataList")] private list_pattern _list_pattern;
    /// <summary>
    /// 型紙データ
    /// </summary>
    public list_pattern list_pattern => _list_pattern;

    [SerializeField] private SpriteList _imgList_pattern;
    /// <summary>
    /// 型紙画像
    /// </summary>
    public SpriteList imgList_pattern => _imgList_pattern;

    [SerializeField] list_items _list_items;
    /// <summary>
    /// アイテムデータ
    /// </summary>
    public list_items list_items => _list_items;

    [SerializeField] private SpriteList _imgList_items;
    /// <summary>
    /// アイテム画像（アイコン）
    /// </summary>
    public SpriteList imgList_items => _imgList_items;

    /// <summary>
    /// Excel上で設定した定数リスト
    /// </summary>
    [SerializeField, Header("CommonConstValue")] private list_constFloat constList_float;
    [SerializeField] private list_constString constList_string;
    [SerializeField] private list_wordsDictionary _list_WordsDictionary;
    public list_wordsDictionary list_WordsDictionary => _list_WordsDictionary;

    [SerializeField, Header("SceneInfo")] private list_sceneInfo _sceneInfo;
    /// <summary>
    /// シーン情報・次のシーンやBGMのIDなどを持つ。・nextSceneIdは、get_nextSceneId()で参照すること
    /// </summary>
    public list_sceneInfo sceneInfo => _sceneInfo;

    [SerializeField] private SpriteList _imgList_bg;
    /// <summary>
    /// 背景画像のリスト
    /// </summary>
    public SpriteList imgList_bg => _imgList_bg;

    [SerializeField, Header("Player")] private list_player_level _list_playerLevel;
    /// <summary>
    /// 職人ランク/工程レベル
    /// </summary>
    public list_player_level list_playerLevel => _list_playerLevel;

    [SerializeField] private list_player_title _list_playerTitle;
    /// <summary>
    /// 称号データ
    /// </summary>
    public list_player_title list_playerTitle => _list_playerTitle;

    [SerializeField, Header("Window")] private GameObject _window_question;
    /// <summary>
    /// 質問ウィンドウObj
    /// </summary>
    public GameObject window_question => _window_question;

    [SerializeField] private GameObject _window_nameInput;
    /// <summary>
    /// 文字入力ウィンドウObj
    /// </summary>
    public GameObject window_nameInput => _window_nameInput;



    /// <summary>
    /// Excel上で設定したゲーム内のFloat定数(?)を取得する
    /// </summary>
    /// <param name="id">定数の要素番号</param>
    /// <returns>定数リストのID番目にある値</returns>
    public float get_constFloat(int id) { return constList_float.list[id].num; }
    /// <summary>
    /// Excel上で設定したゲーム内のString定数(?)を取得する
    /// </summary>
    /// /// <param name="id">定数の要素番号</param>
    /// <returns>定数リストのID番目にある値</returns>
    public string get_constString(int id) { return constList_string.list[id].value; }

    /// <summary>
    /// 遷移先のシーン番号を取得する
    /// </summary>
    /// <param name="nowSceneId">遷移元のシーン番号</param>
    /// <param name="index">（遷移先候補が複数ある場合）その要素番号</param>
    /// <returns>遷移元のシーン番号と、指定した要素番号に基づく、遷移先のシーン番号</returns>
    public int get_nextSceneId(int nowSceneId, int index = 0)
    {
        int[] ids = Array.ConvertAll(_sceneInfo.list[nowSceneId].nextId.Split(','), s => int.TryParse(s, out var x) ? x : -1);
        return ids[index];
    }

    /// <summary>
    /// 指定したランク/レベルに関するレベルデータを取得する
    /// </summary>
    /// <param name="part">欲しいランク/レベルデータ種</param>
    /// <returns>指定したランク/レベルデータ</returns>
    public List<PlayerLevelEntity> get_LevelList(def_komon.creationPart part)
    {
        List<PlayerLevelEntity> list;
        switch (part)
        {
            case def_komon.creationPart.summary:
                list = list_playerLevel.level_player;
                break;

            case def_komon.creationPart.colorCreation:
                list = list_playerLevel.level_creation_colorCreation;
                break;

            case def_komon.creationPart.dyeing:
                list = list_playerLevel.level_creation_dyeing;
                break;

            case def_komon.creationPart.washing:
                list = list_playerLevel.level_creation_washing;
                break;

            default:
                list = list_playerLevel.level_player; // 警告回避
                break;
        }
        return list;
    }

    /// <summary>
    /// 次ランク/レベルまでの必要ポイントを取得する
    /// </summary>
    /// <param name="part">取得したいランク/レベル種</param>
    /// <param name="nowRank">どのレベル時点における、次レベルまでの必要経験値か（原則、現レベル）</param>
    /// <returns>次ランク/レベルまでの必要ポイント</returns>
    public float get_pointToNextLevel(def_komon.creationPart part, int nowRank)
    {
        // 現ランクとして自然数が入力されており、かつ 次レベルがレベルデータの範囲内におさまっているか
        if((nowRank > 0) && (nowRank < list_playerLevel.level_player.Count))
        {
            // [nowRank]＝現ランクの次のランクの要素番号
            float result = get_LevelList(part)[nowRank].total - Manager_CommonGroup.instance.saveM.saveData.playerInfo.get_playerRank_point(part);
            return result;
        }
        else
        {
            return -1f;
        }
    }
}
