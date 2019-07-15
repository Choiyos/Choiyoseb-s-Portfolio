using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IngameSceneButtonCallback : MonoBehaviour
{
    Transform touchCanvas;
    GameObject menuButton;
    GameObject inventoryButton;
    GameObject menuWindow;
    GameObject InventoryWindow;

    public void Init()
    {
        touchCanvas = GameObject.Find("Canvas").transform.GetChild(0);
        menuButton = touchCanvas.GetChild(0).gameObject;
        inventoryButton = touchCanvas.GetChild(1).gameObject;
        menuWindow = touchCanvas.GetChild(3).gameObject;
        InventoryWindow = touchCanvas.GetChild(4).gameObject;

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((eventData) => MenuButtonDown());
        menuButton.GetComponent<EventTrigger>().triggers.Add(entry);
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((eventData) => MenuButtonUp());
        menuButton.GetComponent<EventTrigger>().triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((eventData) => InventoryButtonDown());
        inventoryButton.GetComponent<EventTrigger>().triggers.Add(entry);
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((eventData) => InventoryButtonUp());
        inventoryButton.GetComponent<EventTrigger>().triggers.Add(entry);
    }

    public void MenuButtonDown()
    {
        menuButton.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void InventoryButtonDown()
    {
        inventoryButton.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void MenuButtonUp()
    {
        menuButton.transform.GetChild(0).gameObject.SetActive(false);
        menuWindow.SetActive(true);
    }

    public void InventoryButtonUp()
    {
        inventoryButton.transform.GetChild(0).gameObject.SetActive(false);
        InventoryWindow.SetActive(true);
    }
}
