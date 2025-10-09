using UnityEngine;

/// <summary>
/// ピュアクラスのSingletonクラス
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T Instance;
    protected bool isOnlyOne;

    public static T instance
    {
        get
        {
            // 値が参照されたタイミングで判定
            if (Instance == null)
            {
                // nullだった場合は全オブジェクトを探索
                // 名前が一致するクラスがあった場合は取得する
                Instance = FindObjectOfType<T>();
            }
            return Instance;
        }
    }

    // 継承先でもAwakeを呼び出したい場合は，overrideする
    protected virtual void Awake()
    {
        // 既に同一名のクラスが存在していた場合
        if (instance != this)
        {
            // ゲームオブジェクトごと削除
            Destroy(gameObject);
        }
        else
        {
            isOnlyOne = true;
        }
    }
}