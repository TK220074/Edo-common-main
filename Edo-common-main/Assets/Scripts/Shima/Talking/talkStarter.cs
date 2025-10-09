using UnityEngine;

/// <summary>
/// 会話シナリオの指定と実行
/// </summary>
public class talkStarter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 「使用するシナリオ番号」を、このシーンに来る前に何かしらの方法で id_startScenario に格納しておく
        int useScenarioId = Manager_CommonGroup.instance.id_startScenario;
        messageWindow.instance.setScenario(useScenarioId);
    }
}
