using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Item
{
    public string itemEngName;
    public string itemKorName;
    public string itemDes;          // 아이템의 설명
    public ItemType itemType;       //아이템의 속성 설정


    public enum ItemType
    {
        Quest,
        View,
        Use,
        OneTimeKey
    }
    
    public Item(string itemName, string itemKorName, string itemDes, ItemType itemType)
    {
        this.itemEngName = itemName;
        this.itemKorName = itemKorName;
        this.itemDes = itemDes;
        this.itemType = itemType;
    }
}
