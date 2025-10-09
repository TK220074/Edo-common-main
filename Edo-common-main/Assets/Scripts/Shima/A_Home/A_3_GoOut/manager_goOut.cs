using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class manager_goOut : MonoBehaviour
{   
    [SerializeField, Header("Button_Shop")] private StringList list_shopName; // 店種と店名が交互に格納されているリスト
    [SerializeField] private SpriteList list_shopIcon; // 店のアイコン画像リスト
    [SerializeField] private TextMeshProUGUI[] buttonTexts;
    [SerializeField] private Image[] buttonIconImages;

    private AudioManager audioM;
    
    // Start is called before the first frame update
    void Start()
    {
        Manager_CommonGroup commonM = Manager_CommonGroup.instance;
        audioM = commonM.audioM;

        // ボタンText設定
        for (int i = 0; i < buttonTexts.Length; i++)
        {
            buttonTexts[i].SetTextAndExpandRuby(list_shopName.list[i]);
        }

        // ボタンアイコン設定
        for(int i = 0; i < buttonIconImages.Length; i++)
        {
            buttonIconImages[i].sprite = list_shopIcon.list[i];
        }
    }

    /// <summary>
    /// ショップボタンが押された時の処理
    /// </summary>
    /// <param name="buttonKind">押されたボタンに該当する店種の番号</param>
    public void shopButtonPressed(int buttonKind)
    {
        audioM.SE_Play(AudioManager.WhichSE.Done);

        Manager_CommonGroup m = Manager_CommonGroup.instance;
        m.tempValue[0] = (short)buttonKind;
        m.sceneChanger.SceneChange((byte)m.dataM.get_nextSceneId(m.sceneChanger.get_nowSceneId())); // シーン遷移
    }
}
