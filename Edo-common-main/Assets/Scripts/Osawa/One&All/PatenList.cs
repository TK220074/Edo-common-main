using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class Paten_L
{
    public int id;
    public Texture2D lists;
}

[CreateAssetMenu(menuName = "PatenList_c")]
public class PatenList : ScriptableObject
{
    public Paten_L[] Paten_l;

    public Paten_L GetItemById(int id)
    {
        foreach (var item in Paten_l)
        {
            if (item.id == id)
            {
                return item;
            }
        }
        return null;
    }
}
