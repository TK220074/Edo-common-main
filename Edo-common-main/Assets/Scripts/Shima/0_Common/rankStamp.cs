using UnityEngine;

public class rankStamp : MonoBehaviour
{
    [SerializeField]private UnityEngine.UI.Image img;
    [SerializeField]private SpriteList spriteList;
    [SerializeField]private ColorList rankColorList;

    /// <summary>
    /// ランクアイコンを設定する
    /// </summary>
    /// <param name="rank">表示させるランク</param>
    public void setRangImg(def_komon.komonRank rank) 
    {
        img.sprite = spriteList.list[(int)rank];
        img.color = rankColorList.list[(int)rank];
        this.transform.eulerAngles = new Vector3(0, 0, Random.Range(-15, 15));
    }
}
