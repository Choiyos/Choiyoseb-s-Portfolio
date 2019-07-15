using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    Inventory inventory;
    [SerializeField]
    GameObject slotList;
    List<Item> inventoryList;

    Vector2 defaultPosition;

    void OnEnable()
    {
        Time.timeScale = 0;
        DrawInventory();
    }

    void OnDisable()
    {
        Time.timeScale = 1;
    }

    public void DrawInventory()
    {
        inventoryList = inventory.GetInventory;
        for (int i = 0; i < inventoryList.Count; i++)
        {
            if (inventoryList[i].itemKorName != "")
            {
                Transform temp = slotList.transform.GetChild(i).GetChild(0);
                temp.GetComponent<Image>().sprite = ResourceManager.Instance.GetItemIcon( inventoryList[i].itemEngName);
                temp.gameObject.SetActive(true);
            }
            else
            {
                slotList.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}
