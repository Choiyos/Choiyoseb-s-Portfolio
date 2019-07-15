using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UseItemFunctionList : MonoBehaviour
{
    SceneEventSystem sceneEventSystem;
    GameObject inventoryWindow;
    GameObject itemInfoWindow;
    Image viewWindow;
    TV tv;
    GameObject tvPos;

    void Awake()
    {
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
        inventoryWindow = GameObject.Find("Canvas").transform.GetChild(0).GetChild(4).gameObject;
        itemInfoWindow = inventoryWindow.transform.GetChild(4).gameObject;
        tv = GameObject.Find("IngameMap").transform.GetChild(1).GetChild(9).GetChild(0).GetChild(4).GetComponent<TV>();
        tvPos = GameObject.Find("IngameMap").transform.GetChild(1).GetChild(9).GetChild(0).GetChild(6).gameObject;
        viewWindow = itemInfoWindow.transform.GetChild(7).GetComponent<Image>();
    }

    // Use 아이템의 기능을 모두 담아둠. 사용방법은 ItemUiEvent 클래스에서 지정해두었음. 동적으로 이 스크립트로 연결해 아이템 이름과 매칭해 해당 기능을 작동.
    public void UseItem(string itemName)
    {
        switch (itemName)
        {
            case "remoteController":
                RemoteControllerEvent();
                break;
            case "Lighter":
                if (sceneEventSystem.Player.transform.GetChild(2).GetComponent<Light>().range != 30f)
                {
                    sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "라이터를 사용했다.", 1.5f);
                    sceneEventSystem.Player.transform.GetChild(2).GetComponent<Light>().range = 30f;
                    sceneEventSystem.Player.transform.GetChild(3).GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0.8f);
                    itemInfoWindow.SetActive(false);
                    inventoryWindow.SetActive(false);
                }
                break;

            //View아이템인 경우
            case "BloodedNote":
            case "FoldedPaper":
            case "Hwang'sMemo":
            case "WetNote":
            case "SodiumPostIt":
                viewWindow.gameObject.SetActive(true);
                viewWindow.overrideSprite = ResourceManager.Instance.GetItemIcon(itemName);
                break;
        }
    }

    void RemoteControllerEvent()
    {
        if (sceneEventSystem.Player.GetComponent<PlayerProperty>().StandingSection.transform.parent.name == "2F")
        {
            if (!tv.IsOn)
                sceneEventSystem.PushSystemMessage("TV가 켜졌다.", 1.5f);
            else
                sceneEventSystem.PushSystemMessage("TV가 꺼졌다.", 1.5f);
            StartCoroutine(remoteControllerEvent_());
        }
    }

    IEnumerator remoteControllerEvent_()
    {
        Camera.main.GetComponent<CameraMove>().SetTarget(tvPos.gameObject);
        itemInfoWindow.SetActive(false);
        inventoryWindow.SetActive(false);
        sceneEventSystem.TouchCanvas.SetActive(false);
        tv.TurnTv();
        yield return new WaitForSeconds(3);
        Camera.main.GetComponent<CameraMove>().SetTarget(sceneEventSystem.Player);
        sceneEventSystem.TouchCanvas.SetActive(true);
    }
}
