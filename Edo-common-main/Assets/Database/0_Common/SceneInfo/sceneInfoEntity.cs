[System.Serializable]
public class sceneInfoEntity
{
    //public byte ID;

    /// <summary>
    /// そのシーンの属するゲームパート
    /// </summary>
    public gamePart part;
    
    /// <summary>
    /// シーンの名称（バナー表示用）
    /// </summary>
    public string name;

    public sbyte bgId;

    /// <summary>
    /// 「もどる」選択時のシーンID
    /// </summary>
    public byte returnId;

    /// <summary>
    /// 次のシーンへ遷移する場合のシーンID・遷移先が複数ある場合カンマ区切りで記載し、get_nextSceneId()にてその要素番号を指定してシーン番号を取得する
    /// </summary>
    public string nextId;

    /// <summary>
    /// そのシーンで再生するBGM（無音 = -1）
    /// </summary>
    public sbyte bgmId;

    /// <summary>
    /// そのシーンで再生する環境音（無音 = -1）
    /// </summary>
    public sbyte envBgmId;

    public enum gamePart
    {
        other = 0, // 以下以外（タイトルなど）
        A = 1, // Home
        B = 2, // KomonCreation
        C = 3 // Battle
    }
}