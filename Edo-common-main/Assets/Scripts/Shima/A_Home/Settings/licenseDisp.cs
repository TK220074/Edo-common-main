using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class licenseDisp : MonoBehaviour
{
    [SerializeField]private TextAsset licenseText;
    [SerializeField]private TextMeshProUGUI content;
    [SerializeField]private Scrollbar scrollBar;
    //[SerializeField] private coverPatternBG coverPattern;

    // Start is called before the first frame update
    void Start()
    {
        //coverPattern.setCoverPatternBG();
        content.text = licenseText.ToString();
        scrollBar.value = 1.0f;
    }
}
