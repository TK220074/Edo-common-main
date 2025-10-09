using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 各バトルエリアの情報Entity
/// </summary>

[System.Serializable]
public class BattleStageEntity
{
    //public byte ID;
    /// <summary>
    /// バトルステージのシーン番号
    /// </summary>
    //public byte battleSceneId;
    /// <summary>
    /// そのエリアの名前
    /// </summary>
    public string name;
    /// <summary>
    /// エリアの説明文
    /// </summary>
    public string explain;
    /// <summary>
    /// そのエリアに工房があるか
    /// </summary>
    public bool hasWorkspace;
    /// <summary>
    /// 敵の名前
    /// </summary>
    public string enemyName;
    /// <summary>
    /// 敵が使用する小紋のID
    /// </summary>
    public byte enemyKomonId;
    /// <summary>
    /// バトル開始前のシナリオ番号・-1=無い・シナリオの最後で必ずバトルシーン(10)へ遷移すること
    /// </summary>
    public short scenarioId_beforeStart;
    /// <summary>
    /// バトルクリア後のシナリオ番号・-1=無い
    /// </summary>
    public short scenarioId_afterClear;
    /// <summary>
    /// 初回クリア時の獲得アイテムID
    /// </summary>
    public string getItemId_first;
    /// <summary>
    /// 2周目以降の獲得アイテムID
    /// </summary>
    public string getItemId_after;
    /// <summary>
    /// 初回クリア時の獲得金
    /// </summary>
    public int getMoney_first;
    /// <summary>
    /// 2周目以降の獲得金
    /// </summary>
    public int getMoney_after;
    /// <summary>
    /// ていさつ で手に入るアイテムの候補
    /// </summary>
    public string getItemId_reconnaissance; 
}
