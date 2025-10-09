using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class button_cross_windowClosing : MonoBehaviour
{
    /// <summary>
    /// 指定したGameObjectの非表示方法はDestroyか・falseならSetActive(false)
    /// </summary>
    [SerializeField] private bool doDestroy;
    /// <summary>
    /// 非表示にしたいGameObject
    /// </summary>
    [SerializeField] private GameObject[] closingObjs;

    private AudioManager audioM;

    void Start()
    {
        audioM = Manager_CommonGroup.instance.audioM;
    }

    /// <summary>
    /// closingObjsをdoDestroyの値に応じて非表示にする
    /// </summary>
    public void close()
    {
        audioM.SE_Play(AudioManager.WhichSE.Cancel);

        if (doDestroy)
        {
            foreach(GameObject obj in closingObjs)
            {
                Destroy(obj);
            }
        }
        else
        {
            foreach (GameObject obj in closingObjs)
            {
                obj.SetActive(false);
            }
        }
    }
}
