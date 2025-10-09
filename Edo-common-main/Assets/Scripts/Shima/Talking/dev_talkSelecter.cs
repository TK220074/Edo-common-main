using TMPro;
using UnityEngine;

/// <summary>
/// 会話シナリオの指定と実行
/// </summary>
public class dev_talkSelecter : MonoBehaviour
{
    [SerializeField]private GameObject obj_numInputParent;
    [SerializeField] private TMP_InputField inputField;
    
    // Start is called before the first frame update
    public void talkStart()
    {
        int useScenarioId = int.Parse(inputField.text);
        if (messageWindow.instance.setScenario(useScenarioId))
        {
            Destroy(obj_numInputParent);
        }
    }
}
