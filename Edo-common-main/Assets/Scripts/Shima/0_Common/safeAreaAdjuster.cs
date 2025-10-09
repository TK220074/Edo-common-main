using UnityEngine;

/// <summary>
/// セーフエリアの矩形を調整する
/// https://kingmo.jp/kumonos/unity-safearea-ios-android/
/// </summary>
public class safeAreaAdjuster : MonoBehaviour
{
#if false
    //セーフエリアに合わせたい箇所をtrueにする。
    [SerializeField] bool left;
    [SerializeField] bool right;
    [SerializeField] bool top;
    [SerializeField] bool bottom;
#endif

    private void Start()
    {

        var panel = GetComponent<RectTransform>();
        var area = Screen.safeArea;

        var anchorMin = area.position;
        var anchorMax = area.position + area.size;

#if false
        if (left) anchorMin.x /= Screen.width;
        else anchorMin.x = 0;

        if (right) anchorMax.x /= Screen.width;
        else anchorMax.x = 1;

        if (bottom) anchorMin.y /= Screen.height;
        else anchorMin.y = 0;

        if (top) anchorMax.y /= Screen.height;
        else anchorMax.y = 1;
#else
        anchorMin.x /= Screen.width;
        anchorMax.x /= Screen.width;

        anchorMin.y /= Screen.height;
        anchorMax.y /= Screen.height;
#endif

        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
    }
}