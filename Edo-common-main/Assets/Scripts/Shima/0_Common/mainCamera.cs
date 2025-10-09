using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainCamera : Singleton<mainCamera>
{
    public Camera cameraComponent { get; private set; }

    private Vector3 initialPos; // 起動時の初期位置・シーン遷移時に初期位置へ移動させる目的

    // 減衰比率
    [SerializeField, Header("CameraMove")] private float _attenRate = 3.0f; // 減衰率
    public float attenRate => _attenRate;
    [SerializeField] private float distanceToCharacter = 15f; // キャラとz距離を取る際の距離
    private bool letMove; // 移動処理して良いかフラグ
    private Vector3 destPos; // カメラ移動における目的地座標
    private bool letRotate;
    private Vector3 destRot;

    private Transform[] objsTrans; // カメラ方向を向かせるゲームオブジェクトたち
    private SceneChanger sceneM;

    // Start is called before the first frame update
    void Start()
    {
       if (isOnlyOne)
        {
            DontDestroyOnLoad(this.gameObject);
            cameraComponent = GetComponent<Camera>();
            sceneM = Manager_CommonGroup.instance.sceneChanger;

            initialPos = transform.position;
            resetLookCamObjs();
        }
    }

    void Update()
    {
        // 移動許可されているか
        if(letMove)
        {
            // https://qiita.com/yoship1639/items/9bf6f8ad080b3c496b12
            transform.position = Vector3.Lerp(transform.position, destPos, Time.deltaTime * attenRate);

            if(transform.position == destPos)
            {
                letMove = false;
            }
        }

        if (letRotate)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(destRot), Time.deltaTime * attenRate);
            if(transform.rotation.eulerAngles == destRot)
            {
                letRotate = false;
            }
        }

        // シーン遷移中でなく、かつカメラ方向を向かせるObjがあれば実行
        if(!sceneM.alreadyLoad && (objsTrans.Length > 0))
        {
            objsLookAtCamera();
        }
    }

    /// <summary>
    /// カメラを滑らかに移動させる
    /// </summary>
    /// <param name="toPos">移動先の座標</param>
    /// <param name="keepDistance">その座標から少し距離をとった場所(- distanceToCharacter)へ移動するか</param>
    public void moveCamera(Vector3 toPos, bool keepDistance = false)
    {
        if(keepDistance)
        {
            toPos.z -= distanceToCharacter;
        }
        destPos = toPos;
        letMove = true;
    }
    /// <summary>
    /// カメラを滑らかに回転させる
    /// </summary>
    /// <param name="toRot">回転先の座標</param>
    public void rotateCamera(Vector3 toRot)
    {

        destRot = toRot;
        letRotate = true;
    }

    /// <summary>
    /// カメラ位置の初期化・カメラ方向を向かせる画像の取得
    /// </summary>
    public void resetCamera()
    {
        if(transform.position != initialPos)
        {
            transform.position = initialPos;
        }

        resetLookCamObjs();
    }

    /// <summary>
    /// 2.5D画像をカメラの方へ向かせる
    /// </summary>
    private void objsLookAtCamera()
    {
        Vector3 camPos = transform.position;

        foreach (Transform objTrans in objsTrans)
        {
            camPos.y = objTrans.position.y; // y軸は回転させない

#if false
            // これだとz方向もカメラへ向くため、画像が反転してしまう
            transform.LookAt(camPos);
#else
            // 画像のz向きの関係で反転してしまわないようにする
            // https://detail.chiebukuro.yahoo.co.jp/qa/question_detail/q10142762860?__ysp=dW5pdHkgbG9va0F0IOWPjei7og%3D%3D
            objTrans.rotation = Quaternion.LookRotation(objTrans.position - camPos);
#endif
        }
    }
    /// <summary>
    /// カメラ方向を向く背景画像を取得する
    /// </summary>
    private void resetLookCamObjs()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("obj_lookCam");
        objsTrans = new Transform[objs.Length];
        for (int i = 0; i < objs.Length; i++)
        {
            objsTrans[i] = objs[i].transform;
        }
    }
}