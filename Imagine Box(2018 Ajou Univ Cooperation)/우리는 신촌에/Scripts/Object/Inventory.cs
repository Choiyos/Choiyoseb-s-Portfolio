using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    List<Item> inventory = new List<Item>();

    public List<Item> GetInventory
    {
        get { return inventory; }
    }

    void Start()
    {
        Init();
    }

    void Init() // 테스트 코드
    {
        //itemDatabase.Add(new Item("bandage", 1001, "치료 붕대", Item.ItemType.Use));
        //itemDatabase.Add(new Item("remoteController", 1002, "TV를 조정할 수 있는 리모콘", Item.ItemType.Equipment));
        //itemDatabase.Add(new Item("cellPhone", 1003, "여선생이 떨어뜨린 휴대폰", Item.ItemType.Equipment));
        //FileManager.ListDataGenerate<Item>("Assets/Resources/ItemDatabase/ItemDatabase.txt", itemDatabase);

        //FileManager.DataGenerate<Item>("Assets/Resources/ItemDatabase/cellPhone.txt", new Item("cellPhone", 1003, "여선생이 떨어뜨린 휴대폰", Item.ItemType.Use));
    }

    public void AddItem(Item item)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].itemEngName == "") // 인벤토리에서 빈 공간을 찾고, 데이터베이스에서 해당 아이템이 존재할 경우 등록.
            {
                inventory[i] = item;
                return;
            }
        }
    }

    public void SwapItem(int idx, int idx2) // 인벤토리 내에서 아이템 스왑
    {
        Item temp = inventory[idx];
        inventory[idx] = inventory[idx2];
        inventory[idx2] = temp;
    }

    public Item GetItem(string _itemName)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].itemEngName == _itemName)
            {
                return inventory[i];
            }
        }
        return null;
    }

    public bool IsItemExist(string _itemName)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].itemEngName == _itemName)
            {                
                return true;
            }
        }
        return false;
    }

    public void RemoveItemByName(string _itemName) // 아이템을 인벤토리에서 제거한다.
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].itemEngName == _itemName)
            {
                inventory.Remove(inventory[i]);
                return;
            }
        }
    }

}