using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edoko : MonoBehaviour
{
    
    public bool enemyor;
    public bool battlenow = false;
    public bool battle_start = false;
    public status Status; 
    public edokotype Edokotype;
    public int Brutus = 0;
    public float rnd = 0f;
    public float speed { get; private set; }
    public float battletime { get; private set; }
    public float survival { get; private set; }
    public int support;
    public GameObject anothr;
    private Edoko anotheredoko;
    private float anotherdistance = 500f;

    Battlemanager bmm;

    public enum status
    {
        WAIT,
        STOP,
        FIND,
        BATTLE,
        ADD,
    };
    void statuschange(status next)
    {
        Status = next;
    }
    public enum edokotype
    {
        grass=1,
        flame=2,
        water=3
    };
    
    // Start is called before the first frame update
    void Start()
    {
        bmm = Battlemanager.instance;
        speed = Random.Range(bmm.list_battleValiable.list[4].num, bmm.list_battleValiable.list[5].num); // 移動スピード
        statuschange(status.WAIT);
        //Edokotype = edokotype.water;
    }

    // Update is called once per frame
    void Update()
    {
        switch (Status)
        {
            case status.WAIT:
                battle_reddy();
                break;
            case status.STOP:
                freecheck();
                break;
            case status.FIND:
                Findenemy();
                break;
            case status.BATTLE:
                Battleenemy();
                break;
            case status.ADD:
                Reinforcements();
                break;
        }
        
    }
    void battle_reddy()
    {
        if (bmm.AreYouRedy == true)
        {
            statuschange(status.STOP);
        }

    }

    void freecheck()
    {
        if (anothr == null)
        {
            Debug.Log("探してます？");
            GameObject[] objects = GameObject.FindGameObjectsWithTag("Edokko");
            anotherdistance = 500f;
            foreach (GameObject obj in objects)
            {
                if (obj == this.gameObject)
                {
                    Debug.Log("自分");
                    continue;
                }
                anotheredoko = obj.GetComponent<Edoko>();
                if (enemyor != anotheredoko.enemyor)
                {
                    float distance = Vector3.Distance(this.transform.position, obj.transform.position);
                    if (distance < anotherdistance)
                    {

                        if (!battlenow)
                        {
                            anotherdistance = distance;
                            anothr = obj;
                            continue;
                        }
                    }
                }
                else
                {
                    continue;
                    statuschange(status.ADD);
                }
                statuschange(status.FIND);
            }
        }
        else
        {
            statuschange(status.FIND);
        }
        
    }
    void Findenemy()
    {
        if (anotheredoko.battlenow == true)
        {
            anothr = null;
            statuschange(status.STOP);
        }
        else if (anotheredoko.battlenow == false)
        {
            if (anothr == null)
            {
                statuschange(status.STOP);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, anothr.transform.position, Time.deltaTime);
            }
        }
        else
        {
            statuschange(status.WAIT);
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Edoko>().enemyor == enemyor)
        {
            //battlenow = false;
        }
        else if (!battlenow)
        {
            battlenow = true;
            statuschange(status.BATTLE);
        }
        else if (Status == status.ADD)
        {

        }
    }
    public void Battleenemy()
    {
        GameObject effectObj;

        if (battle_start == false)
        {
            suevive();
            rnd = Random.Range(1f, 99f);
            battle_start = true;
            if (enemyor)
            {
                anotheredoko.battletime = battletime;

                // ボカスカエフェクトの生成
                effectObj = Instantiate(bmm.obj_battleEffect);
                effectObj.transform.parent = this.transform;
                effectObj.transform.position = Vector3.zero;
            }
            if (!enemyor)
            {
                battletime = Random.Range(bmm.list_battleValiable.list[20].num, bmm.list_battleValiable.list[21].num); // 戦闘時間
                                                                                                                       // 自分は敵側かつバトルチュートリアル終了していないか
                if (!bmm.player.isEndTutorial[1])
                {
                    // 敵は必ず負けるようにしたい
                    rnd = 0f;
                    return;
                }
            }
        }
        battletime -= Time.time;

        if (battletime <= -1f)
        {
            if (survival <= rnd)
            {
                if (enemyor == true)
                {
                    bmm.enemyedoko--;
                }
                else
                {
                    bmm.playeredoko--;
                }
                Destroy(this.gameObject);
                //StartCoroutine("judge");

            }
            else
            {
                statuschange(status.STOP);
                battle_start = false;
                battlenow = false;
                if (anothr == null)
                {
                    anothr = null;
                }
                else
                {
                    Vector3 directionToMove = (this.transform.position - anothr.transform.position).normalized;
                    this.transform.position += directionToMove * speed * Time.deltaTime;
                }

            }

            // 勝敗が決したときの歓声SE
            rnd = Random.Range(0, 3);
            Manager_CommonGroup.instance.audioM.SE_Play(58 + (int)rnd);
        }
    }

    IEnumerator judge()
    {
        yield return new WaitForSeconds(battletime);
    }
    void Reinforcements()
    {
        if (anothr==null)
        {
            freecheck();
            //statuschange(status.FIND);
        }
        //float addtime= Random.Range(0.80f, 2.70f);
        //battletime += addtime;
    }
    public void suevive()
    {
        // 勝率調整値
        float addPar = ((bmm.list_battleValiable.list[17].num - bmm.list_battleValiable.list[16].num) / 100f) * bmm.num_yell + bmm.list_battleValiable.list[16].num;
        survival = addPar;

        if (enemyor)
        {
            battletime = Random.Range(bmm.list_battleValiable.list[20].num, bmm.list_battleValiable.list[21].num); // 戦闘時間
            // 自分は敵側かつバトルチュートリアル終了していないか
            if (!bmm.player.isEndTutorial[1])
            {
                // 敵は必ず負けるようにしたい
                survival = 0f;
                return;
            }
        }

        // 勝率
        if (anotheredoko == null)
        {

        }
        else
        {
            if (anotheredoko.enemyor != enemyor)
            {
                if (Edokotype == anotheredoko.Edokotype)
                {
                    // 同種
                    survival *= bmm.list_battleValiable.list[28].num; // タイプ相性に応じた、勝率調整値の調整
                    survival += bmm.list_battleValiable.list[15].num; // 基準勝率
                }
                else if (Edokotype == edokotype.grass && anotheredoko.Edokotype == edokotype.water)
                {
                    // 有利
                    survival *= bmm.list_battleValiable.list[26].num; // タイプ相性に応じた、勝率調整値の調整
                    survival += bmm.list_battleValiable.list[13].num; // 基準勝率
                }
                else if (Edokotype == edokotype.water && anotheredoko.Edokotype == edokotype.flame)
                {
                    // 有利
                    survival *= bmm.list_battleValiable.list[26].num; // タイプ相性に応じた、勝率調整値の調整
                    survival += bmm.list_battleValiable.list[13].num; // 基準勝率
                }
                else if (Edokotype == edokotype.flame && anotheredoko.Edokotype == edokotype.grass)
                {
                    // 有利
                    survival *= bmm.list_battleValiable.list[26].num; // タイプ相性に応じた、勝率調整値の調整
                    survival += bmm.list_battleValiable.list[13].num; // 基準勝率
                }
                else
                {
                    // 不利
                    survival *= bmm.list_battleValiable.list[27].num; // タイプ相性に応じた、勝率調整値の調整
                    survival += bmm.list_battleValiable.list[14].num; // 基準勝率
                }
            }
        }
    }
}
