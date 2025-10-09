using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class button_playerTitle : MonoBehaviour
{
    playerTitleSelector listManager;
    
    // Start is called before the first frame update
    void Start()
    {
        listManager = playerTitleSelector.instance;
    }

    public void pressed()
    {
        listManager.pressedButton((byte)this.gameObject.transform.GetSiblingIndex());
    }
}
