/// <summary>
/// 各染料の、CMYK値の割合
/// </summary>

[System.Serializable]
public class dyeInfoEntity
{
    /// <summary>
    /// その染料のアイテム番号
    /// </summary>
    public ushort itemId;
    public float c;
    public float m;
    public float y;
    public float k;
}
